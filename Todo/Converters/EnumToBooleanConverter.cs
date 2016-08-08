using Seemon.Todo.Extensions;
using Seemon.Todo.Utilities;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Seemon.Todo.Converters
{
    class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string;

            if (parameterString.IsNullOrEmpty())
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object parameterValue = (SortType)Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string;

            if (parameterString.IsNullOrEmpty())
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
    }
}
