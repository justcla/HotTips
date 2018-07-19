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

        public void Initialize()
        {
            textBox1.Text = OptionsPage.OptionString;
        }

        //private void MyUserControl1_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    optionsPage.OptionString = textBox1.Text;
        //}

        //private void textBox1_Leave(object sender, EventArgs e)
        //{
        //    optionsPage.OptionString = textBox1.Text;
        //}

        private void textBox1_LostFocus(object sender, RoutedEventArgs e)
        {
            OptionsPage.OptionString = textBox1.Text;
        }
    }
}
