using Microsoft.Kinect;
using System;
using System.Collections.Generic;


namespace Fizbin.Kinect.Gestures.Segments
{
    /// <summary>
    /// The third part of the swipe up gesture
    /// </summary>
    public class RightUpsegments1: IRelativeGestureSegment
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
            if (skeleton.Joints[JointType.HandLeft].Position.Z < skeleton.Joints[JointType.ShoulderLeft].Position.Z - 0.2 && rightHandGrip == true)
            {
                               
                if (
                    skeleton.Joints[JointType.HandLeft].Position.X > skeleton.Joints[JointType.ShoulderLeft].Position.X &&// 왼손이 왼쪽 어깨보다 오른쪽
                    skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.HipRight].Position.Y//오른손은 가만히
                    )
                {
                 
                    // Left hand Left of Left shoulder


                //
                    if (Math.Atan2(Convert.ToDouble(skeleton.Joints[JointType.HandLeft].Position.X - skeleton.Joints[JointType.HipLeft].Position.X),
                        Convert.ToDouble(skeleton.Joints[JointType.HipLeft].Position.Z - skeleton.Joints[JointType.HandLeft].Position.Z)) > 0.3)
                    {

                        if (Math.Atan2(Convert.ToDouble(skeleton.Joints[JointType.HandLeft].Position.Y - skeleton.Joints[JointType.ShoulderLeft].Position.Y),
                        Convert.ToDouble(skeleton.Joints[JointType.HandLeft].Position.X - skeleton.Joints[JointType.ShoulderLeft].Position.X)) < 0.88 &&
                        Math.Atan2(Convert.ToDouble(skeleton.Joints[JointType.HandLeft].Position.Y - skeleton.Joints[JointType.ShoulderLeft].Position.Y),
                        Convert.ToDouble(skeleton.Joints[JointType.HandLeft].Position.X - skeleton.Joints[JointType.ShoulderLeft].Position.X)) > 0.48
                            
                            )
                        {
                            return_value = GesturePartResult.Succeed;
                        }
                        else return_value = GesturePartResult.Pausing;
                    }
                    else  return_value =  GesturePartResult.Pausing;
                }

                // Debug.WriteLine("GesturePart 2 - right hand below shoulder height but above hip height - FAIL");
                else  return_value =  GesturePartResult.Fail;
            }

            // Debug.WriteLine("GesturePart 2 - Right hand in front of right Shoulder - FAIL");
            else  return_value =  GesturePartResult.Fail;
            returns = new Dictionary<GesturePartResult, float>();
            returns.Add(return_value, result);
            return returns;
        }
    }
}