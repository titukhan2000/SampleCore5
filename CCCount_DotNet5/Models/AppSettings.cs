using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCCount.Models
{
    public class ConnectionStrings
    {
        public ConnectionStrings() { }

        public string SetupUrl { get; set; }
        public string ReportUrl { get; set; }
        public string SetupData_Old { get; set; }
        public string SetupData { get; set; }
        public string ReportSPName { get; set; }
        public string SetupData_NLog { get; set; }

    }

    public class ContentTypes
    {
        public ContentTypes() { }

        public string ExcelXlsx { get; set; }
        public string ExcelXlsm { get; set; }
    }

    public class CCCountSettings
    {
        public CCCountSettings() { }

        public int MarketsMaxRows { get; set; }
    }
}
