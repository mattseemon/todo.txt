using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for CreditView.xaml
    /// </summary>
    public partial class CreditView : Window
    {
        public CreditView()
        {
            InitializeComponent();
        }

        protected void OnClick(object sender, RoutedEventArgs e)
        {
            var link = sender as Hyperlink;
            Process.Start(link.NavigateUri.ToString());
        }
    }
}
