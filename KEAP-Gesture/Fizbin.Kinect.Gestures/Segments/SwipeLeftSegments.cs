using Microsoft.Kinect;
using System.Collections.Generic;
using System;

namespace Fizbin.Kinect.Gestures.Segments
{
    /// <summary>
    /// The first part of the swipe left gesture
    /// </summary>
    public class SwipeLeftSegment1 : IRelativeGestureSegment
    {
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>
        /// based on if the gesture part has been completed</returns>
        public Dictionary<GesturePartResult, float> CheckGesture(Skeleton skeleton,bool leftHandGrip,bool rightHandGrip)
        {
            float result = skeleton.Joints[JointType.HandRight].Position.Z;
            GesturePartResult return_value;
            Dictionary<GesturePartResult, float> returns;

            // right hand in front of right shoulder
            if (Math.Atan2(Convert.ToDouble(skeleton.Joints[JointType.HandRight].Position.X - skeleton.Joints[JointType.HipRight].Position.X),
                        Convert.ToDouble(skeleton.Joints[JointType.HipRight].Position.Z - skeleton.Joints[JointType.HandRight].Position.Z)) > 0.785398 &&
                skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.HipCenter].Position.Y && leftHandGrip == true)
            {
                // right hand below shoulder height but above hip height
                if (skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.ShoulderRight].Position.Y && 
                    skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y)
                {
                    // right hand right of right shoulder
                    if (skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.ShoulderRight].Position.X)
                    {
                        return_value =  GesturePartResult.Succeed;
                    }
                    else return_value = GesturePartResult.Pausing;
                }
                else return_value = GesturePartResult.Fail;
            }
            else return_value = GesturePartResult.Fail;
            returns = new Dictionary<GesturePartResult, float>();
            returns.Add(return_value, result);
            return returns;
        }
    }

    /// <summary>
    /// The second part of the swipe left gesture
    /// </summary>
    public class SwipeLeftSegment2 : IRelativeGestureSegment
    {
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>GesturePartResult based on if the gesture part has been completed</returns>
        public Dictionary<GesturePartResult, float> CheckGesture(Skeleton skeleton,bool leftHandGrip,bool rightHandGrip)
        {
            float result = skeleton.Joints[JointType.HandRight].Position.Z;
            GesturePartResult return_value;
            Dictionary<GesturePartResult, float> returns;

            // right hand in front of right shoulder
            if (skeleton.Joints[JointType.HandRight].Position.Z < skeleton.Joints[JointType.ElbowRight].Position.Z && 
                skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.HipCenter].Position.Y)
            {
                // right hand below shoulder height but above hip height
                if (skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.ShoulderRight].Position.Y && 
                    skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y)
                {
                    // right hand left of right shoulder & right of left shoulder
                    if (Math.Atan2(Convert.ToDouble(skeleton.Joints[JointType.HipRight].Position.X - skeleton.Joints[JointType.HandRight].Position.X),
            Convert.ToDouble(skeleton.Joints[JointType.HipRight].Position.Z - skeleton.Joints[JointType.HandRight].Position.Z)) < 0.25 &&
                        Math.Atan2(Convert.ToDouble(skeleton.Joints[JointType.HipRight].Position.X - skeleton.Joints[JointType.HandRight].Position.X),
            Convert.ToDouble(skeleton.Joints[JointType.HipRight].Position.Z - skeleton.Joints[JointType.HandRight].Position.Z)) > -0.25)
                    {
                       return_value =  GesturePartResult.Succeed;
                    }
                    else return_value = GesturePartResult.Pausing;
                }
                else return_value = GesturePartResult.Fail;
            }
            else return_value = GesturePartResult.Fail;
            returns = new Dictionary<GesturePartResult, float>();
            returns.Add(return_value, result);
            return returns;

        }
    }

    /// <summary>
    /// The third part of the swipe left gesture
    /// </summary>
    public class SwipeLeftSegment3 : IRelativeGestureSegment
    {
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>GesturePartResult based on if the gesture part has been completed</returns>
        public Dictionary<GesturePartResult, float> CheckGesture(Skeleton skeleton,bool leftHandGrip,bool rightHandGrip)
        {
            float result = skeleton.Joints[JointType.HandRight].Position.Z;
            GesturePartResult return_value;
            Dictionary<GesturePartResult, float> returns;

            // //Right hand in front of right Shoulder
            if (skeleton.Joints[JointType.HandRight].Position.Z < skeleton.Joints[JointType.ElbowRight].Position.Z &&
                skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.HipCenter].Position.Y)
            {
                // //right hand below shoulder height but above hip height
                if (skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.ShoulderCenter].Position.Y &&
                    skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y)
                {
                    // //right hand left of center hip
                    if (skeleton.Joints[JointType.HandRight].Position.X < skeleton.Joints[JointType.ShoulderLeft].Position.X)
                    {
                        return_value =  GesturePartResult.Succeed;
                    }

                    else return_value =GesturePartResult.Pausing;
                }

                else return_value =GesturePartResult.Fail;
            }

            else return_value = GesturePartResult.Fail;
            returns = new Dictionary<GesturePartResult, float>();
            returns.Add(return_value, result);
            return returns;

        }
    }
}
