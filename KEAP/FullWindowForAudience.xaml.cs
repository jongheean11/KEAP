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

namespace KEAP
{
    /// <summary>
    /// FullWindowForAudience.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FullWindowForAudience : Window
    {
        public FullWindowForAudience()
        {
            InitializeComponent();
        }

        public FullWindowForAudience(bool is_Dual_Monitor)
        {
            InitializeComponent();
            if (is_Dual_Monitor)
            {
                // create monitor view for presentor

                FullWindowForPresentor Presentor = new FullWindowForPresentor(this);
                Presentor.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                System.Drawing.Rectangle working_Area = System.Windows.Forms.Screen.AllScreens[1].WorkingArea;
                Presentor.Left = working_Area.Left;
                Presentor.Top = working_Area.Top;
                Presentor.Width = working_Area.Width;
                Presentor.Height = working_Area.Height;
                Presentor.WindowState = WindowState.Maximized;
                Presentor.WindowStyle = WindowStyle.None;
                Presentor.Topmost = true;
                Presentor.Show();

            }
            else
            {
                // nothing....
                FullWindowForPresentor Presentor = new FullWindowForPresentor(this);
                Presentor.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                System.Drawing.Rectangle working_Area = System.Windows.Forms.Screen.AllScreens[0].WorkingArea;
                Presentor.Left = working_Area.Left;
                Presentor.Top = working_Area.Top;
                Presentor.Width = working_Area.Width / 2;
                Presentor.Height = working_Area.Height / 2;
                Presentor.WindowStyle = WindowStyle.None;
                Presentor.Topmost = true;
                Presentor.Show();


            }
            InitializeComponent();
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            //            ptr.SendMsgToMain(message.Text);
        }

        public void getDataFromKinect(string data)
        {
            sub_Monitor.Text = data;
            //           sub_Monitor.Text = zzz;
        }
    }
}
