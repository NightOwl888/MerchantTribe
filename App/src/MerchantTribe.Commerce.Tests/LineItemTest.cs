using MerchantTribe.Commerce.Orders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MerchantTribe.Commerce.Tests
{
    
    
    /// <summary>
    ///This is a test class for LineItemTest and is intended
    ///to contain all LineItemTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LineItemTest
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

        #region " Setup and Teardown (Store ID = 1)"

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
        ///A test for LineTotal
        ///</summary>
        [TestMethod()]
        public void CanGetLineTotalWithDiscounts()
        {
            LineItem target = new LineItem();
            target.BasePricePerItem = 39.99m;            
            target.DiscountDetails.Add(new Marketing.DiscountDetail(){ Amount = -20.01m});
            target.DiscountDetails.Add(new Marketing.DiscountDetail() { Amount = -5m });
            
            Decimal actual =  target.LineTotal;

            Assert.AreEqual((39.99m - 20.01m - 5m), actual, "Total was not correct.");

        }

        /// <summary>
        ///A test for LineTotal
        ///</summary>
        [TestMethod()]
        public void CanGetLineTotalWithoutDiscounts()
        {
            LineItem target = new LineItem();
            target.BasePricePerItem = 39.99m;            
            target.DiscountDetails.Add(new Marketing.DiscountDetail() { Amount = -20.01m });
            target.DiscountDetails.Add(new Marketing.DiscountDetail() { Amount = -5m });

            Decimal actual = target.LineTotalWithoutDiscounts;

            Assert.AreEqual(39.99m, actual, "Total was not correct.");

        }

        /// <summary>
        ///A test for LineTotal
        ///</summary>
        [TestMethod()]
        public void CanGetLineTotalWhenNoDiscounts()
        {
            LineItem target = new LineItem();
            target.BasePricePerItem = 39.99m;
            Decimal actual = target.LineTotal;

            Assert.AreEqual(39.99m, actual, "Total was not correct.");
        }

        /// <summary>
        ///A test for LineTotal
        ///</summary>
        [TestMethod()]
        public void CanGetLineTotalWithDiscountsAndMultipleQuantity()
        {
            LineItem target = new LineItem();
            target.BasePricePerItem = 39.99m;
            target.Quantity = 3;
            target.DiscountDetails.Add(new Marketing.DiscountDetail() { Amount = -18m });
            target.DiscountDetails.Add(new Marketing.DiscountDetail() { Amount = -6m });

            Decimal actual = target.LineTotal;

            decimal expected = 39.99m * 3;
            expected -= 18m;
            expected -= 6m;

            Assert.AreEqual(expected, actual, "Total was not correct.");

        }

        /// <summary>
        ///A test for LineTotal
        ///</summary>
        [TestMethod()]
        public void CanGetAdjustedPerItemPrice()
        {
            LineItem target = new LineItem();
            target.BasePricePerItem = 39.99m;
            target.Quantity = 3;
            target.DiscountDetails.Add(new Marketing.DiscountDetail() { Amount = -18m });
            target.DiscountDetails.Add(new Marketing.DiscountDetail() { Amount = -6m });

            Decimal actual = target.AdjustedPricePerItem;
            
            // 39.99 * 3 = 119.97
            // -18       = 101.97
            // -6        =  95.97
            // divided by three = 31.99;
            decimal expected = 31.99m;

            Assert.AreEqual(expected, actual, "Total was not correct.");

        }
    }
}
