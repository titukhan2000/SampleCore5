using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCCount.Models
{
    public class ResponseFromAPI
    {
        public IList<Worksheet> Worksheet { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class Worksheet
    {
        public WorksheetContent WorksheetContent { get; set; }
        public string WorksheetName { get; set; }
        public string ErrorMsg { get; set; }
        public string Notes { get; set; }
        public IList<LineStyle> PublicStyle { get; set; }
    }

    public class WorksheetContent
    {
        public string NumberOfGroups { get; set; }
        public IList<Groups> Groups { get; set; }

    }
    public class Groups
    {
        public IList<WorksheetLines> Headers { get; set; }
        public IList<WorksheetLines> Lines { get; set; }
        public string Name { get; set; }
        public string StartingCell { get; set; }
    }
    public class WorksheetLines
    {
        public string Range { get; set; }
        public string LineString { get; set; } // 12$,14,12%,...
        public string Delimiter { get; set; } // ','
        public IList<LineStyle> LineStyle { get; set; }
        public string Comment { get; set; } // ','

    }
    public class LineStyle
    {
        public string CellAddress { get; set; }
        public string BackColor { get; set; }
        public string ForeColor { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Bold { get; set; } // True , False
        public string Italic { get; set; } // True , False
        public string FontSize { get; set; } //
        public string FontName { get; set; } // 
        public string Align { get; set; } // Left , Right, Center
        public string Border { get; set; } // Non , Left, Right, Top, Bottom, All
        public string BorderWidth { get; set; } // 1
        public string BorderColor { get; set; } // Black

    }
}
