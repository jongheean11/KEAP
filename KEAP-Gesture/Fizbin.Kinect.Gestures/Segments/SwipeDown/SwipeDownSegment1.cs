using Microsoft.Kinect;
using System.Diagnostics;
using System.Collections.Generic;

namespace Fizbin.Kinect.Gestures.Segments
{
    /// <summary>
    /// The first part of the swipe down gesture with the right hand
    /// </summary>
    public class SwipeDownSegment1 : IRelativeGestureSegment
    {
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>
        /// 
        /// 
        /// 
        /// based on if the gesture part has been completed</returns>
        public Dictionary<GesturePartResult, float> CheckGesture(Skeleton skeleton,bool leftHandGrip,bool rightHandGrip)
        {
            float result = skeleton.Joints[JointType.HandRight].Position.Z;
            GesturePartResult return_value;
            Dictionary<GesturePartResult, float> returns;


            // right hand in front of right shoulder
            if (skeleton.Joints[JointType.HandRight].Position.Z < skeleton.Joints[JointType.ElbowRight].Position.Z &&
                skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.ShoulderCenter].Position.Y && leftHandGrip == true)
            {
                // right hand below head height and hand higher than elbow
                if (skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.Head].Position.Y && skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y)
                {
                    // right hand right of right shoulder
                    if (skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.ShoulderRight].Position.X)
                    {
                        return_value =  GesturePartResult.Succeed;
                    }
                    else  return_value =  GesturePartResult.Pausing;
                }
                else  return_value =  GesturePartResult.Fail;
            }
            else  return_value =  GesturePartResult.Fail;
            returns = new Dictionary<GesturePartResult, float>();
            returns.Add(return_value, result);
            return returns;

        }
    }
}