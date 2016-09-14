using Caliburn.Micro;
using ReactiveUI;
using Splat;
using Squirrel;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Seemon.Todo.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Net;
using System.IO;

namespace Seemon.Todo.Utilities
{
    public class AppUpdater : ReactiveObject, IEnableLogger, IDisposable
    {
        private Action<int> ProgressChanged;

        private readonly IUpdateManager updateManager;
        NuGet.SemanticVersion currentVersion;
        NuGet.SemanticVersion updateVersion;
        private UserSettings settings;
        private IWindowManager windowManager;

        private bool updating = false;

        public bool Updating
        {
            get { return this.updating; }
            set { this.RaiseAndSetIfChanged(ref this.updating, value); }
        }

        public AppUpdater(IUpdateManager updateManager, Action<int> progress = null)
        {
            this.Log().Info("Initialize Application Updater.");

            this.updateManager = updateManager;
            this.windowManager = Locator.Current.GetService<IWindowManager>();
            this.settings = Locator.Current.GetService<UserSettings>();
            this.ProgressChanged = progress;

#if PORTABLE
            this.UpdateAppCommand = ReactiveCommand.CreateAsyncTask(_ => this.UpdatePortableAppAsync());
#else
            this.UpdateAppCommand = ReactiveCommand.CreateAsyncTask(_ => this.UpdateAppAsync(this.settings.ConfirmBeforeUpdate));

#endif
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
            bool hasUpdate = await this.CheckUpdateAsync();
            this.updating = true;
            settings.LastUpdateCheck = DateTime.Now;
            bool hasUpdated = false;

            if(hasUpdate)
            {
                if(showUI)
                {
                    this.Log().Info("Display update UI.");
                    TaskDialog td = new TaskDialog();

                    this.Log().Debug("DEBUG: Disbabling auto update since we have to show UI.");
                    this.updating = false;

                    td.InstructionText = "Application Update Available";
                    td.Caption = "TODO.TXT - UPDATE";
                    td.Text = string.Format("A New version of TODO.TXT is available. Do you want to download and install it?\n\nInstalled Version: {0}\nUpdate Version: {1}", this.currentVersion.ToString(), this.UpdateVersion.ToString());
                    td.Icon = TaskDialogStandardIcon.Information;
                    td.StandardButtons = TaskDialogStandardButtons.Cancel;

                    TaskDialogCommandLink btnUpdateNow = new TaskDialogCommandLink("btnUpdateNow", "Download and install this update now");
                    btnUpdateNow.Click += (o, e) =>
                    {
                        this.Log().Debug("DEBUG: Update Alert OK  Clicked.");
                        td.Close(TaskDialogResult.Ok);
                    };

                    td.Controls.Add(btnUpdateNow);

                    if (td.Show() != TaskDialogResult.Ok)
                        updating = true;
                }

                if(updating)
                {
                    this.Log().Info("Application update started.");
                    try
                    {
                        if (await this.CheckUpdateAsync())
                        {
                            this.Log().Info("Updating application to newer version.");
                            ReleaseEntry info = await updateManager.UpdateApp(this.ProgressChanged);
                            this.Log().Info("Completed download and updating");
                            hasUpdated = true;
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
                    updating = false;
                }
            }
            return hasUpdated;
        }

        public async Task<bool> UpdatePortableAppAsync()
        {
            this.Log().Info("Start Application Update.");
            bool hasUpdate = await this.CheckUpdateAsync();

            settings.LastUpdateCheck = DateTime.Now;

            if (hasUpdate)
            {
                TaskDialog td = new TaskDialog();

                td.InstructionText = "Application Update Available";
                td.Caption = "TODO.TXT - UPDATE";
                td.Text = string.Format("A New version of TODO.TXT is available. Do you want to download and install it?\n\nInstalled Version: {0}\nUpdate Version: {1}", CurrentVersion.ToString(), UpdateVersion.ToString());
                td.Icon = TaskDialogStandardIcon.Information;
                td.StandardButtons = TaskDialogStandardButtons.Cancel;

                TaskDialogCommandLink btnUpdateNow = new TaskDialogCommandLink("btnUpdateNow", "Download this update now");
                btnUpdateNow.Click += (o, e) =>
                {
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(AppInfo.PortableLocation, Path.Combine(KnownFolders.GetPath(KnownFolder.Downloads), "TodotxtPortable.zip"));
                    td.Close(TaskDialogResult.Ok);
                };

                td.Controls.Add(btnUpdateNow);

                if (td.Show() == TaskDialogResult.Ok)
                    return true;

                return false;
            }

            return true;
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
