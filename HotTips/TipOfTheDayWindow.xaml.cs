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
        private ITipManager _tipManager;

        public TipOfTheDayWindow()
        {
            InitializeComponent();

            ITipHistoryManager vsTipHistoryManager = VSTipHistoryManager.Instance();
            
            // Create a new Tip Manager during window creation. It should be disposed when the window is closed.
            _tipManager = new TipManager();

            TipCalculator tipCalculator = new TipCalculator(vsTipHistoryManager, _tipManager);
            
            string nextTipURI = tipCalculator.GetNextTipPath();

            TipContentBrowser.Navigate(new Uri(nextTipURI));
            
            // Mark tip as shown
            vsTipHistoryManager.MarkTipAsSeen("Editor-ED002.html");
        }

    }

}
