using Hardcodet.Wpf.TaskbarNotification;
using ReactiveUI;
using Seemon.Todo.Utilities;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Seemon.Todo.ViewModels
{
    public class NotificationIconViewModel : ReactiveObject, IEnableLogger
    {
        private UserSettings settings;

        private Visibility showNotification;

        public NotificationIconViewModel()
        {
            this.settings = Locator.Current.GetService<UserSettings>();

            this.ExitCommand = ReactiveCommand.Create();
            this.ExitCommand.Subscribe(x => App.Current.Shutdown());

            this.ShowNotifyCommand = ReactiveCommand.Create();
            this.ShowNotifyCommand.Subscribe(x => this.ShowNotification = (this.settings.MinimizedToSystemTray || this.settings.CloseToSystemTray) ? Visibility.Visible : Visibility.Hidden);

            this.LeftClickCommand = ReactiveCommand.Create(this.settings.WhenAny(x => x.SingleClickToOpen, (o) => o.Value));
            this.LeftClickCommand.Subscribe(x => this.ShowMainWindow());

            this.DoubleClickCommand = ReactiveCommand.Create();
            this.DoubleClickCommand.Subscribe(x => this.ShowMainWindow());

            this.ShowMainWindowCommand = ReactiveCommand.Create();
            this.ShowMainWindowCommand.Subscribe(x => this.ShowMainWindow());

            this.settings.WhenAnyValue(x => x.MinimizedToSystemTray).InvokeCommand(this, x => x.ShowNotifyCommand);
            this.settings.WhenAnyValue(x => x.CloseToSystemTray).InvokeCommand(this, x => x.ShowNotifyCommand);
        }

        public ReactiveCommand<object> ExitCommand { get; private set; }
        public ReactiveCommand<object> ShowNotifyCommand { get; private set; }


        public Visibility ShowNotification
        {
            get { return this.showNotification; }
            set { this.RaiseAndSetIfChanged(ref this.showNotification, value); }
        }

        public ReactiveCommand<object> ShowMainWindowCommand { get; private set; }
        public ReactiveCommand<object> LeftClickCommand { get; private set; }
        public ReactiveCommand<object> DoubleClickCommand { get; private set; }

        private void ShowMainWindow()
        {
            if (App.Current.MainWindow != null)
            { 
                App.Current.MainWindow.Show();
                App.Current.MainWindow.WindowState = WindowState.Normal;
            }
        }
    }
}
