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




        private FullWindowForPresentor()
        {
            //@grip by minsu
            this.Closing += new System.ComponentModel.CancelEventHandler(Window_Closing);
            this.Loaded += new RoutedEventHandler(Window_Loaded);


            InitializeComponent();



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


            InitializeComponent();



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
                                    aud_View.getRightGripFromKinect("Gripped");

                                }
                                else if (hand.HandType == InteractionHandType.Right && hand.HandEventType == InteractionHandEventType.GripRelease)
                                {
                                    rightHandGrip = false;
                                    rightHandRelease = true;
                                    aud_View.getRightGripFromKinect("Release");
                                }


                            }

                            if (hand.HandType == InteractionHandType.Left)
                            {
                                if (hand.HandType == InteractionHandType.Left && hand.HandEventType == InteractionHandEventType.Grip)
                                {
                                    leftHandRelease = false;
                                    leftHandGrip = true;
                                    aud_View.getLeftGripFromKinect("Gripped");
                                }
                                else if (hand.HandType == InteractionHandType.Left && hand.HandEventType == InteractionHandEventType.GripRelease)
                                {
                                    leftHandGrip = false;
                                    leftHandRelease = true;
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
                typeof(MainWindow),
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
                        zoom = true;
                    }
                }
                else if (zoom == true)
                {
                    if (e.GestureName == "ZoomOut")
                    {
                        Gesture = "ZoomOut\n";
                        aud_View.getDataFromKinect("ZoomOut");
                        zoom = false;
                    }
                }

                switch (e.GestureName)
                {
                    case "TurnSegments":
                        Gesture = "Pause";
                        aud_View.getDataFromKinect("Pause");
                        start = false;
                        break;
                    case "RightDown":
                        Gesture = "RightDown";
                        aud_View.getDataFromKinect("RightDown");
                        break;
                    case "Down":
                        Gesture = "Down";
                        aud_View.getDataFromKinect("Down");
                        break;
                    case "RightUp":
                        Gesture = "RightUp";
                        aud_View.getDataFromKinect("RightUp");
                        break;
                    case "LeftDown":
                        Gesture = "LeftDown";
                        aud_View.getDataFromKinect("LeftDown");
                        break;
                    case "LeftUp":
                        Gesture = "LeftUp";
                        aud_View.getDataFromKinect("LeftUp");
                        break;
                    case "Left":
                        Gesture = "Left";
                        aud_View.getDataFromKinect("Left");
                        break;
                    case "Right":
                        Gesture = "Right";
                        aud_View.getDataFromKinect("Right");
                        break;
                    case "Up":
                        Gesture = "Up";
                        aud_View.getDataFromKinect("Up");
                        break;
                    case "Push":
                        Gesture = "Push";
                        aud_View.getDataFromKinect("Push");
                        break;
                    case "strechedHands":
                        Gesture = "strechedHands\n";
                        aud_View.getDataFromKinect("strechedHands");
                        break;
                    case "SwipeLeft":
                        Gesture = "Swipe Left\n";
                        aud_View.getDataFromKinect("SwipeLeft");
                        break;
                    case "SwipeRight":
                        Gesture = "Swipe Right\n";
                        aud_View.getDataFromKinect("SwipeRight");
                        break;
                    case "SwipeUp":
                        Gesture = "Swipe Up\n";
                        aud_View.getDataFromKinect("SwipeUp");
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
            aud_View.getDataFromKinect(message.Text);
        }
    }
}
