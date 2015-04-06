//------------------------------------------------------------------------------
//----- PublicController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Populates public resource for the view
//
//discussion:   
//
//     

#region Comments
//05.12.14 - TR - updated GPS Type to Horizontal Collection Method
// 05.31.13 - TR - Created 
#endregion Comments


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using System.Data.Objects.DataClasses;

using RestSharp;
using STNServices;
using STNServices.Authentication;
using STNServices.Resources;
using STNWeb.Utilities;
using STNWeb.Models;

namespace STNWeb.Controllers
{
    public class PublicController : Controller
    {
        //
        // GET: /Public/
        public ActionResult HWMInfoPage(int siteId, int hwmId)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                #region SiteInfo
                //get site
                request.Resource = "Sites/{entityId}";
                request.RootElement = "SITE";
                request.AddParameter("entityId", siteId, ParameterType.UrlSegment);
                SITE thisSite = serviceCaller.Execute<SITE>(request);
                ViewData["Site"] = thisSite;

                //get hdatum
                
                request = new RestRequest();
                request.Resource = "Sites/{siteId}/hDatum";
                request.RootElement = "HORIZONTAL_DATUMS";
                request.AddParameter("siteId", thisSite.SITE_ID, ParameterType.UrlSegment);
                ViewData["HDatum"] = serviceCaller.Execute<HORIZONTAL_DATUMS>(request).DATUM_NAME;
                #endregion SiteInfo

                //get Peaks
                GetPeakSummaries(siteId);

                #region HWMinfo
                //get hwm
                request = new RestRequest();
                request.Resource = "HWMs/{entityId}";
                request.RootElement = "HWM";
                request.AddParameter("entityId", hwmId, ParameterType.UrlSegment);
                HWM aHWM = serviceCaller.Execute<HWM>(request);
                ViewData["HWM"] = aHWM;

                //get event
                request = new RestRequest();
                request.Resource = "/HWMs/{hwmId}/Event";
                request.RootElement = "EVENT";
                request.AddParameter("hwmId", aHWM.HWM_ID, ParameterType.UrlSegment);
                EVENT hwmEvent = serviceCaller.Execute<EVENT>(request);
                if (hwmEvent != null) { ViewData["HwmEvent"] = hwmEvent.EVENT_NAME; }

                //get HWM Type
                request = new RestRequest();
                request.Resource = "/HWMTypes/{entityId}";
                request.RootElement = "HWM_TYPES";
                request.AddParameter("entityId", aHWM.HWM_TYPE_ID, ParameterType.UrlSegment);
                HWM_TYPES thisHWMType = serviceCaller.Execute<HWM_TYPES>(request);
                if (thisHWMType != null)
                { ViewData["HWMType"] = thisHWMType.HWM_TYPE; }

                //get HWM Quality
                request = new RestRequest();
                request.Resource = "/HWMQualities/{entityId}";
                request.RootElement = "HWM_QUALITIES";
                request.AddParameter("entityId", aHWM.HWM_QUALITY_ID, ParameterType.UrlSegment);
                HWM_QUALITIES thisHWMQual = serviceCaller.Execute<HWM_QUALITIES>(request);
                if (thisHWMQual != null)
                { ViewData["HWMQaul"] = thisHWMQual.HWM_QUALITY; }

                if (aHWM.FLAG_TEAM_ID != null && aHWM.FLAG_TEAM_ID != 0)
                {
                    //get CollectionTeam
                    request = new RestRequest();
                    request.Resource = "/CollectionTeams/{entityId}";
                    request.RootElement = "COLLECT_TEAM";
                    request.AddParameter("entityId", aHWM.FLAG_TEAM_ID, ParameterType.UrlSegment);
                    ViewData["FlagTeam"] = serviceCaller.Execute<COLLECT_TEAM>(request).DESCRIPTION;
                }

                if (aHWM.SURVEY_TEAM_ID != null && aHWM.SURVEY_TEAM_ID != 0)
                {
                    //get CollectionTeam
                    request = new RestRequest();
                    request.Resource = "/CollectionTeams/{entityId}";
                    request.RootElement = "COLLECT_TEAM";
                    request.AddParameter("entityId", aHWM.SURVEY_TEAM_ID, ParameterType.UrlSegment);
                    ViewData["SurveyTeam"] = serviceCaller.Execute<COLLECT_TEAM>(request).DESCRIPTION;
                }

                //get collectionMethod
                request = new RestRequest();
                request.Resource = "/VerticalMethods/{entityId}";
                request.RootElement = "VERTICAL_COLLECT_METHODS";
                request.AddParameter("entityId", aHWM.VCOLLECT_METHOD_ID, ParameterType.UrlSegment);
                VERTICAL_COLLECT_METHODS thisVCollMtd = serviceCaller.Execute<VERTICAL_COLLECT_METHODS>(request);
                if (thisVCollMtd != null)
                { ViewData["VCollectMethod"] = thisVCollMtd.VCOLLECT_METHOD; }

                //Get Vertical Datum
                request = new RestRequest();
                request.Resource = "/VerticalDatums/{entityId}";
                request.RootElement = "VERTICAL_DATUMS";
                request.AddParameter("entityId", aHWM.VDATUM_ID, ParameterType.UrlSegment);
                VERTICAL_DATUMS thisVD = serviceCaller.Execute<VERTICAL_DATUMS>(request);
                if (thisVD != null)
                { ViewData["VerticalDatum"] = thisVD.DATUM_NAME; }

                //Get marker
                request = new RestRequest();
                request.Resource = "/Markers/{entityId}";
                request.RootElement = "MARKER";
                request.AddParameter("entityId", aHWM.MARKER_ID, ParameterType.UrlSegment);
                MARKER thisMarker = serviceCaller.Execute<MARKER>(request);
                if (thisMarker != null)
                { ViewData["Marker"] = thisMarker.MARKER1; }

                //get Horizontal Collection Method
                request = new RestRequest();
                request.Resource = "/HorizontalMethods/{entityId}";
                request.RootElement = "HORIZONTAL_COLLECT_METHODS";
                request.AddParameter("entityId", aHWM.HCOLLECT_METHOD_ID, ParameterType.UrlSegment);
                HORIZONTAL_COLLECT_METHODS thisHColMethod = serviceCaller.Execute<HORIZONTAL_COLLECT_METHODS>(request);
                if (thisHColMethod != null)
                { ViewData["HColMethod"] = thisHColMethod.HCOLLECT_METHOD; }
                #endregion HWMinfo

                #region FileInfo
                //now get all the files
                request = new RestRequest();
                request.Resource = "/HWMs/{hwmId}/Files/";
                request.RootElement = "ArrayOfFILE";
                request.AddParameter("hwmId", aHWM.HWM_ID, ParameterType.UrlSegment);
                List<FILE> allTheseFiles = serviceCaller.Execute<List<FILE>>(request);
                List<FILE> photoFiles = allTheseFiles.Where(pf => pf.FILETYPE_ID == 1).ToList();

                List<PhotoFileCaption> PhotoCapFiles = new List<PhotoFileCaption>();
                foreach (FILE f in photoFiles)
                {
                    //build caption
                    PhotoFileCaption thisPhoto = new PhotoFileCaption();
                    thisPhoto.FileID = f.FILE_ID.ToString();
                    thisPhoto.FileDesc = f.DESCRIPTION;
                    thisPhoto.SiteDescription = thisSite.SITE_DESCRIPTION;
                    thisPhoto.County = thisSite.COUNTY;
                    thisPhoto.State = thisSite.STATE;
                    thisPhoto.Date = f.FILE_DATE;

                    request = new RestRequest();
                    request.Resource = "/Sources/{entityId}";
                    request.RootElement = "SOURCE";
                    request.AddParameter("entityId", f.SOURCE_ID, ParameterType.UrlSegment);
                    SOURCE thisSource = serviceCaller.Execute<SOURCE>(request);
                    thisPhoto.MemberName = thisSource.SOURCE_NAME;

                    request = new RestRequest();
                    request.Resource = "/Agencies/{entityId}";
                    request.RootElement = "AGENCY";
                    request.AddParameter("entityId", thisSource.AGENCY_ID, ParameterType.UrlSegment);
                    thisPhoto.MemberAgency = serviceCaller.Execute<AGENCY>(request).AGENCY_NAME;
                    PhotoCapFiles.Add(thisPhoto);
                }

                ViewData["PhotoCaptionList"] = PhotoCapFiles;
                #endregion FileInof

                return PartialView();
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        public ActionResult SensorInfoPage(int siteId, int sensorId)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                #region SiteInfo
                //get site
                request.Resource = "Sites/{entityId}";
                request.RootElement = "SITE";
                request.AddParameter("entityId", siteId, ParameterType.UrlSegment);
                SITE thisSite = serviceCaller.Execute<SITE>(request);
                ViewData["Site"] = thisSite;

                //get hdatum
                request = new RestRequest();
                request.Resource = "/HorizontalDatums/{entityId}";
                request.RootElement = "HORIZONTAL_DATUMS";
                request.AddParameter("entityId", thisSite.HDATUM_ID, ParameterType.UrlSegment);
                ViewData["HDatum"] = serviceCaller.Execute<HORIZONTAL_DATUMS>(request).DATUM_NAME;
                #endregion SiteInfo

                //get peaks
                GetPeakSummaries(siteId);

                #region SensorInfo
                //get the sensor
                request = new RestRequest();
                request.Resource = "Instruments/{entityId}";
                request.RootElement = "INSTRUMENT";
                request.AddParameter("entityId", sensorId, ParameterType.UrlSegment);
                INSTRUMENT thisInstrument = serviceCaller.Execute<INSTRUMENT>(request);
                ViewData["Instrument"] = thisInstrument;

                //get event
                request = new RestRequest();
                request.Resource = "/Instruments/{instrumentId}/Event";
                request.RootElement = "EVENT";
                request.AddParameter("instrumentId", thisInstrument.INSTRUMENT_ID, ParameterType.UrlSegment);
                EVENT sensorEvent = serviceCaller.Execute<EVENT>(request);
                ViewData["sensorEvent"] = sensorEvent.EVENT_NAME;

                //pull from log the statuses
                request = new RestRequest();
                request.Resource = "/Instruments/{instrumentId}/InstrumentStatusLog";
                request.RootElement = "ArrayOfInstrumentStatus";
                request.AddParameter("instrumentId", sensorId, ParameterType.UrlSegment);
                List<INSTRUMENT_STATUS> InstrStatusList = serviceCaller.Execute<List<INSTRUMENT_STATUS>>(request);

                foreach (INSTRUMENT_STATUS IS in InstrStatusList)
                {
                    switch (Convert.ToInt32(IS.STATUS_TYPE_ID))
                    {
                        case 1:
                            ViewData["DeployedStatus"] = IS;
                            break;
                        case 2:
                            ViewData["RetrievedStatus"] = IS;
                            break;
                        default:
                            ViewData["LostStatus"] = IS;
                            break;
                    }
                }

                //get Sensor Type if one
                if (thisInstrument.SENSOR_TYPE_ID != null)
                {
                    request = new RestRequest();
                    request.Resource = "/SensorTypes/{entityId}";
                    request.RootElement = "SENSOR_TYPE";
                    request.AddParameter("entityId", thisInstrument.SENSOR_TYPE_ID, ParameterType.UrlSegment);
                    string aSensor = serviceCaller.Execute<SENSOR_TYPE>(request).SENSOR;
                    ViewData["SensorType"] = aSensor;
                }

                ////get deployment_type if one
                if (thisInstrument.DEPLOYMENT_TYPE_ID != null && thisInstrument.DEPLOYMENT_TYPE_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/DeploymentTypes/{entityId}";
                    request.RootElement = "DEPLOYMENT_TYPE";
                    request.AddParameter("entityId", thisInstrument.DEPLOYMENT_TYPE_ID, ParameterType.UrlSegment);
                    string aDepType = serviceCaller.Execute<DEPLOYMENT_TYPE>(request).METHOD;
                    ViewData["DeplType"] = aDepType;
                }
                //get condition
                if (thisInstrument.INST_COLLECTION_ID != null && thisInstrument.INST_COLLECTION_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/InstrCollectConditions/{entityId}";
                    request.RootElement = "INSTR_COLLECTION_CONDITIONS";
                    request.AddParameter("entityId", thisInstrument.INST_COLLECTION_ID, ParameterType.UrlSegment);
                    ViewData["Condition"] = serviceCaller.Execute<INSTR_COLLECTION_CONDITIONS>(request).CONDITION;
                }

                if (InstrStatusList.Count > 1)
                {
                    if (InstrStatusList[0].COLLECTION_TEAM_ID != 0 && InstrStatusList[0].COLLECTION_TEAM_ID != null)
                    {
                        request = new RestRequest();
                        request.Resource = "/CollectionTeams/{entityId}";
                        request.RootElement = "COLLECT_TEAM";
                        request.AddParameter("entityId", InstrStatusList[0].COLLECTION_TEAM_ID, ParameterType.UrlSegment);
                        COLLECT_TEAM ct = serviceCaller.Execute<COLLECT_TEAM>(request);
                        ViewData["depTeamName"] = ct.DESCRIPTION;
                    }
                    if (InstrStatusList[1].COLLECTION_TEAM_ID != 0 && InstrStatusList[1].COLLECTION_TEAM_ID != null)
                    {
                        request = new RestRequest();
                        request.Resource = "/CollectionTeams/{entityId}";
                        request.RootElement = "COLLECT_TEAM";
                        request.AddParameter("entityId", InstrStatusList[1].COLLECTION_TEAM_ID, ParameterType.UrlSegment);
                        COLLECT_TEAM ct = serviceCaller.Execute<COLLECT_TEAM>(request);
                        ViewData["retTeamName"] = ct.DESCRIPTION;
                    }
                }
                #endregion SensorInfo

                #region FileInfo
                request = new RestRequest();
                request.Resource = "/Instruments/{instrumentId}/Files";
                request.RootElement = "ArrayOfFILE";
                request.AddParameter("instrumentId", sensorId, ParameterType.UrlSegment);
                List<FILE> sensorFiles = serviceCaller.Execute<List<FILE>>(request);

                ViewData["dataFiles"] = sensorFiles.Where(df => df.FILETYPE_ID == 2).ToList();
                List<FILE> photoFiles = sensorFiles.Where(pf => pf.FILETYPE_ID == 1).ToList();
                
                //build photo caption
                List<PhotoFileCaption> photoCapFiles = new List<PhotoFileCaption>();
                foreach (FILE f in photoFiles)
                {
                    PhotoFileCaption thisPhoto = new PhotoFileCaption();
                    thisPhoto.FileID = f.FILE_ID.ToString();
                    thisPhoto.FileDesc = f.DESCRIPTION;
                    thisPhoto.SiteDescription = thisSite.SITE_DESCRIPTION;
                    thisPhoto.County = thisSite.COUNTY;
                    thisPhoto.State = thisSite.STATE;
                    thisPhoto.Date = f.FILE_DATE;

                    request = new RestRequest();
                    request.Resource = "/Sources/{entityId}";
                    request.RootElement = "SOURCE";
                    request.AddParameter("entityId", f.SOURCE_ID, ParameterType.UrlSegment);
                    SOURCE thisSource = serviceCaller.Execute<SOURCE>(request);
                    thisPhoto.MemberName = thisSource.SOURCE_NAME;

                    request = new RestRequest();
                    request.Resource = "/Agencies/{entityId}";
                    request.RootElement = "AGENCY";
                    request.AddParameter("entityId", thisSource.AGENCY_ID, ParameterType.UrlSegment);
                    thisPhoto.MemberAgency = serviceCaller.Execute<AGENCY>(request).AGENCY_NAME;
                    photoCapFiles.Add(thisPhoto);
                }
                ViewData["PhotoCaptionList"] = photoCapFiles;

                #endregion FileInfo

                return PartialView();
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        public void GetPeakSummaries(int id)
        {
            //Site Peaks
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Sites/{siteId}/PeakSummaries";
            request.RootElement = "ArrayOfPEAK_SUMMARY";
            request.AddParameter("siteId", id, ParameterType.UrlSegment);
            List<PEAK_SUMMARY> SiteSummaries = serviceCaller.Execute<List<PEAK_SUMMARY>>(request);

            //HWM Peaks
            List<HWM> HWMSummaries = new List<HWM>();
            List<DATA_FILE> DataFileSums = new List<DATA_FILE>(); //each set within loop
            List<PeakSummaryModel> allPeakSums = new List<PeakSummaryModel>();

            //do the rest of this only if there are SiteSummaries
            if (SiteSummaries != null && SiteSummaries.Count >= 1)
            {
                //go through Site peaks and pull out which are hwm and/or datafile
                //add all pieces to PeakSummaryModel to pass to view (Elevation, Date, Time, Event, Description)
                foreach (PEAK_SUMMARY p in SiteSummaries)
                {
                    PeakSummaryModel PeakSum = new PeakSummaryModel();
                    //populate peak pieces 
                    PeakSum.Link2Details = id;

                    PeakSum.PeakStage = p.PEAK_STAGE;// string.Format("{0:n0}", p.PEAK_STAGE);
                    PeakSum.PeakDate = ((DateTime)p.PEAK_DATE).ToString("M/d/yy");
                    PeakSum.PeakTime = ((DateTime)p.PEAK_DATE).ToString("t");
                    PeakSum.DateEstimated = p.IS_PEAK_ESTIMATED;
                    PeakSum.TimeEstimated = p.IS_PEAK_TIME_ESTIMATED;

                    //get hwm peaks
                    request = new RestRequest();
                    request.Resource = "PeakSummaries/{peakSummaryId}/HWMs";
                    request.RootElement = "ArrayOfHWM";
                    request.AddParameter("peakSummaryId", p.PEAK_SUMMARY_ID, ParameterType.UrlSegment);
                    HWMSummaries = serviceCaller.Execute<List<HWM>>(request); // should return list containing only 1

                    //if it is a HWM Peak, add Description to model and get Event
                    if (HWMSummaries != null && HWMSummaries.Count >= 1)
                    {
                        HWM thisOne = HWMSummaries[0];
                        PeakSum.PeakHWMDesc = thisOne.HWM_LOCATIONDESCRIPTION;
                        PeakSum.eventId = thisOne.EVENT_ID;
                        //get event 
                        if (thisOne.EVENT_ID != null)
                        {
                            request = new RestRequest();
                            request.Resource = "/Events/{entityId}";
                            request.RootElement = "EVENT";
                            request.AddParameter("entityId", thisOne.EVENT_ID, ParameterType.UrlSegment);
                            PeakSum.EventName = serviceCaller.Execute<EVENT>(request).EVENT_NAME;
                        }                        
                    }

                    // get datafile peaks
                    request = new RestRequest();
                    request.Resource = "/PeakSummaries/{peakSummaryId}/DataFiles";
                    request.RootElement = "ArrayOfDATA_FILE";
                    request.AddParameter("peakSummaryId", p.PEAK_SUMMARY_ID, ParameterType.UrlSegment);
                    DataFileSums = serviceCaller.Execute<List<DATA_FILE>>(request);

                    //if it is a datafile peak, get instrument for Description and event name
                    if (DataFileSums != null && DataFileSums.Count >=1)
                    {
                        DATA_FILE thisDF = DataFileSums[0];

                        //get event name and description via instrument
                        request = new RestRequest();
                        request.Resource = "Instruments/{entityId}";
                        request.RootElement = "INSTRUMENT";
                        request.AddParameter("entityId", thisDF.INSTRUMENT_ID, ParameterType.UrlSegment);
                        INSTRUMENT thisInstr = serviceCaller.Execute<INSTRUMENT>(request);

                        PeakSum.PeakDataFileDesc = thisInstr.LOCATION_DESCRIPTION;
                        PeakSum.eventId = thisInstr.EVENT_ID;
                        request = new RestRequest();
                        request.Resource = "/Events/{entityId}";
                        request.RootElement = "EVENT";
                        request.AddParameter("entityId", thisInstr.EVENT_ID, ParameterType.UrlSegment);
                        PeakSum.EventName = serviceCaller.Execute<EVENT>(request).EVENT_NAME;                        
                    }
                    allPeakSums.Add(PeakSum);
                }
            }
            ViewData["Peaks"] = allPeakSums;
        }
    }
}
