using Microsoft.Kinect;
using System.Collections.Generic;

namespace Fizbin.Kinect.Gestures.Segments
{
    public class WaveRightSegment1 : IRelativeGestureSegment
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
            // hand above elbow
            float result = skeleton.Joints[JointType.HandRight].Position.Z;
            GesturePartResult return_value;
            Dictionary<GesturePartResult, float> returns;
            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y)
            {
                // hand right of elbow
                if (skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.ElbowRight].Position.X)
                {
                    return_value = GesturePartResult.Succeed;
                }

                // hand has not dropped but is not quite where we expect it to be, pausing till next frame
                else return_value = GesturePartResult.Pausing;
            }

            // hand dropped - no gesture fails
            else return_value = GesturePartResult.Fail;
             returns = new Dictionary<GesturePartResult, float>();
             returns.Add(return_value, result);
             return returns;
        }
    }
    public class WaveRightSegment : IRelativeGestureSegment
    {


        public Dictionary<GesturePartResult, float> CheckGesture(Skeleton skeleton, bool leftHandGrip, bool rightHandGrip)
        {
            // hand above elbow
            float result = skeleton.Joints[JointType.HandRight].Position.Z;
            GesturePartResult return_value;
            Dictionary<GesturePartResult, float> returns;

            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y)
            {
                // hand right of elbow
                if (skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.ElbowRight].Position.X)
                {
                    return_value = GesturePartResult.Succeed;
                } 
                // hand has not dropped but is not quite where we expect it to be, pausing till next frame
                else return_value = GesturePartResult.Pausing;
            }

            // hand dropped - no gesture fails
            else return_value = GesturePartResult.Fail;

            returns = new Dictionary<GesturePartResult, float>();
            returns.Add(return_value, result);
            //float a;
            //if(returns.ContainsKey(GesturePartResult.Succeed))
              //  a = returns[GesturePartResult.Succeed];
                //returns.ContainsKey(GesturePartResult.Pausing)
                  //  returns.ContainsKey(GesturePartResult.Fail)
            
            return returns;
        }
    }

    public class WaveRightSegment2 : IRelativeGestureSegment
    {
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>GesturePartResult based on if the gesture part has been completed</returns>
        public Dictionary<GesturePartResult, float> CheckGesture(Skeleton skeleton, bool leftHandGrip, bool rightHandGrip)
        {
            // hand above elbow
            float result = skeleton.Joints[JointType.HandRight].Position.Z;
            GesturePartResult return_value;
            Dictionary<GesturePartResult, float> returns;
           
            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y)
            {
                // hand right of elbow
                if (skeleton.Joints[JointType.HandRight].Position.X < skeleton.Joints[JointType.ElbowRight].Position.X)
                {
                    return_value= GesturePartResult.Succeed;
                }

                // hand has not dropped but is not quite where we expect it to be, pausing till next frame
                else return_value = GesturePartResult.Pausing;
            }

            // hand dropped - no gesture fails
            else return_value = GesturePartResult.Fail;
            returns = new Dictionary<GesturePartResult, float>();
            returns.Add(return_value, result);
            return returns;
        }
    }
}
