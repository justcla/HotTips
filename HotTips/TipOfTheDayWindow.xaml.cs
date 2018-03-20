using System;
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
            string tipId = GetNextTipId();

            string relativeTipPath = $"/Tips/{tipId}.html";
            return GetLocalExtensionDir() + relativeTipPath;
        }

        private static string GetLocalExtensionDir()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private static string GetNextTipId()
        {
            // TODO: Work out the next tip.
            return "Tip001";
        }
    }

}
