using HotTips.Telemetry;
using Justcla;
using Microsoft.VisualStudio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;

namespace HotTips
{
    /// <summary>
    /// Interaction logic for TipOfTheDayControl.xaml
    /// </summary>
    public partial class TipOfTheDayWindow : Window
    {
        private TipCalculator _tipCalculator;
        private VSTipHistoryManager _tipHistoryManager;
        private ITipManager _tipManager;
        private string currentTip;
        private TipViewModel _tipViewModel;

        private bool isLiked = false;

        private bool isUnLiked = false;

        public TipOfTheDayWindow(TipCalculator tipCalculator)
        {
            InitializeComponent();
            Owner = System.Windows.Application.Current.MainWindow;
            _tipViewModel = new TipViewModel();
            this.DataContext = _tipViewModel;

            _tipCalculator = tipCalculator;
            _tipHistoryManager = tipCalculator.TipHistoryManager;
            _tipManager = tipCalculator.TipManager;

            PopulateDefaultImages();
        }

        private void PopulateDefaultImages()
        {

            var brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri("Tips/images/Like.png", UriKind.Relative));
            LikeButton.Background = brush;

            var brush1 = new ImageBrush();
            brush1.ImageSource = new BitmapImage(new Uri("Tips/images/Dislike.png", UriKind.Relative));
            DislikeButton.Background = brush1;

            isLiked = false;
            isUnLiked = false;

            var settingsButtonBrush = new ImageBrush();
            settingsButtonBrush.ImageSource = new BitmapImage(new Uri("Resources/setting-gear.png", UriKind.Relative));
            SettingsButton.Background = settingsButtonBrush;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            LogTelemetryEvent(TelemetryConstants.DialogClosed);
            this.Close();
        }

        private void NextTipButton_Click(object sender, RoutedEventArgs e)
        {
            GoToNextTip();
            PopulateDefaultImages();
            LogTelemetryEvent(TelemetryConstants.NextTipShown);
        }

        private void PrevTipButton_Click(object sender, RoutedEventArgs e)
        {
            GoToPrevTip();
            PopulateDefaultImages();
            LogTelemetryEvent(TelemetryConstants.PrevTipShown);
        }

        private void MoreLikeThisButton_Click(object sender, RoutedEventArgs e)
        {
            // Send telemetry first as the current tip will be lost afterwards.
            LogTelemetryEvent(TelemetryConstants.MoreLikeThisTipClicked);
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

            currentTip = nextTip.globalTipId;

            // Render the new tip content in the tip viewer
            ShowTipContent(nextTip);

            // Display Group settings
            UpdateGroupDisplayElements(nextTip);

            // Mark tip as shown
            if (markAsSeen)
            {
                _tipHistoryManager.MarkTipAsSeen(currentTip);
            }

            return true;
        }

        private void ShowTipContent(TipInfo nextTip)
        {
            // Fetch the path of the file that contains the tip content
            string contentFilePath = nextTip.contentUri;
            // Read the text into a string
            string tipContentString = ReadStringFromFile(contentFilePath);
            // Update the value in the model (which should be reflected in the view through binding)
            _tipViewModel.TipContent = tipContentString;
        }

        private void UpdateGroupDisplayElements(TipInfo nextTip)
        {
            GroupNameLabel.Content = $"{nextTip.groupName}";
            GroupNameCheckBox.IsChecked = !_tipHistoryManager.IsTipGroupExcluded(nextTip.groupId);
        }

        private string ReadStringFromFile(string contentUri)
        {
            return System.IO.File.ReadAllText(contentUri);
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Add telemetry here for keys pressed
        }

        private void GroupNameCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            string tipGroupId = GroupNameLabel.Content.ToString();
            _tipHistoryManager.MarkTipGroupAsExcluded(tipGroupId);
            LogTelemetryEvent(TelemetryConstants.TipGroupDisabled);
        }

        private void GroupNameCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            string tipGroupId = GroupNameLabel.Content.ToString();
            _tipHistoryManager?.MarkTipGroupAsIncluded(tipGroupId);
            LogTelemetryEvent(TelemetryConstants.TipGroupEnabled);
        }

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            string eventName = string.Empty;
            if (isLiked)
            {
                PopulateLikeImage();
                eventName = TelemetryConstants.TipLikeCanceledEvent;
            }
            else
            {
                PopulateLikeFilledImage();
                eventName = TelemetryConstants.TipLikedEvent;
            }
            PopulateDislikeImage();

            LogTelemetryEvent(eventName);
        }

        private void DislikeButton_Click(object sender, RoutedEventArgs e)
        {
            string eventName = string.Empty;
            if (isUnLiked)
            {
                PopulateDislikeImage();
                eventName = TelemetryConstants.TipDisLikeCanceledEvent;
            }
            else
            {
                PopulateDislikeFilledImage();
                eventName = TelemetryConstants.TipDisLikedEvent;
            }
            PopulateLikeImage();

            LogTelemetryEvent(eventName);

        }

        private void LogTelemetryEvent(string eventName)
        {
            string tipGroupId = GroupNameLabel.Content.ToString();

            TelemetryHelper.LogTelemetryEvent(VSTipHistoryManager.GetInstance(), eventName,
                "TipGroupId", tipGroupId,
                "TipId", currentTip ?? string.Empty);
        }

        private void PopulateLikeImage()
        {
            var brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri("Tips/images/Like.png", UriKind.Relative));
            LikeButton.Background = brush;
            isUnLiked = true;
            isLiked = false;
        }

        private void PopulateDislikeFilledImage()
        {
            isLiked = false;
            var brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri("Tips/images/DislikeFilled.png", UriKind.Relative));
            DislikeButton.Background = brush;
            isUnLiked = true;
        }

        private void PopulateDislikeImage()
        {
            var brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri("Tips/images/Dislike.png", UriKind.Relative));
            DislikeButton.Background = brush;
            isLiked = true;
            isUnLiked = false;
        }

        private void PopulateLikeFilledImage()
        {
            var brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri("Tips/images/LikeFilled.png", UriKind.Relative));
            LikeButton.Background = brush;
            isLiked = true;
            isUnLiked = false;
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            TipOfTheDayPackage.Instance.ShowOptionsPage();
            LogTelemetryEvent(TelemetryConstants.OptionsPageShown);
        }
    }

}
