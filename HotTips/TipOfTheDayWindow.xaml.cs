using Justcla;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace HotTips
{
    public class TipOfTheDay
    {
        private static readonly string TIP_OF_THE_DAY_TITLE = "Tip of the Day";

        private static ITipManager _tipManager;
        private static ITipHistoryManager _tipHistoryManager;
        private static TipCalculator _tipCalculator;

        public static void ShowWindow()
        {
            try
            {
                // TODO: Perf optimisation: First time, use hard-coded tip.
                if (IsFirstTime())
                {
                    // TODO: Set nextTip to hard-coded First-Tip.
                    // Avoid loading tip parser if user is going to close and never run it again.
                }

                _tipManager = new TipManager();
                _tipHistoryManager = VSTipHistoryManager.Instance();
                _tipCalculator = new TipCalculator(_tipHistoryManager, _tipManager);

                TipInfo nextTip = GetNewTip();

                if (nextTip == null)
                {
                    // There's no tip to show. Don't show the Tip of the Day window.
                    Debug.WriteLine("Tip of the Day: There's no tip to show. Will not launch TotD dialog.");
                    return;
                }

                // We have a tip! Let's create the Tip of the Day UI.
                TipOfTheDayWindow tipOfTheDayWindow = new TipOfTheDayWindow(_tipCalculator);

                // Attempt to navigate to the chose tip
                var success = tipOfTheDayWindow.NavigateToTip(nextTip);
                if (!success)
                {
                    // Failed to navigate to tip URI
                    Debug.WriteLine("Tip of the Day: Failed to navigate to tip URI. Will not launch TotD dialog.");
                    return;
                }

                // Now show the dialog
                tipOfTheDayWindow.Show();

                // Mark tip as seen
                _tipHistoryManager.MarkTipAsSeen(nextTip.globalTipId);
            }
            catch (Exception e)
            {
                // Fail gracefully when window will now show
                Debug.WriteLine("Unable to open Tip of the Day: " + e.Message);
                return;
            }
        }

        public static TipInfo GetNewTip()
        {
            TipInfo nextTip = _tipCalculator.GetNextTip();

            if (nextTip == null)
            {
                // No new tips to show. Attempt to reset history and start again.
                nextTip = GetTipAfterResetTipHistory(ref nextTip);
            }

            return nextTip;
        }

        private static TipInfo GetTipAfterResetTipHistory(ref TipInfo nextTip)
        {
            // Check strange case - No new tips and no tips shown.
            if (_tipHistoryManager.GetTipHistory() == null)
            {
                // No tip found and there are none in the history, so we must have no tips at all. (Strange case)
                Debug.WriteLine("No tips seen and no tips yet to see. (Check if tip groups loaded correctly.)");
                return null;
            }

            // Ask user if they want to clear the history and start again from the beginning.
            if (!ShouldResetHistory())
            {
                // User does not want to reset history. Exit nicely.
                return null;
            }

            // Reset Tip History
            _tipHistoryManager.ClearTipHistory();

            // Fetch next tip
            return _tipCalculator.GetNextTip();
        }

        private static bool ShouldResetHistory()
        {
            const string Text = "No new tips to show.\n\nClear tip history and start showing tips from the beginning?";
            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show(Text, TIP_OF_THE_DAY_TITLE, MessageBoxButtons.OKCancel);
            return dialogResult == DialogResult.OK;
        }

        private static bool IsFirstTime()
        {
            // TODO: Determine if is first time
            return false;
        }

    }

    /// <summary>
    /// Interaction logic for TipOfTheDayControl.xaml
    /// </summary>
    public partial class TipOfTheDayWindow : Window
    {
        private TipCalculator _tipCalculator;
        private ITipHistoryManager _tipHistoryManager;
        private ITipManager _tipManager;
        private string currentTip;

        public TipOfTheDayWindow(TipCalculator tipCalculator)
        {
            InitializeComponent();
            Owner = System.Windows.Application.Current.MainWindow;

            _tipCalculator = tipCalculator;
            _tipHistoryManager = tipCalculator.TipHistoryManager;
            _tipManager = tipCalculator.TipManager;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void NextTipButton_Click(object sender, RoutedEventArgs e)
        {
            GoToNextTip();
        }

        private void PrevTipButton_Click(object sender, RoutedEventArgs e)
        {
            GoToPrevTip();
        }

        private void MoreLikeThisButton_Click(object sender, RoutedEventArgs e)
        {
            GoToMoreLikeThis();
        }

        private void GoToNextTip()
        {
            // If the current tip is not the last tip in the tip history, then go to the next tip in the tip history that exists.
            List<string> tipHistory = _tipHistoryManager.GetTipHistory();

            // Is there a tip later in the history than the current tip?
            var currentTipIndex = tipHistory.LastIndexOf(currentTip);   // Use LastIndexOf for performance as it will normally be towards the end of the list.
            TipInfo nextTipInHistory = GetNextTipInHistory(tipHistory, currentTipHistoryIndex: currentTipIndex);
            if (nextTipInHistory != null)
            {
                // Navigate to the next tip in history.
                NavigateToTip(nextTipInHistory, markAsSeen: false);
                return;
            }

            // There are no more tips to show from the history. Find a new tip.
            TipInfo nextTip = TipOfTheDay.GetNewTip();
            var success = NavigateToTip(nextTip, markAsSeen: true);
            if (!success)
            {
                // Failed to show next tip. Close window.
                Close();
            }
        }

        private TipInfo GetNextTipInHistory(List<string> tipHistory, int currentTipHistoryIndex)
        {
            int nextTipIndex = currentTipHistoryIndex + 1;
            if (nextTipIndex >= tipHistory.Count)
            {
                // No additional items in the tip history to check.
                return null;
            }

            var nextTipId = tipHistory[nextTipIndex];

            TipInfo nextTip = _tipManager.GetTipInfo(nextTipId);

            if (nextTip != null)
            {
                // Found a tip. Return it.
                return nextTip;
            }

            // Recursively search forward for a valid next tip.
            return GetNextTipInHistory(tipHistory, nextTipIndex);
        }

        private void GoToPrevTip()
        {
            // Get the index of the current tip in the tip history. (Should always resolve.)
            List<string> tipHistory = _tipHistoryManager.GetTipHistory();
            int currentTipHistoryIndex = tipHistory.LastIndexOf(currentTip);

            // Get the previous tip (if there is one)
            TipInfo previousTip = GetPreviousTip(tipHistory, currentTipHistoryIndex);

            // Back out if there is no previous tip.
            if (previousTip == null)
            {
                Debug.WriteLine("Tip of the Day: There is no previous tip to navigte to.");
                return;
            }

            // Navigate to the previous tip.
            bool success = NavigateToTip(previousTip, markAsSeen: false);
        }

        private TipInfo GetPreviousTip(List<string> tipHistory, int currentTipHistoryIndex)
        {
            int prevTipHistoryIndex = currentTipHistoryIndex - 1;
            if (prevTipHistoryIndex < 0)
            {
                // We've reached the beginning of history. There is no previous.
                return null;
            }

            // Previous tip is the one before the current tip in the tip history.
            string previousTipId = tipHistory[prevTipHistoryIndex];

            // Get the full TipInfo (by the given TipId) from the Tip Manager.
            TipInfo previousTip = _tipManager.GetTipInfo(previousTipId);

            if (previousTip != null)
            {
                // Found a tip! Return it.
                return previousTip;
            }

            // No Previous tip to show. It's possible the tip previously shown no longer exists.
            // Look for next previous tip. (drop the index back one position and try again)
            // repeat - until currentTipHistoryIndex < 1 or previousTip != null
            return GetPreviousTip(tipHistory, prevTipHistoryIndex);
        }

        private void GoToMoreLikeThis()
        {
            // Ask the TipManager for the next tip in the current group
            TipInfo nextTipInGroup = _tipManager.GetNextTipInGroup(currentTip);

            if (nextTipInGroup == null)
            {
                MessageBox.Show("There are no more tips in this group.");
                return;
            }
            
            // Navigate to the next tip
            NavigateToTip(nextTipInGroup, markAsSeen: true);
        }

        //-------- Helper functions --------

        internal bool NavigateToTip(TipInfo nextTip, bool markAsSeen = true)
        {
            if (nextTip == null || string.IsNullOrEmpty(nextTip.contentUri))
            {
                // Unable to navigate. No tip content URI.
                return false;
            }

            // Navigate to the Tip URI
            currentTip = nextTip.globalTipId;
            TipContentBrowser.Navigate(new Uri(nextTip.contentUri));

            // Mark tip as shown
            if (markAsSeen)
            {
                _tipHistoryManager.MarkTipAsSeen(nextTip.globalTipId);
            }

            // Output telemetry: Tip Shown (Consider making this conditional on "markAsSeen")
            VSTelemetryHelper.PostEvent("Justcla/HotTips/TipShown", "TipId", currentTip);

            return true;
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Add telemetry here for keys pressed
        }
    }

}
