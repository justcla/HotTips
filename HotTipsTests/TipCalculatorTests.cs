using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace HotTips
{
    [TestClass]
    public class TipCalculatorTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            System.Diagnostics.Debug.WriteLine("Running test");
            Console.Out.WriteLine("Running test - console output");
            //Assert.Fail("Fail this test");

            string result = TipCalculator.GetNextTipPath();

            Assert.IsTrue(result.Contains("Tips/Tip001.html"));
        }

        [TestMethod]
        public void TestGetAllTips()
        {
            List<TipGroup> allTips = TipCalculator.GetAllTipGroups();
            Assert.IsTrue(allTips.Count == 2);
            var tipGroup = allTips[0];
        }
    }
}
