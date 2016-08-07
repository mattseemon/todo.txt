using Caliburn.Micro.ReactiveUI;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Seemon.Todo.Extensions;
using System.Text.RegularExpressions;

namespace Seemon.Todo.ViewModels
{
    public class SetPriorityViewModel : ReactiveScreen, IEnableLogger
    {
        private readonly string[] priorityValues = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        public string[] PriorityValues
        {
            get { return this.priorityValues; }
        }


        private string priority = string.Empty;
        public string Priority
        {
            get { return this.priority.ToUpper(); }
            set { this.RaiseAndSetIfChanged(ref this.priority, value); }
        }

        public ReactiveCommand<object> OKCommand { get; private set; }
        public ReactiveCommand<object> CancelCommand { get; private set; }
        public ReactiveCommand<object> IncreasePriorityCommand { get; private set; }
        public ReactiveCommand<object> DecreasePriorityCommand { get; private set; }

        public SetPriorityViewModel()
        {
            this.DisplayName = "SET PRIORITY";

            this.OKCommand = ReactiveCommand.Create(this.WhenAny(x => x.Priority, (o) => o.Value.Trim().Length > 0));
            this.OKCommand.Subscribe(x => this.OnOK());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(x => this.OnCancel());

            this.IncreasePriorityCommand = ReactiveCommand.Create();
            this.IncreasePriorityCommand.Subscribe(x => this.OnIncreasePriority());

            this.DecreasePriorityCommand = ReactiveCommand.Create();
            this.DecreasePriorityCommand.Subscribe(x => this.OnDecreasePriority());
        }

        public void OnOK()
        {
            this.TryClose(true);
        }

        public void OnCancel()
        {
            this.TryClose(false);
        }

        public void OnIncreasePriority()
        {
            if(this.Priority.IsNullOrEmpty())
            {
                this.Priority = "A";
                return;
            }

            Regex rgx = new Regex("[A-Z]");
            if(rgx.IsMatch(this.Priority) && this.Priority[0] != 'A')
            {
                char newPriority = (char)((int)this.Priority[0] - 1);
                this.Priority = newPriority.ToString();
            }
        }

        public void OnDecreasePriority()
        {
            if(this.Priority.IsNullOrEmpty())
            {
                this.Priority = "A";
                return;
            }

            Regex rgx = new Regex("[A-Z]");
            if(rgx.IsMatch(this.Priority) && this.Priority[0] != 'Z')
            {
                char newPriority = (char)((int)this.Priority[0] + 1);
                this.Priority = newPriority.ToString();
            }
        }
    }
}
