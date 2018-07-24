using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HotTips.Options
{
    [Guid("BF41E5A7-EF14-4AF4-904C-6CDDA6D56F56")]
    public class GeneralPage : UIElementDialogPage
    {
        private string optionValue = "alpha";

        public string OptionString
        {
            get { return optionValue; }
            set { optionValue = value; }
        }

        protected override UIElement Child
        {
            get
            {
                var control = new OptionsControl();
                control.OptionsPage = this;
                control.Initialize();
                return control;
            }
        }

        //protected override IWin32Window Window
        //{
        //    get
        //    {
        //        MyUserControl page = new MyUserControl();
        //        page.optionsPage = this;
        //        page.Initialize();
        //        return this;
        //    }
        //}
    }
}
