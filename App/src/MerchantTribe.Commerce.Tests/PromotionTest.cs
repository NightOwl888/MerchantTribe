using MerchantTribe.Commerce.Marketing;
using MerchantTribe.Commerce.Marketing.PromotionQualifications;
using MerchantTribe.Commerce.Marketing.PromotionActions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MerchantTribe.Web.Data;
using MerchantTribe.Commerce.Orders;

namespace MerchantTribe.Commerce.Tests
{
    
    
    /// <summary>
    ///This is a test class for PromotionTest and is intended
    ///to contain all PromotionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PromotionTest
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


        [TestMethod]
        public void CanCreateAPromotion()
        {
            Promotion p = new Promotion();
            PromotionRepository r = PromotionRepository.InstantiateForMemory(new RequestContext());

            Assert.IsTrue(r.Create(p), "Create should be true");
            Assert.IsNotNull(p);
            Assert.IsTrue(p.Id > 0, "Promotion was not assigned an Id number");
        }

        [TestMethod]
        public void CanFindPromotionInRepository()
        {
            Promotion p = new Promotion();
            p.Name = "FindMe";
            PromotionRepository r = PromotionRepository.InstantiateForMemory(new RequestContext());

            Assert.IsTrue(r.Create(p), "Create should be true");
            Assert.IsNotNull(p);
            Assert.IsTrue(p.Id > 0, "Promotion was not assigned an Id number");

            Promotion target = r.Find(p.Id);
            Assert.IsNotNull(target, "Found should not be null");
            Assert.AreEqual(p.Id, target.Id, "Id didn't match");
            Assert.AreEqual(p.StoreId, target.StoreId, "Store ID Didn't Match");
            Assert.AreEqual(p.Name, target.Name, "Name didn't match");
        }

        [TestMethod]
        public void CanSaveQualificationsAndActionsInRepository()
        {
            Promotion p = new Promotion();
            p.Name = "FindMe";
            p.AddQualification(new ProductBvin("ABC123"));
            p.AddQualification(new ProductType("TYPE0"));
            p.AddAction(new ProductPriceAdjustment(AmountTypes.Percent, 50));
            PromotionRepository r = PromotionRepository.InstantiateForMemory(new RequestContext());


            Assert.IsTrue(r.Create(p), "Create should be true");
            Assert.IsNotNull(p);
            Assert.IsTrue(p.Id > 0, "Promotion was not assigned an Id number");

            Promotion target = r.Find(p.Id);
            Assert.IsNotNull(target, "Found should not be null");
            Assert.AreEqual(p.Id, target.Id, "Id didn't match");
            Assert.AreEqual(p.StoreId, target.StoreId, "Store ID Didn't Match");
            Assert.AreEqual(p.Name, target.Name, "Name didn't match");

            Assert.AreEqual(p.Qualifications.Count, target.Qualifications.Count, "Qualification count didn't match");
            for (int i = 0; i < p.Qualifications.Count; i++)
            {
                Assert.AreEqual(p.Qualifications[0].Id, target.Qualifications[0].Id, "Id of index " + i.ToString() + " didn't match");
                Assert.AreEqual(p.Qualifications[0].GetType(), target.Qualifications[0].GetType(), "Type of index " + i.ToString() + " didn't match");                
            }

            Assert.AreEqual(p.Actions.Count, target.Actions.Count, "Action count didn't match");
            for (int i = 0; i < p.Actions.Count; i++)
            {                
                Assert.AreEqual(p.Actions[0].Id, target.Actions[0].Id, "Id of action index " + i.ToString() + " didn't match");
                Assert.AreEqual(p.Actions[0].GetType(), target.Actions[0].GetType(), "Type of action index " + i.ToString() + " didn't match");
            }
        }

        [TestMethod]
        public void CanUpdateNameOfPromotion()
        {
            Promotion p = new Promotion();
            p.Name = "Old";
            PromotionRepository r = PromotionRepository.InstantiateForMemory(new RequestContext());
            r.Create(p);

            string updatedName = "New";
            p.Name = updatedName;
            Assert.IsTrue(r.Update(p), "Update should be true");
            Promotion target = r.Find(p.Id);

            Assert.IsNotNull(target, "Found should not be null");
            Assert.AreEqual(p.Id, target.Id, "Id didn't match");
            Assert.AreEqual(p.StoreId, target.StoreId, "Store ID Didn't Match");
            Assert.AreEqual("New", target.Name, "Name didn't match");
        }
        
        [TestMethod()]
        public void CanGetActiveStatus()
        {
            Promotion target = new Promotion();
            target.IsEnabled = true;
            target.StartDateUtc = new DateTime(2010, 09, 1, 0, 0, 0); // Sept 1st, 2010
            target.EndDateUtc = new DateTime(2010, 11, 1, 23, 59, 59); // Nov. 1, 2010


            DateTime currentUtcTime = new DateTime(2010,9,15,23,59,59); // Sept. 15th, 11:59:59 pm 
            PromotionStatus expected = PromotionStatus.Active;
            PromotionStatus actual;
            actual = target.GetStatus(currentUtcTime);
            Assert.AreEqual(expected, actual, "Sept 15th should return active status");


            DateTime currentUtcTime2 = new DateTime(2010, 9, 1, 0, 0, 0); // Sept. 1th, 2010
            PromotionStatus expected2 = PromotionStatus.Active;
            PromotionStatus actual2;
            actual2 = target.GetStatus(currentUtcTime2);
            Assert.AreEqual(expected2, actual2, "Sept 1st should return active status");

            DateTime currentUtcTime3 = new DateTime(2010, 11, 1, 23, 59, 59); // Sept. 1th, 2010
            PromotionStatus expected3 = PromotionStatus.Active;
            PromotionStatus actual3;
            actual3 = target.GetStatus(currentUtcTime3);
            Assert.AreEqual(expected3, actual3, "Nov 1st should return active status");            
        }

        [TestMethod()]
        public void CanGetDisabledStatus()
        {
            Promotion target = new Promotion();
            target.IsEnabled = false;
            target.StartDateUtc = new DateTime(2010, 09, 1, 0, 0, 0); // Sept 1st, 2010
            target.EndDateUtc = new DateTime(2010, 11, 1, 23, 59, 59); // Nov. 1, 2010


            DateTime currentUtcTime = new DateTime(2010, 9, 15, 23, 59, 59); // Sept. 15th, 11:59:59 pm 
            PromotionStatus expected = PromotionStatus.Disabled;
            PromotionStatus actual;
            actual = target.GetStatus(currentUtcTime);
            Assert.AreEqual(expected, actual, "Sept 15th should return disabled");


            DateTime currentUtcTime2 = new DateTime(2009, 9, 1, 0, 0, 0); // Sept. 1th, 2009
            PromotionStatus expected2 = PromotionStatus.Disabled;
            PromotionStatus actual2;
            actual2 = target.GetStatus(currentUtcTime2);
            Assert.AreEqual(expected2, actual2, "Sept 1st from one year ago should return disabled status");

            DateTime currentUtcTime3 = new DateTime(2012, 11, 1, 23, 59, 59); // Sept. 1th, 2012
            PromotionStatus expected3 = PromotionStatus.Disabled;
            PromotionStatus actual3;
            actual3 = target.GetStatus(currentUtcTime3);
            Assert.AreEqual(expected3, actual3, "Nov 1st from 2012 should return disabled status");
        }
        
        [TestMethod()]
        public void CanGetExpiredStatus()
        {
            Promotion target = new Promotion();
            target.IsEnabled = true;
            target.StartDateUtc = new DateTime(2010, 09, 1, 0, 0, 0); // Sept 1st, 2010
            target.EndDateUtc = new DateTime(2010, 11, 1, 23, 59, 59); // Nov. 1, 2010


            DateTime currentUtcTime = new DateTime(2010, 11, 2, 0, 0, 0); // Nov. 2nd
            PromotionStatus expected = PromotionStatus.Expired;
            PromotionStatus actual;
            actual = target.GetStatus(currentUtcTime);
            Assert.AreEqual(expected, actual, "Nov 2nd should return Expired status");


            DateTime currentUtcTime2 = new DateTime(2011, 9, 1, 0, 0, 0); // Sept. 1th, 2011
            PromotionStatus expected2 = PromotionStatus.Expired;
            PromotionStatus actual2;
            actual2 = target.GetStatus(currentUtcTime2);
            Assert.AreEqual(expected2, actual2, "Sept 1st should return Expired status");
        }

        [TestMethod()]
        public void CanGetUpcomingStatus()
        {
            Promotion target = new Promotion();
            target.IsEnabled = true;
            target.StartDateUtc = new DateTime(2010, 09, 1, 0, 0, 0); // Sept 1st, 2010
            target.EndDateUtc = new DateTime(2010, 11, 1, 23, 59, 59); // Nov. 1, 2010


            DateTime currentUtcTime = new DateTime(2010, 08, 1, 0, 0, 0); 
            PromotionStatus expected = PromotionStatus.Upcoming;
            PromotionStatus actual;
            actual = target.GetStatus(currentUtcTime);
            Assert.AreEqual(expected, actual, "Augst 1st should return upcoming status");


            DateTime currentUtcTime2 = new DateTime(2010, 8, 31, 23, 59, 59); // August 31st
            PromotionStatus expected2 = PromotionStatus.Upcoming;
            PromotionStatus actual2;
            actual2 = target.GetStatus(currentUtcTime2);
            Assert.AreEqual(expected2, actual2, "August 31th should return Upcoming status");
        }

        [TestMethod()]
        public void CanPutAProductOnSale()
        {
            Promotion p = new Promotion();
            p.IsEnabled = true;
            p.Name = "Product Sale Test";
            p.CustomerDescription = "A Customer Discount";
            p.StartDateUtc = DateTime.Now.AddDays(-1);
            p.EndDateUtc = DateTime.Now.AddDays(1);
            p.StoreId = 1;
            p.Id = 1;
            Assert.IsTrue(p.AddQualification(new ProductBvin("bvin1234")), "Add Qualification Failed");
            Assert.IsTrue(p.AddAction(new ProductPriceAdjustment(AmountTypes.MonetaryAmount, -20m)), "Add Action Failed");

            Catalog.Product prod = new Catalog.Product();
            prod.Bvin = "bvin1234";
            prod.SitePrice = 59.99m;
            prod.StoreId = 1;

            Catalog.UserSpecificPrice userPrice = new Catalog.UserSpecificPrice(prod, null);

            RequestContext c = new RequestContext();
            MerchantTribeApplication app = MerchantTribeApplication.InstantiateForMemory(c);

            bool actual = p.ApplyToProduct(app, prod, userPrice, null, DateTime.UtcNow);

            Assert.IsTrue(actual, "Promotion should have applied with return value of true");
            Assert.AreEqual(39.99m, userPrice.PriceWithAdjustments(), "Price should have been reduced by $20.00");
            Assert.AreEqual(p.CustomerDescription, userPrice.DiscountDetails[0].Description, "Description should match promotion");
        }

        [TestMethod()]
        public void CanPutAProductOnSalePricedByApp()
        {
            RequestContext c = new RequestContext();            
            MerchantTribeApplication app = MerchantTribeApplication.InstantiateForMemory(c);
            app.CurrentStore = new Accounts.Store();
            app.CurrentStore.Id = 1;

            // Create a Promotion to Test
            Promotion p = new Promotion();
            p.Mode = PromotionType.Sale;
            p.IsEnabled = true;
            p.Name = "Product Sale Test";
            p.CustomerDescription = "$10.00 off Test Sale!";
            p.StartDateUtc = DateTime.Now.AddDays(-1);
            p.EndDateUtc = DateTime.Now.AddDays(1);
            p.StoreId = 1;
            p.Id = 0;
            p.AddQualification(new ProductBvin("bvin1234"));
            p.AddAction(new ProductPriceAdjustment(AmountTypes.MonetaryAmount, -10m));
            app.MarketingServices.Promotions.Create(p);

            // Create a test Product
            Catalog.Product prod = new Catalog.Product();
            prod.Bvin = "bvin1234";
            prod.SitePrice = 59.99m;
            prod.StoreId = 1;
            
            Catalog.UserSpecificPrice actualPrice = app.PriceProduct(prod, null, null);

            Assert.IsNotNull(actualPrice, "Price should not be null");
            Assert.AreEqual(49.99m, actualPrice.PriceWithAdjustments(), "Price should be $49.99");
            Assert.AreEqual(1, actualPrice.DiscountDetails.Count, "Discount Details count should be one");            
            Assert.AreEqual(p.CustomerDescription, actualPrice.DiscountDetails[0].Description, "Description should match promotion");
        }

        [TestMethod()]
        public void CanSkipPromotionIfNoProduct()
        {
            Promotion p = new Promotion();
            p.IsEnabled = true;
            p.Name = "Product Sale Test";
            p.StartDateUtc = DateTime.Now.AddDays(-1);
            p.EndDateUtc = DateTime.Now.AddDays(1);
            p.StoreId = 1;
            p.Id = 1;
            Assert.IsTrue(p.AddQualification(new ProductBvin("bvin1234")), "Add Qualification Failed");
            Assert.IsTrue(p.AddAction(new ProductPriceAdjustment(AmountTypes.MonetaryAmount, -20m)), "Add Action Failed");

            RequestContext c = new RequestContext();
            MerchantTribeApplication app = MerchantTribeApplication.InstantiateForMemory(c);

            bool actual = p.ApplyToProduct(app, null, null, null, DateTime.UtcNow);

            Assert.IsFalse(actual, "Promotion should not have applied");
        }

        /// <summary>
        ///A test for ActionsToXml
        ///</summary>
        [TestMethod()]
        public void ActionsToXmlTest()
        {
            Promotion target = new Promotion();
            ProductPriceAdjustment a1 = new ProductPriceAdjustment(AmountTypes.MonetaryAmount, 1.23m);
            target.AddAction(a1);

            string expected = "<Actions>" + System.Environment.NewLine;

            expected += "  <Action>" + System.Environment.NewLine;
            expected += "    <Id>" + a1.Id.ToString() + "</Id>" + System.Environment.NewLine;
            expected += "    <TypeId>" + a1.TypeId + "</TypeId>" + System.Environment.NewLine;
            expected += "    <Settings>" + System.Environment.NewLine;
            expected += "      <Setting>" + System.Environment.NewLine;            
            expected += "        <Key>AdjustmentType</Key>" + System.Environment.NewLine;
            expected += "        <Value>1</Value>" + System.Environment.NewLine;
            expected += "      </Setting>" + System.Environment.NewLine;
            expected += "      <Setting>" + System.Environment.NewLine;            
            expected += "        <Key>Amount</Key>" + System.Environment.NewLine;
            expected += "        <Value>1.23</Value>" + System.Environment.NewLine;
            expected += "      </Setting>" + System.Environment.NewLine;
            expected += "    </Settings>" + System.Environment.NewLine;
            expected += "  </Action>" + System.Environment.NewLine;

            expected += "</Actions>";

            string actual;
            actual = target.ActionsToXml();
            Assert.AreEqual(expected, actual);            
        }

        /// <summary>
        ///A test for QualificationsFromXml
        ///</summary>
        [TestMethod()]
        public void QualificationsFromXmlTest()
        {
            Promotion expected = new Promotion();
            ProductBvin q1 = new ProductBvin("abc123");
            expected.AddQualification(q1);

            string xml = "<Qualifications>" + System.Environment.NewLine;
            xml += "  <Qualification>" + System.Environment.NewLine;
            xml += "    <Id>" + q1.Id.ToString() + "</Id>" + System.Environment.NewLine;
            xml += "    <TypeId>" + q1.TypeId + "</TypeId>" + System.Environment.NewLine;
            xml += "    <ProcessingCost>0</ProcessingCost>" + System.Environment.NewLine;
            xml += "    <Settings>" + System.Environment.NewLine;
            xml += "      <Setting>" + System.Environment.NewLine;
            xml += "        <Key>ProductIds</Key>" + System.Environment.NewLine;
            xml += "        <Value>abc123</Value>" + System.Environment.NewLine;
            xml += "      </Setting>" + System.Environment.NewLine;
            xml += "    </Settings>" + System.Environment.NewLine;
            xml += "  </Qualification>" + System.Environment.NewLine;            
            xml += "</Qualifications>";

            Promotion actual = new Promotion();
            actual.QualificationsFromXml(xml);

            Assert.AreEqual(expected.Qualifications.Count, actual.Qualifications.Count, "Qualifications count did not match");
            Assert.AreEqual(q1.CurrentProductIds()[0], ((ProductBvin)actual.Qualifications[0]).CurrentProductIds()[0], "Product bvin didn't come through");
            for (int i = 0; i < expected.Qualifications.Count; i++)
            {
                Assert.AreEqual(expected.Qualifications[i].Id, actual.Qualifications[i].Id, "Id didn't match for qualification index " + i.ToString());
                Assert.AreEqual(expected.Qualifications[i].ProcessingCost, actual.Qualifications[i].ProcessingCost, "Processing Cost didn't match for qualification index " + i.ToString());
                Assert.AreEqual(expected.Qualifications[i].Settings.Count, actual.Qualifications[i].Settings.Count, "Settings Count didn't match for qualification index " + i.ToString());
                Assert.AreEqual(expected.Qualifications[i].TypeId, actual.Qualifications[i].TypeId, "TypeId didn't match for qualification index " + i.ToString());                
            }            
        }

        /// <summary>
        ///A test for QualificationsToXml
        ///</summary>
        [TestMethod()]
        public void QualificationsToXmlTest()
        {
            Promotion target = new Promotion();
            ProductBvin q1 = new ProductBvin("abc123");
            target.AddQualification(q1);

            string expected = "<Qualifications>" + System.Environment.NewLine;

            expected += "  <Qualification>" + System.Environment.NewLine;
            expected += "    <Id>" + q1.Id.ToString() + "</Id>" + System.Environment.NewLine;
            expected += "    <TypeId>" + q1.TypeId + "</TypeId>" + System.Environment.NewLine;
            expected += "    <ProcessingCost>0</ProcessingCost>" + System.Environment.NewLine;
            expected += "    <Settings>" + System.Environment.NewLine;
            expected += "      <Setting>" + System.Environment.NewLine;
            expected += "        <Key>ProductIds</Key>" + System.Environment.NewLine;
            expected += "        <Value>abc123</Value>" + System.Environment.NewLine;
            expected += "      </Setting>" + System.Environment.NewLine;
            expected += "    </Settings>" + System.Environment.NewLine;
            expected += "  </Qualification>" + System.Environment.NewLine;

            expected += "</Qualifications>";

            string actual;
            actual = target.QualificationsToXml();
            Assert.AreEqual(expected, actual);     
        }

        /// <summary>
        ///A test for ActionsFromXml
        ///</summary>
        [TestMethod()]
        public void ActionsFromXmlTest()
        {
            Promotion expected = new Promotion();
            ProductPriceAdjustment a1 = new ProductPriceAdjustment(AmountTypes.MonetaryAmount, 1.23m);
            expected.AddAction(a1);

            string xml = "<Actions>" + System.Environment.NewLine;
            xml += "  <Action>" + System.Environment.NewLine;
            xml += "    <Id>" + a1.Id.ToString() + "</Id>" + System.Environment.NewLine;
            xml += "    <TypeId>" + a1.TypeId + "</TypeId>" + System.Environment.NewLine;
            xml += "    <Settings>" + System.Environment.NewLine;
            xml += "      <Setting>" + System.Environment.NewLine;
            xml += "        <Key>AdjustmentType</Key>" + System.Environment.NewLine;
            xml += "        <Value>1</Value>" + System.Environment.NewLine;
            xml += "      </Setting>" + System.Environment.NewLine;
            xml += "      <Setting>" + System.Environment.NewLine;
            xml += "        <Key>Amount</Key>" + System.Environment.NewLine;
            xml += "        <Value>1.23</Value>" + System.Environment.NewLine;
            xml += "      </Setting>" + System.Environment.NewLine;
            xml += "    </Settings>" + System.Environment.NewLine;
            xml += "  </Action>" + System.Environment.NewLine;
            xml += "</Actions>";

            Promotion actual = new Promotion();
            actual.ActionsFromXml(xml);

            Assert.AreEqual(expected.Actions.Count, actual.Actions.Count, "Actions count did not match");
            Assert.AreEqual(a1.Amount, ((ProductPriceAdjustment)actual.Actions[0]).Amount, "Amount didn't come through");
            Assert.AreEqual(a1.AdjustmentType, ((ProductPriceAdjustment)actual.Actions[0]).AdjustmentType, "Adjustment Type didn't come through");
            for (int i = 0; i < expected.Actions.Count; i++)
            {
                Assert.AreEqual(expected.Actions[i].Id, actual.Actions[i].Id, "Id didn't match for action index " + i.ToString());
                Assert.AreEqual(expected.Actions[i].Settings.Count, actual.Actions[i].Settings.Count, "Settings Count didn't match for action index " + i.ToString());
                Assert.AreEqual(expected.Actions[i].TypeId, actual.Actions[i].TypeId, "TypeId didn't match for action index " + i.ToString());
            }            
        }

        [TestMethod()]
        public void CanDiscountAnOrderByCoupon()
        {
            RequestContext c = new RequestContext();
            MerchantTribeApplication app = MerchantTribeApplication.InstantiateForMemory(c);
            app.CurrentStore = new Accounts.Store();
            app.CurrentStore.Id = 1;

            // Create a Promotion to Test
            Promotion p = new Promotion();
            p.Mode = PromotionType.Offer;
            p.IsEnabled = true;
            p.Name = "Discount By Coupon Test";
            p.CustomerDescription = "$20.00 off Test Offer!";
            p.StartDateUtc = DateTime.Now.AddDays(-1);
            p.EndDateUtc = DateTime.Now.AddDays(1);
            p.StoreId = 1;
            p.Id = 0;
            OrderHasCoupon q = new OrderHasCoupon();
            q.AddCoupon("COUPON");
            p.AddQualification(q);
            p.AddAction(new OrderTotalAdjustment(AmountTypes.MonetaryAmount, -20m));
            app.MarketingServices.Promotions.Create(p);

            // Create a test Order
            Order o = new Order();
            o.Items.Add(new LineItem() { BasePricePerItem = 59.99m, ProductName = "Sample Product", ProductSku = "ABC123" });
            app.CalculateOrderAndSave(o);

            Assert.AreEqual(59.99m, o.TotalOrderAfterDiscounts, "Order total should be $59.99 before discounts");

            o.AddCouponCode("COUPON");
            app.CalculateOrderAndSave(o);

            Assert.AreEqual(39.99m, o.TotalOrderAfterDiscounts, "Order total after discounts should be $39.99");
            Assert.AreEqual(-20m, o.TotalOrderDiscounts, "Discount should be -20");
            Assert.AreEqual(59.99m, o.TotalOrderBeforeDiscounts, "Order total with coupon but before discount should be $59.99");
        }

        //[TestMethod()]
        //public void CanDiscountShipping()
        //{
        //    RequestContext c = new RequestContext();
        //    MerchantTribeApplication app = MerchantTribeApplication.InstantiateForMemory(c);
        //    app.CurrentStore = new Accounts.Store();
        //    app.CurrentStore.Id = 1;            

        //    // Create a Promotion to Test
        //    Promotion p = new Promotion();
        //    p.Mode = PromotionType.Offer;
        //    p.IsEnabled = true;
        //    p.Name = "10% Off Shipping Test";
        //    p.CustomerDescription = "10% Off Shipping Test!";
        //    p.StartDateUtc = DateTime.Now.AddDays(-1);
        //    p.EndDateUtc = DateTime.Now.AddDays(1);
        //    p.StoreId = 1;
        //    p.Id = 0;
        //    OrderHasCoupon q = new OrderHasCoupon();
        //    q.AddCoupon("COUPON");
        //    p.AddQualification(q);
        //    p.AddAction(new OrderShippingAdjustment(AmountTypes.Percent,-10m));
        //    app.MarketingServices.Promotions.Create(p);

        //    // Create a test Order
        //    Order o = new Order();
        //    o.TotalShippingBeforeDiscounts = 100m;
        //    o.Items.Add(new LineItem() { BasePricePerItem = 50.00m, ProductName = "Sample Product", ProductSku = "ABC123" });
        //    app.CalculateOrderAndSave(o);
            
        //    Assert.AreEqual(100.00, o.TotalShippingAfterDiscounts, "Shipping should be $100 before discounts");
        //    Assert.AreEqual(150.00, o.TotalGrand, "Grand Total should be $150");

        //    o.AddCouponCode("COUPON");
        //    app.CalculateOrderAndSave(o);

        //    Assert.AreEqual(90.00m, o.TotalShippingAfterDiscounts, "Shipping After Discount should be $90.00");
        //    Assert.AreEqual(-10m, o.TotalShippingDiscounts, "Discount should be -10");
        //    Assert.AreEqual(100.00m, o.TotalShippingBeforeDiscounts, "Total Before Discounts Should be $100.00");
        //    Assert.AreEqual(140.00m, o.TotalGrand, "Grand Total should be $140");
        //}

    }
}
