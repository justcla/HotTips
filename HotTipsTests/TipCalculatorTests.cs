using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Moq;

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

            string result = new TipCalculator(new VSTipHistoryManager()).GetNextTipPath();

            //Assert.IsTrue(result.Contains("Tips/Tip001.html"));
        }

        [TestMethod]
        public void TestGetAllTips()
        {
            // Setup
            Mock<ITipHistoryManager> mockTipHistoryManager = new Mock<ITipHistoryManager>();
            List<string> emptyList = new List<string>();
            mockTipHistoryManager.Setup(m => m.GetAllTipsSeen()).Returns(emptyList);

            TipCalculator tipCalculator = new TipCalculator(mockTipHistoryManager.Object);
            
            // Act
            List<GroupOfTips>[] allTips = tipCalculator.GetPrioritizedTipGroups();
            
            // Verify
            Assert.IsNotNull(allTips);
//            var tipGroup = allTips[0];
            mockTipHistoryManager.Verify();
        }
        
        [TestMethod]
        public void TestGetNextTip()
        {
            // Setup
            Mock<ITipHistoryManager> mockTipHistoryManager = new Mock<ITipHistoryManager>();
            List<string> tipsSeen = new List<string>() {"General-GN001", "Editor-ED001"};
            mockTipHistoryManager.Setup(m => m.GetAllTipsSeen()).Returns(tipsSeen);

            TipCalculator tipCalculator = new TipCalculator(mockTipHistoryManager.Object);
            
            // Act
            TipInfo nextTip = tipCalculator.GetNextTip();
            
            // Verify
            Assert.IsNotNull(nextTip);
            Assert.AreEqual("Editor-ED002", nextTip.globalTipId);
        }
    }
}
