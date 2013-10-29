using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Softdigm.Dynamics.Plugin.Entities
{
    public class OrderLineItem
    {
        IOrganizationService _service;
        ITracingService _tracingService;
        IPluginExecutionContext _context;
        Entity _entity;

        #region Attributes

        public int EstimatedTotalHours { get; set; }
        public int BookedTotalHours { get; set; }
        public int BilledTotalHours { get; set; }
        public int AuthorizedTotalHours { get; set; }
        public int Priority { get; set; }
        public int Status { get; set; }
        public int Phase { get; set; }

        public Guid? OrderID { get; set; }

        #endregion

        public OrderLineItem(IOrganizationService service, ITracingService tracing, IPluginExecutionContext context, Entity entity)
        {
            _service = service;
            _tracingService = tracing;
            _context = context;
            _entity = entity;
        }

        public void ExecutePlugin()
        {
            Entity postOrderLineImage = (Entity)_context.PostEntityImages["postOrderLineImage"];
            OrderID = postOrderLineImage.GetAttributeValue<EntityReference>("new_orderid").Id;
            //get all the orderline items associated with order
            var queryByAttribute = new QueryByAttribute("new_orderline")
            {
                ColumnSet = new ColumnSet("new_AuthorizedHours", "new_EstimatedHours", "new_BookedHours", "new_BilledHours", "new_Priority"),
            };
            queryByAttribute.AddAttributeValue("new_orderid", OrderID);

            var orderLineItems = _service.RetrieveMultiple(queryByAttribute);

            foreach (var orderLineItem in orderLineItems.Entities)
            {
                AuthorizedTotalHours += orderLineItem.GetAttributeValue<int>("new_AuthorizedHours");
                EstimatedTotalHours += orderLineItem.GetAttributeValue<int>("new_EstimatedHours");
                BookedTotalHours += orderLineItem.GetAttributeValue<int>("new_BookedHours");
                BilledTotalHours += orderLineItem.GetAttributeValue<int>("new_BilledHours");

                //get the highest priority
                OptionSetValue lineItemPriority = orderLineItem.GetAttributeValue<OptionSetValue>("new_Priority");
                if (lineItemPriority != null && (Priority == 0 || lineItemPriority.Value < Priority))
                {
                    Priority = lineItemPriority.Value;
                }
            }

            //update order with the roll ups
            Entity order = new Entity("order");
            order.Id = OrderID.Value;
            order["new_TotalAuthorizedHours"] = AuthorizedTotalHours;
            order["new_EstimatedHours"] = EstimatedTotalHours;
            order["new_BookedHours"] = BookedTotalHours;
            order["new_BilledHours"] = BilledTotalHours;
            if (Priority != 0)
            {
                order["new_Priority"] = Priority;
            }

            _service.Update(order);
        }
    }
}
