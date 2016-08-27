using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using ReactiveUI;
using Seemon.Todo.Utilities;
using Splat;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seemon.Todo.ViewModels
{
    public class UpdateViewModel : ReactiveScreen, IEnableLogger
    {
        private AppUpdater updater;
                
        public UpdateViewModel(AppUpdater updater)
        {
            this.DisplayName = "Update TODO.TXT";
            this.updater = updater;
            this.CloseCommand = ReactiveCommand.Create();
            this.CloseCommand.Subscribe(x => this.TryClose(true));
        }

        private int progress;
        public int Progress
        {
            get { return this.progress; }
            set { this.RaiseAndSetIfChanged(ref this.progress, value); }
        }

        public string CurrentVersion
        {
            get { return updater.CurrentVersion.ToString(); }
        }

        public string UpdateVersion
        {
            get { return this.updater.UpdateVersion.ToString(); }
        }

        public ReactiveCommand<object> CloseCommand { get; private set; }

        public void UpdateProgress(int progress)
        {
            this.Progress = progress;
        }
    }
}
