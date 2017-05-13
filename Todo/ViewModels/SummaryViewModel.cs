using Caliburn.Micro.ReactiveUI;
using ReactiveUI;
using Seemon.Todo.Models;
using Seemon.Todo.Views;
using Splat;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;

namespace Seemon.Todo.ViewModels
{
    public class SummaryViewModel : ReactiveScreen, IEnableLogger
    {
           
        public enum SummaryViews
        {
            All,
            Filtered
        }

        private SummaryViews currentView = SummaryViewModel.SummaryViews.Filtered;
        private SummaryView window = null;

        private int totalTasks = 0;
        private int totalProjects = 0;
        private int totalContexts = 0;
        private int totalPriorities = 0;
        private int incomplete = 0;
        private int dueToday = 0;
        private int overdue = 0;
        private int notDue = 0;
        private int noDueDate = 0;
        private int completed = 0;

        private List<string> projects;
        private List<string> contexts;

        public ReactiveCommand<object> UpdateSummaryCommand { get; private set; }

        private List<Task> allTasks = null;
        private List<Task> sortedTasks = null;

        public SummaryViewModel(List<Task> all, List<Task> sorted)
        {
            this.DisplayName = "TASK LIST SUMMARY";

            this.allTasks = all;
            this.sortedTasks = sorted;

            this.UpdateSummaryCommand = ReactiveCommand.Create();
            this.UpdateSummaryCommand.Subscribe(x => this.OnUpdateSummary(x));

            this.UpdateSummaryCommand.Execute(SummaryViews.Filtered);
        }

        public SummaryViews CurrentView
        {
            get { return this.currentView; }
            set { this.RaiseAndSetIfChanged(ref this.currentView, value); }
        }

        public int TotalTasks
        {
            get { return this.totalTasks; }
            set { this.RaiseAndSetIfChanged(ref this.totalTasks, value); }
        }

        public int TotalProjects
        {
            get { return this.totalProjects; }
            set { this.RaiseAndSetIfChanged(ref this.totalProjects, value); }
        }

        public int TotalContexts
        {
            get { return this.totalContexts; }
            set { this.RaiseAndSetIfChanged(ref this.totalContexts, value); }
        }

        public int TotalPriorities
        {
            get { return this.totalPriorities; }
            set { this.RaiseAndSetIfChanged(ref this.totalPriorities, value); }
        }

        public int Incomplete
        {
            get { return this.incomplete; }
            set { this.RaiseAndSetIfChanged(ref this.incomplete, value); }
        }

        public int DueToday
        {
            get { return this.dueToday; }
            set { this.RaiseAndSetIfChanged(ref this.dueToday, value); }
        }

        public int Overdue
        {
            get { return this.overdue; }
            set { this.RaiseAndSetIfChanged(ref this.overdue, value); }
        }

        public int NotDue
        {
            get { return this.notDue; }
            set { this.RaiseAndSetIfChanged(ref this.notDue, value); }
        }

        public int NoDueDate
        {
            get { return this.noDueDate; }
            set { this.RaiseAndSetIfChanged(ref this.noDueDate, value); }
        }

        public int Completed
        {
            get { return this.completed; }
            set { this.RaiseAndSetIfChanged(ref this.completed, value); }
        }

        public List<string> Projects
        {
            get { return this.projects; }
            set
            {
                this.projects = value;
                this.RaisePropertyChanged("Projects");
            }
        }

        public List<string> Contexts
        {
            get { return this.contexts; }
            set
            {
                this.contexts = value;
                this.RaisePropertyChanged("Contexts");
            }
        }

        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);
            this.window = (SummaryView)view;
        }

        public void OnUpdateSummary(object x)
        {
            if (x != null)
            {
                this.CurrentView = (SummaryViews)Enum.Parse(typeof(SummaryViews), x.ToString());
            }
            var list = this.CurrentView == SummaryViews.Filtered ? sortedTasks : allTasks;

            this.TotalTasks = list.Count;

            int incompleteTask = 0, dueTodayTask = 0, overDueTask = 0, completed = 0, noDueDate = 0, notDue = 0;

            projects = new List<string>();
            contexts = new List<string>();

            var priorities = new List<string>();

            foreach (Task t in list)
            {
                if (t.Completed)
                    completed++;

                if (!t.Completed)
                {
                    incompleteTask++;

                    if (!string.IsNullOrEmpty(t.DueDate))
                    {
                        DateTime dueDate;
                        if (DateTime.TryParseExact(t.DueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dueDate))
                        {
                            if (dueDate.Date == DateTime.Today.Date)
                                dueTodayTask++;
                            else if (dueDate.Date < DateTime.Today)
                                overDueTask++;
                            else if (dueDate.Date > DateTime.Today)
                                notDue++;
                        }
                    }
                    else
                        noDueDate++;

                    if (!string.IsNullOrEmpty(t.Priority))
                        if (!priorities.Contains(t.Priority))
                            priorities.Add(t.Priority);
                }

                projects = projects.Union(t.Projects).ToList();
                contexts = contexts.Union(t.Contexts).ToList();
            }

            this.Incomplete = incompleteTask;
            this.Overdue = overDueTask;
            this.DueToday = dueTodayTask;
            this.Completed = completed;
            this.NoDueDate = noDueDate;
            this.NotDue = notDue;
            this.TotalProjects = projects.Count;
            this.TotalContexts = contexts.Count;
            this.TotalPriorities = priorities.Count;
            this.Projects = projects;
            this.Contexts = contexts;
            
        }
    }
}
