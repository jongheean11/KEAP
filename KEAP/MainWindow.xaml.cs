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

namespace KEAP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KEAPCanvas MainCanvas;

        bool pen_Mode = false, text_Mode = false, line_Mode = false, line_Mode_Toggle = false,
            rectangle_Mode = false, polygon_Mode = false, polygon_Mode_Toggle = false, image_Mode = false,
            table_Mode = false,
            canvas_LeftButton_Down=false;
        bool poly_Start_Get = false;
        
        Point Start_Point, Start_Point_Poly;

        int prev_Count_Children = 0, prev_Poly_Count_Children=0;
        public MainWindow()
        {
            InitializeComponent();
            
            // 그냥 최대로 했음
            this.Width = WindowSettings.resolution_Width;
            this.Height = WindowSettings.resolution_Height;

            MainCanvas = new KEAPCanvas()    
            {
                Width = (this.Width/4.45)*3.75,
                Height = (this.Height/4.32)*3,
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
        }

        void MainCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            canvas_LeftButton_Down = true;
            prev_Count_Children = MainCanvas.Children.Count;
            
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
            Start_Point = e.GetPosition(MainCanvas);
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

            if (e.LeftButton == MouseButtonState.Pressed)
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
                    TextBlock textblock = new TextBlock()
                    {
                        Width = x2 - x1,
                        Height = y2 - y1,
                        Background = new SolidColorBrush(Colors.Gray)
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

                }

                else if (table_Mode)
                {

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
            pen_Mode = text_Mode = line_Mode = rectangle_Mode = polygon_Mode = image_Mode = table_Mode = false;
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
        private void Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetAllModeFalse();
            image_Mode = true;
        }

        private void Image_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
        private void Table_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetAllModeFalse();
            table_Mode = true;
        }

        private void Table_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private double GetDistance(double x1, double y1, double x2, double y2)
        {
            double d = 0;
            d = Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
            return d;
        }
    }
}
