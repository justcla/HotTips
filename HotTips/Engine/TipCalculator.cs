using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace HotTips
{
    public class TipCalculator
    {
        private static readonly string TIP_OF_THE_DAY_TITLE = "Tip of the Day";
        private static readonly char GLOBAL_TIP_ID_SEPARATOR = '-';

        // Managers
        private ITipHistoryManager _tipHistoryManager;
        private ITipManager _tipManager;

        public TipCalculator(ITipHistoryManager tipHistoryManager, ITipManager tipManager = null)
        {
            _tipHistoryManager = tipHistoryManager;
            _tipManager = tipManager ?? new TipManager();
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

            return CalculateNextTip();
        }

        private TipInfo CalculateNextTip()
        {
            List<string> tipHistoryList = _tipHistoryManager.GetTipHistory();
            //List<string> tipHistoryList = GetTipHistory();

            string lastSeenGroupId = GetLastTipGroupSeen(tipHistoryList);

            // Get the next tip (using Tip generator algorithm)
            TipInfo nextTip = GetNextTipAlgorithm(lastSeenGroupId);

            // If we found a tip, return it.
            if (nextTip != null) return nextTip;

            // No new tips to show.

            // Check strange case - No new tips and no tips shown.
            if (tipHistoryList == null)
            {
                // No tip found and there are none in the history, so we must have no tips at all. (Strange case)
                Debug.WriteLine("No tips seen and no tips yet to see. (Check if tip groups loaded correctly.)");
                return null;
            }

            // Ask user if they want to start fresh from the beginning.
            return ResetAndGetTipFromBeginning();
        }

        private TipInfo ResetAndGetTipFromBeginning()
        {
            // Ask user if they want to clear the history and start again from the beginning.
            const string Text = "No new tips to show.\n\nClear tip history and start showing tips from the beginning?";
            if (MessageBox.Show(Text, TIP_OF_THE_DAY_TITLE, MessageBoxButtons.OKCancel) != DialogResult.OK)
            {
                // User does not want to reset history. Exit nicely.
                return null;
            }

            // Reset Tip History and fetch next tip
            ClearTipHistory();

            return GetNextTipAlgorithm(lastSeenGroupId: null);
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
                        List<GroupOfTips> tipGroups = _tipManager.GetPrioritizedTipGroups()[groupPri-1];
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
                            //HashSet<string> tipHistorySet = GetTipHistorySet();
                            //if (tipHistorySet.Contains(tipInfo.globalTipId))
                            if (_tipHistoryManager.HasTipBeenSeen(tipInfo.globalTipId))
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
            //tipHistory = null;
            //tipHistorySet = null;
            _tipHistoryManager.ClearTipHistory();
        }

        private bool IsFirstTime()
        {
            // TODO: Determine if is first time
            return false;
        }

    }
}