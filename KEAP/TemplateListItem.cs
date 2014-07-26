using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KEAP
{
    public class TemplateListItem
    {
//        public Rectangle templateSmall { get; set; }       
//        public SolidColorBrush rectangleColor { get; set; }
        public string name { get; set; }
        public string img_Uri { get; set; }
        public string thumb_Img_Uri { get; set; }
        public Color stroke_Brush { get; set; }
        public Color fill_Brush { get; set; }
        public Color font_Brush { get; set; }

        public TemplateListItem(Rectangle templateSmall, SolidColorBrush rectangleColor, string name)
        {
            //this.templateSmall = templateSmall;
            //this.rectangleColor = rectangleColor;
            this.name = name;
        }

        public TemplateListItem(string name, string backgroundImg_Name, Color stroke_Brush, Color fill_Brush, Color font_Brush)
        {
            this.name = name;
            this.thumb_Img_Uri = "pack://application:,,,/KEAP;v1.0.0.0;component/Images/Background/thumbnail_" + backgroundImg_Name;
            this.img_Uri = "pack://application:,,,/KEAP;v1.0.0.0;component/Images/Background/" + backgroundImg_Name;
            this.stroke_Brush = stroke_Brush;
            this.fill_Brush = fill_Brush;
            this.font_Brush = font_Brush;
        }

    }
}
