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

            string nextTipURI = TipCalculator.GetNextTipPath();

            TipContentBrowser.Navigate(new Uri(nextTipURI));
        }

    }

}
