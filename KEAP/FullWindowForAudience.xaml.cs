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
        FullWindowForPresentor pre_View = null;
        MainWindow main_View = null;

        public FullWindowForAudience(MainWindow main, FullWindowForPresentor pre)
        {
            main_View = main;
            pre_View = pre;
            AddKeyBindings();

            InitializeComponent();
            
        }

        public FullWindowForAudience()
        {
            InitializeComponent();
        }

        private void Audience_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Maximized;
            Keyboard.Focus(this);

        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            pre_View = null;
            main_View = null;
            base.OnClosing(e);
        }

        private void AddKeyBindings()
        {
            // escape
            RoutedCommand key_Close = new RoutedCommand();
            key_Close.InputGestures.Add(new KeyGesture(Key.Escape));
            CommandBindings.Add(new CommandBinding(key_Close, Close_KeyEventHandler));
        }

        private void Close_KeyEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            if (main_View != null)
            {
                main_View.Close_SlideShow();
            }
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

        public void getRightGripFromKinect(string data)
        {
            grip_right.Text = data;
            //           sub_Monitor.Text = zzz;
        }

        public void getLeftGripFromKinect(string data)
        {
            grip_left.Text = data;
            //           sub_Monitor.Text = zzz;
        }
    }
}
