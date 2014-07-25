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
            animations = main.animation_Dictionary;
            
            foreach (KEAPCanvas canvas in main.canvas_List)
            {
                List<Dictionary<int, string>> anilist;
                if (animations[i] == null)
                    anilist = null;
                else
                    anilist = animations[i];

                canvas_arr.Add(new KEAPCanvas());
                canvas_arr[i].Width = canvas.Width;
                canvas_arr[i].Height = canvas.Height;
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

                        canvas_arr[i].Children.Add(polygon);
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

                        canvas_arr[i].Children.Add(line);
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

                        canvas_arr[i].Children.Add(image);
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

                        canvas_arr[i].Children.Add(rectangle);
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

                        canvas_arr[i].Children.Add(ellipse);
                    }
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
            AudienceCanvas.Width = this.Width;
            AudienceCanvas.Height = this.Height;
            AudienceCanvas = canvas_arr[0];
            AudienceCanvas.PreviewMouseLeftButtonDown += AudienceCanvas_PreviewMouseLeftButtonDown;
            AudienceCanvas.PreviewMouseRightButtonDown += AudienceCanvas_PreviewMouseRightButtonDown;
            AudienceGrid.Children.Add(AudienceCanvas);

            List<Dictionary<int,string>> anilist = animations[canvas_index];
            
            foreach (Dictionary<int, string> dic in anilist)
            {
                animation_indexes.Add(dic.Keys.First());
            }
            //AudienceCanvas.Arrange(Rec)
        }

        void AudienceCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            canvas_index++;
            AudienceCanvas.PreviewMouseLeftButtonDown -= AudienceCanvas_PreviewMouseLeftButtonDown;
            AudienceCanvas.PreviewMouseRightButtonDown -= AudienceCanvas_PreviewMouseRightButtonDown;
            AudienceGrid.Children.Remove(AudienceCanvas);
            AudienceCanvas = new KEAPCanvas()
            {
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(Colors.White)
            };
            AudienceCanvas.Width = this.Width;
            AudienceCanvas.Height = this.Height;
            
            AudienceCanvas = canvas_arr[canvas_index];
            AudienceCanvas.PreviewMouseLeftButtonDown += AudienceCanvas_PreviewMouseLeftButtonDown;
            AudienceCanvas.PreviewMouseRightButtonDown += AudienceCanvas_PreviewMouseRightButtonDown;
            AudienceGrid.Children.Add(AudienceCanvas);

            List<Dictionary<int, string>> anilist = animations[canvas_index];
            animation_indexes = new List<int>();
            foreach (Dictionary<int, string> dic in anilist)
            {
                animation_indexes.Add(dic.Keys.First());
            }
            animation_index = 0;
        }

        void AudienceCanvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (animation_indexes.Count <= animation_index) return;
            List<Dictionary<int, string>> anilist = animations[canvas_index];
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
          //  sub_Monitor.Text = data;
            //           sub_Monitor.Text = zzz;
        }

        public void getRightGripFromKinect(string data)
        {
       //     grip_right.Text = data;
            //           sub_Monitor.Text = zzz;
        }

        public void getLeftGripFromKinect(string data)
        {
        //    grip_left.Text = data;
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
    }
}
