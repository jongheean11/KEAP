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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace KEAP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<KEAPCanvas> Slides = new List<KEAPCanvas>();
        KEAPCanvas MainCanvas;

        bool pen_Mode = false, text_Mode = false, line_Mode = false, line_Mode_Toggle = false,
            rectangle_Mode = false, polygon_Mode = false, polygon_Mode_Toggle = false, image_Mode = false,
            table_Mode = false, ellipse_Mode = false,
            canvas_LeftButton_Down=false;
        bool poly_Start_Get = false;
        
        Point Start_Point, Start_Point_Poly;

        int prev_Count_Children = 0, prev_Poly_Count_Children=0;

        BitmapImage new_Bitmap_Image;
        string image_Path;
        Rectangle image_Rect;

        int table_Row_Count = 2, table_Column_Count = 3;


        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowSettings.current_WindowState;

            // 그냥 최대로 했음
            this.Width = WindowSettings.resolution_Width;
            this.Height = WindowSettings.resolution_Height;

            MainCanvas = new KEAPCanvas()    
            {
                Width = (WindowSettings.resolution_Width/4.45)*3.75,
                Height = (WindowSettings.resolution_Height/4.32)*3,
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                Background=new SolidColorBrush(Colors.White)
            };
            Grid.SetRow(MainCanvas, 4);
            Grid.SetColumn(MainCanvas, 1);
            Grid.SetRowSpan(MainCanvas, 1);
            Grid.SetColumnSpan(MainCanvas, 1);
            MainCanvas.PreviewMouseLeftButtonDown += MainCanvas_PreviewMouseLeftButtonDown;
            MainCanvas.PreviewMouseMove += MainCanvas_PreviewMouseMove;
            MainCanvas.PreviewMouseLeftButtonUp += MainCanvas_PreviewMouseLeftButtonUp;
            MainGrid.Children.Add(MainCanvas);
            Slides.Add(MainCanvas);
        }

        void MainCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            canvas_LeftButton_Down = true;
            prev_Count_Children = MainCanvas.Children.Count;
            Start_Point = e.GetPosition(MainCanvas);

            if(line_Mode)
            {
                if (line_Mode_Toggle)
                {
                    line_Mode_Toggle = false;
                }
                else
                {
                    line_Mode_Toggle = true;
                }
            }

            if (polygon_Mode)
            {
                if (polygon_Mode_Toggle)
                {
                    
                    prev_Poly_Count_Children++;
                    if (poly_Start_Get)
                    {
                        polygon_Mode_Toggle = false;
                        prev_Poly_Count_Children = 0;
                    }
                }
                else
                {
                    polygon_Mode_Toggle = true;
                    Start_Point_Poly = e.GetPosition(MainCanvas);
                }
            }
            
            if (image_Mode)
            {
                Image image = new Image();
                image.Source = new_Bitmap_Image;
                image.Stretch = Stretch.Fill;
                Canvas.SetLeft(image, Start_Point.X);
                Canvas.SetTop(image, Start_Point.Y);
                MainCanvas.Children.Add(image);
            }

            if (table_Mode)
            {
                Grid grid = new Grid()
                {
                    Background = new SolidColorBrush(Colors.LightGray),
                    Width = 1,
                    Height = 1
                };
                
                int i;
                for (i = 0; i < table_Row_Count; i++)
                {
                    RowDefinition row_Definition = new RowDefinition();
                    grid.RowDefinitions.Add(row_Definition);
                }
                for (i = 0; i < table_Column_Count; i++)
                {
                    ColumnDefinition column_Definition = new ColumnDefinition();
                    grid.ColumnDefinitions.Add(column_Definition);
                }

                int table_Entry_X = 0, table_Entry_Y = 0;
                while (table_Entry_X * table_Entry_Y != table_Row_Count * table_Column_Count)
                {
                    EditableTextBlock textblock = new EditableTextBlock()
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        //BorderBrush = new SolidColorBrush(Colors.Black),
                        Background = new SolidColorBrush(Colors.LightCyan),
                        TextWrapping = TextWrapping.Wrap,
                        //FontStyle = Windows.UI.Text.FontStyle.Normal,
                    };
                    
                    Grid.SetRow(textblock, table_Entry_X);
                    Grid.SetColumn(textblock, table_Entry_Y);
                    Grid.SetRowSpan(textblock, 1);
                    Grid.SetColumnSpan(textblock, 1);

                    textblock.Width = (grid.Width / table_Column_Count);
                    textblock.Height = (grid.Height / table_Row_Count);
                    if (table_Entry_Y == table_Column_Count - 1)
                    {
                        table_Entry_Y = 0;
                        table_Entry_X++;
                    }
                    else
                        table_Entry_Y++;
                    grid.Children.Add(textblock);
                }
                MainCanvas.Children.Add(grid);
            }
        }

        void MainCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            canvas_LeftButton_Down = false;
        }

        void MainCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point End_Point = e.GetPosition(MainCanvas);
            double x1 = Start_Point.X;
            double y1 = Start_Point.Y;
            double x2 = End_Point.X;
            double y2 = End_Point.Y;

            if (e.LeftButton == MouseButtonState.Pressed&&canvas_LeftButton_Down)
            {
                if (pen_Mode)
                {
                    if (GetDistance(x1, y1, x2, y2) > 2.0) // We need to developp this method now 
                    {
                        Line line = new Line()
                        {
                            X1 = x1,
                            Y1 = y1,
                            X2 = x2,
                            Y2 = y2,
                            StrokeThickness = 4.0,
                            Stroke = new SolidColorBrush(Colors.Black)
                        };

                        Start_Point = End_Point;

                        // Draw the line on the canvas by adding the Line object as
                        // a child of the Canvas object.
                        MainCanvas.Children.Add(line);
                    }
                }

                else if (text_Mode)
                {
                    EditableTextBlock textblock = new EditableTextBlock()
                    {
                        Width = x2 - x1,
                        Height = y2 - y1,
                        Background = new SolidColorBrush(Colors.LightGray)
                    };

                    Canvas.SetLeft(textblock, x1);
                    Canvas.SetTop(textblock, y1);

                    if (prev_Count_Children != MainCanvas.Children.Count)
                    {
                        MainCanvas.Children.RemoveAt(prev_Count_Children);
                    }
                    MainCanvas.Children.Add(textblock);
                }

                else if (rectangle_Mode)
                {
                    int area1 = 0, area2 = 0, area3 = 0;
                    Rectangle rectangle = new Rectangle();
                    if (x1 < x2)
                    {
                        rectangle.Width = x2 - x1;
                        area1 = 1;
                    }
                    else
                    {
                        rectangle.Width = x1 - x2;
                        area2 = 1; area3 = 1;
                    }
                    if (y1 < y2)
                    {
                        rectangle.Height = y2 - y1;
                        area3 = 0;
                    }
                    else
                    {
                        rectangle.Height = y1 - y2;
                        area1 = 0; area2 = 0;
                    }
                    rectangle.StrokeThickness = 4.0;
                    rectangle.Stroke = new SolidColorBrush(Colors.Green);
                    if (area1 == 1) { Canvas.SetLeft(rectangle, x1); Canvas.SetTop(rectangle, y1); }
                    else if (area2 == 1) { Canvas.SetLeft(rectangle, x2); Canvas.SetTop(rectangle, y1); }
                    else if (area3 == 1) { Canvas.SetLeft(rectangle, x2); Canvas.SetTop(rectangle, y2); }
                    else { Canvas.SetLeft(rectangle, x1); Canvas.SetTop(rectangle, y2); }

                    if (prev_Count_Children != MainCanvas.Children.Count)
                    {
                        MainCanvas.Children.RemoveAt(prev_Count_Children);
                    }
                    MainCanvas.Children.Add(rectangle);
                }


                else if (image_Mode)
                {
                    int area1 = 0, area2 = 0, area3 = 0;
                    if (x1 < x2)
                    {
                        ((Image)MainCanvas.Children[prev_Count_Children]).Width = x2 - x1;
                        area1 = 1;
                    }
                    else
                    {
                        ((Image)MainCanvas.Children[prev_Count_Children]).Width = x1 - x2;
                        area2 = 1; area3 = 1;
                    }
                    if (y1 < y2)
                    {
                        ((Image)MainCanvas.Children[prev_Count_Children]).Height = y2 - y1;
                        area3 = 0;
                    }
                    else
                    {
                        ((Image)MainCanvas.Children[prev_Count_Children]).Height = y1 - y2;
                        area1 = 0; area2 = 0;
                    }
                    if (area1 == 1) { Canvas.SetLeft(((Image)MainCanvas.Children[prev_Count_Children]), x1); Canvas.SetTop(((Image)MainCanvas.Children[prev_Count_Children]), y1); }
                    else if (area2 == 1) { Canvas.SetLeft(((Image)MainCanvas.Children[prev_Count_Children]), x2); Canvas.SetTop(((Image)MainCanvas.Children[prev_Count_Children]), y1); }
                    else if (area3 == 1) { Canvas.SetLeft(((Image)MainCanvas.Children[prev_Count_Children]), x2); Canvas.SetTop(((Image)MainCanvas.Children[prev_Count_Children]), y2); }
                    else { Canvas.SetLeft(((Image)MainCanvas.Children[prev_Count_Children]), x1); Canvas.SetTop(((Image)MainCanvas.Children[prev_Count_Children]), y2); }
                }

                else if (table_Mode)
                {
                    int area1 = 0, area2 = 0, area3 = 0;
                    if (x1 < x2)
                    {
                        ((Grid)MainCanvas.Children[prev_Count_Children]).Width = x2 - x1;
                        area1 = 1;
                    }
                    else
                    {
                        ((Grid)MainCanvas.Children[prev_Count_Children]).Width = x1 - x2;
                        area2 = 1; area3 = 1;
                    }
                    if (y1 < y2)
                    {
                        ((Grid)MainCanvas.Children[prev_Count_Children]).Height = y2 - y1;
                        area3 = 0;
                    }
                    else
                    {
                        ((Grid)MainCanvas.Children[prev_Count_Children]).Height = y1 - y2;
                        area1 = 0; area2 = 0;
                    }
                    double block_Width= ((Grid)MainCanvas.Children[prev_Count_Children]).Width / table_Column_Count,
                        block_Height = ((Grid)MainCanvas.Children[prev_Count_Children]).Height / table_Row_Count;
                    if (block_Width > 2)
                    {
                        block_Width = block_Width - 2;
                    }
                    if (block_Height > 2)
                    {
                        block_Height = block_Height - 2;
                    }
                    foreach (EditableTextBlock textblock in ((Grid)MainCanvas.Children[prev_Count_Children]).Children)
                    {
                        textblock.Width = block_Width;
                        textblock.Height = block_Height;
                    }

                    if (area1 == 1) { Canvas.SetLeft(((Grid)MainCanvas.Children[prev_Count_Children]), x1); Canvas.SetTop(((Grid)MainCanvas.Children[prev_Count_Children]), y1); }
                    else if (area2 == 1) { Canvas.SetLeft(((Grid)MainCanvas.Children[prev_Count_Children]), x2); Canvas.SetTop(((Grid)MainCanvas.Children[prev_Count_Children]), y1); }
                    else if (area3 == 1) { Canvas.SetLeft(((Grid)MainCanvas.Children[prev_Count_Children]), x2); Canvas.SetTop(((Grid)MainCanvas.Children[prev_Count_Children]), y2); }
                    else { Canvas.SetLeft(((Grid)MainCanvas.Children[prev_Count_Children]), x1); Canvas.SetTop(((Grid)MainCanvas.Children[prev_Count_Children]), y2); }
                }

                else if(ellipse_Mode)
                {
                    int area1 = 0, area2 = 0, area3 = 0;
                    Ellipse ellipse = new Ellipse();
                    if (x1 < x2)
                    {
                        ellipse.Width = x2 - x1;
                        area1 = 1;
                    }
                    else
                    {
                        ellipse.Width = x1 - x2;
                        area2 = 1; area3 = 1;
                    }
                    if (y1 < y2)
                    {
                        ellipse.Height = y2 - y1;
                        area3 = 0;
                    }
                    else
                    {
                        ellipse.Height = y1 - y2;
                        area1 = 0; area2 = 0;
                    }
                    ellipse.StrokeThickness = 4.0;
                    ellipse.Stroke = new SolidColorBrush(Colors.Green);
                    if (area1 == 1) { Canvas.SetLeft(ellipse, x1); Canvas.SetTop(ellipse, y1); }
                    else if (area2 == 1) { Canvas.SetLeft(ellipse, x2); Canvas.SetTop(ellipse, y1); }
                    else if (area3 == 1) { Canvas.SetLeft(ellipse, x2); Canvas.SetTop(ellipse, y2); }
                    else { Canvas.SetLeft(ellipse, x1); Canvas.SetTop(ellipse, y2); }

                    if (prev_Count_Children != MainCanvas.Children.Count)
                    {
                        MainCanvas.Children.RemoveAt(prev_Count_Children);
                    }
                    MainCanvas.Children.Add(ellipse);
                }
            }

            else
            {
                if (line_Mode)
                {
                    if (line_Mode_Toggle)
                    {
                        Line line = new Line()
                        {
                            X1 = x1,
                            Y1 = y1,
                            X2 = x2-1,
                            Y2 = y2-1,
                            StrokeThickness = 4.0,
                            Stroke = new SolidColorBrush(Colors.Black)
                        };
                        if (line.X2 < line.X1)
                        {
                            line.X2 += 2;
                        }
                        if(line.Y2 < line.Y1)
                        {
                            line.Y2 += 2;
                        }

                        if (prev_Count_Children != MainCanvas.Children.Count)
                        {
                            MainCanvas.Children.RemoveAt(prev_Count_Children);
                        }
                        MainCanvas.Children.Add(line);
                    }
                }
                
                else if (polygon_Mode && polygon_Mode_Toggle)
                {
                    if ((prev_Poly_Count_Children >= 2)&&(GetDistance(End_Point.X, End_Point.Y, Start_Point_Poly.X, Start_Point_Poly.Y) < 7.0))
                    {
                        End_Point = Start_Point_Poly;
                        poly_Start_Get = true;
                    }
                    else
                    {
                        poly_Start_Get = false;
                    }

                    Line line;
                    if (!poly_Start_Get)
                    { 
                        line = new Line()
                        {
                            X1 = x1,
                            Y1 = y1,
                            X2 = x2-1,
                            Y2 = y2-1,
                            StrokeThickness = 4.0,
                            Stroke = new SolidColorBrush(Colors.Black)
                        };
                        if (line.X2 < line.X1)
                        {
                            line.X2 += 2;
                        }
                        if (line.Y2 < line.Y1)
                        {
                            line.Y2 += 2;
                        }
                    }
                    else
                    {
                        line = new Line()
                        {
                            X1 = x1,
                            Y1 = y1,
                            X2 = Start_Point_Poly.X,
                            Y2 = Start_Point_Poly.Y,
                            StrokeThickness = 4.0,
                            Stroke = new SolidColorBrush(Colors.Black)
                        };
                    }

                    if (prev_Count_Children != MainCanvas.Children.Count)
                        MainCanvas.Children.RemoveAt(prev_Count_Children);

                    MainCanvas.Children.Add(line);
                }
            }
        }

        void SetAllModeFalse()
        {
            pen_Mode = text_Mode = line_Mode = rectangle_Mode = polygon_Mode = image_Mode = table_Mode = ellipse_Mode = false;
        }

        private void Pen_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetAllModeFalse();
            pen_Mode = true;
        }

        private void Pen_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void Text_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetAllModeFalse();
            text_Mode = true;
        }

        private void Text_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
        private void Line_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetAllModeFalse();
            line_Mode = true;
        }

        private void Line_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
        private void Rectangle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetAllModeFalse();
            rectangle_Mode = true;
        }

        private void Rectangle_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Polygon_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetAllModeFalse();
            polygon_Mode = true;
        }

        private void Polygon_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Image_Click(object sender, RoutedEventArgs e)
        {
            SetAllModeFalse();

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpeg"; // Default file extension
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif|BMP Files (*.bmp)|*.bmp"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                image_Path = dlg.FileName;
                image_Mode = true;

                new_Bitmap_Image = new BitmapImage();
                new_Bitmap_Image.BeginInit();
                new_Bitmap_Image.UriSource = new Uri(image_Path, UriKind.Absolute);
                new_Bitmap_Image.EndInit();
            }
        }

        private void Table_Click(object sender, RoutedEventArgs e)
        {
            SetAllModeFalse();

            
            table_Mode = true;

        }
        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            SetAllModeFalse();
        }

        private double GetDistance(double x1, double y1, double x2, double y2)
        {
            double d = 0;
            d = Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
            return d;
        }

        private void BackMenu_Click(object sender, RoutedEventArgs e)
        {
            ////////////
            AllMenuBorderToWhite();
            BackMenu.BorderBrush = new SolidColorBrush(Colors.LightGray);
            ///////////
        }

        private void HomeMenu_Click(object sender, RoutedEventArgs e)
        {
            AllMenuBorderToWhite();
            HomeMenu.BorderBrush = new SolidColorBrush(Colors.LightGray);

        }

        private void InsertMenu_Click(object sender, RoutedEventArgs e)
        {
            AllMenuBorderToWhite();
            InsertMenu.BorderBrush = new SolidColorBrush(Colors.LightGray);
        }

        private void DesignMenu_Click(object sender, RoutedEventArgs e)
        {
            AllMenuBorderToWhite();
            DesignMenu.BorderBrush = new SolidColorBrush(Colors.LightGray);
        }

        private void AnimationMenu_Click(object sender, RoutedEventArgs e)
        {
            AllMenuBorderToWhite();
            AnimationMenu.BorderBrush = new SolidColorBrush(Colors.LightGray);
        }

        private void ConvertMenu_Click(object sender, RoutedEventArgs e)
        {
            AllMenuBorderToWhite();
            ConvertMenu.BorderBrush = new SolidColorBrush(Colors.LightGray);
        }

        private void StyleMenu_Click(object sender, RoutedEventArgs e)
        {
            AllMenuBorderToWhite();
            BackMenu.BorderBrush = new SolidColorBrush(Colors.LightGray);
        }
        void AllMenuBorderToWhite()
        {
            HomeMenu.BorderBrush = InsertMenu.BorderBrush = DesignMenu.BorderBrush
                = AnimationMenu.BorderBrush = ConvertMenu.BorderBrush = StyleMenu.BorderBrush = new SolidColorBrush(Colors.White);
        }

        private void NewSlideButton_Click(object sender, RoutedEventArgs e)
        {
            KEAPCanvas new_Canvas = new KEAPCanvas()
            {
                Width = (this.Width / 4.45) * 3.75,
                Height = (this.Height / 4.32) * 3,
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(Colors.White)
            };
            Grid.SetRow(new_Canvas, 4);
            Grid.SetColumn(new_Canvas, 1);
            Grid.SetRowSpan(new_Canvas, 1);
            Grid.SetColumnSpan(new_Canvas, 1);

            Slides.Add(new_Canvas);

            Save_SlideView(new_Canvas,Slides.Count);

            Add_Slide(Slides.Count);

            Change_Slide(Slides.Count-1);
            
            
        }

        private void Slide_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Change_Slide(Slide_ListView.SelectedIndex);
        }

        void Add_Slide(int param_count)
        {
            //var xmlFile = System.IO.File.OpenRead(FileSettings.file_Name + "_slide" + Convert.ToString(param_count) + ".xml")/;
            //XmlDocument xmlDoc = await XmlDocument.LoadFromFileAsync(xmlFile);
            //XmlDocument xmlDoc;
            //xmlDoc.Load(FileSettings.file_Name + "_slide" + Convert.ToString(param_count) + ".xml");
           /* var fs=System.IO.File.OpenRead((FileSettings.file_Name + "_slide" + Convert.ToString(param_count) + ".xml"));
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.StreamSource = fs;
            XmlElement root = xmlDoc.DocumentElement;
            XmlNodeList fileNodeList = root.GetElementsByTagName("file");
            //string filename = ((XmlElement)fileNodeList.ElementAt(ItemLists.SelectedIndex)).GetAttribute("id");

            
            ImageSource source = new BitmapImage();
                    StorageFile imagefile = await StorageFile.GetFileFromPathAsync(ImagePath);
                    if (imagefile != null)
                    {
                        var stream = await imagefile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                        using (IRandomAccessStream fileStream = await imagefile.OpenAsync(Windows.Storage.FileAccessMode.Read))
                        {
                            BitmapImage bitmapImage = new BitmapImage();

                            await bitmapImage.SetSourceAsync(fileStream);
                            source = bitmapImage;
                        }
                    }
                    FileLists.Add(new FileComp()
                    {
                        Source=source,
                        Title = "파일이름 : " + munoramaName, 
                        Description = "파일설명 : " + munoramaFD + "\n" });
                }
                ItemLists.ItemsSource = FileLists;*/ 
        }

        void Change_Slide(int index)
        {
            MainCanvas = Slides.ElementAt(index);
        }

        private void Ellipse_Click(object sender, RoutedEventArgs e)
        {
            SetAllModeFalse();
            ellipse_Mode = true;
        }

        void Save_SlideView(KEAPCanvas param_Canvas, int param_count)
        {
            Transform transform = param_Canvas.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            param_Canvas.LayoutTransform = null;

            // Get the size of canvas
            Size size = new Size(param_Canvas.RenderSize.Width, param_Canvas.RenderSize.Height);
            // Measure and arrange the surface
            // VERY IMPORTANT
            param_Canvas.Measure(size);
            param_Canvas.Arrange(new Rect(size));

            // Create a render bitmap and push the surface to it
            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(param_Canvas);
            
            // Create a file stream for saving image
            using (var fs = System.IO.File.OpenWrite(FileSettings.file_Name+"_slide"+Convert.ToString(param_count)+".jpg"))
            {
                // Use png encoder for our data
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                // push the rendered bitmap to it
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                // save the data to the stream
                encoder.Save(fs);
            }

            // Restore previously saved layout
            param_Canvas.LayoutTransform = transform;
        }
    }
}
