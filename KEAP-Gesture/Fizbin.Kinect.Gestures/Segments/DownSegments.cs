using Microsoft.Kinect;
using System.Collections.Generic;
using System;

namespace Fizbin.Kinect.Gestures.Segments
{
    /// <summary>
    /// The third part of the swipe up gesture
    /// </summary>
    public class Downsegments1 : IRelativeGestureSegment
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
            // //Left hand in front of Left shoulder
            if (skeleton.Joints[JointType.HandLeft].Position.Z < skeleton.Joints[JointType.ShoulderLeft].Position.Z - 0.2&&rightHandGrip==true&&leftHandGrip==false)
            {
                // Left hand above head


                if (     //skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.HipRight].Position.Y&&//오른손은 가만히                      
                    Math.Atan2(Convert.ToDouble(skeleton.Joints[JointType.ShoulderLeft].Position.X - skeleton.Joints[JointType.HandLeft].Position.X),
                        Convert.ToDouble(skeleton.Joints[JointType.ShoulderLeft].Position.Y - skeleton.Joints[JointType.HandLeft].Position.Y)) < 0.3 &&
                    Math.Atan2(Convert.ToDouble(skeleton.Joints[JointType.HandLeft].Position.X - skeleton.Joints[JointType.ShoulderLeft].Position.X),
                        Convert.ToDouble(skeleton.Joints[JointType.ShoulderLeft].Position.Y - skeleton.Joints[JointType.HandLeft].Position.Y)) <0.3
                    )
                {

                    // Left hand Left of Left shoulder
                    if (Math.Atan2(Convert.ToDouble(skeleton.Joints[JointType.HandLeft].Position.Y - skeleton.Joints[JointType.ShoulderLeft].Position.Y),
                        Convert.ToDouble(skeleton.Joints[JointType.ShoulderLeft].Position.Z - skeleton.Joints[JointType.HandLeft].Position.Z)) > -0.98&&
                        Math.Atan2(Convert.ToDouble(skeleton.Joints[JointType.HandLeft].Position.Y - skeleton.Joints[JointType.ShoulderLeft].Position.Y),
                        Convert.ToDouble(skeleton.Joints[JointType.ShoulderLeft].Position.Z - skeleton.Joints[JointType.HandLeft].Position.Z)) < -0.58
                        )
                    {
                        return_value = GesturePartResult.Succeed;
                    }
                    else return_value = GesturePartResult.Pausing;
                }

                // Debug.WriteLine("GesturePart 2 - right hand below shoulder height but above hip height - FAIL");
                else return_value = GesturePartResult.Fail;
            }

            // Debug.WriteLine("GesturePart 2 - Right hand in front of right Shoulder - FAIL");
            else return_value = GesturePartResult.Fail;
            returns = new Dictionary<GesturePartResult, float>();
            returns.Add(return_value, result);
            return returns;
        }
    }
}