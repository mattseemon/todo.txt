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
    /// Interaction logic for FilterView.xaml
    /// </summary>
    public partial class FilterView : Window
    {
        public FilterView()
        {
            InitializeComponent();
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.CaretIndex = textBox.Text.Length; 
        }
    }
}
