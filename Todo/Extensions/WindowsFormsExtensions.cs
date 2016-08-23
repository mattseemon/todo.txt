using System.Drawing;
using System.Windows;

namespace Seemon.Todo.Extensions
{
    public static class WindowsFormsExtensions
    {
        public static Rectangle ToRectangle(this Rect rect)
        {
            return new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }
    }
}