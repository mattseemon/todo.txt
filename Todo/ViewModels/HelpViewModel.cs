using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seemon.Todo.ViewModels
{
    public class HelpViewModel : ReactiveScreen, IEnableLogger
    {
        private IWindowManager windowManager;

        public HelpViewModel()
        {
            this.Log().Info("Initialize Help Dialog");
            this.windowManager = Locator.Current.GetService<IWindowManager>();
            this.DisplayName = "HELP"; 
        }

        public void Show()
        {
            this.windowManager.ShowWindow(this);
        }
    }
}
