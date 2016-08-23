using Caliburn.Micro;
using ReactiveUI;
using Splat;
using Squirrel;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Seemon.Todo.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Seemon.Todo.Utilities
{
    public class AppUpdater : IEnableLogger, IDisposable
    {

        private readonly IUpdateManager updateManager;
        NuGet.SemanticVersion currentVersion;
        NuGet.SemanticVersion updateVersion;
        private UserSettings settings;
        private IWindowManager windowManager;

        public AppUpdater(IUpdateManager updateManager)
        {
            this.updateManager = updateManager;
            this.windowManager = Locator.Current.GetService<IWindowManager>();
            this.settings = Locator.Current.GetService<UserSettings>();

            this.UpdateAppCommand = ReactiveCommand.CreateAsyncTask(_ => this.UpdateAppAsync(this.settings.ConfirmBeforeUpdate));

            if (this.settings.CheckForUpdates)
                Observable.Interval(TimeSpan.FromHours(8), RxApp.TaskpoolScheduler)
                    .StartWith(0)
                    .InvokeCommand(this.UpdateAppCommand);
        }

        public NuGet.SemanticVersion CurrentVersion
        {
            get
            {
                if (currentVersion == null)
                {
                    currentVersion = updateManager.CurrentlyInstalledVersion();
                }
                return currentVersion;
            }
        }

        public NuGet.SemanticVersion UpdateVersion
        {
            get
            {
                return updateVersion;
            }
        }

        public ReactiveCommand<bool> UpdateAppCommand { get; private set; }

        public async Task<bool> CheckUpdateAsync()
        {
            bool hasUpdate = false;
            try
            {
                UpdateInfo info = await updateManager.CheckForUpdate();

                this.updateVersion = info.FutureReleaseEntry.Version;

                hasUpdate = !this.CurrentVersion.Equals(info.FutureReleaseEntry.Version);
            }
            catch (System.Net.WebException ex)
            {
                this.Log().Error(string.Format("HasNewUpdate failed with the web exception {0}", ex.Message));
                hasUpdate = false;
            }
            catch (System.Exception ex)
            {
                this.Log().Error(string.Format("HasNewUpdate failed with the unexpected exception {0}", ex.Message));
                hasUpdate = false;
            }
            return hasUpdate;
        }

        private async Task<bool> UpdateAppAsync(bool showUI)
        {
            bool autoUpdate = true;
            bool hasUpdate = await this.CheckUpdateAsync();
            settings.LastUpdateCheck = DateTime.Now;
            bool hasUpdated = false;

            Action<int> progress = null;

            if(hasUpdate)
            {
                if(showUI)
                {
                    TaskDialog td = new TaskDialog();

                    td.InstructionText = "Application Update Available";
                    td.Caption = "TODO.TXT - UPDATE";
                    td.Text = string.Format("A New version of TODO.TXT is available. Do you want to download and install it?\n\nInstalled Version: {0}\nUpdate Version: {1}", this.currentVersion.ToString(), this.UpdateVersion.ToString());
                    td.Icon = TaskDialogStandardIcon.Information;
                    td.StandardButtons = TaskDialogStandardButtons.Cancel;

                    TaskDialogCommandLink btnUpdateNow = new TaskDialogCommandLink("btnUpdateNow", "Download and install this update now");
                    btnUpdateNow.Click += (o, e) =>
                    {
                        UpdateViewModel updateViewModel = new UpdateViewModel(this);
                        progress = updateViewModel.UpdateProgress;
                        windowManager.ShowWindow(updateViewModel);
                        td.Close(TaskDialogResult.Ok);
                    };

                    td.Controls.Add(btnUpdateNow);

                    if (td.Show() != TaskDialogResult.Ok)
                        autoUpdate = false;
                }

                if(autoUpdate)
                {
                    try
                    {
                        if (await this.CheckUpdateAsync())
                        {
                            ReleaseEntry info = await updateManager.UpdateApp(progress);
                            hasUpdated = true;

                            //TODO: Add check to see if restart after install is required.
                            RestartApplication();
                        }
                    }
                    catch (System.Net.WebException ex)
                    {
                        this.Log().Error(string.Format("UpdateApp failed with the web exception {0}", ex.Message));
                        hasUpdated = false;
                    }
                    catch (System.Exception ex)
                    {
                        this.Log().Error(string.Format("UpdateApp failed with the unexpected exception {0}", ex.Message));
                        hasUpdated = false;
                    }
                }
            }
            return hasUpdated;
        }

        public void RestartApplication()
        {
            UpdateManager.RestartApp();
            this.Dispose();
        }

        public void Dispose()
        {
            updateManager.Dispose();
        }
    }
}
