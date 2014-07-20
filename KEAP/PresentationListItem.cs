using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KEAP
{
    class PresentationListItem
    {
        public string presentationSmall { get; set; }
        public string name { get; set; }
        public string date { get; set; }
        public string filePath { get; set; }
        public PresentationListItem(string presentationSmall, string name, string date, string filePath)
        {
            this.presentationSmall = presentationSmall;
            this.name = name;
            this.date = date;
            this.filePath = filePath;
        }
        
    }
}
