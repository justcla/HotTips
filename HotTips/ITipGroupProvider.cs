using System.Collections.Generic;

namespace HotTips
{
    public interface ITipGroupProvider
    {
        List<string> GetGroupDefinitions();
        string GetTipPath(string tipId);
    }
}