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
using System.Reactive.Subjects;

namespace Seemon.Todo.Utilities
{
    public class AppUpdater : ReactiveObject, IEnableLogger, IDisposable
    {
        public enum UpdateStatus
        {
            None,
            Initializing,
            SearchingForUpdates,
            SearchingForUpdatesFailed,
            NoUpdateFound,
            UpdateFound,
            InstallingUpdate,
            UpdateComplete,
            InstallingUpdateFailed
        }

        private Action<int> ProgressChanged;

        private readonly IUpdateManager updateManager;
        private NuGet.SemanticVersion currentVersion;
        private NuGet.SemanticVersion updateVersion;
        private UserSettings settings;
        private IWindowManager windowManager;

        private bool updating = false;

        private Subject<UpdateStatus> currentStatus = new Subject<UpdateStatus>();

        public IObservable<UpdateStatus> Status
        {
            get { return this.currentStatus.AsObservable(); }
        }

        public ReactiveCommand<bool> UpdateAppCommand { get; private set; }

        public AppUpdater(IUpdateManager updateManager, Action<int> progress = null)
        {
            this.Log().Info("Initialize Application Updater.");

            this.currentStatus.OnNext(UpdateStatus.Initializing);

            this.updateManager = updateManager;
            this.windowManager = Locator.Current.GetService<IWindowManager>();
            this.settings = Locator.Current.GetService<UserSettings>();
            this.ProgressChanged = progress;
            this.currentVersion = updateManager.CurrentlyInstalledVersion();

#if PORTABLE
            this.UpdateAppCommand = ReactiveCommand.CreateAsyncTask(_ => this.UpdatePortableAppAsync());
#else
            this.UpdateAppCommand = ReactiveCommand.CreateAsyncTask(_ => this.UpdateAppAsync(this.settings.ConfirmBeforeUpdate));
#endif
            if (this.settings.CheckForUpdates)
                Observable.Interval(TimeSpan.FromHours(8), RxApp.TaskpoolScheduler)
                    .StartWith(0)
                    .InvokeCommand(this.UpdateAppCommand);

            this.currentStatus.OnNext(UpdateStatus.None);

            this.Log().Info("Application Updater Initialized.");
        }

        public async Task<bool> CheckUpdateAsync()
        {
            this.Log().Info("Start Application Update Check.");

            this.currentStatus.OnNext(UpdateStatus.SearchingForUpdates);

            bool hasUpdate = false;
            try
            {
                UpdateInfo info = await updateManager.CheckForUpdate();

                this.updateVersion = info.FutureReleaseEntry.Version;

                hasUpdate = !this.currentVersion.Equals(info.FutureReleaseEntry.Version);

                if (hasUpdate)
                {
                    this.currentStatus.OnNext(UpdateStatus.UpdateFound);
                    this.Log().Info("New application update found. New Version: {0}", info.FutureReleaseEntry.Version.ToString());
                }
                else
                {
                    this.currentStatus.OnNext(UpdateStatus.NoUpdateFound);
                    this.Log().Info("No application updates found.");
                }
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
            this.currentStatus.OnNext(UpdateStatus.None);
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

                    td.InstructionText = "Application Update Available";
                    td.Caption = "TODO.TXT - UPDATE";
                    td.Text = string.Format("A New version of TODO.TXT is available. Do you want to download and install it?\n\nInstalled Version: {0}\nUpdate Version: {1}", this.currentVersion.ToString(), this.updateVersion.ToString());
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
                        updating = false;

                    this.Log().Debug("DEBUG: Application updating set to {0}", updating);
                }

                if(updating)
                {
                    this.Log().Info("Application update started.");
                    

                    try
                    {
                        this.currentStatus.OnNext(UpdateStatus.InstallingUpdate);
                        this.Log().Info("Updating application to newer version.");
                        ReleaseEntry info = await updateManager.UpdateApp(this.ProgressChanged);
                        this.Log().Info("Completed download and updating");
                        hasUpdated = true;
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

                    if (hasUpdated)
                        this.currentStatus.OnNext(UpdateStatus.UpdateComplete);
                    else
                        this.currentStatus.OnNext(UpdateStatus.InstallingUpdateFailed);

                    updating = false;
                }
            }

            this.currentStatus.OnNext(UpdateStatus.None);

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
                td.Text = string.Format("A New version of TODO.TXT is available. Do you want to download and install it?\n\nInstalled Version: {0}\nUpdate Version: {1}", currentVersion.ToString(), updateVersion.ToString());
                td.Icon = TaskDialogStandardIcon.Information;
                td.StandardButtons = TaskDialogStandardButtons.Cancel;

                TaskDialogCommandLink btnUpdateNow = new TaskDialogCommandLink("btnUpdateNow", "Download this update now");
                btnUpdateNow.Click += (o, e) =>
                {
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(AppInfo.PortableLocation, Path.Combine(KnownFolders.GetPath(KnownFolder.Downloads), "TodotxtPortable.7z"));
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
            this.currentStatus.OnCompleted();
            updateManager.Dispose();
        }
    }
}
