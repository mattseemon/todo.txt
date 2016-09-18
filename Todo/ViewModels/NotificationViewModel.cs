using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Seemon.Todo.ViewModels
{
    public class NotificationViewModel : ReactiveObject
    {
        public static string INFO_COLOR_HEX = "#2d89ef";
        public static string WARN_COLOR_HEX = "#ffc40d";
        public static string ERROR_COLOR_HEX = "#b91d47";
        public enum NotificationTypes
        {
            Information,
            Warning,
            Error
        }

        public NotificationViewModel()
        {
            this.CloseCommand = ReactiveCommand.Create();
            this.CloseCommand.Subscribe(x => this.Hide());
            this.ActionCommand = ReactiveCommand.Create();
            this.ActionCommand.Subscribe(x => this.DoAction());

            this.CloseLabel = "Close";
            this.ActionLabel = "More info";
            this.IsActionVisible = false;
        }

        public ReactiveCommand<object> ActionCommand { get; private set; }
        public ReactiveCommand<object> CloseCommand { get; private set; }

        private NotificationTypes notificationType;
        public NotificationTypes NotificationType
        {
            get { return this.notificationType; }
            set { this.RaiseAndSetIfChanged(ref this.notificationType, value); this.SetStyle(); }
        }

        private void SetStyle()
        {
            switch(this.NotificationType)
            {
                
                case NotificationTypes.Error:
                    this.NotificationColor = GetBrushFromHex(ERROR_COLOR_HEX);
                    this.NotificationLabel = "E";
                    break;
                case NotificationTypes.Warning:
                    this.NotificationColor = GetBrushFromHex(WARN_COLOR_HEX);
                    this.NotificationLabel = "W";
                    break;
                case NotificationTypes.Information:
                default:
                    this.NotificationColor = GetBrushFromHex(INFO_COLOR_HEX);
                    this.NotificationLabel = "I";
                    break;
            }
        }

        private Brush GetBrushFromHex(string colorHex)
        {
            return (Brush)(new BrushConverter().ConvertFrom(colorHex));
        }

        private Brush notificationColorBrush;
        public Brush NotificationColor
        {
            get { return this.notificationColorBrush; }
            set { this.RaiseAndSetIfChanged(ref this.notificationColorBrush, value); }
        }

        private string notificationLabel;
        public string NotificationLabel
        {
            get { return this.notificationLabel; }
            set { this.RaiseAndSetIfChanged(ref this.notificationLabel, value); }
        }

        private string closeLabel;
        public string CloseLabel
        {
            get { return this.closeLabel; }
            set { this.RaiseAndSetIfChanged(ref this.closeLabel, value); }
        }

        private string actionLabel;
        public string ActionLabel
        {
            get { return this.actionLabel; }
            set { this.RaiseAndSetIfChanged(ref this.actionLabel, value); }
        }

        private string message;
        public string Message
        {
            get { return this.message; }
            set { this.RaiseAndSetIfChanged(ref this.message, value); }
        }

        private bool isVisible;
        public bool IsVisible
        {
            get { return this.isVisible; }
            set { this.RaiseAndSetIfChanged(ref this.isVisible, value); }
        }

        private bool isActionVisible;
        public bool IsActionVisible
        {
            get { return this.isActionVisible; }
            set { this.RaiseAndSetIfChanged(ref this.isActionVisible, value); }
        }

        private void Hide()
        {
            this.IsVisible = false;
        }

        private Action action = null;
        private void DoAction()
        {
            if (action != null)
                action();
        }

        public void ShowInformation(string message, string close = "Close", int timeout = 5, Action action = null, string actionLabel = "More Info")
        {
            this.NotificationType = NotificationTypes.Information;
            this.SetStage(message, close, timeout, action, actionLabel);
        }

        public void ShowWarning(string message, string close = "Close", int timeout = 5, Action action = null, string actionLabel = "More Info")
        {
            this.NotificationType = NotificationTypes.Warning;
            this.SetStage(message, close, timeout, action, actionLabel);
        }

        public void ShowError(string message, string close = "Close", int timeout = 5, Action action = null, string actionLabel = "More Info")
        {
            this.NotificationType = NotificationTypes.Error;
            this.SetStage(message, close, timeout, action, actionLabel);
        }

        private void SetStage(string message, string close = "Close", int timeout = 5, Action action = null, string actionLabel = "More Info")
        {
            this.Message = message;
            this.CloseLabel = close;
            
            if (action != null)
            {
                this.action = action;
                this.IsActionVisible = true;
                this.ActionLabel = actionLabel;
            }

            this.IsVisible = true;

            if (timeout == 0)
                return;

            Observable.Interval(TimeSpan.FromSeconds(1))
                .Delay(TimeSpan.FromSeconds(timeout))
                .InvokeCommand(CloseCommand);
        }
    }
}
