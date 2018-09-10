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
        private static string installDir = Path.GetDirectoryName(typeof(TipOfTheDayWindow).Assembly.CodeBase);
        private static string resourcesDir = Path.Combine(installDir, "Resources");
        private static string uiImagesDir = Path.Combine(resourcesDir, "UI-images");
        private static string likeEmptyImagePath = Path.Combine(uiImagesDir, "LikeEmpty.png");
        private static string likeFilledImagePath = Path.Combine(uiImagesDir, "LikeFilled.png");
        private static string dislikeEmptyImagePath = Path.Combine(uiImagesDir, "DislikeEmpty.png");
        private static string dislikeFilledImagePath = Path.Combine(uiImagesDir, "DislikeFilled.png");
        private static string settingGearIconPath = Path.Combine(uiImagesDir, "setting-gear.png");

        private TipCalculator _tipCalculator;
        private VSTipHistoryManager _tipHistoryManager;
        private ITipManager _tipManager;
        private TipViewModel _tipViewModel;

        private string currentTip;
        private TipLikeEnum currentTipVoteStatus;

        public TipOfTheDayWindow(TipCalculator tipCalculator)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            _tipViewModel = new TipViewModel();
            this.DataContext = _tipViewModel;

            _tipCalculator = tipCalculator;
            _tipHistoryManager = tipCalculator.TipHistoryManager;
            _tipManager = tipCalculator.TipManager;

            // Initialize UI
            InitializeUIComponents();
        }

        private void InitializeUIComponents()
        {
            SettingsButton.Background = GetImageBrush(settingGearIconPath);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            LogTelemetryEvent(TelemetryConstants.DialogClosed);
            this.Close();
        }

        private void NextTipButton_Click(object sender, RoutedEventArgs e)
        {
            GoToNextTip();
            LogTelemetryEvent(TelemetryConstants.NextTipShown);
        }

        private void PrevTipButton_Click(object sender, RoutedEventArgs e)
        {
            GoToPrevTip();
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
            // If there's a next tip in the Tip History, show that. Otherwise, grab a new tip.
            TipInfo nextTip;
            TipHistoryInfo nextTipInHistory = FindNextTipInHistory();
            if (nextTipInHistory != null)
            {
                // Build a TipInfo object to show in the UI
                nextTip = BuildTipInfoFromHistory(nextTipInHistory);
            }
            else
            {
                // Generate a new Tip to show. Set the LikeStatus to NORMAL.
                nextTip = TipOfTheDay.GetNewTip();
                nextTip.TipLikeStatus = TipLikeEnum.NORMAL;
            }

            // Show the tip
            NavigateToTip(nextTip);
        }

        private void GoToPrevTip()
        {
            TipHistoryInfo previousTipHistory = FindPreviousTipInHistory();

            // Back out if there is no previous tip.
            if (previousTipHistory == null)
            {
                Debug.WriteLine("Tip of the Day: There is no previous tip to navigate to.");
                return;
            }

            // Prepare the TipInfo to show in the UI and navigate to the previous tip.
            TipInfo previousTip = BuildTipInfoFromHistory(previousTipHistory);
            // Show the tip on the UI
            NavigateToTip(previousTip);
        }

        private TipInfo BuildTipInfoFromHistory(TipHistoryInfo tipHistoryInfo)
        {
            TipInfo tipInfo = _tipManager.GetTipInfo(tipHistoryInfo.GlobalTipId);
            tipInfo.TipLikeStatus = tipHistoryInfo.TipLikeStatus;
            return tipInfo;
        }

        private TipHistoryInfo FindNextTipInHistory()
        {
            List<TipHistoryInfo> tipHistoryList = _tipHistoryManager.GetTipHistory();

            // Get the index of the current tip in the TipHistory
            // Perf Note: Use LastIndexOf for performance as the current tip will normally be towards the end of the list.
            int currentTipIndex = tipHistoryList.FindLastIndex(a => a.GlobalTipId.Equals(currentTip));

            // Get the next tip from the TipHistory - if there is one.
            return GetNextTipFromHistory(tipHistoryList, currentTipHistoryIndex: currentTipIndex);
        }

        /// <summary>
        /// Recursive method.
        /// Logic: From the currentTipHistoryIndex, look to see if there's a tip at a later index.
        /// </summary>
        private TipHistoryInfo GetNextTipFromHistory(List<TipHistoryInfo> tipHistory, int currentTipHistoryIndex)
        {
            int nextTipIndex = currentTipHistoryIndex + 1;
            if (nextTipIndex >= tipHistory.Count)
            {
                // No additional items in the tip history to check.
                return null;
            }

            // Get the next tip from history
            TipHistoryInfo nextTipHistoryInfo = tipHistory[nextTipIndex];

            // Check if the tip from history can be found in the current list of known tips.
            var tipExists = _tipManager.TipExists(nextTipHistoryInfo.GlobalTipId);
            if (tipExists)
            {
                // Found a tip. Return the TipHistory.
                return nextTipHistoryInfo;
            }

            // Recursively search forward for a valid next tip.
            return GetNextTipFromHistory(tipHistory, nextTipIndex);
        }

        private TipHistoryInfo FindPreviousTipInHistory()
        {
            // Get the index of the current tip in the tip history. (Should always resolve.)
            List<TipHistoryInfo> tipHistoryList = _tipHistoryManager.GetTipHistory();
            int currentTipHistoryIndex = tipHistoryList.FindLastIndex(a => a.GlobalTipId.Equals(currentTip));

            // Get the previous tip (if there is one)
            return GetPreviousTipFromHistory(tipHistoryList, currentTipHistoryIndex);
        }

        private TipHistoryInfo GetPreviousTipFromHistory(List<TipHistoryInfo> tipHistory, int currentTipHistoryIndex)
        {
            // Recursive method: Stop when we find a tip, or when the tip history index is < 0
            int prevTipHistoryIndex = currentTipHistoryIndex - 1;
            if (prevTipHistoryIndex < 0)
            {
                // We've reached the beginning of history. There is no previous.
                return null;
            }

            // Get the previous tip from history. Tip history is ordered with the most recent last.
            // Previous tip is the one before the current tip in the tip history.
            TipHistoryInfo previousTipHistoryInfo = tipHistory[prevTipHistoryIndex];

            // Get the full TipInfo (by the given TipId) from the Tip Manager.
            var tipExists = _tipManager.TipExists(previousTipHistoryInfo.GlobalTipId);
            if (tipExists)
            {
                // Found a tip! Can return the TipHistory.
                return previousTipHistoryInfo;
            }

            // No Previous tip to show. (It's possible the tip previously shown no longer exists.)
            // Look for next previous tip. (Drop the index back one position and try again)
            // Recursively search backward until currentTipHistoryIndex < 1 or previousTip != null
            return GetPreviousTipFromHistory(tipHistory, prevTipHistoryIndex);
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
            NavigateToTip(nextTipInGroup);
        }

        //-------- Helper functions --------

        internal bool NavigateToTip(TipInfo nextTip)
        {
            if (nextTip == null || string.IsNullOrEmpty(nextTip.contentUri))
            {
                // Unable to navigate. No tip content URI.
                return false;
            }

            // Update currentTip. Used later when looking up tip index in tip history.
            currentTip = nextTip.globalTipId;

            // Perform all UI rendering
            RenderAllTipUIElements(nextTip);

            // Mark tip as seen in Tip History
            _tipHistoryManager.MarkTipAsSeen(currentTip);

            return true;
        }

        private void RenderAllTipUIElements(TipInfo nextTip)
        {
            // Render the new tip content in the tip viewer
            ShowTipContent(nextTip);
            // Display Group settings
            UpdateGroupDisplayElements(nextTip);
            // Update the vote icons
            UpdateTipVoteIcons(nextTip.TipLikeStatus);
        }

        private void ShowTipContent(TipInfo nextTip)
        {
            // Fetch the path of the file that contains the tip content
            string contentFilePath = nextTip.contentUri;
            // Read the text into a string
            string tipContentString = File.ReadAllText(contentFilePath);
            // Update the value in the model (which should be reflected in the view through binding)
            _tipViewModel.TipContent = tipContentString;
        }

        private void UpdateGroupDisplayElements(TipInfo nextTip)
        {
            GroupNameLabel.Content = $"{nextTip.groupName}";
            GroupNameCheckBox.IsChecked = !_tipHistoryManager.IsTipGroupExcluded(nextTip.groupId);
        }

        private void UpdateTipVoteIcons(TipLikeEnum tipLikeEnum)
        {
            switch (tipLikeEnum)
            {
                case TipLikeEnum.LIKE:
                    ShowLikeVote();
                    break;
                case TipLikeEnum.DISLIKE:
                    ShowDislikeVote();
                    break;
                case TipLikeEnum.NORMAL:
                    ShowNoVote();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            RenderLikeToggle();
            _tipHistoryManager.UpdateTipVoteStatus(currentTip, currentTipVoteStatus);
        }

        private void DislikeButton_Click(object sender, RoutedEventArgs e)
        {
            RenderDislikeToggle();
            _tipHistoryManager.UpdateTipVoteStatus(currentTip, currentTipVoteStatus);
        }

        private void RenderLikeToggle()
        {
            if (currentTipVoteStatus == TipLikeEnum.LIKE)
            {
                // Button was already liked. Toggle if off. (No vote)
                ShowNoVote();
            }
            else
            {
                ShowLikeVote();
            }
        }

        private void RenderDislikeToggle()
        {
            if (currentTipVoteStatus == TipLikeEnum.DISLIKE)
            {
                // Button was already disliked. Toggle if off. (No vote)
                ShowNoVote();
            }
            else
            {
                ShowDislikeVote();
            }
        }

        private void LogTelemetryEvent(string eventName)
        {
            string tipGroupId = GroupNameLabel.Content.ToString();

            TelemetryHelper.LogTelemetryEvent(VSTipHistoryManager.GetInstance(), eventName,
                "TipGroupId", tipGroupId,
                "TipId", currentTip ?? string.Empty);
        }

        private void ShowNoVote()
        {
            currentTipVoteStatus = TipLikeEnum.NORMAL;

            DislikeButton.Background = GetImageBrush(dislikeEmptyImagePath);
            LikeButton.Background = GetImageBrush(likeEmptyImagePath);
        }

        private void ShowLikeVote()
        {
            currentTipVoteStatus = TipLikeEnum.LIKE;

            DislikeButton.Background = GetImageBrush(dislikeEmptyImagePath);
            LikeButton.Background = GetImageBrush(likeFilledImagePath);
        }

        private void ShowDislikeVote()
        {
            currentTipVoteStatus = TipLikeEnum.DISLIKE;

            DislikeButton.Background = GetImageBrush(dislikeFilledImagePath);
            LikeButton.Background = GetImageBrush(likeEmptyImagePath);
        }

        private static ImageBrush GetImageBrush(string imagePath)
        {
            return new ImageBrush { ImageSource = new BitmapImage(new Uri(imagePath, UriKind.Absolute)) };
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
