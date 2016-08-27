using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using ReactiveUI;
using Seemon.Todo.Utilities;
using Seemon.Todo.Views;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Seemon.Todo.ViewModels
{
    public class OptionsViewModel : ReactiveScreen, IEnableLogger
    {
        private IWindowManager windowManager;
        private UserSettings settings;
        private OptionsView window;

        public readonly ObservableAsPropertyHelper<bool> startOnWindowsStartup;
        public readonly ObservableAsPropertyHelper<bool> startMinimized;
        public readonly ObservableAsPropertyHelper<bool> minimizedToSystemTray;
        public readonly ObservableAsPropertyHelper<bool> closeToSystemTray;
        public readonly ObservableAsPropertyHelper<bool> singleClickToOpen;
        public readonly ObservableAsPropertyHelper<bool> enableDebugLogging;
        public readonly ObservableAsPropertyHelper<bool> autoHideMainMenu;
        public readonly ObservableAsPropertyHelper<bool> automaticallySelectArchivePath;
        public readonly ObservableAsPropertyHelper<string> archiveFilePath;
        public readonly ObservableAsPropertyHelper<bool> automaticallyArchiveCompletedTasks;
        public readonly ObservableAsPropertyHelper<bool> confirmBeforeDeletingTasks;
        public readonly ObservableAsPropertyHelper<bool> addCreatedDateToTasks;
        public readonly ObservableAsPropertyHelper<bool> moveFocusToTaskListAfterTaskCreation;
        public readonly ObservableAsPropertyHelper<bool> automaticallyRefreshTaskListFromFile;
        public readonly ObservableAsPropertyHelper<bool> useControlEnterToCreateTask;
        public readonly ObservableAsPropertyHelper<bool> preserveWhiteSpaceAndBlankLines;
        public readonly ObservableAsPropertyHelper<bool> applyWordWarpToTaskList;
        public readonly ObservableAsPropertyHelper<bool> filterTextIsCaseSensitive;
        public readonly ObservableAsPropertyHelper<bool> projectAndContextIntellisenseAreCaseSensitive;
        public readonly ObservableAsPropertyHelper<bool> allowGroupingOfTasks;
        public readonly ObservableAsPropertyHelper<int> fontSize;
        public readonly ObservableAsPropertyHelper<FontFamily> fontFamily;
        public readonly ObservableAsPropertyHelper<FamilyTypeface> fontStyle;
        public readonly ObservableAsPropertyHelper<bool> checkForUpdates;
        public readonly ObservableAsPropertyHelper<bool> confirmBeforeUpdate;
        public readonly ObservableAsPropertyHelper<DateTime?> lastUpdateCheck;

        public bool StartOnWindowsStartup
        {
            get { return this.startOnWindowsStartup.Value; }
            set
            {
                this.settings.StartOnWindowsStartup = value;
                StartUpManager.CreateCurrentUserShortcut(value);
            }
        }

        public bool StartMinimized
        {
            get { return this.startMinimized.Value; }
            set { this.settings.StartMinimized = value; }
        }

        public bool MinimizedToSystemTray
        {
            get { return this.minimizedToSystemTray.Value; }
            set { this.settings.MinimizedToSystemTray = value; }
        }

        public bool CloseToSystemTray
        {
            get { return this.closeToSystemTray.Value; }
            set { this.settings.CloseToSystemTray = value; }
        }

        public bool SingleClickToOpen
        {
            get { return this.singleClickToOpen.Value; }
            set { this.settings.SingleClickToOpen = value; }
        }

        public bool EnableDebugLogging
        {
            get { return this.enableDebugLogging.Value; }
            set { this.settings.EnableDebugLogging = value; }
        }

        public bool AutoHideMainMenu
        {
            get { return this.autoHideMainMenu.Value; }
            set { this.settings.AutoHideMainMenu = value; }
        }

        public bool AutomaticallyArchiveCompletedTasks
        {
            get { return this.automaticallyArchiveCompletedTasks.Value; }
            set { this.settings.AutomaticallyArchiveCompletedTasks = value; }
        }

        public bool AutomaticallySelectArchivePath
        {
            get { return this.automaticallySelectArchivePath.Value; }
            set { this.settings.AutomaticallySelectArchivePath = value; }
        }

        public string ArchiveFilePath
        {
            get { return this.archiveFilePath.Value; }
            set { this.settings.ArchiveFilePath = value; }
        }

        public bool ConfirmBeforeDeletingTasks
        {
            get { return this.confirmBeforeDeletingTasks.Value; }
            set { this.settings.ConfirmBeforeDeletingTasks = value; }
        }

        public bool AddCreatedDateToTasks
        {
            get { return this.addCreatedDateToTasks.Value; }
            set { this.settings.AddCreatedDateToTasks = value; }
        }

        public bool MoveFocusToTaskListAfterTaskCreation
        {
            get { return this.moveFocusToTaskListAfterTaskCreation.Value; }
            set { this.settings.MoveFocusToTaskListAfterTaskCreation = value; }
        }

        public bool AutomaticallyRefreshTaskListFromFile
        {
            get { return this.automaticallyRefreshTaskListFromFile.Value; }
            set { this.settings.AutomaticallyRefreshTaskListFromFile = value; }
        }

        public bool UseControlEnterToCreateTask
        {
            get { return this.useControlEnterToCreateTask.Value; }
            set { this.settings.UseControlEnterToCreateTask = value; }
        }

        public bool PreserveWhiteSpaceAndBlankLines
        {
            get { return this.preserveWhiteSpaceAndBlankLines.Value; }
            set { this.settings.PreserveWhiteSpaceAndBlankLines = value; }
        }

        public bool ApplyWordWarpToTaskList
        {
            get { return this.applyWordWarpToTaskList.Value; }
            set { this.settings.ApplyWordWarpToTaskList = value; }
        }

        public bool FilterTextIsCaseSensitive
        {
            get { return this.filterTextIsCaseSensitive.Value; }
            set { this.settings.FilterTextIsCaseSensitive = value; }
        }

        public bool ProjectAndContextIntellisenseAreCaseSensitive
        {
            get { return this.projectAndContextIntellisenseAreCaseSensitive.Value; }
            set { this.settings.ProjectAndContextIntellisenseAreCaseSensitive = value; }
        }

        public bool AllowGroupingOfTasks
        {
            get { return this.allowGroupingOfTasks.Value; }
            set { this.settings.AllowGroupingOfTasks = value; }
        }

        public int FontSize
        {
            get { return this.fontSize.Value; }
            set { this.settings.FontSize = value; }
        }

        public FontFamily FontFamily
        {
            get { return this.fontFamily.Value; }
            set { this.settings.FontFamily = value; }
        }

        public FamilyTypeface FontStyle
        {
            get { return this.fontStyle.Value; }
            set { this.settings.FontStyle = value; }
        }
        public bool CheckForUpdates
        {
            get { return this.checkForUpdates.Value; }
            set { this.settings.CheckForUpdates = value; }
        }

        public bool ConfirmBeforeUpdate
        {
            get { return this.confirmBeforeUpdate.Value; }
            set { this.settings.ConfirmBeforeUpdate = value; }
        }

        public DateTime? LastUpdateCheck
        {
            get { return this.lastUpdateCheck.Value; }
            set
            {
                this.settings.LastUpdateCheck = value;
                this.LastUpdateLabel = value.HasValue ? string.Format("Last Update: {0:MM-dd-yyyy hh:mm tt}", this.LastUpdateCheck.Value) : "Last Update: Never";
            }
        }

        string lastUpdateLabel = string.Empty;
        public string LastUpdateLabel
        {
            get { return lastUpdateLabel; }
            set { this.RaiseAndSetIfChanged(ref this.lastUpdateLabel, value); }
        }

        public ReactiveCommand<object> BrowseArchiveCommand { get; private set; }
        public ReactiveCommand<object> ResetToDefaultsCommand { get; private set; }
        public ReactiveCommand<object> UpdateNowCommand { get; private set; }

        public OptionsViewModel()
        {
            this.Log().Info("Initialize Options Dialog");
            this.windowManager = Locator.Current.GetService<IWindowManager>();
            this.DisplayName = "OPTIONS";
            this.settings = Locator.Current.GetService<UserSettings>();

            this.startOnWindowsStartup = this.settings.WhenAnyValue(x => x.StartOnWindowsStartup).ToProperty(this, x => x.StartOnWindowsStartup);
            this.startMinimized = this.settings.WhenAnyValue(x => x.StartMinimized).ToProperty(this, x => x.StartMinimized);
            this.minimizedToSystemTray = this.settings.WhenAnyValue(x => x.MinimizedToSystemTray).ToProperty(this, x => x.MinimizedToSystemTray);
            this.closeToSystemTray = this.settings.WhenAnyValue(x => x.CloseToSystemTray).ToProperty(this, x => x.CloseToSystemTray);
            this.singleClickToOpen = this.settings.WhenAnyValue(x => x.SingleClickToOpen).ToProperty(this, x => x.SingleClickToOpen);
            this.enableDebugLogging = this.settings.WhenAnyValue(x => x.EnableDebugLogging).ToProperty(this, x => x.EnableDebugLogging);
            this.autoHideMainMenu = this.settings.WhenAnyValue(x => x.AutoHideMainMenu).ToProperty(this, x => x.AutoHideMainMenu);
            this.automaticallyArchiveCompletedTasks = this.settings.WhenAnyValue(x => x.AutomaticallyArchiveCompletedTasks).ToProperty(this, x => x.AutomaticallyArchiveCompletedTasks);
            this.automaticallySelectArchivePath = this.settings.WhenAnyValue(x => x.AutomaticallySelectArchivePath).ToProperty(this, x => x.AutomaticallySelectArchivePath);
            this.archiveFilePath = this.settings.WhenAnyValue(x => x.ArchiveFilePath).ToProperty(this, x => x.ArchiveFilePath);
            this.confirmBeforeDeletingTasks = this.settings.WhenAnyValue(x => x.ConfirmBeforeDeletingTasks).ToProperty(this, x => x.ConfirmBeforeDeletingTasks);
            this.addCreatedDateToTasks = this.settings.WhenAnyValue(x => x.AddCreatedDateToTasks).ToProperty(this, x => x.AddCreatedDateToTasks);
            this.moveFocusToTaskListAfterTaskCreation = this.settings.WhenAnyValue(x => x.MoveFocusToTaskListAfterTaskCreation).ToProperty(this, x => x.MoveFocusToTaskListAfterTaskCreation);
            this.automaticallyRefreshTaskListFromFile = this.settings.WhenAnyValue(x => x.AutomaticallyRefreshTaskListFromFile).ToProperty(this, x => x.AutomaticallyRefreshTaskListFromFile);
            this.useControlEnterToCreateTask = this.settings.WhenAnyValue(x => x.UseControlEnterToCreateTask).ToProperty(this, x => x.UseControlEnterToCreateTask);
            this.preserveWhiteSpaceAndBlankLines = this.settings.WhenAnyValue(x => x.PreserveWhiteSpaceAndBlankLines).ToProperty(this, x => x.PreserveWhiteSpaceAndBlankLines);
            this.applyWordWarpToTaskList = this.settings.WhenAnyValue(x => x.ApplyWordWarpToTaskList).ToProperty(this, x => x.ApplyWordWarpToTaskList);
            this.filterTextIsCaseSensitive = this.settings.WhenAnyValue(x => x.FilterTextIsCaseSensitive).ToProperty(this, x => x.FilterTextIsCaseSensitive);
            this.projectAndContextIntellisenseAreCaseSensitive = this.settings.WhenAnyValue(x => x.ProjectAndContextIntellisenseAreCaseSensitive).ToProperty(this, x => x.ProjectAndContextIntellisenseAreCaseSensitive);
            this.allowGroupingOfTasks = this.settings.WhenAnyValue(x => x.AllowGroupingOfTasks).ToProperty(this, x => x.AllowGroupingOfTasks);
            this.fontSize = this.settings.WhenAnyValue(x => x.FontSize).ToProperty(this, x => x.FontSize);
            this.fontFamily = this.settings.WhenAnyValue(x => x.FontFamily).ToProperty(this, x => x.FontFamily);
            this.fontStyle = this.settings.WhenAnyValue(x => x.FontStyle).ToProperty(this, x => x.FontStyle);
            this.checkForUpdates = this.settings.WhenAnyValue(x => x.CheckForUpdates).ToProperty(this, x => x.CheckForUpdates);
            this.confirmBeforeUpdate = this.settings.WhenAnyValue(x => x.ConfirmBeforeUpdate).ToProperty(this, x => x.ConfirmBeforeUpdate);
            this.lastUpdateCheck = this.settings.WhenAnyValue(x => x.LastUpdateCheck).ToProperty(this, x => x.LastUpdateCheck);
            this.LastUpdateLabel = this.LastUpdateCheck.HasValue ? string.Format("Last Update: {0:MM-dd-yyyy hh:mm tt}", this.LastUpdateCheck.Value) : "Last Update: Never";

            this.BrowseArchiveCommand = ReactiveCommand.Create();
            this.BrowseArchiveCommand.Subscribe(x => this.DoBrowseArchive());

            this.ResetToDefaultsCommand = ReactiveCommand.Create();
            this.ResetToDefaultsCommand.Subscribe(x => this.DoResetToDefaults());

            this.UpdateNowCommand = ReactiveCommand.Create();
            this.UpdateNowCommand.Subscribe(x => this.DoUpdateNow());
        }

        private async void DoUpdateNow()
        {
            this.Log().Info("Starting manual application update check.");
            await Locator.Current.GetService<AppUpdater>().UpdateAppAsync(true);
            this.Log().Info("Completed manual application update check.");
        }

        private void DoResetToDefaults()
        {
            var td = new TaskDialog();
            td.Caption = "TODO.TXT - RESET TO DEFAULTS";
            td.InstructionText = "Do you want to reset all settings to defaults?";
            td.Text = "Will cause all application settings to be reset to default values including, last opened file & filter presets and will restart the application.";
            td.Icon = TaskDialogStandardIcon.Warning;
            td.FooterIcon = TaskDialogStandardIcon.Shield;
            td.FooterText = "This task might require elevated permissions. If prompted, please click on Yes. Clicking no will result in orphaned entries in the windows registry.";
            var btnYes = new TaskDialogCommandLink("btnYes", "Yes, reset all settings");
            btnYes.UseElevationIcon = true;
            btnYes.Click += (o, e) =>
                {
                    Akavache.BlobCache.UserAccount.InvalidateAll();
                    StartUpManager.RemovApplicationShortcutFromCurrentUserStartup();
                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                };

            var btnNo = new TaskDialogCommandLink("btnNo", "No, do not reset settings");
            btnNo.Default = true;
            btnNo.Click += (o, e) =>
                {
                    td.Close();
                };
            td.Controls.Add(btnYes);
            td.Controls.Add(btnNo);
            td.StartupLocation = TaskDialogStartupLocation.CenterOwner;
            td.OwnerWindowHandle = new WindowInteropHelper(window).Handle;
            td.Show();
        }

        public void Show()
        {
            windowManager.ShowDialog(this);
        }

        public void DoBrowseArchive()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = false;
            openFileDialog.CheckPathExists = true;
            openFileDialog.AddExtension = true;
            openFileDialog.DefaultExt = "*.txt";
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog.FileName = "done.txt";
            if (openFileDialog.ShowDialog() == true)
                this.ArchiveFilePath = openFileDialog.FileName;
        }

        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);
            window = (OptionsView)view;
        }
    }
}
