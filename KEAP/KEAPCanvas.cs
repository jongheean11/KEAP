using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KEAP
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:KEAP"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:KEAP;assembly=KEAP"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class KEAPCanvas : Canvas
    {
        private Size initialSize;

        protected override Size MeasureOverride(Size constraint)
        {
            Size panelDesiredSize = new Size();
            foreach (FrameworkElement child in InternalChildren)
            {
                child.Measure(constraint);
                panelDesiredSize = child.DesiredSize;
            }
            return panelDesiredSize;
            //return constraint;
        }
        
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            
         /*   double ratio_Width=final_Rect.Width / previous_Size.Width,
                ratio_Height=final_Rect.Height / previous_Size.Height;
           /*
            foreach (FrameworkElement child in InternalChildren)
            {
   //             child.Width = child.Width * ratio_Width;
     //           child.Height = child.Height * ratio_Height;
             //   child.Arrange(final_Rect);
            }

            previous_Size = arrangeSize;
           /return arrangeSize;
        }*/
            foreach (FrameworkElement child in InternalChildren)
           /* {
                double left = Canvas.GetLeft(child);
                double top = Canvas.GetTop(child);
                Point canvasPoint = new Point(left * ratio_Width, top * ratio_Height);
                child.Arrange(new Rect(canvasPoint.X, canvasPoint.Y, 
                    (child.DesiredSize.Width * ratio_Width),// + canvasPoint.X, 
                    (child.DesiredSize.Height * ratio_Height))); //+ canvasPoint.Y));
            }
            previous_Size = arrangeSize;
            return arrangeSize;*/
            if (initialSize.Height == 0)
            {
                initialSize = arrangeSize;
            }
            double ratio_X = arrangeSize.Width / initialSize.Width;
            double ratio_Y = arrangeSize.Height / initialSize.Height;
            //for (int index = 0; index < this.InternalChildren.Count; index++)
            Rect final_Rect = new Rect();
            foreach (UIElement child in InternalChildren)
            {
                //FrameworkElement child = this.InternalChildren[index];

                double X;
                double Y;
                X = Canvas.GetLeft(child);
                Y = Canvas.GetTop(child);
                Point init_Point = new Point(X, Y);
                if (ratio_X > 1)
                {
                    init_Point.X = ratio_X * X - X;
                }
                if (ratio_Y > 1)
                {
                    init_Point.Y = ratio_Y * Y - Y;
                }
                if (child is Line)
                {
                    //Canvas.SetLeft((child), X * ratio_X);
                    //Canvas.SetTop((child), Y * ratio_Y);
                    ((Line)child).X1 = ((Line)child).X1 * ratio_X;
                    ((Line)child).Y1 = ((Line)child).Y1 * ratio_Y;
                    ((Line)child).X2 = ((Line)child).X2 * ratio_X;
                    ((Line)child).Y2 = ((Line)child).Y2 * ratio_Y;
                 //   ((Line)child).Width = ((Line)child).Width * ratio_X;
                   // ((Line)child).Height = ((Line)child).Height * ratio_Y;
                    child.Arrange(new Rect(0, 0, arrangeSize.Width, arrangeSize.Height));
                }
                else if (child is EditableTextBlock)
                {
                    Canvas.SetLeft((child), X * ratio_X);
                    Canvas.SetTop((child), Y * ratio_Y);
                    ((EditableTextBlock)child).Width = ((EditableTextBlock)child).Width * ratio_X;
                    ((EditableTextBlock)child).Height = ((EditableTextBlock)child).Height * ratio_Y;
                    child.Arrange(new Rect(init_Point, new Size(child.DesiredSize.Width * ratio_X, child.DesiredSize.Height * ratio_Y)));
                }
                else if (child is Shape)
                {
                    Canvas.SetLeft((child), X*ratio_X);
                    Canvas.SetTop((child), Y*ratio_Y);
                    ((Shape)child).Width = ((Shape)child).Width * ratio_X;
                    ((Shape)child).Height = ((Shape)child).Height * ratio_Y;
                    child.Arrange(new Rect(init_Point, new Size(child.DesiredSize.Width * ratio_X, child.DesiredSize.Height * ratio_Y)));
                }
                //final_Rect = new Rect(X*ratio_X, Y*ratio_Y, arrangeSize.Width * ratio_X, arrangeSize.Height * ratio_Y);
                //child.Arrange(final_Rect);
                //Size returnSize = new Size(child.DesiredSize.Width * ratio_X, child.DesiredSize.Height * ratio_Y);
                //child.Arrange((new Rect(new Point(X, (ratio_Y * Y) - Y), child.DesiredSize)));
                //child.Arrange(arrangeSize.Height > initialSize.Height ? (new Rect(new Point((ratio_X * X) - X, (ratio_Y * Y) - Y), returnSize)) : (new Rect(new Point(X, Y), returnSize)));
                //child.Arrange(new Rect(init_Point, new Size(child.DesiredSize.Width * ratio_X, child.DesiredSize.Height * ratio_Y)));
            }
            initialSize = arrangeSize;
            return arrangeSize; // Returns the final Arranged size
        }
    }
}
