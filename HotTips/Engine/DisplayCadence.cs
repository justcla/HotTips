using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotTips.Engine
{
    [Serializable]
    public struct DisplayCadence
    {
        public string Name { get; }
        public TimeSpan Delay { get; }
        public bool ShowEstimate { get; }

        public static DisplayCadence VSStartup { get; } = new DisplayCadence("on solution load", TimeSpan.FromSeconds(0), showEstimate: false);
        public static DisplayCadence Daily { get; } = new DisplayCadence("Daily", TimeSpan.FromDays(1), showEstimate: true);
        public static DisplayCadence Weekly { get; } = new DisplayCadence("Weekly", TimeSpan.FromDays(7), showEstimate: true);
        public static DisplayCadence Monthly { get; } = new DisplayCadence("Monthly", TimeSpan.FromDays(30), showEstimate: true);
        public static DisplayCadence Never { get; } = new DisplayCadence("Never", TimeSpan.MaxValue, showEstimate: false);
        public static IReadOnlyCollection<DisplayCadence> KnownDisplayCadences { get; } = new DisplayCadence[] { VSStartup, Daily, Weekly, Monthly, Never };

        public DisplayCadence(string name, TimeSpan delay, bool showEstimate)
        {
            Name = name;
            Delay = delay;
            ShowEstimate = showEstimate;
        }

        public static DisplayCadence FromName(string name) => KnownDisplayCadences.FirstOrDefault(n => n.Name == name);

        public override string ToString() => Name;
    }
}
