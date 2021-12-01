using CCCount.Functions;
using CCCount.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using NLog;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ADLandingPage.Models;
using System.Text;
using System.Data.SqlClient;
using Microsoft.AspNetCore.HttpOverrides;
namespace CCCount.Controllers
{
    public class HomeController : Controller
    {
        private ICCCountUI _ccCountUI;

        private readonly ConnectionStrings _connectionStrings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ContentTypes _contentTypes;
        private readonly CCCountSettings _ccCountSettings;
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static HttpClient _client = new HttpClient();
        private int[] verticalHeaderPosition = { 0, 74, 148 };
        private int startFieldPosition = 22;
        private int numberOfColumnsAfterData = 12;
        public string HeaderFieldNames;
        public string codeChange="true";

        public HomeController(ICCCountUI ccCountUI,
                              IOptions<ConnectionStrings> connectionStrings,
                              IOptions<ContentTypes> contentTypes,
                              IOptions<CCCountSettings> ccCountSettings,
                              IHostingEnvironment hostingEnvironment,
                              ILogger<HomeController> logger,
                              IHttpContextAccessor httpContextAccessor)
        {
            _ccCountUI = ccCountUI;
            _connectionStrings = connectionStrings.Value;
            _hostingEnvironment = hostingEnvironment;
            _contentTypes = contentTypes.Value;
            _ccCountSettings = ccCountSettings.Value;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        private static string GetUri(HttpRequest request)
        {
            return request.Scheme+"://"+ request.Host.Value+ request.Path;
        }
        bool validateLogin(string applicationName) {
            bool Result = false;
            //try block starts
            try
            {

                // *********************** Start Authentication
                string authModelJson = "";
                string authCheck = "";
                authenticationModel authModel = new authenticationModel();
                if (!(ViewData["Auth"] == null))
                {
                    authModel = (authenticationModel)ViewData["Auth"];
                }

                //  if cookie is not empty transfer value to Session. because the cookie will be expired in seconds

                if (Request.Cookies["Auth"] != null)
                {
                    byte[] key32_1 = Encoding.ASCII.GetBytes("12345678901234567890123456789012");
                    byte[] key32_2 = Encoding.ASCII.GetBytes("abcdefghijklmnopqrstuvwxyzABCDEF");
                    authModelJson = Request.Cookies["Auth"].ToString();
                    string decryptedString = AESThenHMAC.SimpleDecrypt(authModelJson, key32_1, key32_2);
                    authModel = JsonConvert.DeserializeObject<authenticationModel>(decryptedString);
                    ViewData["Auth"] = authModel;

                } 

                if (authModel.userID == null){
                    // RadWindowManager1.RadAlert("No UserID!", 330, 180, "Error Message3", "alertCallBackFn", "Text")
                    string redirectUrlString = "http://dgphxprofs001.dev.gap.com:81/login/login.aspx?URL=" + GetUri(Request);
                    Response.Redirect(redirectUrlString);
                   // Response.Redirect("http://localhost:58572/Login/Login.aspx?URL=" + GetUri(Request));
                    return false;
                    //Request.get
                }


                foreach (application app in authModel.applications)
                {
                    //if (((app.appName == "CC Count")
                    if (((app.appName == applicationName)
                                && (app.authFlag == "True")))
                    {
                        authCheck = "Authorized";
                        Result = true;
                    }

                }



                if ((authCheck != "Authorized"))
                {
                     Response.Redirect("http://dgphxprofs001.dev.gap.com:81/unAuthorized/unAuthorized.aspx");
                    //Response.Redirect("http://localhost:58572/unAuthorized/unAuthorized.aspx");// for local debuggimg
                    return false;
                }

                // ************************End Authentication



                // Get IP address, if possible
                _logger.LogInformation($"Remote IP Address (1): {_httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString()}");
            }
            catch (Exception e)
            {
                Response.Redirect("http://dgphxprofs001.dev.gap.com:81/unAuthorized/unAuthorized.aspx");
                return false;
                // Do nothing
            }
            return Result;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {

            //validateLogin("CC Count");


            try 
            {
                // Get API response object
                //var response = await _client.GetAsync(_connectionStrings.SetupUrl);

                // API response data as JSON object
                //var merchHierarchyData = await response.Content.ReadAsStringAsync();

                // Convert JSON object to CCCountUI object
                //_ccCountUI = JsonConvert.DeserializeObject<CCCountUI>(merchHierarchyData);
            } 
            catch (Exception ex)
            {
                LogError(ex);
                throw new Exception("Error getting hierarchy data, please check your connection and refresh page to try again.");
            }
            
            try 
            {
                // Set connection id, set log
                _ccCountUI.ConnectionId = Guid.NewGuid();
                LogManager.Configuration.Variables["connectionId"] = _ccCountUI.ConnectionId.ToString();
                LogManager.Configuration.Variables["UserID"] = (Environment.UserName != "") ? Environment.UserName : Request.HttpContext.Connection.RemoteIpAddress.ToString();
                ;
                _logger.LogInformation($"Associated connection id for this request: {_ccCountUI.ConnectionId.ToString()}");
            } 
            catch (Exception ex) {
                LogError(ex);
                throw new Exception("Error setting connection id. (.Index())");
            }
            
            return View(_ccCountUI);
        }
  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CCCountUI model)
        {
            string guid = Guid.NewGuid().ToString();
            LogManager.Configuration.Variables["GuidSQL"] = guid;

            _ccCountUI.GuidSQL = guid.ToString();

            var runId = CCCountFunctions.GetShortGuid();
            var startTime = DateTime.Now;

            try {
                // Set connection id
                _logger.LogInformation($"Associated connection id for this request: {model.ConnectionId.ToString()}");
            } catch (Exception ex) {
                LogError(ex);
                throw new Exception("Error setting connection id. .Index() - POST");
            }

            // Log run to dbo.RunHistory
            try {
                using (var conn = new OleDbConnection(_connectionStrings.SetupData)) {
                    var spName = "dbo.usp_LogRunInformation";
                    var cmd = new OleDbCommand(spName, conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@runId", runId);
                    cmd.Parameters.AddWithValue("@connectionId", model.ConnectionId);
                    //cmd.Parameters.AddWithValue("@GuidSQL", model.GuidSQL);
                    cmd.Parameters.AddWithValue("@model", JsonConvert.SerializeObject(model));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            } catch (Exception ex) {
                // Log error but continue report run
                LogError(ex);
            }

            // Set report path
            string strName = HttpContext.User.Identity.Name;
            string wwwrootFolder = _hostingEnvironment.WebRootPath + "/template/";
            string filename = @"CC Count.xlsm";
            //string filename = @"CC Count.xlsx";
            var file = new FileInfo(Path.Combine(wwwrootFolder, filename));

            
            string contentType;

            // Define content type based on extension
            switch (file.Extension) {
                case ".xlsm": {
                        //contentType = _contentTypes.ExcelXlsm;
                        contentType = "application/vnd.ms-excel.sheet.macroEnabled.12";
                        break;
                    }
                case ".xlsx": {
                        contentType = _contentTypes.ExcelXlsx;
                        break;
                    }
                default: {
                        throw new Exception("Extension type not found for report file.");
                    }
            }

            try 
            {
                // Get report data
                ResponseFromAPI reportCallBack =  LoadReportFromStoredProcedureAsync(model, guid);
                
                // Generate report, write to TempData session variable
                using (MemoryStream output = GenerateReport(reportCallBack, model, runId, startTime, file)) {                    
                    TempData[guid] = output.ToArray();
                    _logger.LogInformation("Report has completed streaming to browser");
                }
               // Download(guid, filename, contentType,model.ConnectionId.ToString());

                // Return json data for ajax callback
                //  - This information is used in the .OnSuccess callback
                //  - Data provides browser information to call the 'Download'
                //    action with
                return new JsonResult(new {
                    Guid = guid,
                    ContentType = contentType,
                    Filename = filename,
                    ConnectionGuid = model.ConnectionId.ToString()
                });
            } 
            catch (Exception ex)
            {
                LogError(ex);
                throw new Exception(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult Download(string guid, string filename, string contentType, string connectionGuid, bool initialCheck = false)
        {
            try
            {
                _logger.LogInformation($"Associated connection id for this request: {connectionGuid}");
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw new Exception("Error setting connection id. (.Download())");
            }

            if (initialCheck)
            {
                byte[] data = TempData[guid] as byte[];

                if (data == null)
                {
                    throw new Exception("No data found for selected parameters! Please try again. (.Download())");
                }

                TempData[guid] = data;
                return new JsonResult(true);
            }

            // "Download" the report from the TempData session variable
            if (TempData[guid] == null)
            {
                throw new Exception("No data found for selected parameters! Please try again. (.Download())");
            }

            try
            {
                _logger.LogInformation("Downloading file from browser memory to user hard drive...");

                // Excel report stored in TempData[]
                byte[] data = TempData[guid] as byte[];

                // Download the file!
                //return File(data, "ExcelXlsm", filename);
                return File(data, contentType, filename);
            }
            catch(Exception ex) {
                throw new Exception(ex.Message);
                throw new Exception("Error downloading file! (.Download())");
            }
        }

        private MemoryStream GenerateReport(ResponseFromAPI ResponseFromAPI, CCCountUI model, string runId,
                                            DateTime startTime, FileInfo file)
        {
            var startCol = "B";

            try
            {
                using (var memoryStream = new MemoryStream()) {
                    using (ExcelPackage xlPackage = new ExcelPackage(file)) {

                        var wb = xlPackage.Workbook;
                        var wk = xlPackage.Workbook.Worksheets[1];
                        xlPackage.Workbook.Worksheets.Delete(wk);
                        
                        foreach (Worksheet row in ResponseFromAPI.Worksheet) {
                            if (row.WorksheetName != "Tiers" && row.WorksheetName != "DrilldownData")
                            {
                                if (row.WorksheetName != "Hide")
                                {
                                    ExcelWorksheet Worksheet = xlPackage.Workbook.Worksheets.Add(row.WorksheetName);
                                    Worksheet.View.ShowGridLines = false;
                                    Worksheet.View.FreezePanes(8, 3);
                                    ApplyWorksheetStyle(Worksheet, model);
                                    //Hiding drilldown columns
                                    for (var i=0;i<12;i++)
                                    Worksheet.Column(223+i).Hidden = true;
                                    Worksheet.Cells["h2"].Value = row.Notes;
                                    Worksheet.Cells["h2"].Style.Font.Color.SetColor(Color.Red);
                                    Worksheet.Cells["h2"].Style.Font.Size = 10;
                                    // Generate report groupings
                                    //  - Header
                                    //  - Report data
                                    foreach (Groups Group in row.WorksheetContent.Groups)
                                    {
                                        string groupRange = Group.StartingCell;
                                        string groupRangeLines = (Int32.Parse(Group.StartingCell) + 3).ToString();

                                        // Headers
                                        Worksheet.Cells[startCol + groupRange.ToString()].LoadFromDataTable(WorksheetHeadersToDataTable(Group), false);
                                       // Worksheet.Cells["HO" + groupRange.ToString()].Value = "isNonCore	,DrilldownBrand	,DrilldownMarket,	DrilldownChannel,	DrilldownDivision,	DrilldownDepartment,	DrilldownClass,	DrilldownSubclass,	DrilldownAtt1,	DrilldownAtt2,	DrilldownType,	DrilldownExtraCol";

                                        // Data
                                        Worksheet.Cells[startCol + groupRangeLines.ToString()].LoadFromDataTable(WorksheetLinesToDataTable(Group), false);

                                        // Pivot names
                                        //  - Take name from column 8
                                       // Worksheet.Cells[startCol + Convert.ToString(Int32.Parse(groupRangeLines) - 1)].Value = Group.Lines[0].LineString.Split('|')[8].ToString();

                                        ApplyGroupStyle(Group, Worksheet);
                                    }

                                    // Hide last column (currently GN)
                                    //  - TODO: Make it non-hard-coded!
                                    //  Worksheet.Column(196).Hidden = true;
                                    
                                }

                            }
                            else {
                                ExcelWorksheet Worksheet = xlPackage.Workbook.Worksheets[row.WorksheetName];

                                foreach (Groups Group in row.WorksheetContent.Groups)
                                {
                                    string groupRange = Group.StartingCell;
                                    string groupRangeLines = (Int32.Parse(Group.StartingCell) + 3).ToString();

                                    //// Headers
                                    //Worksheet.Cells[startCol + groupRange.ToString()].LoadFromDataTable(WorksheetHeadersToDataTable(Group), false);

                                    // Data
                                    Worksheet.Cells["A2"].LoadFromDataTable(WorksheetLinesToDataTableOriginal(Group), false);

                                }
                            }
                        }

                        // Write selected parameters to named ranges.  Displayed on _RunInformation tab
                        var informationTab = "_RunInformation";
                        try {
                            //  - Ids
                            wb.Names["rngConnectionId"].Value = model.ConnectionId.ToString();
                            wb.Names["rngRunId"].Value = runId;
                            wb.Names["rngVersion"].Value = PlatformServices.Default.Application.ApplicationVersion;

                            //  - Markets
                            //  - TODO: This part might have a bug for the first index
                            var marketsSb = new StringBuilder();
                            //var selectedBrandIndex = Int32.Parse(model.SelectedBrandIndex);
                            //var selectedBrandIndex = 3;
                            //for (var i = 0; i < model.Brands[selectedBrandIndex].Markets.Count; i++) {
                            //    if (model.Brands[selectedBrandIndex].Markets[i].Checked) {
                            //        marketsSb.Append($"{model.Brands[selectedBrandIndex].Markets[i].Value} " +
                            //                         $"({model.Brands[selectedBrandIndex].Markets[i].Name}), ");
                            //    }
                            //}
                            //wb.Names["rngMarkets"].Value = marketsSb.ToString().Substring(0, marketsSb.Length - 1);

                            //  - List selections
                            wb.Names["rngBrands"].Value = model.SelectedBrandId;
                            wb.Names["rngChannels"].Value = model.SelectedChannelId;
                            wb.Names["rngStartingMonth"].Value = $"{model.SelectedStartDateYearId}-{model.SelectedStartDateMonthId.PadLeft(2, '0')}";
                            wb.Names["rngDivisions"].Value = String.Join<string>(", ", model.SelectedDivisionIds);
                            wb.Names["rngDepartments"].Value = String.Join<string>(", ", model.SelectedDepartmentIds);
                            wb.Names["rngClasses"].Value = String.Join<string>(", ", model.SelectedClassIds);

                            wb.Names["rngBrands"].Value = 3;
                            wb.Names["rngChannels"].Value = 1;
                            wb.Names["rngStartingMonth"].Value = $"10";
                            //wb.Names["rngDivisions"].Value = String.Join<string>(", ", model.SelectedDivisionIds);
                            //wb.Names["rngDepartments"].Value = String.Join<string>(", ", model.SelectedDepartmentIds);
                            //wb.Names["rngClasses"].Value = String.Join<string>(", ", model.SelectedClassIds);

                            //var subclassString = "";
                            //subclassString = String.Join<string>(", ", model.SelectedSubClassIds);
                            //int chunkSize = 30000;
                            //int stringLength = subclassString.Length;
                            //int endSubstring;
                            //int x;


                            //for (x = 0; x < Math.Ceiling(stringLength/Convert.ToDouble(chunkSize)); x++)
                            //{
                            //    endSubstring = chunkSize;
                            //    if ((x +1) * chunkSize > stringLength)
                            //    {
                            //        endSubstring = stringLength - (x * chunkSize);
                            //    }
                            //    wb.Names["rngSubclasses"].Offset(0,x).Value = subclassString.Substring(x * chunkSize, (endSubstring) );

                            //}

                            //wb.Names["rngAttribute1"].Value = (model.SelectedAttributeId1 == "-1") ? "N/A" : model.SelectedAttributeId1;
                            //wb.Names["rngAttribute2"].Value = (model.SelectedAttributeId2 == "-1") ? "N/A" : model.SelectedAttributeId2;
                            //wb.Names["rngDrilldown"].Value = model.IncludeDrilldown;
                            wb.Names["rngDrilldown"].Value = 1;
                            //  - Run times
                            var finishTime = DateTime.Now;
                            wb.Names["rngRunStartTime"].Value = $"{startTime.ToShortTimeString()} {TimeZoneInfo.Local.Id}";
                            wb.Names["rngRunFinishTime"].Value = $"{finishTime.ToShortTimeString()} {TimeZoneInfo.Local.Id}";
                            var elapsedTime = finishTime.Subtract(startTime);
                            wb.Names["rngElapsedRunTime"].Value = $"{elapsedTime.Minutes.ToString().PadLeft(2, '0')}:{elapsedTime.Seconds.ToString().PadLeft(2, '0')} (mm:ss)";

                            // Autosize column
                            wb.Worksheets[informationTab].Column(2).AutoFit();

                            // Move to end and hide
                            wb.Worksheets.MoveToEnd(informationTab);
                            wb.Worksheets[informationTab].Hidden = eWorkSheetHidden.Hidden;
                            wb.Worksheets["Tiers"].Hidden = eWorkSheetHidden.Hidden;
                            wb.Worksheets["DrilldownData"].Hidden = eWorkSheetHidden.Hidden;
                        } catch (Exception ex) {
                            _logger.LogWarning($"Issue writing information to named ranges: {ex.Message}");
                        }
                        

                        // Doesn't work in .NET Core since SignedCMS is not implemented in it yet.
                        //  - Must target 4.6 for this functionality.
                        //  - Works fine now in 4.6
                        //  - Seems not necessary if VBA project exists already
                        //if (file.Extension == ".xlsm") {
                        //    xlPackage.Workbook.CreateVBAProject();
                        //}

                        if (ResponseFromAPI.Worksheet.Count == 0)
                            xlPackage.Workbook.Worksheets.Add("Worksheet1");

                        // Save Excel as memory stream
                        xlPackage.SaveAs(memoryStream);

                        // Log task completion
                        _logger.LogInformation("Report has been generated, streaming to browser now...");

                        // Return the report
                        return memoryStream;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw new Exception("Error generating report! Please try again. (.GenerateReport())");
            }    
        }

        static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            var st = s.Split(' ');
            var result = "";
            foreach (string str1 in st) {
                result = result + char.ToUpper(str1[0]) + str1.Substring(1) + " ";
            }

            return result;
        }
        private void ApplyWorksheetStyle(ExcelWorksheet Worksheet, CCCountUI model)
        {
            try 
            {
                string cellAddress1 = "B3:BV3";
                Worksheet.Cells[cellAddress1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                Worksheet.Cells[cellAddress1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 112, 192));
                Worksheet.Cells[cellAddress1].Style.Font.Color.SetColor(Color.White);
                Worksheet.Cells[cellAddress1].Style.Font.Bold = true;
                Worksheet.Cells[cellAddress1].Style.Font.Size = 16;

                // Get Program Count Proxy Name by ID
               // var programCountProxyName = @"{Program Count Proxy Type}";
                var programCountProxyName = "Auto Generated ID";
                //foreach (var programCountProxy in model.ProgramCountProxies) {
                //    if (programCountProxy.Value == model.SelectedProgramCountProxyId) {
                //        programCountProxyName = programCountProxy.Name;
                //        break;
                //    }
                //}

                // Updated to include actual year selection - kc - 2017.5.23
                model.SelectedStartDateMonthId = "10";
                model.SelectedStartDateYearId = "2020";
                model.SelectedBrandName = "ON";
                DateTime myDate = DateTime.Parse((Int32.Parse(model.SelectedStartDateMonthId)).ToString() + "/1/"+ Int32.Parse(model.SelectedStartDateYearId).ToString());

                // Updated header to use variables rather than hard-coded index - kc - 2017.5.23
                var monthsForward = 5;
                Worksheet.Cells["B3"].Value = $"{model.SelectedBrandName} / {myDate.ToString("MMM", CultureInfo.InvariantCulture)} {myDate.Year.ToString()} To {myDate.AddMonths(monthsForward).ToString("MMM", CultureInfo.InvariantCulture)} {myDate.AddMonths(monthsForward).Year.ToString()} / By {programCountProxyName}";
                
                Worksheet.Cells["B2"].Style.Font.Bold = true;
                Worksheet.Cells["B2"].Style.Font.Size = 20;
                Worksheet.Cells["B2"].Value = UppercaseFirst(Worksheet.Name.ToLower())+ "CC Count";// "Omni CC Count Tool";
                Worksheet.Cells["B2"].Style.Font.Color.SetColor(Color.DarkBlue);

                Worksheet.Column(verticalHeaderPosition[0] + 2).Width = 30;
                Worksheet.Column(verticalHeaderPosition[1]+2).Width = 30;
                Worksheet.Column(verticalHeaderPosition[2] +2).Width = 30;
                for (var i = 0; i < HeaderFieldNames.Split(',').Length; i++)
                    if (HeaderFieldNames.Split(',')[i].Contains("Pct") == true)
                        Worksheet.Column(i + 2).Style.Numberformat.Format = "0.00%";
            } 
            catch (Exception ex)
            {
                LogError(ex);
                throw new Exception("Error generating report! Please try again. (.ApplyWorksheetStyle())");
            }
        }

        private void ApplyGroupStyle(Groups Group, ExcelWorksheet Worksheet)
        {
            try 
            {
                // TODO: Put columns into variables?

                if (Group.Lines.Count > 0) {
                    string cellAddress1 = "B" + Group.StartingCell.ToString() + ":BV" + (Int32.Parse(Group.StartingCell) + 1).ToString();
                    string cellAddress2 = "Bx" + Group.StartingCell.ToString() + ":ER" + (Int32.Parse(Group.StartingCell) + 1).ToString();
                    string cellAddress3 = "Et" + Group.StartingCell.ToString() + ":Hn" + (Int32.Parse(Group.StartingCell) + 1).ToString();
                    //Worksheet.Cells[cellAddress].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    Worksheet.Cells[cellAddress1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Worksheet.Cells[cellAddress1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 112, 192));
                    Worksheet.Cells[cellAddress1].Style.Font.Color.SetColor(Color.White);
                    Worksheet.Cells[cellAddress1].Style.Font.Bold = true;
                    Worksheet.Cells[cellAddress1].Style.Font.Size = 16;
                    Worksheet.Cells[cellAddress1].Style.Border.Top.Style = ExcelBorderStyle.Thin;

                    Worksheet.Cells[cellAddress2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Worksheet.Cells[cellAddress2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(146, 208, 80));
                    Worksheet.Cells[cellAddress2].Style.Font.Color.SetColor(Color.White);
                    Worksheet.Cells[cellAddress2].Style.Font.Bold = true;
                    Worksheet.Cells[cellAddress2].Style.Font.Size = 16;
                    Worksheet.Cells[cellAddress2].Style.Border.Top.Style = ExcelBorderStyle.Thin;

                    Worksheet.Cells[cellAddress3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Worksheet.Cells[cellAddress3].Style.Fill.BackgroundColor.SetColor(Color.Black);
                    Worksheet.Cells[cellAddress3].Style.Font.Color.SetColor(Color.White);
                    Worksheet.Cells[cellAddress3].Style.Font.Bold = true;
                    Worksheet.Cells[cellAddress3].Style.Font.Size = 16;
                    Worksheet.Cells[cellAddress3].Style.Border.Top.Style = ExcelBorderStyle.Thin;

                    // adding zebra color to 
                    for (var n = verticalHeaderPosition[0]; n < verticalHeaderPosition[1]; n+=16)
                    {
                         int[] RangeOfZebraColor = { (Int32.Parse(Group.StartingCell) + 3), (n + 3), (Int32.Parse(Group.StartingCell) + 3 + Group.Lines.Count-1), (n + 10) };
                        Worksheet.Cells[RangeOfZebraColor[0], RangeOfZebraColor[1], RangeOfZebraColor[2], RangeOfZebraColor[3]].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        Worksheet.Cells[RangeOfZebraColor[0], RangeOfZebraColor[1], RangeOfZebraColor[2], RangeOfZebraColor[3]].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 236, 251));

                        int[] RangeOfZebraColor1 = { (Int32.Parse(Group.StartingCell) + 3), (n + 3+ verticalHeaderPosition[1]), (Int32.Parse(Group.StartingCell) + 3 + Group.Lines.Count-1), (n + 10+ verticalHeaderPosition[1]) };
                        Worksheet.Cells[RangeOfZebraColor1[0], RangeOfZebraColor1[1], RangeOfZebraColor1[2], RangeOfZebraColor1[3]].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        Worksheet.Cells[RangeOfZebraColor1[0], RangeOfZebraColor1[1], RangeOfZebraColor1[2], RangeOfZebraColor1[3]].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 236, 251));

                        int[] RangeOfZebraColor2 = { (Int32.Parse(Group.StartingCell) + 3), (n + 3+ verticalHeaderPosition[2]), (Int32.Parse(Group.StartingCell) + 3 + Group.Lines.Count-1), (n + 10+verticalHeaderPosition[2]) };
                        Worksheet.Cells[RangeOfZebraColor2[0], RangeOfZebraColor2[1], RangeOfZebraColor2[2], RangeOfZebraColor2[3]].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        Worksheet.Cells[RangeOfZebraColor2[0], RangeOfZebraColor2[1], RangeOfZebraColor2[2], RangeOfZebraColor2[3]].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 236, 251));
                    }

                    cellAddress1 = "B" + (Int32.Parse(Group.StartingCell) + 1).ToString() + ":Bv" + (Int32.Parse(Group.StartingCell) + 2).ToString();
                    cellAddress2 = "Bx" + (Int32.Parse(Group.StartingCell) + 1).ToString() + ":ER" + (Int32.Parse(Group.StartingCell) + 2).ToString();
                    cellAddress3 = "Et" + (Int32.Parse(Group.StartingCell) + 1).ToString() + ":Hn" + (Int32.Parse(Group.StartingCell) + 2).ToString();

                    Worksheet.Cells[cellAddress1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Worksheet.Cells[cellAddress1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(226, 228, 237));
                    Worksheet.Cells[cellAddress1].Style.Font.Color.SetColor(Color.Black);


                    Worksheet.Cells[cellAddress2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Worksheet.Cells[cellAddress2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(226, 228, 237));
                    Worksheet.Cells[cellAddress2].Style.Font.Color.SetColor(Color.Black);


                    Worksheet.Cells[cellAddress3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Worksheet.Cells[cellAddress3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(226, 228, 237));
                    Worksheet.Cells[cellAddress3].Style.Font.Color.SetColor(Color.Black);


                    cellAddress1 = "B" + (Int32.Parse(Group.StartingCell) + 2).ToString() + ":Bv" + (Int32.Parse(Group.StartingCell) + 2).ToString();
                    cellAddress2 = "Bx" + (Int32.Parse(Group.StartingCell) + 2).ToString() + ":ER" + (Int32.Parse(Group.StartingCell) + 2).ToString();
                    cellAddress3 = "Et" + (Int32.Parse(Group.StartingCell) + 2).ToString() + ":Hn" + (Int32.Parse(Group.StartingCell) + 2).ToString();

                    Worksheet.Cells[cellAddress1].Style.WrapText = true;
                    Worksheet.Cells[cellAddress1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    Worksheet.Cells[cellAddress1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    Worksheet.Cells[cellAddress1].Style.Border.Left.Style = ExcelBorderStyle.Thin;

                    Worksheet.Cells[cellAddress2].Style.WrapText = true;
                    Worksheet.Cells[cellAddress2].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    Worksheet.Cells[cellAddress2].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    Worksheet.Cells[cellAddress2].Style.Border.Left.Style = ExcelBorderStyle.Thin;

                    Worksheet.Cells[cellAddress3].Style.WrapText = true;
                    Worksheet.Cells[cellAddress3].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    Worksheet.Cells[cellAddress3].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    Worksheet.Cells[cellAddress3].Style.Border.Left.Style = ExcelBorderStyle.Thin;

                    cellAddress1 = "B" + (Int32.Parse(Group.StartingCell) + 1).ToString() + ":Bv" + (Int32.Parse(Group.StartingCell) + 1).ToString();
                    cellAddress2 = "Bx" + (Int32.Parse(Group.StartingCell) + 1).ToString() + ":ER" + (Int32.Parse(Group.StartingCell) + 1).ToString();
                    cellAddress3 = "Et" + (Int32.Parse(Group.StartingCell) + 1).ToString() + ":Hn" + (Int32.Parse(Group.StartingCell) + 1).ToString();

                    Worksheet.Cells[cellAddress1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Worksheet.Cells[cellAddress1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(99, 140, 173));
                    Worksheet.Cells[cellAddress1].Style.Font.Color.SetColor(Color.White);

                    Worksheet.Cells[cellAddress2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Worksheet.Cells[cellAddress2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(99, 140, 173));
                    Worksheet.Cells[cellAddress2].Style.Font.Color.SetColor(Color.White);

                    Worksheet.Cells[cellAddress3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Worksheet.Cells[cellAddress3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(99, 140, 173));
                    Worksheet.Cells[cellAddress3].Style.Font.Color.SetColor(Color.White);

                    // Header group
                    int i = 0;
                    foreach (WorksheetLines line in Group.Headers) {
                        var Range1 = "B" + (Int32.Parse(Group.StartingCell) + i).ToString();
                        var Range2 = "Bx" + (Int32.Parse(Group.StartingCell) + i).ToString();
                        var Range3 = "Et" + (Int32.Parse(Group.StartingCell) + i).ToString();
                        Worksheet.Cells[Range1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Worksheet.Cells[Range1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Worksheet.Cells[Range2].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Worksheet.Cells[Range2].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Worksheet.Cells[Range3].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Worksheet.Cells[Range3].Style.Border.Left.Style = ExcelBorderStyle.Thin;

                        // Next row
                        i++;
                    }

                    // Pivot
                    Worksheet.Cells["B" + (Int32.Parse(Group.StartingCell) + i - 1).ToString()].Style.Font.Size = 16;
                    Worksheet.Cells["B" + (Int32.Parse(Group.StartingCell) + i - 1).ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Data group
                    i = 0;
                    foreach (WorksheetLines line in Group.Lines) {
                        var Range1 = "B" + (Int32.Parse(Group.StartingCell) + i + Group.Headers.Count).ToString();
                        var Range2 = "Bx" + (Int32.Parse(Group.StartingCell) + i + Group.Headers.Count).ToString();
                        var Range3 = "Et" + (Int32.Parse(Group.StartingCell) + i + Group.Headers.Count).ToString();

                        var Range11 = "B" + (Int32.Parse(Group.StartingCell) + i + Group.Headers.Count).ToString() + ":Bv" + (Int32.Parse(Group.StartingCell) + i + Group.Headers.Count).ToString();
                        var Range12 = "Bx" + (Int32.Parse(Group.StartingCell) + i + Group.Headers.Count).ToString() + ":ER" + (Int32.Parse(Group.StartingCell) + i + Group.Headers.Count).ToString();
                        var Range13 = "Et" + (Int32.Parse(Group.StartingCell) + i + Group.Headers.Count).ToString() + ":Hn" + (Int32.Parse(Group.StartingCell) + i + Group.Headers.Count).ToString();

                        Worksheet.Cells[Range1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Worksheet.Cells[Range1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Worksheet.Cells[Range2].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Worksheet.Cells[Range2].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Worksheet.Cells[Range3].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Worksheet.Cells[Range3].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        if (line.LineString.Split('|')[11].ToUpper().Contains("TIER") == false && line.LineString.Split('|')[11].ToUpper().Contains("ATTRIBUTE") == false) {
                            Worksheet.Cells[Range11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            Worksheet.Cells[Range11].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 112, 192)); // Blue
                            Worksheet.Cells[Range11].Style.Font.Color.SetColor(Color.White);
                            Worksheet.Cells[Range11].Style.Font.Bold = true;


                            Worksheet.Cells[Range12].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            Worksheet.Cells[Range12].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 112, 192));
                            Worksheet.Cells[Range12].Style.Font.Color.SetColor(Color.White);
                            Worksheet.Cells[Range12].Style.Font.Bold = true;


                            Worksheet.Cells[Range13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            Worksheet.Cells[Range13].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 112, 192));
                            Worksheet.Cells[Range13].Style.Font.Color.SetColor(Color.White);
                            Worksheet.Cells[Range13].Style.Font.Bold = true;
                            if (line.LineString.Split('|')[11].ToUpper().Contains("DEPARTMENT") == true) {
                                Worksheet.Cells[Range1].Value = "  " + Worksheet.Cells[Range1].Value;
                                Worksheet.Cells[Range2].Value = "  " + Worksheet.Cells[Range2].Value;
                                Worksheet.Cells[Range3].Value = "  " + Worksheet.Cells[Range3].Value;
                            }
                            if (line.LineString.Split('|')[11].ToUpper().Contains("CLASS") == true) {
                                Worksheet.Cells[Range1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                Worksheet.Cells[Range2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                Worksheet.Cells[Range3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            }
                            if (line.LineString.Split('|')[11].ToUpper().Contains("FLOW") == true) {
                                Worksheet.Cells[Range1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                Worksheet.Cells[Range1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 176, 240)); // Blue Gray
                                Worksheet.Cells[Range1].Style.Font.Color.SetColor(Color.White);
                                Worksheet.Cells[Range1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                Worksheet.Cells[Range2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                Worksheet.Cells[Range2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 176, 240));
                                Worksheet.Cells[Range2].Style.Font.Color.SetColor(Color.White);
                                Worksheet.Cells[Range2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                Worksheet.Cells[Range3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                Worksheet.Cells[Range3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 176, 240));
                                Worksheet.Cells[Range3].Style.Font.Color.SetColor(Color.White);
                                Worksheet.Cells[Range3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                        } else {
                            Worksheet.Cells[Range1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            Worksheet.Cells[Range1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(226, 228, 237)); // light Gray
                            Worksheet.Cells[Range1].Style.Font.Color.SetColor(Color.Black);
                            Worksheet.Cells[Range1].Style.Font.Bold = true;
                            Worksheet.Cells[Range1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                            Worksheet.Cells[Range2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            Worksheet.Cells[Range2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(226, 228, 237));
                            Worksheet.Cells[Range2].Style.Font.Color.SetColor(Color.Black);
                            Worksheet.Cells[Range2].Style.Font.Bold = true;
                            Worksheet.Cells[Range2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                            Worksheet.Cells[Range3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            Worksheet.Cells[Range3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(226, 228, 237));
                            Worksheet.Cells[Range3].Style.Font.Color.SetColor(Color.Black);
                            Worksheet.Cells[Range3].Style.Font.Bold = true;
                            Worksheet.Cells[Range3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            if (line.LineString.Split('|')[11].ToUpper().Contains("ATTRIBUTE") == true) {
                                Worksheet.Cells[Range1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                Worksheet.Cells[Range1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 176, 240));
                                Worksheet.Cells[Range1].Style.Font.Color.SetColor(Color.White);
                                Worksheet.Cells[Range2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                Worksheet.Cells[Range2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 176, 240));
                                Worksheet.Cells[Range2].Style.Font.Color.SetColor(Color.White);
                                Worksheet.Cells[Range3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                Worksheet.Cells[Range3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 176, 240));
                                Worksheet.Cells[Range3].Style.Font.Color.SetColor(Color.White);
                            }
                        }

                        // Next row
                        i++;
                    }

                    // TODO: Do the same for the first 2 sections.  Use left instead of right so after hide it still shows

                    // Get report group right and bottom borders
                    var lastRow = (Int32.Parse(Group.StartingCell) + i - 1 + Group.Headers.Count).ToString();
                    var lastRowRange = $"B{lastRow}:HN{lastRow}";
                    var lastColumnRange = $"HN{(Int32.Parse(Group.StartingCell)).ToString()}:HN{lastRow}";

                    // Apply final borders to right side and bottom
                    Worksheet.Cells[lastRowRange].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    Worksheet.Cells[lastColumnRange].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw new Exception("Error generating report! Please try again. (.ApplyGroupStyle())");
            }
        }

        private ResponseFromAPI LoadReportFromStoredProcedureAsync([FromBody]CCCountUI Hierarchy, string guid) {

            // Log task start
            _logger.LogInformation("Running stored procedure for report data and aggregation...");

            string wwwrootFolder = _hostingEnvironment.WebRootPath + "\\template\\";
            string filename = @"HeaderTitles.txt";

            string HeaderText = System.IO.File.ReadAllText(Path.Combine(wwwrootFolder, filename));

            var data_Report = new List<DataSet>();
            ResponseFromAPI ResponseFromAPI = new ResponseFromAPI();
            ResponseFromAPI.Worksheet = new List<Worksheet>();
            //var markets = Hierarchy.Brands[Int32.Parse(Hierarchy.SelectedBrandIndex.ToString())].Markets;
            int WorkSheetIndex = 0;
            int GroupIndex = 0;
            int isglobalView = 1;
            string marketID = "1";
            string[] ChannelName = { "Omni", "Retail", "Online" };  // TODO: Change so this isn't hard-coded
            string AllMarketIDs = "";
            //string ChannelID = Hierarchy.SelectedChannelId.ToString();
            //var ChannelIDStart = Int32.Parse(Hierarchy.SelectedChannelId.ToString());
            //var ChannelIDEnd = Int32.Parse(Hierarchy.SelectedChannelId.ToString()) + 1;
            Int32 SelectedStartDateMonthId = 11;
            DateTime myDate = DateTime.Parse(SelectedStartDateMonthId.ToString() + "/1/"+ DateTime.Now.Year.ToString());
            for (var i = 1; i < 7; i++)
            {
                var MonthName = myDate.ToString("MMM", CultureInfo.InvariantCulture);
                HeaderText = HeaderText.Replace("M" + (i).ToString(), MonthName);
                myDate = myDate.AddMonths(1);
            }

            //foreach (Market row in Hierarchy.Brands[Int32.Parse(Hierarchy.SelectedBrandIndex.ToString())].Markets)
            //{
            //    if (row.Checked)
            //    {
            //        AllMarketIDs += row.Value + ",";
            //    }
            //}
            //AllMarketIDs = AllMarketIDs.Substring(0, AllMarketIDs.Length - 1);
            //Hierarchy.Brands[Int32.Parse(Hierarchy.SelectedBrandIndex.ToString())].Markets.Insert(0, new Market());
            //Hierarchy.Brands[Int32.Parse(Hierarchy.SelectedBrandIndex.ToString())].Markets[0].Checked = true;
            //Hierarchy.Brands[Int32.Parse(Hierarchy.SelectedBrandIndex.ToString())].Markets[0].Value = "0";
            ////if (Hierarchy.SelectedComparisonCountProxyId.ToString() == "1") {
            //    HeaderText = HeaderText.Replace("LY Plan", "LY Actuals ");
            //}
            var ChannelIndex = 0;
            string strconnectionstring2 = _connectionStrings.SetupData;
            //marketID = Hierarchy.Brands[Int32.Parse(Hierarchy.SelectedBrandIndex.ToString())].Markets[0].Value;           
            if (WorkSheetIndex > 0) { isglobalView = 0; marketID = "1"; }
            else { isglobalView = 1; marketID = AllMarketIDs; }


            data_Report.Add( CallStoredProcedureAsync(guid));

            //var numberOfExtraTables = 2;
            //// if you add more tables after Tier and Drilldown, you should update numberOfExtraTables

            for (WorkSheetIndex=0; WorkSheetIndex < data_Report[0].Tables[0].Rows.Count; WorkSheetIndex++)
            {
                int tableindex = Int32.Parse(data_Report[0].Tables[0].Rows[WorkSheetIndex].ItemArray[1].ToString());
                int numberOfGroups = Int32.Parse(data_Report[0].Tables[0].Rows[WorkSheetIndex].ItemArray[2].ToString());

                string Header = "";
                string Header1 = "";
                string Header2 = "";
                string Header3 = "";

                // This was added after adding extra columns at end (core/noncore)
                // Make sure this number is correct, otherwise you will have bounds
                // issues.  
                

                ResponseFromAPI.ErrorMsg = "No Error";
                var headerCnt = data_Report[0].Tables[tableindex].Columns.Count - (startFieldPosition+1) - numberOfColumnsAfterData;  
                for (int i = startFieldPosition; i < data_Report[0].Tables[tableindex].Columns.Count - numberOfColumnsAfterData; i++)
                {
                    Header += data_Report[0].Tables[tableindex].Columns[i].Caption + ",";
                    Header1 += HeaderText.Split(',')[i] + ",";
                    Header2 += HeaderText.Split(',')[i + headerCnt] + ",";
                    Header3 += HeaderText.Split(',')[i + (headerCnt * 2)] + ",";
                }

                HeaderFieldNames = Header;

                string WorkSheetName = data_Report[0].Tables[0].Rows[WorkSheetIndex].ItemArray[0].ToString();
                string Notes = data_Report[0].Tables[0].Rows[WorkSheetIndex].ItemArray[3].ToString();
                ResponseFromAPI.Worksheet.Add(new Worksheet());
                ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetName = WorkSheetName;
                ResponseFromAPI.Worksheet[WorkSheetIndex].Notes = Notes;
                ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent = new CCCount.Models.WorksheetContent();
                ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.NumberOfGroups = numberOfGroups.ToString();
                ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups = new List<Groups>();

                var GroupStartingRow = 5;

                for (GroupIndex = 0; GroupIndex < numberOfGroups; GroupIndex++)
                {
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups.Add(new Groups());
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Name = "Attribute";
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].StartingCell = GroupStartingRow.ToString();

                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers = new List<WorksheetLines>();


                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers.Add(new WorksheetLines());
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[0].LineString = Header1;
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[0].Range = "B5:HN5";
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[0].Delimiter = ",";
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[0].Comment = "This line is the start point of headers in Template";

                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers.Add(new WorksheetLines());
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[1].LineString = Header2;
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[1].Range = "B6:HN6";
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[1].Delimiter = ",";

                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers.Add(new WorksheetLines());
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[2].LineString = Header3;
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[2].Range = "B7:HN7";
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[2].Delimiter = ",";

                    //ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].


                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines = new List<WorksheetLines>();
                    for (int i = 0; i < data_Report[0].Tables[tableindex + GroupIndex].Rows.Count; i++)
                    {
                        var Line = string.Join("| ", data_Report[0].Tables[tableindex + GroupIndex].Rows[i].ItemArray);
                        ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines.Add(new WorksheetLines());
                        ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines[ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines.Count - 1].LineString = Line;
                        ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines[ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines.Count - 1].Comment = "This is just a sample line comming back from stored procedure";
                        ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines[ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines.Count - 1].Range = "B" + (8 + i).ToString() + ":FY" + (8 + i).ToString();
                        ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines[ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines.Count - 1].Delimiter = ",";

                    }
                    GroupStartingRow += ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines.Count + ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers.Count + 1;
                }
              }
            //*****************************************************************************************************************
            //***************  DrillDown Tiers Start from here ****************************************************************
            //*****************************************************************************************************************
            string[] extraWorksheetNames = {  "DrilldownData" , "Tiers" };
            for (WorkSheetIndex = data_Report[0].Tables[0].Rows.Count; WorkSheetIndex < data_Report[0].Tables[0].Rows.Count+2; WorkSheetIndex++)
            {
                int tableindex = data_Report[0].Tables.Count-(WorkSheetIndex - data_Report[0].Tables[0].Rows.Count)-1;
                int numberOfGroups = 1;

                string Header = "";
                string Header1 = "";
                string Header2 = "";
                string Header3 = "";

                
                ResponseFromAPI.ErrorMsg = "No Error";
                var headerCnt = data_Report[0].Tables[tableindex].Columns.Count - (startFieldPosition + 1) - numberOfColumnsAfterData;
                for (int i = startFieldPosition; i < data_Report[0].Tables[tableindex].Columns.Count - numberOfColumnsAfterData; i++)
                {
                    Header += data_Report[0].Tables[tableindex].Columns[i].Caption + ",";
                    Header1 += HeaderText.Split(',')[i] + ",";
                    Header2 += HeaderText.Split(',')[i + headerCnt] + ",";
                    Header3 += HeaderText.Split(',')[i + (headerCnt * 2)] + ",";
                }

               // HeaderFieldNames = Header;

                string WorkSheetName = extraWorksheetNames[WorkSheetIndex - data_Report[0].Tables[0].Rows.Count];// data_Report[0].Tables[0].Rows[WorkSheetIndex].ItemArray[0].ToString();
                string Notes = " ";
                ResponseFromAPI.Worksheet.Add(new Worksheet());
                ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetName = WorkSheetName;
                ResponseFromAPI.Worksheet[WorkSheetIndex].Notes = Notes;
                ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent = new CCCount.Models.WorksheetContent();
                ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.NumberOfGroups = numberOfGroups.ToString();
                ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups = new List<Groups>();

                var GroupStartingRow = 2;

                for (GroupIndex = 0; GroupIndex < numberOfGroups; GroupIndex++)
                {
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups.Add(new Groups());
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Name = "Attribute";
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].StartingCell = GroupStartingRow.ToString();

                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers = new List<WorksheetLines>();


                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers.Add(new WorksheetLines());
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[0].LineString = Header1;
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[0].Range = "B5:HN5";
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[0].Delimiter = ",";
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[0].Comment = "This line is the start point of headers in Template";

                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers.Add(new WorksheetLines());
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[1].LineString = Header2;
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[1].Range = "B6:HN6";
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[1].Delimiter = ",";

                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers.Add(new WorksheetLines());
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[2].LineString = Header3;
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[2].Range = "B7:HN7";
                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers[2].Delimiter = ",";

                    //ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].


                    ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines = new List<WorksheetLines>();
                    for (int i = 0; i < data_Report[0].Tables[tableindex + GroupIndex].Rows.Count; i++)
                    {
                        var Line = string.Join("| ", data_Report[0].Tables[tableindex + GroupIndex].Rows[i].ItemArray);
                        ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines.Add(new WorksheetLines());
                        ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines[ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines.Count - 1].LineString = Line;
                        ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines[ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines.Count - 1].Comment = "This is just a sample line comming back from stored procedure";
                        ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines[ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines.Count - 1].Range = "B" + (8 + i).ToString() + ":HN" + (8 + i).ToString();
                        ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines[ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines.Count - 1].Delimiter = ",";

                    }
                    GroupStartingRow += ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Lines.Count + ResponseFromAPI.Worksheet[WorkSheetIndex].WorksheetContent.Groups[GroupIndex].Headers.Count + 1;
                }
            }

            // Log task completion
            _logger.LogInformation("Report data and aggregation completed. Formatting report...");

            return ResponseFromAPI;
        }

        private DataSet CallStoredProcedureAsync(string guid)
        {

            DataSet DataReport = new DataSet(); 
            string strconnectionstring2 = _connectionStrings.SetupData;           
            String _reportGUIDAsString = guid;
           // Hierarchy.IncludeDrilldown = "1";
            string StringTables = _connectionStrings.ReportSPName;
            string UpdateCmd3 = ("exec " + StringTables);
              //  ",@LyActuals='" + Hierarchy.SelectedComparisonCountProxyId.ToString() + "'" +
            //Above line needs to be added for activating LYActual selection (anywhere)
            try
            {
                // Log stored procedure call
                _logger.LogInformation($"Report stored procedure call Start: {UpdateCmd3}");

                string connString = strconnectionstring2.Replace("Provider=SQLOLEDB;", "")+ ";Connection Timeout=93;";
                string query = UpdateCmd3;

                SqlConnection conn = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader;
                cmd.CommandTimeout = 5600;
                conn.Open();
                // create data adapter
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                cmd.Connection = conn;
                // I create a reader and convert it to Dataset
                reader = cmd.ExecuteReader();
                while (!reader.IsClosed)
                    DataReport.Tables.Add().Load(reader);
                conn.Close();
                da.Dispose();
                _logger.LogInformation($"Report stored procedure call End: {UpdateCmd3}");
            } catch (Exception ex) {
                LogError(ex);
                throw new Exception("Error in SQL Server during report execution. Please try again. (.CallStoredProcedureAsync())");
            }
            
            return DataReport;
        }

        private DataTable WorksheetHeadersToDataTable(Groups Group) {

            var dt = new DataTable();
            if (Group.Lines.Count > 0) { 
                var ColCnt = Group.Lines[0].LineString.ToString().Split('|').Length- (startFieldPosition ) - numberOfColumnsAfterData;
                for (var i = 0; i < ColCnt; i++) {
                    dt.Columns.Add(i.ToString() );
                }
                foreach (WorksheetLines line in Group.Headers)
                {
                    var row = dt.NewRow();
                    for (var i = 0; i < ColCnt; i++)
                    {
                        row[i.ToString()] = line.LineString.Split(',')[i];
                    }
                    dt.Rows.Add(row);
                }

            }
            return dt;
        }

        private DataTable WorksheetLinesToDataTable(Groups Group)
        {
            // create a data table
            var dt = new DataTable();
            // check if there is any line in group
            if (Group.Lines.Count > 0)
            {
                // calculating number of columns
                var ColCnt = Group.Lines[0].LineString.ToString().Split('|').Length - (startFieldPosition ) ;
                // creating data table's columns
                for (var i = 0; i < ColCnt ; i++)
                {
                    // check if the column is in vertical header position set to string else set to decimal
                    if (i==verticalHeaderPosition[0] || i== verticalHeaderPosition[1] || i == verticalHeaderPosition[2] || i == (verticalHeaderPosition[1]-1) || i == (verticalHeaderPosition[2] - 1) || i > (ColCnt - numberOfColumnsAfterData))
                        dt.Columns.Add(i.ToString());
                    else
                        dt.Columns.Add(i.ToString(), typeof(Decimal));
                }

                foreach (WorksheetLines line in Group.Lines)
                {
                    var row = dt.NewRow();
                    //if (line.LineString.Split('|')[10].ToString().ToUpper().Contains("TIER") == true)
                    //{ 
                    for (var i = 0; i < ColCnt; i++)
                    { // The numbers needs to be changed to dynamic
                        if (i == verticalHeaderPosition[0] || i == verticalHeaderPosition[1] || i == verticalHeaderPosition[2] || i == (verticalHeaderPosition[1] - 1) || i == (verticalHeaderPosition[2] - 1) || i > (ColCnt - numberOfColumnsAfterData))
                            row[i.ToString()] = line.LineString.Split('|')[i + startFieldPosition];
                        //else
                        //{
                        //    if (i != 6)
                        //    {
                        //        try
                        //        {
                        //            row[i.ToString()] = Decimal.Parse((line.LineString.Split('|')[i + startFieldPosition]) == " " ? "0" : line.LineString.Split('|')[i + startFieldPosition]);
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            continue;
                        //        }


                        //    }
                        //}
                    }
                    //}
                    //else
                    //{
                    //    var i = verticalHeaderPosition[0];
                    //    row[i.ToString()] = line.LineString.Split('|')[i + startFieldPosition];
                    //     i = verticalHeaderPosition[1];
                    //    row[i.ToString()] = line.LineString.Split('|')[i + startFieldPosition];
                    //     i = verticalHeaderPosition[2];
                    //    row[i.ToString()] = line.LineString.Split('|')[i + startFieldPosition];

                    //}
                        dt.Rows.Add(row);
                    }
                }
            return dt;
        }
        private DataTable WorksheetLinesToDataTableOriginal(Groups Group)
        {
            // create a data table
            var dt = new DataTable();
            // check if there is any line in group
            if (Group.Lines.Count > 0)
            {
                // calculating number of columns
                var ColCnt = Group.Lines[0].LineString.ToString().Split('|').Length -1;
                foreach (WorksheetLines line in Group.Lines)
                {
                    if (ColCnt < line.LineString.Split('|').Length) {
                        ColCnt = line.LineString.Split('|').Length;
                    }
                }
                    // creating data table's columns
                    for (var i = 0; i < ColCnt; i++)
                {
                    // check if the column is in vertical header position set to string else set to decimal
               //     if (i == verticalHeaderPosition[0] || i == verticalHeaderPosition[1] || i == verticalHeaderPosition[2] || i == (verticalHeaderPosition[1] - 1) || i == (verticalHeaderPosition[2] - 1))
                        dt.Columns.Add(i.ToString());
               //     else
              //          dt.Columns.Add(i.ToString(), typeof(Decimal));
                }

                foreach (WorksheetLines line in Group.Lines)
                {
                    var row = dt.NewRow();
                    //if (line.LineString.Split('|')[10].ToString().ToUpper().Contains("TIER") == true)
                    //{ 
                    for (var i = 0; i < line.LineString.Split('|').Length; i++)
                    { // The numbers needs to be changed to dynamic
                   //     if (i == verticalHeaderPosition[0] || i == verticalHeaderPosition[1] || i == verticalHeaderPosition[2] || i == (verticalHeaderPosition[1] - 1) || i == (verticalHeaderPosition[2] - 1))
                            row[i.ToString()] = line.LineString.Split('|')[i ];
                    //    else
                   //     {
                            row[i.ToString()] = line.LineString.Split('|')[i];

                           // row[i.ToString()] = Decimal.Parse((line.LineString.Split('|')[i ]) == " " ? "0" : line.LineString.Split('|')[i ]);
                    //    }
                    }
                    //}
                    //else
                    //{
                    //    var i = verticalHeaderPosition[0];
                    //    row[i.ToString()] = line.LineString.Split('|')[i + startFieldPosition];
                    //     i = verticalHeaderPosition[1];
                    //    row[i.ToString()] = line.LineString.Split('|')[i + startFieldPosition];
                    //     i = verticalHeaderPosition[2];
                    //    row[i.ToString()] = line.LineString.Split('|')[i + startFieldPosition];

                    //}
                    dt.Rows.Add(row);
                }
            }
            return dt;
        }
        public IActionResult Error()
        {
            return View();
        }

        [HttpGet]
        public string ClientInfo()
        {
            return Request.Headers["User-Agent"].ToString();
        }

        [HttpGet]
        public string Test()
        {
            var longGuid = Guid.NewGuid().ToString();
            var shortGuid = CCCountFunctions.GetShortGuid();

            //using (var conn = new OleDbConnection(_connectionStrings.SetupData)) {

            //    var spName = "dbo.usp_LogRunInformation";

            //    var cmd = new OleDbCommand(spName, conn);
            //    cmd.CommandType = CommandType.StoredProcedure;

            //    cmd.Parameters.AddWithValue("@connectionId", longGuid);
            //    cmd.Parameters.AddWithValue("@runId", shortGuid);
            //    cmd.Parameters.AddWithValue("@model", JsonConvert.SerializeObject(new SampleCCCountUI()));

            //    conn.Open();
            //    cmd.ExecuteNonQuery();
            //    conn.Close();
            //}

            return $@"Short Guid: {shortGuid}" +
                   $"{Environment.NewLine}" +
                   $"Guid: {longGuid}";
        }

        private void LogError(Exception ex)
        {
            _logger.LogError($"Error in {ex.TargetSite.Name}!  Message: {ex.Message}, StackTrace: {ex.StackTrace}");
        }
    }
}
