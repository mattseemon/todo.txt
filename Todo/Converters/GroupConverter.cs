using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Seemon.Todo.Converters
{
    internal class GroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strings = value as List<string>;

            if ((strings != null && strings.Count == 0) || (value as string == ""))
                return "n/a";

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}