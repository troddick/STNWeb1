//------------------------------------------------------------------------------
//----- ReportingController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Populates Reporting resource for the view
//
//discussion:   
//
//     

#region Comments
// 02.10.14 - TR - Created
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using System.Data.Objects.DataClasses;
using System.Text;

using RestSharp;
using STNServices;
using STNServices.Authentication;
using STNServices.Resources;
using STNWeb.Utilities;
using STNWeb.Models;
using STNWeb.Helpers;


namespace STNWeb.Controllers
{
    [RequireSSL]
    [Authorize]
    public class ReportingController : Controller
    {
        //
        // GET: /Reporting/

        public ActionResult IndexReport()
        {
            ViewBag.CurrentPage = "REPORTING";

            try
            {
                //go see if there are any incomplete reports for this Member
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //get states    
                List<STATES> states = getAllStates();
                //states.Insert(0, "All States");
                ViewData["stateList"] = states;

                //get events
                ViewData["EventList"] = getAllEvents();

                request = new RestRequest();
                request.Resource = "/Members?username={userName}";
                request.RootElement = "MEMBER";
                request.AddParameter("userName", User.Identity.Name, ParameterType.UrlSegment);
                MEMBER thisMember = serviceCaller.Execute<MEMBER>(request);

                request = new RestRequest();
                request.Resource = "/Members/{memberId}/Reports";
                request.RootElement = "ArrayOfREPORTING_METRICS";
                request.AddParameter("memberId", thisMember.MEMBER_ID, ParameterType.UrlSegment);
                List<REPORTING_METRICS> reportList = serviceCaller.Execute<List<REPORTING_METRICS>>(request);
                if (reportList != null)
                {
                    if (reportList.Count >= 1)
                    {
                        //give me just the ones that are incomplete
                        List<REPORTING_METRICS> incompleteReports = reportList.Where(a => a.COMPLETE == 0).ToList();
                        if (incompleteReports.Count >= 1)
                            ViewData["IncompleteReports"] = incompleteReports;
                    }
                }

                ////get Today and Yesterday's states that have Created Reports
                //string today = DateTime.Now.Date.ToShortDateString().ToString();

                //request = new RestRequest();
                //request.Resource = "/ReportingMetrics/ReportsByDate?Date={aDate}";
                //request.RootElement = "ArrayOfREPORTING_METRICS";
                //request.AddParameter("aDate", today, ParameterType.UrlSegment);
                //List<REPORTING_METRICS> TodaysReports = serviceCaller.Execute<List<REPORTING_METRICS>>(request);
                //if (TodaysReports != null)
                //{
                //    if (TodaysReports.Count >= 1)
                //    {
                //        List<ReportingModel> TodayrepModelList = new List<ReportingModel>();
                //        List<List<ReportingModel>> TodeventGroupedReps = new List<List<ReportingModel>>();
                //        foreach (REPORTING_METRICS rm in TodaysReports)
                //        {
                //            ReportingModel rep = new ReportingModel();
                //            rep.aReport = rm;

                //            request = new RestRequest();
                //            request.Resource = "/Events/{entityId}";
                //            request.RootElement = "EVENT";
                //            request.AddParameter("entityId", rm.EVENT_ID, ParameterType.UrlSegment);
                //            rep.anEvent = serviceCaller.Execute<EVENT>(request);

                //            TodayrepModelList.Add(rep);
                //            TodeventGroupedReps = TodayrepModelList.GroupBy(e => e.aReport.EVENT_ID).Select(grp => grp.ToList()).ToList();
                //        }
                //        ViewData["TodayReports"] = TodeventGroupedReps;
                //    }
                //}
                string yesterday = DateTime.Now.AddDays(-1).Date.ToString();

                request = new RestRequest();
                request.Resource = "/ReportingMetrics/ReportsByDate?Date={aDate}";
                request.RootElement = "ArrayOfREPORTING_METRICS";
                request.AddParameter("aDate", yesterday, ParameterType.UrlSegment);
                List<REPORTING_METRICS> YesterdaysReports = serviceCaller.Execute<List<REPORTING_METRICS>>(request);

                if (YesterdaysReports != null)
                {
                    if (YesterdaysReports.Count >= 1)
                    {
                        List<ReportingModel> YestrepModelList = new List<ReportingModel>();
                        List<List<ReportingModel>> YeseventGroupedReps = new List<List<ReportingModel>>();
                        foreach (REPORTING_METRICS rm in YesterdaysReports)
                        {
                            ReportingModel rep = new ReportingModel();
                            rep.aReport = rm;
                            request = new RestRequest();
                            request.Resource = "/Events/{entityId}";
                            request.RootElement = "EVENT";
                            request.AddParameter("entityId", rm.EVENT_ID, ParameterType.UrlSegment);
                            rep.anEvent = serviceCaller.Execute<EVENT>(request);

                            YestrepModelList.Add(rep);
                            YeseventGroupedReps = YestrepModelList.GroupBy(e => e.aReport.EVENT_ID).Select(grp => grp.ToList()).ToList();
                        }
                        ViewData["YesterdayReports"] = YeseventGroupedReps;
                    }
                }
                return View();
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        public PartialViewResult GetTodayReports()
        {
            //get Today and Yesterday's states that have Created Reports
            string today = DateTime.Now.Date.ToShortDateString().ToString();

            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/ReportingMetrics/ReportsByDate?Date={aDate}";
            request.RootElement = "ArrayOfREPORTING_METRICS";
            request.AddParameter("aDate", today, ParameterType.UrlSegment);
            List<REPORTING_METRICS> TodaysReports = serviceCaller.Execute<List<REPORTING_METRICS>>(request);
            if (TodaysReports != null)
            {
                if (TodaysReports.Count >= 1)
                {
                    List<ReportingModel> TodayrepModelList = new List<ReportingModel>();
                    List<List<ReportingModel>> TodeventGroupedReps = new List<List<ReportingModel>>();
                    foreach (REPORTING_METRICS rm in TodaysReports)
                    {
                        ReportingModel rep = new ReportingModel();
                        rep.aReport = rm;

                        request = new RestRequest();
                        request.Resource = "/Events/{entityId}";
                        request.RootElement = "EVENT";
                        request.AddParameter("entityId", rm.EVENT_ID, ParameterType.UrlSegment);
                        rep.anEvent = serviceCaller.Execute<EVENT>(request);

                        TodayrepModelList.Add(rep);
                        TodeventGroupedReps = TodayrepModelList.GroupBy(e => e.aReport.EVENT_ID).Select(grp => grp.ToList()).ToList();
                    }
                    ViewData["TodayReports"] = TodeventGroupedReps;
                }
            }
            return PartialView("TodayReportsPV");
        }

//        [HandleError]
        public PartialViewResult ReportCreate()
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //get events
                ViewData["EventList"] = getAllEvents();

                //get states
                ViewData["StateList"] = getAllStates();

                //get member logged in info
                request = new RestRequest();
                request.Resource = "/Members?username={userName}";
                request.RootElement = "MEMBER";
                request.AddParameter("userName", User.Identity.Name, ParameterType.UrlSegment);
                MEMBER thisMember = serviceCaller.Execute<MEMBER>(request);
                if (thisMember.ROLE_ID == 1) { ViewData["Role"] = "Admin"; }
                if (thisMember.ROLE_ID == 2) { ViewData["Role"] = "Manager"; }
                if (thisMember.ROLE_ID == 3) { ViewData["Role"] = "Field"; }

                //get agency info for address
                request = new RestRequest();
                request.Resource = "/Agencies/{entityId}";
                request.RootElement = "AGENCY";
                request.AddParameter("entityId", thisMember.AGENCY_ID, ParameterType.UrlSegment);
                AGENCY thisAg = serviceCaller.Execute<AGENCY>(request);

                //populate the model for the logged in member (reporter)
                ReportMember Reporter = new ReportMember();
                Reporter.MemberID = thisMember.MEMBER_ID;
                Reporter.MemberName = thisMember.FNAME + " " + thisMember.LNAME;
                Reporter.Phone = thisMember.PHONE;
                Reporter.Email = thisMember.EMAIL;
                Reporter.OfficeName = thisAg.AGENCY_NAME;
                Reporter.OfficeAddress1 = thisAg.ADDRESS + ", " + thisAg.CITY + " " + thisAg.STATE + " " + thisAg.ZIP;
                ViewData["Submitter"] = Reporter;
                
                return PartialView("ReportCreatePV");
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        public PartialViewResult ReportGenerate()
        {
            try
            {
                ViewBag.CurrentPage = "REPORTING";

                //go see if there are any incomplete reports for this Member
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //get states       
                List<STATES> states = getAllStates();
               // states.Insert(0, "All States");
                ViewData["StateList"] = states;

                //get events
                ViewData["EventList"] = getAllEvents();

                //get Today and Yesterday's states that have Created Reports
                string today = DateTime.Now.Date.ToShortDateString().ToString();

                request.Resource = "/ReportingMetrics/ReportsByDate?Date={aDate}";
                request.RootElement = "ArrayOfREPORTING_METRICS";
                request.AddParameter("aDate", today, ParameterType.UrlSegment);
                List<REPORTING_METRICS> TodaysReports = serviceCaller.Execute<List<REPORTING_METRICS>>(request);

                if (TodaysReports != null)
                {
                    if (TodaysReports.Count >= 1)
                    {
                        List<ReportingModel> TodayrepModelList = new List<ReportingModel>();
                        List<List<ReportingModel>> TodeventGroupedReps = new List<List<ReportingModel>>();
                        foreach (REPORTING_METRICS rm in TodaysReports)
                        {
                            ReportingModel rep = new ReportingModel();
                            rep.aReport = rm;
                            request = new RestRequest();
                            request.Resource = "/Events/{eventId}";
                            request.RootElement = "EVENT";
                            request.AddParameter("eventId", rm.EVENT_ID, ParameterType.UrlSegment);
                            rep.anEvent = serviceCaller.Execute<EVENT>(request);

                            TodayrepModelList.Add(rep);
                            TodeventGroupedReps = TodayrepModelList.GroupBy(e => e.aReport.EVENT_ID).Select(grp => grp.ToList()).ToList();
                        }
                        ViewData["TodayReports"] = TodeventGroupedReps;
                    }
                }
                string yesterday = DateTime.Now.AddDays(-1).Date.ToString();

                request = new RestRequest();
                request.Resource = "/ReportingMetrics/ReportsByDate?Date={aDate}";
                request.RootElement = "ArrayOfREPORTING_METRICS";
                request.AddParameter("aDate", yesterday, ParameterType.UrlSegment);
                List<REPORTING_METRICS> YesterdaysReports = serviceCaller.Execute<List<REPORTING_METRICS>>(request);

                if (YesterdaysReports != null)
                {
                    if (YesterdaysReports.Count >= 1)
                    {
                        List<ReportingModel> YestrepModelList = new List<ReportingModel>();
                        List<List<ReportingModel>> YeseventGroupedReps = new List<List<ReportingModel>>();
                        foreach (REPORTING_METRICS rm in TodaysReports)
                        {
                            ReportingModel rep = new ReportingModel();
                            rep.aReport = rm;
                            request = new RestRequest();
                            request.Resource = "/Events/{eventId}";
                            request.RootElement = "EVENT";
                            request.AddParameter("eventId", rm.EVENT_ID, ParameterType.UrlSegment);
                            rep.anEvent = serviceCaller.Execute<EVENT>(request);

                            YestrepModelList.Add(rep);
                            YeseventGroupedReps = YestrepModelList.GroupBy(e => e.aReport.EVENT_ID).Select(grp => grp.ToList()).ToList();
                        }
                        ViewData["YesterdayReports"] = YeseventGroupedReps;

                    }
                }
                return PartialView("ReportGeneratePV");
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        [HttpPost]
        public ActionResult CreateReport(ReportingModel thisReport)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                REPORTING_METRICS aReport = thisReport.aReport;
                CONTACT deplCont = thisReport.DeplContact;
                CONTACT generalCont = thisReport.GenContact;
                CONTACT inlandCont = thisReport.InlandContact;
                CONTACT coastCont = thisReport.CoastContact;
                CONTACT waterCont = thisReport.WaterContact;

                REPORTING_METRICS ReportMetric;

                if (aReport.COMPLETE == null)
                    aReport.COMPLETE = 0;

                //POST
                request = new RestRequest(Method.POST);
                request.Resource = "/ReportingMetrics";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<REPORTING_METRICS>(aReport), ParameterType.RequestBody);
                ReportMetric = serviceCaller.Execute<REPORTING_METRICS>(request);


                //post the Deploy Contact and ReportContact
                if (deplCont.FNAME != null)
                {
                    decimal contactTypeId = GetContactType("Deployed Staff");
                    CONTACT createdDeployContact = PostContactForReport(contactTypeId, ReportMetric.REPORTING_METRICS_ID, deplCont);
                }

                //post the General Contact and ReportContact
                if (generalCont.FNAME != null)
                {
                    decimal contactTypeId = GetContactType("General");
                    CONTACT createdGeneralContact = PostContactForReport(contactTypeId, ReportMetric.REPORTING_METRICS_ID, generalCont);
                }

                //post the Contact
                if (inlandCont.FNAME != null)
                {
                    decimal contactTypeId = GetContactType("Inland Flood");
                    CONTACT createdInlandContact = PostContactForReport(contactTypeId, ReportMetric.REPORTING_METRICS_ID, inlandCont);
                }

                //post the Contact
                if (coastCont.FNAME != null)
                {
                    decimal contactTypeId = GetContactType("Coastal Flood");
                    CONTACT createdCoastContact = PostContactForReport(contactTypeId, ReportMetric.REPORTING_METRICS_ID, coastCont);
                }

                //post the Contact
                if (waterCont.FNAME != null)
                {
                    decimal contactTypeId = GetContactType("Water Quality");
                    CONTACT createdWaterContact = PostContactForReport(contactTypeId, ReportMetric.REPORTING_METRICS_ID, waterCont);
                }

                return RedirectToAction("ReportDetails", new { reportId = ReportMetric.REPORTING_METRICS_ID });
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //details page for a newly created report
        public PartialViewResult ReportDetails(int reportId, string StateDetail)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                ReportingModel aReportModel = new ReportingModel();
                //get report
                request.Resource = "/ReportingMetrics/{entityId}";
                request.RootElement = "REPORTING_METRICS";
                request.AddParameter("entityId", reportId, ParameterType.UrlSegment);
                REPORTING_METRICS thisReport = serviceCaller.Execute<REPORTING_METRICS>(request);

                aReportModel.aReport = thisReport;

                //get the event name
                request = new RestRequest();
                request.Resource = "/Events/{entityId}";
                request.RootElement = "EVENT";
                request.AddParameter("entityId", thisReport.EVENT_ID, ParameterType.UrlSegment);
                ViewData["EventName"] = serviceCaller.Execute<EVENT>(request).EVENT_NAME;

                //get contacts by each type
                request = new RestRequest();
                request.Resource = "/Members/{entityId}";
                request.RootElement = "MEMBER";
                request.AddParameter("entityId", thisReport.MEMBER_ID, ParameterType.UrlSegment);
                MEMBER thisMember = serviceCaller.Execute<MEMBER>(request);

                //get agency info for address
                request = new RestRequest();
                request.Resource = "/Agencies/{entityId}";
                request.RootElement = "AGENCY";
                request.AddParameter("entityId", thisMember.AGENCY_ID, ParameterType.UrlSegment);
                AGENCY thisAg = serviceCaller.Execute<AGENCY>(request);

                //populate the model for the logged in member (reporter)
                ReportMember Reporter = new ReportMember();
                Reporter.MemberID = thisMember.MEMBER_ID;
                Reporter.MemberName = thisMember.FNAME + " " + thisMember.LNAME;
                Reporter.Phone = thisMember.PHONE;
                Reporter.Email = thisMember.EMAIL;
                Reporter.OfficeName = thisAg.AGENCY_NAME;
                Reporter.OfficeAddress1 = thisAg.ADDRESS + ", " + thisAg.CITY + " " + thisAg.STATE + " " + thisAg.ZIP;
                ViewData["Submitter"] = Reporter;

                //get the contacts
                aReportModel.DeplContact = GetContactByReportIDandType(thisReport.REPORTING_METRICS_ID, "Deployed Staff");
                aReportModel.GenContact = GetContactByReportIDandType(thisReport.REPORTING_METRICS_ID, "General");
                aReportModel.InlandContact = GetContactByReportIDandType(thisReport.REPORTING_METRICS_ID, "Inland Flood");
                aReportModel.CoastContact = GetContactByReportIDandType(thisReport.REPORTING_METRICS_ID, "Coastal Flood");
                aReportModel.WaterContact = GetContactByReportIDandType(thisReport.REPORTING_METRICS_ID, "Water Quality");

                if (StateDetail == "true")
                {
                    return PartialView("StateReportDetailsPV", aReportModel);
                }
                else
                {
                    return PartialView("ReportDetailsPV", aReportModel);
                }
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }
        
        private decimal GetContactType(string typeName)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/ContactTypes";
            request.RootElement = "ArrayOfCONTACT_TYPE";
            List<CONTACT_TYPE> types = serviceCaller.Execute<List<CONTACT_TYPE>>(request);

            decimal typeID = types.FirstOrDefault(a => a.TYPE.ToUpper() == typeName.ToUpper()).CONTACT_TYPE_ID;
            return typeID;
        }

        //called when Populate Totals is clicked
        public JsonResult GetYestReportTotals(string date, int eventID, string stateAbbrev)
        {
            try
            {
                //want matching report with event id and state to get personnel and contacts populated for yesterday thisDate.AddDays(-1)
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "ReportingMetrics?Event={eventId}&State={stateName}";
                request.RootElement = "ArrayOfREPORTING_METRICS";
                request.AddParameter("eventId", eventID, ParameterType.UrlSegment);
                request.AddParameter("stateName", stateAbbrev, ParameterType.UrlSegment);
                List<REPORTING_METRICS> reportList = serviceCaller.Execute<List<REPORTING_METRICS>>(request);

                DateTime thisDate = Convert.ToDateTime(date);
                REPORTING_METRICS thisReport = reportList.FirstOrDefault(x => x.REPORT_DATE.Date == thisDate.AddDays(-1).Date && x.COMPLETE == 1);

                ReportingModel thisReportModel = new ReportingModel();
                thisReportModel.aReport = thisReport;
                if (thisReport != null)
                {
                    //get each contacts for this Report and the contactType
                    thisReportModel.DeplContact = GetContactByReportIDandType(thisReport.REPORTING_METRICS_ID, "Deployed Staff");
                    thisReportModel.GenContact = GetContactByReportIDandType(thisReport.REPORTING_METRICS_ID, "General");
                    thisReportModel.InlandContact = GetContactByReportIDandType(thisReport.REPORTING_METRICS_ID, "Inland Flood");
                    thisReportModel.CoastContact = GetContactByReportIDandType(thisReport.REPORTING_METRICS_ID, "Coastal Flood");
                    thisReportModel.WaterContact = GetContactByReportIDandType(thisReport.REPORTING_METRICS_ID, "Water Quality");
                }
                return Json(thisReportModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(e.ToString(), JsonRequestBehavior.AllowGet);
            }
        }

        //called when user clicks an incomplete report date
        public ActionResult GetThisReport(int reportID)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "/ReportingMetrics/{entityId}";
                request.RootElement = "REPORTING_METRICS";
                request.AddParameter("entityId", reportID, ParameterType.UrlSegment);
                REPORTING_METRICS thisReport = serviceCaller.Execute<REPORTING_METRICS>(request);

                ReportingModel thisReportModel = new ReportingModel();
                thisReportModel.aReport = thisReport;
                if (thisReport != null)
                {
                    //get each contacts for this Report and the contactType
                    thisReportModel.DeplContact = GetContactByReportIDandType(thisReport.REPORTING_METRICS_ID, "Deployed Staff");
                    thisReportModel.GenContact = GetContactByReportIDandType(thisReport.REPORTING_METRICS_ID, "General");
                    thisReportModel.InlandContact = GetContactByReportIDandType(thisReport.REPORTING_METRICS_ID, "Inland Flood");
                    thisReportModel.CoastContact = GetContactByReportIDandType(thisReport.REPORTING_METRICS_ID, "Coastal Flood");
                    thisReportModel.WaterContact = GetContactByReportIDandType(thisReport.REPORTING_METRICS_ID, "Water Quality");
                }
                ViewData["EventList"] = getAllEvents();
                ViewData["StateList"] = getAllStates();

                //get member logged in info
                request = new RestRequest();
                request.Resource = "/Members?username={userName}";
                request.RootElement = "MEMBER";
                request.AddParameter("userName", User.Identity.Name, ParameterType.UrlSegment);
                MEMBER thisMember = serviceCaller.Execute<MEMBER>(request);
                if (thisMember.ROLE_ID == 1) { ViewData["Role"] = "Admin"; }
                if (thisMember.ROLE_ID == 2) { ViewData["Role"] = "Manager"; }
                if (thisMember.ROLE_ID == 3) { ViewData["Role"] = "Field"; }

                //get agency info for address
                request = new RestRequest();
                request.Resource = "/Agencies/{entityId}";
                request.RootElement = "AGENCY";
                request.AddParameter("entityId", thisMember.AGENCY_ID, ParameterType.UrlSegment);
                AGENCY thisAg = serviceCaller.Execute<AGENCY>(request);

                //populate the model for the logged in member (reporter)
                ReportMember Reporter = new ReportMember();
                Reporter.MemberID = thisMember.MEMBER_ID;
                Reporter.MemberName = thisMember.FNAME + " " + thisMember.LNAME;
                Reporter.Phone = thisMember.PHONE;
                Reporter.Email = thisMember.EMAIL;
                Reporter.OfficeName = thisAg.AGENCY_NAME;
                Reporter.OfficeAddress1 = thisAg.ADDRESS + ", " + thisAg.CITY + " " + thisAg.STATE + " " + thisAg.ZIP;
                ViewData["Submitter"] = Reporter;
                return PartialView("ReportUpdatePV", thisReportModel);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        [HttpPost]
        public ActionResult UpdateReport(ReportingModel thisReport)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                REPORTING_METRICS aReport = thisReport.aReport;
                CONTACT deplCont = thisReport.DeplContact;
                CONTACT generalCont = thisReport.GenContact;
                CONTACT inlandCont = thisReport.InlandContact;
                CONTACT coastCont = thisReport.CoastContact;
                CONTACT waterCont = thisReport.WaterContact;

                REPORTING_METRICS ReportMetric;

                if (aReport.COMPLETE == null)
                    aReport.COMPLETE = 0;

                //PUT
                request.Resource = "ReportingMetrics/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", aReport.REPORTING_METRICS_ID, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<REPORTING_METRICS>(aReport), ParameterType.RequestBody);
                ReportMetric = serviceCaller.Execute<REPORTING_METRICS>(request);

                //get all the contacts for this report and then remove them in case one or more were edited
                request = new RestRequest();
                request.Resource = "/Contacts?ReportMetric={reportMetricsId}";
                request.RootElement = "ArrayOfCONTACT";
                request.AddParameter("reportMetricsId", aReport.REPORTING_METRICS_ID, ParameterType.UrlSegment);
                List<CONTACT> reportContacts = serviceCaller.Execute<List<CONTACT>>(request);

                if (reportContacts != null)
                {
                    foreach (CONTACT c in reportContacts)
                    {
                        request = new RestRequest(Method.POST);
                        request.Resource = "/Contacts/{reportMetricsId}/removeReportContact";
                        request.AddParameter("reportMetricsId", aReport.REPORTING_METRICS_ID, ParameterType.UrlSegment);
                        request.AddHeader("X-HTTP-Method-Override", "DELETE");
                        request.AddHeader("Content-Type", "application/xml");
                        request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                        request.AddBody(c);
                        serviceCaller.Execute<CONTACT>(request);
                    }
                }

                //post the Deploy Contact and ReportContact
                if (deplCont.FNAME != null)
                {
                    decimal contactTypeId = GetContactType("Deployed Staff");
                    CONTACT createdDeployContact = PostContactForReport(contactTypeId, ReportMetric.REPORTING_METRICS_ID, deplCont);
                }

                //post the General Contact and ReportContact
                if (generalCont.FNAME != null)
                {
                    decimal contactTypeId = GetContactType("General");
                    CONTACT createdGeneralContact = PostContactForReport(contactTypeId, ReportMetric.REPORTING_METRICS_ID, generalCont);
                }

                //post the Contact
                if (inlandCont.FNAME != null)
                {
                    decimal contactTypeId = GetContactType("Inland Flood");
                    CONTACT createdInlandContact = PostContactForReport(contactTypeId, ReportMetric.REPORTING_METRICS_ID, inlandCont);
                }

                //post the Contact
                if (coastCont.FNAME != null)
                {
                    decimal contactTypeId = GetContactType("Coastal Flood");
                    CONTACT createdCoastContact = PostContactForReport(contactTypeId, ReportMetric.REPORTING_METRICS_ID, coastCont);
                }

                //post the Contact
                if (waterCont.FNAME != null)
                {
                    decimal contactTypeId = GetContactType("Water Quality");
                    CONTACT createdWaterContact = PostContactForReport(contactTypeId, ReportMetric.REPORTING_METRICS_ID, waterCont);
                }

                return RedirectToAction("ReportDetails", new { reportId = ReportMetric.REPORTING_METRICS_ID });
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //called when user clicks Populate Totals after returning the report, gets all the instruments and hwm totals
        public JsonResult GetDailyReportTotals(string date, int eventID, string stateAbbrev)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "/ReportingMetrics/DailyReportTotals?Date={date}&Event={eventId}&State={stateName}";
                request.RootElement = "REPORTING_METRICS";
                request.AddParameter("date", date, ParameterType.UrlSegment);
                request.AddParameter("eventId", eventID, ParameterType.UrlSegment);
                request.AddParameter("stateName", stateAbbrev, ParameterType.UrlSegment);
                REPORTING_METRICS sensorHWMtotals = serviceCaller.Execute<REPORTING_METRICS>(request);

                return Json(sensorHWMtotals, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(e.ToString(), JsonRequestBehavior.AllowGet);
            }
        }

        public PartialViewResult ReportByDate(string date)
        {
            try
            {
                DateTime today = Convert.ToDateTime(date).Date;
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/ReportingMetrics/ReportsByDate?Date={aDate}";
                request.RootElement = "ArrayOfREPORTING_METRICS";
                request.AddParameter("aDate", today, ParameterType.UrlSegment);
                List<REPORTING_METRICS> reports = serviceCaller.Execute<List<REPORTING_METRICS>>(request);
                if (reports != null)
                {
                    if (reports.Count >= 1)
                    {
                        List<ReportingModel> repModelList = new List<ReportingModel>();
                        List<List<ReportingModel>> eventGroupedReps = new List<List<ReportingModel>>();
                        foreach (REPORTING_METRICS rm in reports)
                        {
                            ReportingModel rep = new ReportingModel();
                            rep.aReport = rm;
                            request = new RestRequest();
                            request.Resource = "/Events/{entityId}";
                            request.RootElement = "EVENT";
                            request.AddParameter("entityId", rm.EVENT_ID, ParameterType.UrlSegment);
                            rep.anEvent = serviceCaller.Execute<EVENT>(request);
                            repModelList.Add(rep);
                            eventGroupedReps = repModelList.GroupBy(e => e.aReport.EVENT_ID).Select(grp => grp.ToList()).ToList();
                        }
                        ViewData["Reports"] = eventGroupedReps;
                    }
                }
                return PartialView("StateReportsbyDate");
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        public ActionResult ProjectAlert(int reportId)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "/ReportingMetrics/{entityId}";
                request.RootElement = "REPORTING_METRICS";
                request.AddParameter("entityId", reportId, ParameterType.UrlSegment);
                REPORTING_METRICS thisReport = serviceCaller.Execute<REPORTING_METRICS>(request);

                ViewData["Report"] = thisReport;
                ViewData["YesterdayField"] = thisReport.SW_YEST_FIELDPERS + thisReport.WQ_YEST_FIELDPERS;
                ViewData["YesterdayOffice"] = thisReport.SW_YEST_OFFICEPERS + thisReport.WQ_YEST_OFFICEPERS;
                ViewData["MeasurementCts"] = thisReport.TOT_CHECK_MEAS + thisReport.TOT_DISCHARGE_MEAS;

                //get total states responding to this event
                request = new RestRequest();
                request.Resource = "/Events/{eventId}/Reports";
                request.RootElement = "ArrayOfREPORTING_METRICS";
                request.AddParameter("eventId", thisReport.EVENT_ID, ParameterType.UrlSegment);
                List<REPORTING_METRICS> EventReps = serviceCaller.Execute<List<REPORTING_METRICS>>(request);

                ViewData["StateCount"] = EventReps.GroupBy(s => s.STATE).Select(st => st.FirstOrDefault()).Count();

                request = new RestRequest();
                request.Resource = "/Events/{entityId}";
                request.RootElement = "EVENT";
                request.AddParameter("entityId", thisReport.EVENT_ID, ParameterType.UrlSegment);
                EVENT thisEvent = serviceCaller.Execute<EVENT>(request);

                ViewData["Event"] = thisEvent;

                if (thisEvent.EVENT_TYPE_ID == 1)
                {
                    return PartialView("FloodEventPA");
                }
                else
                {
                    return PartialView("HurricaneEventPA");
                }
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        [HttpPost]
        public ActionResult GenerateReport(FormCollection fc)
        {
            try
            {
                int eventId = Convert.ToInt32(fc["EventChosen"]);
                string aDate = fc["QueryDate"];
                string stateNames = fc["StateValue"];
                string buttonClicked = fc["Generate"];

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/ReportingMetrics/FilteredReports?Event={eventId}&States={stateNames}&Date={aDate}";
                request.RootElement = "ArrayOfREPORTING_METRICS";
                request.AddParameter("eventId", eventId, ParameterType.UrlSegment);
                request.AddParameter("stateNames", stateNames, ParameterType.UrlSegment);
                request.AddParameter("aDate", aDate, ParameterType.UrlSegment);
                List<REPORTING_METRICS> reportList = serviceCaller.Execute<List<REPORTING_METRICS>>(request);

                List<ReportingModel> allReports = new List<ReportingModel>();

                if (reportList != null)
                {
                    if (reportList.Count >= 1)
                    {
                        //if buttonClicked == "Display Metrics Summary" ...get all the reports for this event to populate Yesterday People totals
                        //and add each report to the list of reportModel to pass to GeneratedReportsPV view
                        if (buttonClicked == "Display Metrics Summary")
                        {
                            foreach (REPORTING_METRICS rp in reportList)
                            {
                                //get all reports for state and event combo in order to populate cumulative people days
                                request = new RestRequest();
                                request.Resource = "/ReportingMetrics?Event={eventId}&State={stateName}";
                                request.RootElement = "REPORTING_METRICS";
                                request.AddParameter("eventId", eventId, ParameterType.UrlSegment);
                                request.AddParameter("stateName", rp.STATE, ParameterType.UrlSegment);
                                List<REPORTING_METRICS> eventReports = serviceCaller.Execute<List<REPORTING_METRICS>>(request);

                                //have all the reports for this state for this event, make a ReportModel with the values
                                ReportingModel aReport = new ReportingModel();
                                aReport.aReport = rp;
                                aReport.FieldPers_YesterdaySWTotals = eventReports.Sum(er => er.SW_YEST_FIELDPERS);
                                aReport.FieldPers_YesterdayWQTotals = eventReports.Sum(er => er.WQ_YEST_FIELDPERS);
                                aReport.OfficePers_YesterdaySWTotals = eventReports.Sum(er => er.SW_YEST_OFFICEPERS);
                                aReport.OfficePers_YesterdayWQTotals = eventReports.Sum(er => er.WQ_YEST_OFFICEPERS);

                                allReports.Add(aReport);
                            }
                        }
                        else
                        {
                            //else buttonClicked == "Display Contacts Summary" ... for each of these reports, get the contacts
                            //and add each report with the contacts to a list of reportModel to pass to GeneratedContactsPV view
                            foreach (REPORTING_METRICS rp in reportList)
                            {
                                ReportingModel aReport = new ReportingModel();
                                aReport.aReport = rp;
                                //get submitter
                                request = new RestRequest();
                                request.Resource = "Members/{entityId}";
                                request.RootElement = "MEMBER";
                                request.AddParameter("entityId", rp.MEMBER_ID, ParameterType.UrlSegment);
                                MEMBER submit = serviceCaller.Execute<MEMBER>(request);

                                //get submitter's agency
                                request = new RestRequest();
                                request.Resource = "Agencies/{entityId}";
                                request.RootElement = "AGENCY";
                                request.AddParameter("entityId", submit.AGENCY_ID, ParameterType.UrlSegment);
                                AGENCY thisAg = serviceCaller.Execute<AGENCY>(request);

                                //add info to model
                                aReport.submitter = new ReportMember();
                                aReport.submitter.MemberID = submit.MEMBER_ID;
                                aReport.submitter.MemberName = submit.FNAME + " " + submit.LNAME;
                                aReport.submitter.OfficeName = thisAg.AGENCY_NAME;
                                aReport.submitter.OfficeAddress1 = thisAg.ADDRESS + ", ";
                                aReport.submitter.OfficeAddress2 = thisAg.CITY + " " + thisAg.STATE + " " + thisAg.ZIP;
                                aReport.submitter.Email = submit.EMAIL;
                                aReport.submitter.Phone = submit.PHONE;

                                //get all the contacts
                                aReport.DeplContact = GetContactByReportIDandType(rp.REPORTING_METRICS_ID, "Deployed Staff");
                                aReport.GenContact = GetContactByReportIDandType(rp.REPORTING_METRICS_ID, "General");
                                aReport.InlandContact = GetContactByReportIDandType(rp.REPORTING_METRICS_ID, "Inland Flood");
                                aReport.CoastContact = GetContactByReportIDandType(rp.REPORTING_METRICS_ID, "Coastal Flood");
                                aReport.WaterContact = GetContactByReportIDandType(rp.REPORTING_METRICS_ID, "Water Quality");

                                allReports.Add(aReport);
                            }
                        }
                        ViewData["ReportList"] = allReports;
                    }
                }
                //get the event to populate info on the generated page
                request = new RestRequest();
                request.Resource = "Events/{entityId}";
                request.RootElement = "EVENT";
                request.AddParameter("entityId", eventId, ParameterType.UrlSegment);
                EVENT thisEvent = serviceCaller.Execute<EVENT>(request);

                if (thisEvent != null)
                {
                    ViewData["Event"] = thisEvent;

                    //get event type
                    request = new RestRequest();
                    request.Resource = "Events/{eventId}/Type";
                    request.RootElement = "EVENT_TYPE";
                    request.AddParameter("eventId", eventId, ParameterType.UrlSegment);
                    EVENT_TYPE evType = serviceCaller.Execute<EVENT_TYPE>(request);
                    if (evType != null)
                        ViewData["EventType"] = evType.TYPE;

                    //get event status
                    request = new RestRequest();
                    request.Resource = "Events/{eventId}/Status";
                    request.RootElement = "EVENT_STATUS";
                    request.AddParameter("eventId", eventId, ParameterType.UrlSegment);
                    EVENT_STATUS evStat = serviceCaller.Execute<EVENT_STATUS>(request);
                    if (evStat != null)
                        ViewData["EventStatus"] = evStat.STATUS;

                    //get event coordinator
                    request = new RestRequest();
                    request.Resource = "Members/{entityId}";
                    request.RootElement = "MEMBER";
                    request.AddParameter("entityId", thisEvent.EVENT_COORDINATOR, ParameterType.UrlSegment);
                    MEMBER evMember = serviceCaller.Execute<MEMBER>(request);

                    if (evMember != null)
                    {
                        //get member's agency info
                        request = new RestRequest();
                        request.Resource = "Agencies/{entityId}";
                        request.RootElement = "AGENCY";
                        request.AddParameter("entityId", evMember.AGENCY_ID, ParameterType.UrlSegment);
                        AGENCY membAgency = serviceCaller.Execute<AGENCY>(request);

                        //build the eventCoord to pass to view
                        ReportMember EventCoord = new ReportMember();
                        EventCoord.MemberID = evMember.MEMBER_ID;
                        EventCoord.MemberName = evMember.FNAME + " " + evMember.LNAME;
                        EventCoord.OfficeName = membAgency.AGENCY_NAME;
                        EventCoord.Email = evMember.EMAIL;

                        ViewData["EventCoord"] = EventCoord;
                    }
                }

                //which button they clicks determines which page they get back
                if (buttonClicked == "Display Metrics Summary")
                {

                    return PartialView("GeneratedReportsPV");

                }
                else if (buttonClicked == "Generate CSV Summary")
                {
                    StringBuilder sb = csvExport(allReports);

                    return File(new System.Text.UTF8Encoding().GetBytes(sb.ToString()), "text/csv", "PR_" + aDate + ".csv");
                }
                else
                {
                    return PartialView("GeneratedContactsPV");
                }
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        private CONTACT GetContactByReportIDandType(decimal reportId, string contactType)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Contacts?ReportMetric={reportMetricsId}&ContactType={contactTypeId}";
            request.RootElement = "CONTACT";
            request.AddParameter("reportMetricsId", reportId, ParameterType.UrlSegment);
            request.AddParameter("contactTypeId", GetContactType(contactType), ParameterType.UrlSegment);
            CONTACT thisContact = serviceCaller.Execute<CONTACT>(request);
            return thisContact;
        }

        private CONTACT PostContactForReport(decimal contactTypeId, decimal reportId, CONTACT thisContact)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "Contacts/AddReportContact?contactTypeId={ContactTypeId}&reportId={ReportId}";
            request.AddParameter("ContactTypeId", contactTypeId, ParameterType.UrlSegment);
            request.AddParameter("ReportId", reportId, ParameterType.UrlSegment);
            request.RequestFormat = DataFormat.Xml;
            request.AddHeader("Content-Type", "application/xml");
            STNWebSerializer serializer = new STNWebSerializer();
            request.AddParameter("application/xml", serializer.Serialize<CONTACT>(thisContact), ParameterType.RequestBody);
            CONTACT createdContact = serviceCaller.Execute<CONTACT>(request);
            return createdContact;
        }

        private List<EVENT> getAllEvents()
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Events/";
            request.RootElement = "ArrayOfEVENT";
            List<EVENT> allEvents = serviceCaller.Execute<List<EVENT>>(request);
            return allEvents;
        }

        private List<STATES> getAllStates()
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/States";
            request.RootElement = "ArrayOfSTATES";
            List<STATES> stateList = serviceCaller.Execute<List<STATES>>(request);
            
            return stateList;
        }

        private StringBuilder csvExport(List<ReportingModel> allReports)
        {
            StringBuilder sb = new StringBuilder();

            if (allReports.Count >= 1)
            {
                string header1 = "Deployed Personnel And Assets";
                sb.AppendLine(header1);

                sb.AppendLine("State,Employees Not Accounted For-SW,Employees Not Accounted For-WQ,Personnel in the Field-SW (Cum),Personnel in the Field-WQ (Cum),Personnel in the Field-SW (Yesterday),Personnel in the Field-WQ (Yesterday)," +
                    "Personnel in the Field-SW (Today),Personnel in the Field-WQ (Today),Personnel in the Field-SW (Tomorrow),Personnel in the Field-WQ (Tomorrow)," +
                    "Personnel in the Office-SW (Cum),Personnel in the Office-WQ (Cum),Personnel in the Office-SW (Yesterday),Personnel in the Office-WQ (Yesterday),Personnel in the Office-SW (Today),Personnel in the Office-WQ (Today)," +
                    "Personnel in the Office-SW (Tomorrow),Personnel in the Office-WQ (Tomorrow),Vehicles Deployed-SW (Trucks & Cars),Vehicles Deployed-WQ (Trucks & Cars),Vehicles Deployed-SW (Boats),Vehicles Deployed-WQ (Boats),Vehicles Deployed-SW (Other),Vehicles Deployed-WQ (Other)");
                foreach (var a in allReports)
                {
                    REPORTING_METRICS thisOne = a.aReport;
                    sb.AppendLine(thisOne.STATE + "," + thisOne.SW_FIELDPERS_NOTACCT.ToString() + "," + thisOne.WQ_FIELDPERS_NOTACCT.ToString() + "," + a.FieldPers_YesterdaySWTotals.ToString() + "," + a.FieldPers_YesterdayWQTotals.ToString() + "," +
                        thisOne.SW_YEST_FIELDPERS.ToString() + "," + thisOne.WQ_YEST_FIELDPERS.ToString() + "," + thisOne.SW_TOD_FIELDPERS.ToString() + "," + thisOne.WQ_TOD_FIELDPERS.ToString() + "," + thisOne.SW_TMW_FIELDPERS.ToString() + "," +
                        thisOne.WQ_TMW_FIELDPERS.ToString() + "," + a.OfficePers_YesterdaySWTotals.ToString() + "," + a.OfficePers_YesterdayWQTotals.ToString() + "," + thisOne.SW_YEST_OFFICEPERS.ToString() + "," + thisOne.WQ_YEST_OFFICEPERS.ToString() + "," +
                        thisOne.SW_TOD_OFFICEPERS.ToString() + "," + thisOne.WQ_TOD_OFFICEPERS.ToString() + "," + thisOne.SW_TMW_OFFICEPERS.ToString() + "," + thisOne.WQ_TMW_OFFICEPERS.ToString() + "," + thisOne.SW_AUTOS_DEPL.ToString() + "," +
                        thisOne.WQ_AUTOS_DEPL.ToString() + "," + thisOne.SW_BOATS_DEPL.ToString() + "," + thisOne.WQ_BOATS_DEPL.ToString() + "," + thisOne.SW_OTHER_DEPL.ToString() + "," + thisOne.WQ_OTHER_DEPL.ToString());
                }

                sb.AppendLine("");

                string header2 = "Instrumentation and Sampling Counts";
                sb.AppendLine(header2);
                sb.AppendLine("State,Gage Visits,Gages Down at Present, Discharge Measurements (To Date),Discharge Measurements (Planned),Check Measurements (To Date),Check Measurements (Planned),Indirect Measurements,Rating Extentions,Peaks Of Record,QW Gage Visits,Continuous QW Gages Visited,Continuous QW Gages Down,Discrete QW Samples Collected,Sediment Samples Collected");
                foreach (var a in allReports)
                {
                    REPORTING_METRICS thisOne = a.aReport;
                    sb.AppendLine(thisOne.STATE + "," + thisOne.GAGE_VISIT.ToString() + "," + thisOne.GAGE_DOWN.ToString() + "," + thisOne.TOT_DISCHARGE_MEAS.ToString() + "," + thisOne.PLAN_DISCHARGE_MEAS.ToString() +
                        "," + thisOne.TOT_CHECK_MEAS.ToString() + "," + thisOne.PLAN_CHECK_MEAS.ToString() + "," + thisOne.PLAN_INDIRECT_MEAS.ToString() + "," + thisOne.RATING_EXTENS.ToString() + "," + thisOne.GAGE_PEAK_RECORD.ToString() +
                        "," + thisOne.QW_GAGE_VISIT.ToString() + "," + thisOne.QW_CONT_GAGEVISIT.ToString() + "," + thisOne.QW_GAGE_DOWN.ToString() + "," + thisOne.QW_DISCR_SAMPLES.ToString() + "," +
                        thisOne.COLL_SEDSAMPLES.ToString());

                }

                sb.AppendLine("");

                sb.AppendLine("State,RDP-Planned,RDP-Deployed,RDP-Recovered,RDP-Lost,Water Level Sensors-Planned,Water Level Sensors-Deployed,Water Level Sensors-Recovered,Water Level Sensors-Lost,Wave Sensors-Planned,Wave Sensors-Deployed,Wave Sensors-Recovered,Water Sensors-Lost," +
                    "Barometers-Planned,Barometers-Deployed,Barometers-Recovered,Barometers-Lost,Meterological Sensors-Planned,Meterological Sensors-Deployed,Meterological Sensors-Recovered,Meterological Sensors-Lost,HWM-Flagged,HWM-Collected");
                foreach (var a in allReports)
                {
                    REPORTING_METRICS thisOne = a.aReport;
                    sb.AppendLine(thisOne.STATE + "," + thisOne.PLAN_RAPDEPL_GAGE.ToString() + "," + thisOne.DEP_RAPDEPL_GAGE.ToString() + "," + thisOne.REC_RAPDEPL_GAGE.ToString() + "," + thisOne.LOST_RAPDEPL_GAGE.ToString() + "," +
                        thisOne.PLAN_WTRLEV_SENSOR.ToString() + "," + thisOne.DEP_WTRLEV_SENSOR.ToString() + "," + thisOne.REC_WTRLEV_SENSOR.ToString() + "," + thisOne.LOST_WTRLEV_SENSOR.ToString() + "," +
                        thisOne.PLAN_WV_SENS.ToString() + "," + thisOne.DEP_WV_SENS.ToString() + "," + thisOne.REC_WV_SENS.ToString() + "," + thisOne.LOST_WV_SENS.ToString() + "," +
                        thisOne.PLAN_BAROMETRIC.ToString() + "," + thisOne.DEP_BAROMETRIC.ToString() + "," + thisOne.REC_BAROMETRIC.ToString() + "," + thisOne.LOST_BAROMETRIC.ToString() + "," +
                        thisOne.PLAN_METEOROLOGICAL.ToString() + "," + thisOne.DEP_METEOROLOGICAL.ToString() + "," + thisOne.REC_METEOROLOGICAL.ToString() + "," + thisOne.LOST_METEOROLOGICAL.ToString() + "," +
                        thisOne.HWM_FLAGGED.ToString() + "," + thisOne.HWM_COLLECTED.ToString());
                }
            }
            else
            {
                sb.AppendLine("There are no reports for the specified query");
            }
            return sb;
        }
    }
}
