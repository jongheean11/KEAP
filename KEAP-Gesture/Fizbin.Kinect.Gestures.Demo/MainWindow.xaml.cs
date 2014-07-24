//변수 이름
/*ongesturerecognized->제스쳐 이름 저장하는 메소드
 * gestureController ->제스쳐 파악하는 클래스
 * 
 */

using System.Windows;
using System.Windows.Data;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Samples.Kinect.WpfViewers;
using Microsoft.Kinect.Toolkit.Interaction;
using System.ComponentModel;
using System;
using System.Timers;
using Fizbin.Kinect.Gestures.Segments;

//@grip by minsu
using ImaginativeUniversal.Kinect;


using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Controls;

using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Diagnostics;
using System.Collections.Specialized;




//using Microsoft.Samples.Kinect.SwipeGestureRecognizer;


namespace Fizbin.Kinect.Gestures.Demo
{
    
         
         
        // #region private members

  
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Boolean start=false;
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

        public MainWindow()
        {
            
            //@grip by minsu
            this.Closing += new System.ComponentModel.CancelEventHandler(Window_Closing);
            this.Loaded += new RoutedEventHandler(Window_Loaded);

            InitializeComponent();
            /*
            try
            {
                WindowManagerHelper.ProcessName = ConfigurationManager.AppSettings["TargetApplicationProcessName"].ToString();
                SWIPE_RT_GRASP = ConfigurationManager.AppSettings["RightSwipeWithGraspAction"].ToString();
                SWIPE_LFT_GRASP = ConfigurationManager.AppSettings["LeftSwipeWithGraspAction"].ToString();
                DBL_GRASP = ConfigurationManager.AppSettings["DoubleGraspAction"].ToString();
                DBL_PUSH = ConfigurationManager.AppSettings["DoublePushAction"].ToString();
                SWIPE_RT = ConfigurationManager.AppSettings["RightSwipeNoGraspAction"].ToString();
                SWIPE_LFT = ConfigurationManager.AppSettings["LeftSwipeNoGraspAction"].ToString();
                PUSH_RT = ConfigurationManager.AppSettings["RightPush"].ToString();
                PUSH_LFT = ConfigurationManager.AppSettings["LeftPush"].ToString();
            }
            catch (Exception)
            {
                MessageBox.Show("Expected configuration settings are missing.  Please fix this and restart application.");
            }

            */






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
        }//minsu
        //메인 시간마다 초기화



//@grip by minsu

        private bool leftHandGrip = false;
        private bool rightHandGrip = false;

        public static bool rightHandRelease = false;
        public static bool leftHandRelease = false;

        public void interactionStream_InteractionFrameReady(object sender, InteractionFrameReadyEventArgs e)
        {   using (var frame = e.OpenInteractionFrame())
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
                                //if (!rightHandPress && hand.IsPressed)
                                //    rightHandPress = true;

                                //if (hand.HandEventType != InteractionHandEventType.None)
                                //{
                                //    Console.WriteLine("hand.HandEventType :  " + hand.HandEventType + "");
                                //    Console.WriteLine("rightHandGrip :  " + rightHandGrip + "");
                                //}
                                //if (!rightHandGrip && hand.HandEventType == InteractionHandEventType.Grip)
                                //{ 
                                //    rightHandGrip = true;
                                //}
                                //if (!rightHandRelease && hand.HandEventType == InteractionHandEventType.GripRelease)
                                //    rightHandRelease = true;

                                if (!rightHandPress && hand.IsPressed)
                                    rightHandPress = true;


                                    Console.WriteLine("hand.HandEventType :  " + hand.HandEventType + "");
                                    Console.WriteLine("rightHandGrip :  " + rightHandGrip + "");

                                if (hand.HandType == InteractionHandType.Right && hand.HandEventType == InteractionHandEventType.Grip)
                                {
                                    rightHandRelease = false;
                                    rightHandGrip = true;
                                }
                                else if (hand.HandType == InteractionHandType.Right && hand.HandEventType == InteractionHandEventType.GripRelease)
                                {
                                    rightHandGrip = false;
                                    rightHandRelease = true;
                                }

                                
                            }

                            if (hand.HandType == InteractionHandType.Left)
                            {
                                if (hand.HandType == InteractionHandType.Left && hand.HandEventType == InteractionHandEventType.Grip)
                                {
                                    leftHandRelease = false;
                                    leftHandGrip = true;
                                }
                                else if (hand.HandType == InteractionHandType.Left && hand.HandEventType == InteractionHandEventType.GripRelease)
                                {
                                    leftHandGrip = false;
                                    leftHandRelease = true;
                                }
                            }
                        }
                    }
                    AnalyzePresses(rightHandPress, leftHandPress);
                    //AnalyzeGrips(rightHandGrip, rightHandRelease, leftHandGrip, leftHandRelease);                    
                }

                /*
                if (IsLeftGripped && IsRightGripped)
                {
                    if (!IsDoubleGripped)
                    {
                        IsDoubleGripped = true;
                        //DoubleGripped!
                        Gesture = "DoubleGripped!";
                    }
                }
                else
                {*/
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

        //private static void AnalyzeGrips(bool rightHandGrip, bool rightHandRelease, bool leftHandGrip, bool leftHandRelease)
        //{
        //    if (!rightHandGrip)
        //    {

        //        if (rightHandRelease)
        //        {
        //            IsRightGripped = false;
        //        }
        //    }
        //    else
        //    {
        //        IsRightGripped = true;
        //    }

        //    if (!leftHandGrip)
        //    {
        //        if (leftHandRelease)
        //        {
        //            IsLeftGripped = false;
        //        }
        //    }
        //    else
        //    {
        //        IsLeftGripped = true;
        //    }
        //}

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
            gestureController.AddGesture("LeftUp",LeftUpsegments);

            
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



            /*
            IRelativeGestureSegment[] waveRightSegments = new IRelativeGestureSegment[6];
            WaveRightSegment1 waveRightSegment1 = new WaveRightSegment1();
            WaveRightSegment2 waveRightSegment2 = new WaveRightSegment2();
            waveRightSegments[0] = waveRightSegment1;
            waveRightSegments[1] = waveRightSegment2;
            waveRightSegments[2] = waveRightSegment1;
            waveRightSegments[3] = waveRightSegment2;
            waveRightSegments[4] = waveRightSegment1;
            waveRightSegments[5] = waveRightSegment2;
            gestureController.AddGesture("WaveRight", waveRightSegments);
                        
            IRelativeGestureSegment[] waveLeftSegments = new IRelativeGestureSegment[6];
            WaveLeftSegment1 waveLeftSegment1 = new WaveLeftSegment1();
            WaveLeftSegment2 waveLeftSegment2 = new WaveLeftSegment2();
            waveLeftSegments[0] = waveLeftSegment1;
            waveLeftSegments[1] = waveLeftSegment2;
            waveLeftSegments[2] = waveLeftSegment1;
            waveLeftSegments[3] = waveLeftSegment2;
            waveLeftSegments[4] = waveLeftSegment1;
            waveLeftSegments[5] = waveLeftSegment2;
            gestureController.AddGesture("WaveLeft", waveLeftSegments);
            */
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
            
            /*
            IRelativeGestureSegment[] swipeDownSegments = new IRelativeGestureSegment[3];
            swipeDownSegments[0] = new SwipeDownSegment1();
            swipeDownSegments[1] = new SwipeDownSegment2();
            swipeDownSegments[2] = new SwipeDownSegment3();
            gestureController.AddGesture("SwipeDown", swipeDownSegments);*/
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
                   
                        Gesture = "Start";
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
                        zoom = true;
                    }
                }
                else if (zoom == true)
                {
                    if (e.GestureName == "ZoomOut")
                    {
                        Gesture = "ZoomOut\n";
                        zoom = false;
                    }
                }
               
                    switch (e.GestureName)
                    {
                        case "TurnSegments":
                               Gesture = "Pause";
                               start=false;
                               break;                            
                        case "RightDown":
                            Gesture = "RightDown";
                            break;
                        case "Down":
                            Gesture = "Down";
                            break;
                        case "RightUp":
                            Gesture = "RightUp";
                            break;
                        case "LeftDown":
                            Gesture = "LeftDown";
                            break;
                        case "LeftUp":
                            Gesture = "LeftUp";
                            break;
                        case "Left":
                            Gesture = "Left";
                            break;
                        case "Right":
                            Gesture = "Right";
                            break;
                        case "Up":
                            Gesture = "Up";
                            break;
                        case "Push":
                            Gesture = "Push";
                             break;
                        case "strechedHands":
                            Gesture = "strechedHands\n";                            
                            break;
                //        case "JoinedHands":
                 //           Gesture = "JoinedHands\n";
                  //          break;
                        case "SwipeLeft":
                            Gesture = "Swipe Left\n";
                            break;
                        case "SwipeRight":
                            Gesture = "Swipe Right\n";
                            break;
                        case "SwipeUp":
                            Gesture = "Swipe Up\n";
                            break;
            //            case "SwipeDown":
               //             Gesture = "Swipe Down\n";
             //               break;
                        default:
                            break;
                    }
                /*   if (leftHandGrip == true)
            {
                if (rightHandGrip == true)
                {
                    //Gesture = "BothHandgrip";
                    Console.WriteLine("The Both Hand Grip : " + rightHandGrip + "\n\n");
                }
                else
                {
                    //Gesture = "leftHandgrip";
                    Console.WriteLine("The left Hand Grip : " + leftHandGrip + "\n\n");
                }
            }
            else if (rightHandGrip == true)
            {
                //Gesture = "rightHandgrip";
                Console.WriteLine("The right Hand Grip : " + rightHandGrip + "\n\n");
            }//출력검사
*/
                
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
                    gestureController.UpdateAllGestures(skeleton,leftHandGrip,rightHandGrip);
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



    }
}
