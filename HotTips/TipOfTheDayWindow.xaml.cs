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
            Owner = Application.Current.MainWindow;

            ITipHistoryManager vsTipHistoryManager = VSTipHistoryManager.Instance();
            
            // Create a new Tip Manager during window creation. It should be disposed when the window is closed.
            _tipManager = new TipManager();

            TipCalculator tipCalculator = new TipCalculator(vsTipHistoryManager, _tipManager);
            TipInfo nextTip = tipCalculator.GetNextTip();

            TipContentBrowser.Navigate(new Uri(nextTip.contentUri));
            
            // Mark tip as shown
            vsTipHistoryManager.MarkTipAsSeen(nextTip.globalTipId);
        }

    }

}
