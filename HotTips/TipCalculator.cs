using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace HotTips
{
    public class TipCalculator
    {
        public static List<GroupOfTips>[] groupsPriList { get; set; }

        public static string GetNextTipPath()
        {
            TipInfo tipInfo = GetNextTip();
            string rawTipContentPath = tipInfo.contentUri;
            string tipContentUri = EmbeddedTipsProvider.Instance().GetTipPath(rawTipContentPath);
            return tipContentUri;
        }

        private static TipInfo GetNextTip()
        {
            // TODO: Work out the next tip.

            // First time, set tipInfo to Tip001
            return new TipInfo
            {
                tipId = "GN001",
                contentUri = EmbeddedTipsProvider.Instance().GetTipPath("Tip001.html")
            };
        }

        public static List<GroupOfTips>[] GetAllTipGroups()
        {
            // Get all tip group providers
            IEnumerable<ITipGroupProvider> tipGroupProviders = GetTipGroupProviders();
            foreach (ITipGroupProvider tipGroupProvider in tipGroupProviders)
            {
                List<string> groupFiles = tipGroupProvider.GetGroupDefinitions();
                // Parse each tip group
                foreach (string groupFile in groupFiles)
                {
                    // Read the group. Parse all tips. Create a TipGroup object with PriList of ordered Tips.

                    // Check that the groupFile exists
                    System.Diagnostics.Debug.WriteLine($"Reading tip group: {groupFile}");
                    if (!File.Exists(groupFile))
                    {
                        // Unable to read group file from disc. Bail out.
                        System.Diagnostics.Debug.WriteLine($"Unable to read tip group JSON file from disc: {groupFile}");
                        continue;
                    }
                    System.Diagnostics.Debug.WriteLine($"Found file: {groupFile}");

                    // A groupFile is the file path of a JSON file that defines the tips for a group
                    // Parse the group file and extract a TipGroup object with a list of Tips
                    string jsonString = GetJsonStringFromFile(groupFile);
                    TipGroup tipGroup = JsonConvert.DeserializeObject<TipGroup>(jsonString);

                    // Create a new GroupOfTips
                    GroupOfTips groupOfTips = InitializeGroupOfTips(tipGroup);

                    foreach (Tip tip in tipGroup.tips)
                    {
                        // Generate the tip content URI (from the provider)
                        string tipContentUri = tipGroupProvider.GetTipPath(tip.content);
                        tip.content = tipContentUri;

                        // Add the TipInfo to the groupOfTips
                        TipInfo tipInfo = TipInfo.Create(tipGroup, tip, tipContentUri);
                        AddTipToPriListOfTips(groupOfTips, tipInfo);
                    }

                    // Add the TipGroup to the correct PriList of ordered Groups (GroupsPriList)
                    AddTipGroupToGroupsPriList(groupOfTips, tipGroup.groupPriority);
                }
            }

            return groupsPriList;
        }

        private static GroupOfTips InitializeGroupOfTips(TipGroup tipGroup)
        {
            return new GroupOfTips
            {
                groupId = tipGroup.groupId,
                groupName = tipGroup.groupName,
                groupPriority = tipGroup.groupPriority,
                tipsPriList = new List<TipInfo>[3]
            };
        }

        private static void AddTipToPriListOfTips(GroupOfTips groupOfTips, TipInfo tipInfo)
        {
            // Add Tip to the correct prioritized tip list within the groupOfTips
            int tipPriority = tipInfo.priority;
            List<TipInfo> tipList = groupOfTips.tipsPriList[tipPriority - 1];
            // Initialize the tipList if required
            if (tipList == null)
            {
                tipList = new List<TipInfo>();
                groupOfTips.tipsPriList[tipPriority - 1] = tipList;
            }

            tipList.Add(tipInfo);
        }

        private static void AddTipGroupToGroupsPriList(GroupOfTips groupOfTips, int groupPriority)
        {
            // Initialze GroupsPriList if required
            if (groupsPriList == null)
            {
                groupsPriList = new List<GroupOfTips>[3];
            }
            List<GroupOfTips> groupsList = groupsPriList[groupPriority - 1];
            // Initialize groupsList if required
            if (groupsList == null)
            {
                groupsList = new List<GroupOfTips>();
                groupsPriList[groupPriority - 1] = groupsList;
            }
            groupsList.Add(groupOfTips);
        }

        private static string GetJsonStringFromFile(string groupFile)
        {
            string json;
            // Read the file into string
            using (StreamReader r = new StreamReader(groupFile))
            {
                json = r.ReadToEnd();
            }
            return json;
        }

        private static IEnumerable<ITipGroupProvider> GetTipGroupProviders()
        {
            List<ITipGroupProvider> tipGroupProviders = new List<ITipGroupProvider>();
            // Add the Embedded Tips Provider
            tipGroupProviders.Add(EmbeddedTipsProvider.Instance());
            // In future: Add other tip providers
            return tipGroupProviders;
        }
    }

}