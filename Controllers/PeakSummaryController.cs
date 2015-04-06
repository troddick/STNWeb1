//------------------------------------------------------------------------------
//----- PeakSummaryController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Display a master PeakSummary page and link to individual files pages 
//
//discussion:   
//
//     
#region Comments
//10.19.12 - TR - Pulled from File Controller to make it

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
    public class PeakSummaryController : Controller
    {     

        //Details of a peak Summary using PeakSummary ID
        public PartialViewResult PeakSumDetailsPV(int pkId, string FROM, int objID)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                //get this peak summary
                request.Resource = "/PeakSummaries/{entityId}";
                request.RootElement = "PEAK_SUMMARY";
                request.AddParameter("entityId", pkId, ParameterType.UrlSegment);
                PEAK_SUMMARY peakSummary = serviceCaller.Execute<PEAK_SUMMARY>(request);
                //store where this peak is being shown (HWM or DataFile) and the Id of which ever it is
                ViewData["FROM"] = FROM;
                ViewData["objID"] = objID;

                //make sure peak date/time is in utc
                if (peakSummary.TIME_ZONE != "UTC")
                {
                    peakSummary.TIME_ZONE = "UTC";
                    peakSummary.PEAK_DATE = peakSummary.PEAK_DATE.Value.ToUniversalTime();
                }

                //get membername
                request = new RestRequest();
                request.Resource = "MEMBERS/{entityId}";
                request.RootElement = "MEMBER";
                request.AddParameter("entityId", peakSummary.MEMBER_ID, ParameterType.UrlSegment);
                ViewData["MemberUsername"] = serviceCaller.Execute<MEMBER>(request).USERNAME;

                //get datum if one
                if (peakSummary.VDATUM_ID != 0 && peakSummary.VDATUM_ID != null)
                {
                    request = new RestRequest();
                    request.Resource = "/VerticalDatums/{entityId}";
                    request.RootElement = "VERTICAL_DATUMS";
                    request.AddParameter("entityId", peakSummary.VDATUM_ID, ParameterType.UrlSegment);
                    ViewData["VDType"] = serviceCaller.Execute<VERTICAL_DATUMS>(request).DATUM_NAME;
                }
                return PartialView("PeakSumDetailsPV", peakSummary);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //Edit a peak Summary using PeakSummary ID and what page its from (HWM or FILE)
        public PartialViewResult EditPeakSumPV(int id, string FROM, string objID)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/PeakSummaries/{entityId}";
                request.RootElement = "PEAK_SUMMARY";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                PEAK_SUMMARY thisPKS = serviceCaller.Execute<PEAK_SUMMARY>(request);

                //get the members name that added this peak summary
                request = new RestRequest();
                request.Resource = "MEMBERS/{entityId}";
                request.RootElement = "MEMBER";
                request.AddParameter("entityId", thisPKS.MEMBER_ID, ParameterType.UrlSegment);
                ViewData["MemberUsername"] = serviceCaller.Execute<MEMBER>(request).USERNAME;

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

                //make sure peak time is utc
                if (thisPKS.TIME_ZONE != "UTC")
                {
                    thisPKS.TIME_ZONE = "UTC";
                    thisPKS.PEAK_DATE = thisPKS.PEAK_DATE.Value.ToUniversalTime();
                }

                //get vdatums
                request = new RestRequest();
                request.Resource = "/VerticalDatums";
                request.RootElement = "ArrayOfVERTICAL_DATUMS";
                List<VERTICAL_DATUMS> VertDatums = serviceCaller.Execute<List<VERTICAL_DATUMS>>(request);
                ViewData["VDType"] = VertDatums;
                //store where this is being shown (HWM or DataFile) and the id of whichever it is
                ViewData["FROM"] = FROM;
                ViewData["objID"] = objID;

                return PartialView("EditPeakSumPV", thisPKS);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //Edit a peak Summary using PeakSummary ID and what page its from (HWM or FILE)
        [HttpPost]
        public ActionResult EditPeakSum(int id, PeakSummaryModel PeakModel)
        {
            try
            {
                PEAK_SUMMARY editSummary = new PEAK_SUMMARY();
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                
                //store formcollection items
                editSummary.PEAK_SUMMARY_ID = PeakModel.peakID;
                editSummary.MEMBER_ID = PeakModel.memberID;
                string date = PeakModel.PeakDate; // mm/dd/yyyy
                string time = PeakModel.PeakTime; // hh:mm PM

                string datetime = date + " " + time;
                editSummary.PEAK_DATE = DateTime.Parse(datetime, System.Globalization.CultureInfo.InvariantCulture);
                editSummary.IS_PEAK_ESTIMATED = PeakModel.DateEstimated;
                editSummary.IS_PEAK_TIME_ESTIMATED = PeakModel.TimeEstimated;
                editSummary.PEAK_STAGE = PeakModel.PeakStage;
                editSummary.IS_PEAK_STAGE_ESTIMATED = PeakModel.StageEstimated;
                editSummary.PEAK_DISCHARGE = PeakModel.PeakDischarge;
                editSummary.IS_PEAK_DISCHARGE_ESTIMATED = PeakModel.DischargeEstimated;
                editSummary.VDATUM_ID = PeakModel.VdatumID;
                editSummary.HEIGHT_ABOVE_GND = PeakModel.HeightAboveGround;
                editSummary.IS_HAG_ESTIMATED = PeakModel.HAGEstimated;
                editSummary.TIME_ZONE = PeakModel.TimeZone;

                //make sure it's UTC
                if (editSummary.TIME_ZONE != "UTC")
                {
                    editSummary.TIME_ZONE = "UTC";
                    editSummary.PEAK_DATE = editSummary.PEAK_DATE.Value.ToUniversalTime();
                }

                var request = new RestRequest(Method.POST);
                request.Resource = "/PeakSummaries/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<PEAK_SUMMARY>(editSummary), ParameterType.RequestBody);
                PEAK_SUMMARY updatedPeak = serviceCaller.Execute<PEAK_SUMMARY>(request);

                //store for details view
                request = new RestRequest();
                request.Resource = "/Members/{entityId}";
                request.RootElement = "MEMBER";
                request.AddParameter("entityId", PeakModel.memberID, ParameterType.UrlSegment);
                ViewData["MemberUsername"] = serviceCaller.Execute<MEMBER>(request).USERNAME;

                request = new RestRequest();
                request.Resource = "/VerticalDatums/{entityId}";
                request.RootElement = "VERTICAL_DATUMS";
                request.AddParameter("entityId", PeakModel.VdatumID, ParameterType.UrlSegment);
                ViewData["VDType"] = serviceCaller.Execute<VERTICAL_DATUMS>(request).DATUM_NAME;

                return PartialView("PeakSumDetailsPV", updatedPeak);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //goes to Create page for Peak Summary from DataFile Edit page, returns id to details page
        //to repopulate peaksummary info box
        public ViewResult CreatePeakSumForm(int id, string FROM)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/VerticalDatums";
                request.RootElement = "ArrayOfVERTICAL_DATUMS";
                List<VERTICAL_DATUMS> VertDatums = serviceCaller.Execute<List<VERTICAL_DATUMS>>(request);
                ViewData["VerticalDatums"] = VertDatums;
                //pass the object id to the view to carry through to post along with fromPage
                ViewData["FROM"] = FROM;
                ViewData["ObjectID"] = id;

                ///////////////time zone idea////////////////////
                System.Collections.ObjectModel.ReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();
                List<string> USAtimeZones = new List<string>();
                foreach (TimeZoneInfo tzI in timeZones)
                {
                    if (tzI.DisplayName.Contains("US "))
                    {
                        if (tzI.StandardName.Contains("Pacific"))
                        {
                            USAtimeZones.Add("PST");
                        }
                        else if (tzI.StandardName.Contains("Mountain"))
                        {
                            USAtimeZones.Add("MST");
                        }
                        else if (tzI.StandardName.Contains("Central"))
                        {
                            USAtimeZones.Add("CST");
                        }
                        else
                        {
                            USAtimeZones.Add("EST");
                        }
                    }
                }
                USAtimeZones.Insert(0, "UTC");
                ViewData["TimeZones"] = USAtimeZones;

                return View("CreatePeakSumForm");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //post the peaksummary and return the new id to details page to show new info in infobox
        //POST:
        [HttpPost]
        public ActionResult CreatePeakSumForm(PeakSummaryModel peak)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;

                //build the PEAKSUMMARY object from Peak Model
                PEAK_SUMMARY newPeakSum = new PEAK_SUMMARY();

                newPeakSum.MEMBER_ID = GetMember(User.Identity.Name).MEMBER_ID;

                string date = peak.PeakDate; // mm/dd/yyyy
                string time = peak.PeakTime; // hh:mm PM

                string datetime = date + " " + time;
                newPeakSum.PEAK_DATE = DateTime.Parse(datetime, System.Globalization.CultureInfo.InvariantCulture);

                newPeakSum.TIME_ZONE = peak.TimeZone;

                if (newPeakSum.TIME_ZONE != "UTC")
                {
                    newPeakSum.TIME_ZONE = "UTC";
                    newPeakSum.PEAK_DATE = newPeakSum.PEAK_DATE.Value.ToUniversalTime();
                }

                newPeakSum.IS_PEAK_ESTIMATED = peak.DateEstimated;
                newPeakSum.IS_PEAK_TIME_ESTIMATED = peak.TimeEstimated;

                newPeakSum.PEAK_STAGE = peak.PeakStage;
                newPeakSum.IS_PEAK_STAGE_ESTIMATED = peak.StageEstimated;
                newPeakSum.VDATUM_ID = peak.VdatumID;
                newPeakSum.HEIGHT_ABOVE_GND = peak.HeightAboveGround;
                newPeakSum.IS_HAG_ESTIMATED = peak.HAGEstimated;
                newPeakSum.PEAK_DISCHARGE = peak.PeakDischarge;
                newPeakSum.IS_PEAK_DISCHARGE_ESTIMATED = peak.DischargeEstimated;

                //post the new Peak Summary
                var request = new RestRequest(Method.POST);
                request.Resource = "/PeakSummaries";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<PEAK_SUMMARY>(newPeakSum), ParameterType.RequestBody);
                PEAK_SUMMARY createdPeakSum = serviceCaller.Execute<PEAK_SUMMARY>(request);

                //check to see if this is a Peak summary for a File or for a HWM
                //put peakSummary_id into File.PEakSummaryID or HWM.PeakSummaryID
                var FROM = peak.From;
                if (FROM == "File")
                {
                    //use id to get the file, then datafile to assign peak summary id to field
                    FILE aFile = GetFile(peak.objID);

                    DATA_FILE thisDF = GetDataFile(aFile.DATA_FILE_ID);

                    thisDF.PEAK_SUMMARY_ID = createdPeakSum.PEAK_SUMMARY_ID;

                    //now PUT this back to the service
                    request = new RestRequest(Method.POST);
                    request.Resource = "DataFiles/{entityId}";
                    request.RequestFormat = DataFormat.Xml;
                    request.AddParameter("entityId", thisDF.DATA_FILE_ID, ParameterType.UrlSegment);
                    request.AddHeader("X-HTTP-Method-Override", "PUT");
                    request.AddHeader("Content-Type", "application/xml");
                    //Use extended serializer
                    serializer = new STNWebSerializer();
                    request.AddParameter("application/xml", serializer.Serialize<DATA_FILE>(thisDF), ParameterType.RequestBody);
                    serviceCaller.Execute<DATA_FILE>(request);

                    //return to filedetails page using fileid to refresh   
                    return RedirectToAction("FileDetails", "Files", new { id = peak.objID });
                }
                else //(FROM == "HWM")
                {
                    //use id to get HWM file to assign peak summary id to field
                    request = new RestRequest();
                    request.Resource = "HWMs/{entityId}";
                    request.RootElement = "HWM";
                    request.AddParameter("entityId", peak.objID, ParameterType.UrlSegment);
                    HWM aHWM = serviceCaller.Execute<HWM>(request);

                    aHWM.PEAK_SUMMARY_ID = createdPeakSum.PEAK_SUMMARY_ID;

                    //now PUT this back to the service
                    request = new RestRequest(Method.POST);
                    request.Resource = "HWMs/{entityId}";
                    request.RequestFormat = DataFormat.Xml;
                    request.AddParameter("entityId", aHWM.HWM_ID, ParameterType.UrlSegment);
                    request.AddHeader("X-HTTP-Method-Override", "PUT");
                    request.AddHeader("Content-Type", "application/xml");
                    //Use extended serializer
                    serializer = new STNWebSerializer();
                    request.AddParameter("application/xml", serializer.Serialize<HWM>(aHWM), ParameterType.RequestBody);
                    serviceCaller.Execute<HWM>(request);

                    //return to hwmdetails page using hwmid to refresh   
                    return RedirectToAction("Details", "HWMs", new { id = peak.objID });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //returns aDataFile whenever called
        public DATA_FILE GetDataFile(decimal? id)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "DataFiles/{entityId}";
            request.RootElement = "DATA_FILE";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            DATA_FILE aDataFile = serviceCaller.Execute<DATA_FILE>(request);
            return aDataFile;
        }

        //returns member
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

        //returns aFile whenever called
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

    }
}
