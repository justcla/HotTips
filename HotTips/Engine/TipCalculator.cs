using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace HotTips
{
    public class TipCalculator
    {
        public List<GroupOfTips>[] groupsPriList { get; set; }

        // History of tips seen (ordered list)
        private List<string> tipHistory;

        // Fast-lookup tip history HashSet
        private HashSet<string> tipHistorySet;

        private static readonly char GLOBAL_TIP_ID_SEPARATOR = '-';

        public ITipHistoryManager TipHistoryManager { get; set; }
        public ITipManager TipManager { get; private set; }

        public TipCalculator(ITipHistoryManager tipHistoryManager, ITipManager tipManager = null)
        {
            TipHistoryManager = tipHistoryManager;
            TipManager = tipManager ?? new TipManager();
        }

        public string GetNextTipPath()
        {
            TipInfo tipInfo = GetNextTip();
            return tipInfo.contentUri;
        }

        public TipInfo GetNextTip()
        {
            // TODO: Work out the next tip.

            // First time, set tipInfo to Tip001
            if (IsFirstTime())
            {
                return new TipInfo
                {
                    tipId = "GN001",
                    contentUri = EmbeddedTipsProvider.Instance().GetTipPath("Tip001.html")
                };
            }

            var nextTip = CalculateNextTip();

            return nextTip;
        }

        public TipInfo CalculateNextTip()
        {
            TipInfo nextTip;
            // Get Prioritized Tip Groups
            // TODO: Move to class variable
            List<GroupOfTips>[] prioritizedTipGroups = TipManager.GetPrioritizedTipGroups();

            // Get group of last tip seen
            List<string> tipHistoryList = GetTipHistory();
            string lastSeenGroupId = null;
            if (tipHistoryList != null && tipHistoryList.Count > 0)
            {
                string lastTipGlobalId = tipHistoryList.Last();
                // Extract groupId from global tip Id "[GroupId-TipId]"
                lastSeenGroupId = lastTipGlobalId.Split(GLOBAL_TIP_ID_SEPARATOR)[0];
            }

            nextTip = GetNextTipAlgorithm(prioritizedTipGroups, lastSeenGroupId);

            if (nextTip != null || tipHistoryList == null) return nextTip;

            //  if no tips found at any priGroup level, clear history and start again.
            ClearTipHistory();
            return GetNextTipAlgorithm(prioritizedTipGroups, null);
        }

        private TipInfo GetNextTipAlgorithm(List<GroupOfTips>[] prioritizedTipGroups, string lastSeenGroupId)
        {
            // Algorithm:
            //  Go through tips in order, *order determined by priGroup, then natural list ordering, round-robin by rowId.
            for (int priorityBand = 2; priorityBand <= (3 + 3); priorityBand++)
            {
                // Loop increasing scanRow until a scan of a row returns no tips. Then move to next priorityBand.
                int scanRow = 0;
                bool tipFound;
                do
                {
                    // Search the whole priorityBand (ie, [3][1], [2][2], [1][3] for each scanRow.
                    // If a viable item is found, return it immediately.
                    // If the scan finishes and there's a holdItem, return it.
                    // If tips found, but none to show scan whoe priorityBand at next scanRow.
                    // Stop when no tips found in the whole priorityBand for the given scanRow.
                    
                    tipFound = false;
                    TipInfo sameGroupTip = null;

                    for (int i = 1; i <= 3; i++)
                    {
                        int groupPri = priorityBand - i;
                        if (groupPri < 1 || groupPri > 3) break;
                        int tipPri = priorityBand - groupPri;
                        if (tipPri < 1 || tipPri > 3) break;

                        // For each group in the groupPri bucket,
                        List<GroupOfTips> tipGroups = prioritizedTipGroups[groupPri-1];
                        if (tipGroups == null)
                        {
                            // No groups in this group priority
                            continue;
                        }
                        foreach (GroupOfTips tipGroup in tipGroups)
                        {
                            //  If Group is an excluded group, skip to the next group
                            if (IsExcludedGroup(tipGroup.groupId))
                            {
                                // Group exlcuded. Move to the next group.
                                continue;
                            }

                            // Look into the tipPri bucket (if it exists)
                            List<TipInfo>[] tipGroupTipsPriList = tipGroup.tipsPriList;
                            if (tipGroupTipsPriList == null || tipGroupTipsPriList.Length < tipPri)
                            {
                                // This tipGroup has no tips at this tipPri level. Move to next group.
                                continue;
                            }
                            List<TipInfo> tipsAtPriBand = tipGroupTipsPriList[tipPri-1];

                            // Look at scanRow posn for a tip
                            if (tipsAtPriBand == null || scanRow >= tipsAtPriBand.Count)
                            {
                                // No tips left in this group at this scanRow. Move to the next group.
                                continue;
                            }
                            // Assume tipInfo will not be null at this point.
                            TipInfo tipInfo = tipsAtPriBand[scanRow];

                            // TipInfo is not null! We found a tip at this scanRow in this tipPri in this groupPri
                            // Note: Tip might have already been seen.
                            tipFound = true;

                            // If we've seen the tip, move to the next group.
                            if (GetTipHistorySet().Contains(tipInfo.globalTipId))
                            {
                                // Already seen this tip. Skip to the next group.
                                continue;
                            }

                            // If tip is from same group as last tip, hold then show if priGroup exhausted at that scanRow
                            // If tip is NOT from the same group as the last tip shown, we have a good tip to return.
                            if (!WasFromLastSeenTipGroup(tipInfo, lastSeenGroupId))
                            {
                                return tipInfo;
                            }

                            // Hold this tip. It should be shown if no other tip found at this scanRow for groupPri/tipPri.
                            sameGroupTip = tipInfo;
                        }
                    }

                    // If we are holding a tip from the last seen tip group, we can return this tip now,
                    // as it is more important than any tip from the next priorityBand.
                    if (sameGroupTip != null)
                    {
                        {
                            return sameGroupTip;
                        }
                    }

                    // Prepare to scan the priorityBand at the next scanRow.
                    scanRow++;
                    
                // If no tips were found at this scan row, we have exhuasted the scan for this priorityBand.
                } while (tipFound);
            }

            //  if none found at the priGroup level, go to next priGroup level and search again.
            return null;
        }

        private string GetGlobalTipId(TipInfo tipInfo)
        {
            return $"{tipInfo.groupId}{GLOBAL_TIP_ID_SEPARATOR}{tipInfo.tipId}";
        }

        private bool WasFromLastSeenTipGroup(TipInfo tipInfo, string lastSeenGroupId)
        {
            return tipInfo.groupId == lastSeenGroupId;
        }

        private bool IsExcludedGroup(string groupId)
        {
            // TODO: Store excluded groups as class variable
            return GetExcludedGroups().Contains(groupId);
        }

        private HashSet<string> GetExcludedGroups()
        {
            // TODO: Fetch excluded groups from VS store (user should be able to ignore specific groups)
            return new HashSet<string>();
        }

        private void ClearTipHistory()
        {
            tipHistory = null;
            tipHistorySet = null;
            TipHistoryManager.ClearTipHistory();
        }

        private List<string> GetTipHistory()
        {
            if (tipHistory == null)
            {
                tipHistory = LoadTipHistory();
            }

            return tipHistory;
        }

        private List<string> LoadTipHistory()
        {
            // Ask the Tip History Manager for all tips seen
            return TipHistoryManager.GetAllTipsSeen();
        }

        private HashSet<string> GetTipHistorySet()
        {
            if (tipHistorySet == null)
            {
                tipHistorySet = new HashSet<string>(GetTipHistory());
            }

            return tipHistorySet;
        }

        private bool IsFirstTime()
        {
            // TODO: Determine if is first time
            return false;
        }

    }
}