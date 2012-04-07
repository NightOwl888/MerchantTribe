using MerchantTribe.Commerce.Catalog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MerchantTribe.Commerce;
using MerchantTribe.Commerce.Data.EF;
using MerchantTribe.Web.Data;
using MerchantTribe.Web.Logging;

namespace MerchantTribe.Commerce.Tests
{
    
    
    /// <summary>
    ///This is a test class for OptionRepositoryTest and is intended
    ///to contain all OptionRepositoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class OptionRepositoryTest
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

        #region " Setup and Teardown (Store ID = 342)"

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
            c.CurrentStore.Id = 342;
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


        [TestMethod()]
        public void CanAddOptionsToProductInCorrectOrder()
        {
            MerchantTribeApplication app = MerchantTribeApplication.Current;
            app.CurrentStore.Id = 342;

            Product p = new Product();
            p.Sku = "TESTABC";
            p.ProductName = "Test Product ABC";


            Option opt = new Option();
            opt.SetProcessor(OptionTypes.CheckBoxes);
            opt.Name = "Test Option A";
            opt.Items.Add(new OptionItem() { Name = "Item A" });
            opt.Items.Add(new OptionItem() { Name = "Item B" });

            Option opt2 = new Option();
            opt.SetProcessor(OptionTypes.DropDownList);
            opt.Items.Add(new OptionItem() { Name = "Choice One" });
            opt.Items.Add(new OptionItem() { Name = "Choice Two" });

            Option opt3 = Option.Factory(OptionTypes.Html);
            opt3.Name = "Option 3";

            Option opt4 = Option.Factory(OptionTypes.Html);
            opt3.Name = "Option 4";

            Option opt5 = Option.Factory(OptionTypes.Html);
            opt3.Name = "Option 5";

            Option opt6 = Option.Factory(OptionTypes.Html);
            opt3.Name = "Option 6";

            Option opt7 = Option.Factory(OptionTypes.Html);
            opt3.Name = "Option 7";


            // Add the option
            p.Options.Add(opt);
            p.Options.Add(opt2);
            p.Options.Add(opt3);
            p.Options.Add(opt4);
            p.Options.Add(opt5);
            p.Options.Add(opt6);
            p.Options.Add(opt7);            

            Assert.IsTrue(app.CatalogServices.Products.Create(p));

            Product actual = app.CatalogServices.Products.Find(p.Bvin);
            Assert.IsNotNull(actual, "Actual product should not be null");

            Assert.AreEqual(7, actual.Options.Count, "There should be one option on the product");
            
            Assert.AreEqual(opt.Name, actual.Options[0].Name, "Option name didn't match");
            Assert.AreEqual(opt2.Name, actual.Options[1].Name, "Option2 name didn't match");
            Assert.AreEqual(opt3.Name, actual.Options[2].Name, "Option3 name didn't match");
            Assert.AreEqual(opt4.Name, actual.Options[3].Name, "Option4 name didn't match");
            Assert.AreEqual(opt5.Name, actual.Options[4].Name, "Option5 name didn't match");
            Assert.AreEqual(opt6.Name, actual.Options[5].Name, "Option6 name didn't match");
            Assert.AreEqual(opt7.Name, actual.Options[6].Name, "Option7 name didn't match");                        
        }

        /// <summary>
        ///A test for Create
        ///</summary>
        [TestMethod()]
        public void CanAddOptionsToProductWithItemsInCorrectOrder()
        {
            MerchantTribeApplication app = MerchantTribeApplication.Current;

            Product p = new Product();
            p.Sku = "TESTABC";
            p.ProductName = "Test Product ABC";

            Option opt = new Option();
            opt.SetProcessor(OptionTypes.CheckBoxes);
            opt.Name = "Test Option";
            opt.Items.Add(new OptionItem() { Name = "Item A" });
            opt.Items.Add(new OptionItem() { Name = "Item B" });
            opt.Items.Add(new OptionItem() { Name = "Alphabet City" });
            
            // Add the option
            p.Options.Add(opt);

            Assert.IsTrue(app.CatalogServices.Products.Create(p));

            Product actual = app.CatalogServices.Products.Find(p.Bvin);
            Assert.IsNotNull(actual, "Actual product should not be null");
            
            Assert.AreEqual(1, actual.Options.Count, "There should be one option on the product");
            Assert.AreEqual(opt.Name, actual.Options[0].Name, "Option name didn't match");
            Assert.AreEqual(opt.OptionType, actual.Options[0].OptionType, "Option type was incorrect");
            
            Assert.AreEqual(3, actual.Options[0].Items.Count, "Item count on option should be 3");

            Assert.AreEqual(1, actual.Options[0].Items[0].SortOrder, "First sort order should be zero");
            Assert.AreEqual("Item A", actual.Options[0].Items[0].Name, "First Name Didn't Match");

            Assert.AreEqual(2, actual.Options[0].Items[1].SortOrder, "Second sort order should be zero");
            Assert.AreEqual("Item B", actual.Options[0].Items[1].Name, "Second Name Didn't Match");

            Assert.AreEqual(3, actual.Options[0].Items[2].SortOrder, "Third sort order should be zero");
            Assert.AreEqual("Alphabet City", actual.Options[0].Items[2].Name, "Third Name Didn't Match");
        }
    }
}
