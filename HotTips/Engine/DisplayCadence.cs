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

        public static IEnumerable<DisplayCadence> KnownDisplayCadences = new[] { VSStartup, Daily, Weekly, Monthly, Never };
        public static DisplayCadence VSStartup { get; } = new DisplayCadence("on solution load", TimeSpan.FromSeconds(0));
        public static DisplayCadence Daily { get; } = new DisplayCadence("Daily", TimeSpan.FromDays(1));
        public static DisplayCadence Weekly { get; } = new DisplayCadence("Daily", TimeSpan.FromDays(7));
        public static DisplayCadence Monthly { get; } = new DisplayCadence("Monthly", TimeSpan.FromDays(30));
        public static DisplayCadence Never { get; } = new DisplayCadence("Never", TimeSpan.MaxValue);

        public DisplayCadence(string name, TimeSpan delay)
        {
            Name = name;
            Delay = delay;
        }

        public static DisplayCadence FromName(string name) => KnownDisplayCadences.FirstOrDefault(n => n.Name == name);

        public override string ToString() => Name;
    }
}
