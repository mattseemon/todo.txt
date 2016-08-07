using Caliburn.Micro.ReactiveUI;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seemon.Todo.ViewModels
{
    public class SetDueDateViewModel : ReactiveScreen, IEnableLogger
    {
        private DateTime dueDate = DateTime.Now;
        public DateTime DueDate
        {
            get { return this.dueDate; }
            set { this.RaiseAndSetIfChanged(ref this.dueDate, value); }
        }

        public ReactiveCommand<object> OKCommand { get; private set; }
        public ReactiveCommand<object> CancelCommand { get; private set; }

        public SetDueDateViewModel()
        {
            this.DisplayName = "SET DUE DATE";

            this.OKCommand = ReactiveCommand.Create(this.WhenAny(x => x.DueDate, (o) => (o.Value.Date >= DateTime.Now.Date)));
            this.OKCommand.Subscribe(x => this.OnOK());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(x => this.OnCancel());
        }

        public void OnOK()
        {
            this.TryClose(true);
        }

        public void OnCancel()
        {
            this.TryClose(false);
        }
    }
}
