using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotTips
{
    internal class TelemetryConstants
    {
        public const string TipEventPrefix = "Justcla/HotTips/";
        public const string TipShownEvent = TipEventPrefix + "TipShown";
        public const string TipLikedEvent = TipEventPrefix + "TipLiked";
        public const string TipLikeCanceledEvent = TipEventPrefix + "TipLikeCanceled";
        public const string TipDisLikedEvent = TipEventPrefix + "TipDisLiked";
        public const string TipDisLikeCanceledEvent = TipEventPrefix + "TipDisLikeCanceled";
        public const string TipGroupDisabled = TipEventPrefix + "TipGroupDisabled";
        public const string TipGroupEnabled = TipEventPrefix + "TipGroupEnabled";
        public const string MoreLikeThisTipClicked = TipEventPrefix + "MoreTipsSelected";
    }
}
