﻿using System;
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

namespace KEAP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<KEAPCanvas> canvas_List = new List<KEAPCanvas>();
        private ObservableCollection<SlideInfo> Slides_List = new ObservableCollection<SlideInfo>();
        private Dictionary<int, List<BitmapFrame>> bitmapFrame_Dictionary = new Dictionary<int, List<BitmapFrame>>();

        int previous_selection = -1;
        KEAPCanvas MainCanvas;
        
        bool pen_Mode = false, text_Mode = false, line_Mode = false, line_Mode_Toggle = false,
            rectangle_Mode = false, polygon_Mode = false, polygon_Mode_Toggle = false, image_Mode = false,
            table_Mode = false, ellipse_Mode = false;
        
        bool select_Mode = false;

        bool pen_Move_Mode = false, text_Move_Mode = false, line_Move_Mode = false,
            rectangle_Move_Mode = false, polygon_Move_Mode = false, image_Move_Mode = false,
            table_Move_Mode = false, ellipse_Move_Mode = false;
        bool drag_Mode = true;
        bool select_Shape_Mode = false, select_Line_Mode = false, select_Text_Mode = false;

        bool canvas_in_Mode = false;
        bool canvas_LeftButton_Down = false;
        bool poly_Start_Get = false;
        
        Point Start_Point, Start_Point_Poly;

        int prev_Count_Children = 0, prev_Poly_Count_Children=0;

        BitmapImage new_Bitmap_Image;
        string image_Path;
        Rectangle image_Rect;

        Shape selected_Shape;
        Line selected_Line;
        EditableTextBlock selected_Text;

        int table_Row_Count = 2, table_Column_Count = 3;


        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowSettings.current_WindowState;

            // 그냥 최대로 했음
            this.Width = WindowSettings.resolution_Width;
            this.Height = WindowSettings.resolution_Height;
            this.WindowState = WindowState.Maximized;
            MainCanvas = new KEAPCanvas()    
            {
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                Background=new SolidColorBrush(Colors.White)
                
            };
            /*if (((this.Width - this.Width * (92 / 876)) / (this.Height - 92)) < 1.6)
            {
                MainCanvas.Width = ((WindowSettings.resolution_Width - WindowSettings.resolution_Width * (92 / 876)) * 3.75 / 4.45) - 50; 
                MainCanvas.Height = MainCanvas.Width * 0.5625;
            }
            else
            {
                MainCanvas.Height = (WindowSettings.resolution_Height - 92) * (3 / 3.8) - 50;
                MainCanvas.Width = MainCanvas.Height * 16 / 9;
            }*/
            if ((this.Width * 3.75 / 4.45) < (((this.Height - 92) * 3 / 3.8) * 16 / 9))
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
            /*
            if (((this.Width - this.Width * (92 / 876)) / (this.Height - 92)) < 1.6)
            {
                MainCanvas.Width = ((WindowSettings.resolution_Width - WindowSettings.resolution_Width * (92 / 876)) * 3.75 / 4.45) - 50;
                MainCanvas.Height = MainCanvas.Width * 0.5625;
            }
            else
            {
                MainCanvas.Height = (WindowSettings.resolution_Height - 92) * (3 / 3.8) - 50; 
                MainCanvas.Width = MainCanvas.Height * 16 / 9;
            }*/
            if (this.WindowState == WindowState.Maximized)
            {
                MainCanvas.Height = (WindowSettings.current_Height - 92) * (3 / 3.8) - 50;
                MainCanvas.Width = (MainCanvas.Height * (WindowSettings.resolution_Width / WindowSettings.resolution_Height));
            }
            else if ((this.Width * 3.75 / 4.45) < (((this.Height - 92) * 3 / 3.8) * 16 / 9))
            {
                MainCanvas.Width = (WindowSettings.current_Width * 3.75 / 4.45) - 50;
                MainCanvas.Height = (MainCanvas.Width * (WindowSettings.resolution_Height / WindowSettings.resolution_Width));
            }
            else
            {
                MainCanvas.Height = (WindowSettings.current_Height - 92) * (3 / 3.8) - 50;
                MainCanvas.Width = (MainCanvas.Height * (WindowSettings.resolution_Width / WindowSettings.resolution_Height));
            }

            Autoedit_Slide_List(MainCanvas, Slide_ListView.SelectedIndex);
            MainCanvas.Measure(new Size(MainCanvas.Width, MainCanvas.Height));
            MainCanvas.Arrange(new Rect(0, 0, MainCanvas.Width, MainCanvas.Height));
        }

        #region MainCanvas 함수
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
                image.MouseEnter += UIElement_MouseEnter;
                image.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                image.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                image.MouseMove += UIElement_MouseMove;
                image.MouseLeave += UIElement_MouseLeave;
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
                grid.MouseEnter += UIElement_MouseEnter;
                grid.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                grid.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                grid.MouseMove += UIElement_MouseMove;
                grid.MouseLeave += UIElement_MouseLeave;

                MainCanvas.Children.Add(grid);
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
                }
            }
            canvas_LeftButton_Down = false;
            Autoedit_Slide_List(MainCanvas, Slide_ListView.SelectedIndex);   
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
                        MainCanvas.Children.Add(line);
                    }
                }

                else if (text_Mode)
                {
                    int area1 = 0, area2 = 0, area3 = 0;
                    EditableTextBlock textblock = new EditableTextBlock()
                    {
                        Background = new SolidColorBrush(Colors.LightGray),
                        HorizontalAlignment=HorizontalAlignment.Stretch,
                        VerticalAlignment=VerticalAlignment.Stretch
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
                    textblock.MouseEnter += UIElement_MouseEnter;
                    textblock.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                    textblock.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                    textblock.MouseMove += UIElement_MouseMove;
                    textblock.MouseLeave += UIElement_MouseLeave;
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
                    rectangle.MouseEnter += UIElement_MouseEnter;
                    rectangle.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                    rectangle.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                    rectangle.MouseMove += UIElement_MouseMove;
                    rectangle.MouseLeave += UIElement_MouseLeave;
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
                    double block_Width = ((Grid)MainCanvas.Children[prev_Count_Children]).Width / table_Column_Count,
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
                    ellipse.Stroke = new SolidColorBrush(Colors.Green);
                    if (area1 == 1) { Canvas.SetLeft(ellipse, x1); Canvas.SetTop(ellipse, y1); }
                    else if (area2 == 1) { Canvas.SetLeft(ellipse, x2); Canvas.SetTop(ellipse, y1); }
                    else if (area3 == 1) { Canvas.SetLeft(ellipse, x2); Canvas.SetTop(ellipse, y2); }
                    else { Canvas.SetLeft(ellipse, x1); Canvas.SetTop(ellipse, y2); }

                    if (prev_Count_Children != MainCanvas.Children.Count)
                    {
                        MainCanvas.Children.RemoveAt(prev_Count_Children);
                    }
                    ellipse.MouseEnter += UIElement_MouseEnter;
                    ellipse.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                    ellipse.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                    ellipse.MouseMove += UIElement_MouseMove;
                    ellipse.MouseLeave += UIElement_MouseLeave;
                    MainCanvas.Children.Add(ellipse);
                }
            }else
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

                        if (prev_Count_Children != MainCanvas.Children.Count)
                        {
                            MainCanvas.Children.RemoveAt(prev_Count_Children);
                        }
                        line.MouseEnter += UIElement_MouseEnter;
                        line.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
                        line.MouseLeftButtonUp += UIElement_MouseLeftButtonUp;
                        line.MouseMove += UIElement_MouseMove;
                        line.MouseLeave += UIElement_MouseLeave;
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

                    // :TODO 
                    // Polygon
                    MainCanvas.Children.Add(line);
                }
            }
        }

        // TODO
        void UIElement_MouseEnter(object sender, MouseEventArgs e)
        {
            if(IsSelectMode())
            {
                if(sender is EditableTextBlock)
                {
                    select_Text_Mode = true;
                    selected_Text = (EditableTextBlock)sender;
                }
                else if (sender is Line)
                {
                    select_Line_Mode = true;
                    selected_Line = (Line)sender;

                }
                else if (sender is Shape)
                {
                    select_Shape_Mode = true;
                    Console.WriteLine("shape enter");

                    selected_Shape = (Shape)sender;
                }
            }
        }

        // 객체에 마우스를 놓고 클릭하면 드래그 가능하다.
        private void UIElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (select_Text_Mode && selected_Text != null)
            {
                drag_Mode = true;
                selected_Text.CaptureMouse();
            }
            else if (select_Line_Mode && selected_Line != null)
            {
                drag_Mode = true;
                selected_Line.CaptureMouse();
            }
            else if (select_Shape_Mode && selected_Shape != null)
            {
                drag_Mode = true;
                Console.WriteLine("Capture");
                selected_Shape.CaptureMouse();
            }
        }

        // 객체에 마우스를 놓고 떼면 드래그 설정이 풀린다.
        // 이 때 객체를 선택했는지 안했는지를 설정한다.
        private void UIElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (select_Text_Mode && selected_Text != null)
            {
                drag_Mode = false;
                select_Mode = !select_Mode;
                selected_Text.ReleaseMouseCapture();
            }
            else if (select_Line_Mode && selected_Line != null)
            {
                drag_Mode = false;
                select_Mode = !select_Mode;
                selected_Line.ReleaseMouseCapture();
            }
            else if (select_Shape_Mode && selected_Shape != null)
            {
                drag_Mode = false;
                select_Mode = !select_Mode;
                selected_Shape.ReleaseMouseCapture();
            }
        }

        double m_left;
        double m_top;
        private void UIElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsSelectMode() || !drag_Mode) return;
            if(e.LeftButton == MouseButtonState.Pressed)
            { 
                Point End_Point = e.GetPosition(MainCanvas);
                if (select_Text_Mode && selected_Text != null)
                {
                    if (End_Point.X < selected_Text.Width / 2)
                        m_left = 0;
                    else if (End_Point.X < MainCanvas.Width - (selected_Text.Width / 2)) // TODO
                        m_left = End_Point.X - (selected_Text.Width / 2);
                    else
                        m_left = MainCanvas.ActualWidth - selected_Text.Width;

                    if (End_Point.Y < selected_Text.Height / 2)
                        m_top = 0;
                    else if (End_Point.Y < MainCanvas.Height - (selected_Text.Height/ 2)) // TODO
                        m_top = End_Point.Y - (selected_Text.Height / 2);
                    else
                        m_top = MainCanvas.ActualHeight - selected_Text.Height;

                    Canvas.SetLeft(selected_Text, m_left);
                    Canvas.SetTop(selected_Text, m_top);
                }
                else if (select_Line_Mode && selected_Line != null)
                {
                    if (End_Point.X < selected_Line.Width / 2)
                        m_left = 0;
                    else if (End_Point.X < MainCanvas.Width - selected_Line.Width)
                        m_left = End_Point.X - (selected_Line.Width / 2);
                    else
                        m_left = MainCanvas.ActualWidth - selected_Line.Width;

                    if (End_Point.Y < selected_Line.Height / 2)
                        m_top = 0;
                    else if (End_Point.Y < MainCanvas.Height - selected_Line.Height)
                        m_top = End_Point.Y - (selected_Line.Height / 2);
                    else
                        m_top = MainCanvas.ActualHeight - selected_Line.Height;

                    Canvas.SetLeft(selected_Line, m_left);
                    Canvas.SetTop(selected_Line, m_top);
                }
                else if (select_Shape_Mode && selected_Shape != null)
                {
                    if (End_Point.X < selected_Shape.Width / 2)
                        m_left = 0;
                    else if (End_Point.X < MainCanvas.Width - selected_Shape.Width)
                        m_left = End_Point.X - (selected_Shape.Width / 2);
                    else
                        m_left = MainCanvas.ActualWidth - selected_Shape.Width;

                    if (End_Point.Y < selected_Shape.Height / 2)
                        m_top = 0;
                    else if (End_Point.Y < MainCanvas.Height - selected_Shape.Height)
                        m_top = End_Point.Y - (selected_Shape.Height / 2);
                    else
                        m_top = MainCanvas.ActualHeight - selected_Shape.Height;

                    Canvas.SetLeft(selected_Shape, m_left);
                    Canvas.SetTop(selected_Shape, m_top);
                }
            }
        }

        void UIElement_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsSelectMode())
            {
                select_Mode = false;
                if (sender is EditableTextBlock)
                {
                    select_Text_Mode = false;
//                    selected_Text = null;
                }
                else if (sender is Line)
                {
                    select_Line_Mode = false;
//                    selected_Line = null;
                }
                else if (sender is Shape)
                {
                    Console.WriteLine("shape");

                    select_Shape_Mode = false;
//                    selected_Shape = null;
                }
            }
        }
        #endregion

        #region 버튼Boolean Settion
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

        private void Ellipse_Click(object sender, RoutedEventArgs e)
        {
            SetAllModeFalse();
            ellipse_Mode = true;
        }


        void SetAllMoveModeFalse()
        {
            pen_Move_Mode = text_Move_Mode = line_Move_Mode = rectangle_Move_Mode = polygon_Move_Mode = image_Move_Mode = table_Move_Mode = ellipse_Move_Mode = false;
        }


        #endregion

        private bool IsSelectMode()
        {
            return !(pen_Mode || text_Mode || line_Mode || rectangle_Mode || polygon_Mode || image_Mode || table_Mode || ellipse_Mode);
        }
        
        private double GetDistance(double x1, double y1, double x2, double y2)
        {
            double d = 0;
            d = Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
            return d;
        }

        private void BackMenu_Click(object sender, RoutedEventArgs e)
        {
            AllMenuBorderToWhite();
            BackMenu.BorderBrush = new SolidColorBrush(Colors.LightGray);
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
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Background = new SolidColorBrush(Colors.White)
            };
            /*
            if (((this.Width - this.Width * (92 / 876)) / (this.Height - 92)) < 1.6)
            {

                new_Canvas.Width = ((WindowSettings.resolution_Width - WindowSettings.resolution_Width * (92 / 876)) * 3.75 / 4.45) - 50; //= ((WindowSettings.resolution_Width - WindowSettings.resolution_Width * (92 / 876)) * 3.75 / 4.45) - 15*2
                new_Canvas.Height = new_Canvas.Width * 0.5625;
            }
            else
            {
                new_Canvas.Height = (WindowSettings.resolution_Height - 92) * (3 / 3.8) - 50; // = (WindowSettings.resolution_Height - 92) * (3 / 3.8) - 15*2;
                new_Canvas.Width = new_Canvas.Height * 16 / 9;
            }*/
            if ((this.Width * 3.75 / 4.45) < (((this.Height - 92) * 3 / 3.8) * 16 / 9))
            {
                new_Canvas.Width = (WindowSettings.current_Width * 3.75 / 4.45) - 50;
                new_Canvas.Height = (new_Canvas.Height * (WindowSettings.resolution_Height / WindowSettings.resolution_Width));
            }
            else
            {
                new_Canvas.Height = (WindowSettings.current_Height - 92) * (3 / 3.8) - 50;
                new_Canvas.Width = (new_Canvas.Height * (WindowSettings.resolution_Width / WindowSettings.resolution_Height));
            }

            new_Canvas.PreviewMouseLeftButtonDown += MainCanvas_PreviewMouseLeftButtonDown;
            new_Canvas.PreviewMouseLeftButtonUp += MainCanvas_PreviewMouseLeftButtonUp;
            new_Canvas.PreviewMouseMove += MainCanvas_PreviewMouseMove;

            MainCanvas_Border.Child = new_Canvas;
            canvas_List.Add(new_Canvas);

            Add_Slide_List(canvas_List.Count);
            //Slide_ListView.SelectedIndex = canvas_List.Count - 1;
            //Change_Slide(canvas_List.Count - 1);

            Autoedit_Slide_List(new_Canvas, canvas_List.Count-1);
        }

        private void Slide_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Slide_ListView.SelectedIndex == -1 && previous_selection!=-1)
                Slide_ListView.SelectedIndex = 0;
            if (previous_selection != Slide_ListView.SelectedIndex)
            {
                previous_selection = Slide_ListView.SelectedIndex;
            }
            MainCanvas_Border.Child = canvas_List.ElementAt(Slide_ListView.SelectedIndex);
            MainCanvas = canvas_List.ElementAt(Slide_ListView.SelectedIndex);
        }


        public class SlideInfo
        {
            public ImageSource Source { get; set; }
            public string Number { get; set; }
            public double Image_Width { get; set; }
            public double Image_Height { get; set; }
        }

        private Image RenderCanvas(KEAPCanvas param_Canvas)
		{
			Image img = new Image();
			RenderTargetBitmap rtb = new RenderTargetBitmap((int)param_Canvas.Width + 200, (int)param_Canvas.Height + 200, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(param_Canvas);
			img.Source = rtb;
			return img;
		}

        void Add_Slide_List(int param_Number, int num=-1)
        {
            Image image = RenderCanvas(canvas_List.ElementAt(param_Number-1));

            if (num != -1)
            {
                Slides_List.Add(new SlideInfo()
                {
                    Source = image.Source,
                    Number = Convert.ToString(num),
                    Image_Width = (MainCanvas.Width * 0.75 / 4.45) - 15 + 200,
                    Image_Height = (((MainCanvas.Width * 0.75 / 4.45) - 15) * (MainCanvas.Height / MainCanvas.Width)) + 100
                });
            }
            else
            {
                Slides_List.Add(new SlideInfo()
                {
                    Source = image.Source,
                    Number = Convert.ToString(canvas_List.Count),
                    Image_Width = (MainCanvas.Width * 0.75 / 4.45) - 15 + 200,
                    Image_Height = (((MainCanvas.Width * 0.75 / 4.45) - 15) * (MainCanvas.Height / MainCanvas.Width)) + 100
                });
            }

            Slide_ListView.ItemsSource = Slides_List;
        }

        void Autoedit_Slide_List(KEAPCanvas param_Canvas, int param_Number)
        {
            Image image = RenderCanvas(param_Canvas);

            /*ObservableCollection<SlideInfo> new_Slides_List = new ObservableCollection<SlideInfo>();
            int i=0, count=Slide_ListView.Items.Count;
            while (i<count) { 
                new_Slides_List.Add((SlideInfo)Slide_ListView.Items[i]);
                i++;
            }
            new_Slides_List.Insert(param_Number,new SlideInfo()
            {
                Source = image.Source,
                Number = Convert.ToString(canvas_List.Count),
                Image_Width = (MainCanvas.Width*0.75/4.45)-15,
                Image_Height = ((MainCanvas.Width*0.75/4.45)-15)*(MainCanvas.Height/MainCanvas.Width)
            });
            
            new_Slides_List.RemoveAt(param_Number+1);
            *///Slide_ListView.ItemsSource = new_Slides_List;
            Slides_List.Insert(param_Number, new SlideInfo()
            {
                Source = image.Source,
                Number = Convert.ToString(param_Number+1),
                Image_Width = (MainCanvas.Width * 0.75 / 4.45) - 15,
                Image_Height = ((MainCanvas.Width * 0.75 / 4.45) - 15) * (MainCanvas.Height / MainCanvas.Width)
            });
            Slide_ListView.SelectedIndex = param_Number;
            Slides_List.RemoveAt(param_Number + 1);
            Slide_ListView.SelectedIndex = param_Number;
            ////Slides_List.RemoveAt(param_Number);
            //Slide_ListView.SelectedIndex = param_Number;
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
                string name = filename.Substring(0,filename.Length-5);
                
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
            //using (XmlReader xmlFile = XmlReader.Create(param_path + param_name + ".xml"))
            {
                XmlDocument xmlDoc;
                xmlDoc = new XmlDocument();
                //xmlDoc.lLoad(xmlFile);
                
                XmlElement file = xmlDoc.CreateElement("file");
                file.SetAttribute("Time", DateTime.Now.ToLongDateString() + "\\" + DateTime.Now.ToLongTimeString());
                file.SetAttribute("MainCanvas_Width", Convert.ToString(MainCanvas.Width));
                file.SetAttribute("MainCanvas_Height", Convert.ToString(MainCanvas.Height));
                int i = 1;
                foreach (KEAPCanvas canvas in canvas_List)
                {
                    XmlElement canvas_element = xmlDoc.CreateElement("Slide" + Convert.ToString(i));
                    List<BitmapFrame> bitmapFrame_List=null;
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

                            Brush stroke_Brush = thispolygon.Stroke;
                            XmlElement stroke_Color_Of_A = xmlDoc.CreateElement("Stroke_Color_A");//((Color)stroke_Brush.GetValue(SolidColorstroke_Brush.ColorProperty)).A;
                            stroke_Color_Of_A.InnerText = Convert.ToString(((Color)stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).A);
                            P.AppendChild(stroke_Color_Of_A);
                            XmlElement stroke_Color_Of_R = xmlDoc.CreateElement("Stroke_Color_R");
                            stroke_Color_Of_R.InnerText = Convert.ToString(((Color)stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).R);
                            P.AppendChild(stroke_Color_Of_R);
                            XmlElement stroke_Color_Of_G = xmlDoc.CreateElement("Stroke_Color_G");
                            stroke_Color_Of_G.InnerText = Convert.ToString(((Color)stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).G);
                            P.AppendChild(stroke_Color_Of_G);
                            XmlElement stroke_Color_Of_B = xmlDoc.CreateElement("Stroke_Color_B");
                            stroke_Color_Of_B.InnerText = Convert.ToString(((Color)stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).B);
                            P.AppendChild(stroke_Color_Of_B);

                            Brush fill_Brush = thispolygon.Fill;
                            XmlElement fill_Color_Of_A = xmlDoc.CreateElement("Fill_Color_A");//((Color)brush.GetValue(SolidColorBrush.ColorProperty)).A;
                            fill_Color_Of_A.InnerText = Convert.ToString(((Color)fill_Brush.GetValue(SolidColorBrush.ColorProperty)).A);
                            P.AppendChild(fill_Color_Of_A);
                            XmlElement fill_Color_Of_R = xmlDoc.CreateElement("Fill_Color_R");
                            fill_Color_Of_R.InnerText = Convert.ToString(((Color)fill_Brush.GetValue(SolidColorBrush.ColorProperty)).R);
                            P.AppendChild(fill_Color_Of_R);
                            XmlElement fill_Color_Of_G = xmlDoc.CreateElement("Fill_Color_G");
                            fill_Color_Of_G.InnerText = Convert.ToString(((Color)fill_Brush.GetValue(SolidColorBrush.ColorProperty)).G);
                            P.AppendChild(fill_Color_Of_G);
                            XmlElement fill_Color_Of_B = xmlDoc.CreateElement("Fill_Color_B");
                            fill_Color_Of_B.InnerText = Convert.ToString(((Color)fill_Brush.GetValue(SolidColorBrush.ColorProperty)).B);
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

                            Brush brush = thistextblock.Background;
                            XmlElement Color_of_A = xmlDoc.CreateElement("Color_A");//((Color)brush.GetValue(SolidColorBrush.ColorProperty)).A;
                            Color_of_A.InnerText = Convert.ToString(((Color)brush.GetValue(SolidColorBrush.ColorProperty)).A);
                            T.AppendChild(Color_of_A);
                            XmlElement Color_of_R = xmlDoc.CreateElement("Color_R");
                            Color_of_R.InnerText = Convert.ToString(((Color)brush.GetValue(SolidColorBrush.ColorProperty)).R);
                            T.AppendChild(Color_of_R);
                            XmlElement Color_of_G = xmlDoc.CreateElement("Color_G");
                            Color_of_G.InnerText = Convert.ToString(((Color)brush.GetValue(SolidColorBrush.ColorProperty)).G);
                            T.AppendChild(Color_of_G);
                            XmlElement Color_of_B = xmlDoc.CreateElement("Color_B");
                            Color_of_B.InnerText = Convert.ToString(((Color)brush.GetValue(SolidColorBrush.ColorProperty)).B);
                            T.AppendChild(Color_of_B);

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

                            Brush stroke_Brush = thisrectangle.Stroke;
                            XmlElement stroke_Color_Of_A = xmlDoc.CreateElement("Stroke_Color_A");//((Color)stroke_Brush.GetValue(SolidColorstroke_Brush.ColorProperty)).A;
                            stroke_Color_Of_A.InnerText = Convert.ToString(((Color)stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).A);
                            R.AppendChild(stroke_Color_Of_A);
                            XmlElement stroke_Color_Of_R = xmlDoc.CreateElement("Stroke_Color_R");
                            stroke_Color_Of_R.InnerText = Convert.ToString(((Color)stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).R);
                            R.AppendChild(stroke_Color_Of_R);
                            XmlElement stroke_Color_Of_G = xmlDoc.CreateElement("Stroke_Color_G");
                            stroke_Color_Of_G.InnerText = Convert.ToString(((Color)stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).G);
                            R.AppendChild(stroke_Color_Of_G);
                            XmlElement stroke_Color_Of_B = xmlDoc.CreateElement("Stroke_Color_B");
                            stroke_Color_Of_B.InnerText = Convert.ToString(((Color)stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).B);
                            R.AppendChild(stroke_Color_Of_B);

                            if (thisrectangle.Fill != null)
                            {
                                Brush fill_Brush = thisrectangle.Fill;
                                XmlElement fill_Color_Of_A = xmlDoc.CreateElement("Fill_Color_A");//((Color)brush.GetValue(SolidColorBrush.ColorProperty)).A;
                                fill_Color_Of_A.InnerText = Convert.ToString(((Color)fill_Brush.GetValue(SolidColorBrush.ColorProperty)).A);
                                R.AppendChild(fill_Color_Of_A);
                                XmlElement fill_Color_Of_R = xmlDoc.CreateElement("Fill_Color_R");
                                fill_Color_Of_R.InnerText = Convert.ToString(((Color)fill_Brush.GetValue(SolidColorBrush.ColorProperty)).R);
                                R.AppendChild(fill_Color_Of_R);
                                XmlElement fill_Color_Of_G = xmlDoc.CreateElement("Fill_Color_G");
                                fill_Color_Of_G.InnerText = Convert.ToString(((Color)fill_Brush.GetValue(SolidColorBrush.ColorProperty)).G);
                                R.AppendChild(fill_Color_Of_G);
                                XmlElement fill_Color_Of_B = xmlDoc.CreateElement("Fill_Color_B");
                                fill_Color_Of_B.InnerText = Convert.ToString(((Color)fill_Brush.GetValue(SolidColorBrush.ColorProperty)).B);
                                R.AppendChild(fill_Color_Of_B);
                            }

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

                            Brush stroke_Brush = thisellipse.Stroke;
                            XmlElement stroke_Color_Of_A = xmlDoc.CreateElement("Stroke_Color_A");//((Color)stroke_Brush.GetValue(SolidColorstroke_Brush.ColorProperty)).A;
                            stroke_Color_Of_A.InnerText = Convert.ToString(((Color)stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).A);
                            E.AppendChild(stroke_Color_Of_A);
                            XmlElement stroke_Color_Of_R = xmlDoc.CreateElement("Stroke_Color_R");
                            stroke_Color_Of_R.InnerText = Convert.ToString(((Color)stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).R);
                            E.AppendChild(stroke_Color_Of_R);
                            XmlElement stroke_Color_Of_G = xmlDoc.CreateElement("Stroke_Color_G");
                            stroke_Color_Of_G.InnerText = Convert.ToString(((Color)stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).G);
                            E.AppendChild(stroke_Color_Of_G);
                            XmlElement stroke_Color_Of_B = xmlDoc.CreateElement("Stroke_Color_B");
                            stroke_Color_Of_B.InnerText = Convert.ToString(((Color)stroke_Brush.GetValue(SolidColorBrush.ColorProperty)).B);
                            E.AppendChild(stroke_Color_Of_B);
                            
                            if (thisellipse.Fill != null)
                            {
                                Brush fill_Brush = thisellipse.Fill;
                                XmlElement fill_Color_Of_A = xmlDoc.CreateElement("Fill_Color_A");//((Color)brush.GetValue(SolidColorBrush.ColorProperty)).A;
                                fill_Color_Of_A.InnerText = Convert.ToString(((Color)fill_Brush.GetValue(SolidColorBrush.ColorProperty)).A);
                                E.AppendChild(fill_Color_Of_A);
                                XmlElement fill_Color_Of_R = xmlDoc.CreateElement("Fill_Color_R");
                                fill_Color_Of_R.InnerText = Convert.ToString(((Color)fill_Brush.GetValue(SolidColorBrush.ColorProperty)).R);
                                E.AppendChild(fill_Color_Of_R);
                                XmlElement fill_Color_Of_G = xmlDoc.CreateElement("Fill_Color_G");
                                fill_Color_Of_G.InnerText = Convert.ToString(((Color)fill_Brush.GetValue(SolidColorBrush.ColorProperty)).G);
                                E.AppendChild(fill_Color_Of_G);
                                XmlElement fill_Color_Of_B = xmlDoc.CreateElement("Fill_Color_B");
                                fill_Color_Of_B.InnerText = Convert.ToString(((Color)fill_Brush.GetValue(SolidColorBrush.ColorProperty)).B);
                                E.AppendChild(fill_Color_Of_B);
                            }

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
                string extractPath = fullpath.Substring(0,fullpath.Length-filename.Length) + "KEAP_extract_" + name + "\\";

                Directory.CreateDirectory(extractPath);
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    String xmlFile_Path="", xmlFile_Name="";
                    List<String> imageFile_Path_List = new List<String>();
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        entry.ExtractToFile(System.IO.Path.Combine(extractPath, entry.FullName));
                        if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        {
                            xmlFile_Path=extractPath;
                            xmlFile_Name=entry.FullName;    
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
                foreach (XmlElement file in xmlList)
                {
                    double canvas_Width = Convert.ToDouble(file.GetAttribute("MainCanvas_Width")),
                        canvas_Height = Convert.ToDouble(file.GetAttribute("MainCanvas_Height"));

                    int i = 1;
                    canvas_List = new List<KEAPCanvas>();
                    bitmapFrame_Dictionary = new Dictionary<int, List<BitmapFrame>>();
                   // Slide_ListView.SelectedIndex = -1;
                    Slides_List = new ObservableCollection<SlideInfo>();
                    
                    foreach (XmlElement slide in file.ChildNodes)
                    {
                        KEAPCanvas canvas = new KEAPCanvas();
                        canvas.Width = canvas_Width;
                        canvas.Height = canvas_Height;
                        foreach (XmlElement uielement in slide.ChildNodes)
                        {
                            if(uielement.Name=="Polygon") //열기(다각형)
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
                                

                                //중요////////newpolygon.PointerEntered += newpolygon_PointerEntered;

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
                                canvas.Children.Add(newline);
                            }

                            else if (uielement.Name=="EditableTextBlock") //열기(텍스트)
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

                                Color BGcolor = new Color()
                                {
                                    A = Color_A,
                                    G = Color_G,
                                    R = Color_R,
                                    B = Color_B
                                };

                                TextBox newtextbox = new TextBox()
                                {
                                    Width = _points[2] - _points[0],
                                    Height = _points[3] - _points[1],
                                    Text = _Text,
                                    FontSize = _FontSize,
                                    Background = new SolidColorBrush(BGcolor)
                                };
                                /*Thickness margin = newtextbox.Margin;
                                margin.Left = _points[0];
                                margin.Top = _points[1];
                                margin.Right = canvas.Width - _points[2];
                                margin.Bottom = canvas.Width - _points[3];
                                newtextbox.Margin = margin;*/

                                Canvas.SetLeft(newtextbox, _points[0]);
                                Canvas.SetTop(newtextbox, _points[1]);
                                newtextbox.Width = _points[2] - _points[0];
                                newtextbox.Height = _points[3] - _points[1];

                                //중요//////newtextbox.PointerEntered += newtextbox_PointerEntered;
                                //newtextbox.AcceptsReturn = false;
                                //newtextbox.PreventKeyboardDisplayOnProgrammaticFocus = false;
                                //newtextbox.ReleasePointerCaptures();
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

                                if (8 < uielement.ChildNodes.Count)
                                {
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
                                }

                                /*Thickness margin = newtextbox.Margin;
                                margin.Left = _points[0];
                                margin.Top = _points[1];
                                margin.Right = canvas.Width - _points[2];
                                margin.Bottom = canvas.Width - _points[3];
                                newtextbox.Margin = margin;*/

                                Canvas.SetLeft(newrectangle, _points[0]);
                                Canvas.SetTop(newrectangle, _points[1]);

                                //중요//////newtextbox.PointerEntered += newtextbox_PointerEntered;
                                //newtextbox.AcceptsReturn = false;
                                //newtextbox.PreventKeyboardDisplayOnProgrammaticFocus = false;
                                //newtextbox.ReleasePointerCaptures();
                                canvas.Children.Add(newrectangle);
                            }
                            
                            else if (uielement.Name=="Ellipse") //열기(원)
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

                                if (8 < uielement.ChildNodes.Count)
                                {
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
                                }
                                
                                /*Thickness margin = newtextbox.Margin;
                                margin.Left = _points[0];
                                margin.Top = _points[1];
                                margin.Right = canvas.Width - _points[2];
                                margin.Bottom = canvas.Width - _points[3];
                                newtextbox.Margin = margin;*/

                                Canvas.SetLeft(newellipse, _points[0]);
                                Canvas.SetTop(newellipse, _points[1]);

                                //중요//////newtextbox.PointerEntered += newtextbox_PointerEntered;
                                //newtextbox.AcceptsReturn = false;
                                //newtextbox.PreventKeyboardDisplayOnProgrammaticFocus = false;
                                //newtextbox.ReleasePointerCaptures();
                                canvas.Children.Add(newellipse);
                            }

                            else if (uielement.Name=="Image") //열기(이미지)
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
                                 /*
                                Thickness margin = newimage.Margin;
                                margin.Left = _points[0];
                                margin.Top = _points[1];
                                margin.Right = MainCanvas.Width-_points[2];
                                margin.Bottom = MainCanvas.Height-_points[3];
                                newimage.Margin = margin;
                                */
                                Canvas.SetLeft(newimage,_points[0]);
                                Canvas.SetTop(newimage,_points[1]);
                                newimage.Width=_points[2]-_points[0];
                                newimage.Height=_points[3]-_points[1];

                                canvas.Children.Add(newimage);
                                
                            }
                        }
                        canvas_List.Add(canvas);
                    }
                }
                xmlFile.Close();
            }
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
            double x = shape.Margin.Left;
            double y = shape.Margin.Top;
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
            double x = shape.Margin.Left;
            double y = shape.Margin.Top;
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
            double x = shape.Margin.Left;
            double y = shape.Margin.Top;
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
            double x = shape.Margin.Left;
            double y = shape.Margin.Top;
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
            double x = shape.Margin.Left;
            double y = shape.Margin.Top;
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
            double x = shape.Margin.Left;
            double y = shape.Margin.Top;
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
            double x = shape.Margin.Left;
            double y = shape.Margin.Top;
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
            double x = shape.Margin.Left;
            double y = shape.Margin.Top;
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
            DoubleAnimation fadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(1), FillBehavior.HoldEnd);
            shape.BeginAnimation(OpacityProperty, fadeIn);
        }

        // 어두워졌다가 밝아져서 형체가 사라진다.
        private void FadeOut(FrameworkElement shape)
        {
            DoubleAnimation fadeOut = new DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(1), FillBehavior.HoldEnd);
            shape.BeginAnimation(OpacityProperty, fadeOut);
        }

        // 사각형이 커졌다 작아진다
        private void ZoomIn(FrameworkElement shape)
        {
            DoubleAnimation zoomIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(0.5), FillBehavior.HoldEnd);

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
            DoubleAnimation zoomOut = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(0.5), FillBehavior.HoldEnd);

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
    }
}
