using System;
using System.Collections.Generic;
using System.Linq;

namespace HotTips
{
    public class TipCalculator
    {
        private static readonly char GLOBAL_TIP_ID_SEPARATOR = '-';

        // Managers
        public ITipHistoryManager TipHistoryManager;
        public ITipManager TipManager;

        public TipCalculator(ITipHistoryManager tipHistoryManager, ITipManager tipManager = null)
        {
            TipHistoryManager = tipHistoryManager;
            TipManager = tipManager ?? new TipManager();
        }

        public string GetNextTipPath()
        {
            TipInfo tipInfo = GetNextTip();
            return tipInfo?.contentUri;
        }

        public TipInfo GetNextTip()
        {
            List<string> tipHistoryList = TipHistoryManager.GetTipHistory();
            //List<string> tipHistoryList = GetTipHistory();

            string lastSeenGroupId = GetLastTipGroupSeen(tipHistoryList);

            // Get the next tip (using Tip generator algorithm)
            TipInfo nextTip = GetNextTipAlgorithm(lastSeenGroupId);

            return nextTip;
        }

        private static string GetLastTipGroupSeen(List<string> tipHistoryList)
        {
            string lastSeenGroupId = null;
            if (tipHistoryList != null && tipHistoryList.Count > 0)
            {
                string lastTipGlobalId = tipHistoryList.Last();
                // Extract groupId from global tip Id "[GroupId-TipId]"
                lastSeenGroupId = lastTipGlobalId.Split(GLOBAL_TIP_ID_SEPARATOR)[0];
            }
            return lastSeenGroupId;
        }

        private TipInfo GetNextTipAlgorithm(string lastSeenGroupId)
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
                        List<GroupOfTips> tipGroups = TipManager.GetPrioritizedTipGroups()[groupPri-1];
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
                            List<TipInfo>[] tipGroupTipsPriList = tipGroup.TipsPriList;
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

                            TipInfo tipInfo = null;
                            int offset = 0;
                            do
                            {
                                // Assume tipInfo will not be null at this point.
                                tipInfo = tipsAtPriBand[scanRow + offset];
                                offset++;

                                // Find the next unseen tip which doesn't belong to an excluded level.
                                if (!IsExcludedLevel(tipInfo.Level) && !TipHistoryManager.HasTipBeenSeen(tipInfo.globalTipId))
                                {
                                    // Already seen this tip. Skip to the next group.
                                    break;
                                }

                            } while (scanRow + offset < tipsAtPriBand.Count);

                            if (scanRow + offset >= tipsAtPriBand.Count)
                            {
                                // Current group has no unseen tips, move on to the next group.
                                continue;
                            }

                            // TipInfo is not null! We found a tip at this scanRow in this tipPri in this groupPri
                            tipFound = true;


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
                        return sameGroupTip;
                    }

                    // Prepare to scan the priorityBand at the next scanRow.
                    scanRow++;

                // If no tips were found at this scan row, we have exhuasted the scan for this priorityBand.
                } while (tipFound);
            }

            //  if none found at the priGroup level, go to next priGroup level and search again.
            return null;
        }

        private bool IsExcludedLevel(TipLevel level)
        {
            return TipHistoryManager.IsTipLevelExcluded(level);
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
            return TipHistoryManager.IsTipGroupExcluded(groupId);
        }

        private void ClearTipHistory()
        {
            TipHistoryManager.ClearTipHistory();
        }

    }
}