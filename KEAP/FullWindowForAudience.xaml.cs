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

        private void Audience_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Maximized;
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
