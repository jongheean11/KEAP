//이름
/*
 * gestures->찾아야 하는 제스쳐들 모두 저장
 */


using Microsoft.Kinect;
using System;
using System.Collections.Generic;

namespace Fizbin.Kinect.Gestures
{
    public class GestureController
    {
        /// <summary>
        /// The list of all gestures we are currently looking for
        /// </summary>
        private List<Gesture> gestures = new List<Gesture>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GestureController"/> class.
        /// </summary>
        public GestureController()
        {
        }

        /// <summary>
        /// Occurs when [gesture recognised].
        /// </summary>
        public event EventHandler<GestureEventArgs> GestureRecognized;

        /// <summary>
        /// Updates all gestures.
        /// </summary>
        /// <param name="data">The skeleton data.</param>
        public void UpdateAllGestures(Skeleton data,bool leftHandGrip,bool rightHandGrip)
        {
            
         
            foreach (Gesture gesture in this.gestures)
            {
                   gesture.UpdateGesture(data,leftHandGrip,rightHandGrip);
            }
        }
        /*
        public void PrintPosition(Skeleton data)
        {
          
            if (this.tempFrameCount <= 100)
            {
                tempFrameCount = 0;
            }
            else tempFrameCount++;
        }
        */
        private int tempFrameCount = 0;
        /// <summary>
        /// Adds the gesture.
        /// </summary>
        /// <param name="name">The gesture type.</param>
        /// <param name="gestureDefinition">The gesture definition.</param>
        public void AddGesture(string name, IRelativeGestureSegment[] gestureDefinition)
        {
            Gesture gesture = new Gesture(name, gestureDefinition);
            gesture.GestureRecognized += OnGestureRecognized;
            this.gestures.Add(gesture);
        }

        /// <summary>
        /// Handles the GestureRecognized event of the g control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KinectSkeltonTracker.GestureEventArgs"/> instance containing the event data.</param>
        private void OnGestureRecognized(object sender, GestureEventArgs e)
        {
            if (this.GestureRecognized != null)
            {
                this.GestureRecognized(this, e);
            }

            foreach (Gesture g in this.gestures)
            {
                g.Reset();
            }
        }
    }
}
