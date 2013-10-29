using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Xrm.Sdk;
using Softdigm.Dynamics.Plugin.Entities;
using Softdigm.Dynamics.Plugin;

namespace Softdigm.PCScale.Plugin.Tests
{
    [TestClass]
    public class OrderLineItemTest
    {
        Mock<IOrganizationService> mockService;
        Mock<ITracingService> mockTracingService;
        Mock<IPluginExecutionContext> mockContext;
        Entity TestEntity;
        OrderLineItem testLineItem;

        [TestInitialize]
        public void Initialize()
        {
            mockService = new Mock<IOrganizationService>();
            mockTracingService = new Mock<ITracingService>();
            mockContext = new Mock<IPluginExecutionContext>();
            TestEntity = new Entity();
            testLineItem = new OrderLineItem(mockService.Object, mockTracingService.Object, mockContext.Object, TestEntity);
        }

        [TestMethod]
        public void OrderLineItemRollUpTest()
        {
            //arange
            Guid orderId = Guid.NewGuid();
            TestEntity.Attributes["new_orderid"] = new EntityReference("order", orderId);
            EntityImageCollection imgCol = new EntityImageCollection();
            imgCol.Add("postOrderLineImage", TestEntity);

            //handling retrieve multiple
            Entity mockedLineItemA = new Entity("new_orderline");
            mockedLineItemA["new_AuthorizedHours"] = 2;
            //mockedLineItemA["new_EstimatedHours"] = 2;
            //mockedLineItemA["new_BookedHours"] = 2;
            mockedLineItemA["new_BilledHours"] = 4;

            mockedLineItemA["new_Priority"] = new OptionSetValue(1);
            
            Entity mockedLineItemB = new Entity("new_orderline");
            //mockedLineItemB["new_AuthorizedHours"] = 2;
            //mockedLineItemB["new_EstimatedHours"] = 2;
            mockedLineItemB["new_BookedHours"] = 60;
            mockedLineItemB["new_BilledHours"] = 5;

            mockedLineItemB["new_Priority"] = new OptionSetValue(3);
            
            EntityCollection mockedLineItems = new EntityCollection();
            mockedLineItems.EntityName = "";
            mockedLineItems.Entities.Add(mockedLineItemA);
            mockedLineItems.Entities.Add(mockedLineItemB);

            mockContext.Setup(s => s.PostEntityImages).Returns(imgCol);

            mockService.Setup(t =>
                t.RetrieveMultiple(It.IsAny<Microsoft.Xrm.Sdk.Query.QueryByAttribute>())).
                Returns(mockedLineItems);
            mockService.Setup(t => t.Update(mockedLineItemA));

            //act
            testLineItem.ExecutePlugin();

            //assert
            Assert.AreEqual(testLineItem.AuthorizedTotalHours, 2);
            Assert.AreEqual(testLineItem.EstimatedTotalHours, 0);
            Assert.AreEqual(testLineItem.BilledTotalHours, 9);
            Assert.AreEqual(testLineItem.BookedTotalHours, 60);

            Assert.AreEqual(testLineItem.Priority, 1);
        }

        [TestMethod]
        public void OrderLineItemPluginTest()
        {
            //arange
            Guid userId = Guid.NewGuid();
            Mock<IServiceProvider> mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(t => t.GetService(typeof(IPluginExecutionContext))).Returns(mockContext.Object);
            mockContext.Setup(s => s.UserId).Returns(userId);
            mockServiceProvider.Setup(t => t.GetService(typeof(ITracingService))).Returns(mockTracingService.Object);
            Mock<IOrganizationServiceFactory> mockFactory = new Mock<IOrganizationServiceFactory>();
            mockServiceProvider.Setup(t => t.GetService(typeof(IOrganizationServiceFactory))).Returns(mockFactory.Object);
            mockFactory.Setup(t => t.CreateOrganizationService(userId)).Returns(mockService.Object);


            Guid orderId = Guid.NewGuid();

            TestEntity.Attributes["new_orderid"] = new EntityReference("order", orderId);
            EntityImageCollection imgCol = new EntityImageCollection();
            imgCol.Add("postOrderLineImage", TestEntity);
            ParameterCollection initial = new ParameterCollection();
            initial.Add("Target", TestEntity);

            mockContext.Setup(s => s.InputParameters).Returns(initial);

            //handling retrieve multiple
            Entity mockedLineItemA = new Entity("new_orderline");
            mockedLineItemA["new_AuthorizedHours"] = 2;
            //mockedLineItemA["new_EstimatedHours"] = 2;
            //mockedLineItemA["new_BookedHours"] = 2;
            mockedLineItemA["new_BilledHours"] = 4;

            mockedLineItemA["new_Priority"] = new OptionSetValue(3);

            Entity mockedLineItemB = new Entity("new_orderline");
            //mockedLineItemB["new_AuthorizedHours"] = 2;
            //mockedLineItemB["new_EstimatedHours"] = 2;
            mockedLineItemB["new_BookedHours"] = 60;
            mockedLineItemB["new_BilledHours"] = 5;

            mockedLineItemB["new_Priority"] = null;

            EntityCollection mockedLineItems = new EntityCollection();
            mockedLineItems.EntityName = "";
            mockedLineItems.Entities.Add(mockedLineItemA);
            mockedLineItems.Entities.Add(mockedLineItemB);

            mockContext.Setup(s => s.PostEntityImages).Returns(imgCol);

            mockService.Setup(t =>
                t.RetrieveMultiple(It.IsAny<Microsoft.Xrm.Sdk.Query.QueryByAttribute>())).
                Returns(mockedLineItems);
            mockService.Setup(t => t.Update(mockedLineItemA));

            //act
            UpdateOrderLineItems orderLineTest = new UpdateOrderLineItems();
            orderLineTest.Execute(mockServiceProvider.Object);

            //assert
            Assert.AreEqual(orderLineTest.item.AuthorizedTotalHours, 2);
            Assert.AreEqual(orderLineTest.item.EstimatedTotalHours, 0);
            Assert.AreEqual(orderLineTest.item.BilledTotalHours, 9);
            Assert.AreEqual(orderLineTest.item.BookedTotalHours, 60);

            Assert.AreEqual(orderLineTest.item.Priority, 3);
        }

        [TestCleanup]
        public void CleanUp()
        {
            mockService = null;
            mockTracingService = null;
            TestEntity = null;
        }
    }
}
