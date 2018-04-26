using System;
using System.Windows;

namespace HotTips
{
    public class TipOfTheDay
    {
        public static void ShowWindow()
        {
            // Expect window might close during startup if no tips found.
            try
            {
                new TipOfTheDayWindow().Show();
            }
            catch (Exception e)
            {
                // Fail gracefully when window will now show
                System.Diagnostics.Debug.WriteLine("Unable to open Tip of the Day: " + e.Message);
                return;
            }
        }
    }

    /// <summary>
    /// Interaction logic for TipOfTheDayControl.xaml
    /// </summary>
    public partial class TipOfTheDayWindow : Window
    {
        private ITipManager _tipManager;
        private ITipHistoryManager _tipHistoryManager;
        private TipCalculator _tipCalculator;

        public TipOfTheDayWindow()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;

            // Create new objects during window creation. They should be disposed when the window is closed.
            _tipManager = new TipManager();
            _tipHistoryManager = VSTipHistoryManager.Instance();
            _tipCalculator = new TipCalculator(_tipHistoryManager, _tipManager);

            NavigateToNextTip();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void NextTipButton_Click(object sender, RoutedEventArgs e)
        {
            var success = NavigateToNextTip();
            //if (!success)
            //{
                //Close();
            //}
        }

        private bool NavigateToNextTip()
        {
            TipInfo nextTip = _tipCalculator.GetNextTip();

            if (nextTip == null)
            {
                // No tip to show.
                // Close window.
                Close();
                return false;
            }

            TipContentBrowser.Navigate(new Uri(nextTip.contentUri));

            // Mark tip as shown
            _tipHistoryManager.MarkTipAsSeen(nextTip.globalTipId);

            return true;
        }
    }

}
