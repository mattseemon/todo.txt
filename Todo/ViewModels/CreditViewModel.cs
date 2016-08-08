using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Seemon.Todo.ViewModels
{
    public class CreditViewModel : ReactiveScreen, IEnableLogger
    {
        IWindowManager windowManager = null;

        public CreditViewModel()
        {
            this.Log().Info("Initialize Credits Dialog");
            this.DisplayName = "CREDITS";
            this.windowManager = Locator.Current.GetService<IWindowManager>();
        }

        public void Show()
        {
            this.windowManager.ShowDialog(this);
        }

        
    }
}
