using System.Collections.Generic;

namespace HotTips
{
    public class VSTipHistoryManager : ITipHistoryManager
    {
        private static VSTipHistoryManager _instance;
        
        private List<string> _tipsSeen;

        public List<string> GetAllTipsSeen()
        {
            if (_tipsSeen == null) InitialiseTipHistory();
            return _tipsSeen;
        }

        private void InitialiseTipHistory()
        {
            // TODO: Pull tip history from VS settings store
            _tipsSeen = new List<string> {"General-GN001", "Editor-ED001"};
        }

        public void MarkTipAsSeen(string globalTipId)
        {
            // TODO: Add tip ID to the tip history and persist to the VS settings store
            _tipsSeen.Add(globalTipId);
            // TODO: Update the VS settings store with the latest tip history
        }

        public static ITipHistoryManager Instance()
        {
            return _instance ?? (_instance = new VSTipHistoryManager());
        }
    }
}