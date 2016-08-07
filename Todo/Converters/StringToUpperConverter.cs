using System;
using System.Globalization;
using System.Windows.Data;

namespace Seemon.Todo.Converters
{
    class StringToUpperConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null) // && value.GetType().Equals(typeof(string)))
                return value.ToString().ToUpper();

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
