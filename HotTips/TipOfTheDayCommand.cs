using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace HotTips
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class TipOfTheDayCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int TipOfTheDayCmdId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid TipOfTheDayCmdSetGuid = new Guid("3fc91750-97a6-4544-be7b-572f72433e9b");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="TipOfTheDayCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private TipOfTheDayCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));

            // Register command handler for Tip of the Day command
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            commandService.AddCommand(CreateMenuItem(TipOfTheDayCmdSetGuid, TipOfTheDayCmdId));
        }

        private MenuCommand CreateMenuItem(Guid tipOfTheDayCmdSetGuid, int tipOfTheDayCmdId)
        {
            var menuCommandID = new CommandID(TipOfTheDayCmdSetGuid, TipOfTheDayCmdId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            return menuItem;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static TipOfTheDayCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Verify the current thread is the UI thread - the call to AddCommand in TipOfTheDay's constructor requires
            // the UI thread.
            ThreadHelper.ThrowIfNotOnUIThread();

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new TipOfTheDayCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ShowTipOfTheDay();
        }

        public void ShowTipOfTheDay()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = "Use Tip of the Day to learn great features available to you in Visual Studio.";
            string title = "Tip Of The Day";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
