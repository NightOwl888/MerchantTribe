using MerchantTribe.Commerce;
using MerchantTribe.Commerce.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace MerchantTribe.UnitTests
{
    
    
    /// <summary>
    ///This is a test class for VersionHelperTest and is intended
    ///to contain all VersionHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class VersionHelperTest
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


        //[TestMethod()]
        //public void CanCheckForLatestVersion()
        //{
        //    string latestVersionNumber = "5.5";
        //    string currentVersionNumber = "5.4";
        //    bool expected = false;
        //    bool actual;
        //    actual = VersionHelper.IsLatestVersion(latestVersionNumber, currentVersionNumber);
        //    Assert.AreEqual(expected, actual);            
        //}

        //[TestMethod()]
        //public void CanTolerateVersionHigherThanLatest()
        //{
        //    string latestVersionNumber = "5.5";
        //    string currentVersionNumber = "5.6";
        //    bool expected = true;
        //    bool actual;
        //    actual = VersionHelper.IsLatestVersion(latestVersionNumber, currentVersionNumber);
        //    Assert.AreEqual(expected, actual);
        //}

        //[TestMethod()]
        //public void CanCheckForLatestVersionWhenEqual()
        //{
        //    string latestVersionNumber = "5.5";
        //    string currentVersionNumber = "5.5";
        //    bool expected = true;
        //    bool actual;
        //    actual = VersionHelper.IsLatestVersion(latestVersionNumber, currentVersionNumber);
        //    Assert.AreEqual(expected, actual);
        //}

        //[TestMethod()]
        //public void CanTolerateNonNumericVersions()
        //{
        //    string latestVersionNumber = "5.5";
        //    string currentVersionNumber = "5.4 beta";
        //    bool expected = false;
        //    bool actual;
        //    actual = VersionHelper.IsLatestVersion(latestVersionNumber, currentVersionNumber);
        //    Assert.AreEqual(expected, actual);
        //}

        //[TestMethod()]
        //public void CanTolerateNonNumericVersionsOldSPFormat()
        //{
        //    string latestVersionNumber = "5.5";
        //    string currentVersionNumber = "5 SP2";
        //    bool expected = false;
        //    bool actual;
        //    actual = VersionHelper.IsLatestVersion(latestVersionNumber, currentVersionNumber);
        //    Assert.AreEqual(expected, actual);
        //}

        //[TestMethod()]
        //public void ParseVersionToIntegersEmptyTest()
        //{
        //    string input = "";
        //    int[] expected = new int[]{}; 
        //    int[] actual;
        //    actual = VersionHelper.ParseVersionToIntegers(input);
        //    CollectionAssert.AreEqual(expected, actual);            
        //}

        //[TestMethod()]
        //public void ParseVersionToIntegersSimple()
        //{
        //    string input = "5.5";
        //    int[] expected = new int[] {5,5};
        //    int[] actual;
        //    actual = VersionHelper.ParseVersionToIntegers(input);
        //    CollectionAssert.AreEqual(expected, actual);
        //}

        //[TestMethod()]
        //public void ParseVersionToIntegersSingle()
        //{
        //    string input = "5";
        //    int[] expected = new int[] { 5 };
        //    int[] actual;
        //    actual = VersionHelper.ParseVersionToIntegers(input);
        //    CollectionAssert.AreEqual(expected, actual);
        //}

        //[TestMethod()]
        //public void ParseVersionToIntegersWithText()
        //{
        //    string input = "5.6 Beta";
        //    int[] expected = new int[] { 5,6 };
        //    int[] actual;
        //    actual = VersionHelper.ParseVersionToIntegers(input);
        //    CollectionAssert.AreEqual(expected, actual);
        //}

        //[TestMethod()]
        //public void ParseVersionToIntegersWithTextInMiddle()
        //{
        //    string input = "5 Beta 1";
        //    int[] expected = new int[] { 5, 1 };
        //    int[] actual;
        //    actual = VersionHelper.ParseVersionToIntegers(input);
        //    CollectionAssert.AreEqual(expected, actual);
        //}

        //[TestMethod()]
        //public void ParseVersionToIntegersComplex()
        //{
        //    string input = "105.24.23.5 beta-7 2";
        //    int[] expected = new int[] { 105, 24, 23, 5, 7, 2 };
        //    int[] actual;
        //    actual = VersionHelper.ParseVersionToIntegers(input);
        //    CollectionAssert.AreEqual(expected, actual);
        //}

    }
}
