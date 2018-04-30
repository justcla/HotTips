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
            mockTipHistoryManager.Setup(m => m.GetTipHistory()).Returns(emptyList);

            TipCalculator tipCalculator = new TipCalculator(mockTipHistoryManager.Object);
            
            // Act
            //List<GroupOfTips>[] allTips = tipCalculator.GetPrioritizedTipGroups();
            
            // Verify
            //Assert.IsNotNull(allTips);
//            var tipGroup = allTips[0];
            mockTipHistoryManager.Verify();
        }
        
        [TestMethod]
        public void TestGetNextTip()
        {
            // Setup
            Mock<ITipHistoryManager> mockTipHistoryManager = new Mock<ITipHistoryManager>();
            List<string> tipsSeen = new List<string> {"General-GN001", "Editor-ED001"};
            mockTipHistoryManager.Setup(m => m.GetTipHistory()).Returns(tipsSeen);

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
            mockTipHistoryManager.Setup(m => m.GetTipHistory()).Returns(tipsSeen);

            TipCalculator tipCalculator = new TipCalculator(mockTipHistoryManager.Object);
            
            // Act
            TipInfo nextTip = tipCalculator.GetNextTip();
            
            // Verify
            Assert.IsNotNull(nextTip);
            Assert.AreEqual("Editor-ED003", nextTip.globalTipId);
        }

        [TestMethod]
        public void TestGetNextTip3()
        {
            // Setup
            // Mock the TipManager
            Mock<ITipManager> mockTipManager = new Mock<ITipManager>();
            mockTipManager.Setup(m => m.GetPrioritizedTipGroups()).Returns(GenerateTestTipGroups());

            // Mock the TipHistoryManager
            Mock<ITipHistoryManager> mockTipHistoryManager = new Mock<ITipHistoryManager>();
            mockTipHistoryManager.Setup(m => m.GetTipHistory()).Returns(new List<string> { "General-GN001", "Editor-ED001", "Editor-ED002" });

            TipCalculator tipCalculator = new TipCalculator(mockTipHistoryManager.Object, mockTipManager.Object);
            
            // Act
            TipInfo nextTip = tipCalculator.GetNextTip();
            
            // Verify
            Assert.IsNotNull(nextTip);
            Assert.AreEqual("Editor-ED003", nextTip.globalTipId);
        }

        private List<GroupOfTips>[] GenerateTestTipGroups()
        {
            // Create GroupOfTips
            GroupOfTips generalGroup = new GroupOfTips { groupId = "General", groupPriority = 1 };
            GroupOfTips editorGroup = new GroupOfTips { groupId = "Editor", groupPriority = 2 };

            List<TipInfo>[] generalItems = SetupGroupOfTips("General", new string[] { "GN001" });
            generalGroup.TipsPriList = generalItems;
            List<TipInfo>[] editorItems = SetupGroupOfTips("Editor", new string[] { "ED001", "ED002" }, new string[] { "ED003" });
            editorGroup.TipsPriList = editorItems;

            List<GroupOfTips>[] list = new List<GroupOfTips>[3];
            list[0] = new List<GroupOfTips> { generalGroup };
            list[1] = new List<GroupOfTips> { editorGroup };
            return list;
        }

        private static List<TipInfo>[] SetupGroupOfTips(string groupId, string[] p1TipIds = null, string[] p2TipIds = null, string[] p3TipIds = null)
        {
            List<TipInfo>[] tipPriList = new List<TipInfo>[3];
            AddPriorityTips(tipPriList, groupId, p1TipIds, 1);
            AddPriorityTips(tipPriList, groupId, p2TipIds, 2);
            AddPriorityTips(tipPriList, groupId, p3TipIds, 3);
            return tipPriList;
        }

        private static void AddPriorityTips(List<TipInfo>[] tipPriList, string groupId, string[] tipIds, int priority)
        {
            if (tipIds == null) return;
            var tips = new List<TipInfo>();
            foreach (string tipId in tipIds)
            {
                tips.Add(CreateTipInfo(groupId, tipId, priority));
            }
            tipPriList[priority-1] = tips;
        }

        private static TipInfo CreateTipInfo(string groupId, string tipId, int priority = 1)
        {
            return new TipInfo { globalTipId = groupId + "-" + tipId, tipId = tipId, groupId = groupId, priority = priority };
        }
    }
}
