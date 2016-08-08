using Caliburn.Micro.ReactiveUI;
using ReactiveUI;
using Seemon.Todo.Views;
using Splat;
using System;
using System.Windows;

namespace Seemon.Todo.ViewModels
{
    public class AppendTextViewModel : ReactiveScreen, IEnableLogger
    {
        private AppendTextView window;
        private string appendText = string.Empty;

        public string AppendText
        {
            get { return this.appendText; }
            set { this.RaiseAndSetIfChanged(ref this.appendText, value); }
        }

        public ReactiveCommand<object> OKCommand { get; private set; }
        public ReactiveCommand<object> CancelCommand { get; private set; }

        public AppendTextViewModel()
        {
            this.DisplayName = "APPEND TEXT";

            this.OKCommand = ReactiveCommand.Create(this.WhenAny(x => x.AppendText, (o) => o.Value.Trim().Length > 0));
            this.OKCommand.Subscribe(x => this.OnOK());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(x => this.OnCancel());
        }

        protected override void OnViewReady(object view)
        {
            window = (AppendTextView)view;
            base.OnViewReady(view);
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
