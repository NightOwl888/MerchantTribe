using MerchantTribe.Commerce;
using MerchantTribe.Commerce.Orders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace MerchantTribe.Commerce.Tests
{
    
    
    /// <summary>
    ///This is a test class for OrderTest and is intended
    ///to contain all OrderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class OrderTest
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

        #region " Setup and Teardown (StoreID = 1)"

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
            c.CurrentStore.Id = 1;
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


        /// <summary>
        ///A test for Order Constructor
        ///</summary>
        [TestMethod()]
        public void CanAddItemToOrderAndCalculate()
        {
            
            MerchantTribeApplication app = MerchantTribeApplication.Current;
            app.CurrentStore.Id = 1;

            Order target = new Order();
            LineItem li = new LineItem() { BasePricePerItem = 19.99m, 
                                           ProductName = "Sample Product", 
                                           ProductSku = "ABC123", 
                                           Quantity = 2 };
            target.Items.Add(li);
            app.CalculateOrder(target);
            Assert.AreEqual(39.98m, target.TotalOrderBeforeDiscounts, "SubTotal was Incorrect");
            Assert.AreEqual(39.98m, target.TotalGrand, "Grand Total was incorrect");
            
            bool upsertResult = app.OrderServices.Orders.Upsert(target);
            Assert.IsTrue(upsertResult, "Order Upsert Failed");

            Assert.AreEqual(app.CurrentRequestContext.CurrentStore.Id, target.StoreId, "Order store ID was not set correctly");
            Assert.AreNotEqual(string.Empty, target.bvin, "Order failed to get a bvin");
            Assert.AreEqual(1, target.Items.Count, "Item count should be one");
            Assert.AreEqual(target.bvin, target.Items[0].OrderBvin, "Line item didn't receive order bvin");
            Assert.AreEqual(target.StoreId, target.Items[0].StoreId, "Line item didn't recieve storeid");
        }

        [TestMethod()]
        public void CanCalculateShippingWithNonShippingItems()
        {

            MerchantTribeApplication app = MerchantTribeApplication.Current;
            app.CurrentStore.Id = 1;

            // Create Shipping Method            
            Shipping.ShippingMethod m = new Shipping.ShippingMethod();
            m.ShippingProviderId = "3D6623E7-1E2C-444d-B860-A8F542133093"; // Flat Rate Per Item
            m.Settings = new MerchantTribe.Shipping.Services.FlatRatePerItemSettings() { Amount = 1.50m };
            m.Adjustment = 0m;
            m.Bvin = System.Guid.NewGuid().ToString();
            m.Name = "Sample Order";
            m.ZoneId = -100; // US All Zone
            app.OrderServices.ShippingMethods.Create(m);            

            Order target = new Order();
            target.BillingAddress.City = "Richmond";
            target.BillingAddress.CountryBvin = MerchantTribe.Web.Geography.Country.UnitedStatesCountryBvin;
            target.BillingAddress.CountryName = "United States";
            target.BillingAddress.Line1 = "124 Anywhere St.";
            target.BillingAddress.PostalCode = "23233";
            target.BillingAddress.RegionBvin = "VA";
            target.BillingAddress.RegionName = "VA";
                        
            target.ShippingAddress.City = "Richmond";
            target.ShippingAddress.CountryBvin = MerchantTribe.Web.Geography.Country.UnitedStatesCountryBvin;
            target.ShippingAddress.CountryName = "United States";
            target.ShippingAddress.Line1 = "124 Anywhere St.";
            target.ShippingAddress.PostalCode = "23233";
            target.ShippingAddress.RegionBvin = "VA";
            target.ShippingAddress.RegionName = "VA";

            target.ShippingMethodId = m.Bvin;
            target.ShippingProviderId = m.ShippingProviderId;

            LineItem li = new LineItem()
            {
                BasePricePerItem = 19.99m,
                ProductName = "Sample Product",
                ProductSku = "ABC123",
                Quantity = 1,
                ShippingSchedule = -1
            };
            target.Items.Add(li);

            LineItem li2 = new LineItem()
            {
                BasePricePerItem = 19.99m,
                ProductName = "Sample Product 2",
                ProductSku = "ABC1232",
                Quantity = 1,
                ShippingSchedule = 1
            };
            target.Items.Add(li2);

            app.CalculateOrder(target);
            Assert.AreEqual(39.98m, target.TotalOrderBeforeDiscounts, "SubTotal was Incorrect");
            Assert.AreEqual(1.50m, target.TotalShippingBeforeDiscounts, "Shipping Total was Incorrect");
            Assert.AreEqual(41.48m, target.TotalGrand, "Grand Total was incorrect");
        }

        [TestMethod()]
        public void CanSkipShippingWhenOnlyNonShippingItems()
        {

            MerchantTribeApplication app = MerchantTribeApplication.Current;
            app.CurrentStore.Id = 1;

            // Create Shipping Method            
            Shipping.ShippingMethod m = new Shipping.ShippingMethod();
            m.ShippingProviderId = "3D6623E7-1E2C-444d-B860-A8F542133093"; // Flat Rate Per Item
            m.Settings = new MerchantTribe.Shipping.Services.FlatRatePerItemSettings() { Amount = 1.50m };
            m.Adjustment = 0m;
            m.Bvin = System.Guid.NewGuid().ToString();
            m.Name = "Sample Order";
            m.ZoneId = -100; // US All Zone
            app.OrderServices.ShippingMethods.Create(m);

            Order target = new Order();
            target.BillingAddress.City = "Richmond";
            target.BillingAddress.CountryBvin = MerchantTribe.Web.Geography.Country.UnitedStatesCountryBvin;
            target.BillingAddress.CountryName = "United States";
            target.BillingAddress.Line1 = "124 Anywhere St.";
            target.BillingAddress.PostalCode = "23233";
            target.BillingAddress.RegionBvin = "VA";
            target.BillingAddress.RegionName = "VA";

            target.ShippingAddress.City = "Richmond";
            target.ShippingAddress.CountryBvin = MerchantTribe.Web.Geography.Country.UnitedStatesCountryBvin;
            target.ShippingAddress.CountryName = "United States";
            target.ShippingAddress.Line1 = "124 Anywhere St.";
            target.ShippingAddress.PostalCode = "23233";
            target.ShippingAddress.RegionBvin = "VA";
            target.ShippingAddress.RegionName = "VA";

            target.ShippingMethodId = m.Bvin;
            target.ShippingProviderId = m.ShippingProviderId;

            LineItem li = new LineItem()
            {
                BasePricePerItem = 19.99m,
                ProductName = "Sample Product",
                ProductSku = "ABC123",
                Quantity = 1,
                ShippingSchedule = -1
            };
            target.Items.Add(li);

            LineItem li2 = new LineItem()
            {
                BasePricePerItem = 19.99m,
                ProductName = "Sample Product 2",
                ProductSku = "ABC1232",
                Quantity = 1,
                ShippingSchedule = -1
            };
            target.Items.Add(li2);

            app.CalculateOrder(target);
            Assert.AreEqual(39.98m, target.TotalOrderBeforeDiscounts, "SubTotal was Incorrect");
            Assert.AreEqual(0m, target.TotalShippingBeforeDiscounts, "Shipping Total was Incorrect");
            Assert.AreEqual(39.98m, target.TotalGrand, "Grand Total was incorrect");
        }

        [TestMethod()]
        public void CanUseShippingOverrideCorrectly()
        {

            MerchantTribeApplication app = MerchantTribeApplication.Current;
            app.CurrentStore.Id = 1;

            // Create Shipping Method            
            Shipping.ShippingMethod m = new Shipping.ShippingMethod();
            m.ShippingProviderId = "3D6623E7-1E2C-444d-B860-A8F542133093"; // Flat Rate Per Item
            m.Settings = new MerchantTribe.Shipping.Services.FlatRatePerItemSettings() { Amount = 1.50m };
            m.Adjustment = 0m;
            m.Bvin = System.Guid.NewGuid().ToString();
            m.Name = "Sample Order";
            m.ZoneId = -100; // US All Zone
            app.OrderServices.ShippingMethods.Create(m);

            Order target = new Order();
            target.BillingAddress.City = "Richmond";
            target.BillingAddress.CountryBvin = MerchantTribe.Web.Geography.Country.UnitedStatesCountryBvin;
            target.BillingAddress.CountryName = "United States";
            target.BillingAddress.Line1 = "124 Anywhere St.";
            target.BillingAddress.PostalCode = "23233";
            target.BillingAddress.RegionBvin = "VA";
            target.BillingAddress.RegionName = "VA";

            target.ShippingAddress.City = "Richmond";
            target.ShippingAddress.CountryBvin = MerchantTribe.Web.Geography.Country.UnitedStatesCountryBvin;
            target.ShippingAddress.CountryName = "United States";
            target.ShippingAddress.Line1 = "124 Anywhere St.";
            target.ShippingAddress.PostalCode = "23233";
            target.ShippingAddress.RegionBvin = "VA";
            target.ShippingAddress.RegionName = "VA";

            target.ShippingMethodId = m.Bvin;
            target.ShippingProviderId = m.ShippingProviderId;

            LineItem li = new LineItem()
            {
                BasePricePerItem = 19.99m,
                ProductName = "Sample Product",
                ProductSku = "ABC123",
                Quantity = 1,
                ShippingSchedule = -1
            };
            target.Items.Add(li);

            LineItem li2 = new LineItem()
            {
                BasePricePerItem = 19.99m,
                ProductName = "Sample Product 2",
                ProductSku = "ABC1232",
                Quantity = 1,
                ShippingSchedule = 1
            };
            target.Items.Add(li2);

            target.TotalShippingBeforeDiscountsOverride = 5.00m;

            app.CalculateOrder(target);
            Assert.AreEqual(39.98m, target.TotalOrderBeforeDiscounts, "SubTotal was Incorrect");
            Assert.AreEqual(5.00m, target.TotalShippingBeforeDiscounts, "Shipping Total was not overridden");
            Assert.AreEqual(44.98m, target.TotalGrand, "Grand Total was incorrect");
        }

        [TestMethod()]
        public void CanAddCouponToOrder()
        {
            Order target = new Order();
            
            Assert.IsTrue(target.AddCouponCode("coupon"), "Add failed");
            Assert.IsTrue(target.CouponCodeExists("coupon"), "Validate Check Failed");
            Assert.AreEqual(1, target.Coupons.Count, "Coupon count should be one");
            Assert.AreEqual("COUPON", target.Coupons[0].CouponCode, "Code didn't match");
        }

        [TestMethod()]
        public void CanSaveAndRetrieveCouponsInRepository()
        {
            Order target = new Order();            
            OrderRepository repository = OrderRepository.InstantiateForMemory(MerchantTribeApplication.Current.CurrentRequestContext);
            MerchantTribeApplication.Current.CurrentStore.Id = 1;
            repository.Create(target);
            target.AddCouponCode("one");
            target.AddCouponCode("two");
            target.AddCouponCode("three");
            repository.Update(target);

            Order actual = repository.FindForCurrentStore(target.bvin);

            Assert.AreEqual(target.Coupons.Count, actual.Coupons.Count, "Coupon count didn't match");
            for (int i = 0; i < target.Coupons.Count; i++)
            {
                Assert.AreEqual(target.Coupons[i].CouponCode, actual.Coupons[i].CouponCode, "Code didn't match for index " + i);
            }

        }

        [TestMethod()]
        public void CanAddCouponAndGetIdNumber()
        {
            Order target = new Order();
            target.AddCouponCode("one");
            target.AddCouponCode("two");
            OrderRepository repository = OrderRepository.InstantiateForMemory(MerchantTribeApplication.Current.CurrentRequestContext);
            MerchantTribeApplication.Current.CurrentStore.Id = 1;
            repository.Create(target);

            Assert.AreEqual(2, target.Coupons.Count, "Coupon count should be one");
            Assert.AreNotEqual(0, target.Coupons[0].Id, "Coupon id should never be zero");
            Assert.AreNotEqual(target.Coupons[0].Id, target.Coupons[1].Id, "Coupon ids should be unique");
        }

        [TestMethod()]
        public void RemoveCouponCodeTest()
        {
            Order target = new Order();
            OrderRepository repository = OrderRepository.InstantiateForMemory(MerchantTribeApplication.Current.CurrentRequestContext);
            repository.Create(target);
            target.AddCouponCode("one");
            target.AddCouponCode("two");
            target.AddCouponCode("three");
            repository.Update(target);

            Assert.IsTrue(target.RemoveCouponCode(target.Coupons[1].Id), "Call to remove failed");
            Assert.AreEqual(2, target.Coupons.Count, "Target count should be two!");
            Assert.IsTrue(repository.Update(target), "Call to updated failed");

            Order actual = repository.FindForCurrentStore(target.bvin);

            Assert.AreEqual(2, actual.Coupons.Count, "Coupon count didn't match");
            for (int i = 0; i < target.Coupons.Count; i++)
            {
                Assert.AreEqual(target.Coupons[i].CouponCode, actual.Coupons[i].CouponCode, "Code didn't match for index " + i);
            }

        }
    }
}
