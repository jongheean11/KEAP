using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KEAP
{
    class TemplateListItem
    {
        public Rectangle templateSmall { get; set; }
        // 
        public SolidColorBrush rectangleColor { get; set; }
        public string name { get; set; }


        public TemplateListItem(Rectangle templateSmall, SolidColorBrush rectangleColor, string name)
        {
            this.templateSmall = templateSmall;
            this.rectangleColor = rectangleColor;
            this.name = name;
        }
        
    }
}
