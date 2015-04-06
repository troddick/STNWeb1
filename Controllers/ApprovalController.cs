//------------------------------------------------------------------------------
//----- ApprovalController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jonathan Baier USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Display a master Approval page and link to individual Approval Point pages 
//
//discussion:   
//
//     

#region Comments
 // 04.17.13 - TR - Adding filters to Approval Index page
 // 11.30.12 - TR - Added ApprovalInfoBox partial view - removed
 // 11.16.12 - TR - Added Approve
 // 10.24.12 - JB - Added role authorization
 // 10.19.12 - JB - Created
#endregion


using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Linq;

using RestSharp;
using STNServices;
using STNServices.Authentication;
using STNServices.Resources;
using STNWeb.Utilities;
using STNWeb.Models;
using STNWeb.Providers;
using STNWeb.Helpers;

    
namespace STNWeb.Controllers
{
    [RequireSSL]
    //[RoleAuthorize(new string[] {"Admin", "Manager"})]
    public class ApprovalController : Controller
    {
        //Lists all provisional HWMs and DataFiles
        // GET: /Approval/Index
        public ActionResult Index()
        {
            try
            {
                ViewBag.CurrentPage = "APPROVAL";

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //populate dropdowns for filters
                request.Resource = "/Events/";
                request.RootElement = "ArrayOfEvent";
                List<EVENT> eventList = serviceCaller.Execute<List<EVENT>>(request);
                ViewData["EventList"] = eventList;

                request = new RestRequest();
                request.Resource = "/States";
                request.RootElement = "STATES";
                List<STATES> AllStates = serviceCaller.Execute<List<STATES>>(request);
                ViewData["StateList"] = AllStates; // Enum.GetValues(typeof(STNServices.Handlers.HandlerBase.State)).Cast<STNServices.Handlers.HandlerBase.State>().ToList();

                request = new RestRequest();
                request.Resource = "/MEMBERS/";
                request.RootElement = "ArrayOfMember";
                List<MEMBER> MemberList = serviceCaller.Execute<List<MEMBER>>(request);
                List<MEMBER> SortedMembers = MemberList.OrderBy(m => m.LNAME).ToList<MEMBER>();
                ViewData["MemberList"] = SortedMembers;

                return View();
            }
            catch (Exception e)
            {

                return View("../Shared/Error", e);
            }
        }

        //here's my parameters for the filtered hwms and data files i want
        //GET:
        public PartialViewResult FilteredApprovals(FormCollection fc)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                
                string eventId = fc["EVENT_ID"];
                string StateName = fc["STATE_NAME"];
                string MemberId = fc["MEMBER_ID"];

                var request = new RestRequest();
                request.Resource = "/HWMS?IsApproved={approved}&Event={eventId}&TeamMember={memberId}&State={state}";
                request.RootElement = "ArrayOfHWM";
                request.AddParameter("approved", false, ParameterType.UrlSegment);
                request.AddParameter("eventId", eventId, ParameterType.UrlSegment);
                request.AddParameter("memberId", MemberId, ParameterType.UrlSegment);
                request.AddParameter("state", StateName, ParameterType.UrlSegment);
                HWMList theResultingHWMs = serviceCaller.Execute<HWMList>(request);

                List<FilteredApprovals.hwmApproval> Listhwm = new List<FilteredApprovals.hwmApproval>();

                //populate model for display
                if (theResultingHWMs != null && theResultingHWMs.HWMs.Count() > 0)
                {
                    foreach (SimpleHWM h in theResultingHWMs.HWMs)
                    {
                        FilteredApprovals.hwmApproval hwmApproval = new FilteredApprovals.hwmApproval();
                        hwmApproval.HWM_ID = h.HWM_ID;
                        hwmApproval.ELEV_FT = h.ELEV_FT;
                        hwmApproval.SiteNo = h.SITE_NO;
                        Listhwm.Add(hwmApproval);
                    }
                    ViewData["hwmList"] = Listhwm;
                }

                request = new RestRequest();
                request.Resource = "/DataFiles?IsApproved={approved}&Event={eventId}&Processor={memberId}&State={state}";
                request.RootElement = "ArrayOfDATA_FILE";
                request.AddParameter("approved", false, ParameterType.UrlSegment);
                request.AddParameter("eventId", eventId, ParameterType.UrlSegment);
                request.AddParameter("memberId", MemberId, ParameterType.UrlSegment);
                request.AddParameter("state", StateName, ParameterType.UrlSegment);
                List<DATA_FILE> theResultingDFs = serviceCaller.Execute<List<DATA_FILE>>(request);

                List<FilteredApprovals.dataFileApproval> ListDFs = new List<FilteredApprovals.dataFileApproval>();
                if (theResultingDFs != null && theResultingDFs.Count() > 0)
                {
                    foreach (DATA_FILE df in theResultingDFs)
                    {
                        FilteredApprovals.dataFileApproval dfApproval = new FilteredApprovals.dataFileApproval();
                        dfApproval.DF_ID = df.DATA_FILE_ID;
                        dfApproval.Inst_ID = df.INSTRUMENT_ID;
                        request = new RestRequest();
                        request.Resource = "Instruments/{entityId}";
                        request.RootElement = "INSTRUMENT";
                        request.AddParameter("entityId", df.INSTRUMENT_ID, ParameterType.UrlSegment);
                        INSTRUMENT thisInstrument = serviceCaller.Execute<INSTRUMENT>(request);

                        request = new RestRequest();
                        request.Resource = "/SensorTypes/{entityId}";
                        request.RootElement = "SENSOR_TYPE";
                        request.AddParameter("entityId", thisInstrument.SENSOR_TYPE_ID, ParameterType.UrlSegment);
                        dfApproval.SensorType = serviceCaller.Execute<SENSOR_TYPE>(request).SENSOR;

                        request = new RestRequest();
                        request.Resource = "Sites/{entityId}";
                        request.RootElement = "SITE";
                        request.AddParameter("entityId", thisInstrument.SITE_ID, ParameterType.UrlSegment);
                        dfApproval.SiteNo = serviceCaller.Execute<SITE>(request).SITE_NO;
                        ListDFs.Add(dfApproval);
                    }

                    ViewData["dataFileList"] = ListDFs;
                }

                return PartialView();
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }
            
        //This function is now hidden on the page...GET all the hwms and data files that need to be approved
        public PartialViewResult ShowAllApproval()
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "/HWMs?IsApproved={boolean}";
                request.RootElement = "ArrayOfHWMs";
                request.AddParameter("boolean", false, ParameterType.UrlSegment);
                List<HWM> allHWMs = serviceCaller.Execute<List<HWM>>(request);

                List<FilteredApprovals.hwmApproval> Listhwm = new List<FilteredApprovals.hwmApproval>();
                //populate model for display
                foreach (HWM h in allHWMs)
                {
                    FilteredApprovals.hwmApproval hwmApproval = new FilteredApprovals.hwmApproval();
                    hwmApproval.HWM_ID = h.HWM_ID;
                    hwmApproval.ELEV_FT = h.ELEV_FT;

                    request = new RestRequest();
                    request.Resource = "Sites/{entityId}";
                    request.RootElement = "SITE";
                    request.AddParameter("entityId", h.SITE_ID, ParameterType.UrlSegment);
                    SITE thisOne = serviceCaller.Execute<SITE>(request);
                    hwmApproval.SiteNo = thisOne != null ? thisOne.SITE_NO : "";
    
                    Listhwm.Add(hwmApproval);
                }
                ViewData["hwmList"] = Listhwm;

                request = new RestRequest();
                request.Resource = "/DataFiles?IsApproved={boolean}";
                request.RootElement = "ArrayOfDATA_FILE";
                request.AddParameter("boolean", false, ParameterType.UrlSegment);
                List<DATA_FILE> allDFs = serviceCaller.Execute<List<DATA_FILE>>(request);
                List<FilteredApprovals.dataFileApproval> ListDFs = new List<FilteredApprovals.dataFileApproval>();

                foreach (DATA_FILE df in allDFs)
                {
                    FilteredApprovals.dataFileApproval dfApproval = new FilteredApprovals.dataFileApproval();
                    dfApproval.DF_ID = df.DATA_FILE_ID;
                    dfApproval.Inst_ID = df.INSTRUMENT_ID;

                    request = new RestRequest();
                    request.Resource = "Instruments/{entityId}";
                    request.RootElement = "INSTRUMENT";
                    request.AddParameter("entityId", df.INSTRUMENT_ID, ParameterType.UrlSegment);
                    INSTRUMENT thisInstrument = serviceCaller.Execute<INSTRUMENT>(request);

                    if (thisInstrument != null)
                    {
                        request = new RestRequest();
                        request.Resource = "/SensorTypes/{entityId}";
                        request.RootElement = "SENSOR_TYPE";
                        request.AddParameter("entityId", thisInstrument.SENSOR_TYPE_ID, ParameterType.UrlSegment);
                        dfApproval.SensorType = serviceCaller.Execute<SENSOR_TYPE>(request).SENSOR;

                        request = new RestRequest();
                        request.Resource = "Sites/{entityId}";
                        request.RootElement = "SITE";
                        request.AddParameter("entityId", thisInstrument.SITE_ID, ParameterType.UrlSegment);
                        SITE thisOne = serviceCaller.Execute<SITE>(request);
                        dfApproval.SiteNo = thisOne != null ? thisOne.SITE_NO : "";
                    }
                    ListDFs.Add(dfApproval);
                }

                ViewData["dataFileList"] = ListDFs;

                return PartialView("FilteredApprovals");
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }


        //GET approvalInfoBox/id
        public PartialViewResult ApprovalInfoBox(int id, string FROM)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                APPROVAL anApproval = new APPROVAL();
                ViewData["FROM"] = FROM;
                ViewData["ID"] = id;

                //which approval are we interested in
                if (FROM == "HWM")
                {
                    request.Resource = "HWMs/{entityId}";
                    request.RootElement = "HWM";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    HWM aHWM = serviceCaller.Execute<HWM>(request);

                    request = new RestRequest();
                    request.Resource = "/Approvals/{entityId}";
                    request.RootElement = "APPROVAL";
                    request.AddParameter("entityId", aHWM.APPROVAL_ID, ParameterType.UrlSegment);
                    anApproval = serviceCaller.Execute<APPROVAL>(request);
                }
                else if (FROM == "DF")
                {
                    //if it's 0, there is an issue from the uploader not connecting the file to the datafile
                    if (id != 0)
                    {
                        request = new RestRequest();
                        request.Resource = "DataFiles/{entityId}";
                        request.RootElement = "DATA_FILE";
                        request.AddParameter("entityId", id, ParameterType.UrlSegment);
                        DATA_FILE aDataFile = serviceCaller.Execute<DATA_FILE>(request);

                        request = new RestRequest();
                        request.Resource = "/Approvals/{entityId}";
                        request.RootElement = "APPROVAL";
                        request.AddParameter("entityId", aDataFile.APPROVAL_ID, ParameterType.UrlSegment);
                        anApproval = serviceCaller.Execute<APPROVAL>(request);
                    }
                    else
                        anApproval = null;
                }

                if (anApproval != null)
                {
                    //Get Approving Member
                    request = new RestRequest();
                    request.Resource = "Members/{entityId}";
                    request.RootElement = "MEMBER";
                    request.AddParameter("entityId", anApproval.MEMBER_ID, ParameterType.UrlSegment);
                    string memUsername = serviceCaller.Execute<MEMBER>(request).USERNAME;

                    ViewData["ApprovalSummary"] = memUsername;
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

                return PartialView();
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }


        // approve a HWM
        //POST: /HWMs/1/Approve
        [HttpPost]
        public string Approve(int id, string FROM)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                APPROVAL anApproval = new APPROVAL();

                //Determine which is getting approved - a hwm or datafile
                if (FROM == "HWM")
                {
                    //approve it
                    request.Resource = "HWMs/{hwmId}/Approve";
                    request.AddParameter("hwmId", id, ParameterType.UrlSegment);
                    request.RequestFormat = DataFormat.Xml;
                    request.AddHeader("Content-type", "application/xml");
                    anApproval = serviceCaller.Execute<APPROVAL>(request);
                }
                else if (FROM == "DF")
                {
                    request.Resource = "DataFiles/{dataFileId}/Approve";
                    request.AddParameter("dataFileId", id, ParameterType.UrlSegment);
                    request.RequestFormat = DataFormat.Xml;
                    request.AddHeader("Content-type", "application/xml");
                    anApproval = serviceCaller.Execute<APPROVAL>(request);
                }

                string memUsername = string.Empty;
                //now get the approving member
                if (anApproval.APPROVAL_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "Members/{entityId}";
                    request.RootElement = "MEMBER";
                    request.AddParameter("entityId", anApproval.MEMBER_ID, ParameterType.UrlSegment);
                    memUsername = serviceCaller.Execute<MEMBER>(request).USERNAME;

                    ViewData["ApprovalSummary"] = memUsername;
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

                return memUsername;  
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        // approve a HWM
        //POST: /HWMs/1/Unapprove
        [HttpPost]
        public string UnApprove(int id, string FROM)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                //post this request to approval
                request.Resource = "/HWMs/{hwmId}/Unapprove";
                request.AddParameter("hwmId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<APPROVAL>(request);
                return "Provisional";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }


        // approve a DataFile
        //POST: /DataFile/1/Approve
        [HttpPost]
        public string ApproveDF(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                string thisid = id.ToString();
                //post this request to approval
                request.Resource = "DataFiles/{dataFileId}/Approve";
                request.AddParameter("dataFileId", id, ParameterType.UrlSegment);
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-type", "application/xml");
                APPROVAL anApproval = serviceCaller.Execute<APPROVAL>(request);

                string memUsername = string.Empty;
                //now get the approving member
                if (anApproval != null)
                {
                    request = new RestRequest();
                    request.Resource = "Members/{entityId}";
                    request.RootElement = "MEMBER";
                    request.AddParameter("entityId", anApproval.MEMBER_ID, ParameterType.UrlSegment);
                    memUsername = serviceCaller.Execute<MEMBER>(request).USERNAME;

                    ViewData["ApprovalSummary"] = memUsername;
                }
                return memUsername;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
    }
}
