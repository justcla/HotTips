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
            List<string> tipsSeen = new List<string> {"General-GN001", "Editor-ED001"};
            mockTipHistoryManager.Setup(m => m.GetAllTipsSeen()).Returns(tipsSeen);

            TipCalculator tipCalculator = new TipCalculator(mockTipHistoryManager.Object);
            
            // Act
            TipInfo nextTip = tipCalculator.GetNextTip();
            
            // Verify
            Assert.IsNotNull(nextTip);
            Assert.AreEqual("Editor-ED002", nextTip.globalTipId);
        }

        [TestMethod]
        public void TestGetNextTip2()
        {
            // Setup
            Mock<ITipHistoryManager> mockTipHistoryManager = new Mock<ITipHistoryManager>();
            List<string> tipsSeen = new List<string> {"General-GN001", "Editor-ED001", "Editor-ED002"};
            mockTipHistoryManager.Setup(m => m.GetAllTipsSeen()).Returns(tipsSeen);

            TipCalculator tipCalculator = new TipCalculator(mockTipHistoryManager.Object);
            
            // Act
            TipInfo nextTip = tipCalculator.GetNextTip();
            
            // Verify
            Assert.IsNotNull(nextTip);
            Assert.AreEqual("Editor-ED003", nextTip.globalTipId);
        }
    }
}
