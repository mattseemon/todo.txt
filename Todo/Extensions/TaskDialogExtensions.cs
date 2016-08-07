using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System.Windows.Interop;

namespace Seemon.Todo.Extensions
{
    public static class TaskDialogExtensions
    {
        public static TaskDialogResult ShowTaskDialog(this Window source, string text, string instructions, string caption, TaskDialogStandardIcon icon = TaskDialogStandardIcon.None, TaskDialogStandardButtons buttons = TaskDialogStandardButtons.Ok)
        {
            TaskDialog td = new TaskDialog();
            td.InstructionText = instructions;
            td.Caption = caption;
            td.Text = text;
            td.Icon = icon;
            td.OwnerWindowHandle = new WindowInteropHelper(source).Handle;
            td.StartupLocation = TaskDialogStartupLocation.CenterOwner;
            td.StandardButtons = buttons;

            return td.Show();
        }
    }
}
