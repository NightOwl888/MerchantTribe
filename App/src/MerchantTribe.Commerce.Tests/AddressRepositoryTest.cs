using MerchantTribe.Commerce.Contacts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MerchantTribe.Commerce;
using MerchantTribe.Commerce.Data.EF;
using MerchantTribe.Web.Data;
using MerchantTribe.Web.Logging;

namespace MerchantTribe.Commerce.Tests
{
    
    
    /// <summary>
    ///This is a test class for AddressRepositoryTest and is intended
    ///to contain all AddressRepositoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AddressRepositoryTest
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
        ///A test for Create
        ///</summary>
        [TestMethod()]
        public void CanCreateAnAddressAndAssignBvin()
        {
            RequestContext c = MerchantTribeApplication.Current.CurrentRequestContext;
            c.CurrentStore.Id = 12;
            AddressRepository target = AddressRepository.InstantiateForMemory(c);

            Address item = new Address();
            item.City = "Richmond";

            bool expected = true;
            bool actual;
            actual = target.Create(item);

            Assert.AreEqual(expected, actual);
            Assert.AreNotEqual(string.Empty, item.Bvin, "Bvin should not be empty after create.");            
        }

        /// <summary>
        ///A test for FindStoreContactAddress
        ///</summary>
        [TestMethod()]
        public void CanFindStoreAddressIfOneDoesntExist()
        {
            RequestContext c = MerchantTribeApplication.Current.CurrentRequestContext;
            c.CurrentStore.Id = 100;            
            AddressRepository target = AddressRepository.InstantiateForMemory(c);

            int countBefore = target.CountOfAll();
            Assert.AreEqual(0, countBefore, "No Addresses should exist before the request for store address");

            // Target Store Address Should Not Exist            
            Address actual;
            actual = target.FindStoreContactAddress();

            // Make sure we received an address
            Assert.IsNotNull(actual, "Actual address should not be null");
            Assert.AreNotEqual(string.Empty, actual.Bvin, "Bvin should be a valid value");

            // Make sure it was saved in the database
            int countAfter = target.CountOfAll();
            Assert.AreEqual(1, countAfter, "Request for store address should have created one if missing.");

            // Make sure we can update it
            actual.City = "My New City";
            Assert.IsTrue(target.Update(actual), "Update should be true");

            Address actual2 = target.FindStoreContactAddress();
            Assert.IsNotNull(actual2, "Second request should return an address");
            Assert.AreEqual(actual.Bvin, actual2.Bvin, "Both request should return the same address");
            Assert.AreEqual("My New City", actual2.City, "City should have been updated.");
            
        }

       
    }
}
