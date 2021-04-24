using System;
using System.Collections.Generic;
using System.Linq;

namespace CCCount.Models
{
    public interface ICCCountUI
    {

        List<Brand> Brands { get; }
        string SelectedBrandId { get; set; }
        string IncludeDrilldown { get; set; }

        string SelectedBrandIndex { get; set; }
        string SelectedBrandName { get; set; }
        List<string> SelectedDivisionIds { get; set; }
        List<string> SelectedDepartmentIds { get; set; }
        List<string> SelectedClassIds { get; set; }
        List<string> SelectedSubClassIds { get; set; }
        List<string> SelectedMarketIds { get; set; }
        string SelectedChannelId { get; set; }
        string SelectedAttribute1 { get; set; }
        string SelectedAttribute2 { get; set; }
        string SelectedAttributeId1 { get; set; }
        string SelectedAttributeId2 { get; set; }
        List<StartDateYear> StartDateYears { get; }
        string SelectedStartDateYearId { get; set; }
        List<StartDateMonth> StartDateMonths { get; }
        string SelectedStartDateMonthId { get; set; }

        List<ProgramCountProxy> ProgramCountProxies { get; }
        string SelectedProgramCountProxyId { get; set; }

        List<ProgramCountProxy> ComparisonCountProxies { get; }
        string SelectedComparisonCountProxyId { get; set; }

        List<ProgramCountProxy> DrilldownCountProxies { get; }
        string SelectedDrilldownCountProxyId { get; set; }

        bool IsTimeOnOfferSelected { get; set; }
        bool IsRankSelected { get; set; }
        bool IsSeasonCodeSelected { get; set; }
        bool IsHierarchyBreakdownCodeSelected { get; set; }

        Guid ConnectionId { get; set; }
        String GuidSQL { get; set; }
        String UserID { get; set; }
        string ErrorMessage { get; set; }
        int ErrorCode { get; set; }
    }

    public class CCCountUI : ICCCountUI
    {
        private List<ProgramCountProxy> _programCountProxies = new List<ProgramCountProxy> {
            new ProgramCountProxy { Name = "Auto Generated ID", Value = "GI", Selected = true },
            new ProgramCountProxy { Name = "GA Program Number", Value = "PN", Selected = false }
        };
        private List<ProgramCountProxy> _comparisonCountProxies = new List<ProgramCountProxy> {
            new ProgramCountProxy { Name = "Intent", Value = "0", Selected = true },
            new ProgramCountProxy { Name = "Actual", Value = "1", Selected = false }
        };
        private List<ProgramCountProxy> _drilldownCountProxies = new List<ProgramCountProxy>
        {
            new ProgramCountProxy { Name = "YES", Value = "1", Selected = false },
            new ProgramCountProxy { Name = "NO", Value = "0", Selected = true }
        };
        // Must remove once hooked up (all private members below)
        private List<StartDateYear> _years = new List<StartDateYear> {
            new StartDateYear { Name = (DateTime.Now.Year-2).ToString(), Value = (DateTime.Now.Year-2).ToString() },
            new StartDateYear { Name = (DateTime.Now.Year-1).ToString(), Value = (DateTime.Now.Year-1).ToString() },
            new StartDateYear { Name = (DateTime.Now.Year).ToString(), Value = (DateTime.Now.Year).ToString() },
            new StartDateYear { Name = (DateTime.Now.Year+1).ToString(), Value = (DateTime.Now.Year+1).ToString() },
            new StartDateYear { Name = (DateTime.Now.Year+2).ToString(), Value = (DateTime.Now.Year+2).ToString() }
        };

        private List<StartDateMonth> _months = new List<StartDateMonth> {
            new StartDateMonth { Name = "JAN", Value = "1" },
            new StartDateMonth { Name = "FEB", Value = "2" },
            new StartDateMonth { Name = "MAR", Value = "3" },
            new StartDateMonth { Name = "APR", Value = "4" },
            new StartDateMonth { Name = "MAY", Value = "5" },
            new StartDateMonth { Name = "JUN", Value = "6" },
            new StartDateMonth { Name = "JUL", Value = "7" },
            new StartDateMonth { Name = "AUG", Value = "8" },
            new StartDateMonth { Name = "SEP", Value = "9" },
            new StartDateMonth { Name = "OCT", Value = "10" },
            new StartDateMonth { Name = "NOV", Value = "11" },
            new StartDateMonth { Name = "DEC", Value = "12" }
        };

        public CCCountUI()
        {
            this.Brands = new List<Brand>();
        }

        public List<Brand> Brands { get; set; }

        public string SelectedBrandId { get; set; }
        public string IncludeDrilldown { get; set; }
        public string SelectedBrandIndex { get; set; }
        public string SelectedBrandName { get; set; }
        public List<string> SelectedDivisionIds { get; set; }
        public List<string> SelectedDepartmentIds { get; set; }
        public List<string> SelectedClassIds { get; set; }
        public List<string> SelectedSubClassIds { get; set; }

        public List<string> SelectedMarketIds { get; set; }
        public string SelectedChannelId { get; set; }
        public string SelectedAttribute1 { get; set; }
        public string SelectedAttribute2 { get; set; }

        public string SelectedAttributeId1 { get; set; }
        public string SelectedAttributeId2 { get; set; }

        public string SelectedStartDateYearId { get; set; }
        public List<StartDateYear> StartDateYears
        {
            get { return _years; }
            set { }
        }

        public string SelectedStartDateMonthId { get; set; }
        public List<StartDateMonth> StartDateMonths
        {
            get { return _months; }
            set { }
        }

        public string SelectedProgramCountProxyId { get; set; }
        public List<ProgramCountProxy> ProgramCountProxies
        {
            get { return _programCountProxies; }
        }


        public string SelectedComparisonCountProxyId { get; set; }
        public List<ProgramCountProxy> ComparisonCountProxies
        {
            get { return _comparisonCountProxies; }
        }


        public string SelectedDrilldownCountProxyId { get; set; }
        public List<ProgramCountProxy> DrilldownCountProxies
        {
            get { return _drilldownCountProxies; }
        }




        public bool IsTimeOnOfferSelected { get; set; }
        public bool IsRankSelected { get; set; }
        public bool IsSeasonCodeSelected { get; set; }
        public bool IsHierarchyBreakdownCodeSelected { get; set; }

        public Guid ConnectionId { get; set; }
        public string GuidSQL { get; set; }
        public String UserID { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }
    }

    public class SampleCCCountUI : ICCCountUI
    {
        private List<ProgramCountProxy> _programCountProxies = new List<ProgramCountProxy> {
            new ProgramCountProxy { Name = "Auto Generated ID", Value = "GI", Selected = true },
            new ProgramCountProxy { Name = "GA Program Number", Value = "PN", Selected = false }
        };
        private List<ProgramCountProxy> _comparisonCountProxies = new List<ProgramCountProxy> {
            new ProgramCountProxy { Name = "Intent", Value = "0", Selected = true },
            new ProgramCountProxy { Name = "Actual", Value = "1", Selected = false }
        };
        private List<ProgramCountProxy> _drilldownCountProxies = new List<ProgramCountProxy>
        {
            new ProgramCountProxy { Name = "YES", Value = "1", Selected = false },
            new ProgramCountProxy { Name = "NO", Value = "0", Selected = true }
        };
        private List<Brand> _brands = new List<Brand> {
            new Brand { Name = "GAP",
                        Value = "1",
                        Divisions = new List<Division> {
                            new Division { Name = "MENS", Value = "01 112", Departments = new List<Department> {
                                new Department { Name = "MENS BOTTOMS", Value = "01 1120", Classes = new List<Class> {
                                    new Class { Name = "FASH BOT", Value = "01 1120 01" },
                                    new Class { Name = "ACT BTMS", Value = "01 1120 02" },
                                    new Class { Name = "SHORTS", Value = "01 1120 05" } }
                                },
                                new Department { Name = "MENS KNITS", Value = "01 1123", Classes = new List<Class> {
                                    new Class { Name = "FLEECE", Value = "01 1123 04" },
                                    new Class { Name = "BAS TEES", Value = "01 1123 05" },
                                    new Class { Name = "NOVELTY", Value = "01 1123 06" } }
                                },
                                new Department { Name = "MENS SWEATERS", Value = "01 1124", Classes = new List<Class> {
                                    new Class { Name = "CTNBLEND", Value = "01 1124 01" },
                                    new Class { Name = "WL/WLBLD", Value = "01 1124 02" } }
                                } }
                            },
                            new Division { Name = "ACCESSORIES", Value = "01 113", Departments = new List<Department> {
                                new Department { Name = "JEWELRY", Value = "01 1135", Classes = new List<Class> {
                                    new Class { Name = "NECKLACES", Value = "01 1135 00" },
                                    new Class { Name = "ITEMS", Value = "01 1135 01" },
                                    new Class { Name = "BRACELETS", Value = "01 1135 02" } }
                                },
                                new Department { Name = "SUNGLASSES", Value = "01 113", Classes = new List<Class> {
                                    new Class { Name = "MEN SUNG", Value = "01 1134 01" },
                                    new Class { Name = "WMN SUNG", Value = "01 1134 00" } }
                                } }
                            },
                            new Division { Name = "BABY", Value = "01 166", Departments = new List<Department> {
                                new Department { Name = "NEWBORN", Value = "01 1665", Classes = new List<Class> {
                                    new Class { Name = "SWEATERS", Value = "01 1665 01" },
                                    new Class { Name = "1-PIECES", Value = "01 1665 03" },
                                    new Class { Name = "OVERALLS", Value = "01 1665 04" },
                                    new Class { Name = "TOPS", Value = "01 1665 05" },
                                    new Class { Name = "DRESSES", Value = "01 1665 06" },
                                    new Class { Name = "WVN BTMS", Value = "01 1665 07" },
                                    new Class { Name = "DNM BTMS", Value = "01 1665 08" },
                                    new Class { Name = "OUTRWR", Value = "01 1665 09" },
                                    new Class { Name = "SOCKS", Value = "01 1665 10" } }
                                },
                                new Department { Name = "BABYGAP HOME", Value = "01 1666", Classes = new List<Class> {
                                    new Class { Name = "TEXTILES", Value = "01 1666 00" },
                                    new Class { Name = "BATH", Value = "01 1666 01" },
                                    new Class { Name = "RM DECOR", Value = "01 1666 02" } }
                                } }
                            }
                        },
                        Channels = new List<Channel> {
                            new Channel { Name = "OMNI", Value = "0", Selected = true },
                            new Channel { Name = "RETAIL", Value = "1", Selected = false },
                            new Channel { Name = "ONLINE", Value = "2", Selected = false }
                        },
                        Markets = new List<Market> {
                            new Market { Name = "UNITED STATES", Value = "1", Checked = true },
                            new Market { Name = "CANADA", Value = "2", Checked = false },
                            new Market { Name = "EUROPE", Value = "3", Checked = false },
                            new Market { Name = "JAPAN", Value = "4", Checked = false },
                            new Market { Name = "CHINA", Value = "5", Checked = false }
                        },
                        Attribute1 = new List<Attribute> {
                            new Attribute { Name = "Test1", Value = "" },
                            new Attribute { Name = "TestA", Value = "" },
                            new Attribute { Name = "TestB", Value = "" },
                        },
                        IsRankEnabled = true,
                        IsTimeOnOfferEnabled = true
                      },
            new Brand { Name = "BANANA REPUBLIC", Value = "2", Divisions = new List<Division>() },
            new Brand { Name = "OLD NAVY", Value = "3", Divisions = new List<Division>() }
        };

        private List<StartDateYear> _years = new List<StartDateYear> {
            new StartDateYear { Name = (DateTime.Now.Year-2).ToString(), Value = (DateTime.Now.Year-2).ToString() },
            new StartDateYear { Name = (DateTime.Now.Year-1).ToString(), Value = (DateTime.Now.Year-1).ToString() },
            new StartDateYear { Name = (DateTime.Now.Year).ToString(), Value = (DateTime.Now.Year).ToString() },
            new StartDateYear { Name = (DateTime.Now.Year+1).ToString(), Value = (DateTime.Now.Year+1).ToString() },
            new StartDateYear { Name = (DateTime.Now.Year+2).ToString(), Value = (DateTime.Now.Year+2).ToString() }
        };

        private List<StartDateMonth> _months = new List<StartDateMonth> {
            new StartDateMonth { Name = "JAN", Value = "1" },
            new StartDateMonth { Name = "FEB", Value = "2" },
            new StartDateMonth { Name = "MAR", Value = "3" },
            new StartDateMonth { Name = "APR", Value = "4" },
            new StartDateMonth { Name = "MAY", Value = "5" },
            new StartDateMonth { Name = "JUN", Value = "6" },
            new StartDateMonth { Name = "JUL", Value = "7" },
            new StartDateMonth { Name = "AUG", Value = "8" },
            new StartDateMonth { Name = "SEP", Value = "9" },
            new StartDateMonth { Name = "OCT", Value = "10" },
            new StartDateMonth { Name = "NOV", Value = "11" },
            new StartDateMonth { Name = "DEC", Value = "12" }
        };

        public List<Brand> Brands
        {
            get { return _brands; }
        }
        public string SelectedBrandId { get; set; }
        public string IncludeDrilldown { get; set; }

        public string SelectedBrandIndex { get; set; }
        public string SelectedBrandName { get; set; }
        public List<string> SelectedDivisionIds { get; set; }
        public List<string> SelectedDepartmentIds { get; set; }
        public List<string> SelectedClassIds { get; set; }
        public List<string> SelectedSubClassIds { get; set; }
        public List<string> SelectedMarketIds { get; set; }
        public string SelectedChannelId { get; set; }
        public string SelectedAttribute1 { get; set; }
        public string SelectedAttribute2 { get; set; }
        public string SelectedStartDateYearId { get; set; }
        public List<StartDateYear> StartDateYears
        {
            get { return _years; }
        }

        public string SelectedStartDateMonthId { get; set; }
        public List<StartDateMonth> StartDateMonths
        {
            get { return _months; }
        }

        public string SelectedAttributeId1 { get; set; }
        public string SelectedAttributeId2 { get; set; }

        public string SelectedProgramCountProxyId { get; set; }
        public List<ProgramCountProxy> ProgramCountProxies
        {
            get { return _programCountProxies; }
        }

        public string SelectedComparisonCountProxyId { get; set; }
        public List<ProgramCountProxy> ComparisonCountProxies
        {
            get { return _comparisonCountProxies; }
        }

        public string SelectedDrilldownCountProxyId { get; set; }
        public List<ProgramCountProxy> DrilldownCountProxies
        {
            get { return _drilldownCountProxies; }
        }



        public bool IsTimeOnOfferSelected { get; set; }
        public bool IsRankSelected { get; set; }
        public bool IsSeasonCodeSelected { get; set; }
        public bool IsHierarchyBreakdownCodeSelected { get; set; }

        public Guid ConnectionId { get; set; }
        public string GuidSQL { get; set; }
        public String UserID { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }
    }

    public class authenticationModel
    {
        public string receiveTime { get; set; }
        public string senderApplication { get; set; }
        public string GroupName { get; set; }
        public string userID { get; set; }
        public string userName { get; set; }
        public List<application> applications { get; set; }
        public List<ADGroupName> ADGroups { get; set; }
    }
    public class application
    {
        public string appName { get; set; }
        public string authFlag { get; set; }
        public string message { get; set; }
        public string productManagerName { get; set; }
        public string productManagerEmail { get; set; }
        public string accessLevel { get; set; }
        public string sessionTimeout { get; set; }
        public string userGuideURL { get; set; }

    }
    public class ADGroupName
    {
        public string groupNames { get; set; }

    }
}
