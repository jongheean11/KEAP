using KEAP.Animations;
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
using System.Windows.Media.Animation;
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

        KEAPCanvas AudienceCanvas;
        List<KEAPCanvas> canvas_arr;
        Dictionary<int, List<Dictionary<int, string>>> animations = new Dictionary<int, List<Dictionary<int, string>>>();
        int canvas_index = 0, animation_index = 0;
        List<int> animation_indexes = new List<int>();
        
        public FullWindowForAudience(MainWindow main, FullWindowForPresentor pre)
        {
            main_View = main;
            pre_View = pre;
            AddKeyBindings();

            InitializeComponent();
            int i = 0;
            
            canvas_arr = new List<KEAPCanvas>();
            bool ani_null = false;
            if (main.animation_Dictionary.Count == 0) ani_null = true;
            else animations = main.animation_Dictionary;
            
            foreach (KEAPCanvas canvas in main.canvas_List)
            {
                List<Dictionary<int, string>> anilist;
                if (ani_null)
                    anilist = null;
                else
                {
                    if (i < animations.Count)
                        anilist = animations[i];
                    else
                        anilist = null;
                }
                canvas_arr.Add(new KEAPCanvas());
                canvas_arr[i].Width = SystemParameters.WorkArea.Width;
                canvas_arr[i].Height = SystemParameters.PrimaryScreenHeight;
                canvas_arr[i].Background = canvas.Background;
                
                for (int j=0; j<main.canvas_List[i].Children.Count; j++)
                {
                    UIElement ele = main.canvas_List[i].Children[j];
                    if (ele is EditableTextBlock)
                    {
                        EditableTextBlock copyele = ele as EditableTextBlock;
                        EditableTextBlock textblock = new EditableTextBlock()
                        {
                            Text = copyele.Text,
                            Width = copyele.Width * (SystemParameters.WorkArea.Width / canvas.Width),
                            Height = copyele.Height * (SystemParameters.PrimaryScreenHeight / canvas.Height),
                            FontSize = copyele.FontSize,
                            TextAlignment = copyele.TextAlignment,
                            Effect = copyele.Effect,
                            FontWeight = copyele.FontWeight,
                            FontFamily = copyele.FontFamily,
                            Background = copyele.Background,
                            Foreground = copyele.Foreground,
                        };
                        Canvas.SetLeft(textblock, Canvas.GetLeft(copyele) * (SystemParameters.WorkArea.Width / canvas.Width) );
                        Canvas.SetTop(textblock, Canvas.GetTop(copyele) * (SystemParameters.PrimaryScreenHeight / canvas.Height) );

                        canvas_arr[i].Children.Add(textblock);
                    }
                    else if (ele is Polygon)
                    {
                        Polygon copyele = ele as Polygon;
                        Polygon polygon = new Polygon()
                        {
                            Points = copyele.Points,
                            Stroke = copyele.Stroke,
                            StrokeThickness = copyele.StrokeThickness,
                            Fill = copyele.Fill
                        };

                        Point[] copy_points = new Point[polygon.Points.Count];
                        PointCollection copy_collection = new PointCollection();
                        polygon.Points.CopyTo(copy_points, 0);
                        for (int p = 0; p < polygon.Points.Count; p++)
                        {
                            copy_points[p].X = copy_points[p].X * (SystemParameters.WorkArea.Width / canvas.Width) ;
                            copy_points[p].Y = copy_points[p].Y * (SystemParameters.PrimaryScreenHeight / canvas.Height) ;
                            copy_collection.Add(copy_points[p]);
                        }
                        polygon.Points = copy_collection;

                        canvas_arr[i].Children.Add(polygon);
                    }
                    else if (ele is Line)
                    {
                        Line copyele = ele as Line;
                        Line line = new Line()
                        {
                            X1 = copyele.X1 * (SystemParameters.WorkArea.Width / canvas.Width) ,
                            Y1 = copyele.Y1 * (SystemParameters.PrimaryScreenHeight / canvas.Height) ,
                            X2 = copyele.X2 * (SystemParameters.WorkArea.Width / canvas.Width) ,
                            Y2 = copyele.Y2 * (SystemParameters.PrimaryScreenHeight / canvas.Height) ,
                            Stroke = copyele.Stroke,
                            StrokeThickness = copyele.StrokeThickness,
                        };

                        canvas_arr[i].Children.Add(line);
                    }
                    else if (ele is Image)
                    {
                        Image copyele = ele as Image;
                        Image image = new Image()
                        {
                            Source = copyele.Source,
                            Width = copyele.Width * (SystemParameters.WorkArea.Width / canvas.Width) ,
                            Height = copyele.Height * (SystemParameters.PrimaryScreenHeight / canvas.Height) 
                        };
                        Canvas.SetLeft(image, Canvas.GetLeft(copyele) * (SystemParameters.WorkArea.Width / canvas.Width) );
                        Canvas.SetTop(image, Canvas.GetTop(copyele) * (SystemParameters.PrimaryScreenHeight / canvas.Height) );

                        canvas_arr[i].Children.Add(image);
                    }
                    else if (ele is Rectangle)
                    {
                        Rectangle copyele = ele as Rectangle;
                        Rectangle rectangle = new Rectangle()
                        {
                            Width = copyele.Width * (SystemParameters.WorkArea.Width / canvas.Width) ,
                            Height = copyele.Height * (SystemParameters.PrimaryScreenHeight / canvas.Height) ,
                            Fill = copyele.Fill,
                            Stroke = copyele.Stroke,
                            StrokeThickness = copyele.StrokeThickness
                        };
                        Canvas.SetLeft(rectangle, Canvas.GetLeft(copyele) * (SystemParameters.WorkArea.Width / canvas.Width) );
                        Canvas.SetTop(rectangle, Canvas.GetTop(copyele) * (SystemParameters.PrimaryScreenHeight / canvas.Height) );

                        canvas_arr[i].Children.Add(rectangle);
                    }
                    else if (ele is Ellipse)
                    {
                        Ellipse copyele = ele as Ellipse;
                        Ellipse ellipse = new Ellipse()
                        {
                            Width = copyele.Width * (SystemParameters.WorkArea.Width / canvas.Width) ,
                            Height = copyele.Height * (SystemParameters.PrimaryScreenHeight / canvas.Height) ,
                            Fill = copyele.Fill,
                            Stroke = copyele.Stroke,
                            StrokeThickness = copyele.StrokeThickness
                        };
                        Canvas.SetLeft(ellipse, Canvas.GetLeft(copyele) * (SystemParameters.WorkArea.Width / canvas.Width) );
                        Canvas.SetTop(ellipse, Canvas.GetTop(copyele) * (SystemParameters.PrimaryScreenHeight / canvas.Height) );

                        canvas_arr[i].Children.Add(ellipse);
                    }
                    //canvas_arr[i].Measure(new Size(SystemParameters.WorkArea.Width, SystemParameters.MaximizedPrimaryScreenHeight));
                    //canvas_arr[i].Arrange(new Rect(0, 0, SystemParameters.WorkArea.Width, SystemParameters.MaximizedPrimaryScreenHeight));
                }
                if (anilist != null)
                {
                    foreach (Dictionary<int, string> dic in anilist)
                    {
                        string ani = dic[dic.Keys.First()];
                        if (ani.Contains("Bounds") || ani.Contains("Move") || ani.Contains("FadeIn")
                            || ani.Contains("Interlaced") || ani.Contains("Block") || ani.Contains("Circle") || ani.Contains("Radial") || ani.Contains("WaterFall"))
                        {
                            canvas_arr[i].Children[dic.Keys.First()].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                i++;
            }
            
            /*
            for(i=0; i<main.animation_Dictionary.Count; i++)
            {
                animations.Add(i,new Dictionary<int,string>[main.animation_Dictionary[i].Count()]);
                main.animation_Dictionary[i].CopyTo(animations[i]);
            }*/
            
        }

        public FullWindowForAudience()
        {
            InitializeComponent();
        }

        private void Audience_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Maximized;
            Keyboard.Focus(this);
            AudienceCanvas = new KEAPCanvas()
            {
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(Colors.White)
            };
            AudienceCanvas.Width = SystemParameters.WorkArea.Width * (100 / 96);
            AudienceCanvas.Height = SystemParameters.MaximizedPrimaryScreenHeight * (100 / 96);
            AudienceCanvas = canvas_arr[0];
            AudienceCanvas.PreviewMouseLeftButtonDown += AudienceCanvas_PreviewMouseLeftButtonDown;
            AudienceCanvas.PreviewMouseRightButtonDown += AudienceCanvas_PreviewMouseRightButtonDown;
            AudienceGrid.Children.Add(AudienceCanvas);

            List<Dictionary<int, string>> anilist;
            if(canvas_index<animations.Count)
                anilist = animations[canvas_index];
            else
                anilist = null;
            if (anilist != null)
            {
                foreach (Dictionary<int, string> dic in anilist)
                {
                    animation_indexes.Add(dic.Keys.First());
                }
            }
        }

        void AudienceCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Slide_Turn(1);
        }

        void Slide_Turn(int p_Slide_Index)
        {
            if((canvas_index+p_Slide_Index)<0 || (canvas_index+p_Slide_Index)>=canvas_arr.Count) return;
            canvas_index+=p_Slide_Index;
            
            if (!(canvas_arr.Count > canvas_index)) return;
            AudienceCanvas.PreviewMouseLeftButtonDown -= AudienceCanvas_PreviewMouseLeftButtonDown;
            AudienceCanvas.PreviewMouseRightButtonDown -= AudienceCanvas_PreviewMouseRightButtonDown;
            AudienceGrid.Children.Remove(AudienceCanvas);
            AudienceCanvas = new KEAPCanvas()
            {
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(Colors.White)
            };
            AudienceCanvas.Width = SystemParameters.WorkArea.Width * (100 / 96);
            AudienceCanvas.Height = SystemParameters.MaximizedPrimaryScreenHeight * (100 / 96);

            AudienceCanvas = canvas_arr[canvas_index];
            AudienceCanvas.PreviewMouseLeftButtonDown += AudienceCanvas_PreviewMouseLeftButtonDown;
            AudienceCanvas.PreviewMouseRightButtonDown += AudienceCanvas_PreviewMouseRightButtonDown;
            AudienceGrid.Children.Add(AudienceCanvas);

            List<Dictionary<int, string>> anilist;
            if (canvas_index < animations.Count)
                anilist = animations[canvas_index];
            else
                anilist = null;
            if (anilist != null)
            {
                foreach (Dictionary<int, string> dic in anilist)
                {
                    animation_indexes.Add(dic.Keys.First());
                }
            }
            animation_index = 0;
        }

        void AudienceCanvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Animation_Turn();
        }

        private void Animation_Turn()
        {
            if (animation_indexes.Count <= animation_index) return;
            List<Dictionary<int, string>> anilist = animations[canvas_index];
            if (anilist.Count == 0) return;
            string ani = (anilist[animation_index])[animation_indexes[animation_index]];

            if (ani == null) return;
            AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])].Visibility = Visibility.Visible;
            if (ani == "BoundsLTR")
            {
                BoundsLTR(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }
            else if (ani == "BoundsRTL")
            {
                BoundsRTL(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }
            else if (ani == "BoundsTTB")
            {
                BoundsTTB(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }
            else if (ani == "BoundsBTT")
            {
                BoundsBTT(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }
            else if (ani == "MoveLTR")
            {
                MoveLTR(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }
            else if (ani == "MoveRTL")
            {
                MoveRTL(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }
            else if (ani == "MoveTTB")
            {
                MoveTTB(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }
            else if (ani == "MoveBTT")
            {
                MoveBTT(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }
            else if (ani == "FadeIn")
            {
                FadeIn(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }
            else if (ani == "FadeOut")
            {
                FadeOut(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }
            else if (ani == "ZoomIn")
            {
                ZoomIn(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }
            else if (ani == "ZoomOut")
            {
                ZoomOut(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }
            else if (ani == "Tornado")
            {
                Tornado(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }
            else if (ani == "Circle")
            {
                circleAnimation(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }
            else if (ani == "Interlaced")
            {
                interlacedAnimation(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }
            else if (ani == "Block")
            {
                blockAnimation(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }
            else if (ani == "Radial")
            {
                radialAnimation(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }
            else if (ani == "WaterFall")
            {
                WaterFallAnimation(AudienceCanvas.Children[Convert.ToInt32(animation_indexes[animation_index])] as FrameworkElement);
            }

            animation_index++;
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

            // right arrow button
            RoutedCommand key_Right_Arrow = new RoutedCommand();
            key_Right_Arrow.InputGestures.Add(new KeyGesture(Key.Right));
            CommandBindings.Add(new CommandBinding(key_Right_Arrow, Right_Arrow_KeyEventHandler));

            // left arrow button
            RoutedCommand key_Left_Arrow = new RoutedCommand();
            key_Left_Arrow.InputGestures.Add(new KeyGesture(Key.Left));
            CommandBindings.Add(new CommandBinding(key_Left_Arrow, Left_Arrow_KeyEventHandler));

            // up arrow button
            RoutedCommand key_Up_Arrow = new RoutedCommand();
            key_Up_Arrow.InputGestures.Add(new KeyGesture(Key.Up));
            CommandBindings.Add(new CommandBinding(key_Up_Arrow, Up_Arrow_KeyEventHandler));

            // down arrow button
            RoutedCommand key_Down_Arrow = new RoutedCommand();
            key_Down_Arrow.InputGestures.Add(new KeyGesture(Key.Down));
            CommandBindings.Add(new CommandBinding(key_Down_Arrow, Down_Arrow_KeyEventHandler));

            // space button
            RoutedCommand key_Space = new RoutedCommand();
            key_Space.InputGestures.Add(new KeyGesture(Key.Space));
            CommandBindings.Add(new CommandBinding(key_Space, Space_KeyEventHandler));

            //// minus button
            //RoutedCommand key_Minus = new RoutedCommand();
            //key_Minus.InputGestures.Add(new KeyGesture(Key.OemMinus));
            //CommandBindings.Add(new CommandBinding(key_Minus, Minus_KeyEventHandler));

            // enter button
            RoutedCommand key_Enter = new RoutedCommand();
            key_Enter.InputGestures.Add(new KeyGesture(Key.Enter));
            CommandBindings.Add(new CommandBinding(key_Enter, Enter_KeyEventHandler));

        }

        public void Enter_KeyEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            // Same with Right Arrow Button
            // move to next slide or next animation
            sub_Monitor.Text = "Enter_KeyEventHandler";
            Slide_Turn(1);
            //throw new NotImplementedException();
        }

        public void Minus_KeyEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            // ZoomIn action
            // Keyboard Arrow's action change
            sub_Monitor.Text = "Minus_KeyEventHandler";
            //throw new NotImplementedException();
        }

        bool zoom_Toggle = false;
        int zoom_X, zoom_Y;
        public void Space_KeyEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            // ZoomOut action
            // Keyboard Arrow's action change
            sub_Monitor.Text = "Plus_KeyEventHandler";
            if (!zoom_Toggle)
            {
                zoom_X = 2; zoom_Y = 2;
                ZoomIn(zoom_X, zoom_Y);
            }
            else ZoomOut();

            zoom_Toggle = !zoom_Toggle;
            //throw new NotImplementedException();
        }

        public void Down_Arrow_KeyEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            // if(ZoomIn) Move Down ;
            sub_Monitor.Text = "Down_Arrow_KeyEventHandler";
            if (zoom_Toggle)
            {
                if(zoom_Y<=0)
                {
                    ZoomIn(zoom_X, zoom_Y);
                }
                else ZoomIn(zoom_X, --zoom_Y);
            }
           // else
          //  {

          //  }
            //throw new NotImplementedException();
        }

        public void Up_Arrow_KeyEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            sub_Monitor.Text = "Up_Arrow_KeyEventHandler";
            if (zoom_Toggle)
            {
                if (zoom_Y >= 4)
                {
                    ZoomIn(zoom_X, zoom_Y);
                }
                else ZoomIn(zoom_X, ++zoom_Y);
            }
            else
                Animation_Turn();
        }

        public void Left_Arrow_KeyEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            // if(ZoomIn) Move Left;
            // else move to before slide or animation;
            sub_Monitor.Text = "Left_Arrow_KeyEventHandler";
            if (zoom_Toggle)
            {
                if (zoom_X <= 0)
                {
                    ZoomIn(zoom_X, zoom_Y);
                }
                else ZoomIn(--zoom_X, zoom_Y);
            }
            else
                Slide_Turn(-1);
            //throw new NotImplementedException();
        }

        public void Right_Arrow_KeyEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            // if(ZoomIn) Move Right;
            // else move to next slide or animation
            sub_Monitor.Text = "Right_Arrow_KeyEventHandler";
            if (zoom_Toggle)
            {
                if (zoom_X >= 4)
                {
                    ZoomIn(zoom_X, zoom_Y);
                }
                else ZoomIn(++zoom_X, zoom_Y);
            }
            else
                Slide_Turn(1);
            //throw new NotImplementedException();
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

        public void get_SwipeRight_From_Kinect()
        {
            sub_Monitor.Text = "get_SwipeRight_From_Kinect";
        }

        public void get_SwipeLeft_From_Kinect()
        {
            sub_Monitor.Text = "get_SwipeLeft_From_Kinect";
        }

        public void get_SwipeUp_From_Kinect()
        {
            sub_Monitor.Text = "get_SwipeUp_From_Kinect";
        }
        //
        public void get_Start_From_Kinect()
        {
            sub_Monitor.Text = "get_Start_From_Kinect";
        }

        public void get_ZoomIn_From_Kinect()
        {
            sub_Monitor.Text = "get_ZoomIn_From_Kinect";
        }

        public void get_ZoomOut_From_Kinect()
        {
            sub_Monitor.Text = "get_ZoomOut_From_Kinect";
        }

        public void get_Pause_From_Kinect()
        {
            sub_Monitor.Text = "get_Pause_From_Kinect";
        }

        public void get_RightDown_From_Kinect()
        {
            sub_Monitor.Text = "get_RightDown_From_Kinect";
        }

        public void get_Down_From_Kinect()
        {
            sub_Monitor.Text = "get_Down_From_Kinect";
        }

        public void get_RightUp_From_Kinect()
        {
            sub_Monitor.Text = "get_RightUp_From_Kinect";
        }

        public void get_LeftDown_From_Kinect()
        {
            sub_Monitor.Text = "get_LeftDown_From_Kinect";
        }

        public void get_LeftUp_From_Kinect()
        {
            sub_Monitor.Text = "get_LeftUp_From_Kinect";
        }

        public void get_Left_From_Kinect()
        {
            sub_Monitor.Text = "get_Left_From_Kinect";
        }

        public void get_Right_From_Kinect()
        {
            sub_Monitor.Text = "get_Right_From_Kinect";
        }

        public void get_Up_From_Kinect()
        {
            sub_Monitor.Text = "get_Up_From_Kinect";
        }

        public void get_Push_From_Kinect()
        {
            sub_Monitor.Text = "get_Push_From_Kinect";
        }

        public void get_StrechedHands_From_Kinect()
        {
            sub_Monitor.Text = "get_StrechedHands_From_Kinect";
        }


        //debugging
        public void get_Grip_From_Kinect()
        {
            //sub_Monitor.Text = "get_Grip_From_Kinect";
        }

        public void get_Release_From_Kinect()
        {
            //sub_Monitor.Text = "get_Release_From_Kinect";
        }

        public void getDataFromKinect(string data)
        {
          //  sub_Monitor.Text = data;
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
            shape.Visibility = Visibility.Collapsed;
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
        Image zoom_img;
        KEAPCanvas zoomCanvas;
        void ZoomIn(int x, int y)
        {
            zoomCanvas = new KEAPCanvas();
            zoomCanvas.Width = AudienceCanvas.Width;
            zoomCanvas.Height = AudienceCanvas.Height;
            zoomCanvas.Background = null;
            
            Rectangle rec_BG = new Rectangle()
            {
                Width = SystemParameters.WorkArea.Width,
                Height = SystemParameters.PrimaryScreenHeight,
                Stroke = null,
                Fill = AudienceCanvas.Background
            };
            Canvas.SetLeft(rec_BG, -(SystemParameters.WorkArea.Width * (100 / 96) * x / 8));
            Canvas.SetTop(rec_BG, -(WindowSettings.resolution_Height * (100 / 96) * y / 8));
            zoomCanvas.Children.Add(rec_BG);

            UIElement[] copyuiele = new UIElement[AudienceCanvas.Children.Count];
            AudienceCanvas.Children.CopyTo(copyuiele, 0);
            foreach (UIElement ele in copyuiele)
            {
                if (ele is EditableTextBlock)
                {
                    EditableTextBlock copyele = ele as EditableTextBlock;
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
                    Canvas.SetLeft(textblock,Canvas.GetLeft(copyele));
                    Canvas.SetTop(textblock,Canvas.GetTop(copyele));

                    double x1 = Canvas.GetLeft(ele),
                        y1 = Canvas.GetTop(ele);

                    Canvas.SetLeft(textblock, x1 - (SystemParameters.PrimaryScreenWidth * (100 / 100) * x / 8));
                    Canvas.SetTop(textblock, y1 - (SystemParameters.PrimaryScreenHeight * (100 / 100) * y / 8));

                    zoomCanvas.Children.Add(textblock);
                }
                else if (ele is Polygon)
                {
                    Polygon copyele = ele as Polygon;
                    Polygon polygon = new Polygon()
                    {
                        Points = copyele.Points,
                        Stroke = copyele.Stroke,
                        StrokeThickness = copyele.StrokeThickness,
                        Fill = copyele.Fill
                    };

                    Point[] copy_points = new Point[polygon.Points.Count];
                    PointCollection copy_collection = new PointCollection();
                    polygon.Points.CopyTo(copy_points, 0);
                    for (int i = 0; i < polygon.Points.Count; i++)
                    {
                        copy_points[i].X = copy_points[i].X - (SystemParameters.WorkArea.Width * (100 / 100) * x / 8);
                        copy_points[i].Y = copy_points[i].Y - (SystemParameters.PrimaryScreenHeight * (100 / 100) * y / 8);
                        copy_collection.Add(copy_points[i]);
                    }
                    polygon.Points = copy_collection;

                    zoomCanvas.Children.Add(polygon);
                }
                else if (ele is Line)
                {
                    Line copyele = ele as Line;
                    Line line = new Line()
                    {
                        X1 = copyele.X1,
                        Y1 = copyele.Y1,
                        X2 = copyele.X2,
                        Y2 = copyele.Y2,
                        Stroke = copyele.Stroke,
                        StrokeThickness = copyele.StrokeThickness,
                    };

                    line.X1 = ((Line)ele).X1 - (SystemParameters.WorkArea.Width * (100 / 100) * x / 8);
                    line.Y1 = ((Line)ele).Y1 - (SystemParameters.PrimaryScreenHeight * (100 / 100) * y / 8);
                    line.X2 = ((Line)ele).X2 - (SystemParameters.WorkArea.Width * (100 / 100) * y / 8);
                    line.Y2 = ((Line)ele).Y2 - (SystemParameters.PrimaryScreenHeight * (100 / 100) * y / 8);

                    zoomCanvas.Children.Add(line);
                }
                else if (ele is Image)
                {
                    Image copyele = ele as Image;
                    Image image = new Image()
                    {
                        Source = copyele.Source,
                        Width = copyele.Width,
                        Height = copyele.Height,
                    };
                    Canvas.SetLeft(image, Canvas.GetLeft(copyele));
                    Canvas.SetTop(image, Canvas.GetTop(copyele));

                    double x1 = Canvas.GetLeft(ele),
                        y1 = Canvas.GetTop(ele);

                    Canvas.SetLeft(image, x1 - (SystemParameters.WorkArea.Width * (100 / 100) * x / 8));
                    Canvas.SetTop(image, y1 - (SystemParameters.PrimaryScreenHeight * (100 / 100) * y / 8));

                    zoomCanvas.Children.Add(image);
                }
                else if (ele is Rectangle)
                {
                    Rectangle copyele = ele as Rectangle;
                    Rectangle rectangle = new Rectangle()
                    {
                        Width = copyele.Width,
                        Height = copyele.Height,
                        Fill = copyele.Fill,
                        Stroke = copyele.Stroke,
                        StrokeThickness = copyele.StrokeThickness
                    };
                    Canvas.SetLeft(rectangle, Canvas.GetLeft(copyele));
                    Canvas.SetTop(rectangle, Canvas.GetTop(copyele));

                    double x1 = Canvas.GetLeft(ele),
                        y1 = Canvas.GetTop(ele);

                    Canvas.SetLeft(rectangle, x1 - (SystemParameters.WorkArea.Width * (100 / 100) * x / 8));
                    Canvas.SetTop(rectangle, y1 - (SystemParameters.PrimaryScreenHeight * (100 / 100) * y / 8));

                    zoomCanvas.Children.Add(rectangle);
                }
                else if (ele is Ellipse)
                {
                    Ellipse copyele = ele as Ellipse;
                    Ellipse ellipse = new Ellipse()
                    {
                        Width = copyele.Width,
                        Height = copyele.Height,
                        Fill = copyele.Fill,
                        Stroke = copyele.Stroke,
                        StrokeThickness = copyele.StrokeThickness
                    };
                    Canvas.SetLeft(ellipse, Canvas.GetLeft(copyele));
                    Canvas.SetTop(ellipse, Canvas.GetTop(copyele));

                    double x1 = Canvas.GetLeft(ele),
                        y1 = Canvas.GetTop(ele);

                    Canvas.SetLeft(ellipse, x1 - (SystemParameters.WorkArea.Width * (100 / 100) * x / 8));
                    Canvas.SetTop(ellipse, y1 - (SystemParameters.PrimaryScreenHeight * (100 / 100) * y / 8));

                    zoomCanvas.Children.Add(ellipse);
                }
            }

            AudienceGrid.Background = null;
            zoomCanvas.UpdateLayout();
            zoom_img = new Image();

            FrameworkElement TargetVisual = zoomCanvas as FrameworkElement;
            int width, height;
            TargetVisual.Arrange(new Rect(0, 0, 0, 0));
            width = (int)TargetVisual.ActualWidth;
            height = (int)TargetVisual.ActualHeight;
           if(TargetVisual.RenderTransform!=null)
           {
               Rect bounds = new Rect(0, 0, TargetVisual.ActualWidth, TargetVisual.ActualHeight);
               bounds = TargetVisual.RenderTransform.TransformBounds(bounds);
               TargetVisual.Arrange(new Rect(-bounds.Left, -bounds.Top, width, height));
               width = (int)bounds.Width;
               height = (int)bounds.Height;
           }

            RenderTargetBitmap zoom_rtb = new RenderTargetBitmap(width/4 + 402, height/4 + 237, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            AudienceGrid.Children.Insert(0, zoomCanvas);
            AudienceGrid.Children.Remove(AudienceCanvas);
            zoomCanvas.UpdateLayout();
            AudienceGrid.UpdateLayout();
            zoom_rtb.Render(TargetVisual);
            zoom_img.Source = zoom_rtb;
            zoom_img.HorizontalAlignment = HorizontalAlignment.Stretch;
            zoom_img.VerticalAlignment = VerticalAlignment.Stretch;
            zoom_img.Stretch = Stretch.Fill;
            //AudienceGrid.Children.Remove(AudienceCanvas);
            zoom_img.Width = SystemParameters.WorkArea.Width;
            zoom_img.Height = SystemParameters.MaximizedPrimaryScreenHeight;
            
            AudienceGrid.Children.Add(zoom_img);
            
            AudienceGrid.Children.Remove(zoomCanvas);
            //AudienceGrid.Background = new ImageBrush(zoom_img.Source);
            //AudienceGrid.Background = new SolidColorBrush(Colors.Blue);
        }
        void ZoomOut()
        {
            if (AudienceGrid.Children.Contains(AudienceCanvas)) return;
            if (AudienceGrid.Children.Contains(zoomCanvas)) AudienceGrid.Children.Remove(zoomCanvas);
            if (AudienceGrid.Children.Contains(zoom_img)) AudienceGrid.Children.Remove(zoom_img);
            AudienceGrid.Children.Add(AudienceCanvas);
        }
    }
}
