//------------------------------------------------------------------------------
//----- FilesController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Display a master Files page and link to individual files pages 
//
//discussion:   
//
//     
#region Comments
//12.08.12 - TR - added SensorDataFileModel ViewModel for Sensor files
//12.07.12 - TR - Created partial views for edit/details
//12.06.12 - TR - Created SensorFile create/edit/delete/info box
//12.05.12 - TR - Created Info Box for SiteFile
//10.24.12 - TR - Delete working
//10.19.12 - TR - Created

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.IO;

using OpenRasta.IO;
using RestSharp;
using STNServices;
using STNServices.Authentication;
using STNServices.Resources;
using STNWeb.Utilities;
using STNWeb.Models;
using STNWeb.Helpers;

namespace STNWeb.Controllers
{   
    //[Authorize]
    public class FilesController : Controller
    {
      
        #region InfoBoxes (on SiteDetails, HWMDetails, and SensorDetails pages)

        [Authorize]
        // GET: SiteDetail/SiteFileInfoBox/
        public PartialViewResult SiteFileInfoBox(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "Sites/{siteId}/Files";
                request.RootElement = "ArrayOfFile";
                request.AddParameter("siteId", id, ParameterType.UrlSegment);
                List<FILE> fileList = serviceCaller.Execute<List<FILE>>(request);

                //separate the file types for display in the infobox
                List<FILE> photoFiles = fileList.Where(f => f.FILETYPE_ID == 1).ToList();
                if (photoFiles.Count >= 1)
                { 
                    ViewData["PhotoFiles"] = photoFiles; 
                    //now get all the images to show in the scroller
                    //List<FileStreamResult> allPhotos = new List<FileStreamResult>();
                    //foreach (FILE pf in photoFiles)
                    //{
                    //    request = new RestRequest();
                    //    request.Resource = "/FILES/{fileId}/Item";
                    //    request.AddParameter("fileId", pf.FILE_ID, ParameterType.UrlSegment);
                    //    FileStreamResult result = serviceCaller.ExecuteDownload(request);
                    //    allPhotos.Add(result);
                    //}
                    //ViewData["AllPhotoStreams"] = allPhotos;
                    
                }

                
                List<FILE> dataFiles = fileList.Where(f => f.FILETYPE_ID == 2).ToList();
                if (dataFiles.Count >= 1)
                { ViewData["DataFiles"] = dataFiles; }

                List<FILE> AllOtherfiles = fileList.Where(f => (f.FILETYPE_ID >= 3)).ToList();
                if (AllOtherfiles.Count >= 1)
                { ViewData["AllOtherFiles"] = AllOtherfiles; }

                //store Site Id
                ViewData["SiteId"] = id;

                return PartialView("FilesInfoboxes/SiteFileInfoBox");
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        [Authorize]
        public PartialViewResult SiteFileInfoBox4Ev(int evId, int siteId)
        {
            try
            {
                //returns the HWMs for this site and this event
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/Files?Site={siteId}&Event={eventId}";
                request.RootElement = "ArrayOfFILE";
                request.AddParameter("siteId", siteId, ParameterType.UrlSegment);
                request.AddParameter("eventId", evId, ParameterType.UrlSegment);
                List<FILE> SiteEvFiles = serviceCaller.Execute<List<FILE>>(request);

                if (SiteEvFiles.Count >= 1 && SiteEvFiles != null)
                {
                    ViewData["SiteEvFiles"] = SiteEvFiles;

                    //separate the file types for display in the infobox
                    List<FILE> photoFiles = SiteEvFiles.Where(f => f.FILETYPE_ID == 1).ToList();
                    if (photoFiles.Count >= 1)
                    { ViewData["PhotoFiles"] = photoFiles; }

                    List<FILE> dataFiles = SiteEvFiles.Where(f => f.FILETYPE_ID == 2).ToList();
                    if (dataFiles.Count >= 1)
                    { ViewData["DataFiles"] = dataFiles; }

                    List<FILE> AllOtherfiles = SiteEvFiles.Where(f => (f.FILETYPE_ID >= 3)).ToList();
                    if (AllOtherfiles.Count >= 1)
                    { ViewData["AllOtherFiles"] = AllOtherfiles; }
                }

                return PartialView("FilesInfoboxes/SiteFileInfoBox4Ev");
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        [Authorize]
        //GET: InstrumentDetail/SensorFileInfoBox
        public PartialViewResult SensorFileInfoBox(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "Instruments/{InstId}/Files";
                request.RootElement = "ArrayOfFile";
                request.AddParameter("InstId", id, ParameterType.UrlSegment);
                List<FILE> dfList = serviceCaller.Execute<List<FILE>>(request);
                ViewData["dataFileList"] = dfList;

                //separate the file types for display in the infobox
                List<FILE> photoFiles = dfList.Where(f => f.FILETYPE_ID == 1).ToList();
                if (photoFiles.Count >= 1)
                { ViewData["PhotoFiles"] = photoFiles; }

                List<FILE> dataFiles = dfList.Where(f => f.FILETYPE_ID == 2).ToList();
                if (dataFiles.Count >= 1)
                { ViewData["DataFiles"] = dataFiles; }

                List<FILE> AllOtherfiles = dfList.Where(f => (f.FILETYPE_ID >= 3)).ToList();
                if (AllOtherfiles.Count >= 1)
                { ViewData["AllOtherFiles"] = AllOtherfiles; }

                //store Site Id
                request = new RestRequest();
                request.Resource = "Instruments/{entityId}";
                request.RootElement = "INSTRUMENT";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                ViewData["SiteID"] = serviceCaller.Execute<INSTRUMENT>(request).SITE_ID;
                ViewData["SensorId"] = id;

                return PartialView("FilesInfoboxes/SensorFileInfoBox");
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        [Authorize]
        //GET: HWMDetail/HWMFileInfoBox
        public PartialViewResult HWMFileInfoBox(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "HWMs/{HWMId}/Files";
                request.RootElement = "ArrayOfFile";
                request.AddParameter("HWMId", id, ParameterType.UrlSegment);
                List<FILE> fileList = serviceCaller.Execute<List<FILE>>(request);
                ViewData["fileList"] = fileList;

                //separate the file types for display in the infobox
                List<FILE> photoFiles = fileList.Where(f => f.FILETYPE_ID == 1).ToList();
                if (photoFiles.Count >= 1)
                { ViewData["PhotoFiles"] = photoFiles; }

                //List<FILE> dataFiles = fileList.Where(f => f.FILETYPE_ID == 2).ToList();
                //if (dataFiles.Count >= 1)
                //{ ViewData["DataFiles"] = dataFiles; }

                List<FILE> AllOtherfiles = fileList.Where(f => (f.FILETYPE_ID >= 3)).ToList();
                if (AllOtherfiles.Count >= 1)
                { ViewData["AllOtherFiles"] = AllOtherfiles; }

                //store Site Id
                request = new RestRequest();
                request.Resource = "HWMs/{entityId}";
                request.RootElement = "HWM";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                ViewData["SiteID"] = serviceCaller.Execute<HWM>(request).SITE_ID;
                ViewData["HWMID"] = id;

                return PartialView("FilesInfoboxes/HWMFileInfoBox");
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        [Authorize]
        //GET: OPDetail/OPFileInfoBox
        public PartialViewResult OPFileInfoBox(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "ObjectivePoints/{objectivePointId}/Files";
                request.RootElement = "ArrayOfFile";
                request.AddParameter("objectivePointId", id, ParameterType.UrlSegment);
                List<FILE> fileList = serviceCaller.Execute<List<FILE>>(request);
                ViewData["fileList"] = fileList;

                //separate the file types for display in the infobox
                List<FILE> photoFiles = fileList.Where(f => f.FILETYPE_ID == 1).ToList();
                if (photoFiles.Count >= 1)
                { ViewData["PhotoFiles"] = photoFiles; }

                List<FILE> AllOtherfiles = fileList.Where(f => (f.FILETYPE_ID >= 3)).ToList();
                if (AllOtherfiles.Count >= 1)
                { ViewData["AllOtherFiles"] = AllOtherfiles; }

                //store Site Id
                request = new RestRequest();
                request.Resource = "/ObjectivePoints/{entityId}";
                request.RootElement = "OBJECTIVE_POINT";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                ViewData["SiteID"] = serviceCaller.Execute<OBJECTIVE_POINT>(request).SITE_ID;
                ViewData["OP_ID"] = id;

                return PartialView("FilesInfoboxes/OPFileInfoBox");
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }
        #endregion InfoBoxes


        #region Creates

        // containier Create page with PartialView depending on FileType uploading
        //GET: /FileCreate
        [Authorize]
        public ActionResult FileCreate(int Id, string aPage)
        {
            try
            {
                //get FileTypes, Sources for dropdowns
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request = new RestRequest();
                request.Resource = "/FileTypes/";
                request.RootElement = "ArrayOfFILE_TYPE";
                List<FILE_TYPE> FileTypeList = serviceCaller.Execute<List<FILE_TYPE>>(request);

                //go get pageFileTYpeList, sending FileTypeList & aPage, populates ViewData["FileTypeList"]
                GetFileTypeList(FileTypeList, aPage);

                //get this member logged in
                ViewData["aMem"] = GetMember(User.Identity.Name);

                //store typeID passed in. If HWM file, add hwmID. if site file, add siteId if sensor file. add sensorIf
                ViewData["page"] = aPage; //if this page is HWM=know to store this id as HWM_ID
                ViewData["Id"] = Id;
                //if creating from sensor, need to store the SITE_ID too for FILE
                if (aPage == "Sensor")
                {
                    request = new RestRequest();
                    request.Resource = "Instruments/{entityId}";
                    request.RootElement = "INSTRUMENT";
                    request.AddParameter("entityId", Id, ParameterType.UrlSegment);
                    decimal? SensorSiteId = serviceCaller.Execute<INSTRUMENT>(request).SITE_ID;
                    ViewData["sensSiteID"] = SensorSiteId;

                    request = new RestRequest();
                    request.Resource = "/Sites/{entityId}";
                    request.RootElement = "SITE";
                    request.AddParameter("entityId", SensorSiteId, ParameterType.UrlSegment);
                    ViewData["aSite"] = serviceCaller.Execute<SITE>(request);
                    ViewBag.Site = SensorSiteId;
                }
                if (aPage == "SITE")
                {
                    request = new RestRequest();
                    request.Resource = "/Sites/{entityId}";
                    request.RootElement = "SITE";
                    request.AddParameter("entityId", Id, ParameterType.UrlSegment);
                    ViewData["aSite"] = serviceCaller.Execute<SITE>(request);
                    ViewBag.Site = Id;
                }
                if (aPage == "HWM")
                {
                    request = new RestRequest();
                    request.Resource = "HWMs/{entityId}";
                    request.RootElement = "HWM";
                    request.AddParameter("entityId", Id, ParameterType.UrlSegment);
                    HWM thisHWM = serviceCaller.Execute<HWM>(request);
                    ViewData["HWM"] = thisHWM;

                    request = new RestRequest();
                    request.Resource = "/Sites/{entityId}";
                    request.RootElement = "SITE";
                    request.AddParameter("entityId", thisHWM.SITE_ID, ParameterType.UrlSegment);
                    ViewData["aSite"] = serviceCaller.Execute<SITE>(request);
                    ViewBag.Site = thisHWM.SITE_ID;
                }
                if (aPage == "OP")
                {
                    request = new RestRequest();
                    request.Resource = "ObjectivePoints/{entityId}";
                    request.RootElement = "OBJECTIVE_POINT";
                    request.AddParameter("entityId", Id, ParameterType.UrlSegment);
                    OBJECTIVE_POINT thisOP = serviceCaller.Execute<OBJECTIVE_POINT>(request);
                    ViewData["OP"] = thisOP;

                    request = new RestRequest();
                    request.Resource = "/Sites/{entityId}";
                    request.RootElement = "SITE";
                    request.AddParameter("entityId", thisOP.SITE_ID, ParameterType.UrlSegment);
                    ViewData["aSite"] = serviceCaller.Execute<SITE>(request);
                    ViewData["siteID"] = thisOP.SITE_ID;
                }
                return View("FileCreate");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //return the string for Preview Caption click from a Photo upload (create) page
        [Authorize]
        public string CaptionCreator(string filedesc, string memberName, string memberAgency, DateTime dateTaken, int siteId)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "Sites/{entityId}";
                request.RootElement = "SITE";
                request.AddParameter("entityId", siteId, ParameterType.UrlSegment);
                SITE thisSite = serviceCaller.Execute<SITE>(request);

                //      "Photo of {$("#FileDesc").val()} at {SiteDescription}, {County}, {State}, {Date}. Photograph by {@aMember.FNAME @aMember.LNAME}, {@memberAgency}.

                string PhotoCaption = "Photo of " + filedesc + ", at " + thisSite.SITE_DESCRIPTION + ", " + thisSite.COUNTY + ", " + thisSite.STATE + ", " + dateTaken.ToShortDateString() + ". " + memberName + ", " + memberAgency;
                return PhotoCaption;
            }
            catch
            {
                return "Error in generating caption";
            }
        }
        
        //method called from <script> to pull Partial View related to chosen filetype
        // GET: /FileTypePages/GetFileTypePV/
        [Authorize]
        public PartialViewResult GetFileTypePV(int id, int siteID)
        {
            try
            {
                //return the partial view that is needed based on FileType chosen. id = filetype_id
               
                //get the member and member's agency
                MEMBER aMember = GetMember(User.Identity.Name);

                //store the siteID for PhotoFIle caption creator
                ViewData["SiteID"] = siteID;
                ViewData["memberAgID"] = aMember.AGENCY_ID;

                //if id == 1 (photo)
                if (id.Equals(1))
                {
                    ViewData["Member"] = aMember;
                    if (aMember.AGENCY_ID != null)
                    {
                        ViewData["MemberAgency"] = GetAgencyName(aMember.AGENCY_ID);
                    }

                    ViewData["SourceAgency"] = GetAgencyList();

                    return PartialView("FileTypePages/Create/PhotoCreatePV");
                }
                //if id == 2 (data)
                else if (id.Equals(2))
                {
                    //get time zones
                    System.Collections.ObjectModel.ReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();
                    List<string> USAtimeZones = new List<string>();
                    foreach (TimeZoneInfo tzI in timeZones)
                    {
                        if (tzI.DisplayName.Contains("US "))
                        {
                            if (tzI.StandardName.Contains("Pacific"))
                            { USAtimeZones.Add("PST"); }
                            else if (tzI.StandardName.Contains("Mountain"))
                            { USAtimeZones.Add("MST"); }
                            else if (tzI.StandardName.Contains("Central"))
                            { USAtimeZones.Add("CST"); }
                            else
                            { USAtimeZones.Add("EST"); }
                        }
                    }
                    USAtimeZones.Insert(0, "UTC");
                    ViewData["TimeZones"] = USAtimeZones;
                    return PartialView("FileTypePages/Create/DataCreatePV");
                }
                //if id == any thing else (allOther) 3, 4, 5, 6, 7, 8
                else if (id >= 3)
                {
                    ViewData["Member"] = aMember;

                    List<AGENCY> agencyList = GetAgencyList();
                    ViewData["SourceAgency"] = agencyList;

                    return PartialView("FileTypePages/Create/AllOtherCreatePV");
                }
                else
                {
                    return PartialView();
                }
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }
        
        //download the file
        public FileStreamResult DownloadFile(int id)
        {
            try
            {
                //get the fileItem
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/FILES/{fileId}/Item";
                request.AddParameter("fileId", id, ParameterType.UrlSegment);
                FileStreamResult result = serviceCaller.ExecuteDownload(request);
                if (result == null)
                {
                    string path = HttpContext.Server.MapPath("~/Assets/Images/questionMarkSm.png");
                    result = new FileStreamResult(new FileStream(path, FileMode.Open), "image/png");
                }
                return result;
            }
            catch (Exception e)
            {
                FileStreamResult fs = null;
                return fs;
            }
        }


        [Authorize]
        //POST: /FileCreate
        [HttpPost]
        public ActionResult Create(FormCollection thisFormObject, HttpPostedFileBase fileUpload, string Create)
        {
            try
            {
                MemoryStream target = new MemoryStream();
                fileUpload.InputStream.CopyTo(target);
                byte[] data = target.ToArray();
                string fileName = Path.GetFileName(fileUpload.FileName);

                string fileType = string.Empty; //store filetype to determine which viewModel to use
                string fromPage = string.Empty; //store fromPage to determine which page to go to after post
                foreach (var key in thisFormObject.AllKeys)
                {
                    if (key == "FILETYPE_ID")
                    { fileType = thisFormObject[key]; }
                    if (key == "fromPage")
                    { fromPage = thisFormObject[key]; }
                }

                #region POST TO FileType=Data

                if (fileType == "2") //Data file
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest(Method.POST);
                    STNWebSerializer serializer = new STNWebSerializer();

                    List<object> DataFileObj = GetDataFile(fileType, thisFormObject);
                   
                    FILE thisFile = (FILE)DataFileObj[0];
                    DATA_FILE thisDFile = (DATA_FILE)DataFileObj[1];

                    request = new RestRequest(Method.POST);
                    request.Resource = "DataFiles/";
                    request.RequestFormat = DataFormat.Xml;
                    request.AddHeader("Content-Type", "application/xml");
                    serializer = new STNWebSerializer();
                    request.AddParameter("application/xml", serializer.Serialize<DATA_FILE>(thisDFile), ParameterType.RequestBody);
                    DATA_FILE createdDF = serviceCaller.Execute<DATA_FILE>(request);

                    //creates a MultipartHTTP request
                    thisFile.DATA_FILE_ID = createdDF.DATA_FILE_ID;
                    request = new RestRequest(Method.POST);
                    request.Resource = "FILES/bytes";
                    request.RequestFormat = DataFormat.Xml;
                    serializer = new STNWebSerializer();
                    //name of FILEEntity must be FileEntity
                    request.AddParameter("FileEntity", serializer.Serialize<FILE>(thisFile), ParameterType.GetOrPost);
                    //name of the file must be specified as File
                    request.AddFile("File", data, fileName);
                    FILE createdFile = serviceCaller.Execute<FILE>(request);

                    //determine which button was clicked
                    if (Create == "Submit & Add Peak\r\n Summary (Data File)") //Submit & Add HWM
                    {
                        return RedirectToAction("CreatePeakSumForm", "PeakSummary", new { id = createdFile.FILE_ID, FROM = "File" });
                    }
                    else //Submit
                    {
                        return RedirectToAction("Details", "Instruments", new { id = createdFile.INSTRUMENT_ID });
                    }
                }

                #endregion POST to fileType=Data

                #region POST TO FileType=Photo

                else if (fileType == "1") //photo (should be multipartHttpEntity)
                {
                    List<object> SourceFileObj = GetPhotoFile(fileType, thisFormObject);
                    FILE aFile = (FILE)SourceFileObj[0];
                    SOURCE aSource = (SOURCE)SourceFileObj[1];

                    //need to post to SOURCE first to get the sourceID into FILES property
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest(Method.POST);

                    request.Resource = "/Sources";
                    request.RequestFormat = DataFormat.Xml;
                    request.AddHeader("Content-Type", "application/xml");
                    STNWebSerializer serializer = new STNWebSerializer();
                    request.AddParameter("application/xml", serializer.Serialize<SOURCE>(aSource), ParameterType.RequestBody);
                    SOURCE createdSource = serviceCaller.Execute<SOURCE>(request);

                    //store sourceid
                    aFile.SOURCE_ID = createdSource.SOURCE_ID;
                    request = new RestRequest(Method.POST);
                    request.Resource = "FILES/bytes";
                    request.RequestFormat = DataFormat.Xml;
                    serializer = new STNWebSerializer();
                    //name of FILEEntity must be FileEntity
                    request.AddParameter("FileEntity", serializer.Serialize<FILE>(aFile), ParameterType.GetOrPost);
                    //name of the file must be specified as File
                    request.AddFile("File", data, fileName);
                    FILE createdFile = serviceCaller.Execute<FILE>(request);

                    if (fromPage == "SITE")
                    { return RedirectToAction("Details", "Sites", new { id = createdFile.SITE_ID }); }
                    else if (fromPage == "HWM")
                    { return RedirectToAction("Details", "HWMs", new { id = createdFile.HWM_ID }); }
                    else if (fromPage == "Sensor")
                    { return RedirectToAction("Details", "Instruments", new { id = createdFile.INSTRUMENT_ID }); }
                    else
                    { return RedirectToAction("ObjPointDetails", "ObjPoints", new { id = createdFile.OBJECTIVE_POINT_ID }); }
                }
                #endregion POST TO FileType=Photo

                #region POST TO FileType=All others

                else //all others
                {
                    List<object> AllElseFileObj = GetAllElseFile(fileType, thisFormObject);
                    FILE aFile = (FILE)AllElseFileObj[0];
                    SOURCE aSource = (SOURCE)AllElseFileObj[1];

                    //need to post to SOURCE first to get the sourceID into FILES property
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest(Method.POST);

                    request.Resource = "/Sources";
                    request.RequestFormat = DataFormat.Xml;
                    request.AddHeader("Content-Type", "application/xml");
                    STNWebSerializer serializer = new STNWebSerializer();
                    request.AddParameter("application/xml", serializer.Serialize<SOURCE>(aSource), ParameterType.RequestBody);
                    SOURCE createdSource = serviceCaller.Execute<SOURCE>(request);

                    //store sourceid
                    aFile.SOURCE_ID = createdSource.SOURCE_ID;
                    request = new RestRequest(Method.POST);
                    request.Resource = "FILES/bytes";
                    request.RequestFormat = DataFormat.Xml;
                    serializer = new STNWebSerializer();
                    //name of FILEEntity must be FileEntity
                    request.AddParameter("FileEntity", serializer.Serialize<FILE>(aFile), ParameterType.GetOrPost);
                    //name of the file must be specified as File
                    request.AddFile("File", data, fileName);
                    FILE createdFile = serviceCaller.Execute<FILE>(request);

                    if (fromPage == "SITE")
                    {
                        return RedirectToAction("Details", "Sites", new { id = createdFile.SITE_ID });
                    }
                    else if (fromPage == "HWM")
                    {
                        return RedirectToAction("Details", "HWMs", new { id = createdFile.HWM_ID });
                    }
                    else if (fromPage == "Sensor")
                    {
                        return RedirectToAction("Details", "Instruments", new { id = createdFile.INSTRUMENT_ID });
                    }

                    else
                    { return RedirectToAction("ObjPointDetails", "ObjPoints", new { id = createdFile.OBJECTIVE_POINT_ID }); }
                }
                #endregion POST TO FileType=All others
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //called from FileCreate() get pageFileTYpeList, sent FileTypeList & aPage,
        //populates ViewData["FileTypeList"]
        [Authorize]
        public void GetFileTypeList(List<FILE_TYPE> FileTypeList, string aPage)
        {
            try
            {
                List<FILE_TYPE> pageFileTypeList = new List<FILE_TYPE>();
                if (aPage == "HWM")
                {
                    //Photo (1), Historic (3), Field Sheets (4), Level Notes (5), Other (7), Link (8), Sketch (10)
                    pageFileTypeList = FileTypeList.Where(x => x.FILETYPE_ID == 1 || x.FILETYPE_ID == 3 || x.FILETYPE_ID == 4 ||
                        x.FILETYPE_ID == 5 || x.FILETYPE_ID == 7 || x.FILETYPE_ID == 8 || x.FILETYPE_ID == 10).ToList();

                    ViewData["FileTypeList"] = pageFileTypeList;
                }
                if (aPage == "Sensor")
                {
                    //Photo (1), Data (2), Historic (3), Field Sheets (4), Level Notes (5), Other (7), Link (8), Sketch (10)
                    pageFileTypeList = FileTypeList.Where(x => x.FILETYPE_ID == 1 || x.FILETYPE_ID == 2 || x.FILETYPE_ID == 3 || x.FILETYPE_ID == 4 ||
                        x.FILETYPE_ID == 5 || x.FILETYPE_ID == 7 || x.FILETYPE_ID == 8 || x.FILETYPE_ID == 10).ToList();

                    ViewData["FileTypeList"] = pageFileTypeList;
                }
                if (aPage == "SITE")
                {
                    //Photo (1), Historic (3), Field Sheets (4), Level Notes (5), Site Sketch (6), Other (7), Link (8), Sketch (10), LandOwner (11)
                    pageFileTypeList = FileTypeList.Where(x => x.FILETYPE_ID == 1 || x.FILETYPE_ID == 3 || x.FILETYPE_ID == 4 || x.FILETYPE_ID == 5 ||
                        x.FILETYPE_ID == 6 || x.FILETYPE_ID == 7 || x.FILETYPE_ID == 8 || x.FILETYPE_ID == 10 || x.FILETYPE_ID == 11).ToList();

                    ViewData["FileTypeList"] = pageFileTypeList;
                }
                if (aPage == "OP")
                {
                    //Photo (1), Field Sheets (4), Level Notes (5), Other (7), NGS DataSheet (9), Sketch (10)
                    pageFileTypeList = FileTypeList.Where(x => x.FILETYPE_ID == 1 || x.FILETYPE_ID == 4 || x.FILETYPE_ID == 5 ||
                        x.FILETYPE_ID == 7 || x.FILETYPE_ID == 9 || x.FILETYPE_ID == 10).ToList();

                    ViewData["FileTypeList"] = pageFileTypeList;
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }
        }

        
        #endregion Creates

        // general detail page containing PVs for Details page depending on FILETYPE_ID
        // GET: /FileDetails/
        [Authorize]
        public ActionResult FileDetails(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                FILE aFile = GetFile(id);
                if (aFile.SITE_ID != null && aFile.SITE_ID != 0)
                {
                    ViewData["aSite"] = GetSiteNo(aFile.SITE_ID);
                }

                //need sensor's serial num for breadcrumbs
                if (aFile.INSTRUMENT_ID != null && aFile.INSTRUMENT_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "Instruments/{entityId}";
                    request.RootElement = "INSTRUMENT";
                    request.AddParameter("entityId", aFile.INSTRUMENT_ID, ParameterType.UrlSegment);
                    ViewData["SerialNum"] = serviceCaller.Execute<INSTRUMENT>(request).SERIAL_NUMBER;
                }

                if (aFile.DATA_FILE_ID != null && aFile.DATA_FILE_ID != 0)
                {
                    DATA_FILE thisDF = GetDataFile(aFile.DATA_FILE_ID);
                    ViewData["thisDFid"] = thisDF.DATA_FILE_ID;

                    //Get peak summary info
                    request = new RestRequest();
                    request.Resource = "/PeakSummaries/{entityId}";
                    request.RootElement = "PEAK_SUMMARY";
                    request.AddParameter("entityId", thisDF.PEAK_SUMMARY_ID, ParameterType.UrlSegment);
                    PEAK_SUMMARY thisPKS = serviceCaller.Execute<PEAK_SUMMARY>(request);

                    ViewData["PeakSummary"] = thisPKS;

                }

                //get member logged in's role
                request = new RestRequest();
                request.Resource = "/Members?username={userName}";
                request.RootElement = "MEMBER";
                request.AddParameter("userName", User.Identity.Name, ParameterType.UrlSegment);
                MEMBER thisMember = serviceCaller.Execute<MEMBER>(request);
                if (thisMember.ROLE_ID == 1) { ViewData["Role"] = "Admin"; }
                if (thisMember.ROLE_ID == 2) { ViewData["Role"] = "Manager"; }
                if (thisMember.ROLE_ID == 3) { ViewData["Role"] = "Field"; }

                return View("FileDetails", aFile);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }
        
        #region PhotoFiles Details/Edits
        //
        // GET: /PhotoFileDetailsPV/1
        [Authorize]
        public PartialViewResult PhotoFileDetailsPV(int id)
        {
            try
            {
                FILE aFile = GetFile(id);

                SOURCE aSource = GetSource(aFile.SOURCE_ID);
                
                if (aSource != null)
                {
                    if (aSource.AGENCY_ID != 0)
                    {
                        ViewData["AgencyName"] = GetAgencyName(aSource.AGENCY_ID);
                    }
                }

                //Get the Site name for this file, if any
                if (aFile.SITE_ID != null && aFile.SITE_ID != 0)
                {
                    ViewData["aSite"] = GetSiteNo(aFile.SITE_ID);
                }


                //Get the HWM (id + desc) for this file, if any
                if (aFile.HWM_ID != null && aFile.HWM_ID != 0)
                {
                    ViewData["aHwm"] = GetHWM(aFile.HWM_ID);
                }

                //get the fileType.FILETYPE for this file
                ViewData["aFileType"] = GetFileType(aFile.FILETYPE_ID);

                FileSourceModel PhotoFileModel = new FileSourceModel();
                PhotoFileModel.FSM_File = aFile;
                PhotoFileModel.FSM_Source = aSource;

                return PartialView("FileTypePages/Details/PhotoFileDetailsPV", PhotoFileModel);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //
        // GET: /PhotoFileEditPV/1
        [Authorize]
        public PartialViewResult PhotoFileEditPV(int id)
        {
            try
            {
                //get the file
                FILE aFile = GetFile(id);
                //get the filetype.FILETYPE
                ViewData["aFileType"] = GetFileType(aFile.FILETYPE_ID);

                //Get the Site name for this file, if any
                if (aFile.SITE_ID != null && aFile.SITE_ID != 0)
                {
                    ViewData["aSite"] = GetSiteNo(aFile.SITE_ID);
                }

                //Get the HWM (id + desc) for this file, if any
                if (aFile.HWM_ID != null && aFile.HWM_ID != 0)
                {
                    ViewData["aHwm"] = GetHWM(aFile.HWM_ID);
                }

                SOURCE aSource = GetSource(aFile.SOURCE_ID);
                if (aSource != null)
                {
                    List<AGENCY> agencylist = GetAgencyList();
                    ViewData["AgencyList"] = agencylist;
                }

                FileSourceModel FSModel = new FileSourceModel();
                FSModel.FSM_File = aFile;
                FSModel.FSM_Source = aSource;

                return PartialView("FileTypePages/Edit/PhotoFileEditPV", FSModel);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //
        //POST: /SiteFileEdit/1
        [Authorize]
        [HttpPost]
        public PartialViewResult PhotoFileEdit(int id, FileSourceModel thisFileSourceModel)
        {
            try
            {
                FILE thisFile = thisFileSourceModel.FSM_File;
                SOURCE thisSource = thisFileSourceModel.FSM_Source;

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                //POST to FILE
                var request = new RestRequest(Method.POST);

                request.Resource = "FILES/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", thisFile.FILE_ID, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<FILE>(thisFile), ParameterType.RequestBody);
                serviceCaller.Execute<FILE>(request);

                //POST to SOURCE
                STNServiceCaller serviceCaller1 = STNServiceCaller.Instance;
                request = new RestRequest(Method.POST);
                request.Resource = "/Sources/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", thisSource.SOURCE_ID, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer1 = new STNWebSerializer();
                request.AddParameter("application/xml", serializer1.Serialize<SOURCE>(thisSource), ParameterType.RequestBody);
                serviceCaller1.Execute<SOURCE>(request);
                if (thisSource != null)
                {
                    ViewData["AgencyName"] = GetAgencyName(thisSource.AGENCY_ID);
                }

                //Get the Site name for this file, if any
                if (thisFile.SITE_ID != null && thisFile.SITE_ID != 0)
                {
                    ViewData["aSite"] = GetSiteNo(thisFile.SITE_ID);
                }

                //Get the HWM (id + desc) for this file, if any
                if (thisFile.HWM_ID != null && thisFile.HWM_ID != 0)
                {
                    ViewData["aHwm"] = GetHWM(thisFile.HWM_ID);
                }

                //get the fileType for this file, if any
                if (thisFile.FILETYPE_ID != null)
                {
                    ViewData["aFileType"] = GetFileType(thisFile.FILETYPE_ID);
                }


                return PartialView("FileTypePages/Details/PhotoFileDetailsPV", thisFileSourceModel);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        #endregion PhotoFiles Details/Edits

        #region AllOtherFile Details/Edits

        //
        // GET: /AllOtherFileDetailsPV/1
        [Authorize]
        public PartialViewResult AllOtherFileDetailsPV(int id)
        {
            try
            {
                FILE aFile = GetFile(id);

                SOURCE aSource = GetSource(aFile.SOURCE_ID);
                if (aSource.AGENCY_ID != null && aSource.AGENCY_ID != 0)
                {
                    ViewData["AgencyName"] = GetAgencyName(aSource.AGENCY_ID);
                }

                //Get the Site name for this file, if any
                if (aFile.SITE_ID != null && aFile.SITE_ID != 0)
                {
                    ViewData["aSite"] = GetSiteNo(aFile.SITE_ID);
                }

                //Get the HWM (id + desc) for this file, if any
                if (aFile.HWM_ID != null && aFile.HWM_ID != 0)
                {
                    ViewData["aHwm"] = GetHWM(aFile.HWM_ID);
                }

                //get the fileType for this file
                ViewData["aFileType"] = GetFileType(aFile.FILETYPE_ID);

                FileSourceModel FSModel = new FileSourceModel();
                FSModel.FSM_File = aFile;
                FSModel.FSM_Source = aSource;

                return PartialView("FileTypePages/Details/AllOtherFileDetailsPV", FSModel);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //
        // GET: /AllOtherFileEditPV/1
        [Authorize]
        public PartialViewResult AllOtherFileEditPV(int id)
        {
            try
            {
                //get the file
                FILE aFile = GetFile(id);
                //get the filetype.FILETYPE
                ViewData["aFileType"] = GetFileType(aFile.FILETYPE_ID);

                //Get the Site name for this file, if any
                if (aFile.SITE_ID != null && aFile.SITE_ID != 0)
                {
                    ViewData["aSite"] = GetSiteNo(aFile.SITE_ID);
                }

                //Get the HWM (id + desc) for this file, if any
                if (aFile.HWM_ID != null && aFile.HWM_ID != 0)
                {
                    ViewData["aHwm"] = GetHWM(aFile.HWM_ID);
                }

                SOURCE aSource = GetSource(aFile.SOURCE_ID);
                if (aSource != null)
                {
                    List<AGENCY> agencylist = GetAgencyList();
                    ViewData["AgencyList"] = agencylist;
                }

                FileSourceModel FSModel = new FileSourceModel();
                FSModel.FSM_File = aFile;
                FSModel.FSM_Source = aSource;

                return PartialView("FileTypePages/Edit/AllOtherFileEditPV", FSModel);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //
        //POST: /AllOtherFileEdit/1
        [Authorize]
        [HttpPost]
        public PartialViewResult AllOtherFileEdit(int id, FileSourceModel thisFileSourceModel)
        {
            try
            {
                FILE thisFile = thisFileSourceModel.FSM_File;
                SOURCE thisSource = thisFileSourceModel.FSM_Source;

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                //POST to FILE
                var request = new RestRequest(Method.POST);

                request.Resource = "FILES/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", thisFile.FILE_ID, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<FILE>(thisFile), ParameterType.RequestBody);
                serviceCaller.Execute<FILE>(request);

                //POST to SOURCE
                STNServiceCaller serviceCaller1 = STNServiceCaller.Instance;
                request = new RestRequest(Method.POST);
                request.Resource = "/Sources/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", thisSource.SOURCE_ID, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer1 = new STNWebSerializer();
                request.AddParameter("application/xml", serializer1.Serialize<SOURCE>(thisSource), ParameterType.RequestBody);
                serviceCaller1.Execute<SOURCE>(request);
                if (thisSource != null)
                {
                    ViewData["AgencyName"] = GetAgencyName(thisSource.AGENCY_ID);
                }

                //Get the Site name for this file, if any
                if (thisFile.SITE_ID != null && thisFile.SITE_ID != 0)
                {
                    ViewData["aSite"] = GetSiteNo(thisFile.SITE_ID);
                }

                //Get the HWM (id + desc) for this file, if any
                if (thisFile.HWM_ID != null && thisFile.HWM_ID != 0)
                {
                    ViewData["aHwm"] = GetHWM(thisFile.HWM_ID);
                }

                //get the fileType for this file
                ViewData["aFileType"] = GetFileType(thisFile.FILETYPE_ID);

                return PartialView("FileTypePages/Details/AllOtherFileDetailsPV", thisFileSourceModel);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        #endregion AllOtherFile Details/Edits

        #region DataFiles Details/Edits

        //
        //GET: /SensorFiles/DataFileDetailsPV/1
        [Authorize]
        public PartialViewResult DataFileDetailsPV(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                FILE thisFile = GetFile(id);

                DATA_FILE thisDF = GetDataFile(thisFile.DATA_FILE_ID);

                FileDataFileModel aSensorDFModel = new FileDataFileModel();
                aSensorDFModel.FDFM_DataFile = thisDF;
                aSensorDFModel.FDFM_File = thisFile;

                if (thisDF.PROCESSOR_ID != 0 && thisDF.PROCESSOR_ID != null)
                {
                    request.Resource = "/Members/{entityId}";
                    request.RootElement = "MEMBER";
                    request.AddParameter("entityId", thisDF.PROCESSOR_ID, ParameterType.UrlSegment);
                    MEMBER thisProcessor = serviceCaller.Execute<MEMBER>(request);
                    ViewData["Processor"] = thisProcessor.FNAME + " " + thisProcessor.LNAME;
                }

                //get filetype.FILETYPE
                ViewData["aFileType"] = GetFileType(thisFile.FILETYPE_ID);

                //Get the Site name for this file, if any
                if (thisFile.SITE_ID != null && thisFile.SITE_ID != 0)
                {
                    ViewData["aSite"] = GetSiteNo(thisFile.SITE_ID);
                }

                //Get the HWM (id + desc) for this file, if any
                if (thisFile.HWM_ID != null && thisFile.HWM_ID != 0)
                {
                    ViewData["aHwm"] = GetHWM(thisFile.HWM_ID);
                }

                return PartialView("FileTypePages/Details/DataFileDetailsPV", aSensorDFModel);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //
        //GET: /SensorFiles/DataFileEditPV/1
        [Authorize]
        public PartialViewResult DataFileEditPV(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                FILE thisFile = GetFile(id);

                DATA_FILE thisDF = GetDataFile(thisFile.DATA_FILE_ID);

                request.Resource = "/Members/{entityId}";
                request.RootElement = "MEMBER";
                request.AddParameter("entityId", thisDF.PROCESSOR_ID, ParameterType.UrlSegment);
                MEMBER thisProcessor = serviceCaller.Execute<MEMBER>(request);
                ViewData["Processor"] = thisProcessor.FNAME + " " + thisProcessor.LNAME;


                FileDataFileModel aSensorDFModel = new FileDataFileModel();
                aSensorDFModel.FDFM_File = thisFile;
                aSensorDFModel.FDFM_DataFile = thisDF;

                //get time zones
                System.Collections.ObjectModel.ReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();
                List<string> USAtimeZones = new List<string>();
                foreach (TimeZoneInfo tzI in timeZones)
                {
                    if (tzI.DisplayName.Contains("US "))
                    {
                        if (tzI.StandardName.Contains("Pacific"))
                        { USAtimeZones.Add("PST"); }
                        else if (tzI.StandardName.Contains("Mountain"))
                        { USAtimeZones.Add("MST"); }
                        else if (tzI.StandardName.Contains("Central"))
                        { USAtimeZones.Add("CST"); }
                        else
                        { USAtimeZones.Add("EST"); }
                    }
                }
                USAtimeZones.Insert(0, "UTC");
                ViewData["TimeZones"] = USAtimeZones;

                //get filetype.FILETYPE
                ViewData["FileType"] = GetFileType(thisFile.FILETYPE_ID);

                //Get the Site name for this file, if any
                if (thisFile.SITE_ID != null && thisFile.SITE_ID != 0)
                {
                    ViewData["aSite"] = GetSiteNo(thisFile.SITE_ID);
                }

                //Get the HWM (id + desc) for this file, if any
                if (thisFile.HWM_ID != null && thisFile.HWM_ID != 0)
                {
                    ViewData["aHwm"] = GetHWM(thisFile.HWM_ID);
                }

                return PartialView("FileTypePages/Edit/DataFileEditPV", aSensorDFModel);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //
        // POST: /SensorFiles/DataFileEditPV/1
        [Authorize]
        [HttpPost]
        public PartialViewResult DataFileEdit(int id, FileDataFileModel thisSensorDataFile)
        {
            try
            {
                FILE thisFile = thisSensorDataFile.FDFM_File;
                DATA_FILE thisDataFile = thisSensorDataFile.FDFM_DataFile;

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                 
                //make sure timezone and timestamp are in utc
                if (thisDataFile.TIME_ZONE != "UTC")
                {
                    thisDataFile.TIME_ZONE = "UTC";
                    thisDataFile.GOOD_START = thisDataFile.GOOD_START.Value.ToUniversalTime();
                    thisDataFile.GOOD_END = thisDataFile.GOOD_END.Value.ToUniversalTime();
                }

                //post to DATA_FILE
                var request = new RestRequest(Method.POST);
                request.Resource = "DataFiles/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", thisDataFile.DATA_FILE_ID, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<DATA_FILE>(thisDataFile), ParameterType.RequestBody);
                serviceCaller.Execute<DATA_FILE>(request);
                
                //post to FILES
                request = new RestRequest(Method.POST);
                request.Resource = "/Files/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", thisFile.FILE_ID, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<FILE>(thisFile), ParameterType.RequestBody);
                serviceCaller.Execute<FILE>(request);
                        
                //store ViewData props for Details PV
                //get filetype.FILETYPE
                ViewData["aFileType"] = GetFileType(thisFile.FILETYPE_ID);
                //Get the Site name for this file, if any
                if (thisFile.SITE_ID != null && thisFile.SITE_ID != 0)
                {
                    ViewData["aSite"] = GetSiteNo(thisFile.SITE_ID);
                }

                //Get the HWM (id + desc) for this file, if any
                if (thisFile.HWM_ID != null && thisFile.HWM_ID != 0)
                {
                    ViewData["aHwm"] = GetHWM(thisFile.HWM_ID);
                }

                request = new RestRequest();
                request.Resource = "/Members/{entityId}";
                request.RootElement = "MEMBER";
                request.AddParameter("entityId", thisDataFile.PROCESSOR_ID, ParameterType.UrlSegment);
                MEMBER thisProcessor = serviceCaller.Execute<MEMBER>(request);
                ViewData["Processor"] = thisProcessor.FNAME + " " + thisProcessor.LNAME;

                return PartialView("FileTypePages/Details/DataFileDetailsPV", thisSensorDataFile);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }            
        }
                
        #endregion DataFiles Details/Edits

        

        //private functions for Create Post action
        #region private functions for POST create

        //populates the fields for Data file types for post (only Sensors)
        [Authorize]
        private List<object> GetDataFile(string fileTP, FormCollection FC)
        {
            
            FILE thisFile = new FILE();
            DATA_FILE thisDFile = new DATA_FILE();

            thisFile.FILETYPE_ID = Convert.ToDecimal(fileTP);
            thisFile.FILE_URL = FC["FDFM_File.FILE_URL"];
            string memberName = FC["FDFM_DataFile.PROCESSOR"];
            thisDFile.PROCESSOR_ID = GetMember(memberName).MEMBER_ID;
            thisDFile.COLLECT_DATE = Convert.ToDateTime(FC["FDFM_DataFile.COLLECT_DATE"]);
            thisFile.FILE_DATE = Convert.ToDateTime(FC["FDFM_File.FILE_DATE"]);
            thisFile.DESCRIPTION = FC["FDFM_File.DESCRIPTION"];
            thisDFile.GOOD_START = Convert.ToDateTime(FC["FDFM_DataFile.GOOD_START"]);
            thisDFile.GOOD_END = Convert.ToDateTime(FC["FDFM_DataFile.GOOD_END"]);

            thisDFile.TIME_ZONE = FC["FDFM_DataFile.TIME_ZONE"];
            //see what timezone they chose
            if (thisDFile.TIME_ZONE != "UTC")
            {
                thisDFile.TIME_ZONE = "UTC";
                thisDFile.GOOD_START = thisDFile.GOOD_START.Value.ToUniversalTime();
                thisDFile.GOOD_END = thisDFile.GOOD_END.Value.ToUniversalTime();
            }
            thisDFile.ELEVATION_STATUS = FC["ELEVATION_STATUS"];

            thisFile.HWM_ID = Convert.ToDecimal(FC["HWM_ID"]);
            thisFile.SITE_ID = Convert.ToDecimal(FC["SITE_ID"]);
            thisFile.INSTRUMENT_ID = Convert.ToDecimal(FC["SensorID"]);
            thisDFile.INSTRUMENT_ID = Convert.ToDecimal(FC["SensorID"]);
            thisFile.OBJECTIVE_POINT_ID = Convert.ToDecimal(FC["OPID"]);

            List<object> dataFileObjects = new List<object>();
            dataFileObjects.Add(thisFile);
            dataFileObjects.Add(thisDFile);

            return dataFileObjects;
            
        }

        //populates the fields for photo file types for post (Sites, Sensors, or HWM)
        [Authorize]
        private List<object> GetPhotoFile(string filetype, FormCollection FC)
        {
            FILE thisFile = new FILE();
            SOURCE thisSource = new SOURCE();

            thisFile.FILETYPE_ID = Convert.ToDecimal(filetype);
            thisFile.SITE_ID = Convert.ToDecimal(FC["SITE_ID"]);
            thisFile.HWM_ID = Convert.ToDecimal(FC["HWM_ID"]);
            thisFile.OBJECTIVE_POINT_ID = Convert.ToDecimal(FC["OPID"]);
            thisFile.INSTRUMENT_ID = Convert.ToDecimal(FC["SensorID"]);

            thisFile.FILE_URL = FC["FSM_File.FILE_URL"];
            thisSource.SOURCE_NAME = FC["FSM_Source.SOURCE_NAME"];
            thisSource.AGENCY_ID = Convert.ToDecimal(FC["FSM_Source.AGENCY_ID"]);
            thisSource.SOURCE_DATE = Convert.ToDateTime(FC["FSM_Source.SOURCE_DATE"]);            
            thisFile.FILE_DATE = Convert.ToDateTime(FC["FSM_File.FILE_DATE"]);
            thisFile.DESCRIPTION = FC["FSM_File.DESCRIPTION"];
            thisFile.PHOTO_DIRECTION = FC["FSM_File.PHOTO_DIRECTION"];

            //check to see if the lat/long values were entered (different than site/hwm)
            if (FC["FSM_File.LATITUDE_DD"] != "")
            {
                var lat = (FC["FSM_File.LATITUDE_DD"]);
                thisFile.LATITUDE_DD = Math.Round(Convert.ToDecimal(lat), 6); 
            }
            else
            {
                thisFile.LATITUDE_DD = Math.Round(Convert.ToDecimal(FC["LATITUDE_DD"]), 6); 
            }
            if (FC["FSM_File.LONGITUDE_DD"] != "")
            {
                var lon = (FC["FSM_File.LONGITUDE_DD"]);
                thisFile.LONGITUDE_DD = Math.Round(Convert.ToDecimal(lon), 6);
                thisFile.LONGITUDE_DD = thisFile.LONGITUDE_DD.Value < 0 ? thisFile.LONGITUDE_DD.Value : thisFile.LONGITUDE_DD.Value * (-1);
            }
            else
            {
                thisFile.LONGITUDE_DD = Math.Round(Convert.ToDecimal(FC["LONGITUDE_DD"]), 6);
                thisFile.LONGITUDE_DD = thisFile.LONGITUDE_DD.Value < 0 ? thisFile.LONGITUDE_DD.Value : thisFile.LONGITUDE_DD.Value * (-1);
            }
                        
            List<object> photoFileObjects = new List<object>();
            photoFileObjects.Add(thisFile);
            photoFileObjects.Add(thisSource);

            return photoFileObjects;
        }

        //populates the fields for all other file types for post (Sites, Sensors, or HWM)
        [Authorize]
        private List<object> GetAllElseFile(string filetype, FormCollection FC)
        {
            FILE thisFile = new FILE();
            SOURCE thisSource = new SOURCE();

            thisFile.FILETYPE_ID = Convert.ToDecimal(filetype);
            thisFile.SITE_ID = Convert.ToDecimal(FC["SITE_ID"]);
            thisFile.HWM_ID = Convert.ToDecimal(FC["HWM_ID"]);
            thisFile.INSTRUMENT_ID = Convert.ToDecimal(FC["SensorID"]);
            thisFile.OBJECTIVE_POINT_ID = Convert.ToDecimal(FC["OPID"]);

            thisFile.FILE_URL = FC["FSM_File.FILE_URL"];
            thisSource.SOURCE_NAME = FC["FSM_Source.SOURCE_NAME"];
            thisSource.AGENCY_ID = Convert.ToDecimal(FC["FSM_Source.AGENCY_ID"]);
            thisSource.SOURCE_DATE = Convert.ToDateTime(FC["FSM_Source.SOURCE_DATE"]);            
            thisFile.FILE_DATE = Convert.ToDateTime(FC["FSM_File.FILE_DATE"]);
            thisFile.DESCRIPTION = FC["FSM_File.DESCRIPTION"];           

            List<object> FileObjects = new List<object>();
            FileObjects.Add(thisFile);
            FileObjects.Add(thisSource);

            return FileObjects;
        }
        
        #endregion private functions for POST create

        // as of 12.19 this is in the "Delete" actionLink on the FileDetails Page
        //GET: Sites/SiteFileDelete/1
        [Authorize]
        public ActionResult SiteFileDelete(int id, int siteID)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "FILES/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");

                serviceCaller.Execute<FILE>(request);
                return RedirectToAction("Details", "Sites", new { id = siteID });
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }
        
        #region reusable methods called throughout 
        //returns aFile whenever called
        [Authorize]
        public FILE GetFile(decimal id)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "FILES/{entityId}";
            request.RootElement = "FILE";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            FILE aFile = serviceCaller.Execute<FILE>(request);
            return aFile;
        }

        //returns aDataFile whenever called
        [Authorize]
        public DATA_FILE GetDataFile(decimal? id)
        {
            DATA_FILE aDataFile = new DATA_FILE();
            if (id != null)
            {
                //checking if it's null because the uploader may have not connected a file with a datafile
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "DataFiles/{entityId}";
                request.RootElement = "DATA_FILE";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                aDataFile = serviceCaller.Execute<DATA_FILE>(request);
            }
            return aDataFile;
        }

        //returns agencyList whenever called
        [Authorize]
        public List<AGENCY> GetAgencyList()
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/AGENCIES";
            request.RootElement = "ArrayOfAGENCY";
            List<AGENCY> agencyList = serviceCaller.Execute<List<AGENCY>>(request);
            agencyList = agencyList.OrderBy(x => x.AGENCY_NAME).ToList();
            return agencyList;
        }

        //returns aSource whenever called
        [Authorize]
        public SOURCE GetSource(decimal? id)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Sources/{entityId}";
            request.RootElement = "SOURCE";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            SOURCE aSource = serviceCaller.Execute<SOURCE>(request);
            return aSource;
        }
        
        //returns anAgency.AGENCYNAME whenever called
        [Authorize]
        public string GetAgencyName(decimal? id)
        {

            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Agencies/{entityId}";
            request.RootElement = "AGENCY";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            string agencyName = serviceCaller.Execute<AGENCY>(request).AGENCY_NAME;
            return agencyName;
        }

        //returns aFileType.FILETYPE whenever called
        [Authorize]
        public string GetFileType(decimal? id)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/FileTypes/{entityId}";
            request.RootElement = "FILE_TYPE";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            string aFileType = serviceCaller.Execute<FILE_TYPE>(request).FILETYPE;
            return aFileType;
        }

        //returns aSite.SITE_NO whenever called
        [Authorize]
        public string GetSiteNo(decimal? id)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "Sites/{entityId}";
            request.RootElement = "SITE";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            string siteNo = serviceCaller.Execute<SITE>(request).SITE_NO;
            return siteNo;
        }

        //returns aHWM string whenever called
        [Authorize]
        public string GetHWM(decimal? id)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "HWMs/{entityId}";
            request.RootElement = "HWM";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            string HWMdesc = serviceCaller.Execute<HWM>(request).HWM_LOCATIONDESCRIPTION;
            string aHWM = "(" + id.ToString() + ") " + HWMdesc;
            return aHWM;
        }

        //returns member
        [Authorize]
        public MEMBER GetMember(string name)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Members?username={userName}";
            request.RootElement = "MEMBER";
            request.AddParameter("userName", name, ParameterType.UrlSegment);
            MEMBER aMember = serviceCaller.Execute<MEMBER>(request);

            return aMember;
        }
        #endregion reusable methods called throughout
    }
}
