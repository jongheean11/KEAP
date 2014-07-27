using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using KEAP.Animations;
using System.Windows.Media.Animation;
using System.IO.Compression;
using Microsoft.Win32;
using System.Windows.Media.Effects;
using Microsoft.Kinect;

namespace KEAP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FullWindowForAudience Audience = null;
        private FullWindowForPresentor Presentor = null;
        public List<KEAPCanvas> canvas_List = new List<KEAPCanvas>();
        public ObservableCollection<SlideInfo> Slides_List = new ObservableCollection<SlideInfo>();
        public Dictionary<int, List<BitmapFrame>> bitmapFrame_Dictionary = new Dictionary<int, List<BitmapFrame>>();
        public BitmapFrame canvas_Background;
        public BitmapImage canvas_Background_Bitmapimage;
        public ObservableCollection<Ani_List_Info> Anis_List = new ObservableCollection<Ani_List_Info>();
        
        public List<Dictionary<int, string>> animation_List = new List<Dictionary<int, string>>();
        public Dictionary<int, List<Dictionary<int, string>>> animation_Dictionary = new Dictionary<int, List<Dictionary<int, string>>>();

        int previous_selection = -1;
        KEAPCanvas MainCanvas;

        bool pen_Mode = false, text_Mode = false, line_Mode = false, line_Mode_Toggle = false,
            rectangle_Mode = false, polygon_Mode = false, polygon_Mode_Toggle = false, image_Mode = false,
            ellipse_Mode = false, eraseforpen_Mode = false;

        bool select_Mode = false;

        bool pen_Move_Mode = false, text_Move_Mode = false, line_Move_Mode = false,
            rectangle_Move_Mode = false, polygon_Move_Mode = false, image_Move_Mode = false,
            ellipse_Move_Mode = false;
        bool drag_Mode = true;
        
        bool edit_Mode = false, edit_Mode_Ready = false;
        bool canvas_LeftButton_Down = false;
        bool poly_Start_Get = false;
        double shape_x = 0.0;
        double shape_y = 0.0;

        Point Start_Point, Start_Point_Poly, Start_Point_Shape, Begin_Point_Shape;

        int prev_Count_Children = 0, prev_Poly_Count_Children = 0;

        BitmapImage new_Bitmap_Image;
        string image_Path;

        Rectangle edit_Rect;

        UIElement selected_Object;
        EditableTextBlock selected_Text;
        Line selected_Line;
        Polygon selected_Polygon;
        Shape selected_Shape;
        Image selected_Image;

        Brush stroke_Brush, fill_Brush, font_Brush;
        FontFamily font_Family; int font_Family_Index;
        double font_Size; int font_Size_Index;

        public MainWindow(TemplateListItem selected_Template)
        {
            InitializeComponent();
            WindowState = WindowSettings.current_WindowState;

            // 그냥 최대로 했음
            this.Width = WindowSettings.resolution_Width;
            this.Height = WindowSettings.resolution_Height;
            this.WindowState = WindowState.Maximized;

            canvas_Background_Bitmapimage = new BitmapImage();
            canvas_Background_Bitmapimage = new BitmapImage();
            canvas_Background_Bitmapimage.BeginInit();
            canvas_Background_Bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
//            canvas_Background_Bitmapimage.UriSource = new Uri("pack://application:,,,/KEAP;v1.0.0.0;component/Images/Background/white.jpg");
            canvas_Background_Bitmapimage.UriSource = new Uri(selected_Template.img_Uri);
            canvas_Background_Bitmapimage.EndInit();
            canvas_Background = BitmapFrame.Create(canvas_Background_Bitmapimage);

            MainCanvas = new KEAPCanvas()
            {
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                Background = new ImageBrush(canvas_Background_Bitmapimage)
            };
            
            if ((this.Width * 3.75 / 4.45) < (((this.Height - 92) * 3 / 3.8) * (WindowSettings.resolution_Width / WindowSettings.resolution_Height)))
            {
                MainCanvas.Width = (WindowSettings.resolution_Width * 3.75 / 4.45) - 50;
                MainCanvas.Height = (MainCanvas.Height * (WindowSettings.resolution_Width / WindowSettings.resolution_Height));
            }
            else
            {
                MainCanvas.Height = (WindowSettings.resolution_Height - 92) * (3 / 3.8) - 50;
                MainCanvas.Width = MainCanvas.Height * (WindowSettings.resolution_Height / WindowSettings.resolution_Width);
            }

            MainCanvas_Border.Child = MainCanvas;
            MainCanvas.PreviewMouseLeftButtonDown += MainCanvas_PreviewMouseLeftButtonDown;
            MainCanvas.PreviewMouseMove += MainCanvas_PreviewMouseMove;
            MainCanvas.PreviewMouseLeftButtonUp += MainCanvas_PreviewMouseLeftButtonUp;

            canvas_List.Add(MainCanvas);

            Add_Slide_List(canvas_List.Count);
            Slide_ListView.SelectedIndex = 0;
            this.SizeChanged += MainWindow_SizeChanged;
            //stroke_Brush = new SolidColorBrush(Colors.Black);
            //fill_Brush = new SolidColorBrush(Colors.White);
            //font_Brush = new SolidColorBrush(Colors.Black);
            stroke_Brush = new SolidColorBrush(selected_Template.stroke_Brush);
            fill_Brush = new SolidColorBrush(selected_Template.fill_Brush);
            font_Brush = new SolidColorBrush(selected_Template.font_Brush);
            AddKeyBindings();

            Autoedit_Slide_List(MainCanvas, Slide_ListView.SelectedIndex);
            
            AllMenuGridToCollapsed();
            HomeMenu_Grid.Visibility = Visibility.Visible;
        }


        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowSettings.current_WindowState;

            // 그냥 최대로 했음
            this.Width = WindowSettings.resolution_Width;
            this.Height = WindowSettings.resolution_Height;
            this.WindowState = WindowState.Maximized;

            canvas_Background_Bitmapimage = new BitmapImage();
            canvas_Background_Bitmapimage = new BitmapImage();
            canvas_Background_Bitmapimage.BeginInit();
            canvas_Background_Bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            canvas_Background_Bitmapimage.UriSource = new Uri("pack://application:,,,/KEAP;v1.0.0.0;component/Images/Background/background_01.jpg");
            canvas_Background_Bitmapimage.EndInit();
            canvas_Background = BitmapFrame.Create(canvas_Background_Bitmapimage);

            MainCanvas = new KEAPCanvas()
            {
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                Background = new ImageBrush(canvas_Background_Bitmapimage)
            };

            if ((this.Width * 3.75 / 4.45) < (((this.Height - 92) * 3 / 3.8) * (WindowSettings.resolution_Width / WindowSettings.resolution_Height)))
            {
                MainCanvas.Width = (WindowSettings.resolution_Width * 3.75 / 4.45) - 50;
                MainCanvas.Height = (MainCanvas.Height * (WindowSettings.resolution_Width / WindowSettings.resolution_Height));
            }
            else
            {
                MainCanvas.Height = (WindowSettings.resolution_Height - 92) * (3 / 3.8) - 50;
                MainCanvas.Width = MainCanvas.Height * (WindowSettings.resolution_Height / WindowSettings.resolution_Width);
            }

            MainCanvas_Border.Child = MainCanvas;
            MainCanvas.PreviewMouseLeftButtonDown += MainCanvas_PreviewMouseLeftButtonDown;
            MainCanvas.PreviewMouseMove += MainCanvas_PreviewMouseMove;
            MainCanvas.PreviewMouseLeftButtonUp += MainCanvas_PreviewMouseLeftButtonUp;

            canvas_List.Add(MainCanvas);

            Add_Slide_List(canvas_List.Count);
            Slide_ListView.SelectedIndex = 0;
            this.SizeChanged += MainWindow_SizeChanged;
            stroke_Brush = new SolidColorBrush(Colors.Black);
            fill_Brush = new SolidColorBrush(Colors.White);
            font_Brush = new SolidColorBrush(Colors.Black);
            
            AddKeyBindings();

            Autoedit_Slide_List(MainCanvas, Slide_ListView.SelectedIndex);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            WindowSettings.current_Width = availableSize.Width;
            WindowSettings.current_Height = availableSize.Height;
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            WindowSettings.current_Width = arrangeBounds.Width;
            WindowSettings.current_Height = arrangeBounds.Height;
            return base.ArrangeOverride(arrangeBounds);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if ((this.Width * 3.75 / 4.45) < (((this.Height - 92) * 3 / 3.8) * (WindowSettings.resolution_Width / WindowSettings.resolution_Height)))
            {
                //MainCanvas.Width > 0, when "WindowSettings.current_Width" > 59.33... ~= 50 * 3.35 / 3.75
                MainCanvas.Width = (WindowSettings.current_Width * 3.75 / 4.45) - 50;
                MainCanvas.Height = (MainCanvas.Width * (WindowSettings.resolution_Height / WindowSettings.resolution_Width));
            }
            else
            {
                // MainCanvs.height > 0, when  "WindowSettings.current_Height" > 155.33... ~= 50 * 3.8/3 + 92  
                MainCanvas.Height = (WindowSettings.current_Height - 92) * (3 / 3.8) - 50;
                MainCanvas.Width = (MainCanvas.Height * (WindowSettings.resolution_Width / WindowSettings.resolution_Height));
            }

            
            MainCanvas.Measure(new Size(MainCanvas.Width, MainCanvas.Height));
            MainCanvas.Arrange(new Rect(0, 0, MainCanvas.Width, MainCanvas.Height));
            for (int i = 0; i < Slide_ListView.Items.Count; i++)
                Autoedit_Slide_List(canvas_List[i], i);
        }

        #region MainCanvas 함수
        void MainCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            canvas_LeftButton_Down = true;
            prev_Count_Children = MainCanvas.Children.Count;
            Start_Point = e.GetPosition(MainCanvas);

            if (line_Mode)
            {
                if (line_Mode_Toggle)
                {
                    line_Mode_Toggle = false;
                    line_Mode = false;
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

                        Polygon polygon = new Polygon()
                        {
                            StrokeThickness = 4.0,
                            Stroke = stroke_Brush,
                            Fill = fill_Brush
                        };
                        for (int i = 0; i < prev_Poly_Count_Children; i++)
                        {
                            Line templine = (Line)MainCanvas.Children[MainCanvas.Children.Count - prev_Poly_Count_Children + i];
                            polygon.Points.Add(new Point(templine.X1, templine.Y1));
                        }
                        for (int i = 0; i < prev_Poly_Count_Children; i++)
                            MainCanvas.Children.RemoveAt(MainCanvas.Children.Count - 1);

                        polygon.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                        polygon.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                        polygon.MouseMove += UIElement_MouseMove;

                        MainCanvas.Children.Add(polygon);

                        prev_Poly_Count_Children = 0;
                        polygon_Mode = false;
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
                //image.MouseEnter += UIElement_MouseEnter;
                //image.MouseLeave += UIElement_MouseLeave;
                MainCanvas.Children.Add(image);
            }
        }

        void MainCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (MainCanvas.Children.Count != 0)
            {
                if (image_Mode && (MainCanvas.Children[MainCanvas.Children.Count - 1] is Image) && (e.MouseDevice.LeftButton != MouseButtonState.Pressed))
                {
                    int count = 0;
                    foreach (UIElement ele in MainCanvas.Children)
                    {
                        if (ele is Image)
                            count++;
                    }

                    List<BitmapFrame> bitmapFrame_List;
                    bool newslide = false, contin = false;
                    if (bitmapFrame_Dictionary.Count == Slide_ListView.Items.Count)
                    {
                        bitmapFrame_List = bitmapFrame_Dictionary[Slide_ListView.SelectedIndex];
                        if (count > bitmapFrame_List.Count)
                            contin = true;
                    }
                    else
                    {
                        bitmapFrame_List = new List<BitmapFrame>();
                        contin = true;
                        newslide = true;
                    }
                    if (contin)
                    {
                        bitmapFrame_List.Add(BitmapFrame.Create(new_Bitmap_Image));
                        if (newslide)
                            bitmapFrame_Dictionary.Add(Slide_ListView.SelectedIndex, bitmapFrame_List);
                        else
                            bitmapFrame_Dictionary[Slide_ListView.SelectedIndex] = bitmapFrame_List;
                        image_Mode = false;
                    }

                    MainCanvas.Children[MainCanvas.Children.Count - 1].MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                    MainCanvas.Children[MainCanvas.Children.Count - 1].MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                    MainCanvas.Children[MainCanvas.Children.Count - 1].MouseMove += UIElement_MouseMove;
                }
            }
            canvas_LeftButton_Down = false;
            Autoedit_Slide_List(MainCanvas, Slide_ListView.SelectedIndex);

            if (selected_Text != null)
            {
                if ((e.GetPosition(MainCanvas).X < Canvas.GetLeft(selected_Text)) ||
                    (e.GetPosition(MainCanvas).Y < Canvas.GetTop(selected_Text)) ||
                    (e.GetPosition(MainCanvas).X > Canvas.GetLeft(selected_Text) + selected_Text.Width) ||
                    (e.GetPosition(MainCanvas).Y > Canvas.GetTop(selected_Text) + selected_Text.Height))
                {
                    edit_Mode = false; //editing..
                    edit_Mode_Ready = false;
                    MainCanvas.Children.Remove(edit_Rect);
                    BoldButton.Background = new SolidColorBrush(Colors.White);
                    ItalicButton.Background = new SolidColorBrush(Colors.White);
                    UnderlineButton.Background = new SolidColorBrush(Colors.White);
                    ShadowButton.Background = new SolidColorBrush(Colors.White);
                    BoldButton_Clicked = ItalicButton_Clicked = UnderlineButton_Clicked = ShadowButton_Clicked = false;
                }
            }
            else if (selected_Polygon != null)
            {
                if (edit_Rect != null)
                {
                    if ((e.GetPosition(MainCanvas).X < Canvas.GetLeft(edit_Rect) + 5) ||
                        (e.GetPosition(MainCanvas).Y < Canvas.GetTop(edit_Rect) + 5) ||
                        (e.GetPosition(MainCanvas).X > Canvas.GetLeft(edit_Rect) + 5 + edit_Rect.Width - 10) ||
                        (e.GetPosition(MainCanvas).Y > Canvas.GetTop(edit_Rect) + 5 + edit_Rect.Height - 10))
                    {
                        edit_Mode = false; //editing..
                        edit_Mode_Ready = false;
                        MainCanvas.Children.Remove(edit_Rect);

                        selected_Polygon.StrokeDashArray = null;
                    }
                }
            }
                
            else if (selected_Line != null)
            {
                if (edit_Rect != null)
                {
                    if ((e.GetPosition(MainCanvas).X < Canvas.GetLeft(edit_Rect) + 5) ||
                        (e.GetPosition(MainCanvas).Y < Canvas.GetTop(edit_Rect) + 5) ||
                        (e.GetPosition(MainCanvas).X > Canvas.GetLeft(edit_Rect) + 5 + edit_Rect.Width - 10) ||
                        (e.GetPosition(MainCanvas).Y > Canvas.GetTop(edit_Rect) + 5 + edit_Rect.Height - 10))
                    {
                        edit_Mode = false; //editing..
                        edit_Mode_Ready = false;
                        MainCanvas.Children.Remove(edit_Rect);

                        selected_Line.StrokeDashArray = null;
                    }
                }
            }
            else if (selected_Image != null)
            {
                if ((e.GetPosition(MainCanvas).X < Canvas.GetLeft(selected_Image)) ||
                    (e.GetPosition(MainCanvas).Y < Canvas.GetTop(selected_Image)) ||
                    (e.GetPosition(MainCanvas).X > Canvas.GetLeft(selected_Image) + selected_Image.Width) ||
                    (e.GetPosition(MainCanvas).Y > Canvas.GetTop(selected_Image) + selected_Image.Height))
                {
                    edit_Mode = false; //editing..
                    edit_Mode_Ready = false;
                    MainCanvas.Children.Remove(edit_Rect);
                }
            }
            else if (selected_Shape != null)
            {
                if ((e.GetPosition(MainCanvas).X < Canvas.GetLeft(selected_Shape)) ||
                    (e.GetPosition(MainCanvas).Y < Canvas.GetTop(selected_Shape)) ||
                    (e.GetPosition(MainCanvas).X > Canvas.GetLeft(selected_Shape) + selected_Shape.Width) ||
                    (e.GetPosition(MainCanvas).Y > Canvas.GetTop(selected_Shape) + selected_Shape.Height))
                {
                    edit_Mode = false; //editing..
                    edit_Mode_Ready = false;
                    MainCanvas.Children.Remove(edit_Rect);
                }
            }

            Image image_renew = RenderCanvas(MainCanvas);
            ((SlideInfo)(Slide_ListView.Items[Slide_ListView.SelectedIndex])).Source = image_renew.Source;
            text_Mode = image_Mode = rectangle_Mode = ellipse_Mode = false;
        }
        void MainCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point End_Point = e.GetPosition(MainCanvas);
            double x1 = Start_Point.X;
            double y1 = Start_Point.Y;
            double x2 = End_Point.X;
            double y2 = End_Point.Y;

            if (e.LeftButton == MouseButtonState.Pressed && canvas_LeftButton_Down)
            {
                if(eraseforpen_Mode)
                {
                    bool found = false;
                    for (int i = Convert.ToInt32(x2 - 5); i <= Convert.ToInt32(x2 + 5); i++)
                    {
                        for (int j = Convert.ToInt32(y2 - 5); j <= Convert.ToInt32(y2 + 5); j++)
                        {
                            foreach(Line line in MainCanvas.Children)
                            {
                                if (line.StrokeThickness==3.9)
                                {
                                    if (((Convert.ToInt32(line.X1) == i) && (Convert.ToInt32(line.Y1) == j)) || ((Convert.ToInt32(line.X2) == i) && (Convert.ToInt32(line.Y2) == j)))
                                    {
                                        MainCanvas.Children.Remove(line);
                                        found = true;
                                        break;
                                    }
                                }
                            }
                            if (found) break;
                        }
                        if (found) break;
                    }
                }
                else if (pen_Mode)
                {
                    if (GetDistance(x1, y1, x2, y2) > 1.7) // We need to developp this method now 
                    {
                        Line line = new Line()
                        {
                            X1 = x1,
                            Y1 = y1,
                            X2 = x2,
                            Y2 = y2,
                            StrokeThickness = 3.9,
                            Stroke = new SolidColorBrush(Colors.Black)
                        };

                        Start_Point = End_Point;

                        // Draw the line on the canvas by adding the Line object as
                        MainCanvas.Children.Add(line);
                    }
                }

                else if (text_Mode)
                {
                    int area1 = 0, area2 = 0, area3 = 0;
                    EditableTextBlock textblock = new EditableTextBlock()
                    {
                        Background = fill_Brush,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Foreground = font_Brush,
                        FontFamily = font_Family,
                        FontSize = font_Size
                    };

                    if (x1 < x2)
                    {
                        textblock.Width = x2 - x1;
                        area1 = 1;
                    }
                    else
                    {
                        textblock.Width = x1 - x2;
                        area2 = 1; area3 = 1;
                    }
                    if (y1 < y2)
                    {
                        textblock.Height = y2 - y1;
                        area3 = 0;
                    }
                    else
                    {
                        textblock.Height = y1 - y2;
                        area1 = 0; area2 = 0;
                    }
                    if (area1 == 1) { Canvas.SetLeft(textblock, x1); Canvas.SetTop(textblock, y1); }
                    else if (area2 == 1) { Canvas.SetLeft(textblock, x2); Canvas.SetTop(textblock, y1); }
                    else if (area3 == 1) { Canvas.SetLeft(textblock, x2); Canvas.SetTop(textblock, y2); }
                    else { Canvas.SetLeft(textblock, x1); Canvas.SetTop(textblock, y2); }

                    if (prev_Count_Children != MainCanvas.Children.Count)
                    {
                        MainCanvas.Children.RemoveAt(prev_Count_Children);
                    }
                    //textblock.MouseEnter += UIElement_MouseEnter;
                    textblock.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                    textblock.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                    textblock.MouseMove += UIElement_MouseMove;
                    //textblock.MouseLeave += UIElement_MouseLeave;
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
                    rectangle.Stroke = stroke_Brush;
                    rectangle.Fill = fill_Brush;//중요
                    if (area1 == 1) { Canvas.SetLeft(rectangle, x1); Canvas.SetTop(rectangle, y1); }
                    else if (area2 == 1) { Canvas.SetLeft(rectangle, x2); Canvas.SetTop(rectangle, y1); }
                    else if (area3 == 1) { Canvas.SetLeft(rectangle, x2); Canvas.SetTop(rectangle, y2); }
                    else { Canvas.SetLeft(rectangle, x1); Canvas.SetTop(rectangle, y2); }

                    if (prev_Count_Children != MainCanvas.Children.Count)
                    {
                        MainCanvas.Children.RemoveAt(prev_Count_Children);
                    }
                    //rectangle.MouseEnter += UIElement_MouseEnter;
                    rectangle.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                    rectangle.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                    rectangle.MouseMove += UIElement_MouseMove;
                    //rectangle.MouseLeave += UIElement_MouseLeave;
                    MainCanvas.Children.Add(rectangle);
                }


                else if (image_Mode)
                {
                    int area1 = 0, area2 = 0, area3 = 0;
                    if (MainCanvas.Children[prev_Count_Children] is Image)
                    {
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
                }
                
                else if (ellipse_Mode)
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
                    ellipse.Stroke = stroke_Brush;
                    ellipse.Fill = fill_Brush;
                    if (area1 == 1) { Canvas.SetLeft(ellipse, x1); Canvas.SetTop(ellipse, y1); }
                    else if (area2 == 1) { Canvas.SetLeft(ellipse, x2); Canvas.SetTop(ellipse, y1); }
                    else if (area3 == 1) { Canvas.SetLeft(ellipse, x2); Canvas.SetTop(ellipse, y2); }
                    else { Canvas.SetLeft(ellipse, x1); Canvas.SetTop(ellipse, y2); }

                    if (prev_Count_Children != MainCanvas.Children.Count)
                    {
                        MainCanvas.Children.RemoveAt(prev_Count_Children);
                    }
                    //ellipse.MouseEnter += UIElement_MouseEnter;
                    ellipse.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                    ellipse.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                    ellipse.MouseMove += UIElement_MouseMove;
                    //ellipse.MouseLeave += UIElement_MouseLeave;
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
                            X2 = x2 - 1,
                            Y2 = y2 - 1,
                            StrokeThickness = 4.0,
                            Stroke = stroke_Brush
                        };
                        if (line.X2 < line.X1)
                        {
                            line.X2 += 2;
                        }
                        if (line.Y2 < line.Y1)
                        {
                            line.Y2 += 2;
                        }

                        if (prev_Count_Children != MainCanvas.Children.Count)
                        {
                            MainCanvas.Children.RemoveAt(prev_Count_Children);
                        }
                        //line.MouseEnter += UIElement_MouseEnter;
                        line.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                        line.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                        line.MouseMove += UIElement_MouseMove;
                        //line.MouseLeave += UIElement_MouseLeave;
                        MainCanvas.Children.Add(line);
                    }
                }

                else if (polygon_Mode && polygon_Mode_Toggle)
                {
                    if ((prev_Poly_Count_Children >= 2) && (GetDistance(End_Point.X, End_Point.Y, Start_Point_Poly.X, Start_Point_Poly.Y) < 7.0))
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
                            X2 = x2 - 1,
                            Y2 = y2 - 1,
                            StrokeThickness = 4.0,
                            Stroke = stroke_Brush
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

                    // :TODO 
                    // Polygon
                    MainCanvas.Children.Add(line);
                }
            }
        }

        bool ui_LeftButton_Down = false;
        // 객체에 마우스를 놓고 클릭하면 드래그 가능하다.
        private void UIElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (edit_Mode)
            {
                if ((selected_Text != null) || (selected_Polygon != null) || (selected_Line != null) || (selected_Shape != null) || (selected_Image != null))
                {
                    left = right = top = bottom = false;
                    
                    double resize_x1 = Canvas.GetLeft(edit_Rect) + 5,
                        resize_y1 = Canvas.GetTop(edit_Rect) + 5,
                        resize_x2 = Canvas.GetLeft(edit_Rect) + edit_Rect.Width - 5,
                        resize_y2 = Canvas.GetTop(edit_Rect) + edit_Rect.Height - 5,
                        resize_mouse_x = e.GetPosition(MainCanvas).X,
                        resize_mouse_y = e.GetPosition(MainCanvas).Y;

                    if (Math.Abs(resize_mouse_x - resize_x1) < 17)
                    {
                        left = true;
                        drag_Mode = true;
                    }
                    else if (Math.Abs(resize_mouse_x - resize_x2) < 17)
                    {
                        right = true;
                        drag_Mode = true;
                    }
                    if (Math.Abs(resize_mouse_y - resize_y1) < 17)
                    {
                        top = true;
                        drag_Mode = true;
                    }
                    else if (Math.Abs(resize_mouse_y - resize_y2) < 17)
                    {
                        bottom = true;
                        drag_Mode = true;
                    }
                    
                    if (!(left || right || top || bottom))
                    {
                        if (selected_Text != null && sender == selected_Text)
                        {
                            drag_Mode = true;
                            selected_Text.CaptureMouse();
                            Start_Point_Shape.X = ((e.GetPosition(MainCanvas)).X - Canvas.GetLeft(sender as EditableTextBlock));
                            Start_Point_Shape.Y = ((e.GetPosition(MainCanvas)).Y - Canvas.GetTop(sender as EditableTextBlock));
                        }
                        //else if (select_Line_Mode && selected_Line != null)
                        else if (selected_Polygon != null && sender == selected_Polygon)
                        {
                            drag_Mode = true;
                            selected_Polygon.CaptureMouse();
                            Start_Point_Shape.X = ((e.GetPosition(MainCanvas)).X - Canvas.GetLeft(edit_Rect) + 5);
                            Start_Point_Shape.Y = ((e.GetPosition(MainCanvas)).Y - Canvas.GetTop(edit_Rect) + 5);
                        }
                        else if (selected_Line != null && sender == selected_Line)
                        {
                            drag_Mode = true;
                            selected_Line.CaptureMouse();
                            Start_Point_Shape.X = ((e.GetPosition(MainCanvas)).X - Canvas.GetLeft(edit_Rect) + 5);
                            Start_Point_Shape.Y = ((e.GetPosition(MainCanvas)).Y - Canvas.GetTop(edit_Rect) + 5);
                        }
                        //else if (select_Shape_Mode && selected_Shape != null)
                        else if (selected_Image != null && sender == selected_Image)
                        {
                            drag_Mode = true;
                            selected_Image.CaptureMouse();
                            Start_Point_Shape.X = ((e.GetPosition(MainCanvas)).X - Canvas.GetLeft(sender as Image));
                            Start_Point_Shape.Y = ((e.GetPosition(MainCanvas)).Y - Canvas.GetTop(sender as Image));
                        }
                        else if (selected_Shape != null && sender == selected_Shape)
                        {
                            drag_Mode = true;
                            selected_Shape.CaptureMouse();
                            Start_Point_Shape.X = ((e.GetPosition(MainCanvas)).X - Canvas.GetLeft(sender as Shape));
                            Start_Point_Shape.Y = ((e.GetPosition(MainCanvas)).Y - Canvas.GetTop(sender as Shape));
                        }

                        else
                        {
                            SetAllModeFalse();
                            SetAllMoveModeFalse();
                        }
                    }
                }
                else
                {
                    DoubleCollection dashes = new DoubleCollection();
                    dashes.Add(2); dashes.Add(2);

                    if (sender is EditableTextBlock)
                    {
                        selected_Text = sender as EditableTextBlock;
                        if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
                        edit_Rect = new Rectangle()
                        {
                            Width = selected_Text.Width + 10,
                            Height = selected_Text.Height + 10,
                            Stroke = new SolidColorBrush(Colors.Black),
                            StrokeDashArray = dashes
                        };
                        Canvas.SetLeft(edit_Rect, Canvas.GetLeft(selected_Text) - 5);
                        Canvas.SetTop(edit_Rect, Canvas.GetTop(selected_Text) - 5);
                        MainCanvas.Children.Add(edit_Rect);

                        if (selected_Text.FontWeight == FontWeights.Bold)
                        {
                            BoldButton.Background = new SolidColorBrush(Colors.DodgerBlue);
                            BoldButton_Clicked = true;
                        }
                        else
                        {
                            BoldButton.Background = new SolidColorBrush(Colors.White);
                            BoldButton_Clicked = false;
                        }
                        if (selected_Text.FontStyle == FontStyles.Italic)
                        {
                            ItalicButton.Background = new SolidColorBrush(Colors.DodgerBlue);
                            ItalicButton_Clicked = true;
                        }
                        else
                        {
                            ItalicButton.Background = new SolidColorBrush(Colors.White);
                            ItalicButton_Clicked = false;
                        }
                        if (selected_Text.TextDecorations.Count != 0)
                        {
                            UnderlineButton.Background = new SolidColorBrush(Colors.DodgerBlue);
                            UnderlineButton_Clicked = true;
                        }
                        else
                        {
                            UnderlineButton.Background = new SolidColorBrush(Colors.White);
                            UnderlineButton_Clicked = false;
                        }
                        if (selected_Text.Effect != null)
                        {
                            ShadowButton.Background = new SolidColorBrush(Colors.DodgerBlue);
                            ShadowButton_Clicked = true;
                        }
                        else
                        {
                            ShadowButton.Background = new SolidColorBrush(Colors.White);
                            ShadowButton_Clicked = false;
                        }
                        font_Family_Index = 0;
                        foreach (FontFamily fam in Fonts.SystemFontFamilies)
                        {
                            if (fam == selected_Text.FontFamily)
                            {
                                break;
                            }
                            font_Family_Index++;
                        }
                        FontFamily_Combobox.SelectedIndex = font_Family_Index;
                        font_Size_Index = 0;
                        for (int i = 10; i < 60; i=i+2)
                        {
                            if (Convert.ToDouble(i) == selected_Text.FontSize)
                            {
                                break;
                            }
                            font_Size_Index++;
                        }
                        FontSize_Combobox.SelectedIndex = font_Size_Index;
                    }
                    else if (sender is Polygon)
                    {
                        selected_Polygon = sender as Polygon;
                        if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);

                        double min_x = 2000, min_y = 2000, max_x = 0, max_y = 0;
                        foreach (Point p in selected_Polygon.Points)
                        {
                            if (min_x > p.X)
                                min_x = p.X;
                            if (max_x < p.X)
                                max_x = p.X;
                            if (min_y > p.Y)
                                min_y = p.Y;
                            if (max_y < p.Y)
                                max_y = p.Y;
                        }

                        edit_Rect = new Rectangle()
                        {
                            Width = max_x - min_x + 20,
                            Height = max_y - min_y + 20,
                            Stroke = new SolidColorBrush(Colors.White),
                            StrokeDashArray = dashes
                        };
                        
                        Canvas.SetLeft(edit_Rect, min_x - 5);
                        Canvas.SetTop(edit_Rect, min_y - 5);
                        
                        MainCanvas.Children.Add(edit_Rect);

                        selected_Polygon.StrokeDashArray = dashes;
                    }
                    else if (sender is Line)
                    {
                        selected_Line = sender as Line;
                        if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);

                        double min_x, min_y, max_x, max_y;
                        if (selected_Line.X1 > selected_Line.X2)
                        {
                            min_x = selected_Line.X2;
                            max_x = selected_Line.X1;
                        }
                        else
                        {
                            min_x = selected_Line.X1;
                            max_x = selected_Line.X2;
                        }
                        if (selected_Line.Y1 > selected_Line.Y2)
                        {
                            min_y = selected_Line.Y2;
                            max_y = selected_Line.Y1;
                        }
                        else
                        {
                            min_y = selected_Line.Y1;
                            max_y = selected_Line.Y2;
                        }

                        edit_Rect = new Rectangle()
                        {
                            Width = max_x - min_x + 10,
                            Height = max_x - min_x + 10,
                            Stroke = new SolidColorBrush(Colors.Black),
                            StrokeDashArray = dashes
                        };
                        Canvas.SetLeft(edit_Rect, min_x - 5);
                        Canvas.SetTop(edit_Rect, min_y - 5);
                        
                        MainCanvas.Children.Add(edit_Rect);
                        selected_Line.StrokeDashArray = dashes;
                    }
                    else if (sender is Image)
                    {
                        selected_Image = sender as Image;
                        if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
                        edit_Rect = new Rectangle()
                        {
                            Width = selected_Image.Width + 10,
                            Height = selected_Image.Height + 10,
                            Stroke = new SolidColorBrush(Colors.Black),
                            StrokeDashArray = dashes
                        };
                        Canvas.SetLeft(edit_Rect, Canvas.GetLeft(selected_Image) - 5);
                        Canvas.SetTop(edit_Rect, Canvas.GetTop(selected_Image) - 5);
                        
                        MainCanvas.Children.Add(edit_Rect);
                    }
                    else if (sender is Shape)
                    {
                        selected_Shape = sender as Shape;
                        if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
                        edit_Rect = new Rectangle()
                        {
                            Width = selected_Shape.Width + 10,
                            Height = selected_Shape.Height + 10,
                            Stroke = new SolidColorBrush(Colors.Black),
                            StrokeDashArray = dashes
                        };
                        Canvas.SetLeft(edit_Rect, Canvas.GetLeft(selected_Shape) - 5);
                        Canvas.SetTop(edit_Rect, Canvas.GetTop(selected_Shape) - 5);
                        
                        MainCanvas.Children.Add(edit_Rect);
                    }
                }
            }
            else
            {
                if (!(rectangle_Mode || ellipse_Mode || text_Mode || line_Mode || polygon_Mode || pen_Mode || image_Mode))
                {
                    edit_Mode_Ready = true;

                    DoubleCollection dashes = new DoubleCollection();
                    dashes.Add(2); dashes.Add(2);

                    if (sender is EditableTextBlock)
                    {
                        selected_Text = sender as EditableTextBlock;
                        Begin_Point_Shape = e.GetPosition(sender as EditableTextBlock);

                        if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
                        edit_Rect = new Rectangle()
                        {
                            Width = selected_Text.Width + 10,
                            Height = selected_Text.Height + 10,
                            Stroke = new SolidColorBrush(Colors.Black),
                            StrokeDashArray = dashes
                        };
                        Canvas.SetLeft(edit_Rect, Canvas.GetLeft(selected_Text) - 5);
                        Canvas.SetTop(edit_Rect, Canvas.GetTop(selected_Text) - 5);
                        
                        MainCanvas.Children.Add(edit_Rect);

                        if (selected_Text.FontWeight == FontWeights.Bold)
                        {
                            BoldButton.Background = new SolidColorBrush(Colors.DodgerBlue);
                            BoldButton_Clicked = true;
                        }
                        else
                        {
                            BoldButton.Background = new SolidColorBrush(Colors.White);
                            BoldButton_Clicked = false;
                        }
                        if (selected_Text.FontStyle == FontStyles.Italic)
                        {
                            ItalicButton.Background = new SolidColorBrush(Colors.DodgerBlue);
                            ItalicButton_Clicked = true;
                        }
                        else
                        {
                            ItalicButton.Background = new SolidColorBrush(Colors.White);
                            ItalicButton_Clicked = false;
                        }
                        if (selected_Text.TextDecorations.Count != 0)
                        {
                            UnderlineButton.Background = new SolidColorBrush(Colors.DodgerBlue);
                            UnderlineButton_Clicked = true;
                        }
                        else
                        {
                            UnderlineButton.Background = new SolidColorBrush(Colors.White);
                            UnderlineButton_Clicked = false;
                        }
                        if (selected_Text.Effect != null)
                        {
                            ShadowButton.Background = new SolidColorBrush(Colors.DodgerBlue);
                            ShadowButton_Clicked = true;
                        }
                        else
                        {
                            ShadowButton.Background = new SolidColorBrush(Colors.White);
                            ShadowButton_Clicked = false;
                        }
                        font_Family_Index = 0;
                        foreach (FontFamily fam in Fonts.SystemFontFamilies)
                        {
                            if (fam == selected_Text.FontFamily)
                            {
                                break;
                            }
                            font_Family_Index++;
                        }
                        FontFamily_Combobox.SelectedIndex = font_Family_Index;
                        font_Size_Index = 0;
                        for (int i = 10; i < 60; i = i + 2)
                        {
                            if (Convert.ToDouble(i) == selected_Text.FontSize)
                            {
                                break;
                            }
                            font_Size_Index++;
                        }
                        FontSize_Combobox.SelectedIndex = font_Size_Index;
                    }
                    else if (sender is Polygon)
                    {
                        selected_Polygon = sender as Polygon;
                        Begin_Point_Shape = e.GetPosition(sender as Polygon);

                        if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
                        double min_x = 2000, min_y = 2000, max_x = 0, max_y = 0;
                        foreach (Point p in selected_Polygon.Points)
                        {
                            if (min_x > p.X)
                                min_x = p.X;
                            if (max_x < p.X)
                                max_x = p.X;
                            if (min_y > p.Y)
                                min_y = p.Y;
                            if (max_y < p.Y)
                                max_y = p.Y;
                        }

                        edit_Rect = new Rectangle()
                        {
                            Width = max_x - min_x + 10,
                            Height = max_y - min_y + 10,
                            Stroke = new SolidColorBrush(Colors.Black),
                            StrokeDashArray = dashes
                        };

                        Canvas.SetLeft(edit_Rect, min_x - 5);
                        Canvas.SetTop(edit_Rect, min_y - 5);
                        
                        MainCanvas.Children.Add(edit_Rect);

                        selected_Polygon.StrokeDashArray = dashes;
                    }
                    else if (sender is Line)
                    {
                        selected_Line = sender as Line;
                        Begin_Point_Shape = e.GetPosition(sender as Line);

                        if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
                        double min_x, min_y, max_x, max_y;
                        if (selected_Line.X1 > selected_Line.X2)
                        {
                            min_x = selected_Line.X2;
                            max_x = selected_Line.X1;
                        }
                        else
                        {
                            min_x = selected_Line.X1;
                            max_x = selected_Line.X2;
                        }
                        if (selected_Line.Y1 > selected_Line.Y2)
                        {
                            min_y = selected_Line.Y2;
                            max_y = selected_Line.Y1;
                        }
                        else
                        {
                            min_y = selected_Line.Y1;
                            max_y = selected_Line.Y2;
                        }

                        edit_Rect = new Rectangle()
                        {
                            Width = max_x - min_x + 10,
                            Height = max_x - min_x + 10,
                            Stroke = new SolidColorBrush(Colors.Black),
                            StrokeDashArray = dashes
                        };

                        Canvas.SetLeft(edit_Rect, min_x - 5);
                        Canvas.SetTop(edit_Rect, min_y - 5);
                        
                        MainCanvas.Children.Add(edit_Rect);
                        selected_Line.StrokeDashArray = dashes;
                    }
                    else if (sender is Image)
                    {
                        selected_Image = sender as Image;
                        Begin_Point_Shape = e.GetPosition(sender as Image);

                        if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
                        edit_Rect = new Rectangle()
                        {
                            Width = selected_Image.Width + 10,
                            Height = selected_Image.Height + 10,
                            Stroke = new SolidColorBrush(Colors.Black),
                            StrokeDashArray = dashes
                        };
                        Canvas.SetLeft(edit_Rect, Canvas.GetLeft(selected_Image) - 5);
                        Canvas.SetTop(edit_Rect, Canvas.GetTop(selected_Image) - 5);
                        
                        MainCanvas.Children.Add(edit_Rect);
                    }
                    else if (sender is Shape)
                    {
                        selected_Shape = sender as Shape;
                        Begin_Point_Shape = e.GetPosition(sender as Shape);

                        if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
                        edit_Rect = new Rectangle()
                        {
                            Width = selected_Shape.Width + 10,
                            Height = selected_Shape.Height + 10,
                            Stroke = new SolidColorBrush(Colors.Black),
                            StrokeDashArray = dashes
                        };
                        Canvas.SetLeft(edit_Rect, Canvas.GetLeft(selected_Shape) - 5);
                        Canvas.SetTop(edit_Rect, Canvas.GetTop(selected_Shape) - 5);
                        
                        MainCanvas.Children.Add(edit_Rect);
                    }
                }
            }
            Image image_renew = RenderCanvas(MainCanvas);
            ((SlideInfo)(Slide_ListView.Items[Slide_ListView.SelectedIndex])).Source = image_renew.Source;
            ui_LeftButton_Down = true;
        }

        // 객체에 마우스를 놓고 떼면 드래그 설정이 풀린다.
        // 이 때 객체를 선택했는지 안했는지를 설정한다.
        private void UIElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            left = right = top = bottom = false;
            if (edit_Mode_Ready)
            {
                bool found = false;
                if ((!(selected_Text == (sender as EditableTextBlock))) && selected_Text!=null)
                {
                    selected_Text = null;
                    found = true;
                    if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
                    edit_Rect = null;

                    BoldButton.Background = new SolidColorBrush(Colors.White);
                    ItalicButton.Background = new SolidColorBrush(Colors.White);
                    UnderlineButton.Background = new SolidColorBrush(Colors.White);
                    ShadowButton.Background = new SolidColorBrush(Colors.White);
                    BoldButton_Clicked = ItalicButton_Clicked = UnderlineButton_Clicked = ShadowButton_Clicked = false;
                }
                if ((!(selected_Polygon == (sender as Polygon))) && selected_Polygon!=null)
                {
                    selected_Polygon.StrokeDashArray = null;
                    selected_Polygon = null;
                    found = true;
                    if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
                    edit_Rect = null;
                }
                if ((!(selected_Line == (sender as Line))) && selected_Line!=null)
                {
                    selected_Line.StrokeDashArray = null;
                    selected_Line = null;
                    found = true;
                    if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
                    edit_Rect = null;
                }
                if ((!(selected_Image == (sender as Image))) && selected_Image!=null)
                {
                    selected_Image = null;
                    found = true;
                    if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
                    edit_Rect = null;
                }
                if ((!(selected_Shape == (sender as Shape))) && selected_Shape!=null)
                {
                    selected_Shape = null;
                    found = true;
                    if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
                    edit_Rect = null;
                }

                if (!found)
                    edit_Mode = true;
                edit_Mode_Ready = false;
            }

            else if (edit_Mode && (((selected_Text) == (sender as EditableTextBlock)) || ((selected_Polygon) == (sender as Polygon))
                || ((selected_Line) == (sender as Line)) || ((selected_Image) == (sender as Image)) || ((selected_Shape) == (sender as Shape))))
            {
                DoubleCollection dashes = new DoubleCollection();
                dashes.Add(2); dashes.Add(2);
                //if (select_Text_Mode && selected_Text != null)
                if (selected_Text != null)
                {
                    drag_Mode = false;
                    select_Mode = !select_Mode;
                    selected_Text.ReleaseMouseCapture();
                    selected_Text = sender as EditableTextBlock;
                    if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
                    edit_Rect = new Rectangle()
                    {
                        Width = selected_Text.Width + 10,
                        Height = selected_Text.Height + 10,
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeDashArray = dashes
                    };
                    Canvas.SetLeft(edit_Rect, Canvas.GetLeft(selected_Text) - 5);
                    Canvas.SetTop(edit_Rect, Canvas.GetTop(selected_Text) - 5);
                    
                    MainCanvas.Children.Add(edit_Rect);
                    if (selected_Text.FontWeight == FontWeights.Bold)
                    {
                        BoldButton.Background = new SolidColorBrush(Colors.DodgerBlue);
                        BoldButton_Clicked = true;
                    }
                    else
                    {
                        BoldButton.Background = new SolidColorBrush(Colors.White);
                        BoldButton_Clicked = false;
                    }
                    if (selected_Text.FontStyle == FontStyles.Italic)
                    {
                        ItalicButton.Background = new SolidColorBrush(Colors.DodgerBlue);
                        ItalicButton_Clicked = true;
                    }
                    else
                    {
                        ItalicButton.Background = new SolidColorBrush(Colors.White);
                        ItalicButton_Clicked = false;
                    }
                    if (selected_Text.TextDecorations.Count != 0)
                    {
                        UnderlineButton.Background = new SolidColorBrush(Colors.DodgerBlue);
                        UnderlineButton_Clicked = true;
                    }
                    else
                    {
                        UnderlineButton.Background = new SolidColorBrush(Colors.White);
                        UnderlineButton_Clicked = false;
                    }
                    if (selected_Text.Effect != null)
                    {
                        ShadowButton.Background = new SolidColorBrush(Colors.DodgerBlue);
                        ShadowButton_Clicked = true;
                    }
                    else
                    {
                        ShadowButton.Background = new SolidColorBrush(Colors.White);
                        ShadowButton_Clicked = false;
                    }
                    font_Family_Index = 0;
                    foreach (FontFamily fam in Fonts.SystemFontFamilies)
                    {
                        if (fam == selected_Text.FontFamily)
                        {
                            break;
                        }
                        font_Family_Index++;
                    }
                    FontFamily_Combobox.SelectedIndex = font_Family_Index;
                    font_Size_Index = 0;
                    for (int i = 10; i < 60; i = i + 2)
                    {
                        if (Convert.ToDouble(i) == selected_Text.FontSize)
                        {
                            break;
                        }
                        font_Size_Index++;
                    }
                    FontSize_Combobox.SelectedIndex = font_Size_Index;
                }
                if (selected_Polygon != null)
                {
                    drag_Mode = false;
                    select_Mode = !select_Mode;
                    selected_Polygon.ReleaseMouseCapture();
                    if(sender!=null) selected_Polygon = sender as Polygon;
                    if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
                    
                    double min_x = 2000, min_y = 2000, max_x = 0, max_y = 0;
                    foreach (Point p in selected_Polygon.Points)
                    {
                        if (min_x > p.X)
                            min_x = p.X;
                        if (max_x < p.X)
                            max_x = p.X;
                        if (min_y > p.Y)
                            min_y = p.Y;
                        if (max_y < p.Y)
                            max_y = p.Y;
                    }

                    edit_Rect = new Rectangle()
                    {
                        Width = max_x - min_x + 10,
                        Height = max_y - min_y + 10,
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeDashArray = dashes
                    };

                    Canvas.SetLeft(edit_Rect, min_x - 5);
                    Canvas.SetTop(edit_Rect, min_y - 5);
                    
                    MainCanvas.Children.Add(edit_Rect);

                    selected_Polygon.StrokeDashArray = dashes;
                }
                //else if (select_Line_Mode && selected_Line != null)
                else if (selected_Line != null)
                {
                    drag_Mode = false;
                    select_Mode = !select_Mode;
                    selected_Line.ReleaseMouseCapture();
                    if (sender != null) selected_Line = sender as Line;
                    if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);

                    double min_x, min_y, max_x, max_y;
                    if (selected_Line.X1 > selected_Line.X2)
                    {
                        min_x = selected_Line.X2;
                        max_x = selected_Line.X1;
                    }
                    else
                    {
                        min_x = selected_Line.X1;
                        max_x = selected_Line.X2;
                    }
                    if (selected_Line.Y1 > selected_Line.Y2)
                    {
                        min_y = selected_Line.Y2;
                        max_y = selected_Line.Y1;
                    }
                    else
                    {
                        min_y = selected_Line.Y1;
                        max_y = selected_Line.Y2;
                    }

                    edit_Rect = new Rectangle()
                    {
                        Width = max_x-min_x + 10,
                        Height = max_y-min_y + 10,
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeDashArray = dashes
                    };

                    Canvas.SetLeft(edit_Rect, min_x - 5);
                    Canvas.SetTop(edit_Rect, min_y - 5);
                    
                    MainCanvas.Children.Add(edit_Rect);
                    selected_Line.StrokeDashArray = dashes;
                }
                else if (selected_Image != null)
                {
                    drag_Mode = false;
                    select_Mode = !select_Mode;
                    selected_Image.ReleaseMouseCapture();
                    selected_Image = sender as Image;
                    if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
                    edit_Rect = new Rectangle()
                    {
                        Width = selected_Image.Width + 10,
                        Height = selected_Image.Height + 10,
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeDashArray = dashes
                    };
                    Canvas.SetLeft(edit_Rect, Canvas.GetLeft(selected_Image) - 5);
                    Canvas.SetTop(edit_Rect, Canvas.GetTop(selected_Image) - 5);
                    
                    MainCanvas.Children.Add(edit_Rect);
                }

                //else if (select_Shape_Mode && selected_Shape != null)
                else if (selected_Shape != null)
                {
                    drag_Mode = false;
                    select_Mode = !select_Mode;
                    selected_Shape.ReleaseMouseCapture();
                    selected_Shape = sender as Shape;
                    if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
                    edit_Rect = new Rectangle()
                    {
                        Width = selected_Shape.Width + 10,
                        Height = selected_Shape.Height + 10,
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeDashArray = dashes
                    };
                    Canvas.SetLeft(edit_Rect, Canvas.GetLeft(selected_Shape) - 5);
                    Canvas.SetTop(edit_Rect, Canvas.GetTop(selected_Shape) - 5);
                    
                    MainCanvas.Children.Add(edit_Rect);
                }
            }
            Image image_renew = RenderCanvas(MainCanvas);
            ((SlideInfo)(Slide_ListView.Items[Slide_ListView.SelectedIndex])).Source = image_renew.Source;
            if(rectangle_Mode||ellipse_Mode) SetAllModeFalse();
            ui_LeftButton_Down = false;
        }

        double m_left;
        double m_top;
        bool left = false, right = false, top = false, bottom = false;
        private void UIElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsSelectMode() || !drag_Mode) return;
            if (!ui_LeftButton_Down) return;
            if ((!edit_Mode) && edit_Mode_Ready) return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (edit_Rect != null)
                {
                    double x1 = Canvas.GetLeft(edit_Rect) + 5,
                        y1 = Canvas.GetTop(edit_Rect) + 5,
                        x2 = Canvas.GetLeft(edit_Rect) + edit_Rect.Width - 5,
                        y2 = Canvas.GetTop(edit_Rect) + edit_Rect.Height - 5,
                        mouse_x = e.GetPosition(MainCanvas).X,
                        mouse_y = e.GetPosition(MainCanvas).Y;
                    if (selected_Text != null)
                    {
                        x1 = Canvas.GetLeft(selected_Text);
                        y1 = Canvas.GetTop(selected_Text);
                        x2 = Canvas.GetLeft(selected_Text) + selected_Text.Width;
                        y2 = Canvas.GetTop(selected_Text) + selected_Text.Height;
                    }
                    else if (selected_Image!=null)
                    {
                        x1 = Canvas.GetLeft(selected_Image);
                        y1 = Canvas.GetTop(selected_Image);
                        x2 = Canvas.GetLeft(selected_Image) + selected_Image.Width;
                        y2 = Canvas.GetTop(selected_Image) + selected_Image.Height;
                    }
                    else if(selected_Shape!=null)
                    {
                        x1 = Canvas.GetLeft(selected_Shape);
                        y1 = Canvas.GetTop(selected_Shape);
                        x2 = Canvas.GetLeft(selected_Shape) + selected_Shape.Width;
                        y2 = Canvas.GetTop(selected_Shape) + selected_Shape.Height;
                    }

                    if (selected_Text != null)
                    {
                        if (left)
                        {
                            edit_Rect.Width = x2 - mouse_x + 17;
                            selected_Text.Width = edit_Rect.Width - 10;
                            Canvas.SetLeft(edit_Rect, mouse_x - 12);
                            Canvas.SetLeft(selected_Text, mouse_x - 7);
                            Canvas.SetRight(edit_Rect, x2 + 5);
                            Canvas.SetRight(selected_Text, x2);
                        }
                        else if (right)
                        {
                            edit_Rect.Width = mouse_x - x1 + 17;
                            selected_Text.Width = edit_Rect.Width - 10;
                            Canvas.SetLeft(edit_Rect, x1 - 5);
                            Canvas.SetLeft(selected_Text, x1);
                            Canvas.SetRight(edit_Rect, mouse_x + 12);
                            Canvas.SetRight(selected_Text, mouse_x + 7);
                        }
                        if (top)
                        {
                            edit_Rect.Height = y2 - mouse_y + 17;
                            selected_Text.Height = edit_Rect.Height - 10;
                            Canvas.SetTop(edit_Rect, mouse_y - 12);
                            Canvas.SetTop(selected_Text, mouse_y - 7);
                            Canvas.SetBottom(edit_Rect, y2 + 5);
                            Canvas.SetBottom(selected_Text, y2);
                        }
                        else if (bottom)
                        {
                            edit_Rect.Height = mouse_y - y1 + 17;
                            selected_Text.Height = edit_Rect.Height - 10;
                            Canvas.SetTop(edit_Rect, y1 - 5);
                            Canvas.SetTop(selected_Text, y1);
                            Canvas.SetBottom(edit_Rect, mouse_y + 12);
                            Canvas.SetBottom(selected_Text, mouse_y + 7);
                        }
                    }
                    else if (selected_Polygon != null)
                    {
                        int i = 0;
                        PointCollection p_collection = new PointCollection();
                        Point[] points = new Point[selected_Polygon.Points.Count];
                        selected_Polygon.Points.CopyTo(points, 0);
                        bool found = false;
                        foreach (Point p in selected_Polygon.Points)
                        {
                            if ((!found) && ((Math.Abs(p.X - mouse_x) + Math.Abs(p.Y - mouse_y)) < 4))
                            {
                                points[i].X = mouse_x;
                                points[i].Y = mouse_y;
                                found = true;
                            }
                            p_collection.Add(points[i]);
                            i++;
                        }
                        selected_Polygon.Points = p_collection;
                    }
                    else if (selected_Line != null)
                    {
                        if ((Math.Abs(selected_Line.X1 - mouse_x) + Math.Abs(selected_Line.Y1 - mouse_y)) < 4)
                        {
                            selected_Line.X1 = mouse_x;
                            selected_Line.Y1 = mouse_y;
                        }
                        else if ((Math.Abs(selected_Line.X2 - mouse_x) + Math.Abs(selected_Line.Y2 - mouse_y)) < 4)
                        {
                            selected_Line.X1 = mouse_x;
                            selected_Line.Y1 = mouse_y;
                        }
                    }
                    else if (selected_Image != null)
                    {
                        if (left)
                        {
                            edit_Rect.Width = x2 - mouse_x + 17;
                            selected_Image.Width = edit_Rect.Width - 10;
                            Canvas.SetLeft(edit_Rect, mouse_x - 12);
                            Canvas.SetLeft(selected_Image, mouse_x - 7);
                            Canvas.SetRight(edit_Rect, x2 + 5);
                            Canvas.SetRight(selected_Image, x2);
                        }
                        else if (right)
                        {
                            edit_Rect.Width = mouse_x - x1 + 17;
                            selected_Image.Width = edit_Rect.Width - 10;
                            Canvas.SetLeft(edit_Rect, x1 - 5);
                            Canvas.SetLeft(selected_Image, x1);
                            Canvas.SetRight(edit_Rect, mouse_x + 12);
                            Canvas.SetRight(selected_Image, mouse_x + 7);
                        }
                        if (top)
                        {
                            edit_Rect.Height = y2 - mouse_y + 17;
                            selected_Image.Height = edit_Rect.Height - 10;
                            Canvas.SetTop(edit_Rect, mouse_y - 12);
                            Canvas.SetTop(selected_Image, mouse_y - 7);
                            Canvas.SetBottom(edit_Rect, y2 + 5);
                            Canvas.SetBottom(selected_Image, y2);
                        }
                        else if (bottom)
                        {
                            edit_Rect.Height = mouse_y - y1 + 17;
                            selected_Image.Height = edit_Rect.Height - 10;
                            Canvas.SetTop(edit_Rect, y1 - 5);
                            Canvas.SetTop(selected_Image, y1);
                            Canvas.SetBottom(edit_Rect, mouse_y + 12);
                            Canvas.SetBottom(selected_Image, mouse_y + 7);
                        }
                    }
                    else if (selected_Shape != null)
                    {
                        if (left)
                        {
                            edit_Rect.Width = x2 - mouse_x + 17;
                            selected_Shape.Width = edit_Rect.Width - 10;
                            Canvas.SetLeft(edit_Rect, mouse_x - 12);
                            Canvas.SetLeft(selected_Shape, mouse_x - 7);
                            Canvas.SetRight(edit_Rect, x2 + 5);
                            Canvas.SetRight(selected_Shape, x2);
                        }
                        else if (right)
                        {
                            edit_Rect.Width = mouse_x - x1 + 17;
                            selected_Shape.Width = edit_Rect.Width - 10;
                            Canvas.SetLeft(edit_Rect, x1 - 5);
                            Canvas.SetLeft(selected_Shape, x1);
                            Canvas.SetRight(edit_Rect, mouse_x + 12);
                            Canvas.SetRight(selected_Shape, mouse_x + 7);
                        }
                        if (top)
                        {
                            edit_Rect.Height = y2 - mouse_y + 17;
                            selected_Shape.Height = edit_Rect.Height - 10;
                            Canvas.SetTop(edit_Rect, mouse_y - 12);
                            Canvas.SetTop(selected_Shape, mouse_y - 7);
                            Canvas.SetBottom(edit_Rect, y2 + 5);
                            Canvas.SetBottom(selected_Shape, y2);
                        }
                        else if (bottom)
                        {
                            edit_Rect.Height = mouse_y - y1 + 17;
                            selected_Shape.Height = edit_Rect.Height - 10;
                            Canvas.SetTop(edit_Rect, y1 - 5);
                            Canvas.SetTop(selected_Shape, y1);
                            Canvas.SetBottom(edit_Rect, mouse_y + 12);
                            Canvas.SetBottom(selected_Shape, mouse_y + 7);
                        }
                    }
                }
                if (left || right || top || bottom) return;

                Point End_Point = e.GetPosition(MainCanvas);
                if (selected_Text != null)
                {
                    if (End_Point.X < Start_Point_Shape.X)
                        m_left = 0;
                    else if (MainCanvas.Width - End_Point.X > selected_Text.Width - Start_Point_Shape.X)
                        m_left = (End_Point.X - Start_Point_Shape.X);
                    else
                        m_left = MainCanvas.ActualWidth - selected_Text.Width;

                    if (End_Point.Y < Start_Point_Shape.Y)
                        m_top = 0;
                    else if (MainCanvas.Height - End_Point.Y > selected_Text.Height - Start_Point_Shape.Y)
                        m_top = (End_Point.Y - Start_Point_Shape.Y);
                    else
                        m_top = MainCanvas.ActualHeight - selected_Text.Height;

                    Canvas.SetLeft(selected_Text, m_left);
                    Canvas.SetTop(selected_Text, m_top);
                    Canvas.SetLeft(edit_Rect, m_left - 5);
                    Canvas.SetTop(edit_Rect, m_top - 5);
                }
                else if (selected_Polygon != null)
                {
                    double min_x = 2000, min_y = 2000, max_x = 0, max_y = 0;
                    foreach (Point p in selected_Polygon.Points)
                    {
                        if (min_x > p.X)
                            min_x = p.X;
                        if (max_x < p.X)
                            max_x = p.X;
                        if (min_y > p.Y)
                            min_y = p.Y;
                        if (max_y < p.Y)
                            max_y = p.Y;
                    }

                    if (End_Point.X < Start_Point_Shape.X)
                        m_left = 0;
                    else if ((MainCanvas.Width - End_Point.X) > (max_x - min_x - Start_Point_Shape.X))
                        m_left = (End_Point.X - Start_Point_Shape.X);
                    else
                        m_left = MainCanvas.ActualWidth - selected_Polygon.Width;

                    if (End_Point.Y < Start_Point_Shape.Y)
                        m_top = 0;
                    else if ((MainCanvas.Height - End_Point.Y) > (max_y - min_y - Start_Point_Shape.Y))
                        m_top = (End_Point.Y - Start_Point_Shape.Y);
                    else
                        m_top = MainCanvas.ActualHeight - selected_Polygon.Height;

                    double previous_x = Canvas.GetLeft(edit_Rect),
                        previous_y = Canvas.GetTop(edit_Rect);
                    Canvas.SetLeft(edit_Rect, m_left - 5);
                    Canvas.SetTop(edit_Rect, m_top - 5);
                    double diff_x = Canvas.GetLeft(edit_Rect) - previous_x,
                        diff_y = Canvas.GetTop(edit_Rect) - previous_y;

                    Point[] copy_points = new Point[selected_Polygon.Points.Count];
                    PointCollection copy_collection = new PointCollection();
                    selected_Polygon.Points.CopyTo(copy_points, 0);
                    for (int i = 0; i < selected_Polygon.Points.Count; i++ )
                    {
                        copy_points[i].X = copy_points[i].X + diff_x;
                        copy_points[i].Y = copy_points[i].Y + diff_y;
                        copy_collection.Add(copy_points[i]);
                    }
                    selected_Polygon.Points = copy_collection;
                }

                else if (selected_Line != null)
                {
                    double min_x, min_y, max_x, max_y;
                    if (selected_Line.X1 > selected_Line.X2)
                    {
                        min_x = selected_Line.X2;
                        max_x = selected_Line.X1;
                    }
                    else
                    {
                        min_x = selected_Line.X1;
                        max_x = selected_Line.X2;
                    }
                    if (selected_Line.Y1 > selected_Line.Y2)
                    {
                        min_y = selected_Line.Y2;
                        max_y = selected_Line.Y1;
                    }
                    else
                    {
                        min_y = selected_Line.Y1;
                        max_y = selected_Line.Y2;
                    }

                    if (End_Point.X < Start_Point_Shape.X)
                        m_left = 0;
                    else if (MainCanvas.Width - End_Point.X > (max_x - min_x - Start_Point_Shape.X))
                        m_left = (End_Point.X - Start_Point_Shape.X);
                    else
                        m_left = MainCanvas.ActualWidth - selected_Line.Width;

                    if (End_Point.Y < Start_Point_Shape.Y)
                        m_top = 0;
                    else if (MainCanvas.Height - End_Point.Y > (max_y - min_y - Start_Point_Shape.Y))
                        m_top = (End_Point.X - Start_Point_Shape.Y);
                    else
                        m_top = MainCanvas.ActualHeight - selected_Line.Height;

                    double previous_x = Canvas.GetLeft(edit_Rect),
                        previous_y = Canvas.GetTop(edit_Rect);
                    Canvas.SetLeft(edit_Rect, m_left - 5);
                    Canvas.SetTop(edit_Rect, m_top - 5);
                    double diff_x = Canvas.GetLeft(edit_Rect) - previous_x,
                        diff_y = Canvas.GetTop(edit_Rect) - previous_y;

                    selected_Line.X1 = selected_Line.X1 + diff_x;
                    selected_Line.X2 = selected_Line.X2 + diff_x;
                    selected_Line.Y1 = selected_Line.Y1 + diff_y;
                    selected_Line.Y2 = selected_Line.Y2 + diff_y;
                }
                else if (selected_Image != null)
                {
                    if (End_Point.X < Start_Point_Shape.X)
                        m_left = 0;
                    else if (MainCanvas.Width - End_Point.X > selected_Image.Width - Start_Point_Shape.X)
                        m_left = (End_Point.X - Start_Point_Shape.X);
                    else
                        m_left = MainCanvas.ActualWidth - selected_Image.Width;

                    if (End_Point.Y < Start_Point_Shape.Y)
                        m_top = 0;
                    else if (MainCanvas.Height - End_Point.Y > selected_Image.Height - Start_Point_Shape.Y)
                        m_top = (End_Point.Y - Start_Point_Shape.Y);
                    else
                        m_top = MainCanvas.ActualHeight - selected_Image.Height;

                    Canvas.SetLeft(selected_Image, m_left);
                    Canvas.SetTop(selected_Image, m_top);
                    Canvas.SetLeft(edit_Rect, m_left - 5);
                    Canvas.SetTop(edit_Rect, m_top - 5);
                }
                else if (selected_Shape != null)
                {
                    if (End_Point.X < Start_Point_Shape.X)
                        m_left = 0;
                    else if (MainCanvas.Width - End_Point.X > selected_Shape.Width - Start_Point_Shape.X)
                        m_left = (End_Point.X - Start_Point_Shape.X);
                    else
                        m_left = MainCanvas.ActualWidth - selected_Shape.Width;

                    if (End_Point.Y < Start_Point_Shape.Y)
                        m_top = 0;
                    else if (MainCanvas.Height - End_Point.Y > selected_Shape.Height - Start_Point_Shape.Y)
                        m_top = (End_Point.Y - Start_Point_Shape.Y);
                    else
                        m_top = MainCanvas.ActualHeight - selected_Shape.Height;

                    Canvas.SetLeft(selected_Shape, m_left);
                    Canvas.SetTop(selected_Shape, m_top);
                    Canvas.SetLeft(edit_Rect, m_left - 5);
                    Canvas.SetTop(edit_Rect, m_top - 5);
                }
            }
        }

        #endregion

        #region 버튼Boolean Settion
        void SetAllModeFalse()
        {
            pen_Mode = text_Mode = line_Mode = rectangle_Mode = polygon_Mode = image_Mode = ellipse_Mode = eraseforpen_Mode = false;
            if (selected_Polygon != null) selected_Polygon.StrokeDashArray = null;
            if (selected_Line != null) selected_Line.StrokeDashArray = null;
            BoldButton.Background = new SolidColorBrush(Colors.White);
            ItalicButton.Background = new SolidColorBrush(Colors.White);
            UnderlineButton.Background = new SolidColorBrush(Colors.White);
            ShadowButton.Background = new SolidColorBrush(Colors.White);
            BoldButton_Clicked = ItalicButton_Clicked = UnderlineButton_Clicked = ShadowButton_Clicked = false;
            selected_Text = null;
            selected_Polygon = null;
            selected_Line = null;
            selected_Image = null;
            selected_Shape = null;
            if (edit_Rect != null) MainCanvas.Children.Remove(edit_Rect);
            edit_Rect = null;
            edit_Mode_Ready = edit_Mode = false;
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


        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            SetAllModeFalse();
        }

        private void Ellipse_Click(object sender, RoutedEventArgs e)
        {
            SetAllModeFalse();
            ellipse_Mode = true;
        }

        private void ErasePen_Click(object sender, RoutedEventArgs e)
        {
            SetAllModeFalse();
            eraseforpen_Mode = true;
        }

        void SetAllMoveModeFalse()
        {
            pen_Move_Mode = text_Move_Mode = line_Move_Mode = rectangle_Move_Mode = polygon_Move_Mode = image_Move_Mode = ellipse_Move_Mode = eraseforpen_Mode = false;
        }


        #endregion

        private bool IsSelectMode()
        {
            return !(pen_Mode || text_Mode || line_Mode || rectangle_Mode || polygon_Mode || image_Mode || ellipse_Mode || eraseforpen_Mode);
        }

        private double GetDistance(double x1, double y1, double x2, double y2)
        {
            double d = 0;
            d = Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
            return d;
        }
        private void HomeMenu_Click(object sender, RoutedEventArgs e)
        {
            AllMenuBorderToWhite();
            AllMenuGridToCollapsed();
            HomeMenu.BorderBrush = new SolidColorBrush(Colors.LightGray);
            HomeMenu_Grid.Visibility = System.Windows.Visibility.Visible;
        }

        private void InsertMenu_Click(object sender, RoutedEventArgs e)
        {
            AllMenuBorderToWhite();
            AllMenuGridToCollapsed();
            InsertMenu.BorderBrush = new SolidColorBrush(Colors.LightGray);
            InsertMenu_Grid.Visibility = System.Windows.Visibility.Visible;
        }

        private void AnimationMenu_Click(object sender, RoutedEventArgs e)
        {
            AllMenuBorderToWhite();
            AllMenuGridToCollapsed();
            AnimationMenu.BorderBrush = new SolidColorBrush(Colors.LightGray);
            AnimationMenu_Grid.Visibility = System.Windows.Visibility.Visible;
        }

        void AllMenuBorderToWhite()
        {
            HomeMenu.BorderBrush = InsertMenu.BorderBrush = AnimationMenu.BorderBrush = new SolidColorBrush(Colors.White);
        }

        void AllMenuGridToCollapsed()
        {
            HomeMenu_Grid.Visibility = InsertMenu_Grid.Visibility = AnimationMenu_Grid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void NewSlideButton_Click(object sender, RoutedEventArgs e)
        {
            KEAPCanvas new_Canvas = new KEAPCanvas()
            {
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Background = new ImageBrush(canvas_Background_Bitmapimage)
            };

            if ((this.Width * 3.75 / 4.45) < (((this.Height - 92) * 3 / 3.8) * (WindowSettings.resolution_Width / WindowSettings.resolution_Height)))
            {
                new_Canvas.Width = (WindowSettings.current_Width * 3.75 / 4.45) - 50;
                new_Canvas.Height = (MainCanvas.Height * (WindowSettings.resolution_Height / WindowSettings.resolution_Width));
            }
            else
            {
                new_Canvas.Height = (WindowSettings.current_Height - 92) * (3 / 3.8) - 50;
                new_Canvas.Width = (MainCanvas.Height * (WindowSettings.resolution_Width / WindowSettings.resolution_Height));
            }

            new_Canvas.PreviewMouseLeftButtonDown += MainCanvas_PreviewMouseLeftButtonDown;
            new_Canvas.PreviewMouseLeftButtonUp += MainCanvas_PreviewMouseLeftButtonUp;
            new_Canvas.PreviewMouseMove += MainCanvas_PreviewMouseMove;

            MainCanvas_Border.Child = new_Canvas;
            canvas_List.Add(new_Canvas);
            
            Add_Slide_List(canvas_List.Count);

            Autoedit_Slide_List(new_Canvas, canvas_List.Count - 1);

            Image image_renew = RenderCanvas(canvas_List[canvas_List.Count-1]);
            ((SlideInfo)(Slide_ListView.Items[canvas_List.Count-1])).Source = image_renew.Source;
        }

        private void Slide_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Slide_ListView.SelectedIndex == -1 && previous_selection != -1)
                Slide_ListView.SelectedIndex = 0;
            if (previous_selection != Slide_ListView.SelectedIndex)
            {
                previous_selection = Slide_ListView.SelectedIndex;
            }
            MainCanvas_Border.Child = canvas_List.ElementAt(Slide_ListView.SelectedIndex);
            if (edit_Rect != null && Slide_ListView.SelectedIndex != canvas_List.IndexOf(MainCanvas)) MainCanvas.Children.Remove(edit_Rect);
            MainCanvas = canvas_List.ElementAt(Slide_ListView.SelectedIndex);
            Image image = RenderCanvas(MainCanvas);
            ((SlideInfo)(Slide_ListView.Items[Slide_ListView.SelectedIndex])).Source = image.Source;
            //Slide_ListView.Items[Slide_ListView.SelectedIndex]
        }


        public class SlideInfo
        {
            public ImageSource Source { get; set; }
            public string Number { get; set; }
            public double Image_Width { get; set; }
            public double Image_Height { get; set; }
            public double Rect_Height1 { get; set; }
            public double Rect_Height2 { get; set; }
        }

        private Image RenderCanvas(KEAPCanvas param_Canvas)
        {
            Image img = new Image();
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)param_Canvas.Width + 200, (int)param_Canvas.Height + 200, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(param_Canvas);
            img.Source = rtb;
            return img;
        }

        void Add_Slide_List(int param_Number, int num = -1)
        {
            SetAllModeFalse();
            SetAllMoveModeFalse();

            Image image = RenderCanvas(canvas_List.ElementAt(param_Number - 1));

            if (num != -1)
            {
                Slides_List.Add(new SlideInfo()
                {
                    Source = image.Source,
                    Number = Convert.ToString(canvas_List.Count),
                    Image_Width = ((WindowSettings.current_Width) * 0.7 / 4.45) - 35,
                    Image_Height = ((WindowSettings.current_Width) * 0.7 / 4.45) * (WindowSettings.resolution_Height / WindowSettings.resolution_Width),
                    Rect_Height1 = (((WindowSettings.current_Width) * 0.7 / 4.45) * (WindowSettings.resolution_Height / WindowSettings.resolution_Width) * 0.105) * (WindowSettings.current_Width / WindowSettings.current_Height),
                    Rect_Height2 = (((WindowSettings.current_Width) * 0.7 / 4.45) * (WindowSettings.resolution_Height / WindowSettings.resolution_Width) * 0.01) * (WindowSettings.current_Width / WindowSettings.current_Height)
                });
            }
            else
            {
                Slides_List.Add(new SlideInfo()
                {
                    Source = image.Source,
                    Number = Convert.ToString(canvas_List.Count),
                    Image_Width = ((WindowSettings.current_Width) * 0.7 / 4.45) - 35,
                    Image_Height = ((WindowSettings.current_Width) * 0.7 / 4.45) * (WindowSettings.resolution_Height / WindowSettings.resolution_Width),
                    Rect_Height1 = (((WindowSettings.current_Width) * 0.7 / 4.45) * (WindowSettings.resolution_Height / WindowSettings.resolution_Width) * 0.105) * (WindowSettings.current_Width / WindowSettings.current_Height),
                    Rect_Height2 = (((WindowSettings.current_Width) * 0.7 / 4.45) * (WindowSettings.resolution_Height / WindowSettings.resolution_Width) * 0.01) * (WindowSettings.current_Width / WindowSettings.current_Height)
                });
            }

            Slide_ListView.ItemsSource = Slides_List;
        }

        void Autoedit_Slide_List(KEAPCanvas param_Canvas, int param_Number)
        {
            Image image = RenderCanvas(param_Canvas);
            
            if (Main_Border.ActualWidth == 0)
            {
                Slides_List.Insert(param_Number, new SlideInfo()
                {
                    Source = image.Source,
                    Number = Convert.ToString(canvas_List.Count),
                    Image_Width = ((WindowSettings.current_Width) * 0.7 / 4.45) - 35,
                    Image_Height = ((WindowSettings.current_Width) * 0.7 / 4.45) * (WindowSettings.resolution_Height / WindowSettings.resolution_Width),
                    Rect_Height1 = (((WindowSettings.current_Width) * 0.7 / 4.45) * (WindowSettings.resolution_Height / WindowSettings.resolution_Width) * 0.105) * (WindowSettings.current_Width / WindowSettings.current_Height),
                    Rect_Height2 = (((WindowSettings.current_Width) * 0.7 / 4.45) * (WindowSettings.resolution_Height / WindowSettings.resolution_Width) * 0.01) * (WindowSettings.current_Width / WindowSettings.current_Height)
                });
            }

            else
            {
                Slides_List.Insert(param_Number, new SlideInfo()
                {
                    Source = image.Source,
                    Number = Convert.ToString(param_Number+1),
                    Image_Width = ((WindowSettings.current_Width) * 0.7 / 4.45)-35,
                    Image_Height = ((WindowSettings.current_Width) * 0.7 / 4.45) * (WindowSettings.resolution_Height / WindowSettings.resolution_Width),
                    Rect_Height1 = (((WindowSettings.current_Width) * 0.7 / 4.45) * (WindowSettings.resolution_Height / WindowSettings.resolution_Width) * 0.105) * (WindowSettings.current_Width / WindowSettings.current_Height),
                    Rect_Height2 = (((WindowSettings.current_Width) * 0.7 / 4.45) * (WindowSettings.resolution_Height / WindowSettings.resolution_Width) * 0.01) * (WindowSettings.current_Width / WindowSettings.current_Height)
                });
            }
            
            Slide_ListView.SelectedIndex = param_Number;
            
            Slides_List.RemoveAt(param_Number + 1);
            Slide_ListView.SelectedIndex = param_Number;
            
            //((SlideInfo)(Slide_ListView.Items[param_Number])).Source = image.Source;
            Slide_ListView.ItemsSource = Slides_List;
            MainCanvas.Measure(new Size(MainCanvas.Width, MainCanvas.Height));
            MainCanvas.Arrange(new Rect(0, 0, MainCanvas.Width, MainCanvas.Height));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog savedlg = new SaveFileDialog();
            savedlg.FileName = ""; // Default file name
            savedlg.DefaultExt = ".keap"; // Default file extension
            savedlg.Filter = "KEAP File (.keap)|*.keap"; // Filter files by extension 

            Nullable<bool> result = savedlg.ShowDialog();

            if (result == true)
            {
                string filename = savedlg.SafeFileName;
                string name = filename.Substring(0, filename.Length - 5);

                string fullpath = savedlg.FileName;

                string startPath = "c:\\KEAP\\" + name + "\\";
                string zipPath = "c:\\KEAP\\testing\\" + name + ".keap";

                Save_Xml(name, startPath);
                // Declare Variables

                ZipFile.CreateFromDirectory(startPath, zipPath);
            }
        }

        private void Save_Xml(string param_name, string param_path)
        {
            Directory.CreateDirectory(param_path);
            using (var xmlfile = System.IO.File.Create(param_path + param_name + ".xml")) //Convert.ToString(i++))
            {
                String xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><files></files>";
                var doc = new XmlDocument();
                doc.LoadXml(xml);
                doc.Save(xmlfile);
                xmlfile.Dispose();
                xmlfile.Close();
            }
            using (XmlWriter xmlFile = XmlWriter.Create(param_path + param_name + ".xml"))
            {
                XmlDocument xmlDoc;
                xmlDoc = new XmlDocument();

                XmlElement file = xmlDoc.CreateElement("file");
                file.SetAttribute("Time", DateTime.Now.ToLongDateString() + "\\" + DateTime.Now.ToLongTimeString());
                file.SetAttribute("MainCanvas_Width", Convert.ToString(MainCanvas.Width));
                file.SetAttribute("MainCanvas_Height", Convert.ToString(MainCanvas.Height));
                int i = 1;


                FileStream fs_BG;
                string filepath_BG = param_path + param_name + "_Background.png"; // filename_Image_(Canvas#)_(Image#).png
                fs_BG = System.IO.File.OpenWrite(filepath_BG);
                PngBitmapEncoder encoder_BG = new PngBitmapEncoder();
                // push the rendered bitmap to it
                encoder_BG.Frames.Add(BitmapFrame.Create(canvas_Background));
                // save the data to the stream
                encoder_BG.Save(fs_BG);
                //fs.Dispose();
                fs_BG.Close();
                file.SetAttribute("Background_Path", param_name + "_Background.png");

                foreach (KEAPCanvas canvas in canvas_List)
                {
                    XmlElement canvas_element = xmlDoc.CreateElement("Slide" + Convert.ToString(i));
                    List<BitmapFrame> bitmapFrame_List = null;
                    if (i - 1 < bitmapFrame_Dictionary.Count)
                        bitmapFrame_List = bitmapFrame_Dictionary[i - 1];
                    
                    int j = 0;
                    foreach (UIElement obj in canvas.Children)
                    {
                        if (obj is Polygon) //5(다각형)
                        {
                            Polygon thispolygon = obj as Polygon;
                            int index = 0;
                            XmlElement P = xmlDoc.CreateElement("Polygon");
                            XmlElement polygon_count = xmlDoc.CreateElement("Count");
                            polygon_count.InnerText = Convert.ToString(thispolygon.Points.Count);
                            P.AppendChild(polygon_count);
                            while (index < thispolygon.Points.Count)
                            {
                                XmlElement x1 = xmlDoc.CreateElement("X" + Convert.ToString(index));
                                x1.InnerText = Convert.ToString(thispolygon.Points.ElementAt(index).X);
                                P.AppendChild(x1);
                                XmlElement y1 = xmlDoc.CreateElement("Y" + Convert.ToString(index));
                                y1.InnerText = Convert.ToString(thispolygon.Points.ElementAt(index).Y);
                                P.AppendChild(y1);
                                index++;
                            }

                            Brush e_stroke_Brush = thispolygon.Stroke;
                            XmlElement stroke_Color_Of_A = xmlDoc.CreateElement("Stroke_Color_A");//((Color)stroke_Brush.GetValue(SolidColorstroke_Brush.ColorProperty)).A;
                            stroke_Color_Of_A.InnerText = Convert.ToString(((Color)e_stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).A);
                            P.AppendChild(stroke_Color_Of_A);
                            XmlElement stroke_Color_Of_R = xmlDoc.CreateElement("Stroke_Color_R");
                            stroke_Color_Of_R.InnerText = Convert.ToString(((Color)e_stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).R);
                            P.AppendChild(stroke_Color_Of_R);
                            XmlElement stroke_Color_Of_G = xmlDoc.CreateElement("Stroke_Color_G");
                            stroke_Color_Of_G.InnerText = Convert.ToString(((Color)e_stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).G);
                            P.AppendChild(stroke_Color_Of_G);
                            XmlElement stroke_Color_Of_B = xmlDoc.CreateElement("Stroke_Color_B");
                            stroke_Color_Of_B.InnerText = Convert.ToString(((Color)e_stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).B);
                            P.AppendChild(stroke_Color_Of_B);

                            Brush e_fill_Brush = thispolygon.Fill;
                            XmlElement fill_Color_Of_A = xmlDoc.CreateElement("Fill_Color_A");//((Color)brush.GetValue(SolidColorBrush.ColorProperty)).A;
                            fill_Color_Of_A.InnerText = Convert.ToString(((Color)e_fill_Brush.GetValue(SolidColorBrush.ColorProperty)).A);
                            P.AppendChild(fill_Color_Of_A);
                            XmlElement fill_Color_Of_R = xmlDoc.CreateElement("Fill_Color_R");
                            fill_Color_Of_R.InnerText = Convert.ToString(((Color)e_fill_Brush.GetValue(SolidColorBrush.ColorProperty)).R);
                            P.AppendChild(fill_Color_Of_R);
                            XmlElement fill_Color_Of_G = xmlDoc.CreateElement("Fill_Color_G");
                            fill_Color_Of_G.InnerText = Convert.ToString(((Color)e_fill_Brush.GetValue(SolidColorBrush.ColorProperty)).G);
                            P.AppendChild(fill_Color_Of_G);
                            XmlElement fill_Color_Of_B = xmlDoc.CreateElement("Fill_Color_B");
                            fill_Color_Of_B.InnerText = Convert.ToString(((Color)e_fill_Brush.GetValue(SolidColorBrush.ColorProperty)).B);
                            P.AppendChild(fill_Color_Of_B);

                            canvas_element.AppendChild(P);
                        }

                        else if (obj is Line) //저장(라인)
                        {
                            Line thisline = obj as Line;
                            XmlElement L = xmlDoc.CreateElement("Line");
                            XmlElement x1 = xmlDoc.CreateElement("X1");
                            x1.InnerText = Convert.ToString(thisline.X1);
                            L.AppendChild(x1);
                            XmlElement y1 = xmlDoc.CreateElement("Y1");
                            y1.InnerText = Convert.ToString(thisline.Y1);
                            L.AppendChild(y1);
                            XmlElement x2 = xmlDoc.CreateElement("X2");
                            x2.InnerText = Convert.ToString(thisline.X2);
                            L.AppendChild(x2);
                            XmlElement y2 = xmlDoc.CreateElement("Y2");
                            y2.InnerText = Convert.ToString(thisline.Y2);
                            L.AppendChild(y2);

                            Brush brush = thisline.Stroke;
                            XmlElement Color_of_A = xmlDoc.CreateElement("Color_A");//((Color)brush.GetValue(SolidColorBrush.ColorProperty)).A;
                            Color_of_A.InnerText = Convert.ToString(((Color)brush.GetValue(SolidColorBrush.ColorProperty)).A);
                            L.AppendChild(Color_of_A);
                            XmlElement Color_of_R = xmlDoc.CreateElement("Color_R");
                            Color_of_R.InnerText = Convert.ToString(((Color)brush.GetValue(SolidColorBrush.ColorProperty)).R);
                            L.AppendChild(Color_of_R);
                            XmlElement Color_of_G = xmlDoc.CreateElement("Color_G");
                            Color_of_G.InnerText = Convert.ToString(((Color)brush.GetValue(SolidColorBrush.ColorProperty)).G);
                            L.AppendChild(Color_of_G);
                            XmlElement Color_of_B = xmlDoc.CreateElement("Color_B");
                            Color_of_B.InnerText = Convert.ToString(((Color)brush.GetValue(SolidColorBrush.ColorProperty)).B);
                            L.AppendChild(Color_of_B);

                            canvas_element.AppendChild(L);
                        }

                        else if (obj is EditableTextBlock) //저장(텍스트)
                        {
                            EditableTextBlock thistextblock = obj as EditableTextBlock;
                            XmlElement T = xmlDoc.CreateElement("Textblock");
                            XmlElement x1 = xmlDoc.CreateElement("X1");
                            x1.InnerText = Convert.ToString(Canvas.GetLeft(thistextblock));
                            T.AppendChild(x1);
                            XmlElement y1 = xmlDoc.CreateElement("Y1");
                            y1.InnerText = Convert.ToString(Canvas.GetTop(thistextblock));
                            T.AppendChild(y1);
                            XmlElement x2 = xmlDoc.CreateElement("X2");
                            x2.InnerText = Convert.ToString(Canvas.GetLeft(thistextblock) + thistextblock.Width);
                            T.AppendChild(x2);
                            XmlElement y2 = xmlDoc.CreateElement("Y2");
                            y2.InnerText = Convert.ToString(Canvas.GetTop(thistextblock) + thistextblock.Height);
                            T.AppendChild(y2);

                            XmlElement text = xmlDoc.CreateElement("Text");
                            text.InnerText = thistextblock.Text;
                            T.AppendChild(text);
                            XmlElement fontsize = xmlDoc.CreateElement("FontSize");
                            fontsize.InnerText = Convert.ToString(thistextblock.FontSize);
                            T.AppendChild(fontsize);

                            Brush e_Fill_Brush = thistextblock.Background;
                            XmlElement Fill_Color_of_A = xmlDoc.CreateElement("Fill_Color_A");//((Color)e_Fill_Brush.GetValue(SolidColorBrush.ColorProperty)).A;
                            Fill_Color_of_A.InnerText = Convert.ToString(((Color)e_Fill_Brush.GetValue(SolidColorBrush.ColorProperty)).A);
                            T.AppendChild(Fill_Color_of_A);
                            XmlElement Fill_Color_of_R = xmlDoc.CreateElement("Fill_Color_R");
                            Fill_Color_of_R.InnerText = Convert.ToString(((Color)e_Fill_Brush.GetValue(SolidColorBrush.ColorProperty)).R);
                            T.AppendChild(Fill_Color_of_R);
                            XmlElement Fill_Color_of_G = xmlDoc.CreateElement("Fill_Color_G");
                            Fill_Color_of_G.InnerText = Convert.ToString(((Color)e_Fill_Brush.GetValue(SolidColorBrush.ColorProperty)).G);
                            T.AppendChild(Fill_Color_of_G);
                            XmlElement Fill_Color_of_B = xmlDoc.CreateElement("Fill_Color_B");
                            Fill_Color_of_B.InnerText = Convert.ToString(((Color)e_Fill_Brush.GetValue(SolidColorBrush.ColorProperty)).B);
                            T.AppendChild(Fill_Color_of_B);

                            Brush e_Foreground_Brush = thistextblock.Background;
                            XmlElement Foreground_Color_of_A = xmlDoc.CreateElement("Foreground_Color_A");//((Color)e_Foreground_Brush.GetValue(SolidColorBrush.ColorProperty)).A;
                            Foreground_Color_of_A.InnerText = Convert.ToString(((Color)e_Foreground_Brush.GetValue(SolidColorBrush.ColorProperty)).A);
                            T.AppendChild(Foreground_Color_of_A);
                            XmlElement Foreground_Color_of_R = xmlDoc.CreateElement("Foreground_Color_R");
                            Foreground_Color_of_R.InnerText = Convert.ToString(((Color)e_Foreground_Brush.GetValue(SolidColorBrush.ColorProperty)).R);
                            T.AppendChild(Foreground_Color_of_R);
                            XmlElement Foreground_Color_of_G = xmlDoc.CreateElement("Foreground_Color_G");
                            Foreground_Color_of_G.InnerText = Convert.ToString(((Color)e_Foreground_Brush.GetValue(SolidColorBrush.ColorProperty)).G);
                            T.AppendChild(Foreground_Color_of_G);
                            XmlElement Foreground_Color_of_B = xmlDoc.CreateElement("Foreground_Color_B");
                            Foreground_Color_of_B.InnerText = Convert.ToString(((Color)e_Foreground_Brush.GetValue(SolidColorBrush.ColorProperty)).B);
                            T.AppendChild(Foreground_Color_of_B);

                            XmlElement textalignment = xmlDoc.CreateElement("TextAlignment");
                            textalignment.InnerText = thistextblock.TextAlignment.ToString();
                            T.AppendChild(textalignment);

                            XmlElement e_FontFamily = xmlDoc.CreateElement("FontFamily");
                            e_FontFamily.InnerText = thistextblock.FontFamily.ToString();
                            T.AppendChild(e_FontFamily);

                            XmlElement e_FontWeight = xmlDoc.CreateElement("FontWeight");
                            e_FontWeight.InnerText = thistextblock.FontWeight.ToString();
                            T.AppendChild(e_FontWeight);

                            XmlElement e_Effect = xmlDoc.CreateElement("Effect");
                            if (thistextblock.Effect != null)
                                e_Effect.InnerText = "1";
                            else
                                e_Effect.InnerText = "0";
                            T.AppendChild(e_Effect);
                            
                            canvas_element.AppendChild(T);
                        }

                        else if (obj is Rectangle) //저장(사각형)
                        {
                            Rectangle thisrectangle = obj as Rectangle;

                            XmlElement R = xmlDoc.CreateElement("Rectangle");
                            XmlElement rectangle_left = xmlDoc.CreateElement("X");
                            rectangle_left.InnerText = Convert.ToString(Canvas.GetLeft(thisrectangle));
                            R.AppendChild(rectangle_left);
                            XmlElement rectangle_top = xmlDoc.CreateElement("Y");
                            rectangle_top.InnerText = Convert.ToString(Canvas.GetTop(thisrectangle));
                            R.AppendChild(rectangle_top);

                            XmlElement rectangle_width = xmlDoc.CreateElement("Width");
                            rectangle_width.InnerText = Convert.ToString(thisrectangle.Width);
                            R.AppendChild(rectangle_width);
                            XmlElement rectangle_height = xmlDoc.CreateElement("Height");
                            rectangle_height.InnerText = Convert.ToString(thisrectangle.Height);
                            R.AppendChild(rectangle_height);

                            Brush e_stroke_Brush = thisrectangle.Stroke;
                            XmlElement stroke_Color_Of_A = xmlDoc.CreateElement("Stroke_Color_A");//((Color)stroke_Brush.GetValue(SolidColorstroke_Brush.ColorProperty)).A;
                            stroke_Color_Of_A.InnerText = Convert.ToString(((Color)e_stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).A);
                            R.AppendChild(stroke_Color_Of_A);
                            XmlElement stroke_Color_Of_R = xmlDoc.CreateElement("Stroke_Color_R");
                            stroke_Color_Of_R.InnerText = Convert.ToString(((Color)e_stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).R);
                            R.AppendChild(stroke_Color_Of_R);
                            XmlElement stroke_Color_Of_G = xmlDoc.CreateElement("Stroke_Color_G");
                            stroke_Color_Of_G.InnerText = Convert.ToString(((Color)e_stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).G);
                            R.AppendChild(stroke_Color_Of_G);
                            XmlElement stroke_Color_Of_B = xmlDoc.CreateElement("Stroke_Color_B");
                            stroke_Color_Of_B.InnerText = Convert.ToString(((Color)e_stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).B);
                            R.AppendChild(stroke_Color_Of_B);


                            Brush e_fill_Brush = thisrectangle.Fill;
                            XmlElement fill_Color_Of_A = xmlDoc.CreateElement("Fill_Color_A");//((Color)brush.GetValue(SolidColorBrush.ColorProperty)).A;
                            fill_Color_Of_A.InnerText = Convert.ToString(((Color)e_fill_Brush.GetValue(SolidColorBrush.ColorProperty)).A);
                            R.AppendChild(fill_Color_Of_A);
                            XmlElement fill_Color_Of_R = xmlDoc.CreateElement("Fill_Color_R");
                            fill_Color_Of_R.InnerText = Convert.ToString(((Color)e_fill_Brush.GetValue(SolidColorBrush.ColorProperty)).R);
                            R.AppendChild(fill_Color_Of_R);
                            XmlElement fill_Color_Of_G = xmlDoc.CreateElement("Fill_Color_G");
                            fill_Color_Of_G.InnerText = Convert.ToString(((Color)e_fill_Brush.GetValue(SolidColorBrush.ColorProperty)).G);
                            R.AppendChild(fill_Color_Of_G);
                            XmlElement fill_Color_Of_B = xmlDoc.CreateElement("Fill_Color_B");
                            fill_Color_Of_B.InnerText = Convert.ToString(((Color)e_fill_Brush.GetValue(SolidColorBrush.ColorProperty)).B);
                            R.AppendChild(fill_Color_Of_B);

                            canvas_element.AppendChild(R);
                        }

                        else if (obj is Ellipse) //저장(원)
                        {
                            Ellipse thisellipse = obj as Ellipse;

                            XmlElement E = xmlDoc.CreateElement("Ellipse");
                            XmlElement ellipse_left = xmlDoc.CreateElement("X");
                            ellipse_left.InnerText = Convert.ToString(Canvas.GetLeft(thisellipse));
                            E.AppendChild(ellipse_left);
                            XmlElement ellipse_top = xmlDoc.CreateElement("Y");
                            ellipse_top.InnerText = Convert.ToString(Canvas.GetTop(thisellipse));
                            E.AppendChild(ellipse_top);

                            XmlElement ellipse_width = xmlDoc.CreateElement("Width");
                            ellipse_width.InnerText = Convert.ToString(thisellipse.Width);
                            E.AppendChild(ellipse_width);
                            XmlElement ellipse_height = xmlDoc.CreateElement("Height");
                            ellipse_height.InnerText = Convert.ToString(thisellipse.Height);
                            E.AppendChild(ellipse_height);

                            Brush e_stroke_Brush = thisellipse.Stroke;
                            XmlElement stroke_Color_Of_A = xmlDoc.CreateElement("Stroke_Color_A");//((Color)stroke_Brush.GetValue(SolidColorstroke_Brush.ColorProperty)).A;
                            stroke_Color_Of_A.InnerText = Convert.ToString(((Color)e_stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).A);
                            E.AppendChild(stroke_Color_Of_A);
                            XmlElement stroke_Color_Of_R = xmlDoc.CreateElement("Stroke_Color_R");
                            stroke_Color_Of_R.InnerText = Convert.ToString(((Color)e_stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).R);
                            E.AppendChild(stroke_Color_Of_R);
                            XmlElement stroke_Color_Of_G = xmlDoc.CreateElement("Stroke_Color_G");
                            stroke_Color_Of_G.InnerText = Convert.ToString(((Color)e_stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).G);
                            E.AppendChild(stroke_Color_Of_G);
                            XmlElement stroke_Color_Of_B = xmlDoc.CreateElement("Stroke_Color_B");
                            stroke_Color_Of_B.InnerText = Convert.ToString(((Color)e_stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).B);
                            E.AppendChild(stroke_Color_Of_B);


                            Brush e_fill_Brush = thisellipse.Fill;
                            XmlElement fill_Color_Of_A = xmlDoc.CreateElement("Fill_Color_A");//((Color)brush.GetValue(SolidColorBrush.ColorProperty)).A;
                            fill_Color_Of_A.InnerText = Convert.ToString(((Color)e_fill_Brush.GetValue(SolidColorBrush.ColorProperty)).A);
                            E.AppendChild(fill_Color_Of_A);
                            XmlElement fill_Color_Of_R = xmlDoc.CreateElement("Fill_Color_R");
                            fill_Color_Of_R.InnerText = Convert.ToString(((Color)e_fill_Brush.GetValue(SolidColorBrush.ColorProperty)).R);
                            E.AppendChild(fill_Color_Of_R);
                            XmlElement fill_Color_Of_G = xmlDoc.CreateElement("Fill_Color_G");
                            fill_Color_Of_G.InnerText = Convert.ToString(((Color)e_fill_Brush.GetValue(SolidColorBrush.ColorProperty)).G);
                            E.AppendChild(fill_Color_Of_G);
                            XmlElement fill_Color_Of_B = xmlDoc.CreateElement("Fill_Color_B");
                            fill_Color_Of_B.InnerText = Convert.ToString(((Color)e_fill_Brush.GetValue(SolidColorBrush.ColorProperty)).B);
                            E.AppendChild(fill_Color_Of_B);


                            canvas_element.AppendChild(E);
                        }

                        else if (obj is Image) //저장(이미지)
                        {
                            Image thisimage = obj as Image;
                            XmlElement I = xmlDoc.CreateElement("Image");
                            XmlElement x1 = xmlDoc.CreateElement("X1");
                            x1.InnerText = Convert.ToString(Canvas.GetLeft(thisimage));
                            I.AppendChild(x1);
                            XmlElement y1 = xmlDoc.CreateElement("Y1");
                            y1.InnerText = Convert.ToString(Canvas.GetTop(thisimage));
                            I.AppendChild(y1);
                            XmlElement x2 = xmlDoc.CreateElement("X2");
                            x2.InnerText = Convert.ToString(Canvas.GetLeft(thisimage) + thisimage.Width);
                            I.AppendChild(x2);
                            XmlElement y2 = xmlDoc.CreateElement("Y2");
                            y2.InnerText = Convert.ToString(Canvas.GetTop(thisimage) + thisimage.Height);
                            I.AppendChild(y2);

                            FileStream fs;
                            BitmapFrame frame = bitmapFrame_List.ElementAt(j);
                            string filepath = param_path + param_name + "_Image_" + Convert.ToString(i) + "_" + Convert.ToString(j) + ".png"; // filename_Image_(Canvas#)_(Image#).png

                            // Create a file stream for saving image
                            fs = System.IO.File.OpenWrite(filepath);
                            // Use png encoder for our data
                            PngBitmapEncoder encoder = new PngBitmapEncoder();
                            // push the rendered bitmap to it
                            encoder.Frames.Add(BitmapFrame.Create(frame));
                            // save the data to the stream
                            encoder.Save(fs);
                            //fs.Dispose();
                            fs.Close();

                            XmlElement image_path = xmlDoc.CreateElement("Path");
                            image_path.InnerText = param_name + "_Image_" + Convert.ToString(i) + "_" + Convert.ToString(j++) + ".png";
                            I.AppendChild(image_path);

                            canvas_element.AppendChild(I);
                        }
                    }

                    List<Dictionary<int, string>> ani_List = animation_Dictionary[i - 1];
                    //int p = 0;
                    foreach (Dictionary<int, string> dic in ani_List)
                    {
                        XmlElement animation = xmlDoc.CreateElement("Animation");
                        animation.InnerText = Convert.ToString(dic.Keys.First()) + "." + dic[dic.Keys.First()];
                        canvas_element.AppendChild(animation);
                    }

                    file.AppendChild(canvas_element);
                    i++;
                }
                file.WriteTo(xmlFile);
                xmlDoc.Save(xmlFile);
                xmlFile.Close();
            }
            MainCanvas = canvas_List[0];
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog opendlg = new Microsoft.Win32.OpenFileDialog();
            opendlg.DefaultExt = ".keap"; // Default file extension
            opendlg.Filter = "KEAP File (.keap)|*.keap"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = opendlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                string filename = opendlg.SafeFileName;
                string name = filename.Substring(0, filename.Length - 5);

                string fullpath = opendlg.FileName;

                string zipPath = fullpath;
                string extractPath = fullpath.Substring(0, fullpath.Length - filename.Length) + "KEAP_extract_" + name + "\\";

                Directory.CreateDirectory(extractPath);
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    String xmlFile_Path = "", xmlFile_Name = "";
                    List<String> imageFile_Path_List = new List<String>();
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        entry.ExtractToFile(System.IO.Path.Combine(extractPath, entry.FullName));
                        if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        {
                            xmlFile_Path = extractPath;
                            xmlFile_Name = entry.FullName;
                        }
                        else
                            imageFile_Path_List.Add(extractPath + entry.FullName);
                    }
                    Open_Xml(xmlFile_Name, xmlFile_Path, imageFile_Path_List);
                }

                Directory.Delete(extractPath, true);
                for (int p = 0; p < canvas_List.Count; p++)
                    Add_Slide_List(p + 1, p + 1);

                for (int p = 0; p < canvas_List.Count; p++)
                    Autoedit_Slide_List(canvas_List.ElementAt(p), p);

                Slide_ListView.SelectedIndex = 0;
            }
        }

        private void Open_Xml(string param_name, string param_path, List<string> imageFile_Path_List)
        {
            using (XmlReader xmlFile = XmlReader.Create(param_path + param_name))
            {
                XmlDocument xmlDoc;
                xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);

                XmlNodeList xmlList = xmlDoc.GetElementsByTagName("file");
                animation_Dictionary = new Dictionary<int, List<Dictionary<int, string>>>();
                foreach (XmlElement file in xmlList)
                {
                    double canvas_Width = Convert.ToDouble(file.GetAttribute("MainCanvas_Width")),
                        canvas_Height = Convert.ToDouble(file.GetAttribute("MainCanvas_Height"));

                    int i = 1;
                    canvas_List = new List<KEAPCanvas>();
                    bitmapFrame_Dictionary = new Dictionary<int, List<BitmapFrame>>();
                    // Slide_ListView.SelectedIndex = -1;
                    Slides_List = new ObservableCollection<SlideInfo>();

                    string background_Path = file.GetAttribute("Background_Path");
                    
                    canvas_Background_Bitmapimage = new BitmapImage();
                    canvas_Background_Bitmapimage.BeginInit();
                    canvas_Background_Bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    canvas_Background_Bitmapimage.UriSource = new Uri(param_path + background_Path, UriKind.Absolute);
                    canvas_Background_Bitmapimage.EndInit();
                    
                    
                    canvas_Background = BitmapFrame.Create(canvas_Background_Bitmapimage);
                    ImageBrush imagebrush_BG = new ImageBrush(canvas_Background_Bitmapimage);

                    foreach (XmlElement slide in file.ChildNodes)
                    {
                        KEAPCanvas canvas = new KEAPCanvas();
                        canvas.Width = canvas_Width;
                        canvas.Height = canvas_Height;
                        canvas.Background = imagebrush_BG;
                        
                        animation_List = new List<Dictionary<int, string>>();
                        foreach (XmlElement uielement in slide.ChildNodes)
                        {
                            if (uielement.Name == "Polygon") //열기(다각형)
                            {
                                int p = 0;
                                XmlNode xmlnode;
                                xmlnode = uielement.ChildNodes[p];
                                double counts = double.Parse(xmlnode.InnerText);
                                p++;
                                List<double> _points = new List<double>();
                                Polygon newpolygon = new Polygon()
                                {
                                    StrokeThickness = 2.0
                                };
                                while (p < counts * 2 + 1)
                                {
                                    xmlnode = uielement.ChildNodes[p];
                                    _points.Add(double.Parse(xmlnode.InnerText));
                                    if (p % 2 == 0)
                                        newpolygon.Points.Add(new Point(_points[p - 2], _points[p - 1]));
                                    p++;
                                }

                                xmlnode = uielement.ChildNodes[p];
                                Byte stroke_Color_A = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte stroke_Color_R = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte stroke_Color_G = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte stroke_Color_B = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                Color stroke_Color = new Color()
                                {
                                    A = stroke_Color_A,
                                    G = stroke_Color_G,
                                    R = stroke_Color_R,
                                    B = stroke_Color_B
                                };
                                newpolygon.Stroke = new SolidColorBrush(stroke_Color);

                                xmlnode = uielement.ChildNodes[p];
                                Byte fill_Color_A = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte fill_Color_R = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte fill_Color_G = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte fill_Color_B = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                Color fill_Color = new Color()
                                {
                                    A = fill_Color_A,
                                    G = fill_Color_G,
                                    R = fill_Color_R,
                                    B = fill_Color_B
                                };

                                newpolygon.Fill = new SolidColorBrush(fill_Color);

                                newpolygon.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                                newpolygon.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                                newpolygon.MouseMove += UIElement_MouseMove;

                                canvas.Children.Add(newpolygon);
                            }

                            else if (uielement.Name == "Line") //열기(선)
                            {
                                int p = 0;
                                XmlNode xmlnode;
                                List<double> _points = new List<double>();
                                while (p < 4)
                                {
                                    xmlnode = uielement.ChildNodes[p];
                                    _points.Add(double.Parse(xmlnode.InnerText));
                                    p++;
                                }
                                xmlnode = uielement.ChildNodes[p];
                                Byte Color_A = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte Color_R = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte Color_G = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte Color_B = Convert.ToByte(xmlnode.InnerText);

                                Color linecolor = new Color()
                                {
                                    A = Color_A,
                                    G = Color_G,
                                    R = Color_R,
                                    B = Color_B
                                };
                                Line newline = new Line()
                                {
                                    X1 = _points.ElementAt(0),
                                    Y1 = _points.ElementAt(1),
                                    X2 = _points.ElementAt(2),
                                    Y2 = _points.ElementAt(3),
                                    StrokeThickness = 4.0,
                                    Stroke = new SolidColorBrush(linecolor)
                                };
                                //중요/////////////newline.PointerEntered += newline_PointerEntered;

                                newline.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                                newline.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                                newline.MouseMove += UIElement_MouseMove;

                                canvas.Children.Add(newline);
                            }

                            else if (uielement.Name == "EditableTextBlock") //열기(텍스트)
                            {
                                int p = 0;
                                XmlNode xmlnode;
                                List<double> _points = new List<double>();
                                while (p < 4)
                                {
                                    xmlnode = uielement.ChildNodes[p];
                                    _points.Add(double.Parse(xmlnode.InnerText));
                                    p++;
                                }
                                string _Text = uielement.ChildNodes[p].InnerText;
                                p++;
                                double _FontSize = Convert.ToDouble(uielement.ChildNodes[p].InnerText);
                                p++;

                                xmlnode = uielement.ChildNodes[p];
                                Byte fill_Color_A = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte fill_Color_R = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte fill_Color_G = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte fill_Color_B = Convert.ToByte(xmlnode.InnerText);
                                p++;

                                Color fillcolor = new Color()
                                {
                                    A = fill_Color_A,
                                    G = fill_Color_G,
                                    R = fill_Color_R,
                                    B = fill_Color_B
                                };

                                xmlnode = uielement.ChildNodes[p];
                                Byte font_Color_A = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte font_Color_R = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte font_Color_G = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte font_Color_B = Convert.ToByte(xmlnode.InnerText);
                                p++;

                                Color fontcolor = new Color()
                                {
                                    A = font_Color_A,
                                    G = font_Color_G,
                                    R = font_Color_R,
                                    B = font_Color_B
                                };

                                xmlnode = uielement.ChildNodes[p];
                                TextAlignment textalignment;
                                if(xmlnode.InnerText=="Center")
                                    textalignment = TextAlignment.Center;
                                else if(xmlnode.InnerText=="Left")
                                    textalignment = TextAlignment.Left;
                                else if (xmlnode.InnerText == "Right")
                                    textalignment = TextAlignment.Right;
                                else
                                    textalignment = TextAlignment.Justify;
                                p++;

                                xmlnode = uielement.ChildNodes[p];
                                FontFamily fontfamily = new FontFamily(xmlnode.ToString());
                                p++;

                                xmlnode = uielement.ChildNodes[p];
                                FontWeight fontweight;
                                if (xmlnode.InnerText == "Bold")
                                    fontweight = FontWeights.Bold;
                                else
                                    fontweight = FontWeights.Normal;
                                p++;

                                xmlnode = uielement.ChildNodes[p];
                                Effect effect;
                                if(xmlnode.InnerText == "1")
                                    effect = new DropShadowEffect() { ShadowDepth = 4, Direction = 315, Color = Colors.Black, Opacity = 0.5 };
                                else
                                    effect = null;
                                p++;

                                EditableTextBlock newtextbox = new EditableTextBlock()
                                {
                                    Width = _points[2] - _points[0],
                                    Height = _points[3] - _points[1],
                                    Text = _Text,
                                    FontSize = _FontSize,
                                    Foreground = new SolidColorBrush(fontcolor),
                                    Background = new SolidColorBrush(fillcolor),
                                    FontFamily = fontfamily,
                                    FontWeight = fontweight,
                                    TextAlignment = textalignment
                                };
                                if (effect != null)
                                    newtextbox.Effect = new DropShadowEffect() { ShadowDepth = 4, Direction = 315, Color = Colors.Black, Opacity = 0.5 };
                                
                                Canvas.SetLeft(newtextbox, _points[0]);
                                Canvas.SetTop(newtextbox, _points[1]);
                                newtextbox.Width = _points[2] - _points[0];
                                newtextbox.Height = _points[3] - _points[1];

                                //중요//////newtextbox.PointerEntered += newtextbox_PointerEntered;

                                newtextbox.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                                newtextbox.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                                newtextbox.MouseMove += UIElement_MouseMove;

                                canvas.Children.Add(newtextbox);
                            }

                            else if (uielement.Name == "Rectangle") //열기(사각형)
                            {
                                int p = 0;
                                XmlNode xmlnode;
                                List<double> _points = new List<double>();
                                while (p < 4)
                                {
                                    xmlnode = uielement.ChildNodes[p];
                                    _points.Add(double.Parse(xmlnode.InnerText));
                                    p++;
                                }

                                xmlnode = uielement.ChildNodes[p];
                                Byte stroke_Color_A = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte stroke_Color_R = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte stroke_Color_G = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte stroke_Color_B = Convert.ToByte(xmlnode.InnerText);
                                p++;

                                Color stroke_Color = new Color()
                                {
                                    A = stroke_Color_A,
                                    G = stroke_Color_G,
                                    R = stroke_Color_R,
                                    B = stroke_Color_B
                                };

                                Rectangle newrectangle = new Rectangle()
                                {
                                    Width = _points[2],
                                    Height = _points[3],
                                    Stroke = new SolidColorBrush(stroke_Color)
                                };


                                xmlnode = uielement.ChildNodes[p];
                                Byte fill_Color_A = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte fill_Color_R = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte fill_Color_G = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte fill_Color_B = Convert.ToByte(xmlnode.InnerText);

                                Color fill_color = new Color()
                                {
                                    A = fill_Color_A,
                                    G = fill_Color_G,
                                    R = fill_Color_R,
                                    B = stroke_Color_B
                                };
                                newrectangle.Fill = new SolidColorBrush(fill_color);
                                

                                Canvas.SetLeft(newrectangle, _points[0]);
                                Canvas.SetTop(newrectangle, _points[1]);

                                newrectangle.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                                newrectangle.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                                newrectangle.MouseMove += UIElement_MouseMove;

                                canvas.Children.Add(newrectangle);
                            }

                            else if (uielement.Name == "Ellipse") //열기(원)
                            {
                                int p = 0;
                                XmlNode xmlnode;
                                List<double> _points = new List<double>();
                                while (p < 4)
                                {
                                    xmlnode = uielement.ChildNodes[p];
                                    _points.Add(double.Parse(xmlnode.InnerText));
                                    p++;
                                }

                                xmlnode = uielement.ChildNodes[p];
                                Byte stroke_Color_A = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte stroke_Color_R = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte stroke_Color_G = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte stroke_Color_B = Convert.ToByte(xmlnode.InnerText);
                                p++;

                                Color stroke_Color = new Color()
                                {
                                    A = stroke_Color_A,
                                    G = stroke_Color_G,
                                    R = stroke_Color_R,
                                    B = stroke_Color_B
                                };

                                Ellipse newellipse = new Ellipse()
                                {
                                    Width = _points[2],
                                    Height = _points[3],
                                    Stroke = new SolidColorBrush(stroke_Color)
                                };

                                xmlnode = uielement.ChildNodes[p];
                                Byte fill_Color_A = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte fill_Color_R = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte fill_Color_G = Convert.ToByte(xmlnode.InnerText);
                                p++;
                                xmlnode = uielement.ChildNodes[p];
                                Byte fill_Color_B = Convert.ToByte(xmlnode.InnerText);

                                Color fill_color = new Color()
                                {
                                    A = fill_Color_A,
                                    G = fill_Color_G,
                                    R = fill_Color_R,
                                    B = stroke_Color_B
                                };

                                newellipse.Fill = new SolidColorBrush(fill_color);
                                

                                Canvas.SetLeft(newellipse, _points[0]);
                                Canvas.SetTop(newellipse, _points[1]);

                                //중요//////newtextbox.PointerEntered += newtextbox_PointerEntered;
                                newellipse.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                                newellipse.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                                newellipse.MouseMove += UIElement_MouseMove;

                                canvas.Children.Add(newellipse);
                            }

                            else if (uielement.Name == "Image") //열기(이미지)
                            {
                                int p = 0;
                                XmlNode xmlnode;
                                List<double> _points = new List<double>();
                                while (p < 4)
                                {
                                    xmlnode = uielement.ChildNodes[p];
                                    _points.Add(double.Parse(xmlnode.InnerText));
                                    p++;
                                }

                                xmlnode = uielement.ChildNodes[p];
                                p++;

                                Image newimage = new Image();
                                BitmapImage bitmap_Image = new BitmapImage();

                                bitmap_Image.BeginInit();
                                bitmap_Image.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap_Image.UriSource = new Uri(param_path + xmlnode.InnerText, UriKind.Absolute);
                                bitmap_Image.EndInit();
                                newimage.Source = bitmap_Image;
                                newimage.Stretch = Stretch.Fill;
                                ////////////////////////////////////////////20140724
                                Canvas.SetLeft(newimage, _points[0]);
                                Canvas.SetTop(newimage, _points[1]);
                                newimage.Width = _points[2] - _points[0];
                                newimage.Height = _points[3] - _points[1];

                                canvas.Children.Add(newimage);
                            }
                        }
                        canvas_List.Add(canvas);

                        XmlNodeList ani_list = slide.GetElementsByTagName("Animation");
                        foreach (XmlElement ani in ani_list)
                        {
                            string inner = ani.InnerText;
                            string[] inner_List = inner.Split('.');
                            int index = Convert.ToInt32(inner_List[0]);
                            string anime = inner_List[1];
                            Dictionary<int, string> add_dic = new Dictionary<int, string>();
                            animation_List.Add(add_dic);
                        }
                        animation_Dictionary.Add(i - 1, animation_List);
                    }
                }
                xmlFile.Close();
            }
            SetAllModeFalse();
            SetAllMoveModeFalse();
        }

        private void Save_Slides(string param_filename, string param_path)
        {
            int i = 1;
            foreach (KEAPCanvas canvas in canvas_List)
            {
                Transform transform = canvas.LayoutTransform;
                // reset current transform (in case it is scaled or rotated)
                canvas.LayoutTransform = null;

                // Get the size of canvas
                Size size = new Size(canvas.RenderSize.Width, canvas.RenderSize.Height);
                // Measure and arrange the surface
                // VERY IMPORTANT
                canvas.Measure(size);
                canvas.Arrange(new Rect(size));

                // Create a render bitmap and push the surface to it
                RenderTargetBitmap renderBitmap =
                  new RenderTargetBitmap(
                    (int)canvas.Width + 200,
                    (int)canvas.Height + 200,
                    96d,
                    96d,
                    PixelFormats.Default);
                renderBitmap.Render(canvas);

                // Create a file stream for saving image
                using (var fs = System.IO.File.OpenWrite(param_path + param_filename + "_Slide" + Convert.ToString(i++) + ".png"))
                {
                    // Use png encoder for our data
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    // push the rendered bitmap to it
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                    // save the data to the stream
                    encoder.Save(fs);
                    //fs.Dispose();
                    fs.Close();
                }


                // Restore previously saved layout
                canvas.LayoutTransform = transform;
            }
        }

        #region animation effects

        // TODO:움직이기 전 최초 좌표를 설정해줘야함. 지금은 임의로 -500값으로 진행중.
        // 왼쪽에서 등장
        private void BoundsLTR(FrameworkElement shape)
        {
            double x = 0.0;
            double y = 0.0;

            double xMove = 500;

            ThicknessAnimation bounceAnimation = new ThicknessAnimation();
            BounceEase BounceOrientation = new BounceEase();
            BounceOrientation.Bounces = 4;
            BounceOrientation.Bounciness = 2;
            bounceAnimation.From = new Thickness(143, 0, 0, 0);
            bounceAnimation.From = new Thickness(x - xMove, y, 0, 0);

            bounceAnimation.To = new Thickness(x, y, 0, 0);
            bounceAnimation.EasingFunction = BounceOrientation;

            shape.BeginAnimation(MarginProperty, bounceAnimation);
        }

        // TODO:움직이기 전 최초 좌표를 설정해줘야함. 지금은 임의로 -500값으로 진행중.
        // 오른쪽에서 등장
        private void BoundsRTL(FrameworkElement shape)
        {
            double x = 0.0;
            double y = 0.0;

            double xMove = 500;

            ThicknessAnimation bounceAnimation = new ThicknessAnimation();
            BounceEase BounceOrientation = new BounceEase();
            BounceOrientation.Bounces = 4;
            BounceOrientation.Bounciness = 2;
            bounceAnimation.From = new Thickness(143, 0, 0, 0);
            bounceAnimation.From = new Thickness(x + xMove, y, 0, 0);

            bounceAnimation.To = new Thickness(x, y, 0, 0);
            bounceAnimation.EasingFunction = BounceOrientation;

            shape.BeginAnimation(MarginProperty, bounceAnimation);
        }

        // TODO:움직이기 전 최초 좌표를 설정해줘야함. 지금은 임의로 -500값으로 진행중.
        // 위쪽에서 등장
        private void BoundsTTB(FrameworkElement shape)
        {
            double x = 0.0;
            double y = 0.0;
            double yMove = 500;

            ThicknessAnimation bounceAnimation = new ThicknessAnimation();
            BounceEase BounceOrientation = new BounceEase();
            BounceOrientation.Bounces = 4;
            BounceOrientation.Bounciness = 2;
            bounceAnimation.From = new Thickness(143, 0, 0, 0);
            bounceAnimation.From = new Thickness(x, y - yMove, 0, 0);

            bounceAnimation.To = new Thickness(x, y, 0, 0);
            bounceAnimation.EasingFunction = BounceOrientation;

            shape.BeginAnimation(MarginProperty, bounceAnimation);
        }

        // TODO:움직이기 전 최초 좌표를 설정해줘야함. 지금은 임의로 -500값으로 진행중.
        // 아래쪽에서 등장
        private void BoundsBTT(FrameworkElement shape)
        {
            double x = 0.0;
            double y = 0.0;

            double yMove = 500;

            ThicknessAnimation bounceAnimation = new ThicknessAnimation();
            BounceEase BounceOrientation = new BounceEase();
            BounceOrientation.Bounces = 4;
            BounceOrientation.Bounciness = 2;
            bounceAnimation.From = new Thickness(143, 0, 0, 0);
            bounceAnimation.From = new Thickness(x, y + yMove, 0, 0);

            bounceAnimation.To = new Thickness(x, y, 0, 0);
            bounceAnimation.EasingFunction = BounceOrientation;

            shape.BeginAnimation(MarginProperty, bounceAnimation);
        }

        // TODO:움직이기 전 최초 좌표를 설정해줘야함. 지금은 임의로 -500값으로 진행중.
        // 왼쪽에서 등장
        private void MoveLTR(FrameworkElement shape)
        {
            double x = 0.0;
            double y = 0.0;

            double xMove = 500;

            PowerEase power = new PowerEase();
            ThicknessAnimation linearAnimation = new ThicknessAnimation();
            linearAnimation.From = new Thickness(x - xMove, y, 0, 0);
            linearAnimation.To = new Thickness(x, y, 0, 0);
            linearAnimation.EasingFunction = power;

            shape.BeginAnimation(MarginProperty, linearAnimation);
        }

        // TODO:움직이기 전 최초 좌표를 설정해줘야함. 지금은 임의로 -500값으로 진행중.
        // 오른쪽에서 등장
        private void MoveRTL(FrameworkElement shape)
        {
            double x = 0.0;
            double y = 0.0;

            double xMove = 500;

            PowerEase power = new PowerEase();
            ThicknessAnimation linearAnimation = new ThicknessAnimation();
            linearAnimation.From = new Thickness(x + xMove, y, 0, 0);
            linearAnimation.To = new Thickness(x, y, 0, 0);
            linearAnimation.EasingFunction = power;

            shape.BeginAnimation(MarginProperty, linearAnimation);
        }

        // TODO:움직이기 전 최초 좌표를 설정해줘야함. 지금은 임의로 -500값으로 진행중.
        // 위쪽에서 등장
        private void MoveTTB(FrameworkElement shape)
        {
            double x = 0.0;
            double y = 0.0;

            double yMove = 500;

            PowerEase power = new PowerEase();
            ThicknessAnimation linearAnimation = new ThicknessAnimation();
            linearAnimation.From = new Thickness(x, y - yMove, 0, 0);
            linearAnimation.To = new Thickness(x, y, 0, 0);
            linearAnimation.EasingFunction = power;

            shape.BeginAnimation(MarginProperty, linearAnimation);
        }

        // TODO:움직이기 전 최초 좌표를 설정해줘야함. 지금은 임의로 -500값으로 진행중.
        // 아래쪽에서 등장
        private void MoveBTT(FrameworkElement shape)
        {
            double x = 0.0;
            double y = 0.0;

            double yMove = 500;

            PowerEase power = new PowerEase();
            ThicknessAnimation linearAnimation = new ThicknessAnimation();
            linearAnimation.From = new Thickness(x, y + yMove, 0, 0);
            linearAnimation.To = new Thickness(x, y, 0, 0);
            linearAnimation.EasingFunction = power;

            shape.BeginAnimation(MarginProperty, linearAnimation);
        }

        // 밝아졌다가 형체가 나타난다.
        private void FadeIn(FrameworkElement shape)
        {
            DoubleAnimation fadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(1), FillBehavior.Stop);
            shape.BeginAnimation(OpacityProperty, fadeIn);
        }

        // 어두워졌다가 밝아져서 형체가 사라진다.
        private void FadeOut(FrameworkElement shape)
        {
            DoubleAnimation fadeOut = new DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(1), FillBehavior.Stop);
            shape.BeginAnimation(OpacityProperty, fadeOut);
        }

        // 사각형이 커졌다 작아진다
        private void ZoomIn(FrameworkElement shape)
        {
            DoubleAnimation zoomIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(0.5), FillBehavior.Stop);

            CustomZoomInEaseOutFunction be = new CustomZoomInEaseOutFunction();
            be.EasingMode = EasingMode.EaseOut;

            ScaleTransform trans = new ScaleTransform();
            shape.RenderTransform = trans;
            shape.RenderTransformOrigin = new Point(0.5, 0.5);
            zoomIn.EasingFunction = be;
            trans.BeginAnimation(ScaleTransform.ScaleXProperty, zoomIn);
            trans.BeginAnimation(ScaleTransform.ScaleYProperty, zoomIn);
        }

        // 사각형이 작아진다 커졌다
        private void ZoomOut(FrameworkElement shape)
        {
            DoubleAnimation zoomOut = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(0.5), FillBehavior.Stop);

            CustomZoomOutEaseOutFunction be = new CustomZoomOutEaseOutFunction();
            be.EasingMode = EasingMode.EaseOut;

            ScaleTransform trans = new ScaleTransform();
            shape.RenderTransform = trans;
            shape.RenderTransformOrigin = new Point(0.5, 0.5);
            zoomOut.EasingFunction = be;
            trans.BeginAnimation(ScaleTransform.ScaleXProperty, zoomOut);
            trans.BeginAnimation(ScaleTransform.ScaleYProperty, zoomOut);
        }

        // 360도 회전, 0.5초
        private void Tornado(FrameworkElement shape)
        {
            DoubleAnimation dbRotate = new DoubleAnimation(0, 360, new Duration(TimeSpan.FromSeconds(0.5)));

            RotateTransform rotate = new RotateTransform();
            shape.RenderTransform = rotate;
            shape.RenderTransformOrigin = new Point(0.5, 0.5);
            rotate.BeginAnimation(RotateTransform.AngleProperty, dbRotate);
        }
        private static void circleAnimation(FrameworkElement shape)
        {
            CircleAnimation circleAnimationHelper = new CircleAnimation();
            //            circleAnimationHelper.MakeCircleAnimation((FrameworkElement)shape, shape.Width, shape.Height, new TimeSpan(0, 0, 1));
            circleAnimationHelper.MakeCircleAnimation((FrameworkElement)shape, shape.Width, shape.Height, TimeSpan.FromSeconds(1));
        }
        private static void interlacedAnimation(FrameworkElement shape)
        {
            InterlacedAnimation interlacedAnimation = new InterlacedAnimation();
            interlacedAnimation.MakeInterlacedAnimation((FrameworkElement)shape, shape.Width, shape.Height, TimeSpan.FromSeconds(1));
            //            interlacedAnimation.MakeInterlacedAnimation((FrameworkElement)shape, shape.Width, shape.Height, new TimeSpan(0, 0, 1));
        }

        private static void blockAnimation(FrameworkElement shape)
        {
            BlockAnimation blockAnimation = new BlockAnimation();
            //            blockAnimation.MakeBlockAnimation((FrameworkElement)shape, shape.Width, shape.Height, new TimeSpan(0, 0, 1));
            blockAnimation.MakeBlockAnimation((FrameworkElement)shape, shape.Width, shape.Height, TimeSpan.FromSeconds(1));
        }
        private static void radialAnimation(FrameworkElement shape)
        {
            RadialAnimation radialAnimation = new RadialAnimation();
            //            radialAnimation.MakeRadiaAnimation((FrameworkElement)shape, shape.Width, shape.Height, new TimeSpan(0, 0, 1));
            radialAnimation.MakeRadiaAnimation((FrameworkElement)shape, shape.Width, shape.Height, TimeSpan.FromMilliseconds(250));
        }
        private static void WaterFallAnimation(FrameworkElement shape)
        {
            WaterFallAnimation WaterFall = new WaterFallAnimation();
            //            WaterFall.MakeWaterFallAnimation((FrameworkElement)shape, shape.Width, shape.Height, new TimeSpan(0, 0, 1));
            WaterFall.MakeWaterFallAnimation((FrameworkElement)shape, shape.Width, shape.Height, TimeSpan.FromSeconds(1));
        }
        #endregion

        #region 애니메이션 Dictionary에 Mapping 부분
        private void BoundsLTR_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("BoundsLTR");
        }

        private void BoundsRTL_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("BoundsRTL");
        }

        private void BoundsTTB_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("BoundsTTB");
        }

        private void BoundsBTT_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("BoundsBTT");
        }

        private void MoveLTR_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("MoveLTR");
        }

        private void MoveRTL_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("MoveRTL");
        }

        private void MoveTTB_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("MoveTTB");
        }

        private void MoveBTT_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("MoveBTT");
        }

        private void FadeIn_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("FadeIn");
            
        }

        private void FadeOut_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("FadeOut");
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("ZoomIn"); 
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("ZoomOut");
        }

        private void Tornado_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("Tornado");
        }

        private void Circle_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("Circle");
        }

        private void Interlaced_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("Interlaced");
        }

        private void Block_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("Block");
        }

        private void Radial_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("Radial");
        }

        private void WaterFall_Click(object sender, RoutedEventArgs e)
        {
            AniManaging("WaterFall");
        }

        private void AniManaging(string anitype)
        {
            List<Dictionary<int, string>> ani_List = new List<Dictionary<int, string>>();
            if (animation_Dictionary.Count == Slide_ListView.Items.Count)
            {
                ani_List = animation_Dictionary[Slide_ListView.SelectedIndex];
            }
            else
            {
                ani_List = new List<Dictionary<int, string>>();
            }

            Dictionary<int, string> temp_dic = new Dictionary<int, string>();
            bool found = false;
            FrameworkElement ele = null;
            if (selected_Text != null) ele = selected_Text;
            else if (selected_Polygon != null) ele = selected_Polygon;
            else if (selected_Line != null) ele = selected_Line;
            else if (selected_Image != null) ele = selected_Image;
            else if (selected_Shape != null) ele = selected_Shape;
            else return;

            foreach (Dictionary<int, string> dic in ani_List)
            {
                if (dic.Keys.Contains(MainCanvas.Children.IndexOf(ele)))
                {
                    found = true;
                    if (dic[MainCanvas.Children.IndexOf(ele)] == anitype)
                        break;
                    else
                    {
                        string temp = dic[MainCanvas.Children.IndexOf(ele)];
                        temp = temp + "/" + anitype;
                    }
                }
            }
            if (!found)
            {
                temp_dic.Add(MainCanvas.Children.IndexOf(ele), anitype);
                ani_List.Add(temp_dic);
                animation_Dictionary[canvas_List.IndexOf(MainCanvas)] = ani_List;
            }
            Add_AnimationList(Slide_ListView.SelectedIndex);
        }

        #endregion

        #region Combobox 및 컬러 메뉴

        private void FontFamily_Combobox_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (FontFamily FontFamily in Fonts.SystemFontFamilies)
            {
                FontFamily_Combobox.Items.Add(FontFamily.Source);
            }
            FontFamily_Combobox.SelectedIndex = 0;
   
        }

        private void FontFamily_Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selected_Text != null)
                selected_Text.FontFamily = new FontFamily(FontFamily_Combobox.SelectedItem.ToString());
            font_Family = new FontFamily(FontFamily_Combobox.SelectedItem.ToString());
            font_Family_Index = FontFamily_Combobox.SelectedIndex;
        }

        private void FontSize_Combobox_Loaded(object sender, RoutedEventArgs e)
        {
            for(int i=10; i<60; i=i+2)
            {
                FontSize_Combobox.Items.Add(Convert.ToString(i));
            }
            FontSize_Combobox.SelectedIndex = 0;
        }

        private void FontSize_Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selected_Text != null)
                selected_Text.FontSize = Convert.ToDouble(FontSize_Combobox.SelectedItem.ToString());
            font_Size = Convert.ToDouble(FontSize_Combobox.SelectedItem.ToString());
            font_Size_Index = FontSize_Combobox.SelectedIndex;
        }

        private void ComboFontColor_Loaded(object sender, RoutedEventArgs e)
        {
            ComboFontColor.superCombo.SelectedIndex = 7;
            ComboFontColor.superCombo.SelectionChanged += new SelectionChangedEventHandler(ComboFontColor_SelectionChanged);
        }

        void ComboFontColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selected_Text != null)
                selected_Text.Foreground = ComboFontColor.SelectedColor;
            font_Brush = ComboFontColor.SelectedColor;
        }

        private void ComboFillColor_Loaded(object sender, RoutedEventArgs e)
        {
            if (ComboFillColor.superCombo.SelectedIndex == -1)
                ComboFillColor.superCombo.SelectedIndex = 137;
            else
                ComboFillColor.superCombo.SelectedIndex = 7;
            ComboFillColor.superCombo.SelectionChanged += new SelectionChangedEventHandler(ComboFillColor_SelectionChanged);
        }
        void ComboFillColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selected_Text != null)
                selected_Text.Background = ComboFillColor.SelectedColor;
            else if (selected_Polygon != null)
                selected_Polygon.Fill = ComboFillColor.SelectedColor;
            else if (selected_Shape != null)
                selected_Shape.Fill = ComboFillColor.SelectedColor;

            fill_Brush = ComboFillColor.SelectedColor;
        }
        #endregion

        private void ComboLineColor_Loaded(object sender, RoutedEventArgs e)
        {
            ComboLineColor.superCombo.SelectedIndex = 7;
            ComboLineColor.superCombo.SelectionChanged += new SelectionChangedEventHandler(ComboLineColor_SelectionChanged);
        }

        void ComboLineColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selected_Polygon != null)
                selected_Polygon.Stroke = ComboLineColor.SelectedColor;
            else if (selected_Line != null)
                selected_Line.Stroke = ComboLineColor.SelectedColor;
            else if (selected_Shape != null)
                selected_Shape.Stroke = ComboLineColor.SelectedColor;

            stroke_Brush = ComboLineColor.SelectedColor;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            Application.Current.Shutdown();
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
            if (Audience != null)
            {
                Audience.Close();
            }
            if (Presentor != null)
            {
                Presentor.Close();
            }
        }

        public void Close_SlideShow()
        {
            if (Audience != null)
            {
                Audience.Close();
            }
            if (Presentor != null)
            {
                Presentor.Close();
            }
        }

        private void Slide_Show_Click(object sender, RoutedEventArgs e)
        {
            SetAllModeFalse();
            SetAllMoveModeFalse();
            if (System.Windows.Forms.Screen.AllScreens.Length > 1) // if dual monitor
            {
                // create Window with second monitor
                Audience = new FullWindowForAudience(this, Presentor);
                Audience.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                System.Drawing.Rectangle working_Area = System.Windows.Forms.Screen.AllScreens[1].WorkingArea;
                Audience.Left = working_Area.Left;
                Audience.Top = working_Area.Top;
                Audience.Width = working_Area.Width;
                Audience.Height = working_Area.Height;
                //                Audience.WindowState = WindowState.Maximized;
                Audience.WindowStyle = WindowStyle.None;
                Audience.Topmost = true;
                Audience.Show();

                KinectSensor _sensor = KinectSensor.KinectSensors.FirstOrDefault();  /////

                if (_sensor == null)
                {
                    KinectAlert alert = new KinectAlert();
                    alert.ShowDialog();
                    return;
                }
                else { 

                    Presentor = new FullWindowForPresentor(this, Audience);
    //                FullWindowForPresentor Presentor = new FullWindowForPresentor();

                    Presentor.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                    System.Drawing.Rectangle working_Area2 = System.Windows.Forms.Screen.AllScreens[0].WorkingArea;
                    Presentor.Left = working_Area2.Left;
                    Presentor.Top = working_Area2.Top;
                    //SIZE
                    //                Presentor.Width = working_Area2.Width;
                    //                Presentor.Height = working_Area2.Height;
                    //Presentor.WindowState = WindowState.Maximized;
                    Presentor.WindowStyle = WindowStyle.None;
                    Presentor.Topmost = true;
                    Presentor.Show();
                }

             }
            else if (System.Windows.Forms.Screen.AllScreens.Length == 1) // no dual monitor
            {
                Audience = new FullWindowForAudience(this, Presentor);
                Audience.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                System.Drawing.Rectangle working_Area = System.Windows.Forms.Screen.AllScreens[0
                    ].WorkingArea;
                Audience.Left = working_Area.Left;
                Audience.Top = working_Area.Top;
                Audience.Width = working_Area.Width;
                Audience.Height = working_Area.Height;
                //                Audience.WindowState = WindowState.Maximized;
                Audience.WindowStyle = WindowStyle.None;
                Audience.Topmost = true;
                Audience.Show();
            }
            else
            {
                // elert
            }
        }

        bool BoldButton_Clicked = false;
        private void BoldButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (selected_Text == null) return;
            if (BoldButton_Clicked)
            {
                BoldButton.Background = new SolidColorBrush(Colors.White);
                selected_Text.FontWeight = FontWeights.Normal;
            }
            else
            {
                BoldButton.Background = new SolidColorBrush(Colors.DodgerBlue);
                selected_Text.FontWeight = FontWeights.Bold;
            }
            BoldButton_Clicked = !BoldButton_Clicked;
        }

        bool ItalicButton_Clicked = false;
        private void ItalicButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (selected_Text == null) return;
            if (ItalicButton_Clicked)
            {
                ItalicButton.Background = new SolidColorBrush(Colors.White);
                selected_Text.FontStyle = FontStyles.Normal;
            }
            else
            {
                ItalicButton.Background = new SolidColorBrush(Colors.DodgerBlue);
                selected_Text.FontStyle = FontStyles.Italic;
            }
            ItalicButton_Clicked = !ItalicButton_Clicked;
        }

        bool UnderlineButton_Clicked = false;
        private void UnderlineButton_Click(object sender, RoutedEventArgs e)
        {
            if (selected_Text == null) return;
            if (UnderlineButton_Clicked)
            {
                UnderlineButton.Background = new SolidColorBrush(Colors.White);
                selected_Text.TextDecorations.RemoveAt(0);
            }
            else
            {
                UnderlineButton.Background = new SolidColorBrush(Colors.DodgerBlue);
                selected_Text.TextDecorations.Add(TextDecorations.Underline);
            }
            UnderlineButton_Clicked = !UnderlineButton_Clicked;
        }

        bool ShadowButton_Clicked = false;
        private void ShadowButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShadowButton_Clicked)
            {
                ShadowButton.Background = new SolidColorBrush(Colors.White);
                selected_Text.Effect = null;
            }
            else
            {
                ShadowButton.Background = new SolidColorBrush(Colors.DodgerBlue);
                selected_Text.Effect = new DropShadowEffect() { ShadowDepth = 4, Direction = 315, Color = Colors.Black, Opacity = 0.5 };
            }
            ShadowButton_Clicked = !ShadowButton_Clicked;
        }

        private void NormalButton_Click(object sender, RoutedEventArgs e)
        {
            if (selected_Text == null) return;
            selected_Text.TextAlignment = TextAlignment.Justify;
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (selected_Text == null) return;
            selected_Text.TextAlignment = TextAlignment.Left;
        }

        private void CenterButton_Click(object sender, RoutedEventArgs e)
        {
            if (selected_Text == null) return;
            selected_Text.TextAlignment = TextAlignment.Center;
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            if (selected_Text == null) return;
            else selected_Text.TextAlignment = TextAlignment.Right;
        }

        private void MoveFront_Click(object sender, RoutedEventArgs e)
        {
            int index;
            if (selected_Text != null)
            {
                index = MainCanvas.Children.IndexOf(selected_Text);
                if (index == MainCanvas.Children.Count - 1) return;
                MainCanvas.Children.Remove(selected_Text);
                MainCanvas.Children.Insert(index+1, selected_Text);
            }
            else if (selected_Polygon != null)
            {
                index = MainCanvas.Children.IndexOf(selected_Polygon);
                if (index == MainCanvas.Children.Count - 1) return;
                MainCanvas.Children.Remove(selected_Polygon);
                MainCanvas.Children.Insert(index+1, selected_Polygon);
            }
            else if (selected_Line != null)
            {
                index = MainCanvas.Children.IndexOf(selected_Line);
                if (index == MainCanvas.Children.Count - 1) return;
                MainCanvas.Children.Remove(selected_Line);
                MainCanvas.Children.Insert(index+1, selected_Line);
            }
            else if (selected_Image != null)
            {
                index = MainCanvas.Children.IndexOf(selected_Image);
                if (index == MainCanvas.Children.Count - 1) return;
                MainCanvas.Children.Remove(selected_Image);
                MainCanvas.Children.Insert(index+1, selected_Image);
            }
            else if (selected_Shape != null)
            {
                index = MainCanvas.Children.IndexOf(selected_Shape);
                if (index == MainCanvas.Children.Count - 1) return;
                MainCanvas.Children.Remove(selected_Shape);
                MainCanvas.Children.Insert(index+1, selected_Shape);
            }
            Image image_renew = RenderCanvas(MainCanvas);
            ((SlideInfo)(Slide_ListView.Items[Slide_ListView.SelectedIndex])).Source = image_renew.Source;
        }

        private void MoveBack_Click(object sender, RoutedEventArgs e)
        {
            int index;
            if (selected_Text != null)
            {
                index = MainCanvas.Children.IndexOf(selected_Text);
                if (index == 0) return;
                MainCanvas.Children.Remove(selected_Text);
                MainCanvas.Children.Insert(index - 1, selected_Text);
            }
            else if (selected_Polygon != null)
            {
                index = MainCanvas.Children.IndexOf(selected_Polygon);
                if (index == 0) return;
                MainCanvas.Children.Remove(selected_Polygon);
                MainCanvas.Children.Insert(index - 1, selected_Polygon);
            }
            else if (selected_Line != null)
            {
                index = MainCanvas.Children.IndexOf(selected_Line);
                if (index == 0) return;
                MainCanvas.Children.Remove(selected_Line);
                MainCanvas.Children.Insert(index - 1, selected_Line);
            }
            else if (selected_Image != null)
            {
                index = MainCanvas.Children.IndexOf(selected_Image);
                if (index == 0) return;
                MainCanvas.Children.Remove(selected_Image);
                MainCanvas.Children.Insert(index - 1, selected_Image);
            }
            else if (selected_Shape != null)
            {
                index = MainCanvas.Children.IndexOf(selected_Shape);
                if (index == 0) return;
                MainCanvas.Children.Remove(selected_Shape);
                MainCanvas.Children.Insert(index - 1, selected_Shape);
            }
            Image image_renew = RenderCanvas(MainCanvas);
            ((SlideInfo)(Slide_ListView.Items[Slide_ListView.SelectedIndex])).Source = image_renew.Source;
        }

        bool cut_Mode = false;
        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            if (selected_Text != null)
            {
                selected_Object = selected_Text;
                MainCanvas.Children.Remove(selected_Text);
                cut_Mode = true;
            }
            else if (selected_Polygon != null)
            {
                selected_Object = selected_Polygon;
                MainCanvas.Children.Remove(selected_Polygon);
                cut_Mode = true;
            }
            else if (selected_Line != null)
            {
                selected_Object = selected_Line;
                MainCanvas.Children.Remove(selected_Line);
                cut_Mode = true;
            }
            else if (selected_Image != null)
            {
                selected_Object = selected_Image;
                MainCanvas.Children.Remove(selected_Image);
                cut_Mode = true;
            }
            else if (selected_Shape != null)
            {
                selected_Object = selected_Shape;
                MainCanvas.Children.Remove(selected_Shape);
                cut_Mode = true;
            }
            Image image_renew = RenderCanvas(MainCanvas);
            ((SlideInfo)(Slide_ListView.Items[Slide_ListView.SelectedIndex])).Source = image_renew.Source;
        }

        bool copy_Mode = false;
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (selected_Text != null)
            {
                selected_Object = selected_Text;
                copy_Mode = true;
            }
            else if (selected_Polygon != null)
            {
                selected_Object = selected_Polygon;
                copy_Mode = true;
            }
            else if (selected_Line != null)
            {
                selected_Object = selected_Line;
                copy_Mode = true;
            }
            else if (selected_Image != null)
            {
                selected_Object = selected_Image;
                copy_Mode = true;
            }
            else if (selected_Shape != null)
            {
                selected_Object = selected_Shape;
                copy_Mode = true;
            }
            Image image_renew = RenderCanvas(MainCanvas);
            ((SlideInfo)(Slide_ListView.Items[Slide_ListView.SelectedIndex])).Source = image_renew.Source;
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            if (cut_Mode || copy_Mode)
            {
                if (selected_Object is EditableTextBlock)
                {
                    EditableTextBlock copyele = selected_Object as EditableTextBlock;
                    EditableTextBlock textblock = new EditableTextBlock()
                    {
                        Text = copyele.Text,
                        Width = copyele.Width,
                        Height = copyele.Height,
                        FontSize = copyele.FontSize,
                        TextAlignment = copyele.TextAlignment,
                        Effect = copyele.Effect,
                        FontWeight = copyele.FontWeight,
                        FontFamily = copyele.FontFamily,
                        Background = copyele.Background,
                        Foreground = copyele.Foreground,
                    };
                    
                    MainCanvas.Children.Remove(edit_Rect);
                    
                    Canvas.SetLeft(textblock, (MainCanvas.Width - selected_Text.Width) / 2);
                    Canvas.SetTop(textblock, (MainCanvas.Height - selected_Text.Height) / 2);
                    textblock.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                    textblock.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                    textblock.MouseMove += UIElement_MouseMove;
                    MainCanvas.Children.Add(textblock);
                    selected_Text = textblock;
                    Canvas.SetLeft(edit_Rect, Canvas.GetLeft(textblock));
                    Canvas.SetTop(edit_Rect, Canvas.GetLeft(textblock));
                    
                    MainCanvas.Children.Add(edit_Rect);
                }
                else if (selected_Polygon != null) // not fixed
                {
                    Polygon copyele = selected_Object as Polygon;
                    Polygon polygon = new Polygon()
                    {
                        Fill = copyele.Fill,
                        Stroke = copyele.Stroke,
                        StrokeThickness = copyele.StrokeThickness
                    };
                    
                    MainCanvas.Children.Remove(edit_Rect);
                    
                    for (int i = 0; i < copyele.Points.Count; i++)
                    {
                        polygon.Points[i] = new Point(copyele.Points[i].X - (Canvas.GetLeft(edit_Rect) - (MainCanvas.Width - edit_Rect.Width) / 2), 
                            copyele.Points[i].Y - (Canvas.GetTop(edit_Rect) - (MainCanvas.Height - edit_Rect.Height) / 2));
                    }
                    polygon.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                    polygon.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                    polygon.MouseMove += UIElement_MouseMove;
                    selected_Polygon = polygon;
                    MainCanvas.Children.Add(polygon);
                    Canvas.SetLeft(edit_Rect, (MainCanvas.Width - edit_Rect.Width) / 2);
                    Canvas.SetTop(edit_Rect, (MainCanvas.Height - edit_Rect.Height) / 2);
                    
                    MainCanvas.Children.Add(edit_Rect);
                    edit_Mode = true;
                }
                else if (selected_Line != null) // not fixed
                {
                    Line copyele = selected_Object as Line;
                    Line line = new Line()
                    {
                        X1 = copyele.X1,
                        Y1 = copyele.Y1,
                        X2 = copyele.X2,
                        Y2 = copyele.Y2,
                        Stroke = copyele.Stroke,
                        StrokeThickness = copyele.StrokeThickness
                    };

                    MainCanvas.Children.Remove(edit_Rect);

                    line.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                    line.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                    line.MouseMove += UIElement_MouseMove;
                    selected_Line = line;
                    MainCanvas.Children.Add(line);
                    Canvas.SetLeft(edit_Rect, (MainCanvas.Width - edit_Rect.Width) / 2);
                    Canvas.SetTop(edit_Rect, (MainCanvas.Height - edit_Rect.Height) / 2);
                    
                    MainCanvas.Children.Add(edit_Rect);
                    edit_Mode = true;
                }
                else if (selected_Image != null)
                {
                    Image copyele = selected_Object as Image;
                    Image image = new Image()
                    {
                        Width = copyele.Width,
                        Height = copyele.Height,
                        Source = copyele.Source
                    };

                    MainCanvas.Children.Remove(edit_Rect);

                    Canvas.SetLeft(image, (MainCanvas.Width - selected_Shape.Width) / 2);
                    Canvas.SetTop(image, (MainCanvas.Height - selected_Shape.Height) / 2);
                    image.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                    image.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                    image.MouseMove += UIElement_MouseMove;
                    selected_Image = image;
                    MainCanvas.Children.Add(image);
                    Canvas.SetLeft(edit_Rect, (MainCanvas.Width - edit_Rect.Width) / 2);
                    Canvas.SetTop(edit_Rect, (MainCanvas.Height - edit_Rect.Height) / 2);
                    
                    MainCanvas.Children.Add(edit_Rect);
                    edit_Mode = true;
                }
                else if (selected_Shape != null)
                {
                    if (selected_Shape is Rectangle)
                    {
                        Rectangle copyele = selected_Object as Rectangle;
                        Rectangle rectangle = new Rectangle()
                        {
                            Width = copyele.Width,
                            Height = copyele.Height,
                            Fill = copyele.Fill,
                            Stroke = copyele.Stroke,
                            StrokeThickness = copyele.StrokeThickness
                        };

                        MainCanvas.Children.Remove(edit_Rect);

                        Canvas.SetLeft(rectangle, (MainCanvas.Width - selected_Shape.Width) / 2);
                        Canvas.SetTop(rectangle, (MainCanvas.Height - selected_Shape.Height) / 2);
                        rectangle.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                        rectangle.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                        rectangle.MouseMove += UIElement_MouseMove;
                        selected_Shape = rectangle;
                        MainCanvas.Children.Add(rectangle);
                        Canvas.SetLeft(edit_Rect, (MainCanvas.Width - edit_Rect.Width) / 2);
                        Canvas.SetTop(edit_Rect, (MainCanvas.Height - edit_Rect.Height) / 2);
                        
                        MainCanvas.Children.Add(edit_Rect);
                        edit_Mode = true;
                    }
                    else
                    {
                        Ellipse copyele = selected_Object as Ellipse;
                        Ellipse ellipse = new Ellipse()
                        {
                            Width = copyele.Width,
                            Height = copyele.Height,
                            Fill = copyele.Fill,
                            Stroke = copyele.Stroke,
                            StrokeThickness = copyele.StrokeThickness
                        };

                        MainCanvas.Children.Remove(edit_Rect);

                        Canvas.SetLeft(ellipse, (MainCanvas.Width - selected_Shape.Width) / 2);
                        Canvas.SetTop(ellipse, (MainCanvas.Height - selected_Shape.Height) / 2);
                        ellipse.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                        ellipse.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                        ellipse.MouseMove += UIElement_MouseMove;
                        selected_Shape = ellipse;
                        MainCanvas.Children.Add(ellipse);
                        Canvas.SetLeft(edit_Rect, (MainCanvas.Width - edit_Rect.Width) / 2);
                        Canvas.SetTop(edit_Rect, (MainCanvas.Height - edit_Rect.Height) / 2);
                        
                        MainCanvas.Children.Add(edit_Rect);
                        edit_Mode = true;
                    }
                }
            }

            if (cut_Mode) cut_Mode = false;
            
            Image image_renew = RenderCanvas(MainCanvas);
            ((SlideInfo)(Slide_ListView.Items[Slide_ListView.SelectedIndex])).Source = image_renew.Source;
        }

        private void Erase_Click(object sender, RoutedEventArgs e)
        {
            if (selected_Text != null)
                MainCanvas.Children.Remove(selected_Text);
            else if (selected_Polygon != null)
                MainCanvas.Children.Remove(selected_Polygon);
            else if (selected_Line != null)
                MainCanvas.Children.Remove(selected_Line);
            else if (selected_Image != null)
                MainCanvas.Children.Remove(selected_Image);
            else if (selected_Shape != null)
                MainCanvas.Children.Remove(selected_Shape);
        }

        private void SlideBackgroundSetButton_Click(object sender, RoutedEventArgs e)
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
                //image_Mode = true;

                canvas_Background_Bitmapimage = new BitmapImage();

                canvas_Background_Bitmapimage.BeginInit();
                canvas_Background_Bitmapimage.UriSource = new Uri(image_Path, UriKind.Absolute);
                canvas_Background_Bitmapimage.EndInit();

                canvas_Background = BitmapFrame.Create(canvas_Background_Bitmapimage);

                ImageBrush imagebrush = new ImageBrush(canvas_Background_Bitmapimage);
                imagebrush.Stretch = Stretch.Fill;
                int i = 0;
                foreach (KEAPCanvas canvas in canvas_List)
                {
                    canvas.Background = imagebrush;
                    Image image_renew = RenderCanvas(canvas);
                    Slides_List.ElementAt(i).Source = image_renew.Source;
                    ((SlideInfo)(Slide_ListView.Items[i])).Source = image_renew.Source;
                    i++;
                }
                
                MainCanvas.Background = imagebrush;
            }
        }

        public class Ani_List_Info
        {
            public string Number { get; set; }
            public string AnimationName { get; set; }
        }

        void Add_AnimationList(int index)
        {
            List<Dictionary<int,string>> ani_list = animation_Dictionary[index];
            Anis_List.Clear();
            int i = 1;
            foreach (Dictionary<int,string> dic in ani_list)
            {
                Anis_List.Add(new Ani_List_Info()
                {
                    Number = Convert.ToString(i),
                    AnimationName = dic.Values.First()
                });
                i++;
            }
            Animation_ListBox.ItemsSource = Anis_List;
            Animation_ListBox.UpdateLayout();
        }

        private void BoundsLTR_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            if (selected_Text != null) BoundsLTR(selected_Text);
            else if (selected_Polygon != null) BoundsLTR(selected_Polygon);
            else if (selected_Line != null) BoundsLTR(selected_Line);
            else if (selected_Image != null) BoundsLTR(selected_Image);
            else if (selected_Shape != null) BoundsLTR(selected_Shape);
            edit_Rect.Visibility = Visibility.Visible;
        }

        private void BoundsRTL_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            if (selected_Text != null) BoundsRTL(selected_Text);
            else if (selected_Polygon != null) BoundsRTL(selected_Polygon);
            else if (selected_Line != null) BoundsRTL(selected_Line);
            else if (selected_Image != null) BoundsRTL(selected_Image);
            else if (selected_Shape != null) BoundsRTL(selected_Shape);
            edit_Rect.Visibility = Visibility.Visible;
        }

        private void BoundsTTB_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            if (selected_Text != null) BoundsTTB(selected_Text);
            else if (selected_Polygon != null) BoundsTTB(selected_Polygon);
            else if (selected_Line != null) BoundsTTB(selected_Line);
            else if (selected_Image != null) BoundsTTB(selected_Image);
            else if (selected_Shape != null) BoundsTTB(selected_Shape);
            edit_Rect.Visibility = Visibility.Visible;
        }

        private void BoundsBTT_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            if (selected_Text != null) BoundsBTT(selected_Text);
            else if (selected_Polygon != null) BoundsBTT(selected_Polygon);
            else if (selected_Line != null) BoundsBTT(selected_Line);
            else if (selected_Image != null) BoundsBTT(selected_Image);
            else if (selected_Shape != null) BoundsBTT(selected_Shape);
            edit_Rect.Visibility = Visibility.Visible;
        }

        private void MoveLTR_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            if (selected_Text != null) MoveLTR(selected_Text);
            else if (selected_Polygon != null) MoveLTR(selected_Polygon);
            else if (selected_Line != null) MoveLTR(selected_Line);
            else if (selected_Image != null) MoveLTR(selected_Image);
            else if (selected_Shape != null) MoveLTR(selected_Shape);
            edit_Rect.Visibility = Visibility.Visible;
        }

        private void MoveRTL_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            if (selected_Text != null) MoveRTL(selected_Text);
            else if (selected_Polygon != null) MoveRTL(selected_Polygon);
            else if (selected_Line != null) MoveRTL(selected_Line);
            else if (selected_Image != null) MoveRTL(selected_Image);
            else if (selected_Shape != null) MoveRTL(selected_Shape);
            edit_Rect.Visibility = Visibility.Visible;
        }

        private void MoveTTB_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            if (selected_Text != null) MoveTTB(selected_Text);
            else if (selected_Polygon != null) MoveTTB(selected_Polygon);
            else if (selected_Line != null) MoveTTB(selected_Line);
            else if (selected_Image != null) MoveTTB(selected_Image);
            else if (selected_Shape != null) MoveTTB(selected_Shape);
            edit_Rect.Visibility = Visibility.Visible;
        }

        private void MoveBTT_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            if (selected_Text != null) MoveBTT(selected_Text);
            else if (selected_Polygon != null) MoveBTT(selected_Polygon);
            else if (selected_Line != null) MoveBTT(selected_Line);
            else if (selected_Image != null) MoveBTT(selected_Image);
            else if (selected_Shape != null) MoveBTT(selected_Shape);
            edit_Rect.Visibility = Visibility.Visible;
        }

        private void FadeIn_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            if (selected_Text != null) FadeIn(selected_Text);
            else if (selected_Polygon != null) FadeIn(selected_Polygon);
            else if (selected_Line != null) FadeIn(selected_Line);
            else if (selected_Image != null) FadeIn(selected_Image);
            else if (selected_Shape != null) FadeIn(selected_Shape);
            edit_Rect.Visibility = Visibility.Visible;
        }

        private void FadeOut_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            edit_Rect.UpdateLayout();
            if (selected_Text != null) { FadeOut(selected_Text); selected_Text.Visibility = Visibility.Visible; selected_Text.UpdateLayout(); }
            else if (selected_Polygon != null) { FadeOut(selected_Polygon); selected_Polygon.Visibility = Visibility.Visible; selected_Polygon.UpdateLayout(); }
            else if (selected_Line != null) { FadeOut(selected_Line); selected_Line.Visibility = Visibility.Visible; selected_Line.UpdateLayout(); }
            else if (selected_Image != null) { FadeOut(selected_Image); selected_Image.Visibility = Visibility.Visible; selected_Image.UpdateLayout(); }
            else if (selected_Shape != null) { FadeOut(selected_Shape); selected_Shape.Visibility = Visibility.Visible; selected_Shape.UpdateLayout(); }
            edit_Rect.Visibility = Visibility.Visible;
            
        }

        private void ZoomIn_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            if (selected_Text != null) ZoomIn(selected_Text);
            else if (selected_Polygon != null) ZoomIn(selected_Polygon);
            else if (selected_Line != null) ZoomIn(selected_Line);
            else if (selected_Image != null) ZoomIn(selected_Image);
            else if (selected_Shape != null) ZoomIn(selected_Shape);
            edit_Rect.Visibility = Visibility.Visible;
        }

        private void ZoomOut_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            if (selected_Text != null) ZoomOut(selected_Text);
            else if (selected_Polygon != null) ZoomOut(selected_Polygon);
            else if (selected_Line != null) ZoomOut(selected_Line);
            else if (selected_Image != null) ZoomOut(selected_Image);
            else if (selected_Shape != null) ZoomOut(selected_Shape);
            edit_Rect.Visibility = Visibility.Visible;
        }

        private void Tornado_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            if (selected_Text != null) Tornado(selected_Text);
            else if (selected_Polygon != null) Tornado(selected_Polygon);
            else if (selected_Line != null) Tornado(selected_Line);
            else if (selected_Image != null) Tornado(selected_Image);
            else if (selected_Shape != null) Tornado(selected_Shape);
            edit_Rect.Visibility = Visibility.Visible;
        }

        private void Circle_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            if (selected_Text != null) circleAnimation(selected_Text);
            else if (selected_Polygon != null) circleAnimation(selected_Polygon);
            else if (selected_Line != null) circleAnimation(selected_Line);
            else if (selected_Image != null) circleAnimation(selected_Image);
            else if (selected_Shape != null) circleAnimation(selected_Shape);
            edit_Rect.Visibility = Visibility.Visible;
        }

        private void Interlaced_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            if (selected_Text != null) interlacedAnimation(selected_Text);
            else if (selected_Polygon != null) interlacedAnimation(selected_Polygon);
            else if (selected_Line != null) interlacedAnimation(selected_Line);
            else if (selected_Image != null) interlacedAnimation(selected_Image);
            else if (selected_Shape != null) interlacedAnimation(selected_Shape);
            edit_Rect.Visibility = Visibility.Visible;
        }

        private void Block_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            if (selected_Text != null) blockAnimation(selected_Text);
            else if (selected_Polygon != null) blockAnimation(selected_Polygon);
            else if (selected_Line != null) blockAnimation(selected_Line);
            else if (selected_Image != null) blockAnimation(selected_Image);
            else if (selected_Shape != null) blockAnimation(selected_Shape);
            edit_Rect.Visibility = Visibility.Visible;
        }

        private void Radial_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            if (selected_Text != null) radialAnimation(selected_Text);
            else if (selected_Polygon != null) radialAnimation(selected_Polygon);
            else if (selected_Line != null) radialAnimation(selected_Line);
            else if (selected_Image != null) radialAnimation(selected_Image);
            else if (selected_Shape != null) radialAnimation(selected_Shape);
            edit_Rect.Visibility = Visibility.Visible;
        }

        private void WaterFall_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (edit_Rect == null) return;
            edit_Rect.Visibility = Visibility.Collapsed;
            if (selected_Text != null) WaterFallAnimation(selected_Text);
            else if (selected_Polygon != null) WaterFallAnimation(selected_Polygon);
            else if (selected_Line != null) WaterFallAnimation(selected_Line);
            else if (selected_Image != null) WaterFallAnimation(selected_Image);
            else if (selected_Shape != null) WaterFallAnimation(selected_Shape);
            edit_Rect.Visibility = Visibility.Visible;
        }

    }
}
