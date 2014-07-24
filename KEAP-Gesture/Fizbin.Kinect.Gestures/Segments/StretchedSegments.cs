using Microsoft.Kinect;
using System.Collections.Generic;

namespace Fizbin.Kinect.Gestures.Segments
{
    public class StretchedSegments : IRelativeGestureSegment
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

            // elbow is upper then shoulder
            if (skeleton.Joints[JointType.ShoulderLeft].Position.Y < skeleton.Joints[JointType.ElbowLeft].Position.Y && skeleton.Joints[JointType.ShoulderRight].Position.Y < skeleton.Joints[JointType.ElbowRight].Position.Y)
            {
                // Hands between shoulder and hip
                if (skeleton.Joints[JointType.ElbowLeft].Position.Y < skeleton.Joints[JointType.WristLeft].Position.Y &&
                    skeleton.Joints[JointType.ElbowRight].Position.Y < skeleton.Joints[JointType.WristRight].Position.Y)
                {           return_value =  GesturePartResult.Succeed;
                }
                else return_value = GesturePartResult.Pausing;
            }
            else return_value = GesturePartResult.Fail;
            returns = new Dictionary<GesturePartResult, float>();
            returns.Add(return_value, result);
            return returns;    
            }           
            
        }

    
}
