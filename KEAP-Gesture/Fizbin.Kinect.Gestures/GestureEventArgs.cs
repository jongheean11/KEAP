﻿using System;

namespace Fizbin.Kinect.Gestures
{
    /// <summary>
    /// The gesture event arguments
    /// </summary>
    public class GestureEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GestureEventArgs"/> class.
        /// </summary>
        /// <param name="type">The gesture type.</param>
        /// <param name="trackingID">The tracking ID.</param>
        public GestureEventArgs(string name, int trackingId, float z)
        {
            this.TrackingId = trackingId;
            this.GestureName = name;
     //       this.x = x;
     //       this.y = y;
            this.status_z = z;
            


        }

        /// <summary>
        /// Gets or sets the type of the gesture.
        /// </summary>
        /// <value>
        /// The name of the gesture.
        /// </value>
        public string GestureName { get; set; }

        /// <summary>
        /// Gets or sets the tracking ID.
        /// </summary>
        /// <value>
        /// The tracking ID.
        /// </value>
        public int TrackingId { get; set; }


    //    public int x{get ;set;}
     //   public int y { get; set; }
        public float status_z { get; set; }
        
    }
}
