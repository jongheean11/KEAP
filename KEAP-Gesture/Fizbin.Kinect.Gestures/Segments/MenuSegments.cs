using Microsoft.Kinect;
using System.Collections.Generic;
using System;
namespace Fizbin.Kinect.Gestures.Segments
{
    /// <summary>
    /// The menu gesture segment
    /// </summary>
    public class MenuSegment1 : IRelativeGestureSegment
    {
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>
        /// 
        /// 
        /// based on if the gesture part has been completed</returns>
        public Dictionary<GesturePartResult, float> CheckGesture(Skeleton skeleton,bool leftHandGrip,bool rightHandGrip)
        {
            



            float result = skeleton.Joints[JointType.HandRight].Position.Z;
            GesturePartResult return_value;
            Dictionary<GesturePartResult, float> returns;

            // Left and right hands below hip
            if (skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y && 
                skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y&&
                skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.ShoulderLeft].Position.Y &&
                skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.ShoulderRight].Position.Y)
            {
                // 어꺠 와 팔꿈치 사이의 각도 >35
                if (Math.Atan2(Convert.ToDouble(skeleton.Joints[JointType.ShoulderLeft].Position.X - skeleton.Joints[JointType.ElbowLeft].Position.X),
                        Convert.ToDouble(skeleton.Joints[JointType.ShoulderLeft].Position.Y - skeleton.Joints[JointType.ElbowLeft].Position.Y)) > 0.58 &&
                    Math.Atan2(Convert.ToDouble(skeleton.Joints[JointType.HandRight].Position.X - skeleton.Joints[JointType.ShoulderRight].Position.X),
                        Convert.ToDouble(skeleton.Joints[JointType.ShoulderRight].Position.Y - skeleton.Joints[JointType.HandRight].Position.Y)) > 0.58)
                {
                    //어깨와 손사이의 각도>45
                    if (Math.Atan2(Convert.ToDouble(skeleton.Joints[JointType.ShoulderLeft].Position.X - skeleton.Joints[JointType.HandLeft].Position.X),
                        Convert.ToDouble(skeleton.Joints[JointType.ShoulderLeft].Position.Y - skeleton.Joints[JointType.HandLeft].Position.Y)) > 0.785398 &&
                        Math.Atan2(Convert.ToDouble(skeleton.Joints[JointType.HandRight].Position.X - skeleton.Joints[JointType.ShoulderRight].Position.X),
                        Convert.ToDouble(skeleton.Joints[JointType.ShoulderRight].Position.Y - skeleton.Joints[JointType.HandRight].Position.Y)) > 0.785398)
                    {
                         return_value = GesturePartResult.Succeed;
                    }
                    else return_value = GesturePartResult.Pausing;
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
