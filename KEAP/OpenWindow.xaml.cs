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
    /// Interaction logic for OpenWindow.xaml
    /// </summary>
    public partial class OpenWindow : Window
    {
        public OpenWindow()
        {
            InitializeComponent();
            WindowState = WindowSettings.current_WindowState;
            
            //그냥 최대로 했음.
            this.Width = WindowSettings.resolution_Width;
            this.Height = WindowSettings.resolution_Height;
        }

        private void Click(object sender, RoutedEventArgs e)
        {
            save_Settings();
            
            //App.Current.MainWindow = main;
            FileSettings.file_Path = "C://KEAP";
            FileSettings.file_Name = "NewFile" + Convert.ToString(FileSettings.count);
            FileSettings.count++;

            MainWindow main = new MainWindow();
            main.Show();
            this.Hide();
        }

        void save_Settings()
        {
            WindowSettings.current_Width = Width;
            WindowSettings.current_Height = Height;
            WindowSettings.current_WindowState = WindowState;
        }
    }
}
