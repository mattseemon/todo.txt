using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using ReactiveUI;
using Seemon.Todo.Converters;
using Seemon.Todo.Extensions;
using Seemon.Todo.Models;
using Seemon.Todo.Utilities;
using Seemon.Todo.Views;
using Splat;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Seemon.Todo.ViewModels
{
    public class ShellViewModel : ReactiveScreen, IEnableLogger, IDisposable
    {
        private IWindowManager windowManager;
        private CollectionView collectionView = null;
        private FileChangeObserver fileChangeObserver = null;
        private ShellView window = null;
        private Task updating;
        private int countItemsInCurrentGroup;
        private List<CollectionViewGroup> collectionViewGroups;
        private int nextGroupAtTaskNumber;
        private List<Task> selectedTasks;
        private string statusInformation = string.Empty;
        private bool showCalendar = false;
        private bool showPrintPreview = false;
        private AppUpdater appUpdater = null;
        private IEnumerable<Task> sortedTasks;

        private int updateProgress = 0;
        private int totalTasks = 0;
        private int filteredTasks = 0;
        private int incompleteTasks = 0;
        private int tasksDueToday = 0;
        private int tasksOverDue = 0;

        public readonly ObservableAsPropertyHelper<SortType> sortType;
        public readonly ObservableAsPropertyHelper<string> currentFilter;

        public UserSettings UserSettings { get; private set; }
        public OptionsViewModel OptionsViewModel { get; private set; }
        public AboutViewModel AboutViewModel { get; private set; }
        public HelpViewModel HelpViewModel { get; private set; }
        public NotificationViewModel Notification { get; private set; }

        public ReactiveCommand<object> FileNewCommand { get; private set; }
        public ReactiveCommand<object> ToolsOptionsCommand { get; private set; }
        public ReactiveCommand<object> HelpAboutCommand { get; private set; }
        public ReactiveCommand<object> HelpViewHelpCommand { get; private set; }
        public ReactiveCommand<object> FileExitCommand { get; private set; }
        public ReactiveCommand<object> ToolsShowCalendarCommand { get; private set; }
        public ReactiveCommand<object> ToogleFileChangeObserverCommand { get; private set; }
        public ReactiveCommand<object> UpdateTitleCommand { get; private set; }
        public ReactiveCommand<object> FileOpenCommand { get; private set; }
        public ReactiveCommand<object> FilePrintCommand { get; private set; }
        public ReactiveCommand<object> FilePrintPreviewCommand { get; private set; }
        public ReactiveCommand<object> FileArchiveCompletedTasksCommand { get; private set; }
        public ReactiveCommand<object> FileReloadFileCommand { get; private set; }
        public ReactiveCommand<object> TaskNewCommand { get; private set; }
        public ReactiveCommand<object> TaskUpdateCommand { get; private set; }
        public ReactiveCommand<object> TaskAppendCommand { get; private set; }
        public ReactiveCommand<object> TaskDeleteCommand { get; private set; }
        public ReactiveCommand<object> TaskDuplicateCommand { get; private set; }
        public ReactiveCommand<object> TaskToggleCompletionCommand { get; private set; }
        public ReactiveCommand<object> TaskSetPriorityCommand { get; private set; }
        public ReactiveCommand<object> TaskIncreasePriorityCommand { get; private set; }
        public ReactiveCommand<object> TaskDecreasePriorityCommand { get; private set; }
        public ReactiveCommand<object> TaskRemovePriorityCommand { get; private set; }
        public ReactiveCommand<object> TaskSetDueDateCommand { get; private set; }
        public ReactiveCommand<object> TaskPostponeCommand { get; private set; }
        public ReactiveCommand<object> TaskIncreaseDueDateCommand { get; private set; }
        public ReactiveCommand<object> TaskDecreaseDueDateCommand { get; private set; }
        public ReactiveCommand<object> TaskRemoveDueDateCommand { get; private set; }
        public ReactiveCommand<object> TaskSummaryCommand { get; private set; }
        public ReactiveCommand<object> KeyUpEventCommand { get; private set; }
        public ReactiveCommand<object> KeyDownEventCommand { get; private set; }
        public ReactiveCommand<object> SortCommand { get; private set; }
        public ReactiveCommand<object> FilterDefineFiltersCommand { get; private set; }
        public ReactiveCommand<object> FilterCommand { get; private set; }
        public ReactiveCommand<object> PrintPreviewCancelCommand { get; private set; }
        public ReactiveCommand<object> ToolsSwitchModeCommand { get; private set; }
        public ReactiveCommand<object> HelpViewErrorLogCommand { get; private set; }
        public ReactiveCommand<object> HelpCheckForUpdatesCommand { get; private set; }

        public IDisposable updateStatusSubscription;

        public TaskList TaskManager { get; set; }

        public SortType SortType
        {
            get { return this.sortType.Value; }
            set { this.UserSettings.SelectedSortType = value; }
        }

        public bool IsPortable
        {
            get { return !AppInfo.PortableStoragePath.IsNullOrEmpty(); }
        }

        public string CurrentFilter
        {
            get { return this.currentFilter.Value; }
        }

        public string StatusInformation
        {
            get { return this.statusInformation; }
            set { this.RaiseAndSetIfChanged(ref this.statusInformation, value); }
        }

        public bool ShowCalendar
        {
            get { return this.showCalendar; }
            set { this.RaiseAndSetIfChanged(ref this.showCalendar, value); this.ShowHideCalendar(); }
        }

        public bool ShowPrintPreview
        {
            get { return this.showPrintPreview; }
            set { this.RaiseAndSetIfChanged(ref this.showPrintPreview, value); }
        }

        public int UpdateProgress
        {
            get { return this.updateProgress; }
            set { this.RaiseAndSetIfChanged(ref this.updateProgress, value); }
        }

        public int TotalTasks
        {
            get { return this.totalTasks; }
            set { this.RaiseAndSetIfChanged(ref this.totalTasks, value); }
        }

        public int FilteredTasks
        {
            get { return this.filteredTasks; }
            set { this.RaiseAndSetIfChanged(ref this.filteredTasks, value); }
        }

        public int IncompleteTasks
        {
            get { return this.incompleteTasks; }
            set { this.RaiseAndSetIfChanged(ref this.incompleteTasks, value); }
        }

        public int TasksDueToday
        {
            get { return this.tasksDueToday; }
            set { this.RaiseAndSetIfChanged(ref this.tasksDueToday, value); }
        }

        public int TasksOverDue
        {
            get { return this.tasksOverDue; }
            set { this.RaiseAndSetIfChanged(ref this.tasksOverDue, value); }
        }

        public ShellViewModel()
        {
            this.Log().Info("Initializing todo.txt window");
            this.windowManager = Locator.Current.GetService<IWindowManager>();
            this.UserSettings = Locator.Current.GetService<UserSettings>();

            this.DisplayName = "TODO.TXT";

            this.OptionsViewModel = new OptionsViewModel();
            this.AboutViewModel = new AboutViewModel();
            this.HelpViewModel = new HelpViewModel();
            this.Notification = new NotificationViewModel();
            
            this.FileNewCommand = ReactiveCommand.Create();
            this.FileNewCommand.Subscribe(x => this.OnFileNew());

            this.FileOpenCommand = ReactiveCommand.Create();
            this.FileOpenCommand.Subscribe(x => this.OnFileOpen());

            this.FilePrintCommand = ReactiveCommand.Create();
            this.FilePrintCommand.Subscribe(x => this.OnFilePrint());

            this.FilePrintPreviewCommand = ReactiveCommand.Create();
            this.FilePrintPreviewCommand.Subscribe(x => this.OnFilePrintPreview());

            this.PrintPreviewCancelCommand = ReactiveCommand.Create();
            this.PrintPreviewCancelCommand.Subscribe(x => { this.ShowPrintPreview = false; this.window.dvPrintPreview.Focus(); });

            this.FileArchiveCompletedTasksCommand = ReactiveCommand.Create(this.UserSettings.WhenAny(x => x.AutomaticallyArchiveCompletedTasks, (x) => !x.Value));
            this.FileArchiveCompletedTasksCommand.Subscribe(x => this.ArchiveCompleted());

            this.FileReloadFileCommand = ReactiveCommand.Create();
            this.FileReloadFileCommand.Subscribe(x => this.ReloadFile());

            this.FileExitCommand = ReactiveCommand.Create();
            this.FileExitCommand.Subscribe(x => App.Current.Shutdown());

            this.TaskNewCommand = ReactiveCommand.Create();
            this.TaskNewCommand.Subscribe(x => this.OnAddNewTask());

            this.TaskUpdateCommand = ReactiveCommand.Create(this.WhenAny(x => x.window.lbTasks.SelectedItems.Count, y => y.window.txtTask.IsFocused, (x, y) => x.Value == 1 && !y.Value));
            this.TaskUpdateCommand.Subscribe(x => this.OnUpdateTask());

            this.TaskAppendCommand = ReactiveCommand.Create(this.WhenAny(x => x.window.lbTasks.SelectedItems.Count, y => y.window.txtTask.IsFocused, (x, y) => x.Value > 0 && !y.Value));
            this.TaskAppendCommand.Subscribe(x => this.OnAppendTasks());

            this.TaskDeleteCommand = ReactiveCommand.Create(this.WhenAny(x => x.window.lbTasks.SelectedItems.Count, y => y.window.txtTask.IsFocused, (x, y) => x.Value > 0 && !y.Value));
            this.TaskDeleteCommand.Subscribe(x => this.OnDeleteTasks());

            this.TaskDuplicateCommand = ReactiveCommand.Create(this.WhenAny(x => x.window.lbTasks.SelectedItems.Count, y => y.window.txtTask.IsFocused, (x, y) => x.Value == 1 && !y.Value));
            this.TaskDuplicateCommand.Subscribe(x => this.OnDuplicateTask());

            this.TaskToggleCompletionCommand = ReactiveCommand.Create(this.WhenAny(x => x.window.lbTasks.SelectedItems.Count, y => y.window.txtTask.IsFocused, (x, y) => x.Value > 0 && !y.Value));
            this.TaskToggleCompletionCommand.Subscribe(x => this.OnToggleCompletionTask());

            this.TaskSetPriorityCommand = ReactiveCommand.Create(this.WhenAny(x => x.window.lbTasks.SelectedItems.Count, y => y.window.txtTask.IsFocused, (x, y) => x.Value > 0 && !y.Value));
            this.TaskSetPriorityCommand.Subscribe(x => this.OnSetPriority());

            this.TaskIncreasePriorityCommand = ReactiveCommand.Create(this.WhenAny(x => x.window.lbTasks.SelectedItems.Count, y => y.window.txtTask.IsFocused, (x, y) => x.Value > 0 && !y.Value));
            this.TaskIncreasePriorityCommand.Subscribe(x => this.OnIncreasePriority());

            this.TaskDecreasePriorityCommand = ReactiveCommand.Create(this.WhenAny(x => x.window.lbTasks.SelectedItems.Count, y => y.window.txtTask.IsFocused, (x, y) => x.Value > 0 && !y.Value));
            this.TaskDecreasePriorityCommand.Subscribe(x => this.OnDecreasePriority());

            this.TaskRemovePriorityCommand = ReactiveCommand.Create(this.WhenAny(x => x.window.lbTasks.SelectedItems.Count, y => y.window.txtTask.IsFocused, (x, y) => x.Value > 0 && !y.Value));
            this.TaskRemovePriorityCommand.Subscribe(x => this.OnRemovePriority());

            this.TaskSetDueDateCommand = ReactiveCommand.Create(this.WhenAny(x => x.window.lbTasks.SelectedItems.Count, y => y.window.txtTask.IsFocused, (x, y) => x.Value > 0 && !y.Value));
            this.TaskSetDueDateCommand.Subscribe(x => this.OnSetDueDate());

            this.TaskPostponeCommand = ReactiveCommand.Create(this.WhenAny(x => x.window.lbTasks.SelectedItems.Count, y => y.window.txtTask.IsFocused, (x, y) => x.Value > 0 && !y.Value));
            this.TaskPostponeCommand.Subscribe(x => this.OnPostPone());

            this.TaskIncreaseDueDateCommand = ReactiveCommand.Create(this.WhenAny(x => x.window.lbTasks.SelectedItems.Count, y => y.window.txtTask.IsFocused, (x, y) => x.Value > 0 && !y.Value));
            this.TaskIncreaseDueDateCommand.Subscribe(x => this.OnIncreaseDueDate());

            this.TaskDecreaseDueDateCommand = ReactiveCommand.Create(this.WhenAny(x => x.window.lbTasks.SelectedItems.Count, y => y.window.txtTask.IsFocused, (x, y) => x.Value > 0 && !y.Value));
            this.TaskDecreaseDueDateCommand.Subscribe(x => this.OnDecreaseDueDate());

            this.TaskRemoveDueDateCommand = ReactiveCommand.Create(this.WhenAny(x => x.window.lbTasks.SelectedItems.Count, y => y.window.txtTask.IsFocused, (x, y) => x.Value > 0 && !y.Value));
            this.TaskRemoveDueDateCommand.Subscribe(x => this.OnRemoveDueDate());

            this.TaskSummaryCommand = ReactiveCommand.Create();
            this.TaskSummaryCommand.Subscribe(x => this.OnTaskSummary());

            this.SortCommand = ReactiveCommand.Create();
            this.SortCommand.Subscribe(x => this.OnSort(x));

            this.FilterDefineFiltersCommand = ReactiveCommand.Create();
            this.FilterDefineFiltersCommand.Subscribe(x => this.OnDefineFilters());

            this.FilterCommand = ReactiveCommand.Create();
            this.FilterCommand.Subscribe(x => this.OnFilter(x));

            this.ToolsOptionsCommand = ReactiveCommand.Create();
            this.ToolsOptionsCommand.Subscribe(x => this.OptionsViewModel.Show());

            this.HelpAboutCommand = ReactiveCommand.Create();
            this.HelpAboutCommand.Subscribe(x => this.AboutViewModel.Show());

            this.HelpViewErrorLogCommand = ReactiveCommand.Create();
            this.HelpViewErrorLogCommand.Subscribe(x => this.OnFeatureNotImplemented("View error log."));

            this.HelpCheckForUpdatesCommand = ReactiveCommand.Create();
            this.HelpCheckForUpdatesCommand.Subscribe(x => this.OnCheckForUpdates());

            this.HelpViewHelpCommand = ReactiveCommand.Create();
            this.HelpViewHelpCommand.Subscribe(x => this.HelpViewModel.Show());

            this.ToolsShowCalendarCommand = ReactiveCommand.Create();
            this.ToolsShowCalendarCommand.Subscribe(x => this.ShowCalendar = !this.ShowCalendar);

            this.ToogleFileChangeObserverCommand = ReactiveCommand.Create();
            this.ToogleFileChangeObserverCommand.Subscribe(x => this.ToggleFileChangeObserver());

            this.UpdateTitleCommand = ReactiveCommand.Create();
            this.UpdateTitleCommand.Subscribe(x => this.UpdateWindowTitle());

            this.KeyUpEventCommand = ReactiveCommand.Create();
            this.KeyUpEventCommand.Subscribe(x => this.EmulateUpArrow());

            this.KeyDownEventCommand = ReactiveCommand.Create();
            this.KeyDownEventCommand.Subscribe(x => this.EmulateDownArrow());

            this.sortType = this.UserSettings.WhenAnyValue(x => x.SelectedSortType).ToProperty(this, x => x.SortType);
            this.currentFilter = this.UserSettings.WhenAnyValue(x => x.CurrentFilter).ToProperty(this, x => x.CurrentFilter);

            this.UserSettings.WhenAnyValue(x => x.AutomaticallyRefreshTaskListFromFile).InvokeCommand(this, x => x.ToogleFileChangeObserverCommand);
            this.UserSettings.WhenAnyValue(x => x.LastLoadedFilePath).InvokeCommand(this, x => x.ToogleFileChangeObserverCommand);
            this.UserSettings.WhenAnyValue(x => x.LastLoadedFilePath).InvokeCommand(this, x => x.UpdateTitleCommand);

            this.selectedTasks = new List<Task>();
            this.SortType = UserSettings.SelectedSortType;

            this.Log().Info("Completed initalization");
        }

        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);
            window = (ShellView)view;

            this.Log().Info("Loading last opened todo.txt file.");
            if (!string.IsNullOrEmpty(this.UserSettings.LastLoadedFilePath) && File.Exists(this.UserSettings.LastLoadedFilePath))
                LoadTasks(this.UserSettings.LastLoadedFilePath);
            else
                this.UserSettings.LastLoadedFilePath = string.Empty;

            this.Log().Info("Initializing application updater");
            this.appUpdater = new AppUpdater(Locator.Current.GetService<IUpdateManager>());
            this.updateStatusSubscription = this.appUpdater.Status.Subscribe(this.OnUpdateStatusChanged);

            Locator.CurrentMutable.Register(() => this.appUpdater, typeof(AppUpdater));
        }

        private void OnUpdateStatusChanged(AppUpdater.UpdateStatus currentStatus)
        {
            switch(currentStatus)
            {
                case AppUpdater.UpdateStatus.NoUpdateFound:
                    if(manualUpdateCheck)
                    {
                        this.Notification.ShowInformation("There are no updates currently available.");
                    }
                    break;
                case AppUpdater.UpdateStatus.UpdateComplete:
                    this.Notification.ShowInformation("The latest version of TODO.TXT has been installed. Please restart the application to see the new changes", "Close", 0, () =>
                    {
                        this.appUpdater.RestartApplication();
                    }, "Restart");
                    break;
            }
        }

        public void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Log().Info("Closing all windows.");
            this.HelpViewModel.TryClose();
            this.AboutViewModel.TryClose();
            this.OptionsViewModel.TryClose();
        }

        public void OnTaskTextKeyUp(object sender, KeyEventArgs e)
        {
            if (TaskManager == null)
            {
                window.ShowTaskDialog("Please use File\u2794Open (Ctrl+O) to open an existing file or File\u2794New (Ctrl+N) to create a new todo.txt file.", 
                    "You don't have a todo.txt file open.",
                    "TODO.TXT - Open or Create todo.txt file", TaskDialogStandardIcon.Information, TaskDialogStandardButtons.Close);
                e.Handled = false;
                window.lbTasks.Focus();
                return;
            }

            if(ShouldAddTask(e))
            {
                if (updating == null)
                    AddTaskFromTextBox();
                else
                    UpdateTaskFromTextBox();

                window.txtTask.Text = string.Empty;
                return;
            }

            switch(e.Key)
            {
                case Key.Escape:
                    updating = null;
                    if (window.txtTask.Text == string.Empty)
                    {
                        GetSelectedTasks();
                        SetSelectedTasks();
                    }
                    else
                        window.txtTask.Text = string.Empty;
                    break;
            }
        }

        private void OnFeatureNotImplemented(string feature)
        {
            window.ShowTaskDialog(string.Format("{0} feature is not implemented. Will be updated in future releases.", feature), "Feature not implemented", "TODO.TXT", TaskDialogStandardIcon.Information, TaskDialogStandardButtons.Ok);
        }

        private void OnFileNew()
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = "todo.txt";
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text files (*.txt)|*.txt|All Files (*.*)|*.*";

            var result = dialog.ShowDialog();

            if (result.Value)
            {
                File.WriteAllText(dialog.FileName, string.Empty);
                LoadTasks(dialog.FileName);
            }
        }

        private void OnFileOpen()
        {
            var dialog = new OpenFileDialog();
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text files (*.txt)|*.txt|All Files (*.*)|*.*";

            var result = dialog.ShowDialog();

            if (result.Value)
                LoadTasks(dialog.FileName);
        }

        private void OnFilePrint()
        {
            PrintDialog dialog = new PrintDialog();
            FlowDocument printDocument = this.GetPrintContents();

            if (dialog.ShowDialog() != true) return;

            printDocument.ColumnWidth = 99999;
            printDocument.PageHeight = dialog.PrintableAreaHeight;
            printDocument.PageWidth = dialog.PrintableAreaWidth;

            IDocumentPaginatorSource ps = printDocument as IDocumentPaginatorSource;

            dialog.PrintDocument(ps.DocumentPaginator, "TODO.TXT Tasks");

            PrintPreviewCancelCommand.Execute(null);
        }

        private void OnFilePrintPreview()
        {
            window.dvPrintPreview.Document = GetPrintContents();
            this.ShowPrintPreview = true;
        }

        private void OnAddNewTask()
        {
            string filters = string.Empty;

            foreach (var filter in UserSettings.CurrentFilter.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (filter.Substring(0, 1) != "-")
                {
                    if (filter.Contains("due:active"))
                        filters = filters + " due:today";
                    else if (filter.Equals("DONE", StringComparison.Ordinal) || filter.Equals("-DONE", StringComparison.Ordinal))
                        continue;
                    else
                        filters = filters + " " + filter;
                }
            }

            window.txtTask.Text = filters;
            window.txtTask.Focus();
        }

        private void OnUpdateTask()
        {
            updating = (Task)window.lbTasks.SelectedItem;
            window.txtTask.Text = updating.ToString();
            window.txtTask.Select(window.txtTask.Text.Length, 0);
            window.txtTask.Focus();
        }

        private void OnAppendTasks()
        {
            string appendText = ShowAppendView();
            if (appendText.IsNullOrEmpty())
                return;

            ModifySelectedTasks(AppendTaskText, appendText);
        }

        private void OnDeleteTasks()
        {
            bool isTaskListFocused = window.lbTasks.IsKeyboardFocusWithin;

            var result = ShowDeleteConfirmation(window.lbTasks.SelectedItems.Count);

            if (!result)
                return;

            if (window.lbTasks.SelectedItems.Contains(updating))
                updating = null;

            GetSelectedTasks();
            DisableFileChangeObserver();

            try
            {
                TaskManager.ReloadTasks();

                foreach (var task in window.lbTasks.SelectedItems)
                    TaskManager.Delete((Task)task);
            }
            catch (Exception ex)
            {
                ex.Handle("Error deleting tasks", (IEnableLogger)this, this.window);
            }

            UpdateDisplayedTasks();

            if (isTaskListFocused)
                SelectTaskByIndex(0);

            EnableFileChangeObserver();
        }

        private void OnDuplicateTask()
        {
            var currentTask = window.lbTasks.SelectedItem as Task;
            if (currentTask != null)
            {
                window.txtTask.Text = currentTask.Raw;
                window.txtTask.Select(window.txtTask.Text.Length, 0);
                window.txtTask.Focus();
            }
        }

        private void OnToggleCompletionTask()
        {
            ModifySelectedTasks(SetTaskCompletion, null);
            if (UserSettings.AutomaticallyArchiveCompletedTasks && AreThereCompletedTasks())
                ArchiveCompleted();
        }

        private void OnSetPriority()
        {
            string priority = ShowSetPriorityView();

            if (string.IsNullOrEmpty(priority) || !Char.IsLetter((char)priority[0]))
                return;

            ModifySelectedTasks(SetTaskPriority, priority);
        }

        private void OnIncreasePriority()
        {
            ModifySelectedTasks(IncreaseTaskPriority, null);
        }

        private void OnDecreasePriority()
        {
            ModifySelectedTasks(DecreaseTaskPriority, null);
        }

        private void OnRemovePriority()
        {
            ModifySelectedTasks(RemovePriority, null);
        }

        private void OnSetDueDate()
        {
            DateTime? newDueDate = ShowSetDueDateView();

            if (newDueDate == null)
                return;

            ModifySelectedTasks(SetTaskDueDate, newDueDate);
        }

        private void OnPostPone()
        {
            int days = ShowPostPoneView();

            if (days == 0) return;

            ModifySelectedTasks(PostponeTask, days);
        }

        private void OnIncreaseDueDate()
        {
            ModifySelectedTasks(PostponeTask, 1);
        }

        private void OnDecreaseDueDate()
        {
            ModifySelectedTasks(PostponeTask, -1);
        }

        private void OnRemoveDueDate()
        {
            ModifySelectedTasks(RemoveTaskDueDate, null);
        }

        private void OnTaskSummary()
        {
            SummaryViewModel summaryViewModel = new SummaryViewModel(this.TaskManager.Tasks, sortedTasks.ToList());
            windowManager.ShowDialog(summaryViewModel);
        }

        private void OnDefineFilters()
        {
            ShowFilterView();
            GetSelectedTasks();
            UpdateDisplayedTasks();
            SetSelectedTasks();
        }

        private void OnSort(object x)
        {
            if (x != null)
            {
                this.SortType = (SortType)Enum.Parse(typeof(SortType), x.ToString());
            }
            this.SortTasks();
        }

        private void OnFilter(object x)
        {
            var preset = x != null ? int.Parse(x.ToString()) : -1;
            ApplyFilterPreset(preset);
        }

        private bool manualUpdateCheck = false;
        private async void OnCheckForUpdates()
        {
            this.Log().Info("Starting manual application update check.");
            this.manualUpdateCheck = true;
            await Locator.Current.GetService<AppUpdater>().UpdateAppAsync(true);
            this.manualUpdateCheck = false;
            this.Log().Info("Completed manual application update check.");
        }

        private void UpdateWindowTitle()
        {
            var displayString = string.Empty;

            if (UserSettings.LastLoadedFilePath.IsNullOrEmpty())
                displayString = string.Format("TODO.TXT");
            else
                displayString = string.Format("TODO.TXT - {0}", UserSettings.LastLoadedFilePath.ToUpper().Replace(@"\TODO.TXT", string.Empty));

            this.DisplayName = displayString;
        }

        private void AddTaskFromTextBox()
        {
            string taskRaw = window.txtTask.Text;
            if (!UserSettings.PreserveWhiteSpaceAndBlankLines)
                taskRaw = taskRaw.Trim();

            var taskString = taskRaw;

            if (taskString.Length == 0)
                return;

            if(UserSettings.AddCreatedDateToTasks)
            {
                var tmpTask = new Task(taskString);
                var today = DateTime.Today.ToString("yyyy-MM-dd");

                if(tmpTask.CreationDate.IsNullOrEmpty())
                {
                    if (tmpTask.Priority.IsNullOrEmpty())
                        taskString = today + " " + taskString;
                    else
                        taskString = taskString.Insert(tmpTask.Priority.Length, " " + today);
                }
            }

            try
            {
                Task task = new Task(taskString);
                TaskManager.Add(task);

                if (UserSettings.MoveFocusToTaskListAfterTaskCreation)
                {
                    window.lbTasks.Focus();
                    selectedTasks.Clear();
                    selectedTasks.Add(task);
                    UpdateDisplayedTasks();
                    SetSelectedTasks();
                }
                else
                {
                    GetSelectedTasks();
                    UpdateDisplayedTasks();
                    SetSelectedTasks();
                    window.txtTask.Focus();
                }
            }
            catch(TaskException ex)
            {
                window.ShowTaskDialog(ex.InnerException.Message, ex.Message, "TODO.TXT - Error Occurred", TaskDialogStandardIcon.Error, TaskDialogStandardButtons.Close);
            }
        }


        private void UpdateTaskFromTextBox()
        {
            string taskRaw = window.txtTask.Text;

            if (!UserSettings.PreserveWhiteSpaceAndBlankLines)
                taskRaw = window.txtTask.Text.Trim();

            var task = new Task(taskRaw);

            selectedTasks.Clear();
            selectedTasks.Add(task);
            try
            {
                TaskManager.UpdateTask(updating, task);
            }
            catch (Exception ex)
            {
                ex.Handle("Error updating task", (IEnableLogger)this, this.window);
            }
            updating = null;
            UpdateDisplayedTasks();
            SetSelectedTasks();
        }

        private void ShowHideCalendar()
        {
            string calendarString = string.Empty;

            if (this.ShowCalendar)
            {
                for (int i = 1; i < 8; i++)
                {
                    var today = DateTime.Now.AddDays(i).ToString("ddd MM/dd").ToUpper();
                    calendarString += string.Format(" {0} |", today);
                }
                if (calendarString.EndsWith(" |"))
                    calendarString = calendarString.Substring(0, calendarString.Length - 1);

                calendarString = calendarString.Trim();
            }
            else
                calendarString = string.Empty;

            this.StatusInformation = calendarString;
        }

        private void EnableFileChangeObserver()
        {
            if (!UserSettings.AutomaticallyArchiveCompletedTasks)
                return;

            if (string.IsNullOrEmpty(this.UserSettings.LastLoadedFilePath))
                return;

            if (fileChangeObserver != null)
            {
                fileChangeObserver.Dispose();
                fileChangeObserver = null;
            }

            this.Log().Debug("DEBUG: Enabling the file change observer for {0}", UserSettings.LastLoadedFilePath);
            fileChangeObserver = new FileChangeObserver();
            fileChangeObserver.OnFileChanged += () => window.Dispatcher.BeginInvoke(new System.Action(ReloadFile));
            fileChangeObserver.ObserveFile(UserSettings.LastLoadedFilePath);
            this.Log().Debug("DEBUG: File change observer enabled");
        }

        private void DisableFileChangeObserver()
        {
            if (fileChangeObserver == null)
                return;

            this.Log().Debug("DEBUG: Diabling the file change observer for {0}", UserSettings.LastLoadedFilePath);
            fileChangeObserver.Dispose();
            fileChangeObserver = null;
            this.Log().Debug("DEBUG: File change observer disabled");
        }

        private void ToggleFileChangeObserver()
        {
            if (this.UserSettings.AutomaticallyRefreshTaskListFromFile)
                this.EnableFileChangeObserver();
            else
                this.DisableFileChangeObserver();
        }

        private void GetSelectedTasks()
        {
            this.selectedTasks.Clear();

            foreach (var task in window.lbTasks.SelectedItems)
                this.selectedTasks.Add((Task)task);
        }

        public string GetSelectedTasksText()
        {
            int itemCount = 0;

            this.GetSelectedTasks();

            StringBuilder sbuilder = new StringBuilder(string.Empty);
            foreach (var item in this.selectedTasks)
            {
                itemCount++;
                if (itemCount > 1)
                    sbuilder.Append(Environment.NewLine);
                sbuilder.Append(item.Raw);
            }

            return sbuilder.ToString();
        }

        public void InsertStringsAsTasks(string[] lines)
        {
            this.DisableFileChangeObserver();

            foreach (var item in lines)
            {
                TaskManager.Add(new Task(item));
            }

            this.UpdateDisplayedTasks();
            this.EnableFileChangeObserver();
        }

        private bool CanExecuteOnSingleTask()
        {
            return (window.lbTasks.SelectedItems.Count == 1 && !window.txtTask.IsFocused);
        }

        private bool CanExecuteOnMultipleTasks()
        {
            return (window.lbTasks.SelectedItems.Count > 1 && !window.txtTask.IsFocused);
        }

        private void SetSelectedTasks()
        {
            if (selectedTasks == null || selectedTasks.Count == 0)
            {
                window.lbTasks.SelectedIndex = 0;
                return;
            }

            window.lbTasks.SelectedItems.Clear();
            int selectedItemCount = 0;

            for (int i = 0; i < window.lbTasks.Items.Count; i++)
            {
                Task listBoxItemTask = window.lbTasks.Items[i] as Task;

                int j = 0;
                while (j < selectedTasks.Count)
                {
                    Task task = selectedTasks[j];
                    if (listBoxItemTask.Raw.Equals(task.Raw))
                    {
                        window.lbTasks.SelectedItems.Add(window.lbTasks.Items[i]);

                        selectedItemCount++;
                        if (selectedItemCount == 1)
                            SelectTaskByIndex(i);

                        selectedTasks.RemoveAt(j);
                        break;
                    }
                    else
                        j++;
                }
            }

            if (selectedItemCount == 0)
            {
                window.lbTasks.SelectedIndex = 0;
                SelectTaskByIndex(0);
            }
        }

        private void SelectTaskByIndex(int index)
        {
            try
            {
                var listBoxItem = (ListBoxItem)window.lbTasks.ItemContainerGenerator.ContainerFromItem(window.lbTasks.Items[index]);
                listBoxItem.Focus();
            }
            catch
            {
                window.lbTasks.Focus();
            }
        }

        public void LoadTasks(string filePath)
        {
            this.Log().Info("Loading tasks from file {0}", filePath);
            try
            {
                this.TaskManager = new TaskList(filePath, UserSettings.PreserveWhiteSpaceAndBlankLines);
                UserSettings.LastLoadedFilePath = filePath;
                EnableFileChangeObserver();
                UpdateDisplayedTasks();

                if (UserSettings.AutomaticallyArchiveCompletedTasks && UserSettings.AutomaticallySelectArchivePath)
                    UserSettings.ArchiveFilePath = filePath.Replace("todo.txt", "done.txt");
            }
            catch (Exception ex)
            {
                ex.Handle("An error occured while opening " + filePath, (IEnableLogger)this, this.window);
            }
        }

        private void ReloadFile()
        {
            this.Log().Info("Reloading todo.txt file");
            try
            {
                TaskManager.ReloadTasks();
            }
            catch (Exception ex)
            {
                ex.Handle("Error loading tasks from todo.txt file", (IEnableLogger)this, this.window);
            }
            GetSelectedTasks();
            UpdateDisplayedTasks();
            SetSelectedTasks();
        }

        private void UpdateDisplayedTasks()
        {
            if (TaskManager == null)
                return;

            string sortProperty = string.Empty;

            try
            {
                sortedTasks = FilterTasks(TaskManager.Tasks);
                sortedTasks = SortTasks(sortedTasks);

                switch (SortType)
                {
                    case SortType.Project:
                        sortProperty = "Projects";
                        break;
                    case SortType.Context:
                        sortProperty = "Contexts";
                        break;
                    case SortType.DueDate:
                        sortProperty = "DueDate";
                        break;
                    case SortType.Completed:
                        sortProperty = "CompletedDate";
                        break;
                    case SortType.Priority:
                        sortProperty = "Priority";
                        break;
                    case SortType.Created:
                        sortProperty = "CreationDate";
                        break;
                }

                collectionView = (CollectionView)CollectionViewSource.GetDefaultView(sortedTasks);

                if (UserSettings.AllowGroupingOfTasks && SortType != SortType.Alphabetical && SortType != SortType.None)
                {
                    if (collectionView.CanGroup)
                    {
                        var groupDescription = new PropertyGroupDescription(sortProperty);
                        groupDescription.Converter = new GroupConverter();
                        collectionView.GroupDescriptions.Add(groupDescription);
                    }
                }
                else
                    collectionView.GroupDescriptions.Clear();

                var filteredTasks = sortedTasks.ToList();

                window.lbTasks.ItemsSource = sortedTasks;
                window.lbTasks.UpdateLayout();

                UpdateSummary(filteredTasks);
            }
            catch (Exception ex)
            {
                //this.Log().ErrorException("Error while sorting tasks", ex);
                ex.Handle("Error while sorting tasks", (IEnableLogger)this, this.window);
            }
        }

        protected void UpdateSummary(List<Task> selectedTaskList)
        {
            this.Log().Info("Updating task summary");
            this.TotalTasks = TaskManager.Tasks.Count;
            this.FilteredTasks = selectedTaskList.Count;

            int incompleteTask = 0, dueTodayTask = 0, overDueTask = 0;

            foreach(Task t in selectedTaskList)
            {
                if(!t.Completed)
                {
                    incompleteTask++;

                    if(!string.IsNullOrEmpty(t.DueDate))
                    {
                        DateTime dueDate;
                        if(DateTime.TryParseExact(t.DueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dueDate))
                        {
                            if (dueDate.Date == DateTime.Today.Date)
                                dueTodayTask++;
                            else if (dueDate.Date < DateTime.Today)
                                overDueTask++;
                        }
                    }
                }
            }

            this.IncompleteTasks = incompleteTask;
            this.TasksOverDue = overDueTask;
            this.TasksDueToday = dueTodayTask;
        }

        private bool IsTaskSelected()
        {
            return (window.lbTasks.SelectedItems.Count == 1);
        }

        private bool AreTasksSelected()
        {
            return (window.lbTasks.SelectedItems.Count > 0);
        }

        private bool AreThereCompletedTasks()
        {
            GetSelectedTasks();

            foreach(var task in selectedTasks)
            {
                if (task.Completed)
                    return true;
            }

            return false;
        }

        private bool ShouldAddTask(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (UserSettings.UseControlEnterToCreateTask)
                {
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                        return true;
                }
                else
                    return true;
            }
            return false;
        }

        private void EmulateDownArrow()
        {
            SendKeyDownEvent(window.lbTasks, Key.Down);
        }

        private void EmulateUpArrow()
        {
            SendKeyDownEvent(window.lbTasks, Key.Up);
        }

        private void SendKeyDownEvent(Control target, Key Key)
        {
            var routedEvent = Keyboard.KeyDownEvent;

            target.RaiseEvent(
                new KeyEventArgs(
                    Keyboard.PrimaryDevice,
                    PresentationSource.FromVisual(target),
                    0,
                    Key)
                { RoutedEvent = routedEvent }
                );
        }

        public void SortTasks()
        {
            GetSelectedTasks();            
            UpdateDisplayedTasks();
            SetSelectedTasks();
        }

        public IEnumerable<Task> SortTasks(IEnumerable<Task> tasks)
        {
            this.Log().Debug("DEBUG: Sorting {0} tasks by {1}", tasks.Count().ToString(), SortType.ToString());

            switch(SortType)
            {
                case SortType.Completed:
                    return tasks.OrderBy(t => t.Completed)
                        .ThenBy(t => t.Priority.IsNullOrEmpty() ? "(zzz)" : t.Priority)
                        .ThenBy(t => t.DueDate.IsNullOrEmpty() ? "9999-99-99" : t.DueDate)
                        .ThenBy(t => t.CreationDate.IsNullOrEmpty() ? "9999-99-99" : t.CreationDate);
                case SortType.Context:
                    return tasks.OrderBy(t =>
                        {
                            var s = "";
                            if (t.Contexts != null && t.Contexts.Count > 0)
                                s += t.PrimaryContext;
                            else
                                s += "zzz";
                            return s;
                        })
                        .ThenBy(t => t.Completed)
                        .ThenBy(t => t.Priority.IsNullOrEmpty() ? "(zzz)" : t.Priority)
                        .ThenBy(t => t.DueDate.IsNullOrEmpty() ? "9999-99-99" : t.DueDate)
                        .ThenBy(t => t.CreationDate.IsNullOrEmpty() ? "9999-99-99" : t.CreationDate);
                case SortType.Alphabetical:
                    return tasks.OrderBy(t => t.Raw);
                case SortType.DueDate:
                    return tasks.OrderBy(t => t.DueDate.IsNullOrEmpty() ? "9999-99-99" : t.DueDate)
                        .ThenBy(t => t.Completed)
                        .ThenBy(t => t.Priority.IsNullOrEmpty() ? "(zzz)" : t.Priority)
                        .ThenBy(t => t.CreationDate.IsNullOrEmpty() ? "9999-99-99" : t.CreationDate);
                case SortType.Priority:
                    return tasks.OrderBy(t => t.Priority.IsNullOrEmpty() ? "(zzz)" : t.Priority)
                        .ThenBy(t => t.Completed)
                        .ThenBy(t => t.DueDate.IsNullOrEmpty() ? "9999-99-99" : t.DueDate)
                        .ThenBy(t => t.CreationDate.IsNullOrEmpty() ? "9999-99-99" : t.CreationDate);
                case SortType.Project:
                    return tasks.OrderBy(t =>
                        {
                            var s = "";
                            if (t.Projects != null && t.Projects.Count > 0)
                                s += t.PrimaryProject;
                            else
                                s += "zzz";
                            return s;
                        })
                        .ThenBy(t => t.Completed)
                        .ThenBy(t => t.Priority.IsNullOrEmpty() ? "(zzz)" : t.Priority)
                        .ThenBy(t => t.DueDate.IsNullOrEmpty() ? "9999-99-99" : t.DueDate)
                        .ThenBy(t => t.CreationDate.IsNullOrEmpty() ? "9999-99-99" : t.CreationDate);
                case SortType.Created:
                    return tasks.OrderBy(t => t.CreationDate.IsNullOrEmpty() ? "9999-99-99" : t.CreationDate)
                        .ThenBy(t => t.Completed)
                        .ThenBy(t => t.Priority.IsNullOrEmpty() ? "(zzz)" : t.Priority)
                        .ThenBy(t => t.DueDate.IsNullOrEmpty() ? "9999-99-99" : t.DueDate);
                default:
                    return tasks;
            }
        }

        public IEnumerable<Task> FilterTasks(IEnumerable<Task> tasks)
        {
            var filters = UserSettings.CurrentFilter;
            var comparer = UserSettings.FilterTextIsCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

            if (filters.IsNullOrEmpty())
                return tasks;

            var filteredTasks = new List<Task>();

            foreach (var task in tasks)
            {
                bool include = true;
                foreach (var filter in filters.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (filter.Equals("due:today", StringComparison.OrdinalIgnoreCase)
                        && task.DueDate == DateTime.Now.ToString("yyyy-MM-dd"))
                        continue;
                    else if (filter.Equals("due:future", StringComparison.OrdinalIgnoreCase)
                        && task.DueDate.IsDateGreaterThan(DateTime.Now))
                        continue;
                    else if (filter.Equals("due:past", StringComparison.OrdinalIgnoreCase)
                        && task.DueDate.IsDateLessThan(DateTime.Now))
                        continue;
                    else if (filter.Equals("due:active", StringComparison.OrdinalIgnoreCase)
                        && !task.DueDate.IsNullOrEmpty()
                        && !task.DueDate.IsDateGreaterThan(DateTime.Now))
                        continue;
                    else if (filter.Equals("-due:today", StringComparison.OrdinalIgnoreCase)
                        && task.DueDate == DateTime.Now.ToString("yyyy-MM-dd"))
                    {
                        include = false;
                        continue;
                    }
                    else if (filter.Equals("-due:future", StringComparison.OrdinalIgnoreCase)
                        && task.DueDate.IsDateGreaterThan(DateTime.Now))
                    {
                        include = false;
                        continue;
                    }
                    else if (filter.Equals("-due:past", StringComparison.OrdinalIgnoreCase)
                        && task.DueDate.IsDateLessThan(DateTime.Now))
                    {
                        include = false;
                        continue;
                    }
                    else if (filter.Equals("-due:active", StringComparison.OrdinalIgnoreCase)
                        && !task.DueDate.IsNullOrEmpty()
                        && !task.DueDate.IsDateGreaterThan(DateTime.Now))
                    {
                        include = false;
                        continue;
                    }
                    else if (filter.Equals("-DONE", StringComparison.Ordinal) && task.Completed)
                    {
                        include = false;
                        continue;
                    }
                    else if (filter.Equals("DONE", StringComparison.Ordinal) && !task.Completed)
                    {
                        include = false;
                        continue;
                    }
                    else if (filter.Substring(0, 1) == "-")
                    {
                        if (task.Raw.Contains(filter.Substring(1), comparer))
                            include = false;
                    }
                    else if (!task.Raw.Contains(filter, comparer))
                        include = false;
                }

                if (include)
                    filteredTasks.Add(task);
            }
            return filteredTasks;
        }

        public void ModifySelectedTasks(Func<Task, dynamic, Task> function, dynamic parameter = null)
        {
            DisableFileChangeObserver();
            GetSelectedTasks();
            TaskManager.ReloadTasks();

            foreach(var task in selectedTasks)
            {
                Task newTask = function(task, parameter);
                TaskManager.UpdateTask(task, newTask);
                task.Raw = newTask.Raw;
            }

            UpdateDisplayedTasks();
            SetSelectedTasks();
            EnableFileChangeObserver();
        }

        private Task AppendTaskText(Task task, dynamic text = null)
        {
            return new Task(string.Concat(task.Raw, " ", text));
        }

        private Task SetTaskCompletion(Task task, dynamic parameter=null)
        {
            task.Completed = !task.Completed;
            var tmpTask = new Task(task.ToString());
            return tmpTask;
        }

        private void ArchiveCompleted()
        {
            if (!File.Exists(UserSettings.ArchiveFilePath))
                OptionsViewModel.Show();

            if (!File.Exists(UserSettings.ArchiveFilePath))
                return;

            GetSelectedTasks();

            DisableFileChangeObserver();

            var archiveList = new TaskList(UserSettings.ArchiveFilePath, UserSettings.PreserveWhiteSpaceAndBlankLines);
            var completed = TaskManager.Tasks.Where(t => t.Completed);

            TaskManager.ReloadTasks();

            foreach(var task in completed)
            {
                archiveList.Add(task);
                TaskManager.Delete(task);
            }

            UpdateDisplayedTasks();
            SetSelectedTasks();

            EnableFileChangeObserver();
        }

        private Task SetTaskPriority(Task task, dynamic priority)
        {
            Regex rgx = new Regex(@"^\((?<priority>[A-Z])\)\s"); // matches priority strings such as "(A) " (including trailing space)

            string oldTaskRawText = task.ToString();
            string oldPriority = rgx.Match(oldTaskRawText).Groups["priority"].Value.Trim();

            string newPriorityRaw = string.Format("({0}) ", priority);
            string newRawText = string.IsNullOrEmpty(oldPriority) ? newPriorityRaw + oldTaskRawText : rgx.Replace(oldTaskRawText, newPriorityRaw);

            return new Task(newRawText);
        }

        private Task IncreaseTaskPriority(Task task, dynamic parameter = null)
        {
            Task newTask = new Task(task.Raw);
            newTask.IncreasePriority();
            return newTask;
        }

        private Task DecreaseTaskPriority(Task task, dynamic parameter = null)
        {
            Task newTask = new Task(task.Raw);
            newTask.DecreasePriority();
            return newTask;
        }

        private Task RemovePriority(Task task, dynamic parameter = null)
        {
            Task newTask = new Task(task.Raw);
            newTask.SetPriority(' ');
            return newTask;
        }

        private Task SetTaskDueDate(Task task, dynamic newDueDate)
        {
            Regex rgx = new Regex(@"(?<=(^|\s)due:)(?<date>(\d{4})-(\d{2})-(\d{2}))");
            
            string oldTaskRawText = task.Raw;
            string oldDueDateText = rgx.Match(oldTaskRawText).Groups["date"].Value.Trim();

            string newTaskRawText = string.IsNullOrEmpty(oldDueDateText) ?
                oldTaskRawText + string.Format(" due:{0:yyyy-MM-dd}", newDueDate) :
                rgx.Replace(oldTaskRawText, ((DateTime)newDueDate).ToString("yyyy-MM-dd"));

            return new Task(newTaskRawText);
        }

        private Task PostponeTask(Task task, dynamic days)
        {
            DateTime oldDueDate = (task.DueDate.Length > 0) ? Convert.ToDateTime(task.DueDate) : DateTime.Today;

            DateTime newDueDate = oldDueDate.AddDays(days);

            string updateRaw = (task.DueDate.Length > 0) ?
                task.Raw.Replace(string.Format("due:{0}", task.DueDate), string.Format("due:{0:yyyy-MM-dd}", newDueDate)) :
                string.Format("{0} due:{1:yyyy-MM-dd}", task.Raw.ToString(), newDueDate);

            return new Task(updateRaw);
        }

        private Task RemoveTaskDueDate(Task task, dynamic parameter = null)
        {
            Regex rgx = new Regex(@"(?i:(^|\s)due:(\d{4})-(\d{2})-(\d{2}))*");
            Task newTask = new Task(rgx.Replace(task.Raw, "").TrimStart(' '));
            return newTask;
        }

        private void ApplyFilterPreset(int preset)
        {
            switch(preset)
            {
                case 0:
                    UserSettings.CurrentFilter = string.Empty;
                    break;
                case 1:
                    UserSettings.CurrentFilter = UserSettings.FilterPreset1;
                    break;
                case 2:
                    UserSettings.CurrentFilter = UserSettings.FilterPreset2;
                    break;
                case 3:
                    UserSettings.CurrentFilter = UserSettings.FilterPreset3;
                    break;
                case 4:
                    UserSettings.CurrentFilter = UserSettings.FilterPreset4;
                    break;
                case 5:
                    UserSettings.CurrentFilter = UserSettings.FilterPreset5;
                    break;
                case 6:
                    UserSettings.CurrentFilter = UserSettings.FilterPreset6;
                    break;
                case 7:
                    UserSettings.CurrentFilter = UserSettings.FilterPreset7;
                    break;
                case 8:
                    UserSettings.CurrentFilter = UserSettings.FilterPreset8;
                    break;
                case 9:
                    UserSettings.CurrentFilter = UserSettings.FilterPreset9;
                    break;
            }

            GetSelectedTasks();
            UpdateDisplayedTasks();
            SetSelectedTasks();
        }

        private string ShowAppendView()
        {
            AppendTextViewModel appendViewModel = new AppendTextViewModel();
            var result = windowManager.ShowDialog(appendViewModel);
            if (result.Value)
                return appendViewModel.AppendText.Trim();

            return string.Empty;
        }

        private bool ShowDeleteConfirmation(int? count = null)
        {
            if (!UserSettings.ConfirmBeforeDeletingTasks)
                return true;

            bool returnValue = false;

            TaskDialog td = new TaskDialog();
            td.InstructionText = "Permanentaly delete selected tasks?";
            td.Icon = TaskDialogStandardIcon.Information;
            td.FooterCheckBoxText = "Do not show this dialog again";
            td.Caption = "TODO.TXT - CONFIRM DELETION";
            td.Cancelable = true;

            if (count.HasValue)
                td.Text = string.Format("{0} task(s) will be deleted immediately. You cannot undo this action.", count.Value);

            TaskDialogCommandLink btnDelete = new TaskDialogCommandLink("btnDelete", "Yes, delete the selected tasks");
            btnDelete.Click += (o, e) =>
            {
                td.Close(TaskDialogResult.Yes);
            };
            TaskDialogCommandLink btnCancel = new TaskDialogCommandLink("btnCancel", "No, do not delete");
            btnCancel.Default = true;
            btnCancel.Click += (o, e) =>
            {
                td.Close(TaskDialogResult.No);
            };

            td.Controls.Add(btnDelete);
            td.Controls.Add(btnCancel);

            td.StartupLocation = TaskDialogStartupLocation.CenterOwner;
            td.OwnerWindowHandle = new WindowInteropHelper(window).Handle;

            if (td.Show() == TaskDialogResult.Yes)
                returnValue = true;

            if (td.FooterCheckBoxChecked.HasValue && td.FooterCheckBoxChecked.Value)
                UserSettings.ConfirmBeforeDeletingTasks = false;

            return returnValue;
        }

        private string ShowSetPriorityView()
        {
            // Get the default priority from the selected task to load into the Set Priority dialog
            Task selectedTask = (Task)window.lbTasks.SelectedItem;
            string selectedRawText = selectedTask.ToString();

            Regex rgx = new Regex(@"^\((?<priority>[A-Z])\)\s"); // matches priority strings such as "(A) " (including trailing space)
            //string selectedPriorityRaw = rgx.Match(selectedRawText).ToString(); // Priority letter plus parentheses and trailing space
            string selectedPriority = rgx.Match(selectedRawText).Groups["priority"].Value.Trim();

            SetPriorityViewModel setPriorityViewModel = new SetPriorityViewModel();
            setPriorityViewModel.Priority = string.IsNullOrEmpty(selectedPriority) ? "A" : selectedPriority;

            var result = windowManager.ShowDialog(setPriorityViewModel);
            if (result.HasValue && result.Value)
                return setPriorityViewModel.Priority;

            return string.Empty;
        }

        private DateTime? ShowSetDueDateView()
        {
            Task lastSelectedTask = (Task)window.lbTasks.SelectedItem;
            string oldTaskRawText = lastSelectedTask.ToString();

            Regex rgx = new Regex(@"(?<=\sdue:)(?<date>(\d{4})-(\d{2})-(\d{2}))");
            string oldDueDateText = rgx.Match(oldTaskRawText).Groups["date"].Value.Trim();

            SetDueDateViewModel setDueDateViewModel = new SetDueDateViewModel();
            setDueDateViewModel.DueDate = string.IsNullOrEmpty(oldDueDateText) ? DateTime.Today : DateTime.Parse(oldDueDateText); ;

            var result = windowManager.ShowDialog(setDueDateViewModel);
            if (result.HasValue && result.Value)
                return setDueDateViewModel.DueDate;

            return null;
        }

        private int ShowPostPoneView()
        {
            int iDays = 0;

            PostponeViewModel postponeViewModel = new PostponeViewModel();

            var result = windowManager.ShowDialog(postponeViewModel);
            if (result.HasValue && result.Value)
            {
                string sPostpone = postponeViewModel.PostponeText.Trim();

                sPostpone = sPostpone.ToLower();

                if (sPostpone == "monday" || sPostpone == "tuesday" || sPostpone == "wednesday" || sPostpone == "thursday"
                    || sPostpone == "friday" || sPostpone == "saturday" || sPostpone == "sunday" || sPostpone == "mon"
                    || sPostpone == "tue" || sPostpone == "wed" || sPostpone == "thur" || sPostpone == "fri"
                    || sPostpone == "sat" || sPostpone == "sun")
                {
                    DateTime due = DateTime.Now;
                    var count = 0;
                    bool isValid = false;

                    ModifySelectedTasks(SetTaskDueDate, DateTime.Today);

                    do
                    {
                        count++;
                        due = due.AddDays(1);

                        isValid = string.Equals(due.ToString("dddd", new CultureInfo("en-US")), sPostpone, StringComparison.CurrentCultureIgnoreCase)
                            || string.Equals(due.ToString("ddd", new CultureInfo("en-US")), sPostpone, StringComparison.CurrentCultureIgnoreCase);
                    } while (!isValid && count < 7);

                    return count;
                }

                if (sPostpone.Length > 0)
                {
                    try
                    {
                        iDays = Convert.ToInt32(sPostpone);
                    }
                    catch { }
                }
            }

            return iDays;
        }

        private bool ShowFilterView()
        {
            bool returnValue = false;

            FilterViewModel filterViewModel = new FilterViewModel(this.TaskManager);
            var result = windowManager.ShowDialog(filterViewModel);

            if (result.HasValue && result.Value)
                returnValue = result.Value;

            return returnValue;
        }

        

        private FlowDocument GetPrintContents()
        {
            FlowDocument document = new FlowDocument();

            document.FontFamily = UserSettings.FontFamily;

            var p = new Paragraph(new Run("TODO.TXT"));
            p.FontSize = 24;
            document.Blocks.Add(p);

            var t = new Table();
            document.Blocks.Add(t);
            t.CellSpacing = 0;

            t.FontSize = 12;

            for (int x = 0; x < 5; x++)
                t.Columns.Add(new TableColumn());

            t.Columns[0].Width = new GridLength(40);
            t.Columns[1].Width = new GridLength(90);
            t.Columns[2].Width = new GridLength(90);
            t.Columns[3].Width = new GridLength(90);

            t.RowGroups.Add(new TableRowGroup() { Name = "Header" });
            t.RowGroups[0].Rows.Add(new TableRow());

            var r = t.RowGroups[0].Rows[0];
            r.FontSize = 18;
            r.FontWeight = FontWeights.Bold;

            r.Cells.Add(new TableCell(new Paragraph(new Run(string.Empty))) { BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(3), BorderBrush = new SolidColorBrush(Colors.Black) });
            r.Cells.Add(new TableCell(new Paragraph(new Run("Done"))) { BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(3), BorderBrush = new SolidColorBrush(Colors.Black) });
            r.Cells.Add(new TableCell(new Paragraph(new Run("Created"))) { BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(3), BorderBrush = new SolidColorBrush(Colors.Black) });
            r.Cells.Add(new TableCell(new Paragraph(new Run("Due"))) { BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(3), BorderBrush = new SolidColorBrush(Colors.Black) });
            r.Cells.Add(new TableCell(new Paragraph(new Run("Details"))) { BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(3), BorderBrush = new SolidColorBrush(Colors.Black) });

            int currentTaskNumber = 0;
            nextGroupAtTaskNumber = 0;

            int currentGroupNumber = 0;
            int currentRowNumber = -1;

            if(collectionView.Groups.IsNullOrEmpty())
            {
                t.RowGroups.Add(new TableRowGroup() { Name = "Tasks" });
                currentGroupNumber++;
                currentRowNumber = 0;
            }

            foreach (Task task in window.lbTasks.Items)
            {
                if (UserSettings.AllowGroupingOfTasks)
                {
                    if (!collectionView.Groups.IsNullOrEmpty() && currentTaskNumber == nextGroupAtTaskNumber)
                    {
                        if (collectionViewGroups.IsNullOrEmpty())
                            collectionViewGroups = collectionView.Groups.Cast<CollectionViewGroup>().ToList();

                        List<GroupStyle> name = window.lbTasks.GroupStyle.ToList();

                        var groupName = GetGroupHeaderName();

                        t.RowGroups.Add(new TableRowGroup());
                        currentGroupNumber++;
                        currentRowNumber = -1;

                        if (!groupName.IsNullOrEmpty())
                        {
                            t.RowGroups[currentGroupNumber].Rows.Add(new TableRow());
                            currentRowNumber++;

                            r = t.RowGroups[currentGroupNumber].Rows[currentRowNumber];

                            r.FontSize = 14;
                            r.FontWeight = FontWeights.DemiBold;

                            r.Cells.Add(new TableCell(new Paragraph(new Run(groupName))));
                            r.Cells[0].ColumnSpan = 5;

                            r.Cells[0].Padding = new Thickness(3, 12, 3, 3);
                            r.Cells[0].BorderBrush = Brushes.Silver;
                            r.Cells[0].BorderThickness = new Thickness(0, 0, 0, .2);
                            r.Cells[0].TextAlignment = TextAlignment.Left;
                        }
                        nextGroupAtTaskNumber = countItemsInCurrentGroup + currentTaskNumber;
                    }
                }

                t.RowGroups[currentGroupNumber].Rows.Add(new TableRow());
                currentRowNumber++;

                r = t.RowGroups[currentGroupNumber].Rows[currentRowNumber];

                if (task.Completed)
                {
                    r.Cells.Add(new TableCell(new Paragraph(new Run("x"))));
                    r.Cells[0].FontWeight = FontWeights.Bold;
                    r.Cells[0].TextAlignment = TextAlignment.Center;
                    r.Cells.Add(new TableCell(new Paragraph(new Run(task.CompletedDate))));
                    r.Cells[1].FontStyle = FontStyles.Italic;
                    r.Cells[1].Foreground = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    r.Cells.Add(new TableCell(new Paragraph(new Run((task.Priority.IsNullOrEmpty() || task.Priority.ToUpper() == "N/A") ? string.Empty : task.Priority))));
                    r.Cells[0].FontWeight = FontWeights.Bold;
                    r.Cells[0].TextAlignment = TextAlignment.Center;

                    r.Cells.Add(new TableCell(new Paragraph(new Run(string.Empty))));
                }

                r.Cells.Add(new TableCell(new Paragraph(new Run((task.CreationDate.IsNullOrEmpty() || task.CreationDate.ToUpper() == "N/A") ? string.Empty : task.CreationDate))));
                r.Cells[2].FontStyle = FontStyles.Italic;
                r.Cells[2].Foreground = new SolidColorBrush(Colors.Red);

                r.Cells.Add(new TableCell(new Paragraph(new Run((task.DueDate.IsNullOrEmpty() || task.DueDate.ToUpper() == "N/A") ? string.Empty : task.DueDate))));
                r.Cells[3].FontStyle = FontStyles.Italic;
                r.Cells[3].Foreground = new SolidColorBrush(Colors.Blue);

                p = new Paragraph(new Run(task.Body) { TextDecorations = task.Completed ? TextDecorations.Strikethrough : null });

                task.Projects.ForEach(project => p.Inlines.Add(new Run() { Text = project, Foreground = Brushes.Red }));
                task.Contexts.ForEach(context => p.Inlines.Add(new Run() { Text = context, Foreground = Brushes.Green }));

                r.Cells.Add(new TableCell(p));

                if (currentRowNumber % 2 == 0)
                    r.Background = new SolidColorBrush(Color.FromRgb(239, 244, 255));

                foreach (var c in r.Cells)
                {
                    c.BorderBrush = new SolidColorBrush(Colors.Silver);
                    c.BorderThickness = new Thickness(0, 0, 0, 0);
                    c.Padding = new Thickness(3);
                }
                currentTaskNumber++;
            }

            p = new Paragraph(new Run(string.Format("Copyright © Matt Seemon 2016. Generated at {0}", DateTime.Now.ToString("yyyy-MM-dd hh:mm tt"))));
            p.FontSize = 10;
            p.BorderBrush = Brushes.Silver;
            p.BorderThickness = new Thickness(0, 1, 0, 0);
            p.Padding = new Thickness(0, 3, 0, 0);

            document.Blocks.Add(p);
            
            return document;
        }

        private string GetGroupHeaderName()
        {
            if(!collectionView.Groups.IsNullOrEmpty() && collectionView.GroupDescriptions != null && collectionView.Groups.Count > 0)
            {
                countItemsInCurrentGroup = collectionViewGroups[0].ItemCount;
                var groupName = collectionViewGroups[0].Name.ToString();

                collectionViewGroups.RemoveAt(0);
                return groupName;   
            }
            return string.Empty;
        }

        public void Dispose() { }
    }
}