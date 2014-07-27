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
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Samples.Kinect.WpfViewers;
using Microsoft.Kinect.Toolkit.Interaction;
using System.ComponentModel;
using System.Timers;
using Fizbin.Kinect.Gestures.Segments;
using Fizbin.Kinect.Gestures;
//@grip by minsu
using ImaginativeUniversal.Kinect;
using System.Windows.Navigation;
using System.Configuration;
using System.Diagnostics;
using System.Collections.Specialized;
using KEAP.Animations;
using System.Windows.Media.Animation;

namespace KEAP
{
    /// <summary>
    /// FullWindowForPresentor.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FullWindowForPresentor : Window, INotifyPropertyChanged
    {
        FullWindowForAudience aud_View = null;
        MainWindow main_View = null;

        private Boolean start = false;
        private Boolean zoom = false;

        //@grip by minsu
        private KinectSensor _sensor;
        //  private Recognizer _swipeRecognizer;
        private InteractionStream _interactionStream;
        private UserInfo[] _userInfos;

        private readonly string SWIPE_RT_GRASP;
        private readonly string SWIPE_LFT_GRASP;
        private readonly string SWIPE_RT;
        private readonly string SWIPE_LFT;
        private readonly string DBL_GRASP;
        private readonly string DBL_PUSH;
        private readonly string PUSH_LFT;
        private readonly string PUSH_RT;
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();

        private Skeleton[] skeletons = new Skeleton[0];

        Timer _clearTimer;

        // skeleton gesture recognizer
        private GestureController gestureController;

        KEAPCanvas AudienceCanvas;
        List<KEAPCanvas> canvas_arr;
        Dictionary<int, List<Dictionary<int, string>>> animations = new Dictionary<int, List<Dictionary<int, string>>>();
        int canvas_index = 0, animation_index = 0;
        List<int> animation_indexes = new List<int>();
        

        private FullWindowForPresentor()
        {
            //@grip by minsu
            this.Closing += new System.ComponentModel.CancelEventHandler(Window_Closing);
            this.Loaded += new RoutedEventHandler(Window_Loaded);

            DataContext = this;
            InitializeComponent();

            // initialize the Kinect sensor manager
            KinectSensorManager = new KinectSensorManager();
            KinectSensorManager.KinectSensorChanged += this.KinectSensorChanged;

            //@grip by minsu
            _userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];

            // locate an available sensor
            sensorChooser.Start();

            // bind chooser's sensor value to the local sensor manager
            var kinectSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.KinectSensorManager, KinectSensorManager.KinectSensorProperty, kinectSensorBinding);

            // add timer for clearing last detected gesture
            _clearTimer = new Timer(2000);
            _clearTimer.Elapsed += new ElapsedEventHandler(clearTimer_Elapsed);
        }

        public FullWindowForPresentor(MainWindow main, FullWindowForAudience audience)
        {
            main_View = main;
            aud_View = audience;
            AddKeyBindings();
            //@grip by minsu
            this.Closing += new System.ComponentModel.CancelEventHandler(Window_Closing);
            this.Loaded += new RoutedEventHandler(Window_Loaded);

            DataContext = this;
            InitializeComponent();

            // initialize the Kinect sensor manager
            KinectSensorManager = new KinectSensorManager();
            KinectSensorManager.KinectSensorChanged += this.KinectSensorChanged;

            //@grip by minsu
            _userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];

            // locate an available sensor
            sensorChooser.Start();

            // bind chooser's sensor value to the local sensor manager
            var kinectSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.KinectSensorManager, KinectSensorManager.KinectSensorProperty, kinectSensorBinding);

            // add timer for clearing last detected gesture
            _clearTimer = new Timer(2000);
            _clearTimer.Elapsed += new ElapsedEventHandler(clearTimer_Elapsed);

            // Audience
            int i = 0;
            canvas_arr = new List<KEAPCanvas>();

            foreach (KEAPCanvas canvas in main.canvas_List)
            {
                bool ani_null = false;
                if (!main.animation_Dictionary.Keys.Contains(i)) ani_null = true;
                else animations = main.animation_Dictionary;
                List<Dictionary<int, string>> anilist = new List<Dictionary<int, string>>();
                if (ani_null)
                    anilist = null;
                else
                {
                    //    if (i < animations.Count)
                    Dictionary<int, string>[] ani_arr = new Dictionary<int, string>[main.animation_Dictionary[i].Count];
                    //main.animation_Dictionary[i].CopyTo(animations);
                    main.animation_Dictionary[i].CopyTo(ani_arr);
                    for (int k = 0; k < main.animation_Dictionary[i].Count; k++)
                    {
                        Dictionary<int, string> tempdic = new Dictionary<int, string>();
                        tempdic.Add(ani_arr[k].Keys.First(), ani_arr[k].Values.First());
                        anilist.Add(tempdic);
                    }
                    //    else
                    //        anilist = null;
                }
                canvas_arr.Add(new KEAPCanvas());
                canvas_arr[i].Width = SystemParameters.WorkArea.Width;
                canvas_arr[i].Height = SystemParameters.PrimaryScreenHeight;
                canvas_arr[i].Background = canvas.Background;

                for (int j = 0; j < main.canvas_List[i].Children.Count; j++)
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
                        Canvas.SetLeft(textblock, Canvas.GetLeft(copyele) * (SystemParameters.WorkArea.Width / canvas.Width));
                        Canvas.SetTop(textblock, Canvas.GetTop(copyele) * (SystemParameters.PrimaryScreenHeight / canvas.Height));

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
                            copy_points[p].X = copy_points[p].X * (SystemParameters.WorkArea.Width / canvas.Width);
                            copy_points[p].Y = copy_points[p].Y * (SystemParameters.PrimaryScreenHeight / canvas.Height);
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
                            X1 = copyele.X1 * (SystemParameters.WorkArea.Width / canvas.Width),
                            Y1 = copyele.Y1 * (SystemParameters.PrimaryScreenHeight / canvas.Height),
                            X2 = copyele.X2 * (SystemParameters.WorkArea.Width / canvas.Width),
                            Y2 = copyele.Y2 * (SystemParameters.PrimaryScreenHeight / canvas.Height),
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
                            Width = copyele.Width * (SystemParameters.WorkArea.Width / canvas.Width),
                            Height = copyele.Height * (SystemParameters.PrimaryScreenHeight / canvas.Height)
                        };
                        Canvas.SetLeft(image, Canvas.GetLeft(copyele) * (SystemParameters.WorkArea.Width / canvas.Width));
                        Canvas.SetTop(image, Canvas.GetTop(copyele) * (SystemParameters.PrimaryScreenHeight / canvas.Height));

                        canvas_arr[i].Children.Add(image);
                    }
                    else if (ele is Rectangle)
                    {
                        Rectangle copyele = ele as Rectangle;
                        Rectangle rectangle = new Rectangle()
                        {
                            Width = copyele.Width * (SystemParameters.WorkArea.Width / canvas.Width),
                            Height = copyele.Height * (SystemParameters.PrimaryScreenHeight / canvas.Height),
                            Fill = copyele.Fill,
                            Stroke = copyele.Stroke,
                            StrokeThickness = copyele.StrokeThickness
                        };
                        Canvas.SetLeft(rectangle, Canvas.GetLeft(copyele) * (SystemParameters.WorkArea.Width / canvas.Width));
                        Canvas.SetTop(rectangle, Canvas.GetTop(copyele) * (SystemParameters.PrimaryScreenHeight / canvas.Height));

                        canvas_arr[i].Children.Add(rectangle);
                    }
                    else if (ele is Ellipse)
                    {
                        Ellipse copyele = ele as Ellipse;
                        Ellipse ellipse = new Ellipse()
                        {
                            Width = copyele.Width * (SystemParameters.WorkArea.Width / canvas.Width),
                            Height = copyele.Height * (SystemParameters.PrimaryScreenHeight / canvas.Height),
                            Fill = copyele.Fill,
                            Stroke = copyele.Stroke,
                            StrokeThickness = copyele.StrokeThickness
                        };
                        Canvas.SetLeft(ellipse, Canvas.GetLeft(copyele) * (SystemParameters.WorkArea.Width / canvas.Width));
                        Canvas.SetTop(ellipse, Canvas.GetTop(copyele) * (SystemParameters.PrimaryScreenHeight / canvas.Height));

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

            kinect_Skeleton_Viewer.Width = SystemParameters.WorkArea.Width / 2;
            kinect_Skeleton_Viewer.Height = SystemParameters.WorkArea.Width * 3 / 8; 
            Thickness margin = new Thickness();
            margin.Left = SystemParameters.WorkArea.Width / 4;
            margin.Right = SystemParameters.WorkArea.Width / 4;
            margin.Top = (SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Width * 3 / 8) / 2;
            margin.Bottom = margin.Top;
            kinect_Skeleton_Viewer.Margin = margin;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            aud_View = null;
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



        private void Enter_KeyEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            aud_View.Enter_KeyEventHandler(sender, e);
        }

        private void Space_KeyEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            aud_View.Space_KeyEventHandler(sender, e);
        }

        private void Down_Arrow_KeyEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            aud_View.Down_Arrow_KeyEventHandler(sender, e);
        }

        private void Up_Arrow_KeyEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            aud_View.Up_Arrow_KeyEventHandler(sender, e);
        }

        private void Left_Arrow_KeyEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            aud_View.Left_Arrow_KeyEventHandler(sender, e);
        }

        private void Right_Arrow_KeyEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            aud_View.Right_Arrow_KeyEventHandler(sender, e);
        }

        private void Close_KeyEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            if (main_View != null)
            {
                main_View.Close_SlideShow();
            }
        }

        private bool leftHandGrip = false;
        private bool rightHandGrip = false;
        public static bool rightHandRelease = false;
        public static bool leftHandRelease = false;

        public void interactionStream_InteractionFrameReady(object sender, InteractionFrameReadyEventArgs e)
        {
            using (var frame = e.OpenInteractionFrame())
            {
                if (frame == null)
                    return;

                frame.CopyInteractionDataTo(_userInfos);
                if (_userInfos != null && _userInfos.Length > 0)
                {
                    bool rightHandPress = false;
                    bool leftHandPress = false;
                    //rightHandGrip = false;

                    foreach (var info in _userInfos)
                    {
                        foreach (var hand in info.HandPointers)
                        {
                            if (hand.HandType == InteractionHandType.Right)
                            {
                                if (!rightHandPress && hand.IsPressed)
                                    rightHandPress = true;

                                if (hand.HandType == InteractionHandType.Right && hand.HandEventType == InteractionHandEventType.Grip)
                                {
                                    rightHandRelease = false;
                                    rightHandGrip = true;
                                    aud_View.get_Grip_From_Kinect();
                                    RightHandGrip.Text = "RightHand Gripped";
                                    aud_View.getRightGripFromKinect("Gripped");

                                }
                                else if (hand.HandType == InteractionHandType.Right && hand.HandEventType == InteractionHandEventType.GripRelease)
                                {
                                    rightHandGrip = false;
                                    rightHandRelease = true;
                                    aud_View.get_Release_From_Kinect();
                                    RightHandGrip.Text = "RightHand Release";
                                    aud_View.getRightGripFromKinect("Release");
                                }
                            }

                            if (hand.HandType == InteractionHandType.Left)
                            {
                                if (hand.HandType == InteractionHandType.Left && hand.HandEventType == InteractionHandEventType.Grip)
                                {
                                    leftHandRelease = false;
                                    leftHandGrip = true;
                                    LeftHandGrip.Text = "LeftHand Gripped";
                                    aud_View.getLeftGripFromKinect("Gripped");
                                }
                                else if (hand.HandType == InteractionHandType.Left && hand.HandEventType == InteractionHandEventType.GripRelease)
                                {
                                    leftHandGrip = false;
                                    leftHandRelease = true;
                                    LeftHandGrip.Text = "LeftHand Release";
                                    aud_View.getLeftGripFromKinect("Release");
                                }
                            }
                        }
                    }
                    AnalyzePresses(rightHandPress, leftHandPress);
                }

                if (IsLeftGripped)
                {
                    IsLeftGripped = true;
                    //Gesture = "LeftGriped!";
                    //_clearTimer.Start();
                }
                if (IsRightGripped)
                {
                    IsRightGripped = true;
                    //Gesture = "RightGriped!";
                    //_clearTimer.Start();
                }

                IsDoubleGripped = false;
                //}

            }
        }


        private void AnalyzePresses(bool rightHandPress, bool leftHandPress)
        {
            if (!rightHandPress)
            {
                IsRightPressed = false;
            }
            else
            {
                IsRightPressed = true;
            }

            if (!leftHandPress)
            {
                IsLeftPressed = false;
            }
            else
            {
                IsLeftPressed = true;
            }
        }


        #region properties

        public static bool IsRightGripped
        {
            get;
            set;
        }

        public static bool IsLeftGripped
        {
            get;
            set;
        }

        public static bool IsDoubleGripped
        {
            get;
            set;
        }

        private bool _isRightPressed;
        public bool IsRightPressed
        {
            get { return _isRightPressed; }
            set
            {
                if (_isRightPressed != value)
                {
                    _isRightPressed = value;
                    if (value == true)
                    {
                        //Console.WriteLine("RightPressed!!\n\n\n");
                    }

                }
            }
        }

        private bool _isLeftPressed;
        public bool IsLeftPressed
        {
            get { return _isLeftPressed; }
            set
            {
                if (_isLeftPressed != value)
                {
                    _isLeftPressed = value;
                    if (value == true)
                    {
                        //Console.WriteLine("LeftPressed!!\n\n\n");
                    }

                }
            }
        }

        public static bool IsDoublePressed
        {
            get;
            set;
        }

        #endregion


        #region Kinect Discovery & Setup

        private void KinectSensorChanged(object sender, KinectSensorManagerEventArgs<KinectSensor> args)
        {

            if (null != args.OldValue)
                UninitializeKinectServices(args.OldValue);

            if (null != args.NewValue)
            {

                InitializeKinectServices(KinectSensorManager, args.NewValue);
            }

        }
        //minsu
        //이전값이 NULL이 아니면 uninitialize
        //새로운값이 NULL이 아니면 initialize

        /// <summary>
        /// Kinect enabled apps should customize which Kinect services it initializes here.
        /// </summary>
        /// <param name="kinectSensorManager"></param>
        /// <param name="sensor"></param>
        private void InitializeKinectServices(KinectSensorManager kinectSensorManager, KinectSensor sensor)
        {
            //@grip by minsu

            if (sensor == null)
                return;

            // Application should enable all streams first.
            // configure the color stream
            kinectSensorManager.ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;
            kinectSensorManager.ColorStreamEnabled = true;

            // configure the depth stream
            kinectSensorManager.DepthStreamEnabled = false;

            kinectSensorManager.TransformSmoothParameters =
                new TransformSmoothParameters
                {
                    Smoothing = 0.5f,
                    Correction = 0.5f,
                    Prediction = 0.5f,
                    JitterRadius = 0.05f,
                    MaxDeviationRadius = 0.04f
                };

            // configure the skeleton stream
            //#
            sensor.SkeletonFrameReady += OnSkeletonFrameReady;
            kinectSensorManager.SkeletonStreamEnabled = true;

            kinectSensorManager.KinectSensorEnabled = true;

            if (!kinectSensorManager.KinectSensorAppConflict)
            {
                // initialize the gesture recognizer
                gestureController = new GestureController();
                gestureController.GestureRecognized += OnGestureRecognized;

                // register the gestures for this demo
                RegisterGestures();
            }

            //@

        }
        //minsu
        //제스쳐리코그 나이져 초기화

        /// <summary>
        /// Kinect enabled apps should uninitialize all Kinect services that were initialized in InitializeKinectServices() here.
        /// </summary>
        /// <param name="sensor"></param>
        private void UninitializeKinectServices(KinectSensor sensor)
        {
            // unregister the event handlers
            sensor.SkeletonFrameReady -= OnSkeletonFrameReady;
            gestureController.GestureRecognized -= OnGestureRecognized;
        }

        #endregion Kinect Discovery & Setup

        /// <summary>
        /// Helper function to register all available 
        /// </summary>
        private void RegisterGestures()
        {

            // define the gestures for the demo


            IRelativeGestureSegment[] strechedSegments = new IRelativeGestureSegment[20];
            StretchedSegments strechedSegment = new StretchedSegments();
            for (int i = 0; i < 20; i++)
            {
                // gesture consists of the same thing 10 times 
                strechedSegments[i] = strechedSegment;
            }
            gestureController.AddGesture("strechedHands", strechedSegments);


            IRelativeGestureSegment[] UpSegments = new IRelativeGestureSegment[2];
            Upsegments1 UpSegment = new Upsegments1();
            for (int i = 0; i < 2; i++)
            {
                // gesture consists of the same thing 10 times 
                UpSegments[i] = UpSegment;
            }
            gestureController.AddGesture("Up", UpSegments);


            IRelativeGestureSegment[] LeftSegments = new IRelativeGestureSegment[2];
            Leftsegments1 LeftSegment = new Leftsegments1();
            for (int i = 0; i < 2; i++)
            {
                // gesture consists of the same thing 10 times 
                LeftSegments[i] = LeftSegment;
            }
            gestureController.AddGesture("Left", LeftSegments);


            IRelativeGestureSegment[] RightSegments = new IRelativeGestureSegment[2];
            RightSegments1 RightSegment = new RightSegments1();
            for (int i = 0; i < 2; i++)
            {
                // gesture consists of the same thing 10 times 
                RightSegments[i] = RightSegment;
            }
            gestureController.AddGesture("Right", RightSegments);


            IRelativeGestureSegment[] LeftUpsegments = new IRelativeGestureSegment[2];
            LeftUpsegments1 LeftUpsegment = new LeftUpsegments1();
            for (int i = 0; i < 2; i++)
            {
                // gesture consists of the same thing 10 times 
                LeftUpsegments[i] = LeftUpsegment;
            }
            gestureController.AddGesture("LeftUp", LeftUpsegments);


            IRelativeGestureSegment[] Downsegments = new IRelativeGestureSegment[2];
            Downsegments1 Downsegment = new Downsegments1();
            for (int i = 0; i < 2; i++)
            {
                // gesture consists of the same thing 10 times 
                Downsegments[i] = Downsegment;
            }
            gestureController.AddGesture("Down", Downsegments);

            IRelativeGestureSegment[] RightDownSegments = new IRelativeGestureSegment[2];
            RightDownSegments1 RightDownSegment = new RightDownSegments1();
            for (int i = 0; i < 2; i++)
            {
                // gesture consists of the same thing 10 times 
                RightDownSegments[i] = RightDownSegment;
            }
            gestureController.AddGesture("RightDown", RightDownSegments);





            IRelativeGestureSegment[] LeftDownsegments = new IRelativeGestureSegment[2];
            LeftDownsegments1 LeftDownsegment = new LeftDownsegments1();
            for (int i = 0; i < 2; i++)
            {
                // gesture consists of the same thing 10 times 
                LeftDownsegments[i] = LeftDownsegment;
            }
            gestureController.AddGesture("LeftDown", LeftDownsegments);


            IRelativeGestureSegment[] RightUpsegments = new IRelativeGestureSegment[2];
            RightUpsegments1 RightUpsegment = new RightUpsegments1();
            for (int i = 0; i < 2; i++)
            {
                // gesture consists of the same thing 10 times 
                RightUpsegments[i] = RightUpsegment;
            }
            gestureController.AddGesture("RightUp", RightUpsegments);



            IRelativeGestureSegment[] joinedhandsSegments = new IRelativeGestureSegment[20];
            JoinedHandsSegment1 joinedhandsSegment = new JoinedHandsSegment1();
            for (int i = 0; i < 20; i++)
            {
                // gesture consists of the same thing 20 times 
                joinedhandsSegments[i] = joinedhandsSegment;
            }
            gestureController.AddGesture("JoinedHands", joinedhandsSegments);



            IRelativeGestureSegment[] menuSegments = new IRelativeGestureSegment[20];
            MenuSegment1 menuSegment = new MenuSegment1();
            for (int i = 0; i < 20; i++)
            {
                // gesture consists of the same thing 20 times 
                menuSegments[i] = menuSegment;
            }
            gestureController.AddGesture("Menu", menuSegments);

            IRelativeGestureSegment[] swipeleftSegments = new IRelativeGestureSegment[3];
            swipeleftSegments[0] = new SwipeLeftSegment1();
            swipeleftSegments[1] = new SwipeLeftSegment2();
            swipeleftSegments[2] = new SwipeLeftSegment3();
            gestureController.AddGesture("SwipeLeft", swipeleftSegments);

            IRelativeGestureSegment[] PushSegments = new IRelativeGestureSegment[3];
            PushSegments[0] = new PushSegment1();
            PushSegments[1] = new PushSegment2();
            PushSegments[2] = new PushSegment3();
            gestureController.AddGesture("Push", PushSegments);

            IRelativeGestureSegment[] swiperightSegments = new IRelativeGestureSegment[3];
            swiperightSegments[0] = new SwipeRightSegment1();
            swiperightSegments[1] = new SwipeRightSegment2();
            swiperightSegments[2] = new SwipeRightSegment3();
            gestureController.AddGesture("SwipeRight", swiperightSegments);

            IRelativeGestureSegment[] TurnSegments = new IRelativeGestureSegment[3];
            TurnSegments1 TurnSegments1 = new TurnSegments1();
            TurnSegments2 TurnSegments2 = new TurnSegments2();
            TurnSegments3 TurnSegments3 = new TurnSegments3();
            TurnSegments[0] = TurnSegments1;
            TurnSegments[1] = TurnSegments2;
            TurnSegments[2] = TurnSegments3;
            gestureController.AddGesture("TurnSegments", TurnSegments);


            IRelativeGestureSegment[] zoomInSegments = new IRelativeGestureSegment[3];
            zoomInSegments[0] = new ZoomSegment1();
            zoomInSegments[1] = new ZoomSegment2();
            zoomInSegments[2] = new ZoomSegment3();
            gestureController.AddGesture("ZoomIn", zoomInSegments);

            IRelativeGestureSegment[] zoomOutSegments = new IRelativeGestureSegment[3];
            zoomOutSegments[0] = new ZoomSegment3();
            zoomOutSegments[1] = new ZoomSegment2();
            zoomOutSegments[2] = new ZoomSegment1();
            gestureController.AddGesture("ZoomOut", zoomOutSegments);

            IRelativeGestureSegment[] swipeUpSegments = new IRelativeGestureSegment[3];
            swipeUpSegments[0] = new SwipeUpSegment1();
            swipeUpSegments[1] = new SwipeUpSegment2();
            swipeUpSegments[2] = new SwipeUpSegment3();
            gestureController.AddGesture("SwipeUp", swipeUpSegments);

        }


        #region Properties

        public static readonly DependencyProperty KinectSensorManagerProperty =
            DependencyProperty.Register(
                "KinectSensorManager",
                typeof(KinectSensorManager),
                typeof(FullWindowForPresentor),
                new PropertyMetadata(null));

        public KinectSensorManager KinectSensorManager
        {
            get { return (KinectSensorManager)GetValue(KinectSensorManagerProperty); }
            set { SetValue(KinectSensorManagerProperty, value); }
        }

        /// <summary>
        /// Gets or sets the last recognized gesture.
        /// </summary>
        private string _gesture;
        public String Gesture
        {
            get { return _gesture; }

            private set
            {
                if (_gesture == value)
                    return;

                _gesture = value;

                if (this.PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Gesture"));
            }
        }

        private string _values;
        public String Values
        {
            get { return _values; }

            private set
            {
                if (_values == value)
                    return;

                _values = value;

                if (this.PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Values"));
            }
        }


        //minsu
        //제스쳐 등록

        #endregion Properties


        #region Events

        /// <summary>
        /// Event implementing INotifyPropertyChanged interface.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events


        #region Event Handlers

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Gesture event arguments.</param>
        private void OnGestureRecognized(object sender, GestureEventArgs e)
        {
            if (start == false)
            {
                if (e.GestureName == "Menu")
                {
                    //SendMsg
                    Gesture = "Start";
                    aud_View.getDataFromKinect("Start");
                    aud_View.get_Start_From_Kinect();
                    start = true;
                }
            }
            else
            {
                if (zoom == false)
                {
                    if (e.GestureName == "ZoomIn")
                    {
                        Gesture = "ZoomIn\n";
                        aud_View.getDataFromKinect("ZoomIn");
                        aud_View.get_ZoomIn_From_Kinect();
                        aud_View.zooming();
                        zoom = true;
                    }
                }
                else if (zoom == true)
                {
                    if (e.GestureName == "ZoomOut")
                    {
                        Gesture = "ZoomOut\n";
                        aud_View.getDataFromKinect("ZoomOut");
                        aud_View.get_ZoomOut_From_Kinect();
                        aud_View.zooming();
                        zoom = false;
                    }
                }

                switch (e.GestureName)
                {
                    case "TurnSegments":
                        Gesture = "Pause";
                        //aud_View.getDataFromKinect("Pause");
                        aud_View.get_Pause_From_Kinect();
                        start = false;
                        break;
                    case "RightDown":
                        Gesture = "RightDown";
                        //aud_View.getDataFromKinect("RightDown");
                        aud_View.DownArrow();
                        aud_View.RightArrow();
                        aud_View.get_RightDown_From_Kinect();
                        break;
                    case "Down":
                        Gesture = "Down";
                        //aud_View.getDataFromKinect("Down");
                        aud_View.DownArrow();
                        aud_View.get_Down_From_Kinect();
                        break;
                    case "RightUp":
                        Gesture = "RightUp";
                        //aud_View.getDataFromKinect("RightUp");
                        aud_View.RightArrow();
                        aud_View.UpArrow();
                        aud_View.get_RightUp_From_Kinect();
                        break;
                    case "LeftDown":
                        Gesture = "LeftDown";
                        //aud_View.getDataFromKinect("LeftDown");
                        aud_View.LeftArrow();
                        aud_View.DownArrow();
                        aud_View.get_LeftDown_From_Kinect();
                        break;
                    case "LeftUp":
                        Gesture = "LeftUp";
                        //aud_View.getDataFromKinect("LeftUp");
                        aud_View.LeftArrow();
                        aud_View.UpArrow();
                        aud_View.get_LeftUp_From_Kinect();
                        break;
                    case "Left":
                        Gesture = "Left";
                        //aud_View.getDataFromKinect("Left");
                        aud_View.LeftArrow();
                        aud_View.get_Left_From_Kinect();
                        break;
                    case "Right":
                        Gesture = "Right";
                        //aud_View.getDataFromKinect("Right");
                        aud_View.RightArrow();
                        aud_View.get_Right_From_Kinect();
                        break;
                    case "Up":
                        Gesture = "Up";
                        //aud_View.getDataFromKinect("Up");
                        aud_View.UpArrow();
                        aud_View.get_Up_From_Kinect();
                        break;
                    case "Push":
                        Gesture = "Push";
                        //aud_View.getDataFromKinect("Push");
                        aud_View.UpArrow();
                        aud_View.get_Push_From_Kinect();
                        break;
                    case "strechedHands":
                        Gesture = "strechedHands\n";
                        //aud_View.getDataFromKinect("strechedHands");
                        aud_View.get_StrechedHands_From_Kinect();
                        break;
                    case "SwipeLeft":
                        Gesture = "Swipe Left\n";
                        //aud_View.getDataFromKinect("SwipeLeft");
                        aud_View.LeftArrow();
                        aud_View.get_SwipeLeft_From_Kinect();
                        break;
                    case "SwipeRight":
                        Gesture = "Swipe Right\n";
                        //aud_View.getDataFromKinect("SwipeRight");
                        aud_View.RightArrow();
                        aud_View.get_SwipeRight_From_Kinect();
                        break;
                    case "SwipeUp":
                        Gesture = "Swipe Up\n";
                        //aud_View.getDataFromKinect("SwipeUp");
                        aud_View.get_SwipeUp_From_Kinect();
                        break;
                    default:
                        break;
                }
            }

            _clearTimer.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;

                // resize the skeletons array if needed
                if (skeletons.Length != frame.SkeletonArrayLength)
                    skeletons = new Skeleton[frame.SkeletonArrayLength];

                // get the skeleton data
                frame.CopySkeletonDataTo(skeletons);

                foreach (var skeleton in skeletons)
                {
                    // skip the skeleton if it is not being tracked
                    if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                        continue;
                    // update the gesture controller
                    gestureController.UpdateAllGestures(skeleton, leftHandGrip, rightHandGrip);
                }
            }
        }

        /// <summary>
        /// Clear text after some time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clearTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Gesture = "";
            Values = "";
            _clearTimer.Stop();
        }

        #endregion Event Handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //CreateSwipeGestureRecognizer(ref _swipeRecognizer);
            _userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];
            KinectSensor.KinectSensors.StatusChanged += new EventHandler<StatusChangedEventArgs>(KinectSensors_StatusChanged);

            _sensor = KinectSensor.KinectSensors.FirstOrDefault();  /////

            InitSensor(_sensor);  /////

            // maximized
            this.WindowState = System.Windows.WindowState.Maximized;

            // audience
            AudienceCanvas = new KEAPCanvas()
            {
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(Colors.White)
            };
            AudienceCanvas.Width = SystemParameters.WorkArea.Width;
            AudienceCanvas.Height = SystemParameters.MaximizedPrimaryScreenHeight;
            AudienceCanvas = canvas_arr[0];
            //AudienceCanvas.PreviewMouseLeftButtonDown += AudienceCanvas_PreviewMouseLeftButtonDown;
            //AudienceCanvas.PreviewMouseRightButtonDown += AudienceCanvas_PreviewMouseRightButtonDown;
            AudienceGrid.Children.Insert(0,AudienceCanvas);

            List<Dictionary<int, string>> anilist;
            if (animations.Keys.Contains(canvas_index))
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


        private void InitSensor(KinectSensor sensor)
        {
            if (sensor == null)
                return;

            _interactionStream = new InteractionStream(sensor, new InteractionClient());  //////////문제
            //_interactionStream.InteractionFrameReady += new EventHandler<InteractionFrameReadyEventArgs>(interactionStream_InteractionFrameReady);  /////
            _interactionStream.InteractionFrameReady += new EventHandler<InteractionFrameReadyEventArgs>(interactionStream_InteractionFrameReady);

            sensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(sensor_DepthFrameReady);
            sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(sensor_SkeletonFrameReady);

            sensor.ColorStream.Enable();
            sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            sensor.SkeletonStream.Enable();
            sensor.Start();

        }
        void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;

                Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);
                var accelerometerReading = _sensor.AccelerometerGetCurrentReading();
                _interactionStream.ProcessSkeleton(skeletons, accelerometerReading, frame.Timestamp);
                //  _swipeRecognizer.Recognize(sender, frame, skeletons);

            }
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            DeInitSensor(_sensor);
        }
        void sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame == null)
                    return;

                _interactionStream.ProcessDepth(frame.GetRawPixelData(), frame.Timestamp);
            }
        }

        private void DeInitSensor(KinectSensor sensor)
        {
            if (sensor == null)
                return;

            sensor.SkeletonFrameReady -= sensor_SkeletonFrameReady;
            sensor.DepthFrameReady -= sensor_DepthFrameReady;

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (a, b) =>
            {
                sensor.Stop();

                sensor.SkeletonStream.Disable();
                sensor.DepthStream.Disable();
                sensor.ColorStream.Disable();

                sensor = null;
            };
            bw.RunWorkerAsync();

            if (_interactionStream != null)
            {
                _interactionStream.InteractionFrameReady -= interactionStream_InteractionFrameReady;
            }
        }


        void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            if (e.Status == KinectStatus.Disconnected)
            {
                if (_sensor != null)
                {
                    DeInitSensor(_sensor);
                }
            }

            if (e.Status == KinectStatus.Connected)
            {
                _sensor = e.Sensor;
                InitSensor(_sensor);
            }
        }

        // not using. this maximized code will be in the "Window_Loaded" method
        private void Presentor_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Maximized;
        }


        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            //aud_View.getDataFromKinect(message.Text);
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
            Canvas.SetLeft(rec_BG, -(SystemParameters.WorkArea.Width * x / 8));
            Canvas.SetTop(rec_BG, -(WindowSettings.resolution_Height * y / 8));
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
                    Canvas.SetLeft(textblock, Canvas.GetLeft(copyele));
                    Canvas.SetTop(textblock, Canvas.GetTop(copyele));

                    double x1 = Canvas.GetLeft(ele),
                        y1 = Canvas.GetTop(ele);

                    Canvas.SetLeft(textblock, x1 - (SystemParameters.PrimaryScreenWidth * x / 8));
                    Canvas.SetTop(textblock, y1 - (SystemParameters.PrimaryScreenHeight * y / 8));

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
                        copy_points[i].X = copy_points[i].X - (SystemParameters.WorkArea.Width * x / 8);
                        copy_points[i].Y = copy_points[i].Y - (SystemParameters.PrimaryScreenHeight * y / 8);
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

                    line.X1 = ((Line)ele).X1 - (SystemParameters.WorkArea.Width * x / 8);
                    line.Y1 = ((Line)ele).Y1 - (SystemParameters.PrimaryScreenHeight * y / 8);
                    line.X2 = ((Line)ele).X2 - (SystemParameters.WorkArea.Width * y / 8);
                    line.Y2 = ((Line)ele).Y2 - (SystemParameters.PrimaryScreenHeight * y / 8);

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

                    Canvas.SetLeft(image, x1 - (SystemParameters.WorkArea.Width * x / 8));
                    Canvas.SetTop(image, y1 - (SystemParameters.PrimaryScreenHeight * y / 8));

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

                    Canvas.SetLeft(rectangle, x1 - (SystemParameters.WorkArea.Width * x / 8));
                    Canvas.SetTop(rectangle, y1 - (SystemParameters.PrimaryScreenHeight * y / 8));

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

                    Canvas.SetLeft(ellipse, x1 - (SystemParameters.WorkArea.Width * x / 8));
                    Canvas.SetTop(ellipse, y1 - (SystemParameters.PrimaryScreenHeight * y / 8));

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
            if (TargetVisual.RenderTransform != null)
            {
                Rect bounds = new Rect(0, 0, TargetVisual.ActualWidth, TargetVisual.ActualHeight);
                bounds = TargetVisual.RenderTransform.TransformBounds(bounds);
                TargetVisual.Arrange(new Rect(-bounds.Left, -bounds.Top, width, height));
                width = (int)bounds.Width;
                height = (int)bounds.Height;
            }

            RenderTargetBitmap zoom_rtb = new RenderTargetBitmap(width / 4 + 402, height / 4 + 237, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            
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
