using Seemon.Todo.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Seemon.Todo.Models
{
    public enum Due
    {
        NotDue,
        Today,
        Overdue
    }

    public class Task : IComparable
    {
        private const string CompletedPattern = @"^X\s((\d{4})-(\d{2})-(\d{2}))?";
        private const string PriorityPattern = @"^(?<priority>\([A-Z]\)\s)";
        private const string CreatedDatePattern = @"(?<date>(\d{4})-(\d{2})-(\d{2}))";
        private const string DueRelativePattern = @"due:(?<dateRelative>today|tomorrow|monday|tuesday|wednesday|thursday|friday|saturday|sunday)";
        private const string DueDatePattern = @"due:(?<date>(\d{4})-(\d{2})-(\d{2}))";
        private const string ProjectPattern = @"(?<proj>(?<=^|\s)\+[^\s]+)";
        private const string ContextPattern = @"(^|\s)(?<context>\@[^\s]+)";

        private bool completed;

        public List<string> Projects { get; set; }
        public string PrimaryProject { get; private set; }
        public List<string> Contexts { get; set; }
        public string PrimaryContext { get; private set; }
        public string DueDate { get; set; }
        public string CompletedDate { get; set; }
        public string CreationDate { get; set; }
        public string Priority { get; set; }
        public string Body { get; set; }
        public string Raw { get; set; }

        public bool Completed
        {
            get { return this.completed; }
            set
            {
                //this.RaiseAndSetIfChanged(ref this.completed, value);
                this.completed = value;
                if (this.completed)
                {
                    this.CompletedDate = DateTime.Now.ToString("yyyy-MM-dd");
                    this.Priority = string.Empty;
                }
                else
                    this.CompletedDate = string.Empty;
            }
        }

        public Due IsTaskDue
        {
            get
            {
                if (this.Completed)
                    return Due.NotDue;

                var tmp = new DateTime();

                if (!DateTime.TryParse(this.DueDate, out tmp))
                    return Due.NotDue;

                if (tmp < DateTime.Today)
                    return Due.Overdue;

                if (tmp == DateTime.Today)
                    return Due.Today;

                return Due.NotDue;
            }
        }

        public Task(string raw)
        {
            raw = raw.Replace(Environment.NewLine, "");

            var reg = new Regex(DueRelativePattern, RegexOptions.IgnoreCase);
            var dueDateRelative = reg.Match(raw).Groups["dateRelative"].Value.Trim();

            if (!dueDateRelative.IsNullOrEmpty())
            {
                var isValid = false;

                var due = new DateTime();
                dueDateRelative = dueDateRelative.ToLower();

                switch (dueDateRelative)
                {
                    case "today":
                        due = DateTime.Today;
                        isValid = true;
                        break;
                    case "tomorrow":
                        due = DateTime.Now.AddDays(1);
                        isValid = true;
                        break;
                    case "monday":
                    case "tuesday":
                    case "wednesday":
                    case "thursday":
                    case "friday":
                    case "saturday":
                    case "sunday":
                        due = DateTime.Now;
                        var count = 0;

                        do
                        {
                            count++;
                            due = due.AddDays(1);
                            isValid = string.Equals(due.ToString("dddd", new CultureInfo("en-US")), dueDateRelative, StringComparison.CurrentCultureIgnoreCase);
                        } while (!isValid && (count < 7));
                        break;
                }

                if (isValid)
                    raw = reg.Replace(raw, "due:" + due.ToString("yyyy-MM-dd"));
            }

            this.Raw = raw;

            reg = new Regex(CompletedPattern, RegexOptions.IgnoreCase);
            var s = reg.Match(raw).Value.Trim();

            if (string.IsNullOrEmpty(s))
            {
                this.Completed = false;
                this.CompletedDate = string.Empty;
            }
            else
            {
                this.Completed = true;
                if (s.Length > 1)
                    this.CompletedDate = s.Substring(2);
            }

            raw = reg.Replace(raw, "");

            reg = new Regex(PriorityPattern, RegexOptions.IgnoreCase);
            this.Priority = reg.Match(raw).Groups["priority"].Value.Trim();
            raw = reg.Replace(raw, "");

            reg = new Regex(DueDatePattern);
            this.DueDate = reg.Match(raw).Groups["date"].Value.Trim();
            raw = reg.Replace(raw, "");

            reg = new Regex(CreatedDatePattern);
            this.CreationDate = reg.Match(raw).Groups["date"].Value.Trim();
            raw = reg.Replace(raw, "");

            var projectSet = new SortedSet<string>();
            reg = new Regex(ProjectPattern);
            var projects = reg.Matches(raw);
            this.PrimaryProject = string.Empty;
            int i = 0;

            foreach (Match project in projects)
            {
                var p = project.Groups["proj"].Value.Trim();
                projectSet.Add(p);
                if (i == 0)
                    this.PrimaryProject = p;
                i++;
            }
            this.Projects = projectSet.ToList<string>();
            raw = reg.Replace(raw, "");

            var contextSet = new SortedSet<string>();
            reg = new Regex(ContextPattern);
            var contexts = reg.Matches(raw);
            this.PrimaryContext = string.Empty;
            i = 0;

            foreach (Match context in contexts)
            {
                var c = context.Groups["context"].Value.Trim();
                contextSet.Add(c);
                if (i == 0)
                    this.PrimaryContext = c;
                i++;
            }
            this.Contexts = contextSet.ToList<string>();
            raw = reg.Replace(raw, "");

            this.Body = raw.TrimSpaces();

            this.Raw = this.Raw.TrimSpaces();
        }

        public Task(string priority, List<string> projects, List<string> contexts, string body, string dueDate = "", bool completed = false)
        {
            this.Priority = priority;
            this.Projects = projects;
            this.Contexts = contexts;
            this.DueDate = dueDate;
            this.Body = body;
            this.Completed = completed;
        }

        public int CompareTo(object obj)
        {
            var other = (Task)obj;

            return string.Compare(this.Raw, other.Raw);
        }

        public override string ToString()
        {
            var str = string.Empty;

            if (!string.IsNullOrEmpty(this.Raw))
            {
                var reg = new Regex(CompletedPattern, RegexOptions.IgnoreCase);
                var rawCompleted = reg.IsMatch(this.Raw);

                str = this.Raw;

                if (rawCompleted != this.Completed)
                {
                    if (this.Completed)
                    {
                        str = Regex.Replace(this.Raw, PriorityPattern, "");
                        str = "x " + this.CompletedDate + " " + str;
                    }
                    else
                    {
                        str = reg.Replace(this.Raw, "").Trim();
                    }
                }
            }
            else
            {
                str = string.Format("{0}{1}{2} {3} {4}",
                    this.Completed ? "x " + CompletedDate + " " : "",
                    this.Priority == null ? "N/A" : this.Priority + " ",
                    this.Body,
                    string.Join(" ", this.Projects),
                    string.Join(" ", this.Contexts));
            }

            return str.Trim();
        }

        public void IncreasePriority()
        {
            this.ChangePriority(-1);
        }

        public void DecreasePriority()
        {
            this.ChangePriority(1);
        }

        private void ChangePriority(int asciiShift)
        {
            if (this.Priority.IsNullOrEmpty())
                this.SetPriority('A');
            else
            {
                var current = this.Priority[1];

                var newPriority = (char)((current) + asciiShift);

                if (Char.IsLetter(newPriority))
                    this.SetPriority(newPriority);
            }
        }

        public void SetPriority(char priority)
        {
            var priorityString = char.IsLetter(priority) ? new string(new[] { '(', priority, ')' }) : "";

            if(!this.Raw.IsNullOrEmpty())
            {
                if (this.Priority.IsNullOrEmpty())
                    this.Raw = priorityString + " " + this.Raw;
                else
                    this.Raw = this.Raw.Replace(this.Priority, priorityString);
            }

            this.Raw = this.Raw.Trim();
            this.Priority = priorityString;
        }
    }
}
