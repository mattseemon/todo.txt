using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using ReactiveUI;
using Seemon.Todo.Utilities;
using Splat;
using System;

namespace Seemon.Todo.ViewModels
{
    public class AboutViewModel : ReactiveScreen, IEnableLogger
    {
        private IWindowManager windowManager;

        public CreditViewModel CreditViewModel { get; private set; }

        public AboutViewModel()
        {
            this.Log().Info("Initialize About Dialog");
            this.windowManager = Locator.Current.GetService<IWindowManager>();
            this.DisplayName = "ABOUT";
            this.CreditViewModel = new CreditViewModel();

            this.ShowCreditsCommand = ReactiveCommand.Create();
            this.ShowCreditsCommand.Subscribe(x => this.CreditViewModel.Show());
        }

        public void Show()
        {
            this.windowManager.ShowDialog(this);
        }

        public string Title { get { return AppInfo.Title; } }

        public string Product { get { return AppInfo.Product; } }

        public string Version { get { return "Version: " + AppInfo.Version; } }

        public string FullVersion { get { return "Version: " + AppInfo.FullVersion; } }

        public string Description { get { return AppInfo.Description; } }

        public string Company { get { return AppInfo.Company; } }

        public string Copyright { get { return AppInfo.Copyright; } }

        public string OriginalNotice { get { return "This is a re-write of todotxt.net by Ben Huges. All credit for the work goes to him. You can find the original version at http://benrhughes.github.io/todotxt.net/"; } }

        public ReactiveCommand<object> ShowCreditsCommand { get; private set; }
    }
}
