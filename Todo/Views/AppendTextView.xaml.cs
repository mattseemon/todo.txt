using Seemon.Todo.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Seemon.Todo.Views
{
    /// <summary>
    /// Interaction logic for AppendTextView.xaml
    /// </summary>
    public partial class AppendTextView : Window
    {
        public AppendTextView()
        {
            InitializeComponent();
            
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
            base.OnSourceInitialized(e);
        }

        private void OnWindowActivated(object sender, EventArgs e)
        {
            txtAppendText.Focus();
        }
    }
}
