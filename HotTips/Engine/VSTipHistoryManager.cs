using HotTips.Engine;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace HotTips
{
    public class VSTipHistoryManager
    {
        private static readonly string SVsSettingsPersistenceManagerGuid = "9B164E40-C3A2-4363-9BC5-EB4039DEF653";

        private static readonly string TIP_OF_THE_DAY_SETTINGS = "TipOfTheDay";
        private static readonly string TIP_HISTORY = TIP_OF_THE_DAY_SETTINGS + "_TipHistory";
        private static readonly string EXCLUDED_TIP_GROUPS = TIP_OF_THE_DAY_SETTINGS + "_ExcludedTipGroups";
        private static readonly string EXCLUDED_TIP_LEVELS = TIP_OF_THE_DAY_SETTINGS + "_ExcludedTipLevels";
        private static readonly string TIP_CADENCE = TIP_OF_THE_DAY_SETTINGS + "_Cadence";
        private static readonly string TIP_LAST_DISPLAY = TIP_OF_THE_DAY_SETTINGS + "_NextDisplay";

        private static readonly bool RoamSettings = true;

        private static VSTipHistoryManager _instance;

        private ISettingsManager _settingsManager;

        private List<TipHistoryInfo> _tipHistory;
        private HashSet<string> _excludedTipGroups;
        private HashSet<TipLevel> _excludedTipLevels;

        private bool _solutionOpenedOnce;

        public static VSTipHistoryManager GetInstance()
        {
            return _instance ?? (_instance = new VSTipHistoryManager());
        }

        public VSTipHistoryManager()
        {
            // Initialize SettingsManager - Note: Must instaniate on UI thread.
            ThreadHelper.ThrowIfNotOnUIThread();
            _settingsManager = (ISettingsManager)ServiceProvider.GlobalProvider.GetService(new Guid(SVsSettingsPersistenceManagerGuid));

            // Initialize Tip History (tips seen and their votes)
            _tipHistory = InitTipHistory();

            // Intialize Excluded Groups (ie. Shell, Editor, Debugging)
            _excludedTipGroups = InitExcludedGroupsSet();

            // Initialize Excluded Levels (ie. Beginer, Advanced)
            _excludedTipLevels = InitExcludedTipLevelSet();
        }

        private List<TipHistoryInfo> InitTipHistory()
        {
            // Get the raw string for tip history from the settings store (Comma separated string of TipIDs)
            _settingsManager.TryGetValue(TIP_HISTORY, out string tipHistoryStr);

            return DeserializeTipHistory(tipHistoryStr);
        }

        private HashSet<string> InitExcludedGroupsSet()
        {
            // Get raw string from settings store
            _settingsManager.TryGetValue(EXCLUDED_TIP_GROUPS, out string excludedGroups);

            return DeserializeExcludedGroups(excludedGroups);
        }

        private HashSet<TipLevel> InitExcludedTipLevelSet()
        {
            // Get the raw string from the settings store
            _settingsManager.TryGetValue(EXCLUDED_TIP_LEVELS, out string excludedTipLevelsRaw);

            return DeserializeExcludedTipLevels(excludedTipLevelsRaw);
        }

        public bool HasTipBeenSeen(string globalTipId)
        {
            List<TipHistoryInfo> tipsSeen = GetTipHistory();
            return tipsSeen != null && tipsSeen.Exists(a => a.GlobalTipId.Equals(globalTipId));
        }

        public List<TipHistoryInfo> GetTipHistory()
        {
            if (_tipHistory == null)
            {
                // Is it even possible to be int this state?
                // Initialize tip history
                _tipHistory = InitTipHistory();
            }
            return _tipHistory;
        }

        public void MarkTipAsSeen(string globalTipId)
        {
            // Back out if tip already seen
            List<TipHistoryInfo> tipsSeen = GetTipHistory();
            if (tipsSeen.Exists(a => a.GlobalTipId.Equals(globalTipId)))
            {
                // Item is already in the history. No need to add it again.
                return;
            }

            // Add tip ID to the tip history and persist to the VS settings store
            tipsSeen.Add(new TipHistoryInfo() { GlobalTipId = globalTipId });

            // Keep Settings Store in sync
            PersistTipHistoryToRoamingSettings();
        }

        public void UpdateTipVoteStatus(string globalTipId, TipLikeEnum currentTipVoteStatus)
        {
            // Grab the existing entry for this tip if there is one
            // Otherwise, create new Tip History entry
            TipHistoryInfo tipHistoryInfo;
            if (_tipHistory.Exists(a => a.GlobalTipId.Equals(globalTipId)))
            {
                tipHistoryInfo = _tipHistory.Find(a => a.GlobalTipId.Equals(globalTipId));
            }
            else
            {
                tipHistoryInfo = new TipHistoryInfo
                {
                    GlobalTipId = globalTipId
                };
                _tipHistory.Add(tipHistoryInfo);
            }

            // Update the voting status for the tip
            tipHistoryInfo.TipLikeStatus = currentTipVoteStatus;

            // Keep Settings Store in sync
            // Update the VS settings store with the latest tip history
            PersistTipHistoryToRoamingSettings();
        }


        public void ClearTipHistory()
        {
            // Clear the local tips object
            _tipHistory = new List<TipHistoryInfo>();

            // Delete the entries from the Settings Store by setting to Empty string
            PersistTipHistoryToRoamingSettings();
        }

        public bool IsTipGroupExcluded(string tipGroupId)
        {
            return _excludedTipGroups.Contains(tipGroupId);
        }

        public void MarkTipGroupAsExcluded(string tipGroupId)
        {
            _excludedTipGroups.Add(tipGroupId);
            PersistExcludedGroupsToRoamingSettings(_excludedTipGroups);
        }

        public void MarkTipGroupAsIncluded(string tipGroupId)
        {
            _excludedTipGroups.Remove(tipGroupId);
            PersistExcludedGroupsToRoamingSettings(_excludedTipGroups);
        }

        public async Task SetCadenceAsync(DisplayCadence cadence)
        {
            await _settingsManager.SetValueAsync(TIP_CADENCE, cadence.Name, isMachineLocal: true);
        }

        public DisplayCadence GetCadence()
        {
            var cadenceString = _settingsManager.GetValueOrDefault(TIP_CADENCE, string.Empty);
            return string.IsNullOrEmpty(cadenceString) ? DisplayCadence.Startup : DisplayCadence.FromName(cadenceString);
        }

        public async Task SetLastDisplayTimeNowAsync() => await SetLastDisplayTimeAsync(DateTime.UtcNow);

        public async Task SetLastDisplayTimeAsync(DateTime dateTime)
        {
            await _settingsManager.SetValueAsync(TIP_LAST_DISPLAY, dateTime, isMachineLocal: true);
        }

        public DateTime GetLastDisplayTime()
        {
            return _settingsManager.GetValueOrDefault(TIP_LAST_DISPLAY, DateTime.UtcNow);
        }

        public bool ShouldShowTip(DisplayCadence cadence)
        {
            var localLastDisplayTime = GetLastDisplayTime().ToLocalTime();
            var localStartOfDay = localLastDisplayTime.Date;
            var withDelay = localStartOfDay + cadence.Delay;
            var appropriateShowTime = withDelay.ToUniversalTime();
            return DateTime.UtcNow >= appropriateShowTime;
        }

        public void HandleVsInitialized()
        {
            var cadence = GetCadence();
            if (cadence == DisplayCadence.Never || cadence == DisplayCadence.SolutionLoad)
                return;

            if (ShouldShowTip(cadence))
            {
                TipOfTheDay.ShowWindow();
                Task.Run(async () => await SetLastDisplayTimeNowAsync());
            }
        }

        public void HandleSolutionOpened()
        {
            var cadence = GetCadence();

            // Allow one operation when cadence is Startup and multiple when cadence is SolutionLoad
            if (!(cadence == DisplayCadence.Startup && !_solutionOpenedOnce)
                && !(cadence == DisplayCadence.SolutionLoad))
                return;

            if (ShouldShowTip(cadence))
            {
                TipOfTheDay.ShowWindow();
                _solutionOpenedOnce = true; // When DisplayCadence is Startup, show tip only once
                Task.Run(async () => await SetLastDisplayTimeNowAsync());
            }
        }

        public bool IsTipLevelExcluded(TipLevel level)
        {
            return _excludedTipLevels.Contains(level);
        }

        public void MarkTipLevelAsIncluded(TipLevel level)
        {
            _excludedTipLevels.Remove(level);
            PersistExcludedLevelsToRoamingSettings();
        }

        public void MarkTipLevelAsExcluded(TipLevel level)
        {
            _excludedTipLevels.Add(level);
            PersistExcludedLevelsToRoamingSettings();
        }

        public ISet<string> GetAllExcludedTipGroups()
        {
            return _excludedTipGroups;
        }

        public ISet<TipLevel> GetAllExcludedTipLevels()
        {
            return _excludedTipLevels;
        }

        //----------- Serialize / Deserialize to SettingsStore -------------------

        /// <summary>
        /// Update the VS settings store with the latest tip history in memory (Roaming user settings)
        /// </summary>
        private void PersistTipHistoryToRoamingSettings()
        {
            // Note: Mark the history as Roaming (ie. Not local machine only). This way a user won't see the same tips on different machines.
            string serializedTipHistory = SerializeTipHistory(_tipHistory);
            _settingsManager.SetValueAsync(TIP_HISTORY, serializedTipHistory, isMachineLocal: !RoamSettings);
        }

        private void PersistExcludedGroupsToRoamingSettings(HashSet<string> excludedTipGroups)
        {
            string value = string.Join(",", excludedTipGroups);
            _settingsManager.SetValueAsync(EXCLUDED_TIP_GROUPS, value, isMachineLocal: !RoamSettings);
        }

        private void PersistExcludedLevelsToRoamingSettings()
        {
            string serializedExcludedTipLevels = SerializeExcludedTipLevels(_excludedTipLevels);
            _settingsManager.SetValueAsync(EXCLUDED_TIP_LEVELS, serializedExcludedTipLevels, isMachineLocal: !RoamSettings);
        }

        //--------- Static methods --------------

        private static string SerializeTipHistory(List<TipHistoryInfo> tipsSeen)
        {
            string rawStr = String.Empty;

            // Expected: [TipId]:[Vote],[TipId]:[Vote]
            foreach (var tipHistoryItem in tipsSeen)
            {
                // Join each entry with a comma
                if (!string.IsNullOrEmpty(rawStr)) rawStr += ",";
                // Add the new entry. Serialize it with [TipId]:[Vote] eg. "ED001:1"
                rawStr += string.Concat(tipHistoryItem.GlobalTipId, ':', (int)tipHistoryItem.TipLikeStatus);
            }

            return rawStr;
        }

        private static List<TipHistoryInfo> DeserializeTipHistory(string tipHistoryStr)
        {
            // Return an empty list in no items found.
            List<TipHistoryInfo> tipsInfo = new List<TipHistoryInfo>();

            // Deserialize the raw string. Format = [TipId]:[Vote],[TipId]:[Vote] (eg. "ED001:1,SHL001:0")
            // Note: Vote field might be missing (esp if people have previously run the older version before voting was stored)
            if (!string.IsNullOrEmpty(tipHistoryStr))
            {
                // Split tip history items on comma (',') and parse each tip history item
                foreach (string tipHistoryItemStr in tipHistoryStr.Split(','))
                {
                    TipHistoryInfo tipHistoryItem = DeserializeTipHistoryItem(tipHistoryItemStr);
                    tipsInfo.Add(tipHistoryItem);
                }
            }

            return tipsInfo;
        }

        private static TipHistoryInfo DeserializeTipHistoryItem(string tipHistoryItemStr)
        {
            TipHistoryInfo tipHistoryItem = new TipHistoryInfo();

            // Split on colon ':' eg. [TipId]:[Vote]
            string[] parts = tipHistoryItemStr.Split(':');

            // The first part is the GlobalTipId
            tipHistoryItem.GlobalTipId = parts[0];

            // The second part is the Vote
            if (parts.Length > 0)
            {
                string tipVote = parts[1];
                if (Enum.TryParse(tipVote, out TipLikeEnum tipVoteEnum))
                {
                    tipHistoryItem.TipLikeStatus = tipVoteEnum;
                }
            }

            return tipHistoryItem;
        }

        private static HashSet<string> DeserializeExcludedGroups(string excludedGroups)
        {
            return string.IsNullOrEmpty(excludedGroups)
                                        ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                                        : new HashSet<string>(excludedGroups.Split(','), StringComparer.OrdinalIgnoreCase);
        }

        private static string SerializeExcludedTipLevels(HashSet<TipLevel> excludedTipLevels)
        {
            return string.Join(",", excludedTipLevels.Select(l => l.ToString()));
        }

        private static HashSet<TipLevel> DeserializeExcludedTipLevels(string excludedTipLevelsRaw)
        {
            // Will return an empty set if none already seen.
            var excludedTipLevels = new HashSet<TipLevel>();

            // Parse the string into a Set of excluded tip levels
            if (!string.IsNullOrEmpty(excludedTipLevelsRaw))
            {
                // Parse the Excluded Tip Levels
                foreach (var level in excludedTipLevelsRaw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (Enum.TryParse(level, out TipLevel value))
                    {
                        excludedTipLevels.Add(value);
                    }
                }
            }

            return excludedTipLevels;
        }

    }
}