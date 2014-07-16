using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KEAP
{
    class WindowSettings
    {
        public static double resolution_Width = SystemParameters.MaximizedPrimaryScreenWidth;
        public static double resolution_Height = SystemParameters.MaximizedPrimaryScreenHeight;
        public static double current_Width;
        public static double current_Height;
        public static WindowState current_WindowState = WindowState.Maximized;
    }
}
