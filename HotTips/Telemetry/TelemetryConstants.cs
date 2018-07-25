using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotTips.Telemetry
{
    internal class TelemetryConstants
    {
        public const string TipEventPrefix = "Justcla/HotTips/";
        public const string TipShownEvent = TipEventPrefix + "TipShown";
        public const string NextTipShown = TipEventPrefix + "NextTipShown";
        public const string PrevTipShown = TipEventPrefix + "PrevTipShown";
        public const string TipLikedEvent = TipEventPrefix + "TipLiked";
        public const string TipLikeCanceledEvent = TipEventPrefix + "TipLikeCanceled";
        public const string TipDisLikedEvent = TipEventPrefix + "TipDisLiked";
        public const string TipDisLikeCanceledEvent = TipEventPrefix + "TipDisLikeCanceled";
        public const string TipGroupDisabled = TipEventPrefix + "TipGroupDisabled";
        public const string TipGroupEnabled = TipEventPrefix + "TipGroupEnabled";
        public const string MoreLikeThisTipClicked = TipEventPrefix + "MoreTipsSelected";
        public const string DialogClosed = TipEventPrefix + "TipOfTheDayDialogClosed";

        // Options page Events
        public const string OptionsPageShown = TipEventPrefix + "TipOptionsPageShown";
        public const string OptionsPageDismissed = TipEventPrefix + "TipOptionsPageDismissed";
        public const string NextShowChanged = TipEventPrefix + "WhenToShowNextChanged";

        public const string TipLevelEnabled = TipEventPrefix + "TipLevelEnabled";
        public const string TipLevelDisabled = TipEventPrefix + "TipLevelDisabled";

        public const string PackageLoad = TipEventPrefix + "HotTipsPackageLoad";

    }
}
