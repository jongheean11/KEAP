using Microsoft.Kinect;
using System;
using System.Collections.Generic;

namespace Fizbin.Kinect.Gestures
{
    class Gesture
    {
        /// <summary>
        /// The parts that make up this gesture
        /// </summary>
        private IRelativeGestureSegment[] gestureParts;

        /// <summary>
        /// The current gesture part that we are matching against
        /// </summary>
        private int currentGesturePart = 0;

        /// <summary>
        /// the number of frames to pause for when a pause is initiated
        /// </summary>
        private int pausedFrameCount = 10;

        /// <summary>
        /// The current frame that we are on
        /// </summary>
        private int frameCount = 0;

        /// <summary>
        /// Are we paused?
        /// </summary>
        private bool paused = false;

        /// <summary>
        /// The name of gesture that this is
        /// </summary>
        private string name;
        /// <summary>
        /// Initializes a new instance of the <see cref="Gesture"/> class.
        /// </summary>
        /// <param name="type">The type of gesture.</param>
        /// <param name="gestureParts">The gesture parts.</param>
        public Gesture(string name, IRelativeGestureSegment[] gestureParts)
        {
            this.gestureParts = gestureParts;
            this.name = name;
        }

        /// <summary>
        /// Occurs when [gesture regnised].
        /// </summary>
        public event EventHandler<GestureEventArgs> GestureRecognized;


        public Dictionary<GesturePartResult, float> result;

        /// <summary>
        /// Updates the gesture.
        /// </summary>
        /// <param name="data">The skeleton data.</param>
        /// 

        public void UpdateGesture(Skeleton data,bool leftHandGrip,bool rightHandGrip)
        {
            if (this.paused)
            {
                if (this.frameCount == this.pausedFrameCount)
                {
                    this.paused = false;
                }

                this.frameCount++;
            }
            
                        //GesturePartResult result = this.gestureParts[this.currentGesturePart].CheckGesture(data);

            //for (int n = 0; n < this.gestureParts.Length; n++) { 
                result = this.gestureParts[this.currentGesturePart].CheckGesture(data,leftHandGrip,rightHandGrip);
            //}
            //if (result == GesturePartResult.Succeed)
            if(result.ContainsKey(GesturePartResult.Succeed))
            {
                if (this.currentGesturePart + 1 < this.gestureParts.Length)
                {
                    this.currentGesturePart++;
                    this.frameCount = 0;
                    this.pausedFrameCount = 10;
                    this.paused = true;                    
                }
                else
                {
                    if (this.GestureRecognized != null)
                    {
                        this.GestureRecognized(this, new GestureEventArgs(this.name, data.TrackingId, result[GesturePartResult.Succeed]));
                        this.Reset();
                    }
                }
            }
            //else if (result == GesturePartResult.Fail || this.frameCount == 50)
            else if (result.ContainsKey(GesturePartResult.Fail) || this.frameCount == 50)
            {
                this.currentGesturePart = 0;
                this.frameCount = 0;
                this.pausedFrameCount = 5;
                this.paused = true;
                
            }
            else
            {
                this.frameCount++;
                this.pausedFrameCount = 5;
                this.paused = true;
            }
        }

        

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            this.currentGesturePart = 0;
            this.frameCount = 0;
            this.pausedFrameCount = 5;
            this.paused = true;
        }
    }
}
