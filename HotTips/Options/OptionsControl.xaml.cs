using HotTips.Engine;
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
        }

        internal CustomPage OptionsPage { get; set; }
        private DateTime LastDisplayTime { get; set; }

        public void Initialize()
        {
            var groups = GetTipGroups();
            foreach (var g in groups)
            {
                var checkbox = new CheckBox()
                {
                    IsChecked = g.Value,
                    Content = g.Key
                };

                checkbox.Checked += Checkbox_Checked;
                checkbox.Unchecked += Checkbox_Unchecked;

                TipGroupsListBox.Children.Add(checkbox);
            }

            ShowAgainComboBox.ItemsSource = DisplayCadence.KnownDisplayCadences;
            ShowAgainComboBox.SelectedValue = VSTipHistoryManager.Instance().GetCadence();
            LastDisplayTime = VSTipHistoryManager.Instance().GetLastDisplayTime();
            ShowAgainComboBox.SelectionChanged += ShowAgainComboBox_SelectionChanged;
        }

        private void ShowAgainComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newCadence = DisplayCadence.FromName(ShowAgainComboBox.SelectedValue.ToString());
            var nextDisplayTime = LastDisplayTime.Add(newCadence.Delay);
            ShowAgainTextBlock.Text = nextDisplayTime.ToString();
            VSTipHistoryManager.Instance().SetCadenceAsync(ShowAgainComboBox.SelectedValue.ToString());
        }

        private Dictionary<string, bool> GetTipGroups()
        {
            var tipManager = new TipManager();
            var allTipGroups = tipManager.GetPrioritizedTipGroups();
            var tipGroupStatus = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var tipGroup in allTipGroups.Where(t => t != null).SelectMany(t => t))
            {
                tipGroupStatus[tipGroup.groupId] = VSTipHistoryManager.Instance().IsTipGroupExcluded(tipGroup.groupId);
            }

            return tipGroupStatus;
        }

        private void Checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (e.Source is CheckBox checkbox)
            {
                VSTipHistoryManager.Instance().MarkTipGroupAsExcluded(checkbox.Content.ToString().Trim());
            }
        }

        private void Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (e.Source is CheckBox checkbox)
            {
                VSTipHistoryManager.Instance().MarkTipGroupAsIncluded(checkbox.Content.ToString().Trim());
            }
        }
    }
}
