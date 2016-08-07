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
    /// Interaction logic for SetDueDateView.xaml
    /// </summary>
    public partial class SetDueDateView : Window
    {
        public SetDueDateView()
        {
            InitializeComponent();
            dpDueDate.Focus();
        }

        private void OnDatePickerKeyDown(object sender, KeyEventArgs e)
        {
            var dp = sender as DatePicker;

            if (dp == null) return;

            if(e.Key == Key.T)
            {
                e.Handled = true;
                dp.SetValue(DatePicker.SelectedDateProperty, DateTime.Today);
                return;
            }

            if (!dp.SelectedDate.HasValue) return;

            var date = dp.SelectedDate.Value;
            if (e.Key == Key.Up)
            {
                e.Handled = true;
                dp.SetValue(DatePicker.SelectedDateProperty, date.AddDays(1));
                return;
            }

            if (e.Key == Key.Down)
            {
                e.Handled = true;
                dp.SetValue(DatePicker.SelectedDateProperty, date.AddDays(-1));
                return;
            }
        }
    }
}
