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
//            WindowState = WindowSettings.current_WindowState;
            LoadPresentaions();
            //그냥 최대로 했음.
            //this.Width = WindowSettings.resolution_Width;
            //this.Height = WindowSettings.resolution_Height;
        }

        private void LoadPresentaions()
        {
            List<PresentationListItem> pItems = new List<PresentationListItem>();

            //TODO : add pItems read from presentation files
            pItems.Add(new PresentationListItem() { presentationSmall = "Images/Button/newslide.png", name = "test", date = "test" });
            recentPresentationsList.ItemsSource = pItems;
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            //save_Settings();
            //MainWindow main = new MainWindow();
            ////App.Current.MainWindow = main;
            //main.Show();
            //FileSettings.file_Path = null;
            //this.Hide();
            this.Close();
        }

        private void Click(object sender, RoutedEventArgs e)
        {
            save_Settings();
            MainWindow main = new MainWindow();
            //App.Current.MainWindow = main;
            main.Show();
            FileSettings.file_Path = null;
            this.Hide();
        }

        void save_Settings()
        {
            WindowSettings.current_Width = Width;
            WindowSettings.current_Height = Height;
            WindowSettings.current_WindowState = WindowState;
        }

        private void recentPresentations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void recentPresentationsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // TODO : open selected file
        }
    }
}
