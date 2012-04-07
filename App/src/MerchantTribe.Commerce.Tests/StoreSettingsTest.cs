using MerchantTribe.Commerce.Accounts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MerchantTribe.Commerce.Tests
{
    
    
    /// <summary>
    ///This is a test class for StoreSettingsTest and is intended
    ///to contain all StoreSettingsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StoreSettingsTest
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
        ///A test for AllowApiToClearUntil
        ///</summary>
        [TestMethod()]
        public void DateTimeUtcStringTester()
        {
            DateTime current = DateTime.UtcNow;            

            string serialized = current.ToString("u");

            DateTime actual = DateTime.UtcNow.AddDays(-1);
            DateTime.TryParse(serialized, out actual);
            actual = actual.ToUniversalTime();

            Assert.AreEqual(current.ToString(), actual.ToString(), "String output doesn't match");            
        }
    }
}
