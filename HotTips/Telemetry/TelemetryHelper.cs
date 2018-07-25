using Justcla;
using System.Linq;

namespace HotTips.Telemetry
{
    internal static class TelemetryHelper
    {
        public static void LogTelemetryEvent(VSTipHistoryManager manager, string eventName, params object[] nameAndProperties)
        {
            var excludedGroups = manager.GetAllExcludedTipGroups();
            var excludedTipLevels = manager.GetAllExcludedTipLevels();
            var tipHistory = manager.GetTipHistory();

            string excludedGroupStr = string.Join(";", excludedGroups);
            string excludedLevelStr = string.Join(";", excludedTipLevels.Select(l => l.ToString()));
            string tipHistoryStr = string.Join(";", tipHistory);

            var properties = nameAndProperties.Concat(
                new[]
                {
                    "ExcludedTipGroups", excludedGroupStr,
                    "ExcludedTipLevels", excludedLevelStr,
                    "TipHistory", tipHistoryStr
                }).ToArray();

            VSTelemetryHelper.PostEvent(eventName,
                properties);
        }

    }
}
