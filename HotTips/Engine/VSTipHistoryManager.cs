using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Collections.Generic;

namespace HotTips
{
    public class VSTipHistoryManager : ITipHistoryManager
    {
        private const string TIP_OF_THE_DAY_SETTINGS = "TipOfTheDay";
        private const string TIP_HISTORY = "TipHistory";
        private static VSTipHistoryManager _instance;

        //private IServiceProvider ServiceProvider;
        private ShellSettingsManager SettingsManager;
        //private SettingsStore SettingsStore;

        //private WritableSettingsStore _userSettingsStore;
        //private WritableSettingsStore UserSettingsStore
        //{
        //    get
        //    {
        //        if (_userSettingsStore == null)
        //        {
        //            ShellSettingsManager settingsManager = new ShellSettingsManager(this.ServiceProvider);
        //            _userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        //        }
        //        return _userSettingsStore;
        //    }
        //}

        private List<string> _tipsSeen;

        public static ITipHistoryManager Instance()
        {
            return _instance ?? (_instance = new VSTipHistoryManager());
        }

        public List<string> GetAllTipsSeen()
        {
            if (_tipsSeen == null) InitialiseTipHistory();
            return _tipsSeen;
        }

        private void InitialiseTipHistory()
        {
            SettingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);

            // Pull tip history from VS settings store
            SettingsStore settingsStore = SettingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);

            // Get the TipHIstory (Comma separated string of TipIDs)
            string tipHistory = GetTipHistoryFromSettingsStore(settingsStore);
            if (tipHistory != null)
            {
                _tipsSeen = new List<string>(tipHistory.Split(','));
            } else
            {
                _tipsSeen = new List<string>();
            }

            //_tipsSeen = new List<string> { "General-GN001", "Editor-ED001" };
        }

        private static string GetTipHistoryFromSettingsStore(SettingsStore settingsStore)
        {
            // Extract values from UserSettingsStore
            string collectionPath = TIP_OF_THE_DAY_SETTINGS;
            if (settingsStore.CollectionExists(collectionPath))
            {
                return settingsStore.GetString(collectionPath, TIP_HISTORY);
            }
            return null;
        }

        public void MarkTipAsSeen(string globalTipId)
        {
            // Add tip ID to the tip history and persist to the VS settings store
            List<string> tipsSeen = GetAllTipsSeen();
            tipsSeen.Add(globalTipId);

            // Update the VS settings store with the latest tip history
            string tipHistoryRaw = String.Join(",", tipsSeen);
            // Get Writable settings store and update value
            WritableSettingsStore settingsStore = SettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            string collectionPath = TIP_OF_THE_DAY_SETTINGS;
            settingsStore.CreateCollection(collectionPath);
            settingsStore.SetString(collectionPath, TIP_HISTORY, tipHistoryRaw);
        }

        public void ClearTipHistory()
        {
            // Clear the local tips object
            _tipsSeen = null;

            // Delete the entries from the Settings Store
            WritableSettingsStore settingsStore = SettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            string collectionPath = TIP_OF_THE_DAY_SETTINGS;
            if (settingsStore.CollectionExists(collectionPath))
            {
                settingsStore.DeleteCollection(collectionPath);
            }
        }
    }
}