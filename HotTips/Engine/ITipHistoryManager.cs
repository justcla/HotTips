using System.Collections.Generic;

namespace HotTips
{
    public interface ITipHistoryManager
    {
        List<string> GetAllTipsSeen();
        void  MarkTipAsSeen(string globalTipId);
    }
}