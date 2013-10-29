using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Softdigm.Dynamics.Plugin.Entities;

namespace Softdigm.Dynamics.Plugin
{
    /// <summary>
    /// This plugin is used to update order entity fields, when a line item is created or updated
    /// </summary>
    public class UpdateOrderLineItems : IPlugin
    {
        public IOrganizationService service;
        public ITracingService tracingService;
        public Entity entity;
        public OrderLineItem item { get; set; }

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            entity = (Entity)context.InputParameters["Target"];

            item = new OrderLineItem(service, tracingService, context, entity);
            item.ExecutePlugin();
        }
    }
}
