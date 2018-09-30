using System.Collections.Generic;

namespace HotTips
{
    public interface ITipManager
    {
        List<GroupOfTips>[] GetPrioritizedTipGroups();
        bool TipExists(string globalTipId);
        TipInfo GetTipInfo(string globalTipId);
        TipInfo GetNextTipInGroup(string globalTipId);
    }
}