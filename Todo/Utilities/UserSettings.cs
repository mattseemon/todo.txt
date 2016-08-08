using Akavache;
using Lager;
using System.Windows.Media;

namespace Seemon.Todo.Utilities
{
    public class UserSettings : SettingsStorage
    {
        public UserSettings(IBlobCache blobCache = null)
            : base("__SETTINGS__", blobCache ?? BlobCache.UserAccount)
        { }

        public bool StartOnWindowsStartup
        {
            get { return this.GetOrCreate(false); }
            set { this.SetOrCreate(value); }
        }

        public bool StartMinimized
        {
            get { return this.GetOrCreate(false); }
            set { this.SetOrCreate(value); }
        }

        public bool MinimizedToSystemTray
        {
            get { return this.GetOrCreate(true); }
            set { this.SetOrCreate(value); }
        }

        public bool CloseToSystemTray
        {
            get { return this.GetOrCreate(false); }
            set { this.SetOrCreate(value); }
        }

        public bool SingleClickToOpen
        {
            get { return this.GetOrCreate(false); }
            set { this.SetOrCreate(value); }
        }

        public bool AutoHideMainMenu
        {
            get { return this.GetOrCreate(true); }
            set { this.SetOrCreate(value); }
        }

        public bool EnableDebugLogging
        {
            get { return this.GetOrCreate(false); }
            set { this.SetOrCreate(value); }
        }

        public string ArchiveFilePath
        {
            get { return this.GetOrCreate(string.Empty); }
            set { this.SetOrCreate(value); }
        }

        public bool AutomaticallySelectArchivePath
        {
            get { return this.GetOrCreate(false); }
            set { this.SetOrCreate(value); }
        }

        public bool AutomaticallyArchiveCompletedTasks
        {
            get { return this.GetOrCreate(false); }
            set { this.SetOrCreate(value); }
        }

        public bool ConfirmBeforeDeletingTasks
        {
            get { return this.GetOrCreate(true); }
            set { this.SetOrCreate(value); }
        }

        public bool AddCreatedDateToTasks
        {
            get { return this.GetOrCreate(false); }
            set { this.SetOrCreate(value); }
        }

        public bool MoveFocusToTaskListAfterTaskCreation
        {
            get { return this.GetOrCreate(true); }
            set { this.SetOrCreate(value); }
        }

        public bool AutomaticallyRefreshTaskListFromFile
        {
            get { return this.GetOrCreate(false); }
            set { this.SetOrCreate(value); }
        }

        public bool UseControlEnterToCreateTask
        {
            get { return this.GetOrCreate(false); }
            set { this.SetOrCreate(value); }
        }

        public bool PreserveWhiteSpaceAndBlankLines
        {
            get { return this.GetOrCreate(false); }
            set { this.SetOrCreate(value); }
        }

        public bool ApplyWordWarpToTaskList
        {
            get { return this.GetOrCreate(false); }
            set { this.SetOrCreate(value); }
        }

        public bool FilterTextIsCaseSensitive
        {
            get { return this.GetOrCreate(false); }
            set { this.SetOrCreate(value); }
        }

        public bool ProjectAndContextIntellisenseAreCaseSensitive
        {
            get { return this.GetOrCreate(false); }
            set { this.SetOrCreate(value); }
        }

        public bool AllowGroupingOfTasks
        {
            get { return this.GetOrCreate(true); }
            set { this.SetOrCreate(value); }
        }

        public SortType SelectedSortType
        {
            get { return this.GetOrCreate(SortType.None); }
            set { this.SetOrCreate(value); }
        }

        public string LastLoadedFilePath
        {
            get { return this.GetOrCreate(string.Empty); }
            set { this.SetOrCreate(value); }
        }

        public int FontSize
        {
            get { return this.GetOrCreate(12); }
            set { this.SetOrCreate(value); }
        }

        public FontFamily FontFamily
        {
            get { return this.GetOrCreate(new FontFamily("Segoe UI")); }
            set
            {
                this.SetOrCreate(value);
                this.FontStyle = value.FamilyTypefaces[0];
            }
        }

        public FamilyTypeface FontStyle
        {
            get { return this.GetOrCreate(this.FontFamily.FamilyTypefaces[0]); }
            set { this.SetOrCreate(value); }
        }

        public string CurrentFilter
        {
            get { return this.GetOrCreate(string.Empty); }
            set { this.SetOrCreate(value); }
        }

        public string FilterPreset1
        {
            get { return this.GetOrCreate(string.Empty); }
            set { this.SetOrCreate(value); }
        }

        public string FilterPreset2
        {
            get { return this.GetOrCreate(string.Empty); }
            set { this.SetOrCreate(value); }
        }

        public string FilterPreset3
        {
            get { return this.GetOrCreate(string.Empty); }
            set { this.SetOrCreate(value); }
        }

        public string FilterPreset4
        {
            get { return this.GetOrCreate(string.Empty); }
            set { this.SetOrCreate(value); }
        }

        public string FilterPreset5
        {
            get { return this.GetOrCreate(string.Empty); }
            set { this.SetOrCreate(value); }
        }

        public string FilterPreset6
        {
            get { return this.GetOrCreate(string.Empty); }
            set { this.SetOrCreate(value); }
        }

        public string FilterPreset7
        {
            get { return this.GetOrCreate(string.Empty); }
            set { this.SetOrCreate(value); }
        }

        public string FilterPreset8
        {
            get { return this.GetOrCreate(string.Empty); }
            set { this.SetOrCreate(value); }
        }

        public string FilterPreset9
        {
            get { return this.GetOrCreate(string.Empty); }
            set { this.SetOrCreate(value); }
        }
    }
}
