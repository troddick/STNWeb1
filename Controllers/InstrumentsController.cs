//------------------------------------------------------------------------------
//----- InstrumentsController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Populates Instrument resource for the view
//
//     

#region Comments
// 04.16.13 - TR - Working on Details/edit page to distinquish between deployed and retrieved/lost, adding edit for each one
// 02.14.13 - TR - Updated whole controller to use InstrumentModel, creating/updating/viewing Instrument & Instrument_Status
// 12.05.12 - TR - Added PopulateDepTypeDDL (create: choose Sensor Type = affects dropdown options for dep types)
// 12.03.12 - TR - Add Partial Views for Details and Edit
// 11.30.12 - TR - Created Partial view for Info Box that goes on Sites Details and HWM Details pages
// 10.29.12 - TR - Create, edit added (not posting yet)
// 10.10.12 - JB - Created from old web app
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
using System.Web.Routing;

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
    [RequireSSL]
    [Authorize]
    public class InstrumentsController : Controller
    {
        //Info box that shows list of instruments for a site
        // GET: /Instruments/InstrumentInfoBox/
        public PartialViewResult InstrumentInfoBox(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "Sites/{siteId}/Instruments";
                request.RootElement = "ArrayOfInstruments";
                request.AddParameter("siteId", id, ParameterType.UrlSegment);
                List<INSTRUMENT> siteInstruments = serviceCaller.Execute<List<INSTRUMENT>>(request);

                //holders
                List<InstrSensorType> PropInstruments = new List<InstrSensorType>();
                List<INSTRUMENT> DepInstruments = new List<INSTRUMENT>();
                List<INSTRUMENT> RetInstruments = new List<INSTRUMENT>();
                List<INSTRUMENT> LostInstruments = new List<INSTRUMENT>();

                //break the instruments up by status
                if (siteInstruments != null)
                {
                    foreach (INSTRUMENT i in siteInstruments)
                    {
                        //get the status and store in appropriate viewdata
                        request = new RestRequest();
                        request.Resource = "/Instruments/{instrumentId}/InstrumentStatus";
                        request.RootElement = "INSTRUMENT_STATUS";
                        request.AddParameter("instrumentId", i.INSTRUMENT_ID, ParameterType.UrlSegment);
                        decimal? statType = serviceCaller.Execute<INSTRUMENT_STATUS>(request).STATUS_TYPE_ID;

                        switch (Convert.ToInt32(statType))
                        {
                            case 4:
                                InstrSensorType aPropInst = new InstrSensorType();
                                aPropInst.InstrID = Convert.ToString(i.INSTRUMENT_ID);
                                //get dep type
                                request = new RestRequest();
                                request.Resource = "Instruments/{instrumentId}/DeploymentType";
                                request.RootElement = "DEPLOYMENT_TYPE";
                                request.AddParameter("instrumentId", i.INSTRUMENT_ID, ParameterType.UrlSegment);
                                aPropInst.DepType = serviceCaller.Execute<DEPLOYMENT_TYPE>(request).METHOD;
                                PropInstruments.Add(aPropInst);
                                break;
                            case 1:
                                DepInstruments.Add(i);
                                break;
                            case 2:
                                RetInstruments.Add(i);
                                break;
                            case 3:
                                LostInstruments.Add(i);
                                break;
                        }
                    }
                    if (PropInstruments.Count >= 1)
                        ViewData["Proposed"] = PropInstruments;
                    if (DepInstruments.Count >= 1)
                        ViewData["Deployed"] = DepInstruments;
                    if (RetInstruments.Count >= 1)
                        ViewData["Retrieved"] = RetInstruments;
                    if (LostInstruments.Count >= 1)
                        ViewData["Lost"] = LostInstruments;
                }
                ViewData["SiteId"] = id;
                //get deployment types for the proposed Sensor area
                request = new RestRequest();
                request.Resource = "/DeploymentTypes";
                request.RootElement = "ArrayOfDEPLOYMENT_TYPE";
                ViewData["DeploymentTypeList"] = serviceCaller.Execute<List<DEPLOYMENT_TYPE>>(request);

                return PartialView();
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //POST:
        //these deployment type sensor are to be added as proposed sensors to this site
        [HttpPost]
        public JsonResult AddProposedSiteSensors(int siteId, string[] ProposedSiteSensors)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //Proposed Sensors for each one checked, create it for this site with status of Proposed
                if (ProposedSiteSensors.Length >= 1 && ProposedSiteSensors != null)
                {
                    foreach (string i in ProposedSiteSensors)
                    {
                        //make an instrument
                        INSTRUMENT proposedInst = new INSTRUMENT();
                        proposedInst.DEPLOYMENT_TYPE_ID = Convert.ToDecimal(i);
                        proposedInst.SITE_ID = siteId;

                        //get this deployment's type sensor type
                        request = new RestRequest();
                        request.Resource = "/DeploymentTypes/{deploymentTypeId}/SensorType";
                        request.RootElement = "SENSOR_TYPE";
                        request.AddParameter("deploymentTypeId", proposedInst.DEPLOYMENT_TYPE_ID, ParameterType.UrlSegment);
                        proposedInst.SENSOR_TYPE_ID = serviceCaller.Execute<SENSOR_TYPE>(request).SENSOR_TYPE_ID;

                        //now post it as proposed
                        request = new RestRequest(Method.POST);
                        request.Resource = "Instruments/";
                        request.RequestFormat = DataFormat.Xml;
                        request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                        request.AddBody(proposedInst);
                        INSTRUMENT createdInst = serviceCaller.Execute<INSTRUMENT>(request);

                        //now make the Instr_Status
                        INSTRUMENT_STATUS proposedInsStat = new INSTRUMENT_STATUS();
                        proposedInsStat.INSTRUMENT_ID = createdInst.INSTRUMENT_ID;
                        proposedInsStat.STATUS_TYPE_ID = 4; //proposed
                        proposedInsStat.TIME_ZONE = "UTC";
                        proposedInsStat.TIME_STAMP = Convert.ToDateTime(Request.Form["TIME_STAMP"]);

                        //get the member logged in to store in the collectionTeam id field
                        //will be member id only for proposed sensors
                        request = new RestRequest();
                        request.Resource = "/Members?username={userName}";
                        request.RootElement = "MEMBER";
                        request.AddParameter("userName", User.Identity.Name, ParameterType.UrlSegment);
                        proposedInsStat.COLLECTION_TEAM_ID = serviceCaller.Execute<MEMBER>(request).MEMBER_ID;

                        //now post it
                        request = new RestRequest(Method.POST);
                        request.Resource = "InstrumentStatus/";
                        request.RequestFormat = DataFormat.Xml;
                        request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                        request.AddBody(proposedInsStat);
                        INSTRUMENT_STATUS createdStat = serviceCaller.Execute<INSTRUMENT_STATUS>(request);
                    }
                }

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        //Info box that shows list of instruments for a site AND event
        // GET: /Instruments/InstrumentInfoBox4Ev/
        public PartialViewResult InstrumentInfoBox4Ev(int evId, int siteId)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                //get all the proposed sensors regardless of event
                request.Resource = "Sites/{siteId}/Instruments";
                request.RootElement = "ArrayOfInstruments";
                request.AddParameter("siteId", siteId, ParameterType.UrlSegment);
                List<INSTRUMENT> siteInstruments = serviceCaller.Execute<List<INSTRUMENT>>(request);

                //holders
                List<InstrSensorType> PropInstruments = new List<InstrSensorType>();
                foreach (INSTRUMENT ins in siteInstruments)
                {
                    //get the status
                    request = new RestRequest();
                    request.Resource = "/Instruments/{instrumentId}/InstrumentStatus";
                    request.RootElement = "INSTRUMENT_STATUS";
                    request.AddParameter("instrumentId", ins.INSTRUMENT_ID, ParameterType.UrlSegment);
                    decimal? statType = serviceCaller.Execute<INSTRUMENT_STATUS>(request).STATUS_TYPE_ID;

                    if (statType == 4)
                    {
                        InstrSensorType aPropInst = new InstrSensorType();
                        aPropInst.InstrID = Convert.ToString(ins.INSTRUMENT_ID);
                        //get dep type
                        request = new RestRequest();
                        request.Resource = "Instruments/{instrumentId}/DeploymentType";
                        request.RootElement = "DEPLOYMENT_TYPE";
                        request.AddParameter("instrumentId", ins.INSTRUMENT_ID, ParameterType.UrlSegment);
                        aPropInst.DepType = serviceCaller.Execute<DEPLOYMENT_TYPE>(request).METHOD;
                        PropInstruments.Add(aPropInst);

                    }
                }
                if (PropInstruments.Count >= 1)
                    ViewData["Proposed"] = PropInstruments;

                //return the Instruments for this site and this event
                request = new RestRequest();
                request.Resource = "/Sites/{siteId}/Instruments?Event={eventId}";
                request.RootElement = "ArrayOfInstruments";
                request.AddParameter("siteId", siteId, ParameterType.UrlSegment);
                request.AddParameter("eventId", evId, ParameterType.UrlSegment);
                List<INSTRUMENT> SiteEvInstr = serviceCaller.Execute<List<INSTRUMENT>>(request);

                if (SiteEvInstr != null && SiteEvInstr.Count >= 1)
                {
                    //holders
                    //List<InstrSensorType> PropInstruments = new List<InstrSensorType>();
                    List<INSTRUMENT> DepInstruments = new List<INSTRUMENT>();
                    List<INSTRUMENT> RetInstruments = new List<INSTRUMENT>();
                    List<INSTRUMENT> LostInstruments = new List<INSTRUMENT>();

                    foreach (INSTRUMENT i in SiteEvInstr)
                    {
                        //get the status and store in appropriate viewdata
                        request = new RestRequest();
                        request.Resource = "/Instruments/{instrumentId}/InstrumentStatus";
                        request.RootElement = "INSTRUMENT_STATUS";
                        request.AddParameter("instrumentId", i.INSTRUMENT_ID, ParameterType.UrlSegment);
                        decimal? statType = serviceCaller.Execute<INSTRUMENT_STATUS>(request).STATUS_TYPE_ID;

                        switch (Convert.ToInt32(statType))
                        {
                            case 1:
                                DepInstruments.Add(i);
                                break;
                            case 2:
                                RetInstruments.Add(i);
                                break;
                            case 3:
                                LostInstruments.Add(i);
                                break;
                        }
                    }

                    if (DepInstruments.Count >= 1)
                        ViewData["Deployed"] = DepInstruments;
                    if (RetInstruments.Count >= 1)
                        ViewData["Retrieved"] = RetInstruments;
                    if (LostInstruments.Count >= 1)
                        ViewData["Lost"] = LostInstruments;
                }

                //get deployment types for the proposed Sensor area
                request = new RestRequest();
                request.Resource = "/DeploymentTypes";
                request.RootElement = "ArrayOfDEPLOYMENT_TYPE";
                ViewData["DeploymentTypeList"] = serviceCaller.Execute<List<DEPLOYMENT_TYPE>>(request);

                ViewData["SiteId"] = siteId;

                return PartialView();
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //main page containing partial views for details/edit pages
        // GET: /Instruments/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                InstrumentModel InstrModel = new InstrumentModel();
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;

                //get member logged in's role
                var request = new RestRequest();
                request.Resource = "/Members?username={userName}";
                request.RootElement = "MEMBER";
                request.AddParameter("userName", User.Identity.Name, ParameterType.UrlSegment);
                MEMBER thisMember = serviceCaller.Execute<MEMBER>(request);
                if (thisMember.ROLE_ID == 1) { ViewData["Role"] = "Admin"; }
                if (thisMember.ROLE_ID == 2) { ViewData["Role"] = "Manager"; }
                if (thisMember.ROLE_ID == 3) { ViewData["Role"] = "Field"; }

                //get this instrument
                InstrModel.Instr = getThisInstrument(id);

                //get Site number
                ViewData["SiteNo"] = getThisSite(InstrModel.Instr.SITE_ID).SITE_NO;

                //get all statuses for this instrument, sorted by timestamp (most recent on top)
                //unless it is deployed and retrieved on the same day
                InstrModel.InstrStatusList = getInstrumentStatusLog(id);
                foreach (INSTRUMENT_STATUS instr in InstrModel.InstrStatusList)
                {
                    switch (Convert.ToInt32(instr.STATUS_TYPE_ID))
                    {
                        case 1:
                            InstrModel.DeplInstrStatus = instr;
                            break;
                        case 2:
                            InstrModel.RetrInstrStatus = instr;
                            break;
                        case 4:
                            InstrModel.PropInstrStatus = instr;
                            break;
                        default:
                            InstrModel.LostInstrStatus = instr;
                            break;
                    }

                }
                return View("Details", InstrModel);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // Deployed Instrument's Details
        // GET: /Instruments/DetailsPV/1
        public PartialViewResult DepInstrDetailsPV(int id)
        {
            try
            {
                InstrumentModel InstrModel = new InstrumentModel();
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //get this instrument
                INSTRUMENT anInstrument = getThisInstrument(id);
                //get Site number
                ViewData["SiteNo"] = getThisSite(anInstrument.SITE_ID).SITE_NO;

                InstrModel.Instr = anInstrument;

                //pull from log and grab the deployed one for top details
                InstrModel.InstrStatusList = getInstrumentStatusLog(id);


                //get each one in the model
                foreach (INSTRUMENT_STATUS IS in InstrModel.InstrStatusList)
                {
                    switch (Convert.ToInt32(IS.STATUS_TYPE_ID))
                    {
                        case 1:
                            InstrModel.DeplInstrStatus = IS;
                            break;
                        case 2:
                            InstrModel.RetrInstrStatus = IS;
                            break;
                        case 3:
                            InstrModel.LostInstrStatus = IS;
                            break;
                        default:
                            InstrModel.PropInstrStatus = IS;
                            //get the member who proposed it
                            request = new RestRequest();
                            request.Resource = "Members/{entityId}";
                            request.RootElement = "MEMBER";
                            request.AddParameter("entityId", InstrModel.PropInstrStatus.COLLECTION_TEAM_ID, ParameterType.UrlSegment);
                            MEMBER proposedBy = serviceCaller.Execute<MEMBER>(request);
                            ViewData["proposedBy"] = proposedBy.FNAME + " " + proposedBy.LNAME;
                            break;
                    }
                }
                //if they didn't specify a timezone (prior to being required), set it to utc
                if (InstrModel.RetrInstrStatus != null)
                {
                    if (string.IsNullOrEmpty(InstrModel.RetrInstrStatus.TIME_ZONE) || InstrModel.RetrInstrStatus.TIME_ZONE != "UTC")
                    {
                        InstrModel.RetrInstrStatus.TIME_ZONE = "UTC";
                        InstrModel.RetrInstrStatus.TIME_STAMP = InstrModel.RetrInstrStatus.TIME_STAMP.Value.ToUniversalTime();
                    }
                }
                //if they didn't specify a timezone (prior to being required), set it to utc
                if (InstrModel.DeplInstrStatus != null)
                {
                    if (string.IsNullOrEmpty(InstrModel.DeplInstrStatus.TIME_ZONE) || InstrModel.DeplInstrStatus.TIME_ZONE != "UTC")
                    {
                        InstrModel.DeplInstrStatus.TIME_ZONE = "UTC";
                        InstrModel.DeplInstrStatus.TIME_STAMP = InstrModel.DeplInstrStatus.TIME_STAMP.Value.ToUniversalTime();
                    }
                }
                //if they didn't specify a timezone (prior to being required), set it to utc
                if (InstrModel.LostInstrStatus != null)
                {
                    if (string.IsNullOrEmpty(InstrModel.LostInstrStatus.TIME_ZONE) || InstrModel.LostInstrStatus.TIME_ZONE != "UTC")
                    {
                        InstrModel.LostInstrStatus.TIME_ZONE = "UTC";
                        InstrModel.LostInstrStatus.TIME_STAMP = InstrModel.LostInstrStatus.TIME_STAMP.Value.ToUniversalTime();
                    }
                }
                //if they didn't specify a timezone (prior to being required), set it to utc
                if (InstrModel.PropInstrStatus != null)
                {
                    if (string.IsNullOrEmpty(InstrModel.PropInstrStatus.TIME_ZONE) || InstrModel.PropInstrStatus.TIME_ZONE != "UTC")
                    {
                        InstrModel.PropInstrStatus.TIME_ZONE = "UTC";
                        if (InstrModel.PropInstrStatus.TIME_STAMP.HasValue)
                        {
                            InstrModel.PropInstrStatus.TIME_STAMP = InstrModel.PropInstrStatus.TIME_STAMP.Value.ToUniversalTime();
                        }
                    }
                }
                //get event if one
                if (anInstrument.EVENT_ID != null && anInstrument.EVENT_ID != 0)
                {
                    ViewData["anEvent"] = GetAnEvent(anInstrument.EVENT_ID).EVENT_NAME;
                }

                //get Sensor Type if one
                if (anInstrument.SENSOR_TYPE_ID != null)
                {
                    request = new RestRequest();
                    request.Resource = "/SensorTypes/{entityId}";
                    request.RootElement = "SENSOR_TYPE";
                    request.AddParameter("entityId", anInstrument.SENSOR_TYPE_ID, ParameterType.UrlSegment);
                    string aSensor = serviceCaller.Execute<SENSOR_TYPE>(request).SENSOR;
                    ViewData["SensorType"] = aSensor;
                }

                //get Sensor Brand if one
                if (anInstrument.SENSOR_BRAND_ID != null)
                {
                    request = new RestRequest();
                    request.Resource = "/SensorBrands/{entityId}";
                    request.RootElement = "SENSOR_BRAND";
                    request.AddParameter("entityId", anInstrument.SENSOR_BRAND_ID, ParameterType.UrlSegment);
                    string aSenBrand = serviceCaller.Execute<SENSOR_BRAND>(request).BRAND_NAME;
                    ViewData["SensorBrand"] = aSenBrand;
                }

                ////get deployment_type if one
                if (anInstrument.DEPLOYMENT_TYPE_ID != null && anInstrument.DEPLOYMENT_TYPE_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/DeploymentTypes/{entityId}";
                    request.RootElement = "DEPLOYMENT_TYPE";
                    request.AddParameter("entityId", anInstrument.DEPLOYMENT_TYPE_ID, ParameterType.UrlSegment);
                    string aDepType = serviceCaller.Execute<DEPLOYMENT_TYPE>(request).METHOD;
                    ViewData["DeplType"] = aDepType;
                }
                ////get housing_type if one
                if (anInstrument.HOUSING_TYPE_ID != null && anInstrument.HOUSING_TYPE_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/HousingTypes/{entityId}";
                    request.RootElement = "HOUSING_TYPE";
                    request.AddParameter("entityId", anInstrument.HOUSING_TYPE_ID, ParameterType.UrlSegment);
                    string aHouseType = serviceCaller.Execute<HOUSING_TYPE>(request).TYPE_NAME;
                    ViewData["HouseType"] = aHouseType;
                }
                //get tape down info if any
                OP_MEASUREMENTS opMeasure = new OP_MEASUREMENTS();
                if (InstrModel.DeplInstrStatus != null)
                {
                    opMeasure = getOPMeasures(InstrModel.DeplInstrStatus.INSTRUMENT_STATUS_ID);
                
                    if (opMeasure != null)
                    {
                        ViewData["StatusOPmeasurements"] = BuildOPMeasModel(opMeasure);
                    }
                    
                    if (InstrModel.DeplInstrStatus.COLLECTION_TEAM_ID != 0 && InstrModel.DeplInstrStatus.COLLECTION_TEAM_ID != null)
                    {
                        ViewData["teamName"] = getTeam(InstrModel.DeplInstrStatus.COLLECTION_TEAM_ID).DESCRIPTION;
                    }
                }                

                return PartialView(InstrModel);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }
                
        // Deployed Instrument's Details
        // GET: /Instruments/DetailsPV/1
        public ActionResult ProposedDetails(int id)
        {
            try
            {
                InstrumentModel InstrModel = new InstrumentModel();
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //get this instrument
                INSTRUMENT anInstrument = getThisInstrument(id);
                //get Site number
                ViewData["SiteNo"] = getThisSite(anInstrument.SITE_ID).SITE_NO;

                InstrModel.Instr = anInstrument;

                //pull from log and grab the proposed one for top details
                InstrModel.InstrStatusList = getInstrumentStatusLog(id);

                //just want the proposed one here
                foreach (INSTRUMENT_STATUS IS in InstrModel.InstrStatusList)
                {
                    if (IS.STATUS_TYPE_ID == 4)
                    {
                        InstrModel.PropInstrStatus = IS;
                    }
                }

                //get member who proposed it (stored in InstrStatus.COLLECTION_TEAM_ID -- just for proposed)
                request = new RestRequest();
                request.Resource = "Members/{entityId}";
                request.RootElement = "MEMBER";
                request.AddParameter("entityId", InstrModel.PropInstrStatus.COLLECTION_TEAM_ID, ParameterType.UrlSegment);
                MEMBER proposedBy = serviceCaller.Execute<MEMBER>(request);
                ViewData["proposedBy"] = proposedBy.FNAME + " " + proposedBy.LNAME;

                //get event if one
                if (anInstrument.EVENT_ID != null && anInstrument.EVENT_ID != 0)
                {
                    ViewData["anEvent"] = GetAnEvent(anInstrument.EVENT_ID).EVENT_NAME;
                }

                //get Sensor Type if one
                if (anInstrument.SENSOR_TYPE_ID != null)
                {
                    request = new RestRequest();
                    request.Resource = "/SensorTypes/{entityId}";
                    request.RootElement = "SENSOR_TYPE";
                    request.AddParameter("entityId", anInstrument.SENSOR_TYPE_ID, ParameterType.UrlSegment);
                    ViewData["SensorType"] = serviceCaller.Execute<SENSOR_TYPE>(request).SENSOR;
                }

                //get Sensor Brand if one
                if (anInstrument.SENSOR_BRAND_ID != null)
                {
                    request = new RestRequest();
                    request.Resource = "/SensorBrands/{entityID}";
                    request.RootElement = "SENSOR_BRAND";
                    request.AddParameter("entityID", anInstrument.SENSOR_BRAND_ID, ParameterType.UrlSegment);
                    ViewData["SensorBrand"] = serviceCaller.Execute<SENSOR_BRAND>(request).BRAND_NAME;
                }

                ////get deployment_type if one
                if (anInstrument.DEPLOYMENT_TYPE_ID != null && anInstrument.DEPLOYMENT_TYPE_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/DeploymentTypes/{entityId}";
                    request.RootElement = "DEPLOYMENT_TYPE";
                    request.AddParameter("entityId", anInstrument.DEPLOYMENT_TYPE_ID, ParameterType.UrlSegment);
                    ViewData["DeplType"] = serviceCaller.Execute<DEPLOYMENT_TYPE>(request).METHOD;
                }

                return View(InstrModel);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // Deployed Instrument's Edit
        // GET: /Instruments/Edit/5
        public PartialViewResult DepInstrEditPV(int id)
        {
            try
            {
                InstrumentModel InstrModel = new InstrumentModel();

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;

                //get this instrument
                InstrModel.Instr = getThisInstrument(id);

                ViewData["SiteNo"] = getThisSite(InstrModel.Instr.SITE_ID).SITE_NO;
                //pull from log (newest on top)
                InstrModel.InstrStatusList = getInstrumentStatusLog(id);

                //get the deployed one from this list
                InstrModel.DeplInstrStatus = InstrModel.InstrStatusList.SingleOrDefault(inst => inst.STATUS_TYPE_ID == 1);

                //make sure the timezone is utc
                if (InstrModel.DeplInstrStatus.TIME_STAMP.HasValue)
                {
                    if (InstrModel.DeplInstrStatus.TIME_ZONE != "UTC")
                    {
                        InstrModel.DeplInstrStatus.TIME_ZONE = "UTC";
                        InstrModel.DeplInstrStatus.TIME_STAMP = InstrModel.DeplInstrStatus.TIME_STAMP.Value.ToUniversalTime();
                    }
                }
                ViewData["TeamName"] = InstrModel.DeplInstrStatus.COLLECTION_TEAM_ID != null ? getTeam(InstrModel.DeplInstrStatus.COLLECTION_TEAM_ID).DESCRIPTION : "";

                //get sensor types for dropdown
                var request = new RestRequest();
                request.Resource = "/SensorTypes";
                request.RootElement = "ArrayOfSENSOR_TYPE";
                List<SENSOR_TYPE> SensTypeList = serviceCaller.Execute<List<SENSOR_TYPE>>(request);
                ViewData["SensorTypes"] = SensTypeList;

                //get sensor brands for dropdown
                request = new RestRequest();
                request.Resource = "/SensorBrands";
                request.RootElement = "ArrayOfSENSOR_BRAND";
                List<SENSOR_BRAND> SensBrandList = serviceCaller.Execute<List<SENSOR_BRAND>>(request);
                ViewData["SensorBrands"] = SensBrandList;

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

                ////get sensor deployment types for dropdown
                request = new RestRequest();
                request.Resource = "/SensorTypes/{sensorTypeID}/DeploymentTypes";
                request.RootElement = "ArrayOfDEPLOYMENT_TYPE";
                request.AddParameter("sensorTypeID", InstrModel.Instr.SENSOR_TYPE_ID, ParameterType.UrlSegment);
                List<DEPLOYMENT_TYPE> DepTypeList = serviceCaller.Execute<List<DEPLOYMENT_TYPE>>(request);
                ViewData["DeplTypes"] = DepTypeList;

                //get all housing types
                request = new RestRequest();
                request.Resource = "/HousingTypes";
                request.RootElement = "ArrayOfHOUSING_TYPE";
                ViewData["HousingTypes"] = serviceCaller.Execute<List<HOUSING_TYPE>>(request);

                //get tape down info if any
                OP_MEASUREMENTS opMeasures = getOPMeasures(InstrModel.DeplInstrStatus.INSTRUMENT_STATUS_ID);

                if (opMeasures != null)
                {
                    ViewData["StatusOPmeasurements"] = BuildOPMeasModel(opMeasures);
                }

                //get the site's OPs for instrumentStatus
                request = new RestRequest();
                request.Resource = "/Sites/{siteId}/ObjectivePoints";
                request.RootElement = "ArrayOfOBJECTIVE_POINT";
                request.AddParameter("siteId", InstrModel.Instr.SITE_ID, ParameterType.UrlSegment);
                List<OBJECTIVE_POINT> siteOPs = serviceCaller.Execute<List<OBJECTIVE_POINT>>(request);
                if (siteOPs.Count >= 1)
                    ViewData["SiteOPs"] = siteOPs;

                //get events for drowdown
                ViewData["EventList"] = GetEvents();

                //Pass Instrument Status
                ViewData["StatusTypes"] = "Deployed";

                //Pass to view
                return PartialView("DepInstrEditPV", InstrModel);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        // Deployed Instrument's Post Edit
        //POST: /Instruments/Edit/5
        [HttpPost]
        public PartialViewResult DepInstrEdit(int id, InstrumentModel anInstrument)
        {
            try
            {
                INSTRUMENT thisInstr = anInstrument.Instr;
                INSTRUMENT_STATUS thisInstrStatus = anInstrument.DeplInstrStatus;
                OP_MEASUREMENTS thisOPMeasures = anInstrument.OPMeas;

                //get radio button choice (Minutes or seconds)
                var radio = Convert.ToString(Request.Form["IntervalUnit"]);
                //Interval is stored as seconds, do conversion based on radio
                if (radio == "Minute")
                {
                    thisInstr.INTERVAL = thisInstr.INTERVAL * 60;
                }

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/Instruments/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", thisInstr.INSTRUMENT_ID, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<INSTRUMENT>(thisInstr), ParameterType.RequestBody);
                INSTRUMENT updatedInstrument = serviceCaller.Execute<INSTRUMENT>(request);

                //check to see if timestamp is still in utc, if not convert it.
                if (thisInstrStatus.TIME_ZONE != "UTC")
                {
                    thisInstrStatus.TIME_ZONE = "UTC";
                    thisInstrStatus.TIME_STAMP = thisInstrStatus.TIME_STAMP.Value.ToUniversalTime();
                }

                request = new RestRequest(Method.POST);
                request.Resource = "/InstrumentStatus/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", thisInstrStatus.INSTRUMENT_STATUS_ID, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //use extended serializer
                serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<INSTRUMENT_STATUS>(thisInstrStatus), ParameterType.RequestBody);
                INSTRUMENT_STATUS updatedInstStat = serviceCaller.Execute<INSTRUMENT_STATUS>(request);

                if (thisOPMeasures.OBJECTIVE_POINT_ID < 1)
                {
                    //No obj Pt Id or it's 0 means delete or edit existing
                    //get it 
                    OP_MEASUREMENTS thisOne = getOPMeasures(updatedInstStat.INSTRUMENT_STATUS_ID);
                    
                    //see if checkbox was clicked to remove this Tape Down
                    var remove = Convert.ToString(Request.Form["RemoveThis"]);
                    if (thisOne != null)
                    {
                        if (remove == "1")
                        {
                            //remove it
                            request = new RestRequest(Method.POST);
                            request.Resource = "/OPMeasurements/{entityId}";
                            request.AddParameter("entityId", thisOne.OP_MEASUREMENTS_ID, ParameterType.UrlSegment);
                            request.AddHeader("X-HTTP-Method-Override", "DELETE");
                            request.AddHeader("Content-Type", "application/xml");
                            serviceCaller.Execute<OP_MEASUREMENTS>(request);
                        }
                        else
                        {
                            //no OBJPT_ID and remove isn't checked - just edited the existing one
                            //put the objective point id back in
                            thisOPMeasures.OBJECTIVE_POINT_ID = thisOne.OBJECTIVE_POINT_ID;

                            request = new RestRequest(Method.POST);
                            request.Resource = "/OPMeasurements/{entityId}";
                            request.RequestFormat = DataFormat.Xml;
                            request.AddParameter("entityId", thisOPMeasures.OP_MEASUREMENTS_ID, ParameterType.UrlSegment);
                            request.AddHeader("X-HTTP-Method-Override", "PUT");
                            request.AddHeader("Content-Type", "application/xml");
                            //use extended serializer
                            serializer = new STNWebSerializer();
                            request.AddParameter("application/xml", serializer.Serialize<OP_MEASUREMENTS>(thisOPMeasures), ParameterType.RequestBody);
                            serviceCaller.Execute<OP_MEASUREMENTS>(request);
                        }
                    }
                }
                else
                {
                    //thisOPMeasures != null and/or objPtId has a value - they are posting a new one
                    PostOPMeasurement(updatedInstStat.INSTRUMENT_STATUS_ID, thisOPMeasures);
                }
              
                ViewData["teamName"] = getTeam(thisInstrStatus.COLLECTION_TEAM_ID).DESCRIPTION;

                //get viewData props for details page
                //get event if one
                if (thisInstr.EVENT_ID != null && thisInstr.EVENT_ID != 0)
                {
                    ViewData["anEvent"] = GetAnEvent(thisInstr.EVENT_ID).EVENT_NAME;
                }

                //get Sensor Type if one
                if (thisInstr.SENSOR_TYPE_ID != null && thisInstr.SENSOR_TYPE_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/SensorTypes/{entityId}";
                    request.RootElement = "SENSOR_TYPE";
                    request.AddParameter("entityId", thisInstr.SENSOR_TYPE_ID, ParameterType.UrlSegment);
                    string aSensor = serviceCaller.Execute<SENSOR_TYPE>(request).SENSOR;
                    ViewData["SensorType"] = aSensor;
                }
                ////get deployment_type if one
                if (thisInstr.DEPLOYMENT_TYPE_ID != null && thisInstr.DEPLOYMENT_TYPE_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/DeploymentTypes/{entityId}";
                    request.RootElement = "DEPLOYMENT_TYPE";
                    request.AddParameter("entityId", thisInstr.DEPLOYMENT_TYPE_ID, ParameterType.UrlSegment);
                    string aDepType = serviceCaller.Execute<DEPLOYMENT_TYPE>(request).METHOD;
                    ViewData["DeplType"] = aDepType;
                }
                
                //Get Instrument Status
                if (thisInstrStatus.STATUS_TYPE_ID != null && thisInstrStatus.STATUS_TYPE_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/StatusTypes/{entityId}";
                    request.RootElement = "STATUS_TYPE";
                    request.AddParameter("entityId", thisInstrStatus.STATUS_TYPE_ID, ParameterType.UrlSegment);
                    string statType = serviceCaller.Execute<STATUS_TYPE>(request).STATUS;
                    ViewData["StatType"] = statType;
                }
                //get tape down info if any
                OP_MEASUREMENTS opMeasures = getOPMeasures(thisInstrStatus.INSTRUMENT_STATUS_ID);

                if (opMeasures != null)
                {
                    ViewData["StatusOPmeasurements"] = BuildOPMeasModel(opMeasures);
                }

                InstrumentModel updatedModel = new InstrumentModel();
                updatedModel.Instr = updatedInstrument;
                updatedModel.DeplInstrStatus = updatedInstStat;
                

                //pull from log and grab the deployed one for top details
                updatedModel.InstrStatusList = getInstrumentStatusLog(updatedInstrument.INSTRUMENT_ID);

                return PartialView("DepInstrDetailsPV", updatedModel);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        // Retrieved Instrument's Details
        // GET: /Intruments/InstrRetrievedDetails/3
        public PartialViewResult RetInstrDetailsPV(int id)
        {
            try
            {
                //get instrument status log
                List<INSTRUMENT_STATUS> InStList = getInstrumentStatusLog(id);
                //get this instrument
                INSTRUMENT thisInstrument = getThisInstrument(id);

                InstrumentModel InstrModel = new InstrumentModel();
                InstrModel.Instr = thisInstrument;

                //get each one in the model
                foreach (INSTRUMENT_STATUS IS in InStList)
                {
                    switch (Convert.ToInt32(IS.STATUS_TYPE_ID))
                    {
                        case 1:
                            InstrModel.DeplInstrStatus = IS;
                            break;
                        case 2:
                            InstrModel.RetrInstrStatus = IS;
                            break;
                        case 3:
                            InstrModel.LostInstrStatus = IS;
                            break;
                        default:
                            InstrModel.PropInstrStatus = IS;
                            break;
                    }
                }

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                //get tape down info if any (first make sure this is a retrieval details and not a lost details
                if (InstrModel.RetrInstrStatus != null)
                {
                    //make sure timezone and timestamp are in utc
                    if (InstrModel.RetrInstrStatus.TIME_STAMP.HasValue)
                    {
                        if (InstrModel.RetrInstrStatus.TIME_ZONE != "UTC")
                        {
                            InstrModel.RetrInstrStatus.TIME_ZONE = "UTC";
                            InstrModel.RetrInstrStatus.TIME_STAMP = InstrModel.RetrInstrStatus.TIME_STAMP.Value.ToUniversalTime();
                        }
                    }

                    OP_MEASUREMENTS opMeasures = getOPMeasures(InstrModel.RetrInstrStatus.INSTRUMENT_STATUS_ID);

                    if (opMeasures != null)
                    {
                        ViewData["StatusOPmeasurements"] = BuildOPMeasModel(opMeasures);
                    }
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
                if (InstrModel.RetrInstrStatus != null)
                {
                    if (InstrModel.RetrInstrStatus.COLLECTION_TEAM_ID != null)
                    {
                        //been retrieved an not lost
                        ViewData["teamName"] = getTeam(InstrModel.RetrInstrStatus.COLLECTION_TEAM_ID).DESCRIPTION;
                    }
                    ViewData["StatType"] = "Retrieved";
                }
                else if (InstrModel.LostInstrStatus != null)
                {
                    //been lost and not retrieved
                    //if (InstrModel.RetrInstrStatus.COLLECTION_TEAM_ID != null)
                    //{
                        ViewData["teamName"] = getTeam(InstrModel.LostInstrStatus.COLLECTION_TEAM_ID).DESCRIPTION;
                    //}
                    ViewData["StatType"] = "Lost";
                }

                return PartialView(InstrModel);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        // Retrieved Instrument's Edit
        // GET: /Intruments/RetInstrEditPV/3
        public PartialViewResult RetInstrEditPV(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/Instruments/{instrumentId}/InstrumentStatus";
                request.RootElement = "INSTRUMENT_STATUS";
                request.AddParameter("instrumentId", id, ParameterType.UrlSegment);
                INSTRUMENT_STATUS thisInstrStatus = serviceCaller.Execute<INSTRUMENT_STATUS>(request);

                //make sure timezone and timestamp are in utc
                if (thisInstrStatus.TIME_STAMP.HasValue)
                {
                    if (thisInstrStatus.TIME_ZONE != "UTC")
                    {
                        thisInstrStatus.TIME_ZONE = "UTC";
                        thisInstrStatus.TIME_STAMP = thisInstrStatus.TIME_STAMP.Value.ToUniversalTime();
                    }
                }
                //get this instrument
                INSTRUMENT thisInstrument = getThisInstrument(thisInstrStatus.INSTRUMENT_ID);
                InstrumentModel InstModel = new InstrumentModel();
                InstModel.Instr = thisInstrument;

                switch (Convert.ToInt32(thisInstrStatus.STATUS_TYPE_ID))
                {
                    case 2:
                        InstModel.RetrInstrStatus = thisInstrStatus;
                        ViewData["StatType"] = "Retrieved";
                        break;
                    case 3:
                        InstModel.LostInstrStatus = thisInstrStatus;
                        ViewData["StatType"] = "Lost";
                        break;
                }

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

                //get the site's OPs for instrumentStatus
                request = new RestRequest();
                request.Resource = "/Sites/{siteId}/ObjectivePoints";
                request.RootElement = "ArrayOfOBJECTIVE_POINT";
                request.AddParameter("siteId", thisInstrument.SITE_ID, ParameterType.UrlSegment);
                List<OBJECTIVE_POINT> siteOPs = serviceCaller.Execute<List<OBJECTIVE_POINT>>(request);
                if (siteOPs.Count >= 1)
                    ViewData["SiteOPs"] = siteOPs;

                //get tape down info if any
                OP_MEASUREMENTS opMeasures = getOPMeasures(InstModel.RetrInstrStatus.INSTRUMENT_STATUS_ID);

                if (opMeasures != null)
                {
                    ViewData["StatusOPmeasurements"] = BuildOPMeasModel(opMeasures);
                }

                ViewData["teamName"] = thisInstrStatus.COLLECTION_TEAM_ID != null ? getTeam(thisInstrStatus.COLLECTION_TEAM_ID).DESCRIPTION : "";

                //get conditions
                request = new RestRequest();
                request.Resource = "/InstrCollectConditions";
                request.RootElement = "ArrayOfINSTR_COLLECTION_CONDITIONS";
                ViewData["Conditions"] = serviceCaller.Execute<List<INSTR_COLLECTION_CONDITIONS>>(request);

                return PartialView("RetInstrEditPV", InstModel);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }
        
        //POST: post edits to retrieved/lost sensor
        [HttpPost]
        public PartialViewResult RetInstrEdit(int id, InstrumentModel anIM)
        {
            try
            {
                INSTRUMENT thisInstrument = anIM.Instr;
                INSTRUMENT_STATUS thisInStrStat = anIM.RetrInstrStatus != null ? anIM.RetrInstrStatus : anIM.LostInstrStatus;
                OP_MEASUREMENTS thisOPMeasures = anIM.OPMeas;

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/Instruments/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", thisInstrument.INSTRUMENT_ID, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<INSTRUMENT>(thisInstrument), ParameterType.RequestBody);
                INSTRUMENT successINs = serviceCaller.Execute<INSTRUMENT>(request);

                //check to see if timestamp is still in utc, if not convert it.
                if (thisInStrStat.TIME_ZONE != "UTC")
                {
                    thisInStrStat.TIME_ZONE = "UTC";
                    thisInStrStat.TIME_STAMP = thisInStrStat.TIME_STAMP.Value.ToUniversalTime();
                }

                request = new RestRequest(Method.POST);
                request.Resource = "/InstrumentStatus/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", thisInStrStat.INSTRUMENT_STATUS_ID, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //use extended serializer
                serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<INSTRUMENT_STATUS>(thisInStrStat), ParameterType.RequestBody);
                INSTRUMENT_STATUS successIS = serviceCaller.Execute<INSTRUMENT_STATUS>(request);

                if (thisOPMeasures.OBJECTIVE_POINT_ID < 1)
                {
                    //No obj Pt Id or it's 0 means delete or edit existing, or did nothing with none present
                    //get it 
                    OP_MEASUREMENTS thisOne = getOPMeasures(successIS.INSTRUMENT_STATUS_ID);

                    //see if checkbox was clicked to remove this Tape Down
                    var remove = Convert.ToString(Request.Form["RemoveThis"]);
                    if (remove == "1")
                    {
                        //remove it
                        request = new RestRequest(Method.POST);
                        request.Resource = "/OPMeasurements/{entityId}";
                        request.AddParameter("entityId", thisOne.OP_MEASUREMENTS_ID, ParameterType.UrlSegment);
                        request.AddHeader("X-HTTP-Method-Override", "DELETE");
                        request.AddHeader("Content-Type", "application/xml");
                        serviceCaller.Execute<OP_MEASUREMENTS>(request);
                    }
                    else
                    {
                        if (thisOne != null)
                        {
                            //then they're was one originally. could get here with 0 id, meaning they didn't add one or edit, and there wasn't one originally
                            //no OBJPT_ID and remove isn't checked - just edited the existing one
                            //put the objective point id back in
                            thisOPMeasures.OBJECTIVE_POINT_ID = thisOne.OBJECTIVE_POINT_ID;

                            request = new RestRequest(Method.POST);
                            request.Resource = "/OPMeasurements/{entityId}";
                            request.RequestFormat = DataFormat.Xml;
                            request.AddParameter("entityId", thisOPMeasures.OP_MEASUREMENTS_ID, ParameterType.UrlSegment);
                            request.AddHeader("X-HTTP-Method-Override", "PUT");
                            request.AddHeader("Content-Type", "application/xml");
                            //use extended serializer
                            serializer = new STNWebSerializer();
                            request.AddParameter("application/xml", serializer.Serialize<OP_MEASUREMENTS>(thisOPMeasures), ParameterType.RequestBody);
                            serviceCaller.Execute<OP_MEASUREMENTS>(request);
                        }
                    }
                }
                else
                {
                    //thisOPMeasures != null and/or objPtId has a value - they are posting a new one
                    PostOPMeasurement(successIS.INSTRUMENT_STATUS_ID, thisOPMeasures);
                }

                //populate for details page 
                InstrumentModel inModel = new InstrumentModel();
                inModel.Instr = successINs;
                if (anIM.RetrInstrStatus != null)
                {
                    inModel.RetrInstrStatus = successIS;
                    ViewData["StatType"] = "Retrieved";
                }
                else
                {
                    inModel.LostInstrStatus = successIS;
                    ViewData["StatType"] = "Lost";
                }
                //get tape down info if any
                OP_MEASUREMENTS opMeasures = getOPMeasures(anIM.RetrInstrStatus.INSTRUMENT_STATUS_ID);

                if (opMeasures != null)
                {
                    ViewData["StatusOPmeasurements"] = BuildOPMeasModel(opMeasures);
                }

                ViewData["teamName"] = getTeam(successIS.COLLECTION_TEAM_ID).DESCRIPTION;

                //get condition
                request = new RestRequest();
                request.Resource = "/InstrCollectConditions/{entityId}";
                request.RootElement = "INSTR_COLLECTION_CONDITIONS";
                request.AddParameter("entityId", thisInstrument.INST_COLLECTION_ID, ParameterType.UrlSegment);
                ViewData["Condition"] = serviceCaller.Execute<INSTR_COLLECTION_CONDITIONS>(request).CONDITION;
                
                return PartialView("RetInstrDetailsPV", inModel);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        // Retrieve Instrument
        ////GET: /Instruments/Retrieve/5
        public ActionResult InstrRetrievePV(int id)
        {
            //need to choose a collection team first
            if (Session["TeamId"] == null)
            {
                string returnUrl = Request.UrlReferrer.ToString();// "../STNWeb/Instruments/Create?siteId=" + siteID;

                return RedirectToAction("Index", "Home", new { returnUrl = returnUrl });
            }

            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                InstrumentModel instrModel = new InstrumentModel();
                //get this instrument
                instrModel.Instr = getThisInstrument(id);

                //get status types
                var request = new RestRequest();
                request.Resource = "/StatusTypes/";
                request.RootElement = "ArrayOfSTATUS_TYPE";
                List<STATUS_TYPE> StatusType = serviceCaller.Execute<List<STATUS_TYPE>>(request);

                ViewData["StatusTypes"] = StatusType.Where(st => st.STATUS_TYPE_ID != 1 && st.STATUS_TYPE_ID != 4).ToList();

                //get CollectionConditions
                request = new RestRequest();
                request.Resource = "/InstrCollectConditions/";
                request.RootElement = "ArrayOfINSTR_COLLECTION_CONDITIONS";
                List<INSTR_COLLECTION_CONDITIONS> CollectionConditions = serviceCaller.Execute<List<INSTR_COLLECTION_CONDITIONS>>(request);
                ViewData["Conditions"] = CollectionConditions;

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

                //get the site's RPs for instrumentStatus
                request = new RestRequest();
                request.Resource = "/Sites/{siteId}/ObjectivePoints";
                request.RootElement = "ArrayOfOBJECTIVE_POINT";
                request.AddParameter("siteId", instrModel.Instr.SITE_ID, ParameterType.UrlSegment);
                List<OBJECTIVE_POINT> siteOPs = serviceCaller.Execute<List<OBJECTIVE_POINT>>(request);
                if (siteOPs.Count >= 1)
                    ViewData["SiteOPs"] = siteOPs;

                //Pass to view
                return PartialView("InstrRetrievePV", instrModel);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }
        
        //retrieve an instrument. return boolean to AJAX that refreshes whole page
        // POST: /Instruments/Retrieve/5
        [HttpPost]
        //public ActionResult Retrieve(InstrumentModel IM, string submit)
        public JsonResult Retrieve(InstrumentModel IM, string submit)
        {
            INSTRUMENT thisInstrument = IM.Instr;
            INSTRUMENT_STATUS thisInStat = IM.RetrInstrStatus;
            OP_MEASUREMENTS thisOPs = IM.OPMeas;

            try
            {
                //first update the instrument with the new condition
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/Instruments/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", thisInstrument.INSTRUMENT_ID, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<INSTRUMENT>(thisInstrument), ParameterType.RequestBody);
                INSTRUMENT updatedInstr = serviceCaller.Execute<INSTRUMENT>(request);
                
                //find out if they used UTC time, if not, convert from what they chose to UTC for storage.
                if (thisInStat.TIME_ZONE != "UTC")
                {
                    thisInStat.TIME_ZONE = "UTC";
                    thisInStat.TIME_STAMP = thisInStat.TIME_STAMP.Value.ToUniversalTime();
                }
                
                //now post the new status
                request = new RestRequest(Method.POST);
                request.Resource = "InstrumentStatus/";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //use extended serializer
                serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<INSTRUMENT_STATUS>(thisInStat), ParameterType.RequestBody);
                INSTRUMENT_STATUS createdInst = serviceCaller.Execute<INSTRUMENT_STATUS>(request);

                //if Reference Points were chosen, post the OP_MEASUREMENTS
                if (thisOPs != null)
                {
                    if (thisOPs.OBJECTIVE_POINT_ID >= 1)
                    {
                        PostOPMeasurement(createdInst.INSTRUMENT_STATUS_ID, thisOPs);
                    }
                }
                if (submit == "Retrieve")
                {
                    //return RedirectToAction("RetInstrDetailsPV", new { id = thisInstrument.INSTRUMENT_ID }); //
                    return Json("details", JsonRequestBehavior.AllowGet);
                }
                else 
                {
                    //return RedirectToAction("FileCreate", "Files", new { id = thisInstrument.INSTRUMENT_ID, aPage = "Sensor"});//
                    return Json("data", JsonRequestBehavior.AllowGet);
                }

            }
            catch 
            {
                //return PartialView(e.ToString());
                return Json("error", JsonRequestBehavior.AllowGet);
            }
        }
        
        // GET: /Instruments/Create
        public ActionResult Create(int siteID)
        {
            //need to choose a collection team first
            if (Session["TeamId"] == null)
            {
                string returnUrl = "../STNWeb/Instruments/Create?siteId=" + siteID;

                return RedirectToAction("Index", "Home", new { returnUrl = returnUrl });
            }
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                ViewData["SiteID"] = siteID;
                //get the site's OPs for instrumentStatus
                request.Resource = "/Sites/{siteId}/ObjectivePoints";
                request.RootElement = "ArrayOfOBJECTIVE_POINT";
                request.AddParameter("siteId", siteID, ParameterType.UrlSegment);
                List<OBJECTIVE_POINT> siteOPs = serviceCaller.Execute<List<OBJECTIVE_POINT>>(request);
                if (siteOPs.Count >= 1)
                    ViewData["SiteOPs"] = siteOPs;

                //get sensor types for dropdown
                request = new RestRequest();
                request.Resource = "/SensorTypes";
                request.RootElement = "ArrayOfSENSOR_TYPE";
                List<SENSOR_TYPE> SensTypeList = serviceCaller.Execute<List<SENSOR_TYPE>>(request);
                ViewData["SensorTypes"] = SensTypeList;

                //get sensor bra ds for dropdown
                request = new RestRequest();
                request.Resource = "/SensorBrands";
                request.RootElement = "ArrayOfSENSOR_BRAND";
                List<SENSOR_BRAND> SensBrandList = serviceCaller.Execute<List<SENSOR_BRAND>>(request);
                ViewData["SensorBrands"] = SensBrandList;

                //get housing types for dropdown
                request = new RestRequest();
                request.Resource = "/HousingTypes";
                request.RootElement = "ArrayOfHOUSING_TYPE";
                List<HOUSING_TYPE> HouseTypeList = serviceCaller.Execute<List<HOUSING_TYPE>>(request);
                ViewData["HousingTypes"] = HouseTypeList.OrderBy(x => x.HOUSING_TYPE_ID).ToList();

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

                //get deployment types for dropdown
                request = new RestRequest();
                request.Resource = "/DEPLOYMENT_TYPE/";
                request.RootElement = "ArrayOfDEPLOYMENT_TYPE";
                List<DEPLOYMENT_TYPE> DepTypeList = serviceCaller.Execute<List<DEPLOYMENT_TYPE>>(request);
                ViewData["DeplTypes"] = DepTypeList;

                //get events for drowdown
                ViewData["EventList"] = GetEvents();

                return View();
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        } 

        //deploy a Proposed Sensor
        //GET:
        public ActionResult DeployProposedSensor(int id, int siteId)
        {
            //need to choose a collection team first
            if (Session["TeamId"] == null)
            {
                //grab the url to bring back there
                string returnUrl = "../STNWeb/Instruments/DeployProposedSensor/" + id + "?siteId=" + siteId;
                return RedirectToAction("Index", "Home", new { returnUrl = returnUrl });
            }
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //get the instrument
                request.Resource = "/Instruments/{entityId}";
                request.RootElement = "INSTRUMENT";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                INSTRUMENT propInst = serviceCaller.Execute<INSTRUMENT>(request);

                //get the instrument_Status
                request = new RestRequest();
                request.Resource = "/Instruments/{instrumentId}/InstrumentStatus";
                request.RootElement = "INSTRUMENT_STATUS";
                request.AddParameter("instrumentId", id, ParameterType.UrlSegment);
                INSTRUMENT_STATUS propInstStat = serviceCaller.Execute<INSTRUMENT_STATUS>(request);

                //get housing types for dropdown
                request = new RestRequest();
                request.Resource = "/HousingTypes";
                request.RootElement = "ArrayOfHOUSING_TYPE";
                List<HOUSING_TYPE> HouseTypeList = serviceCaller.Execute<List<HOUSING_TYPE>>(request);
                ViewData["HousingTypes"] = HouseTypeList.OrderBy(x => x.HOUSING_TYPE_ID).ToList();

                //get the site's RPs for instrumentStatus
                request = new RestRequest();
                request.Resource = "/Sites/{siteId}/ObjectivePoints";
                request.RootElement = "ArrayOfOBJECTIVE_POINT";
                request.AddParameter("siteId", siteId, ParameterType.UrlSegment);
                List<OBJECTIVE_POINT> siteOPs = serviceCaller.Execute<List<OBJECTIVE_POINT>>(request);
                if (siteOPs.Count >= 1)
                    ViewData["SiteOPs"] = siteOPs;

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

                //get the member who proposed it (stored in COLLECTION_TEAM_ID only for Proposed Sensors
                request = new RestRequest();
                request.Resource = "/Members/{entityId}";
                request.RootElement = "MEMBER";
                request.AddParameter("entityId", propInstStat.COLLECTION_TEAM_ID, ParameterType.UrlSegment);
                MEMBER proposedBy = serviceCaller.Execute<MEMBER>(request);
                ViewData["ProposedBy"] = proposedBy.FNAME + " " + proposedBy.LNAME;

                //here's deployment type, give me correct sensor type to store
                request = new RestRequest();
                request.Resource = "/DeploymentTypes/{deploymentTypeId}/SensorType";
                request.RootElement = "SENSOR_TYPE";
                request.AddParameter("deploymentTypeId", propInst.DEPLOYMENT_TYPE_ID, ParameterType.UrlSegment);
                SENSOR_TYPE aSensorType = serviceCaller.Execute<SENSOR_TYPE>(request);
                ViewData["aSensorType"] = aSensorType;

                //get all the sensor Types
                request = new RestRequest();
                request.Resource = "/SensorTypes";
                request.RootElement = "ArrayOfSENSOR_TYPE";
                List<SENSOR_TYPE> SensTypeList = serviceCaller.Execute<List<SENSOR_TYPE>>(request);
                ViewData["SensorTypes"] = SensTypeList;

                //store deployment Type
                request = new RestRequest();
                request.Resource = "/DeploymentTypes/{entityId}";
                request.RootElement = "DEPLOYMENT_TYPE";
                request.AddParameter("entityId", propInst.DEPLOYMENT_TYPE_ID, ParameterType.UrlSegment);
                DEPLOYMENT_TYPE aDeploymentType = serviceCaller.Execute<DEPLOYMENT_TYPE>(request);
                ViewData["aDeploymentType"] = aDeploymentType;

                //get sensor brands for dropdown
                request = new RestRequest();
                request.Resource = "/SensorBrands/";
                request.RootElement = "ArrayOfSENSOR_BRAND";
                List<SENSOR_BRAND> SensBrandList = serviceCaller.Execute<List<SENSOR_BRAND>>(request);
                ViewData["SensorBrands"] = SensBrandList;

                //get deployment types for dropdown
                request = new RestRequest();
                request.Resource = "/DeploymentTypes/";
                request.RootElement = "ArrayOfDEPLOYMENT_TYPE";
                List<DEPLOYMENT_TYPE> DepTypeList = serviceCaller.Execute<List<DEPLOYMENT_TYPE>>(request);
                ViewData["DeplTypes"] = DepTypeList;

                //store siteId
                ViewData["siteId"] = siteId;

                //get events for drowdown
                ViewData["EventList"] = GetEvents();

                //store in the model
                InstrumentModel thisPropInstr = new InstrumentModel();
                thisPropInstr.Instr = propInst;
                thisPropInstr.PropInstrStatus = propInstStat;

                return View(thisPropInstr);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // POST: /Instruments/Create
        [HttpPost]
        public ActionResult Create(InstrumentModel IM)
        {
            try
            {
                INSTRUMENT thisInstrument = IM.Instr;
                INSTRUMENT_STATUS thisStatus = IM.DeplInstrStatus;
                OP_MEASUREMENTS thisOPMeasurements = IM.OPMeas;

                //get radio button choice (Minutes or seconds)
                var radio = Convert.ToString(Request.Form["IntervalUnit"]);
                //Interval is stored as seconds, do conversion based on radio
                if (radio == "Minute")
                {
                    //convert interval to seconds
                    thisInstrument.INTERVAL = thisInstrument.INTERVAL * 60;
                }
                
                //find out if they used UTC time, if not, convert from what they chose to UTC for storage.
                if (thisStatus.TIME_ZONE != "UTC")
                {
                    thisStatus.TIME_ZONE = "UTC";
                    thisStatus.TIME_STAMP = thisStatus.TIME_STAMP.Value.ToUniversalTime();
                }

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "Instruments/";
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(thisInstrument);
                INSTRUMENT createdInst = serviceCaller.Execute<INSTRUMENT>(request);

                string siteId = createdInst.SITE_ID.ToString();
                thisStatus.INSTRUMENT_ID = createdInst.INSTRUMENT_ID;
                
                request = new RestRequest(Method.POST);
                request.Resource = "InstrumentStatus/";
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(thisStatus);
                INSTRUMENT_STATUS intStat = serviceCaller.Execute<INSTRUMENT_STATUS>(request);

                //if Reference Points were chosen, post the OP_MEASUREMENTS
                if (thisOPMeasurements != null)
                {
                    PostOPMeasurement(intStat.INSTRUMENT_STATUS_ID, thisOPMeasurements);
                }
                
                return RedirectToAction( "Details", "Instruments", new { id = createdInst.INSTRUMENT_ID });
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //PUT:
        [HttpPost]
        public ActionResult DeployProposed(InstrumentModel IM)
        {
            //POST create code copy/pasted here. need to adjust to make PUTs
            try
            {
                INSTRUMENT thisInstrument = IM.Instr;
                INSTRUMENT_STATUS thisStatus = IM.DeplInstrStatus;
                OP_MEASUREMENTS thisOPMeasurements = IM.OPMeas;

                //get radio button choice (Minutes or seconds)
                var radio = Convert.ToString(Request.Form["IntervalUnit"]);
                //Interval is stored as seconds, do conversion based on radio
                if (radio == "Minute")
                {
                    //convert interval to seconds
                    thisInstrument.INTERVAL = thisInstrument.INTERVAL * 60;
                }

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/Instruments/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", thisInstrument.INSTRUMENT_ID, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<INSTRUMENT>(thisInstrument), ParameterType.RequestBody);
                INSTRUMENT updatedInst = serviceCaller.Execute<INSTRUMENT>(request);

                //find out if they used UTC time, if not, convert from what they chose to UTC for storage.
                if (thisStatus.TIME_ZONE != "UTC")
                {
                    thisStatus.TIME_ZONE = "UTC";
                    thisStatus.TIME_STAMP = thisStatus.TIME_STAMP.Value.ToUniversalTime();
                }

                string siteId = updatedInst.SITE_ID.ToString();
                thisStatus.INSTRUMENT_ID = updatedInst.INSTRUMENT_ID;

                request = new RestRequest(Method.POST);
                request.Resource = "InstrumentStatus/";
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(thisStatus);
                INSTRUMENT_STATUS intStat = serviceCaller.Execute<INSTRUMENT_STATUS>(request);

                //if Reference Points were chosen, post the OP_MEASUREMENTS
                if (thisOPMeasurements != null)
                {
                    PostOPMeasurement(intStat.INSTRUMENT_STATUS_ID, thisOPMeasurements);
                }

                return RedirectToAction("Details", "Sites", new { id = updatedInst.SITE_ID, evId = updatedInst.EVENT_ID });
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // GET: /Instruments/Delete/5
        public ActionResult Delete(int id, int siteID)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;

                //delete the op_measurements, files and instrument_statuses first

                //get Instrument's statuses
                var request = new RestRequest();
                request.Resource = "/Instruments/{instrumentId}/InstrumentStatusLog";
                request.RootElement = "ArrayOfINSTRUMENT_STATUS";
                request.AddParameter("instrumentId", id, ParameterType.UrlSegment);
                List<INSTRUMENT_STATUS> allInstStats = serviceCaller.Execute<List<INSTRUMENT_STATUS>>(request);

                //foreach of these instrument status' - delete the opMeasurement, and then delete the status
                foreach (INSTRUMENT_STATUS insStat in allInstStats)
                {
                    //get the opMeasurement
                    request = new RestRequest();
                    request.Resource = "InstrumentStatus/{instrumentStatusId}/InstrMeasurements";
                    request.RootElement = "ArrayOfOP_MEASUREMENTS";
                    request.AddParameter("instrumentStatusId", insStat.INSTRUMENT_STATUS_ID, ParameterType.UrlSegment);
                    List<OP_MEASUREMENTS> opMeasList = serviceCaller.Execute<List<OP_MEASUREMENTS>>(request);

                    if (opMeasList.Count >= 1)
                    {
                        foreach (OP_MEASUREMENTS opm in opMeasList)
                        {
                            //delete it
                            request = new RestRequest(Method.POST);
                            request.Resource = "/OPMeasurements/{entityId}";
                            request.AddParameter("entityId", opm.OP_MEASUREMENTS_ID, ParameterType.UrlSegment);
                            request.AddHeader("X-HTTP-Method-Override", "DELETE");
                            request.AddHeader("Content-Type", "application/xml");
                            serviceCaller.Execute<OP_MEASUREMENTS>(request);
                        }
                    }

                    //now delete the instrument status
                    request = new RestRequest(Method.POST);
                    request.Resource = "/InstrumentStatus/{entityId}";
                    request.AddParameter("entityId", insStat.INSTRUMENT_STATUS_ID, ParameterType.UrlSegment);
                    request.AddHeader("X-HTTP-Method-Override", "DELETE");
                    request.AddHeader("Content-Type", "application/xml");
                    serviceCaller.Execute<INSTRUMENT_STATUS>(request);
                }

                //get any files to delete
                request = new RestRequest();
                request.Resource = "/Instruments/{instrumentId}/Files";
                request.RootElement = "ArrayOfFILE";
                request.AddParameter("instrumentId", id, ParameterType.UrlSegment);
                List<FILE> theseFiles = serviceCaller.Execute<List<FILE>>(request);

                if (theseFiles.Count >= 1)
                {
                    //see if any are datafiles
                    foreach (FILE f in theseFiles)
                    {
                        if (f.DATA_FILE_ID.HasValue)
                        {
                            //delete it
                            request = new RestRequest(Method.POST);
                            request.Resource = "/DataFiles/{entityId}";
                            request.AddParameter("entityId", f.DATA_FILE_ID, ParameterType.UrlSegment);
                            request.AddHeader("X-HTTP-Method-Override", "DELETE");
                            request.AddHeader("Content-Type", "application/xml");
                            serviceCaller.Execute<DATA_FILE>(request);
                        }
                        //delete the file
                        request = new RestRequest(Method.POST);
                        request.Resource = "/Files/{entityId}";
                        request.AddParameter("entityId", f.DATA_FILE_ID, ParameterType.UrlSegment);
                        request.AddHeader("X-HTTP-Method-Override", "DELETE");
                        request.AddHeader("Content-Type", "application/xml");
                        serviceCaller.Execute<FILE>(request);
                    }
                }

                //now delete the instrument
                request = new RestRequest(Method.POST);
                request.Resource = "Instruments/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<INSTRUMENT>(request);

                return RedirectToAction("Details", "Sites", new { id = siteID });
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //dropdown list values depending on sensor type
        //GET: /Instruments/GetDeptTypeList/1
        public JsonResult GetDeptTypeList(int id)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request = new RestRequest();
            request.Resource = "/SensorTypes/{sensorTypeID}/DeploymentTypes";
            request.RootElement = "ArrayOfDEPLOYMENT_TYPE";
            request.AddParameter("sensorTypeID", id, ParameterType.UrlSegment);
            List<DEPLOYMENT_TYPE> DepTypes = serviceCaller.Execute<List<DEPLOYMENT_TYPE>>(request);

            return Json(DepTypes);
        }
    
        //
        // GET: /Instruments/4/DeploymentLocation
        public PartialViewResult GetDepLocationPV(int id)
        {
            switch (id)
            {
                case 1:
                    return PartialView("SensorDepLocation/LandCreatePV");
                case 2:
                    return PartialView("SensorDepLocation/WaterCreatePV");
                default:
                    return PartialView("SensorDepLocation/BridgeCreatePV");
            }
        }


        #region controller called functions
        //method called several times to get the instrument
        private INSTRUMENT getThisInstrument(decimal? id)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "Instruments/{entityId}";
            request.RootElement = "INSTRUMENT";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            INSTRUMENT thisInstrument = serviceCaller.Execute<INSTRUMENT>(request);

            return thisInstrument;
        }
       
        //method called several times to get collection team name
        private COLLECT_TEAM getTeam(decimal? id)
        {
            //get team
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/CollectionTeams/{entityId}";
            request.RootElement = "COLLECT_TEAM";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            COLLECT_TEAM ct = serviceCaller.Execute<COLLECT_TEAM>(request);
            return ct;
        }

        //method called several times to get the site
        private SITE getThisSite(decimal? id)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "Sites/{entityId}";
            request.RootElement = "SITE";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            SITE thisSite = serviceCaller.Execute<SITE>(request);
            
            return thisSite;
        }

        //method called several times to get the instrument status log
        private List<INSTRUMENT_STATUS> getInstrumentStatusLog(decimal? id)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Instruments/{instrumentId}/InstrumentStatusLog";
            request.RootElement = "ArrayOfInstrumentStatus";
            request.AddParameter("instrumentId", id, ParameterType.UrlSegment);
            List<INSTRUMENT_STATUS> StatusList = serviceCaller.Execute<List<INSTRUMENT_STATUS>>(request);

            return StatusList;
        }

        //method called several places to get anEvent
        private EVENT GetAnEvent(decimal? id)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Events/{entityId}";
            request.RootElement = "EVENT";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            EVENT thisEvent = serviceCaller.Execute<EVENT>(request);
            return thisEvent;
        }

        //method called several places to get allevents
        private List<EVENT> GetEvents()
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Events/";
            request.RootElement = "ArrayOfEvents";
            List<EVENT> EventList = serviceCaller.Execute<List<EVENT>>(request);
            return EventList;
        }

        private string GetObjPointName(decimal p)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/ObjectivePoints/{entityId}";
            request.RootElement = "OBJECTIVE_POINT";
            request.AddParameter("entityId", p, ParameterType.UrlSegment);
            string opName = serviceCaller.Execute<OBJECTIVE_POINT>(request).NAME;
            return opName;
        }

        private OP_MEASUREMENTS getOPMeasures(decimal? insStatId) 
        {
            //get tape down info if any
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/InstrumentStatus/{instrumentStatusId}/InstrMeasurements";
            request.RootElement = "ArrayOfOP_MEASUReMENTS";
            request.AddParameter("instrumentStatusId", insStatId, ParameterType.UrlSegment);
            List<OP_MEASUREMENTS> opMeasList = serviceCaller.Execute<List<OP_MEASUREMENTS>>(request);
            return opMeasList.FirstOrDefault();
        }

        private void PostOPMeasurement(decimal? insStatID, OP_MEASUREMENTS thisOPMeasure)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "InstrumentStatus/{instrumentStatusId}/AddInstrMeasurement";
                request.AddParameter("instrumentStatusId", insStatID, ParameterType.UrlSegment);
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(thisOPMeasure);
                serviceCaller.Execute<OP_MEASUREMENTS>(request);
            }
            catch (Exception e)
            {
                e.ToString();
            }
        }

        private OPMeasModel BuildOPMeasModel(OP_MEASUREMENTS opMeasure) 
        {
            OPMeasModel thisOne = new OPMeasModel();
            thisOne.OPMeasure_ID = opMeasure.OP_MEASUREMENTS_ID;
            thisOne.ObjPointName = GetObjPointName(opMeasure.OBJECTIVE_POINT_ID);
            //thisOne.OP_ID = opMeasure.OBJECTIVE_POINT_ID;  ..don't want this because i am checking for this in the edit to see if a new one was added
            thisOne.InstStat_ID = opMeasure.INSTRUMENT_STATUS_ID;
            thisOne.Type = opMeasure.TYPE;
            thisOne.FromRP = opMeasure.FROM_RP;
            thisOne.HangingLength = opMeasure.HANGING_LENGTH;
            thisOne.WaterSurface = opMeasure.WATER_SURFACE;
            thisOne.GroundSurface = opMeasure.GROUND_SURFACE;

            return thisOne;
        }

        #endregion controller called functions
    }
}