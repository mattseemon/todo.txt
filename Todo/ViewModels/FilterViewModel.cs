using Caliburn.Micro.ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using Seemon.Todo.Views;
using Seemon.Todo.Models;
using Seemon.Todo.Utilities;
using Splat;
using System.Windows.Input;

namespace Seemon.Todo.ViewModels
{
    public class FilterViewModel : ReactiveScreen
    {
        private FilterView window = null;
        private string filter = string.Empty;
        private string preset1;
        private string preset2;
        private string preset3;
        private string preset4;
        private string preset5;
        private string preset6;
        private string preset7;
        private string preset8;
        private string preset9;

        public TaskList TaskManager { get; private set; }
        public UserSettings UserSettings { get; private set; }

        public string Filter
        {
            get { return this.filter; }
            set { this.RaiseAndSetIfChanged(ref this.filter, value); }
        }

        public string Preset1
        {
            get { return this.preset1; }
            set { this.RaiseAndSetIfChanged(ref this.preset1, value); }
        }

        public string Preset2
        {
            get { return this.preset2; }
            set { this.RaiseAndSetIfChanged(ref this.preset2, value); }
        }

        public string Preset3
        {
            get { return this.preset3; }
            set { this.RaiseAndSetIfChanged(ref this.preset3, value); }
        }

        public string Preset4
        {
            get { return this.preset4; }
            set { this.RaiseAndSetIfChanged(ref this.preset4, value); }
        }

        public string Preset5
        {
            get { return this.preset5; }
            set { this.RaiseAndSetIfChanged(ref this.preset5, value); }
        }

        public string Preset6
        {
            get { return this.preset6; }
            set { this.RaiseAndSetIfChanged(ref this.preset6, value); }
        }

        public string Preset7
        {
            get { return this.preset7; }
            set { this.RaiseAndSetIfChanged(ref this.preset7, value); }
        }

        public string Preset8
        {
            get { return this.preset8; }
            set { this.RaiseAndSetIfChanged(ref this.preset8, value); }
        }

        public string Preset9
        {
            get { return this.preset9; }
            set { this.RaiseAndSetIfChanged(ref this.preset9, value); }
        }

        public ReactiveCommand<object> OKCommand { get; private set; }
        public ReactiveCommand<object> CancelCommand { get; private set; }
        public ReactiveCommand<object> ClearActiveCommand { get; private set; }
        public ReactiveCommand<object> ClearAllCommand { get; private set; }

        public FilterViewModel(TaskList taskManager)
        {
            this.DisplayName = "TODO.TXT - FILTERS";

            this.UserSettings = Locator.Current.GetService<UserSettings>();
            this.TaskManager = taskManager;

            this.OKCommand = ReactiveCommand.Create();
            this.OKCommand.Subscribe(x => this.OnOK());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(x => this.OnCancel());

            this.ClearActiveCommand = ReactiveCommand.Create();
            this.ClearActiveCommand.Subscribe(x => this.OnClearActive());

            this.ClearAllCommand = ReactiveCommand.Create();
            this.ClearAllCommand.Subscribe(x => this.OnClearAll());
        }

        private bool previousKeyWasEnter = false;

        public void OnPreviewKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Enter && previousKeyWasEnter)
                this.OnOK();
            else if (e.Key == Key.Enter)
                previousKeyWasEnter = true;
            else
                previousKeyWasEnter = false;
        }

        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);

            window = (FilterView)view;
            this.UpdateControls();
            window.txtFilter.Focus();
        }

        private void UpdateControls()
        {
            this.Filter = UserSettings.CurrentFilter;
            this.Preset1 = UserSettings.FilterPreset1;
            this.Preset2 = UserSettings.FilterPreset2;
            this.Preset3 = UserSettings.FilterPreset3;
            this.Preset4 = UserSettings.FilterPreset4;
            this.Preset5 = UserSettings.FilterPreset5;
            this.Preset6 = UserSettings.FilterPreset6;
            this.Preset7 = UserSettings.FilterPreset7;
            this.Preset8 = UserSettings.FilterPreset8;
            this.Preset9 = UserSettings.FilterPreset9;
        }

        private void OnOK()
        {
            UserSettings.FilterPreset1 = this.Preset1.Trim();
            UserSettings.FilterPreset2 = this.Preset2.Trim();
            UserSettings.FilterPreset3 = this.Preset3.Trim();
            UserSettings.FilterPreset4 = this.Preset4.Trim();
            UserSettings.FilterPreset5 = this.Preset5.Trim();
            UserSettings.FilterPreset6 = this.Preset6.Trim();
            UserSettings.FilterPreset7 = this.Preset7.Trim();
            UserSettings.FilterPreset8 = this.Preset8.Trim();
            UserSettings.FilterPreset9 = this.Preset9.Trim();
            UserSettings.CurrentFilter = this.Filter.Trim();

            this.TryClose(true);
        }

        private void OnCancel()
        {
            this.TryClose(false);
        }

        private void OnClearActive()
        {
            this.Filter = string.Empty;
        }

        private void OnClearAll()
        {
            this.Filter = string.Empty;
            this.Preset1 = string.Empty;
            this.Preset2 = string.Empty;
            this.Preset3 = string.Empty;
            this.Preset4 = string.Empty;
            this.Preset5 = string.Empty;
            this.Preset6 = string.Empty;
            this.Preset7 = string.Empty;
            this.Preset8 = string.Empty;
            this.Preset9 = string.Empty;
        }
    }
}