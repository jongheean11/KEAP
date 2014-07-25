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
            if (initialSize.Height == 0)
            {
                initialSize = arrangeSize;
            }
            double ratio_X = arrangeSize.Width / initialSize.Width;
            double ratio_Y = arrangeSize.Height / initialSize.Height;
            
            foreach (UIElement child in InternalChildren)
            {
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
                
                if (child is EditableTextBlock)
                {
                    Canvas.SetLeft((child), X * ratio_X);
                    Canvas.SetTop((child), Y * ratio_Y);
                    ((EditableTextBlock)child).Width = ((EditableTextBlock)child).Width * ratio_X;
                    ((EditableTextBlock)child).Height = ((EditableTextBlock)child).Height * ratio_Y;
                    child.Arrange(new Rect(init_Point, new Size(child.DesiredSize.Width * ratio_X, child.DesiredSize.Height * ratio_Y)));
                }
                else if (child is Polygon)
                {
                    //Canvas.SetLeft((child), X * ratio_X);
                    //Canvas.SetTop((child), Y * ratio_Y);
                    for (int i = 0; i < ((Polygon)child).Points.Count; i++)
                    {
                        ((Polygon)child).Points[i] = new Point(((Polygon)child).Points[i].X * ratio_X, ((Polygon)child).Points[i].Y * ratio_Y);
                    }
                    //   ((Line)child).Width = ((Line)child).Width * ratio_X;
                    // ((Line)child).Height = ((Line)child).Height * ratio_Y;
                    child.Arrange(new Rect(0, 0, arrangeSize.Width, arrangeSize.Height));
                }
                else if (child is Line)
                {
                    ((Line)child).X1 = ((Line)child).X1 * ratio_X;
                    ((Line)child).Y1 = ((Line)child).Y1 * ratio_Y;
                    ((Line)child).X2 = ((Line)child).X2 * ratio_X;
                    ((Line)child).Y2 = ((Line)child).Y2 * ratio_Y;
                    //   ((Line)child).Width = ((Line)child).Width * ratio_X;
                    // ((Line)child).Height = ((Line)child).Height * ratio_Y;
                    child.Arrange(new Rect(0, 0, arrangeSize.Width, arrangeSize.Height));
                }
                else if (child is Image)
                {
                    Canvas.SetLeft((child), X * ratio_X);
                    Canvas.SetTop((child), Y * ratio_Y);
                    ((Image)child).Width = ((Image)child).Width * ratio_X;
                    ((Image)child).Height = ((Image)child).Height * ratio_Y;
                    child.Arrange(new Rect(init_Point, new Size(child.DesiredSize.Width * ratio_X, child.DesiredSize.Height * ratio_Y)));
                }
                else if (child is Rectangle)
                {
                    Canvas.SetLeft((child), X*ratio_X);
                    Canvas.SetTop((child), Y*ratio_Y);
                    ((Rectangle)child).Width = ((Rectangle)child).Width * ratio_X;
                    ((Rectangle)child).Height = ((Rectangle)child).Height * ratio_Y;
                    child.Arrange(new Rect(init_Point, new Size(child.DesiredSize.Width * ratio_X, child.DesiredSize.Height * ratio_Y)));
                }
                else if (child is Ellipse)
                {
                    Canvas.SetLeft((child), X * ratio_X);
                    Canvas.SetTop((child), Y * ratio_Y);
                    ((Ellipse)child).Width = ((Ellipse)child).Width * ratio_X;
                    ((Ellipse)child).Height = ((Ellipse)child).Height * ratio_Y;
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
