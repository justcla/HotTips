using System;
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

            ITipHistoryManager vsTipHistoryManager = VSTipHistoryManager.Instance();
            
            string nextTipURI = new TipCalculator(vsTipHistoryManager).GetNextTipPath();

            TipContentBrowser.Navigate(new Uri(nextTipURI));
            
            // Mark tip as shown
            vsTipHistoryManager.MarkTipAsSeen("Editor-ED002.html");
        }

    }

}
