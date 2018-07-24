using HotTips.Engine;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace HotTips
{
    public class VSTipHistoryManager : ITipHistoryManager
    {
        private static readonly string SVsSettingsPersistenceManagerGuid = "9B164E40-C3A2-4363-9BC5-EB4039DEF653";
        private static readonly string TIP_OF_THE_DAY_SETTINGS = "TipOfTheDay";
        private static readonly string TIP_HISTORY = "TipHistory";
        private static readonly string EXCLUDED_TIP_GROUPS = TIP_OF_THE_DAY_SETTINGS+"_ExcludedTipGroups";
        private static readonly string TIP_CADENCE = TIP_OF_THE_DAY_SETTINGS + "_Cadence";
        private static readonly string TIP_NEXT_DISPLAY = TIP_OF_THE_DAY_SETTINGS + "_NextDisplay";
        private static readonly bool RoamSettings = true;

        private static VSTipHistoryManager _instance;

        private ISettingsManager SettingsManager;
        private List<string> _tipsSeen;
        private HashSet<string> _excludedTipGroups;

        public static ITipHistoryManager GetInstance()

        {
            return _instance ?? (_instance = new VSTipHistoryManager());
        }

        public bool HasTipBeenSeen(string globalTipId)
        {
            List<string> tipsSeen = GetTipHistory();
            return tipsSeen != null && tipsSeen.Contains(globalTipId);
        }

        public List<string> GetTipHistory()
        {
            return _tipsSeen;
        }

        public VSTipHistoryManager()
        {
            // Note: Must instaniate on UI thread.
            ThreadHelper.ThrowIfNotOnUIThread();
            SettingsManager = (ISettingsManager)ServiceProvider.GlobalProvider.GetService(new Guid(SVsSettingsPersistenceManagerGuid));

            // Pull tip history from VS settings store (Comma separated string of TipIDs)
            string tipHistory = GetTipHistoryFromSettingsStore();
            _tipsSeen = (!string.IsNullOrEmpty(tipHistory)) ? new List<string>(tipHistory.Split(',')) : new List<string>();

            SettingsManager.TryGetValue(EXCLUDED_TIP_GROUPS, out string excludedGroups);

            _excludedTipGroups = (!string.IsNullOrEmpty(excludedGroups))
                ? new HashSet<string>(excludedGroups.Split(','), StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        private string GetTipHistoryFromSettingsStore()
        {
            string collectionPath = TIP_OF_THE_DAY_SETTINGS;
            SettingsManager.TryGetValue(collectionPath, out string tipHistoryRaw);
            return tipHistoryRaw;
        }

        public void MarkTipAsSeen(string globalTipId)
        {
            // Back out if tip already seen
            List<string> tipsSeen = GetTipHistory();
            if (tipsSeen.Contains(globalTipId))
            {
                // Item is already in the history. No need to add it again.
                return;
            }

            // Add tip ID to the tip history and persist to the VS settings store
            tipsSeen.Add(globalTipId);

            // Update the VS settings store with the latest tip history
            string tipHistoryRaw = String.Join(",", tipsSeen);
            // Get Writable settings store and update value
            string collectionPath = TIP_OF_THE_DAY_SETTINGS;
            //ISettingsList settingsList = SettingsManager.GetOrCreateList(collectionPath, isMachineLocal: !RoamSettings);
            SettingsManager.SetValueAsync(collectionPath, tipHistoryRaw, isMachineLocal: !RoamSettings);
        }

        public void ClearTipHistory()
        {
            // Clear the local tips object
            _tipsSeen = null;

            // Delete the entries from the Settings Store by setting to Empty string
            string collectionPath = TIP_OF_THE_DAY_SETTINGS;
            SettingsManager.SetValueAsync(collectionPath, string.Empty, isMachineLocal: !RoamSettings);

        }

        public bool IsTipGroupExcluded(string tipGroupId)
        {
            return _excludedTipGroups.Contains(tipGroupId);
        }

        public void MarkTipGroupAsExcluded(string tipGroupId)
        {
            _excludedTipGroups.Add(tipGroupId);
            StoreExcludedGroupsToSettings();
        }

        private void StoreExcludedGroupsToSettings()
        {
            string value = string.Join(",", _excludedTipGroups);
            SettingsManager.SetValueAsync(EXCLUDED_TIP_GROUPS, value, isMachineLocal: !RoamSettings);
        }

        public void MarkTipGroupAsIncluded(string tipGroupId)
        {
            _excludedTipGroups.Remove(tipGroupId);
            StoreExcludedGroupsToSettings();
        }

        public async Task SetCadenceAsync(DisplayCadence cadence)
        {
            await SettingsManager.SetValueAsync(TIP_CADENCE, cadence.Name, isMachineLocal: true);
        }

        public DisplayCadence GetCadence()
        {
            var cadenceString = SettingsManager.GetValueOrDefault(TIP_CADENCE, string.Empty);
            return string.IsNullOrEmpty(cadenceString) ? DisplayCadence.VSStartup : DisplayCadence.FromName(cadenceString);
        }

        public async Task SetLastDisplayTimeNowAsync() => await SetLastDisplayTimeAsync(DateTime.UtcNow);

        public async Task SetLastDisplayTimeAsync(DateTime dateTime)
        {
            await SettingsManager.SetValueAsync(TIP_NEXT_DISPLAY, dateTime, isMachineLocal: true);
        }

        public DateTime GetLastDisplayTime()
        {
            return SettingsManager.GetValueOrDefault(TIP_NEXT_DISPLAY, DateTime.UtcNow);
        }

        public bool ShouldShowTip()
        {
            return DateTime.UtcNow >= GetLastDisplayTime() + GetCadence().Delay;
        }
    }
}