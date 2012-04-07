using MerchantTribe.Commerce.Orders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MerchantTribe.Commerce;

namespace MerchantTribe.Commerce.Tests
{
    
    
    /// <summary>
    ///This is a test class for OrderServiceTest and is intended
    ///to contain all OrderServiceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class OrderServiceTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        #region " Setup and Teardown (StoreID = 230)"

        /// <summary>
        /// Sets up the global static MerchantTribeApplication so the rest of the application can use it without
        /// having to explicitly create an instance.
        /// </summary>
        [TestInitialize]
        public void InstantiateMerchantTribeApplicationInContext()
        {
            RequestContext c = new RequestContext();
            MerchantTribeApplication app = MerchantTribeApplication.InstantiateForMemory(c);
            c.CurrentStore = new Accounts.Store();
            c.CurrentStore.Id = 230;
            MerchantTribe.Commerce.MerchantTribeApplication.Current = app;
        }

        /// <summary>
        /// Destroys the global context so state cannot be accidentally transferred between tests.
        /// </summary>
        [TestCleanup]
        public void RemoveMerchantTribeApplicationFromContext()
        {
            MerchantTribe.Commerce.MerchantTribeApplication.Current = null;
        }

        #endregion

                      
        [TestMethod]
        public void CanCreateShippingMethod()
        {
 
            MerchantTribeApplication app = MerchantTribeApplication.Current;
            app.CurrentStore.Id = 230;

            Shipping.ShippingMethod target = new Shipping.ShippingMethod();
            target.Adjustment = 1.23m;
            target.AdjustmentType = Shipping.ShippingMethodAdjustmentType.Percentage;
            target.Bvin = string.Empty;
            //target.CustomProperties.Add("bvsoftware", "mykey", "myvalue");
            target.Name = "Test Name";
            target.Settings.AddOrUpdate("MySetting", "MySetVal");
            target.ShippingProviderId = "123456";
            target.ZoneId = -101;

            Assert.IsTrue(app.OrderServices.ShippingMethods.Create(target));
            Assert.AreNotEqual(string.Empty, target.Bvin, "Bvin should not be empty");
        }

        [TestMethod]
        public void CanRetrieveShippingMethod()
        {
            MerchantTribeApplication app = MerchantTribeApplication.Current;
            app.CurrentStore.Id = 230;

            Shipping.ShippingMethod target = new Shipping.ShippingMethod();
            target.Adjustment = 1.23m;
            target.AdjustmentType = Shipping.ShippingMethodAdjustmentType.Percentage;
            target.Bvin = string.Empty;
            //target.CustomProperties.Add("bvsoftware", "mykey", "myvalue");
            target.Name = "Test Name";
            target.Settings.AddOrUpdate("MySetting", "MySetVal");
            target.ShippingProviderId = "123456";
            target.ZoneId = -101;

            app.OrderServices.ShippingMethods.Create(target);
            Assert.AreNotEqual(string.Empty, target.Bvin, "Bvin should not be empty");

            Shipping.ShippingMethod actual = app.OrderServices.ShippingMethods.Find(target.Bvin);
            Assert.IsNotNull(actual, "Actual should not be null");

            Assert.AreEqual(actual.Adjustment,target.Adjustment);
            Assert.AreEqual(actual.AdjustmentType,target.AdjustmentType);
            Assert.AreEqual(actual.Bvin,target.Bvin);
            //Assert.AreEqual(actual.CustomProperties[0].Key, target.CustomProperties[0].Key);
            Assert.AreEqual(actual.Name,target.Name);
            Assert.AreEqual(actual.Settings["MySetting"], target.Settings["MySetting"]);
            Assert.AreEqual(actual.ShippingProviderId,target.ShippingProviderId);
            Assert.AreEqual(actual.ZoneId,target.ZoneId);

        }

        [TestMethod]
        public void CanUpdateShippingMethod()
        {
            MerchantTribeApplication app = MerchantTribeApplication.Current;
            app.CurrentStore.Id = 230;

            Shipping.ShippingMethod target = new Shipping.ShippingMethod();
            target.Adjustment = 1.23m;
            target.AdjustmentType = Shipping.ShippingMethodAdjustmentType.Percentage;
            target.Bvin = string.Empty;
            //target.CustomProperties.Add("bvsoftware", "mykey", "myvalue");
            target.Name = "Test Name";
            target.Settings.AddOrUpdate("MySetting", "MySetVal");
            target.ShippingProviderId = "123456";
            target.ZoneId = -101;

            app.OrderServices.ShippingMethods.Create(target);
            Assert.AreNotEqual(string.Empty, target.Bvin, "Bvin should not be empty");

            target.Adjustment = 1.95m;
            target.AdjustmentType = Shipping.ShippingMethodAdjustmentType.Amount;                        
            target.Name = "Test Name Updated";
            target.Settings.AddOrUpdate("MySetting", "MySetVal 2");
            target.ShippingProviderId = "1Update";
            target.ZoneId = -100;
            Assert.IsTrue(app.OrderServices.ShippingMethods.Update(target));

            Shipping.ShippingMethod actual = app.OrderServices.ShippingMethods.Find(target.Bvin);
            Assert.IsNotNull(actual, "Actual should not be null");

            Assert.AreEqual(actual.Adjustment, target.Adjustment);
            Assert.AreEqual(actual.AdjustmentType, target.AdjustmentType);
            Assert.AreEqual(actual.Bvin, target.Bvin);
            //Assert.AreEqual(actual.CustomProperties[0].Key, target.CustomProperties[0].Key);
            Assert.AreEqual(actual.Name, target.Name);
            Assert.AreEqual(actual.Settings["MySetting"], target.Settings["MySetting"]);
            Assert.AreEqual(actual.ShippingProviderId, target.ShippingProviderId);
            Assert.AreEqual(actual.ZoneId, target.ZoneId);
        }


        [TestMethod]
        public void CanDeleteShippingMethod()
        {
            MerchantTribeApplication app = MerchantTribeApplication.Current;
            app.CurrentStore.Id = 230;

            Shipping.ShippingMethod target = new Shipping.ShippingMethod();
            target.Adjustment = 1.23m;
            target.AdjustmentType = Shipping.ShippingMethodAdjustmentType.Percentage;
            target.Bvin = string.Empty;
            //target.CustomProperties.Add("bvsoftware", "mykey", "myvalue");
            target.Name = "Test Name";
            target.Settings.AddOrUpdate("MySetting", "MySetVal");
            target.ShippingProviderId = "123456";
            target.ZoneId = -101;

            Assert.IsTrue(app.OrderServices.ShippingMethods.Create(target));
            Assert.AreNotEqual(string.Empty, target.Bvin, "Bvin should not be empty");

            Assert.IsTrue(app.OrderServices.ShippingMethods.Delete(target.Bvin), "Delete should be true");
            Shipping.ShippingMethod actual = app.OrderServices.ShippingMethods.Find(target.Bvin);
            Assert.IsNull(actual, "Actual should be null after delete");
        }
    }
}
