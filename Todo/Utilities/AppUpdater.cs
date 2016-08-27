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
            this.Log().Info("Initialize Application Updater.");

            this.updateManager = updateManager;
            this.windowManager = Locator.Current.GetService<IWindowManager>();
            this.settings = Locator.Current.GetService<UserSettings>();

            this.UpdateAppCommand = ReactiveCommand.CreateAsyncTask(_ => this.UpdateAppAsync(this.settings.ConfirmBeforeUpdate));

            if (this.settings.CheckForUpdates)
                Observable.Interval(TimeSpan.FromHours(8), RxApp.TaskpoolScheduler)
                    .StartWith(0)
                    .InvokeCommand(this.UpdateAppCommand);

            this.Log().Info("Application Updater Initialized.");
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
            this.Log().Info("Start Application Update Check.");
            bool hasUpdate = false;
            try
            {
                UpdateInfo info = await updateManager.CheckForUpdate();

                this.updateVersion = info.FutureReleaseEntry.Version;

                hasUpdate = !this.CurrentVersion.Equals(info.FutureReleaseEntry.Version);

                if (hasUpdate)
                    this.Log().Info("New application update found. New Version: {0}", info.FutureReleaseEntry.Version.ToString());
                else
                    this.Log().Info("No application updates found.");
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
            this.Log().Info("Application update check completed.");
            return hasUpdate;
        }

        public async Task<bool> UpdateAppAsync(bool showUI)
        {
            this.Log().Info("Start Application Update.");
            UpdateViewModel updateViewModel = null;
            bool autoUpdate = true;
            bool hasUpdate = await this.CheckUpdateAsync();
            settings.LastUpdateCheck = DateTime.Now;
            bool hasUpdated = false;

            Action<int> progress = null;

            if(hasUpdate)
            {
                if(showUI)
                {
                    this.Log().Info("Display update UI.");
                    TaskDialog td = new TaskDialog();

                    td.InstructionText = "Application Update Available";
                    td.Caption = "TODO.TXT - UPDATE";
                    td.Text = string.Format("A New version of TODO.TXT is available. Do you want to download and install it?\n\nInstalled Version: {0}\nUpdate Version: {1}", this.currentVersion.ToString(), this.UpdateVersion.ToString());
                    td.Icon = TaskDialogStandardIcon.Information;
                    td.StandardButtons = TaskDialogStandardButtons.Cancel;

                    TaskDialogCommandLink btnUpdateNow = new TaskDialogCommandLink("btnUpdateNow", "Download and install this update now");
                    btnUpdateNow.Click += (o, e) =>
                    {
                        updateViewModel = new UpdateViewModel(this);
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
                    this.Log().Info("Application update started.");
                    try
                    {
                        if (await this.CheckUpdateAsync())
                        {
                            this.Log().Info("Updating application to newer version.");
                            ReleaseEntry info = await updateManager.UpdateApp(progress);
                            this.Log().Info("Completed download and updating");
                            hasUpdated = true;

                            if (updateViewModel != null)
                            {
                                updateViewModel.TryClose(true);
                                this.Log().Info("Closed update UI.");
                            }

                            this.Log().Info("Restarting application after update.");
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
