using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace KEAP.Animations
{
    class CustomZoomInEaseOutFunction : EasingFunctionBase
    {
        public CustomZoomInEaseOutFunction()
            : base()
        {

        }

        protected override double EaseInCore(double normalizedTime)
        {
            //            return -(normalizedTime * (normalizedTime - 1));
            return 3 * (normalizedTime * (normalizedTime - 1));
        }

        protected override Freezable CreateInstanceCore()
        {
            return new CustomZoomInEaseOutFunction();
        }
    }

    class CustomZoomOutEaseOutFunction : EasingFunctionBase
    {
        public CustomZoomOutEaseOutFunction()
            : base()
        {

        }

        protected override double EaseInCore(double normalizedTime)
        {
            //            return -(normalizedTime * (normalizedTime - 1));
            return -3 * (normalizedTime * (normalizedTime - 1));
        }

        protected override Freezable CreateInstanceCore()
        {
            return new CustomZoomOutEaseOutFunction();
        }
    }

}
