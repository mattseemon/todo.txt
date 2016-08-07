using Microsoft.WindowsAPICodePack.Dialogs;
using Splat;
using System;
using System.Windows;
using System.Windows.Interop;

namespace Seemon.Todo.Extensions
{
    public static class ExceptionExtensions
    {
        public static void Handle(this Exception ex, string errorMessage, IEnableLogger view, Window window)
        {
            view.Log().ErrorException(errorMessage, ex);

            TaskDialog td = new TaskDialog();
            td.Caption = "TODO.TXT - ERROR";
            td.InstructionText = errorMessage;
            td.Text = "Please see Help \u2794 View Error Log for more details.";
            td.StandardButtons = TaskDialogStandardButtons.Ok;
            td.Icon = TaskDialogStandardIcon.Error;
            td.OwnerWindowHandle = new WindowInteropHelper(window).Handle;
            td.StartupLocation = TaskDialogStartupLocation.CenterOwner;
            td.Show();
        }
    }
}
