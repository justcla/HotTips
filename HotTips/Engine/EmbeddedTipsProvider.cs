using System.Collections.Generic;

namespace HotTips
{
    public class EmbeddedTipsProvider : ITipGroupProvider
    {
        private static EmbeddedTipsProvider _instance;
        public static EmbeddedTipsProvider Instance()
        {
            if (_instance == null)
            {
                _instance = new EmbeddedTipsProvider();
            }
            return _instance;
        }

        public List<string> GetGroupDefinitions()
        {
            // TODO: Read all files from the /Groups dir
            List<string> tipGroups = new List<string>();
            tipGroups.Add(GetGroupPath("general"));
            tipGroups.Add(GetGroupPath("editor"));
            tipGroups.Add(GetGroupPath("shell"));
            tipGroups.Add(GetGroupPath("navigation"));
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
