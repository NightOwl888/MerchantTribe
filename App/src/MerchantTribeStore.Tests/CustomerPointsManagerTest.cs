using MerchantTribe.Commerce;
using MerchantTribe.Commerce.Membership;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MerchantTribe.UnitTests
{
    
    
    /// <summary>
    ///This is a test class for CustomerPointsManagerTest and is intended
    ///to contain all CustomerPointsManagerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CustomerPointsManagerTest
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
            c.CurrentStore = new MerchantTribe.Commerce.Accounts.Store();
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
        ///A test for DollarCreditForPoints
        ///</summary>
        [TestMethod()]
        public void CanCalculateDollarValueOfPoints()
        {
            string connectionString = string.Empty;
            int pointsIssuedPerDollar = 1;
            int pointsNeededForDollarCredit = 100;
            long storeId = -1;
            CustomerPointsManager target = CustomerPointsManager.InstantiateForMemory(pointsIssuedPerDollar, pointsNeededForDollarCredit, storeId);
            
            int points = 487;
            Decimal expected = 4.87m;
            Decimal actual;
            actual = target.DollarCreditForPoints(points);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for PointsNeededForPurchaseAmount
        ///</summary>
        [TestMethod()]
        public void CanCalculatePointsNeededForSpend()
        {
            string connectionString = string.Empty;
            int pointsIssuedPerDollar = 1;
            int pointsNeededForDollarCredit = 100;
            long storeId = -1;
            CustomerPointsManager target = CustomerPointsManager.InstantiateForMemory(pointsIssuedPerDollar, pointsNeededForDollarCredit, storeId);

            Decimal purchaseAmount = 57.83m;
            int expected = 5783;
            int actual;
            actual = target.PointsNeededForPurchaseAmount(purchaseAmount);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for PointsToIssueForSpend
        ///</summary>
        [TestMethod()]
        public void CanIssuePointsForSpendEvenDollars()
        {
            string connectionString = string.Empty;
            int pointsIssuedPerDollar = 1;
            int pointsNeededForDollarCredit = 100;
            long storeId = -1;
            CustomerPointsManager target = CustomerPointsManager.InstantiateForMemory(pointsIssuedPerDollar, pointsNeededForDollarCredit, storeId);
            Decimal spend = 2.00m;
            int expected = 2;
            int actual;
            actual = target.PointsToIssueForSpend(spend);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for PointsToIssueForSpend
        ///</summary>
        [TestMethod()]
        public void CanIssuePointsForSpendUneven()
        {
            string connectionString = string.Empty;
            int pointsIssuedPerDollar = 1;
            int pointsNeededForDollarCredit = 100;
            long storeId = -1;
            CustomerPointsManager target = CustomerPointsManager.InstantiateForMemory(pointsIssuedPerDollar, pointsNeededForDollarCredit, storeId);
            Decimal spend = 43.51m;
            int expected = 43;
            int actual;
            actual = target.PointsToIssueForSpend(spend);
            Assert.AreEqual(expected, actual);
        }
    }
}
