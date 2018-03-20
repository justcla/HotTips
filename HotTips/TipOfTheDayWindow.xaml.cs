using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;

namespace HotTips
{
    public class TipOfTheDay
    {
        public static void ShowWindow()
        {
            new TipOfTheDayWindow().Show();
        }
    }

    /// <summary>
    /// Interaction logic for TipOfTheDayControl.xaml
    /// </summary>
    public partial class TipOfTheDayWindow : Window
    {
        public TipOfTheDayWindow()
        {
            InitializeComponent();

            TipContentBrowser.Navigate(new Uri(GetNextTipPath()));
        }

        private static string GetNextTipPath()
        {
            TipInfo tipInfo = GetNextTip();
            ITipGroupProvider tipProvider = tipInfo.Provider;

            string tipId = tipInfo.TipId;
            string tipPath = tipProvider.GetTipPath(tipInfo.Content);

            return tipPath;
        }

        private static TipInfo GetNextTip()
        {
            // TODO: Work out the next tip.
            TipInfo tipInfo = null;

            // First time, set tipInfo to Tip001
            tipInfo = new TipInfo
            {
                Provider = new EmbeddedTipsProvider(),
                TipId = "GN001",
                Content = "Tip002.html"
            };

            // Get all tip group providers
            List<ITipGroupProvider> tipGroupProviders = GetTipGroupProviders();
            foreach (ITipGroupProvider tipGroupProvider in tipGroupProviders)
            {
                List<string> groupFiles = tipGroupProvider.GetGroupDefinitions();
                // Parse each tip group
                foreach (string groupFile in groupFiles)
                {
                    // A groupFile is the file path of a JSON file that defines the tips for a group
                    // Read the file (as stream)
                    System.Diagnostics.Debug.WriteLine($"Reading tip group: {groupFile}");
                    if (File.Exists(groupFile))
                    {
                        System.Diagnostics.Debug.WriteLine($"Found file: {groupFile}");
                    }

                    // TODO: Generate list of groups, with list of tips
                }
            }

            return tipInfo;
        }

        private static List<ITipGroupProvider> GetTipGroupProviders()
        {
            List<ITipGroupProvider> tipGroupProviders = new List<ITipGroupProvider>();
            tipGroupProviders.Add(new EmbeddedTipsProvider());
            return tipGroupProviders;
        }
    }

    internal class TipInfo
    {
        public ITipGroupProvider Provider { get; internal set; }
        public string TipId { get; internal set; }
        public string Content { get; internal set; }
    }
}
