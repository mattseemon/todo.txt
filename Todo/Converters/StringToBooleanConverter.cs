using Seemon.Todo.Extensions;
using Seemon.Todo.Utilities;
using Splat;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Seemon.Todo.Converters
{
    class StringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var userSettings = Locator.Current.GetService<UserSettings>();
            string parameterString = parameter as string;

            if (parameterString.IsNullOrEmpty())
                return DependencyProperty.UnsetValue;

            var selectedPreset = string.Empty;

            if (userSettings.CurrentFilter.IsNullOrEmpty())
                return false;

            if (userSettings.CurrentFilter == userSettings.FilterPreset1)
                selectedPreset = "Preset1";
            else if (userSettings.CurrentFilter == userSettings.FilterPreset2)
                selectedPreset = "Preset2";
            else if (userSettings.CurrentFilter == userSettings.FilterPreset3)
                selectedPreset = "Preset3";
            else if (userSettings.CurrentFilter == userSettings.FilterPreset4)
                selectedPreset = "Preset4";
            else if (userSettings.CurrentFilter == userSettings.FilterPreset5)
                selectedPreset = "Preset5";
            else if (userSettings.CurrentFilter == userSettings.FilterPreset6)
                selectedPreset = "Preset6";
            else if (userSettings.CurrentFilter == userSettings.FilterPreset7)
                selectedPreset = "Preset7";
            else if (userSettings.CurrentFilter == userSettings.FilterPreset8)
                selectedPreset = "Preset8";
            else if (userSettings.CurrentFilter == userSettings.FilterPreset9)
                selectedPreset = "Preset9";

            return parameterString.Equals(selectedPreset);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
