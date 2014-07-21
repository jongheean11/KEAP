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
    /// FullWindowForPresentor.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FullWindowForPresentor : Window
    {
        FullWindowForAudience aud_View = null;
        private FullWindowForPresentor()
        {
            InitializeComponent();
        }

        public FullWindowForPresentor(FullWindowForAudience audience)
        {
            aud_View = audience;
            InitializeComponent();
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            aud_View.getDataFromKinect(message.Text);
        }
    }
}
