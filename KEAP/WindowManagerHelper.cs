namespace ImaginativeUniversal.Kinect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Drawing.Imaging;


    public static class WindowManagerHelper
    {

        static IntPtr applicationHandle;
        private static string PROCESS_NAME = "";

        public static string ProcessName
        {
            get { return PROCESS_NAME; }
            set { PROCESS_NAME = value; }
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private static void FindProcess(string processName)
        {
            var process = Process.GetProcessesByName(processName).FirstOrDefault();
            if (process != null && process.MainWindowHandle != IntPtr.Zero)
                applicationHandle = process.MainWindowHandle;
        }

        public static void SendKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            if (key.ToUpper().Contains("PRTSCN"))
            {
                PrintScreen();
                return;
            }

            FindProcess(PROCESS_NAME);
            SetForegroundWindow(applicationHandle);

            System.Windows.Forms.SendKeys.SendWait(key);
        }

        public static void PrintScreen()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(printscreen as Image);
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            string fullpath = string.Format(@"{0}\{1}.jpg", path, DateTime.Now.ToString("Hmmssft"));
            printscreen.Save(fullpath, ImageFormat.Jpeg);

        }
    }
}

