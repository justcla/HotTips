using HotTips.Engine;
using HotTips.Telemetry;
using Justcla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HotTips.Options
{
    /// <summary>
    /// Interaction logic for MyUserControl.xaml
    /// </summary>
    public partial class OptionsControl : UserControl
    {
        public OptionsControl()
        {
            InitializeComponent();

            this.IsVisibleChanged += OptionsControl_IsVisibleChanged;
        }

        private void OptionsControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool visibility && visibility == true)
            {
                InitializeTipGroups();
                InitializeTipLevels();
                LogTelemetryEvent(TelemetryConstants.OptionsPageShown);
            }
            else
            {
                LogTelemetryEvent(TelemetryConstants.OptionsPageDismissed);
            }
        }

        private void LogTelemetryEvent(string eventName, params object[] namesAndProperties)
        {
            TelemetryHelper.LogTelemetryEvent(VSTipHistoryManager.GetInstance() ,eventName,
                namesAndProperties);
        }

        internal OptionsPage OptionsPage { get; set; }
        private DateTime LastDisplayTime { get; set; }

        public void Initialize()
        {
        }

        private void InitializeTipGroups()
        {
            var groups = GetTipGroups();
            TipGroupsListBox.Children.Clear();
            foreach (var g in groups)
            {
                var checkbox = new CheckBox()
                {
                    IsChecked = g.Value,
                    Content = g.Key
                };

                checkbox.Checked += Checkbox_Tip_Group_Checked;
                checkbox.Unchecked += Checkbox_Tip_Group_Unchecked;

                TipGroupsListBox.Children.Add(checkbox);
            }

            ShowAgainComboBox.ItemsSource = DisplayCadence.KnownDisplayCadences;
            ShowAgainComboBox.SelectedValue = VSTipHistoryManager.GetInstance().GetCadence();
            LastDisplayTime = VSTipHistoryManager.GetInstance().GetLastDisplayTime();
            ShowAgainComboBox.SelectionChanged += ShowAgainComboBox_SelectionChanged;
            UpdateShowAgainUI();
        }

        private async void ShowAgainComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                UpdateShowAgainUI();
                string newCadence = ShowAgainComboBox.SelectedValue.ToString();
                await VSTipHistoryManager.GetInstance().SetCadenceAsync(DisplayCadence.FromName(newCadence));

                var oldCadence = e.RemovedItems?[0]?.ToString();
                LogTelemetryEvent(TelemetryConstants.NextShowChanged, "NewCadence", newCadence, "OldCadence", oldCadence);
            }
            catch
            { }
        }
        private void InitializeTipLevels()
        {
            var tipLevels = Enum.GetValues(typeof(TipLevel));
            TipLevelsListBox.Children.Clear();

            foreach (var l in tipLevels)
            {
                var checkbox = new CheckBox()
                {
                    IsChecked = !VSTipHistoryManager.GetInstance().IsTipLevelExcluded((TipLevel)l),
                    Content = (TipLevel)l
                };

                checkbox.Checked += CheckBox_Tip_Level_Checked;
                checkbox.Unchecked += Checkbox_Tip_Level_Unchecked;
                TipLevelsListBox.Children.Add(checkbox);
            }
        }

        private void Checkbox_Tip_Level_Unchecked(object sender, RoutedEventArgs e)
        {
            if (e.Source is CheckBox checkbox)
            {
                if (Enum.TryParse(checkbox.Content.ToString(), out TipLevel level))
                {
                    VSTipHistoryManager.GetInstance().MarkTipLevelAsExcluded(level);
                    LogTelemetryEvent(TelemetryConstants.TipLevelDisabled, "TipLevel", level);
                }
            }
        }

        private void CheckBox_Tip_Level_Checked(object sender, RoutedEventArgs e)
        {
            if (e.Source is CheckBox checkbox)
            {
                if (Enum.TryParse(checkbox.Content.ToString(), out TipLevel level))
                {
                    VSTipHistoryManager.GetInstance().MarkTipLevelAsIncluded(level);
                    LogTelemetryEvent(TelemetryConstants.TipLevelEnabled, "TipLevel", level);
                }
            }
        }

        private void UpdateShowAgainUI()
        {
            var newCadence = DisplayCadence.FromName(ShowAgainComboBox.SelectedValue.ToString());
            if (newCadence.ShowEstimate)
            {
                ShowAgainTextBlock.Text = LastDisplayTime.Add(newCadence.Delay).ToLocalTime().ToString("d");
                ShowAgainPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ShowAgainPanel.Visibility = Visibility.Collapsed;
            }
        }

        private Dictionary<string, bool> GetTipGroups()
        {
            var tipManager = new TipManager();
            var allTipGroups = tipManager.GetPrioritizedTipGroups();
            var tipGroupStatus = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var tipGroup in allTipGroups.Where(t => t != null).SelectMany(t => t))
            {
                tipGroupStatus[tipGroup.groupId] = !VSTipHistoryManager.GetInstance().IsTipGroupExcluded(tipGroup.groupId);
            }

            return tipGroupStatus;
        }

        private void Checkbox_Tip_Group_Unchecked(object sender, RoutedEventArgs e)
        {
            if (e.Source is CheckBox checkbox)
            {
                string tipGroupId = checkbox.Content.ToString().Trim();
                VSTipHistoryManager.GetInstance().MarkTipGroupAsExcluded(tipGroupId);
                LogTelemetryEvent(TelemetryConstants.TipGroupDisabled, "TipGroupId", tipGroupId);
            }
        }

        private void Checkbox_Tip_Group_Checked(object sender, RoutedEventArgs e)
        {
            if (e.Source is CheckBox checkbox)
            {
                string tipGroupId = checkbox.Content.ToString().Trim();
                VSTipHistoryManager.GetInstance().MarkTipGroupAsIncluded(tipGroupId);
                LogTelemetryEvent(TelemetryConstants.TipGroupEnabled, "TipGroupId", tipGroupId);
            }
        }
    }
}
