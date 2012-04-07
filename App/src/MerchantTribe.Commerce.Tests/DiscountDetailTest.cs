using MerchantTribe.Commerce.Marketing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace MerchantTribe.Commerce.Tests
{
    
    
    /// <summary>
    ///This is a test class for DiscountDetailTest and is intended
    ///to contain all DiscountDetailTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DiscountDetailTest
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
        ///A test for ListToXml
        ///</summary>
        [TestMethod()]
        public void CanCreateXmlFromDiscountDetailList()
        {
            List<DiscountDetail> details = new List<DiscountDetail>();
            DiscountDetail d1 = new DiscountDetail() { Description = "Hello, World", Amount = -1.56m };
            details.Add(d1);
            DiscountDetail d2 = new DiscountDetail() { Description = "Cool Item Two", Amount = -1.10m };
            details.Add(d2);

            string expected = "<DiscountDetails>" + System.Environment.NewLine;
            
            expected += "  <DiscountDetail>" + System.Environment.NewLine;
            expected += "    <Id>" + d1.Id.ToString() + "</Id>" + System.Environment.NewLine;
            expected += "    <Description>Hello, World</Description>" + System.Environment.NewLine;
            expected += "    <Amount>-1.56</Amount>" + System.Environment.NewLine;
            expected += "  </DiscountDetail>" + System.Environment.NewLine;

            expected += "  <DiscountDetail>" + System.Environment.NewLine;
            expected += "    <Id>" + d2.Id.ToString() + "</Id>" + System.Environment.NewLine;
            expected += "    <Description>Cool Item Two</Description>" + System.Environment.NewLine;
            expected += "    <Amount>-1.10</Amount>" + System.Environment.NewLine;
            expected += "  </DiscountDetail>" + System.Environment.NewLine;

            expected += "</DiscountDetails>";

            string actual;
            actual = DiscountDetail.ListToXml(details);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ListToXml
        ///</summary>
        [TestMethod()]
        public void CanCreateXmlFromDiscountDetailListWhenEmpty()
        {
            List<DiscountDetail> details = new List<DiscountDetail>();

            string expected = "<DiscountDetails />";

            string actual;
            actual = DiscountDetail.ListToXml(details);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ListFromXml
        ///</summary>
        [TestMethod()]
        public void CanReadDiscountDetailListFromXml()
        {
            List<DiscountDetail> details = new List<DiscountDetail>();
            DiscountDetail d1 = new DiscountDetail() { Description = "Hello, World", Amount = -1.56m };
            details.Add(d1);
            DiscountDetail d2 = new DiscountDetail() { Description = "Cool Item Two", Amount = -1.10m };
            details.Add(d2);
            string xml = DiscountDetail.ListToXml(details);
            
            List<DiscountDetail> actual;
            actual = DiscountDetail.ListFromXml(xml);
            
            Assert.AreEqual(details.Count, actual.Count, "Count of items didn't match");
            for (int i = 0; i < details.Count; i++)
            {
                Assert.AreEqual(details[i].Amount, actual[i].Amount, "Amount Didn't Match");
                Assert.AreEqual(details[i].Description, actual[i].Description, "Description Didn't Match");
                Assert.AreEqual(details[i].Id, actual[i].Id, "Id Didn't Match");
            }
        }

        /// <summary>
        ///A test for ListFromXml
        ///</summary>
        [TestMethod()]
        public void CanReadDiscountDetailListFromXmlWhenEmpty()
        {
            string xml = string.Empty;

            List<DiscountDetail> actual;
            actual = DiscountDetail.ListFromXml(xml);

            Assert.IsNotNull(actual, "List should not be null after reading.");
            Assert.AreEqual(actual.Count, 0, "Actual count should be zero");
        }

        /// <summary>
        ///A test for ListFromXml
        ///</summary>
        [TestMethod()]
        public void CanReadDiscountDetailListFromXmlWhenNoElementsInside()
        {
            string xml = "<DiscountDetails />";

            List<DiscountDetail> actual;
            actual = DiscountDetail.ListFromXml(xml);

            Assert.IsNotNull(actual, "List should not be null after reading.");
            Assert.AreEqual(actual.Count, 0, "Actual count should be zero");
        }
    }
}
