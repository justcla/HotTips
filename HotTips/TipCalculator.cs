using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace HotTips
{
    public class TipCalculator
    {
        public static string GetNextTipPath()
        {
            TipInfo tipInfo = GetNextTip();
            ITipGroupProvider tipProvider = tipInfo.Provider;

            string tipId = tipInfo.TipId;
            string tipPath = tipProvider.GetTipPath(tipInfo.Content);

            return tipPath;
        }

        private static TipInfo GetNextTip()
        {
            // TODO: Work out the next tip.
            TipInfo tipInfo = null;

            // First time, set tipInfo to Tip001
            tipInfo = new TipInfo
            {
                Provider = new EmbeddedTipsProvider(),
                TipId = "GN001",
                Content = "Tip001.html"
            };

            //List<TipInfo> allTips = GetAllTips();

            return tipInfo;
        }

        public static List<TipGroup> GetAllTipGroups()
        {
            var allTips = new List<TipGroup>();

            // Get all tip group providers
            List<ITipGroupProvider> tipGroupProviders = GetTipGroupProviders();
            foreach (ITipGroupProvider tipGroupProvider in tipGroupProviders)
            {
                List<string> groupFiles = tipGroupProvider.GetGroupDefinitions();
                // Parse each tip group
                foreach (string groupFile in groupFiles)
                {
                    // A groupFile is the file path of a JSON file that defines the tips for a group
                    // Read the file (as stream)
                    System.Diagnostics.Debug.WriteLine($"Reading tip group: {groupFile}");
                    if (File.Exists(groupFile))
                    {
                        System.Diagnostics.Debug.WriteLine($"Found file: {groupFile}");
                        string jsonString = GetJsonStringFromFile(groupFile);
                        // Parse the group file and extract a TipGroup object with a list of Tips
                        TipGroup tipGroup = JsonConvert.DeserializeObject<TipGroup>(jsonString);

                        // Add each tip group
                        allTips.Add(tipGroup);
                    }

                    // TODO: Generate list of groups, with list of tips
                }
            }

            return allTips;
        }

        private static string GetJsonStringFromFile(string groupFile)
        {
            string json = null;
            // Read the file into string
            using (StreamReader r = new StreamReader(groupFile))
            {
                json = r.ReadToEnd();
            }
            return json;
        }

        private static List<ITipGroupProvider> GetTipGroupProviders()
        {
            List<ITipGroupProvider> tipGroupProviders = new List<ITipGroupProvider>();
            // Add the Embedded Tips Provider
            tipGroupProviders.Add(new EmbeddedTipsProvider());
            // In future: Add other tip providers
            return tipGroupProviders;
        }
    }

    public class TipInfo
    {
        public ITipGroupProvider Provider { get; internal set; }
        public string TipId { get; internal set; }
        public string Content { get; internal set; }
    }

}