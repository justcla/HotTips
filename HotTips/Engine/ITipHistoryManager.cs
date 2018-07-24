using HotTips.Engine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        Task SetCadenceAsync(DisplayCadence cadence);
        DisplayCadence GetCadence();
        Task SetLastDisplayTimeNowAsync();
        Task SetLastDisplayTimeAsync(DateTime dateTime);
        DateTime GetLastDisplayTime();
        bool ShouldShowTip(DisplayCadence cadence);
        void HandleVsInitialized();
        void HandleSolutionOpened();
        bool IsTipLevelExcluded(TipLevel tipLevel);
        void MarkTipLevelAsExcluded(TipLevel tipLevel);
        void MarkTipLevelAsIncluded(TipLevel tipLevel);
    }
}