using Microsoft.Kinect;
using System.Collections.Generic;
using System;



namespace Fizbin.Kinect.Gestures.Segments
{
    /// <summary>
    /// The first part of the swipe right gesture
    /// </summary>
    public class TurnSegments1 : IRelativeGestureSegment
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
                    if (
                    skeleton.Joints[JointType.HipRight].Position.Z < skeleton.Joints[JointType.HandRight].Position.Z + 0.45)
                    {
                        // Debug.WriteLine("GesturePart 0 - left hand below shoulder height but above hip height - PASS");
                        // //left hand left of left Shoulder
                        // //몸보다 앞
                        if (skeleton.Joints[JointType.HipRight].Position.Z > skeleton.Joints[JointType.HandRight].Position.Z)
                        {
                            // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - PASS");
                            return_value = GesturePartResult.Succeed;
                        }
                        else return_value = GesturePartResult.Pausing;

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
    public class TurnSegments2 : IRelativeGestureSegment
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
            // //left hand in front of left Shoulder
            //손이 몸앞에 있다.
            if (skeleton.Joints[JointType.HipRight].Position.Z > skeleton.Joints[JointType.HandRight].Position.Z && leftHandGrip == true)
            {
                //오른어깨가 오른손보다 아래 있다.
                if (skeleton.Joints[JointType.ShoulderLeft].Position.Y < skeleton.Joints[JointType.HandRight].Position.Y)
                {
                    //오른쪽 어깨보다 오른손이 오른족에 있다.
                    if (
                  skeleton.Joints[JointType.HipRight].Position.Z < skeleton.Joints[JointType.HandRight].Position.Z + 0.45)
                    {
                        if (skeleton.Joints[JointType.ShoulderRight].Position.X < skeleton.Joints[JointType.HandRight].Position.X)
                        {
                            return_value = GesturePartResult.Succeed;
                        }

                            // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - UNDETERMINED");
                        else return_value = GesturePartResult.Pausing;
                    }
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
    public class TurnSegments3 : IRelativeGestureSegment
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
            // //left hand in front of left Shoulder

            //손이 몸앞에 있다.
            if (skeleton.Joints[JointType.HipRight].Position.Z > skeleton.Joints[JointType.HandRight].Position.Z && leftHandGrip == true)
            {
                //오른어깨가 오른손보다 아래 있다.
                if (skeleton.Joints[JointType.ShoulderRight].Position.Y > skeleton.Joints[JointType.HandRight].Position.Y)
                {
                    //오른쪽 어깨보다 오른손이 오른족에 있다.
                    if (skeleton.Joints[JointType.HipRight].Position.Z < skeleton.Joints[JointType.HandRight].Position.Z + 0.45)
                    {
                        if (
                            //skeleton.Joints[JointType.ShoulderRight].Position.X < skeleton.Joints[JointType.HandRight].Position.X - 0.2

                            Math.Atan2(Convert.ToDouble(skeleton.Joints[JointType.HandRight].Position.X - skeleton.Joints[JointType.ShoulderRight].Position.X),
                            Convert.ToDouble(skeleton.Joints[JointType.ShoulderRight].Position.Y - skeleton.Joints[JointType.HandRight].Position.Y)) > 0.98
                            )
                        {
                            return_value = GesturePartResult.Succeed;
                        }


                            // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - UNDETERMINED");
                        else return_value = GesturePartResult.Pausing;
                    }
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
}
