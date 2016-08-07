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
    public class PostponeViewModel : ReactiveScreen, IEnableLogger
    {
        private string postponeText = string.Empty;
        public string PostponeText
        {
            get { return this.postponeText; }
            set { this.RaiseAndSetIfChanged(ref this.postponeText, value); }
        }

        public ReactiveCommand<object> OKCommand { get; private set; }
        public ReactiveCommand<object> CancelCommand { get; private set; }

        public PostponeViewModel()
        {
            this.DisplayName = "POSTPONE";

            this.OKCommand = ReactiveCommand.Create(this.WhenAny(x => x.PostponeText, (o) => o.Value.Trim().Length > 0));
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
