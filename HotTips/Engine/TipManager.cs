using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EnvDTE;

namespace HotTips
{
    internal class TipManager : ITipManager
    {
        private List<GroupOfTips>[] _groupsPriList;
        private Dictionary<string, TipInfo> _allTips;
        private List<GroupOfTips> _allGroups { get; set; }

        public List<GroupOfTips>[] GetPrioritizedTipGroups()
        {
            if (_groupsPriList == null)
            {
                ProcessTipGroupProviders();
            }
            return _groupsPriList;
        }

        public bool TipExists(string globalTipId)
        {
            if (_allTips == null)
            {
                DebugLogInitAllTips(globalTipId);
                return false;
            }
            return _allTips.ContainsKey(globalTipId);
        }

        public TipInfo GetTipInfo(string globalTipId)
        {
            if (_allTips == null)
            {
                DebugLogInitAllTips(globalTipId);
                return null;
            }
            return _allTips[globalTipId];
        }

        public TipInfo GetNextTipInGroup(string globalTipId)
        {
            // Get groupId and tipId of current tip
            string currentGroupId = TipInfo.GetGroupId(globalTipId);
            string currentTipId = TipInfo.GetTipId(globalTipId);

            // See if there is a further tip later in the list
            GroupOfTips currentGroup = _allGroups.Find(x => x.groupId == currentGroupId);
            List<string> tips = currentGroup.TipsSorted;
            int currentTipIndex = tips.IndexOf(currentTipId);
            int nextTipIndex = currentTipIndex + 1;
            if (nextTipIndex >= tips.Count)
            {
                // There are no more tips in this group
                Debug.WriteLine($"Tip of the Day: There are no more tips in tip group '{currentGroupId}' after tip '{globalTipId}'");
                return null;
            }

            // We have a tip. Fetch it and return it.
            string nextTipId = tips[nextTipIndex];
            return _allTips[TipInfo.GetGlobalTipId(currentGroupId, nextTipId)];
        }

        /// <summary>
        /// Top level Tip Group Process. Import all groups from all group providers.
        /// </summary>
        private void ProcessTipGroupProviders()
        {
            var newTips = new Dictionary<string, TipInfo>();

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
                    Debug.WriteLine($"Reading tip group: {groupFile}");
                    if (!File.Exists(groupFile))
                    {
                        // Unable to read group file from disc. Bail out.
                        Debug.WriteLine($"Unable to read tip group JSON file from disc: {groupFile}");
                        continue;
                    }

                    Debug.WriteLine($"Found file: {groupFile}");

                    // A groupFile is the file path of a JSON file that defines the tips for a group
                    // Parse the group file and extract a TipGroup object with a list of Tips
                    string jsonString = GetJsonStringFromFile(groupFile);
                    TipGroup tipGroup = JsonConvert.DeserializeObject<TipGroup>(jsonString);

                    ProcessTipGroup(tipGroupProvider, tipGroup, newTips);
                }
            }

            // Update the allTips list
            _allTips = newTips;
        }

        /// <summary>
        /// Import a single tip group. Parse all the tips declared in the group. Sort them by priority.
        /// </summary>
        private void ProcessTipGroup(ITipGroupProvider tipGroupProvider, TipGroup tipGroup, Dictionary<string, TipInfo> newTips)
        {
            // Create a new GroupOfTips for the tip group being processed
            GroupOfTips groupOfTips = GroupOfTips.Create(tipGroup);

            foreach (Tip tip in tipGroup.tips)
            {
                // Generate the tip content URI (from the provider)
                string tipContentUri = tipGroupProvider.GetTipPath(tip.content);
                tip.content = tipContentUri;

                // Add the TipInfo to the groupOfTips
                TipInfo tipInfo = TipInfo.Create(tipGroup, tip, tipContentUri);
                AddTipToPriListOfTips(groupOfTips, tipInfo);

                // Also add to the AllTips lookup dictionary
                newTips.Add(tipInfo.globalTipId, tipInfo);
            }

            // Build an ordered tips list (based on priority)
            List<string> tipsList = CreateSortedTipsList(groupOfTips.TipsPriList);
            groupOfTips.TipsSorted = tipsList;

            // Add the TipGroup to the correct PriList of ordered Groups (GroupsPriList)
            AddTipGroupToGroupsPriList(groupOfTips, tipGroup.groupPriority);

            // Add the TipGroup to the AllGroups lookup
            AddTipGroupToAllGroupsSet(groupOfTips);
        }

        private static List<string> CreateSortedTipsList(List<TipInfo>[] tipsPriList)
        {
            var tipsList = new List<string>();

            // For each of the priority groups (1-3), check for null and add all tips to the ordered list.
            var tipPriLists = tipsPriList.Where(x => x != null);
            foreach (List<TipInfo> tipPriList in tipPriLists)
            {
                tipsList.AddRange(tipPriList.Select(x => x.tipId));
            }

            return tipsList;
        }

        private void AddTipToPriListOfTips(GroupOfTips groupOfTips, TipInfo tipInfo)
        {
            // Add Tip to the correct prioritized tip list within the groupOfTips
            int tipPriority = tipInfo.priority;
            List<TipInfo> tipList = groupOfTips.TipsPriList[tipPriority - 1];
            // Initialize the tipList if required
            if (tipList == null)
            {
                tipList = new List<TipInfo>();
                groupOfTips.TipsPriList[tipPriority - 1] = tipList;
            }

            tipList.Add(tipInfo);
        }

        private void AddTipGroupToGroupsPriList(GroupOfTips groupOfTips, int groupPriority)
        {
            // Initialze GroupsPriList if required
            if (_groupsPriList == null)
            {
                _groupsPriList = new List<GroupOfTips>[3];
            }

            List<GroupOfTips> groupsList = _groupsPriList[groupPriority - 1];
            // Initialize groupsList if required
            if (groupsList == null)
            {
                groupsList = new List<GroupOfTips>();
                _groupsPriList[groupPriority - 1] = groupsList;
            }

            groupsList.Add(groupOfTips);
        }

        private void AddTipGroupToAllGroupsSet(GroupOfTips groupOfTips)
        {
            if (_allGroups == null)
            {
                _allGroups = new List<GroupOfTips>();
            }

            _allGroups.Add(groupOfTips);
        }

        private string GetJsonStringFromFile(string groupFile)
        {
            string json;
            // Read the file into string
            using (StreamReader r = new StreamReader(groupFile))
            {
                json = r.ReadToEnd();
            }

            return json;
        }

        private IEnumerable<ITipGroupProvider> GetTipGroupProviders()
        {
            List<ITipGroupProvider> tipGroupProviders = new List<ITipGroupProvider>();
            // Add the Embedded Tips Provider
            tipGroupProviders.Add(EmbeddedTipsProvider.Instance());
            // In future: Add other tip providers
            return tipGroupProviders;
        }

        private static void DebugLogInitAllTips(string globalTipId)
        {
            Debug.WriteLine($"Cannot find tip: {globalTipId}. Internal _allTips dictionary has not been initialized.");
        }

    }
}