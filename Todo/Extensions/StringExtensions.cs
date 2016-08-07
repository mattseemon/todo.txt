using System;
using System.Text.RegularExpressions;

namespace Seemon.Todo.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static bool IsDateGreaterThan(this string dateString, DateTime date)
        {
            if (dateString.IsNullOrEmpty())
                return false;

            DateTime comparisonDate;
            if (!DateTime.TryParse(dateString, out comparisonDate))
                return false;

            return comparisonDate.Date > date.Date;
        }

        public static bool IsDateLessThan(this string dateString, DateTime date)
        {
            if (dateString.IsNullOrEmpty())
                return false;

            DateTime comparisonDate;
            if (!DateTime.TryParse(dateString, out comparisonDate))
                return false;

            return comparisonDate.Date < date.Date;
        }

        public static string TrimSpaces(this string value)
        {
            if (value.IsNullOrEmpty())
                return string.Empty;

            Regex regEx = new Regex("[ ]{2,}", RegexOptions.None);
            value = regEx.Replace(value, " ");

            return value.Trim();
        }
    }
}
