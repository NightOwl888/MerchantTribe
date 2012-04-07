using MerchantTribe.Commerce.Content.Parts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace MerchantTribe.Commerce.Tests
{
    
    [TestClass()]
    public class PartContainerTest
    {

        private TestContext testContextInstance;

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

        [TestMethod()]
        public void CanAddContentPart()
        {
            RootColumn root = new RootColumn();
            ColumnContainer target = new ColumnContainer(root);
            IContentPart part1 = new Html() { Id = "1234", RawHtml = "Hello, World" };            
            bool expected = true; 
            bool actual;
            actual = target.Columns[0].AddPart(part1);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void CanListColumnsOnCreateTest()
        {
            RootColumn root = new RootColumn();
            ColumnContainer target = new ColumnContainer(root); 
            List<Column> expected = new List<Column>(); 
            List<Column> actual;
            actual = target.Columns;
            Assert.IsNotNull(actual, "List of columns should never be null");
            Assert.AreEqual(2, actual.Count, "Column Count should always be 2 on new containers");
        }

        [TestMethod()]
        public void CanListParts()
        {
            RootColumn root = new RootColumn();
            ColumnContainer target = new ColumnContainer(root);

            Html part1 = new Html() { Id = "1234", RawHtml = "Hello, World" };
            
            List<IContentPart> expected = new List<IContentPart>() { part1 };
            List<IContentPart> actual;
            target.Columns[0].AddPart(part1);
            actual = target.Columns[0].Parts;
            Assert.AreEqual(1, actual.Count, "Count should be one");
            Assert.AreEqual(expected[0].Id, actual[0].Id, "part id value didn't match as expected");            
        }

        [TestMethod()]
        public void CanRemoveSingleContentPart()
        {
            RootColumn root = new RootColumn();
            ColumnContainer target = new ColumnContainer(root);

            Html part1 = new Html() { Id = "1234", RawHtml = "Hello, World" };
            
            target.Columns[0].AddPart(part1);
            
            bool expected = true; 
            bool actual;
            actual = target.Columns[0].RemovePart(part1.Id);
            Assert.AreEqual(expected, actual);

            List<IContentPart> partsAfter = target.Columns[0].Parts;
            Assert.AreEqual(0, partsAfter.Count, "Part count should be zero after remove");
        }

        [TestMethod()]
        public void CanRemoveContentPartFromList()
        {
            RootColumn root = new RootColumn();
            ColumnContainer target = new ColumnContainer(root);

            Html part1 = new Html() { Id = "1234", RawHtml = "Hello, World" };
            Html part2 = new Html() { Id = "5678", RawHtml = "Hello, World 2" };

            target.Columns[0].AddPart(part1);
            target.Columns[0].AddPart(part2);
            
            bool expected = true;
            bool actual;
            
            actual = target.Columns[0].RemovePart(part1.Id);
            Assert.AreEqual(expected, actual);

            List<IContentPart> partsAfter = target.Columns[0].Parts;
            Assert.AreEqual(1, partsAfter.Count, "Part count should be one after remove");
            Assert.AreEqual(part2.Id, partsAfter[0].Id, "Wrong part was removed!");
        }

        [TestMethod()]
        public void CanFailWhenRemovingContentPart()
        {
            RootColumn root = new RootColumn();
            ColumnContainer target = new ColumnContainer(root);

            Html part1 = new Html() { Id = "1234", RawHtml = "Hello, World" };
            Html part2 = new Html() { Id = "5678", RawHtml = "Hello, World 2" };
            target.Columns[0].AddPart(part1);
            target.Columns[0].AddPart(part2);
            
            string nonExistingPartId = "56789";                        
            bool expected = false;
            bool actual;
            actual = target.Columns[0].RemovePart(nonExistingPartId);
            Assert.AreEqual(expected, actual);

            List<IContentPart> partsAfter = target.Columns[0].Parts;
            Assert.AreEqual(2, partsAfter.Count, "Part count should be two after failed remove");           
        }

        private Html SamplePart1 = new Html() {Id="Part1", RawHtml="<p>This is Part 1</p>"};
        private Html SamplePart2 = new Html() {Id="Part2", RawHtml="<p>This is Part 2</p>"};
        private Html SamplePart3 = new Html() {Id="Part3", RawHtml="<p>This is <strong>Part 3</strong></p>"};
        private Html SamplePart4 = new Html() { Id = "Part4", RawHtml = "<ul><li>This is Part 4 a</li><li>This is Part 4 b</li></ul>" };
           
        [TestMethod()]
        public void CanRenderForDisplayWhenEmpty()
        {
            RootColumn root = new RootColumn();
                                                                     
            string expected = "<div class=\"cols\">";
            expected += "<div class=\"grid_12\" >";
            expected += "</div>";
            expected += "<div class=\"clearcol\"></div>";
            expected += "</div>"; 
            string actual;
            actual = root.RenderForDisplay(null, null);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void CanRenderForDisplayWithOneColumnContainer()
        {
            RootColumn root = new RootColumn();
            root.Id = "root";

            ColumnContainer target = new ColumnContainer(root);
            target.Id = "target";
            root.Parts.Add(target);
            
            string expected = "<div class=\"cols\">";
            expected += "<div class=\"grid_12\" >";

            expected += "<div class=\"cols editable issortable spacerabove\"><div class=\"grid_6\" ></div><div class=\"grid_6l\" ></div><div class=\"clearcol\"></div></div>";

            expected += "</div>";
            expected += "<div class=\"clearcol\"></div>";
            expected += "</div>";

            string actual;
            actual = root.RenderForDisplay(null, null);
            Assert.AreEqual(expected, actual);           
        }

        [TestMethod()]
        public void CanRenderForDisplayWithOneColumnContainerAndOnePart()
        {
            RootColumn root = new RootColumn();
            root.Id = "root";

            ColumnContainer target = new ColumnContainer(root);
            target.Id = "target";
            target.Columns[0].Parts.Add(SamplePart1);
            target.Columns[1].Parts.Add(SamplePart4);
            root.Parts.Add(target);

            string expected = "<div class=\"cols\">";
            expected += "<div class=\"grid_12\" >";

            expected += "<div class=\"cols editable issortable spacerabove\"><div class=\"grid_6\" >";
            expected += SamplePart1.RawHtml;
            expected += "</div>";

            expected += "<div class=\"grid_6l\" >";
            expected += SamplePart4.RawHtml;
            expected += "</div><div class=\"clearcol\"></div></div>";

            expected += "</div>";
            expected += "<div class=\"clearcol\"></div>";
            expected += "</div>";

            string actual;
            actual = root.RenderForDisplay(null, null);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        public void CanSetNumberOfColumns()
        {
            ColumnContainer target = new ColumnContainer(new RootColumn());
            Assert.IsTrue(target.SetNumberOfColumns(3), "Should be able to set three columns");
            Assert.AreEqual(3, target.Columns.Count, "Column Count should be three");


            ColumnContainer target2 = new ColumnContainer(new RootColumn());
            Assert.IsTrue(target2.SetNumberOfColumns(7), "Should be able to set seven columns");
            Assert.AreEqual(7, target2.Columns.Count, "Column Count should be sevent");

        }

        [TestMethod()]
        public void CanNotSetColumnsToOne()
        {
            ColumnContainer target = new ColumnContainer(new RootColumn());
            Assert.IsFalse(target.SetNumberOfColumns(1), "Should not be able to set columns to one");
        }

        [TestMethod()]
        public void CanNotSetColumnsToMoreThanParentSize()
        {
            ColumnContainer target = new ColumnContainer(new RootColumn());
            Assert.IsFalse(target.SetNumberOfColumns(13), "Should not be able to set columns to more than size 12");

            ColumnContainer target2 = new ColumnContainer(new Column() { Size = ColumnSize.Size5 });
            Assert.IsFalse(target2.SetNumberOfColumns(6), "Should not be able to set columns to more than size 5");
        }

        [TestMethod()]
        public void CanSizeColumnsOnCreate()
        {
            TestColumnSizes(new Column() { Size = ColumnSize.Size12 }, ColumnSize.Size6, ColumnSize.Size6);
            TestColumnSizes(new Column() { Size = ColumnSize.Size11 }, ColumnSize.Size6, ColumnSize.Size5);
            TestColumnSizes(new Column() { Size = ColumnSize.Size10 }, ColumnSize.Size5, ColumnSize.Size5);
            TestColumnSizes(new Column() { Size = ColumnSize.Size9 }, ColumnSize.Size5, ColumnSize.Size4);
            TestColumnSizes(new Column() { Size = ColumnSize.Size8 }, ColumnSize.Size4, ColumnSize.Size4);
            TestColumnSizes(new Column() { Size = ColumnSize.Size7 }, ColumnSize.Size4, ColumnSize.Size3);
            TestColumnSizes(new Column() { Size = ColumnSize.Size6 }, ColumnSize.Size3, ColumnSize.Size3);
            TestColumnSizes(new Column() { Size = ColumnSize.Size5 }, ColumnSize.Size3, ColumnSize.Size2);
            TestColumnSizes(new Column() { Size = ColumnSize.Size4 }, ColumnSize.Size2, ColumnSize.Size2);
            TestColumnSizes(new Column() { Size = ColumnSize.Size3 }, ColumnSize.Size2, ColumnSize.Size1);
            TestColumnSizes(new Column() { Size = ColumnSize.Size2 }, ColumnSize.Size1, ColumnSize.Size1);
        }

        private void TestColumnSizes(IColumn parent, ColumnSize expected1, ColumnSize expected2)
        {
            ColumnContainer target = new ColumnContainer(parent);
            Assert.AreEqual(2, target.Columns.Count, "Container should start with two columns");
            Assert.AreEqual(expected1, target.Columns[0].Size, "Column 1 was not the correct size for start of " + parent.ToString());
            Assert.AreEqual(expected2, target.Columns[1].Size, "Column 2 was not the correct size for start of " + parent.ToString());
        }

        [TestMethod()]
        public void CanAddColumnToContainer()
        {
            RootColumn root = new RootColumn();
            ColumnContainer target = new ColumnContainer(root);
            Assert.AreEqual(2, target.Columns.Count, "Container should start with two columns");            
            Assert.IsTrue(target.AddColumns(1), "Should return true when adding column");
            Assert.AreEqual(3, target.Columns.Count, "Column Cound should be 3");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[0].Size, "First column number should be size 4");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[1].Size, "Second column number should be size 4");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[2].Size, "Third column number should be size 4");            
        }


        [TestMethod()]
        public void CanNotAddMoreColumnsThanSpaceAvailable()
        {
            RootColumn root = new RootColumn();
            ColumnContainer target = new ColumnContainer(root);
            Assert.AreEqual(2, target.Columns.Count, "Container should have 2 column to start");            
            Assert.IsFalse(target.AddColumns(11), "Adding more columns than parent size should return false");
        }

        [TestMethod()]
        public void CanResizeColumnsUsingShorthand()
        {
            RootColumn root = new RootColumn();
            ColumnContainer target = new ColumnContainer(root);            
            target.SetNumberOfColumns(3);                                                
            Assert.AreEqual(3, target.Columns.Count, "Column Cound should be 3");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[0].Size, "First column number should be size 4");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[1].Size, "Second column number should be size 4");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[2].Size, "Third column number should be size 4");

            Assert.IsTrue(target.SetColumnSizes("10,1,1"), "Column set size should be true");

            Assert.AreEqual(ColumnSize.Size10, target.Columns[0].Size, "First column number should be size 10");
            Assert.AreEqual(ColumnSize.Size1, target.Columns[1].Size, "Second column number should be size 1");
            Assert.AreEqual(ColumnSize.Size1, target.Columns[2].Size, "Third column number should be size 1");
        }

        [TestMethod()]
        public void CanResizeColumnsUsingShorthand2()
        {            
            ColumnContainer target = new ColumnContainer(new Column() { Size = ColumnSize.Size9 });
            target.SetNumberOfColumns(5);
            Assert.AreEqual(5, target.Columns.Count, "Column Cound should be 5");

            Assert.IsTrue(target.SetColumnSizes("4w,2w,1,1,1"), "Column set size should be true");

            Assert.AreEqual(ColumnSize.Size4, target.Columns[0].Size, "First column number should be size 4");
            Assert.IsTrue(target.Columns[0].NoGutter, "First Column should have no gutter turned on");
            Assert.AreEqual(ColumnSize.Size2, target.Columns[1].Size, "Second column number should be size 2");
            Assert.IsTrue(target.Columns[1].NoGutter, "Second Column should have no gutter turned on");
            Assert.AreEqual(ColumnSize.Size1, target.Columns[2].Size, "Third column number should be size 1");
            Assert.AreEqual(ColumnSize.Size1, target.Columns[3].Size, "Fourth column number should be size 1");
            Assert.AreEqual(ColumnSize.Size1, target.Columns[4].Size, "Fifth column number should be size 1");
        }

        [TestMethod()]
        public void CanResizeColumnTestMax()
        {
            RootColumn root = new RootColumn();
            ColumnContainer target = new ColumnContainer(root);
            Assert.AreEqual(2, target.Columns.Count, "Container should start with two columns");
            Assert.IsTrue(target.AddColumns(1), "Should return true when adding column");
            Assert.AreEqual(3, target.Columns.Count, "Column Cound should be 3");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[0].Size, "First column number should be size 4");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[1].Size, "Second column number should be size 4");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[2].Size, "Third column number should be size 4");

            Assert.IsTrue(target.ResizeColumn(0, ColumnSize.Size10), "Column should resize to size 10");

            Assert.AreEqual(ColumnSize.Size10, target.Columns[0].Size, "First column number should be size 10");
            Assert.AreEqual(ColumnSize.Size1, target.Columns[1].Size, "Second column number should be size 1");
            Assert.AreEqual(ColumnSize.Size1, target.Columns[2].Size, "Third column number should be size 1");
        }

        [TestMethod()]
        public void CanResizeColumnTestBigger()
        {
            RootColumn root = new RootColumn();
            ColumnContainer target = new ColumnContainer(root);
            Assert.AreEqual(2, target.Columns.Count, "Container should start with two columns");
            Assert.IsTrue(target.AddColumns(1), "Should return true when adding column");
            Assert.AreEqual(3, target.Columns.Count, "Column Cound should be 3");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[0].Size, "First column number should be size 4");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[1].Size, "Second column number should be size 4");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[2].Size, "Third column number should be size 4");

            Assert.IsTrue(target.ResizeColumn(0, ColumnSize.Size5), "Column should resize to size 5");

            Assert.AreEqual(ColumnSize.Size5, target.Columns[0].Size, "First column number should be size 5");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[1].Size, "Second column number should be size 4");
            Assert.AreEqual(ColumnSize.Size3, target.Columns[2].Size, "Third column number should be size 3");
        }

        [TestMethod()]
        public void CanResizeColumnTestSmaller()
        {
            RootColumn root = new RootColumn();
            ColumnContainer target = new ColumnContainer(root);
            Assert.AreEqual(2, target.Columns.Count, "Container should start with two columns");
            Assert.IsTrue(target.AddColumns(1), "Should return true when adding column");
            Assert.AreEqual(3, target.Columns.Count, "Column Cound should be 3");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[0].Size, "First column number should be size 4");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[1].Size, "Second column number should be size 4");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[2].Size, "Third column number should be size 4");

            Assert.IsTrue(target.ResizeColumn(0, ColumnSize.Size2), "Column should resize to size 2");

            Assert.AreEqual(ColumnSize.Size2, target.Columns[0].Size, "First column number should be size 2");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[1].Size, "Second column number should be size 4");
            Assert.AreEqual(ColumnSize.Size6, target.Columns[2].Size, "Third column number should be size 6");
        }

        [TestMethod()]
        public void CanFailToResizeColumnWhenBiggerThanMax()
        {
            RootColumn root = new RootColumn();
            ColumnContainer target = new ColumnContainer(root);
            Assert.AreEqual(2, target.Columns.Count, "Container should start with two columns");
            Assert.IsTrue(target.AddColumns(1), "Should return true when adding column");
            Assert.AreEqual(3, target.Columns.Count, "Column Cound should be 3");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[0].Size, "First column number should be size 4");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[1].Size, "Second column number should be size 4");
            Assert.AreEqual(ColumnSize.Size4, target.Columns[2].Size, "Third column number should be size 4");

            Assert.IsFalse(target.ResizeColumn(0, ColumnSize.Size11), "Column should not resize to size 11");            
        }

        //[TestMethod()]
        //public void CanRenderColumnsInsideColumns()
        //{
            
        //    ColumnContainer target = new ColumnContainer();           
        //    target.AddContentPart(PART1ID, 0);
        //    ColumnContainer insideColumns = new ColumnContainer();
        //    ColumnSize parentSize = ColumnSize.Size12;
        //    insideColumns.AddColumn(parentSize);
        //    Assert.AreEqual(2, insideColumns.ListColumns().Count, "Column Count should be two");
        //    Assert.IsTrue(insideColumns.SetColumnSize(0, ColumnSize.Size3, true), "Set Size should return true");
        //    Assert.AreEqual(ColumnSize.Size3, insideColumns.ListColumns()[0].Size, "First Column Should be Size 3");
        //    Assert.AreEqual(ColumnSize.Size9, insideColumns.ListColumns()[1].Size, "Second Column Should be Size 9");

        //    RenderingContext context = GetSampleRenderingContext();
        //    string expected = "<div class=\"cols\">";
        //    expected += "<div class=\"grid_12\">";
        //    expected += PART1HTML;

        //    expected += "<div class=\"cols\">";
        //    expected += "<div class=\"grid_3w\"></div>";
        //    expected += "<div class=\"grid_9l\"></div>";
        //    expected += "<div class=\"clearcol\"></div>";
        //    expected += "</div>";

        //    expected += "</div>";
        //    expected += "<div class=\"clearcol\"></div>";
        //    expected += "</div>";

        //}

        //[TestMethod()]
        //public void CanSetSortedPartIds()
        //{
        //    ColumnContainer target = new ColumnContainer();
        //    List<string> sortedPartIds = new List<string> { PART1ID, PART2ID, PART3ID }; 
        //    int columnNumber = 0; 
        //    bool expected = true; 
        //    bool actual;
        //    actual = target.SetSortedPartIds(sortedPartIds, columnNumber);
        //    Assert.AreEqual(expected, actual);

        //    List<string> afterSort = target.ListPartIds(columnNumber);
        //    Assert.IsNotNull(afterSort, "List should not be null after sort set");
        //    Assert.AreEqual(sortedPartIds.Count, afterSort.Count, "Count of items didn't match");
        //    Assert.AreEqual(sortedPartIds[0], afterSort[0], "Item 0 didn't match");
        //    Assert.AreEqual(sortedPartIds[1], afterSort[1], "Item 1 didn't match");
        //    Assert.AreEqual(sortedPartIds[2], afterSort[2], "Item 2 didn't match");
        //}
    }
}
