using Caliburn.Micro.ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seemon.Todo.ViewModels
{
    public class CrashViewModel : ReactiveScreen
    {
        public CrashViewModel(Exception ex)
        {
            this.DisplayName = "TODO.TXT CRASHED";

            this.CrashReportContent = ex.ToString();
        }

        public string CrashReportContent { get; private set; }
    }
}
