using CSharpSim.Classes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;

namespace CSharpSim_Test
{
    
    
    /// <summary>
    ///This is a test class for WorldTest and is intended
    ///to contain all WorldTest Unit Tests
    ///</summary>
    [TestClass()]
    public class WorldTest
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


        /// <summary>
        ///A test for Interset
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CSharpSim.exe")]
        public void IntersetTest()
        {
            var target = new World_Accessor();
            var p0 = new Point(0,0); 
            var p1 = new Point(3,3); 
            var p2 = new Point(0,3); 
            var p3 = new Point(3,0); 
            var expected = new Vector(1.5,1.5);
            var actual = target.Interset(p0, p1, p2, p3);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
