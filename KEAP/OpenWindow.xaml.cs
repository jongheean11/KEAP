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
        PresentationListItem toOpenPresentationInfo = null;
        TemplateListItem toOpenTemplateInfo = 
                new TemplateListItem(
                    "기본 바탕",
                    "background_01.jpg",
                    Colors.Black,
                    Colors.White,
                    Colors.Black);
        public OpenWindow()
        {
            InitializeComponent();
//            WindowState = WindowSettings.current_WindowState;
            LoadPresentaions();
            LoadTemplates();
            openBtn.IsEnabled = false;

            //그냥 최대로 했음.
            //this.Width = WindowSettings.resolution_Width;
            //this.Height = WindowSettings.resolution_Height;
        }

        private void LoadPresentaions()
        {
            List<PresentationListItem> pItems = new List<PresentationListItem>();
            //TODO : add pItems read from presentation files

            //test
            PresentationListItem test1 = new PresentationListItem("Images/Button/newslide.png", "test", "date", "filepath");
            pItems.Add(test1);
            //test fin
            recentPresentationsList.ItemsSource = pItems;
        }
        #region initial template list
        private void LoadTemplates()
        {
            List<TemplateListItem> tItems = new List<TemplateListItem>();

            //test
            //TemplateListItem test1 = new TemplateListItem(new Rectangle(), new SolidColorBrush(Color.FromRgb(255, 0, 0)), "test");

            TemplateListItem template1 = 
                new TemplateListItem(
                    "기본 바탕",
                    "background_01.jpg",
                    Colors.Black,
                    Colors.White,
                    Colors.Black);
            tItems.Add(template1);

            TemplateListItem template2 =
                new TemplateListItem(
                    "검은 바탕",
                    "background_02.jpg",
                    Colors.White,
                    Colors.Black,
                    Colors.White);
            tItems.Add(template2);

            TemplateListItem template3 =
                new TemplateListItem(
                    "비온 후",
                    "background_03.jpg",
                    Colors.Black,
                    Colors.White,
                    Colors.Black);
            tItems.Add(template3);

            TemplateListItem template4 =
                new TemplateListItem(
                    "하얀 결",
                    "background_04.jpg",
                    Colors.Black,
                    Colors.White,
                    Colors.Black);
            tItems.Add(template4);

            TemplateListItem template5 =
                new TemplateListItem(
                    "숲 속 칠판",
                    "background_05.jpg",
                    Colors.Black,
                    Colors.White,
                    Colors.Black);
            tItems.Add(template5);

            TemplateListItem template6 =
                new TemplateListItem(
                    "잔디",
                    "background_06.jpg",
                    Colors.Black,
                    Colors.White,
                    Colors.Black);
            tItems.Add(template6);


            TemplateListItem template7 =
                new TemplateListItem(
                    "테두리",
                    "background_07.jpg",
                    Colors.Black,
                    Colors.White,
                    Colors.Black);
            tItems.Add(template7);

            TemplateListItem template8 =
                new TemplateListItem(
                    "팬더",
                    "background_08.jpg",
                    Colors.Black,
                    Colors.White,
                    Colors.Black);
            tItems.Add(template8);

            TemplateListItem template9 =
                new TemplateListItem(
                    "방울",
                    "background_09.jpg",
                    Colors.Black,
                    Colors.White,
                    Colors.Black);
            tItems.Add(template9);

            TemplateListItem template10 =
                new TemplateListItem(
                    "사자",
                    "background_10.jpg",
                    Colors.Black,
                    Colors.White,
                    Colors.Black);
            tItems.Add(template10);


            //tItems.Add(test1);
            //tItems.Add(test1);
            //tItems.Add(test1);
            //tItems.Add(test1);
            //tItems.Add(test1);
            //tItems.Add(test1);
            //TemplateListItem test2 = new TemplateListItem(new Rectangle(), new SolidColorBrush(Color.FromRgb(0, 255, 0)), "test2");
            //tItems.Add(test2);
            //tItems.Add(test2);
            //TemplateListItem test3 = new TemplateListItem(new Rectangle(), new SolidColorBrush(Color.FromRgb(0, 0, 255)), "test3");
            //tItems.Add(test3);
            //tItems.Add(test3);
            ////test fin

            templateList.ItemsSource = tItems;
        
        }
        # endregion
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

        private void Open(object sender, RoutedEventArgs e)
        {
            // if toOpenPresentationInfo is not null, open the presentation file
            if (toOpenPresentationInfo != null)
                PresentationOpen(toOpenPresentationInfo);
            else
                openBtn.IsEnabled = false;
            // else alert using diagram
        }

        private void Click(object sender, RoutedEventArgs e)
        {
            save_Settings();
            
            //App.Current.MainWindow = main;
            FileSettings.file_Path = "C://KEAP";
            FileSettings.file_Name = "NewFile" + Convert.ToString(FileSettings.count);
            FileSettings.count++;

            TemplateOpen(toOpenTemplateInfo);
//            MainWindow main = new MainWindow();
            //MainWindow main = new MainWindow();
            //main.Show();
            //this.Hide();
        }

        void save_Settings()
        {
            WindowSettings.current_Width = Width;
            WindowSettings.current_Height = Height;
            WindowSettings.current_WindowState = WindowState;
        }

        private void templateList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected_item = templateList.SelectedItem as TemplateListItem;
            toOpenTemplateInfo = selected_item;
            TemplateOpen(toOpenTemplateInfo);
        }

        private void templateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected_item = templateList.SelectedItem as TemplateListItem;
            toOpenTemplateInfo = selected_item;
        }

        private void recentPresentations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            openBtn.IsEnabled = true;
            var selected_item = recentPresentationsList.SelectedItem as PresentationListItem;
            toOpenPresentationInfo = selected_item;
            PresentationOpen(toOpenPresentationInfo);
        }

        private void recentPresentationsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // TODO : open selected file
            var selected_item = recentPresentationsList.SelectedItem as PresentationListItem;
            toOpenPresentationInfo = selected_item;
            PresentationOpen(toOpenPresentationInfo);

        }

        private void PresentationOpen(PresentationListItem toOpenPresentationInfo)
        {
//            throw new NotImplementedException();
        }

        private void TemplateOpen(TemplateListItem toOpenTemplateInfo)
        {
            //            throw new NotImplementedException();
            save_Settings();

            //App.Current.MainWindow = main;
            FileSettings.file_Path = "C://KEAP";
            FileSettings.file_Name = "NewFile" + Convert.ToString(FileSettings.count);
            FileSettings.count++;

            //            MainWindow main = new MainWindow();
            MainWindow main = new MainWindow(toOpenTemplateInfo);
            main.Show();
            this.Hide();
            
        }
    }
}
