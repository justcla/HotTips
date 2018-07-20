using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;

namespace HotTips.Engine
{
    [Export(typeof(ICommandHandler))]
    [ContentType("text")]
    [Name("HotTips command listener")]
    class CommandHandler :
        ICommandHandler<CommentSelectionCommandArgs>,
        ICommandHandler<ToggleCompletionModeCommandArgs>,
        ICommandHandler<TypeCharCommandArgs>
    {
        public string DisplayName => "HotTips command listener";

        // Use CommandState.Unspecified to not interfere with execution, 
        // but if there are not command handlers that mark this command as Available,
        // then we won't run
        public CommandState GetCommandState(ToggleCompletionModeCommandArgs args) => CommandState.Unspecified;

        public bool ExecuteCommand(CommentSelectionCommandArgs args, CommandExecutionContext executionContext)
        {
            // Record that user used the Comment Selection command
            return false; // If we return true, the command is marked as "handled" and not actually executed
        }

        public CommandState GetCommandState(CommentSelectionCommandArgs args) => CommandState.Unspecified;

        public bool ExecuteCommand(ToggleCompletionModeCommandArgs args, CommandExecutionContext executionContext)
        {
            // Record that user used the Ctrl+Alt+Space command to use Suggestion Mode
            return false; // If we return true, the command is marked as "handled" and not actually executed
        }

        public CommandState GetCommandState(TypeCharCommandArgs args) => CommandState.Unspecified;

        public bool ExecuteCommand(TypeCharCommandArgs args, CommandExecutionContext executionContext)
        {
            return false;
        }
    }
}
