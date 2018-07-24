using System.Collections.Generic;

namespace HotTips
{
    public interface ITipHistoryManager
    {
        List<TipHistoryInfo> GetTipHistory();
        bool HasTipBeenSeen(string globalTipId);
        void MarkTipAsSeen(TipHistoryInfo globalTipId);
        void ClearTipHistory();
        bool IsTipGroupExcluded(string tipGroupId);
        void MarkTipGroupAsExcluded(string tipGroupId);
        void MarkTipGroupAsIncluded(string tipGroupId);

        void SaveTipStatus(TipHistoryInfo globalTipId);
    }
}