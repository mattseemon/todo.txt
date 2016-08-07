using System.Windows;

namespace Seemon.Todo.Utilities
{
    public class ToolTipExtended : DependencyObject
    {
        public static readonly DependencyProperty ToolTipHeaderProperty = DependencyProperty.RegisterAttached("ToolTipHeader", typeof(string), typeof(ToolTipExtended), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty ToolTipContentProperty = DependencyProperty.RegisterAttached("ToolTipContent", typeof(string), typeof(ToolTipExtended), new PropertyMetadata(string.Empty));

        public static string GetToolTipHeader(DependencyObject d)
        {
            return (string)d.GetValue(ToolTipHeaderProperty);
        }

        public static void SetToolTipHeader(DependencyObject d, string value)
        {
            d.SetValue(ToolTipHeaderProperty, value);
        }

        public static string GetToolTipContent(DependencyObject d)
        {
            return (string)d.GetValue(ToolTipContentProperty);
        }

        public static void SetToolTipContent(DependencyObject d, string value)
        {
            d.SetValue(ToolTipContentProperty, value);
        }
    }
}
