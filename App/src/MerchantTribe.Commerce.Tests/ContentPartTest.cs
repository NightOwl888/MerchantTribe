using MerchantTribe.Commerce.Content.Parts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MerchantTribe.Commerce.Tests
{
    
    
    /// <summary>
    ///This is a test class for RootColumnTest and is intended
    ///to contain all RootColumnTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ContentPartTest
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
        ///A test for SerializeToXmlString
        ///</summary>
        [TestMethod()]
        public void CanSerializeRootColumnToXmlString()
        {
            RootColumn target = new RootColumn();            
            ColumnContainer cols = new ColumnContainer(target);
            target.AddPart(cols);
            cols.SetColumns("2,10");
            cols.Columns[0].AddPart(new Html() { Id="test1", RawHtml = "sidebar" });
            cols.Columns[1].AddPart(new Html() { Id="test2", RawHtml = "<h1>hello, world!</h1>" });
            cols.Columns[1].AddPart(new Html() { Id="test3", RawHtml = "<p>Lorem Ipsum</p>" });

            ColumnContainer cols2 = new ColumnContainer(cols.Columns[1]);
            cols2.SetColumns("4w,6");
            cols2.Columns[0].AddPart(new Html() { Id = "test4", RawHtml = "inside" });
            cols2.Columns[1].AddPart(new Html() { Id = "test5", RawHtml = "outside" });
            cols.Columns[1].AddPart(cols2);

                            
            string expected = "<rootcolumn>";
            expected += "<id>" + target.Id + "</id>";
            expected += "<parts>";

            expected += "<part><id>" + cols.Id + "</id><typecode>columncontainer</typecode>";
            expected += "<spacerabove>true</spacerabove>";
            expected += "<columns>";
            
            expected += "<part><id>" + cols.Columns[0].Id + "</id><typecode>column</typecode>";
            expected += "<size>Size2</size><nogutter>false</nogutter>";
            expected += "<parts>";
            expected += "<part><id>test1</id><typecode>htmlpart</typecode><rawhtml>sidebar</rawhtml></part>";
            expected += "</parts>";
            expected += "</part>";

            expected += "<part><id>" + cols.Columns[1].Id + "</id><typecode>column</typecode>";
            expected += "<size>Size10</size><nogutter>false</nogutter>";
            expected += "<parts>";
            expected += "<part><id>test2</id><typecode>htmlpart</typecode><rawhtml>&lt;h1&gt;hello, world!&lt;/h1&gt;</rawhtml></part>";
            expected += "<part><id>test3</id><typecode>htmlpart</typecode><rawhtml>&lt;p&gt;Lorem Ipsum&lt;/p&gt;</rawhtml></part>";
            expected += "<part><id>" + cols2.Id + "</id><typecode>columncontainer</typecode>";
            expected += "<spacerabove>true</spacerabove>";
            expected += "<columns>";
            
            expected += "<part><id>" + cols2.Columns[0].Id + "</id><typecode>column</typecode>";
            expected += "<size>Size4</size><nogutter>true</nogutter>";
            expected += "<parts>";
            expected += "<part><id>test4</id><typecode>htmlpart</typecode><rawhtml>inside</rawhtml></part>";
            expected += "</parts>";
            expected += "</part>";
            expected += "<part><id>" + cols2.Columns[1].Id + "</id><typecode>column</typecode>";
            expected += "<size>Size6</size><nogutter>false</nogutter>";
            expected += "<parts>";
            expected += "<part><id>test5</id><typecode>htmlpart</typecode><rawhtml>outside</rawhtml></part>";
            expected += "</parts>";
            expected += "</part>";

            expected += "</columns>";
            expected += "</part>";

            expected += "</parts>";
            expected += "</part>";

            expected += "</columns>";
            expected += "</part>";

            expected += "</parts>";
            expected += "</rootcolumn>";
            
            string actual;
            actual = target.SerializeToString();                            
            Assert.AreEqual(expected, actual);            
        }


        [TestMethod()]
        public void CanDeserializeRootColumnFromXmlString()
        {
            string source = "<rootcolumn>";
            source += "<id>rootcol</id>";
            source += "<parts>";            
            source += "<part><id>col1</id><typecode>columncontainer</typecode>";
            source += "<spacerabove>true</spacerabove>";
            source += "<columns>";            
            source += "<part><id>col1a</id><typecode>column</typecode>";
            source += "<size>Size2</size><nogutter>false</nogutter>";
            source += "<parts>";
            source += "<part><id>test1</id><typecode>htmlpart</typecode><rawhtml>sidebar</rawhtml></part>";
            source += "</parts>";
            source += "</part>";            
            source += "<part><id>col1b</id><typecode>column</typecode>";
            source += "<size>Size10</size><nogutter>false</nogutter>";
            source += "<parts>";
            source += "<part><id>test2</id><typecode>htmlpart</typecode><rawhtml>&lt;h1&gt;hello, world!&lt;/h1&gt;</rawhtml></part>";
            source += "<part><id>test3</id><typecode>htmlpart</typecode><rawhtml>&lt;p&gt;Lorem Ipsum&lt;/p&gt;</rawhtml></part>";
            source += "<part><id>col2</id><typecode>columncontainer</typecode>";
            source += "<spacerabove>true</spacerabove>";
            source += "<columns>";            
            source += "<part><id>col2a</id><typecode>column</typecode>";
            source += "<size>Size4</size><nogutter>true</nogutter>";
            source += "<parts>";
            source += "<part><id>test4</id><typecode>htmlpart</typecode><rawhtml>inside</rawhtml></part>";
            source += "</parts>";
            source += "</part>";
            source += "<part><id>col2b</id><typecode>column</typecode>";
            source += "<size>Size6</size><nogutter>false</nogutter>";
            source += "<parts>";
            source += "<part><id>test5</id><typecode>htmlpart</typecode><rawhtml>outside</rawhtml></part>";
            source += "</parts>";
            source += "</part>";            
            source += "</columns>";
            source += "</part>";            
            source += "</parts>";
            source += "</part>";            
            source += "</columns>";
            source += "</part>";            
            source += "</parts>";
            source += "</rootcolumn>";



            RootColumn expected = new RootColumn();
            expected.Id = "rootcol";
            ColumnContainer cols = new ColumnContainer(expected);
            cols.Id = "col1";
            expected.AddPart(cols);
            cols.SetColumns("2,10");
            cols.Columns[0].Id = "col1a";
            cols.Columns[1].Id = "col1b";
            cols.Columns[0].AddPart(new Html() { Id = "test1", RawHtml = "sidebar" });
            cols.Columns[1].AddPart(new Html() { Id = "test2", RawHtml = "<h1>hello, world!</h1>" });
            cols.Columns[1].AddPart(new Html() { Id = "test3", RawHtml = "<p>Lorem Ipsum</p>" });

            ColumnContainer cols2 = new ColumnContainer(cols.Columns[1]);
            cols2.Id = "col2";
            cols2.SetColumns("4w,6");
            cols2.Columns[0].Id = "col2a";
            cols2.Columns[1].Id = "col2b";
            cols2.Columns[0].AddPart(new Html() { Id = "test4", RawHtml = "inside" });
            cols2.Columns[1].AddPart(new Html() { Id = "test5", RawHtml = "outside" });
            cols.Columns[1].AddPart(cols2);


            RootColumn actual = new RootColumn();
            actual.DeserializeFromXml(source);

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.MinimumSize(), actual.MinimumSize());
            Assert.AreEqual(expected.NoGutter, actual.NoGutter);
            Assert.AreEqual(1, actual.Parts.Count);

            ColumnContainer actualCols = (ColumnContainer)actual.Parts[0];
            Assert.IsNotNull(actualCols);

            Assert.AreEqual(cols.Id, actualCols.Id);
            Assert.AreEqual(cols.Columns.Count, actualCols.Columns.Count);
            Assert.AreEqual(cols.MinimumSize(), actualCols.MinimumSize());

            // Column 1
            Assert.AreEqual(cols.Columns[0].Id, actualCols.Columns[0].Id);
            Assert.AreEqual(cols.Columns[0].NoGutter, actualCols.Columns[0].NoGutter);
            Assert.AreEqual(cols.Columns[0].Parts.Count, actualCols.Columns[0].Parts.Count);
            Assert.AreEqual(cols.Columns[0].Size, actualCols.Columns[0].Size);
            // Part 1 in Column 1          
            Html test1 = (Html)actualCols.Columns[0].Parts[0];
            Assert.IsNotNull(test1);
            Assert.AreEqual("test1", test1.Id);
            Assert.AreEqual("sidebar", test1.RawHtml);


            // Column 2
            Assert.AreEqual(cols.Columns[1].Id, actualCols.Columns[1].Id);
            Assert.AreEqual(cols.Columns[1].NoGutter, actualCols.Columns[1].NoGutter);
            Assert.AreEqual(cols.Columns[1].Parts.Count, actualCols.Columns[1].Parts.Count);
            Assert.AreEqual(cols.Columns[1].Size, actualCols.Columns[1].Size);

            // Part 1 in Column 2
            Html test2 = (Html)actualCols.Columns[1].Parts[0];
            Assert.IsNotNull(test2);
            Assert.AreEqual("test2", test2.Id);
            Assert.AreEqual("<h1>hello, world!</h1>", test2.RawHtml);

            // Part 2 in Column 2
            Html test3 = (Html)actualCols.Columns[1].Parts[1];
            Assert.IsNotNull(test3);
            Assert.AreEqual("test3", test3.Id);
            Assert.AreEqual("<p>Lorem Ipsum</p>", test3.RawHtml);

            // ColumnContainer 2 inside column 2
            ColumnContainer actualCols2 = (ColumnContainer)actualCols.Columns[1].Parts[2];
            Assert.IsNotNull(actualCols2);
            Assert.AreEqual(cols2.Columns.Count, actualCols2.Columns.Count);
            Assert.AreEqual(cols2.Id, actualCols2.Id);
            Assert.AreEqual(cols2.MinimumSize(), actualCols2.MinimumSize());

            // ColumnContainer2 column 1
            Assert.AreEqual(cols2.Columns[0].Id, actualCols2.Columns[0].Id);
            Assert.AreEqual(cols2.Columns[0].NoGutter, actualCols2.Columns[0].NoGutter);
            Assert.AreEqual(cols2.Columns[0].Parts.Count, actualCols2.Columns[0].Parts.Count);
            Assert.AreEqual(cols2.Columns[0].Size, actualCols2.Columns[0].Size);
            // ColumnContainer2 Part 1 in Column 1          
            Html test4 = (Html)actualCols2.Columns[0].Parts[0];
            Assert.IsNotNull(test4);
            Assert.AreEqual("test4", test4.Id);
            Assert.AreEqual("inside", test4.RawHtml);

            // ColumnContainer2 column 2
            Assert.AreEqual(cols2.Columns[1].Id, actualCols2.Columns[1].Id);
            Assert.AreEqual(cols2.Columns[1].NoGutter, actualCols2.Columns[1].NoGutter);
            Assert.AreEqual(cols2.Columns[1].Parts.Count, actualCols2.Columns[1].Parts.Count);
            Assert.AreEqual(cols2.Columns[1].Size, actualCols2.Columns[1].Size);
            
            // ColumnContainer2 Part 1 in Column 2          
            Html test5 = (Html)actualCols2.Columns[1].Parts[0];
            Assert.IsNotNull(test5);
            Assert.AreEqual("test5", test5.Id);
            Assert.AreEqual("outside", test5.RawHtml);

            // Make sure everything renders as expected
            string expectedRender = expected.RenderForDisplay(MerchantTribeApplication.Current, null);
            string actualRender = actual.RenderForDisplay(MerchantTribeApplication.Current, null);
            Assert.AreEqual(expectedRender, actualRender);
        }
    }
}
