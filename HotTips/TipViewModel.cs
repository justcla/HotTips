using Microsoft.VisualStudio.PlatformUI;

namespace HotTips
{
    internal class TipViewModel : ObservableObject
    {
        public TipViewModel()
        {
            TipContent = "Loading tip content...";
        }

        private string tipContent;
        public string TipContent
        {
            get
            {
                return tipContent;
            }
            set
            {
                SetProperty(ref tipContent, value);
            }
        }
    }
}