using System.Collections.Generic;

namespace HotTips
{
    public interface ITipHistoryManager
    {
        List<string> GetTipHistory();
        bool HasTipBeenSeen(string globalTipId);
        void MarkTipAsSeen(string globalTipId);
        void ClearTipHistory();
        bool IsTipGroupExcluded(string tipGroupId);
        void MarkTipGroupAsExcluded(string tipGroupId);
        void MarkTipGroupAsIncluded(string tipGroupId);
    }
}