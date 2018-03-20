using System.Collections.Generic;

namespace HotTips
{
    class EmbeddedTipsProvider : ITipGroupProvider
    {
        public List<string> GetGroupDefinitions()
        {
            List<string> tipGroups = new List<string>();
            tipGroups.Add(GetGroupPath("general"));
            tipGroups.Add(GetGroupPath("editor"));
            return tipGroups;
        }

        private static string GetGroupPath(string groupName)
        {
            return Utils.GetLocalExtensionDir() + "/Groups/" + groupName + ".json";
        }

        public string GetTipPath(string contentFile)
        {
            return Utils.GetLocalExtensionDir() + "/Tips/" + contentFile;
        }
    }
}
