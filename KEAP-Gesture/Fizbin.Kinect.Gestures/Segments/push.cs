using Microsoft.Kinect;
using System.Collections.Generic;
using System;



namespace Fizbin.Kinect.Gestures.Segments
{
    /// <summary>
    /// The first part of the swipe right gesture
    /// </summary>
    public class PushSegment1 : IRelativeGestureSegment
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
            //어깨사이
            if (skeleton.Joints[JointType.ShoulderLeft].Position.X < skeleton.Joints[JointType.HandRight].Position.X &&
                        skeleton.Joints[JointType.ShoulderRight].Position.X > skeleton.Joints[JointType.HandRight].Position.X && leftHandGrip == true)
            {
                //어꺠와 힙 사이
                if (skeleton.Joints[JointType.HipRight].Position.Y < skeleton.Joints[JointType.HandRight].Position.Y
                    && skeleton.Joints[JointType.ShoulderRight].Position.Y > skeleton.Joints[JointType.HandRight].Position.Y)
                {
                    // Debug.WriteLine("GesturePart 0 - left hand below shoulder height but above hip height - PASS");
                    // //left hand left of left Shoulder
                    // //몸보다 앞
                    if (skeleton.Joints[JointType.HipRight].Position.Z > skeleton.Joints[JointType.HandRight].Position.Z &&
                skeleton.Joints[JointType.HipRight].Position.Z < skeleton.Joints[JointType.HandRight].Position.Z + 0.18)
                    {
                        // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - PASS");
                        return_value = GesturePartResult.Succeed;
                    }

                    // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - UNDETERMINED");
                    else return_value = GesturePartResult.Pausing;
                }

                // Debug.WriteLine("GesturePart 0 - left hand below shoulder height but above hip height - FAIL");
                else return_value = GesturePartResult.Pausing;
            }

            // Debug.WriteLine("GesturePart 0 - left hand in front of left Shoulder - FAIL");
            else return_value = GesturePartResult.Fail;
            returns = new Dictionary<GesturePartResult, float>();
            returns.Add(return_value, result);
            return returns;
        }
    }

    /// <summary>
    /// The second part of the swipe right gesture
    /// </summary>
    public class PushSegment2 : IRelativeGestureSegment
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
            // //left hand in front of left ShoulderF
            //왼쪽 어깨보다 오른손이 오른족에 있다.
            if (skeleton.Joints[JointType.ShoulderLeft].Position.X < skeleton.Joints[JointType.HandRight].Position.X && leftHandGrip == true)
            {
                //왼쪽어깨가 오른손보다 아래 있다.
                if (skeleton.Joints[JointType.ShoulderLeft].Position.Y < skeleton.Joints[JointType.HandRight].Position.Y)
                {
                    //0.18과 0.45 사이에 손이 있다.
                    if (skeleton.Joints[JointType.HipRight].Position.Z > skeleton.Joints[JointType.HandRight].Position.Z + 0.18 &&
                    skeleton.Joints[JointType.HipRight].Position.Z < skeleton.Joints[JointType.HandRight].Position.Z + 0.45)
                    {
                        return_value = GesturePartResult.Succeed;
                    }

                        // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - UNDETERMINED");
                    else return_value = GesturePartResult.Pausing;
                }
                else return_value = GesturePartResult.Pausing;
                // Debug.WriteLine("GesturePart 0 - left hand below shoulder height but above hip height - FAIL");
            }
            // Debug.WriteLine("GesturePart 1 - left hand in front of left Shoulder - FAIL");
            else return_value = GesturePartResult.Fail;
            returns = new Dictionary<GesturePartResult, float>();
            returns.Add(return_value, result);
            return returns;
        }
    }

    /// <summary>
    /// The third part of the swipe right gesture
    /// </summary>
    public class PushSegment3 : IRelativeGestureSegment
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
            //왼쪽 어깨보다 오른손이 오른족에 있다.
            if (skeleton.Joints[JointType.ShoulderLeft].Position.X < skeleton.Joints[JointType.HandRight].Position.X && leftHandGrip == true)
            {
                //왼쪽어깨가 오른손보다 아래 있다.
                if (skeleton.Joints[JointType.Head].Position.Y > skeleton.Joints[JointType.HandRight].Position.Y)
                {
                    //0.45이상 힙과 차이가 난다
                    if (skeleton.Joints[JointType.HipLeft].Position.Z > skeleton.Joints[JointType.HandRight].Position.Z + 0.45)
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
