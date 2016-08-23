using Caliburn.Micro;
using ReactiveUI;
using Seemon.Todo.Utilities;
using Seemon.Todo.ViewModels;
using Splat;
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
using System.Windows.Shapes;
using Seemon.Todo.Extensions;

namespace Seemon.Todo.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window, IEnableLogger
    {
        UserSettings settings = null;
        HotKey hotKey = null;
        
        public ShellView()
        {
            InitializeComponent();

            settings = Locator.Current.GetService<UserSettings>();

            this.CloseToTrayCommand = ReactiveCommand.Create();
            this.CloseToTrayCommand.Subscribe(x => this.ToggleCloseToTray());

            this.AutoHideMainMenuCommand = ReactiveCommand.Create();
            this.AutoHideMainMenuCommand.Subscribe(x => this.ToogleAutoHideMainMenu());

            this.SetFontCommand = ReactiveCommand.Create();
            this.SetFontCommand.Subscribe(x => this.SetUIFont());

            this.settings.WhenAnyValue(x => x.AutoHideMainMenu).InvokeCommand(this, x => x.AutoHideMainMenuCommand);
            this.settings.WhenAnyValue(x => x.CloseToSystemTray).InvokeCommand(this, x => x.CloseToTrayCommand);
            this.settings.WhenAnyValue(x => x.FontFamily).InvokeCommand(this, x => x.SetFontCommand);
            this.settings.WhenAnyValue(x => x.FontStyle).InvokeCommand(this, x => x.SetFontCommand);
            this.settings.WhenAnyValue(x => x.FontSize).InvokeCommand(this, x => x.SetFontCommand);

            try
            {
                hotKey = new HotKey(ModifierKeys.Windows | ModifierKeys.Alt, HotKeyWinApi.Keys.O, this);
                hotKey.HotKeyPressed += OnHotKeyPressed;
            }
            catch(Exception) { }

            this.SetWindowPosition();

            lbTasks.Focus();
        }

        private void SetUIFont()
        {
            lbTasks.FontFamily = settings.FontFamily;
            lbTasks.FontSize = settings.FontSize;
            lbTasks.FontStyle = settings.FontStyle.Style;
            lbTasks.FontStretch = settings.FontStyle.Stretch;
            lbTasks.FontWeight = settings.FontStyle.Weight;
        }

        private void SetSelected(MenuItem item)
        {
            var parent = (MenuItem)item.Parent;
            foreach (MenuItem i in parent.Items)
                i.IsChecked = false;

            item.IsChecked = true;
        }

        private void OnHotKeyPressed(HotKey obj)
        {
            if(this.WindowState == WindowState.Minimized)
            {
                this.Show();
                this.Activate();
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.Hide();
                this.WindowState = WindowState.Minimized;
            }
        }

        public ReactiveCommand<object> CloseToTrayCommand { get; private set; }
        public ReactiveCommand<object> AutoHideMainMenuCommand { get; private set; }
        public ReactiveCommand<object> SetFontCommand { get; private set; }

        private void ToggleCloseToTray()
        {
            if (this.settings.CloseToSystemTray)
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            else
                Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private void ToogleAutoHideMainMenu()
        {
            if (settings.AutoHideMainMenu)
            {
                menuMain.Height = 0;
                InputManager.Current.EnterMenuMode += OnEnterMenuMode;
                InputManager.Current.LeaveMenuMode += OnLeaveMenuMode;
            }
            else
            {
                menuMain.ClearValue(HeightProperty);
                InputManager.Current.EnterMenuMode -= OnEnterMenuMode;
                InputManager.Current.LeaveMenuMode -= OnLeaveMenuMode;
            }
        }

        private void OnLeaveMenuMode(object sender, EventArgs e)
        {
            menuMain.Height = 0;
        }

        private void OnEnterMenuMode(object sender, EventArgs e)
        {
            if (InputManager.Current.MostRecentInputDevice == InputManager.Current.PrimaryKeyboardDevice)
                menuMain.ClearValue(HeightProperty);
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized && settings.MinimizedToSystemTray)
                this.Hide();
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (settings.CloseToSystemTray)
            {
                e.Cancel = true;
                this.WindowState = WindowState.Minimized;
                this.Hide();
                hotKey.Dispose();
            }
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (settings == null)
                return;

            if (settings.StartMinimized)
                this.WindowState = WindowState.Minimized;
        }

        private void OnTasksCopyCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lbTasks.SelectedItems.Count > 0) && (!txtTask.IsFocused);
            e.Handled = true;
        }

        private void OnTasksCopyExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var viewModel = DataContext as ShellViewModel;

            Clipboard.SetDataObject(viewModel.GetSelectedTasksText());
        }

        private void OnTasksPasteCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Clipboard.ContainsText();
            e.Handled = true;
        }

        private void OnTasksPasteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!Clipboard.ContainsText())
                return;

            var viewModel = DataContext as ShellViewModel;

            string clipboardText = Clipboard.GetText();
            string[] lines = clipboardText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            viewModel.InsertStringsAsTasks(lines);
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > 0 && e.NewSize.Width > 0 && WindowState != WindowState.Maximized)
            {
                this.settings.WindowLocation = new Rect(this.Left, this.Top, this.Width, this.Height);
            }
            if (WindowState == WindowState.Maximized || WindowState == WindowState.Minimized)
            {
                this.settings.WindowLocation = this.RestoreBounds;
            }
        }

        private bool IsVisibleOnAnyScreen(Rect rect)
        {
            foreach(var screen in System.Windows.Forms.Screen.AllScreens)
            {
                if (screen.WorkingArea.IntersectsWith(rect.ToRectangle()))
                    return true;
            }
            return false;
        }

        private void SetWindowPosition()
        {
            if (this.settings.WindowLocation != Rect.Empty && IsVisibleOnAnyScreen(this.settings.WindowLocation))
            {
                this.Top = settings.WindowLocation.Top;
                this.Left = settings.WindowLocation.Left;
                this.Width = settings.WindowLocation.Width;
                this.Height = settings.WindowLocation.Height;
            }
        }

        private void OnWindowLocationChanged(object sender, EventArgs e)
        {
            if (this.Left >= 0 && this.Top >= 0 && (WindowState != WindowState.Maximized || WindowState != WindowState.Minimized))
            {
                this.settings.WindowLocation = new Rect(Convert.ToInt32(this.Left), Convert.ToInt32(this.Top), Convert.ToInt32(this.Width), Convert.ToInt32(this.Height));
            }
        }
    }
}