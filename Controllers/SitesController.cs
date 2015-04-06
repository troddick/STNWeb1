//------------------------------------------------------------------------------
//----- SiteController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Populates site resource for the view
//
//discussion:   
//
//     

#region Comments
// 02.11.13 - TR - Create site added to _layout
// 02.10.13 - TR - Search box added to _layout search by site No.
// 02.05.13 - TR - created methods for duplicate code
// 02.04.13 - TR - Added PopupSitesDetails Partial View 
// 01.28.13 - TR - LandownerContact added to Sites
// 01.07.13 - TR - added method to GET address info based on Lat/Long (Nominatim.openstreetmap.org)
// 11.30.12 - TR - Partial view for all side boxes (HWMs, Sensors, Files, 
// 11.26.12 - TR - Added Partial views for details and edit
// 10.31.12 - TR - Added Create
// 09.27.12 - JB - Created from old web app
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
using System.Web.Script.Serialization;

using System.Text.RegularExpressions;

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
    public class SitesController : Controller
    {
        //GET: /Sites/ViewSite
        //query page to retrieve list of sites
        public ActionResult ViewSite()
        {
            try
            {
                ViewBag.CurrentPage = "SITES";

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //populate dropdowns for filters
                request.Resource = "/Events/";
                request.RootElement = "ArrayOfEvent";
                List<EVENT> eventList = serviceCaller.Execute<List<EVENT>>(request);
                List<EVENT> sortedEvents = eventList.OrderBy(x => x.EVENT_STATUS_ID).ThenByDescending(x => x.EVENT_END_DATE).ToList<EVENT>();

                ViewData["EventList"] = sortedEvents;

                request = new RestRequest();
                request.Resource = "/SensorTypes/";
                request.RootElement = "ArrayOfSENSOR_TYPE";
                List<SENSOR_TYPE> sensorList = serviceCaller.Execute<List<SENSOR_TYPE>>(request);
                ViewData["SensorList"] = sensorList;

                ViewData["StateList"] = GetStates();// Enum.GetValues(typeof(STNServices.Handlers.HandlerBase.State)).Cast<STNServices.Handlers.HandlerBase.State>().ToList();

                request = new RestRequest();
                request.Resource = "/NetworkNames/";
                request.RootElement = "ArrayOfNETWORK_NAME";
                List<NETWORK_NAME> netNameList = serviceCaller.Execute<List<NETWORK_NAME>>(request);
                netNameList = netNameList.OrderBy(x => x.NAME).ToList();

                ViewData["NetNameList"] = netNameList;

                return View();
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //here's my parameters, get me the sites
        public PartialViewResult FilteredSites(FormCollection fc)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                string eventId = fc["EVENT_ID"];
                string StateName = fc["STATE_NAME"];
                string SensorType = fc["SENSOR_TYPE"];
                string NetName = fc["NETWORK_NAME"];

                request.Resource = "/Sites?Event={eventId}&State={stateName}&SensorType={sensorTypeId}&NetworkName={networknameId}";
                request.RootElement = "ArrayOfSITE";
                request.AddParameter("eventId", eventId, ParameterType.UrlSegment);
                request.AddParameter("stateName", StateName, ParameterType.UrlSegment);
                request.AddParameter("sensorTypeId", SensorType, ParameterType.UrlSegment);
                request.AddParameter("networknameId", NetName, ParameterType.UrlSegment);
                SiteList theResultingSites = serviceCaller.Execute<SiteList>(request);

                ViewData["ResultingSites"] = theResultingSites.Sites;
                return PartialView(theResultingSites);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //
        //GET: /Sites/SiteDE/5
        [Authorize]
        public ActionResult SiteByNo(string SiteNo)
        {
            try
            {

                string SiteNumber = SiteNo.ToUpper();
                //get the site
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/Sites?bySiteNo={siteNo}";
                request.RootElement = "SITE";
                request.AddParameter("siteNo", SiteNumber, ParameterType.UrlSegment);
                SITE thisSite = serviceCaller.Execute<SITE>(request);
                //make sure site was found
                if (thisSite != null)
                {
                    //get the datum.ABBREV
                    string returned = GetDatum(thisSite.HDATUM_ID);
                    ViewData["aHorizontalDatum"] = returned != "null" ? returned : "not provided";

                    //get event list            
                    request = new RestRequest();
                    request.Resource = "EVENTS?Site={siteId}";
                    request.RootElement = "ArrayOfEvents";
                    request.AddParameter("siteId", thisSite.SITE_ID, ParameterType.UrlSegment);
                    List<EVENT> EventList = serviceCaller.Execute<List<EVENT>>(request);
                    ViewData["EventList"] = EventList;
                    string id = thisSite.SITE_ID.ToString();
                    return Json(id, JsonRequestBehavior.AllowGet);//RedirectToAction("Details", "Sites", new { id = thisSite.SITE_ID });
                }
                else
                {
                    return Json("nothing", JsonRequestBehavior.AllowGet);// RedirectToAction("Mapper", "Home");
                }
            }
            catch (Exception e)
            {
                return Json(e.ToString(), JsonRequestBehavior.AllowGet);
            }
        }
        //
        //GET: /Sites/SiteDE/5
        [Authorize]
        public ActionResult Details(Int32 id, decimal? evId)
        {
            try
            {
                //get the site
                SITE aSite = GetSite(id);

                //get the datum.ABBREV
                string returned = GetDatum(aSite.HDATUM_ID);
                ViewData["aHorizontalDatum"] = returned != "null" ? returned : "not provided";
                
                //get event list
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "EVENTS?Site={siteId}";
                request.RootElement = "ArrayOfEvent";
                request.AddParameter("siteId", id, ParameterType.UrlSegment);
                List<EVENT> EventList = serviceCaller.Execute<List<EVENT>>(request);
                ViewData["EventList"] = EventList;
                //store incoming eventId if one has been chosen
                if (evId != 0 && evId != null)
                {
                    ViewData["EventID"] = evId;
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

                return View("Details", aSite);
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return View("../Shared/Error", e);
            }
        }

        //
        // GET: /Sites/SiteDetailsPV/5
        [Authorize]
        public PartialViewResult SiteDetailsPV(int id)
        {
            try
            {
                SITE site = GetSite(id);

                //make sure latitude is only 6 dec places
                site.LATITUDE_DD = Math.Round(site.LATITUDE_DD, 6);

                string returned = GetDatum(site.HDATUM_ID);
                ViewData["aHorizontalDatum"] = returned != "null" ? returned : "not provided";

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                //get Site Deployment Priority
                if (site.PRIORITY_ID != null && site.PRIORITY_ID > 0)
                {
                    request.Resource = "/Sites/{siteId}/DeploymentPriorities";
                    request.RootElement = "DEPLOYMENT_PRIORITY";
                    request.AddParameter("siteId", site.SITE_ID, ParameterType.UrlSegment);
                    ViewData["aPriority"] = serviceCaller.Execute<DEPLOYMENT_PRIORITY>(request).PRIORITY_NAME;
                }

                //get Site Network Types
                request = new RestRequest();
                request.Resource = "/sites/{siteId}/networkTypes";
                request.RootElement = "ArrayOfNETWORK_TYPE";
                request.AddParameter("siteId", id, ParameterType.UrlSegment);
                List<NETWORK_TYPE> siteNetworksTypes = serviceCaller.Execute<List<NETWORK_TYPE>>(request);

                if (siteNetworksTypes != null)
                    ViewData["siteNetTypes"] = siteNetworksTypes;

                //get Site Network Names
                request = new RestRequest();
                request.Resource = "/sites/{siteId}/networkNames";
                request.RootElement = "ArrayOfNETWORK_NAME";
                request.AddParameter("siteId", id, ParameterType.UrlSegment);
                List<NETWORK_NAME> siteNetworkNames = serviceCaller.Execute<List<NETWORK_NAME>>(request);

                if (siteNetworkNames != null)
                    ViewData["siteNetNames"] = siteNetworkNames;

                //Get Landowner info
                request = new RestRequest();
                request.Resource = "/LandOwners/{entityId}";
                request.RootElement = "LANDOWNERCONTACT";
                request.AddParameter("entityId", site.LANDOWNERCONTACT_ID, ParameterType.UrlSegment);
                LANDOWNERCONTACT theLandowner = serviceCaller.Execute<LANDOWNERCONTACT>(request);

                if (theLandowner != null)
                {
                    string fName = theLandowner.FNAME;
                    string lName = theLandowner.LNAME;
                    string FullName = fName + " " + lName;
                    ViewData["LandOwnerContact"] = FullName;
                }

                //add parts to model for view
                SiteModel siteModel = new SiteModel();

                siteModel.aSite = site;
                siteModel.aLandOwner = theLandowner;

                //Peak Summary Table//
                GetPeakSummaries(id);

                //get horizontal collection Method
                if (site.HCOLLECT_METHOD_ID != null && site.HCOLLECT_METHOD_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/HorizontalMethods/{entityId}";
                    request.RootElement = "HORIZONTAL_COLLECT_METHODS";
                    request.AddParameter("entityId", site.HCOLLECT_METHOD_ID, ParameterType.UrlSegment);
                    HORIZONTAL_COLLECT_METHODS thisHColMethod = serviceCaller.Execute<HORIZONTAL_COLLECT_METHODS>(request);
                    if (thisHColMethod != null)
                    { ViewData["HColMethod"] = thisHColMethod.HCOLLECT_METHOD; }
                }

                //get site housings info if any
                request = new RestRequest();
                request.Resource = "/Sites/{siteId}/SiteHousings";
                request.RootElement = "ArrayOfSITE_HOUSING";
                request.AddParameter("siteId", site.SITE_ID, ParameterType.UrlSegment);
                List<SITE_HOUSING> siteHouses = serviceCaller.Execute<List<SITE_HOUSING>>(request);
                List<SiteHousingModel> SiteHouse_ModelList = new List<SiteHousingModel>();
                if (siteHouses != null)
                {
                    if (siteHouses.Count >= 1)
                    {
                        foreach (SITE_HOUSING rpm in siteHouses)
                        {
                            SiteHousingModel thisOne = new SiteHousingModel(
                                rpm.SITE_HOUSING_ID, rpm.SITE_ID, rpm.HOUSING_TYPE_ID,
                                GetHousingTypeName(rpm.HOUSING_TYPE_ID), rpm.LENGTH,
                                rpm.MATERIAL, rpm.NOTES, rpm.AMOUNT);

                            SiteHouse_ModelList.Add(thisOne);
                        }
                        ViewData["SiteHousings"] = SiteHouse_ModelList;
                    }
                }

                //see if there's any site sketch files
                request = new RestRequest();
                request.Resource = "/Sites/{siteId}/Files";
                request.RootElement = "ArrayOfFILE";
                request.AddParameter("siteId", site.SITE_ID, ParameterType.UrlSegment);
                List<FILE> siteFiles = serviceCaller.Execute<List<FILE>>(request);
                if (siteFiles.Count >= 1)
                {
                    List<FILE> sketchFiles = siteFiles.Where(x => x.FILETYPE_ID == 6).ToList();
                    if (sketchFiles != null && sketchFiles.Count >= 1)
                        ViewData["sketchFiles"] = sketchFiles;
                }

                //get creator
                request = new RestRequest();
                request.Resource = "/Members/{entityId}";
                request.RootElement = "MEMBER";
                request.AddParameter("entityId", site.MEMBER_ID, ParameterType.UrlSegment);
                MEMBER creator = serviceCaller.Execute<MEMBER>(request);
                if (creator != null)
                    ViewData["creator"] = creator.FNAME + " " + creator.LNAME;

                return PartialView("SiteDetailsPV", siteModel);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //
        // GET: /Sites/GetAddress
        public object GetAddress(string latitude, string longitude)
        {
            try
            {
                HttpWebRequest request = null;
                HttpWebResponse response = null;
                object result = null;

                request = WebRequest.Create(String.Format(
                    "http://nominatim.openstreetmap.org/reverse?format=xml&lat={0}&lon={1}", latitude, longitude)) as HttpWebRequest;

                using (response = request.GetResponse() as HttpWebResponse)
                {
                    System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream());

                    result = reader.ReadToEnd();
                }//end using

                return result;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        
        //
        //GET: /Sites/2/ObjectivePoints
        [Authorize]
        public Boolean CheckOP(int siteId)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                Boolean exists = false;
                request.Resource = "Sites/{SiteId}/ObjectivePoints";
                request.RootElement = "ArrayOfOBJECTIVE_POINT";
                request.AddParameter("SiteId", siteId, ParameterType.UrlSegment);
                List<OBJECTIVE_POINT> opList = serviceCaller.Execute<List<OBJECTIVE_POINT>>(request);
                if (opList.Count > 0)
                { exists = true; }

                return exists;
            }
            catch 
            {
                return false;
            }
        }

        //
        // GET: /Sites/Create
        [Authorize]
        public ActionResult Create(decimal? latitude, decimal? longitude)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //get HDatum for dropdown
                request.Resource = "/HorizontalDatums";
                request.RootElement = "ArrayOfHORIZONTAL_DATUMS";
                ViewData["HDatumList"] = serviceCaller.Execute<List<HORIZONTAL_DATUMS>>(request);

                //Get Horizontal Collection Methods
                request = new RestRequest();
                request.Resource = "/HorizontalMethods";
                request.RootElement = "ArrayOfHORIZONTAL_COLLECT_METHODS";
                ViewData["hCollectMethodList"] = serviceCaller.Execute<List<HORIZONTAL_COLLECT_METHODS>>(request);

                //get housing types for dropdown
                request = new RestRequest();
                request.Resource = "/HousingTypes";
                request.RootElement = "ArrayOfHOUSING_TYPE";
                List<HOUSING_TYPE> HouseTypeList = serviceCaller.Execute<List<HOUSING_TYPE>>(request);
                ViewData["HousingTypes"] = HouseTypeList.OrderBy(x => x.HOUSING_TYPE_ID).ToList();

                //get Priorities for dropdown
                request = new RestRequest();
                request.Resource = "/DeploymentPriorities";
                request.RootElement = "ArrayOfDEPLOYMENT_PRIORITY";
                ViewData["PriorityList"] = serviceCaller.Execute<List<DEPLOYMENT_PRIORITY>>(request);

                //get network types for checkboxes
                request = new RestRequest();
                request.Resource = "/NetworkTypes";
                request.RootElement = "ArrayOfNETWORK_TYPE";
                ViewData["NetworkTypeList"] = serviceCaller.Execute<List<NETWORK_TYPE>>(request);

                //get network names for checkboxes
                request = new RestRequest();
                request.Resource = "/NetworkNames";
                request.RootElement = "ArrayOfNETWORK_NAME";
                ViewData["NetworkNameList"] = serviceCaller.Execute<List<NETWORK_NAME>>(request);

                //get deployment types for checkboxes
                request = new RestRequest();
                request.Resource = "/DeploymentTypes";
                request.RootElement = "ArrayOfDEPLOYMENT_TYPE";
                ViewData["DeploymentTypeList"] = serviceCaller.Execute<List<DEPLOYMENT_TYPE>>(request);

                //if there's a lat and long, pass it on to the Create page to do it's thing
                if (latitude != null)
                {
                    decimal l = latitude.Value;
                    ViewData["latitude"] = Math.Round(l, 6);
                    decimal longi = longitude.Value < 0 ? longitude.Value : longitude.Value * (-1);
                    ViewData["longitude"] = Math.Round(longi, 6);
                    ViewData["HCollectMethod"] = 4; //Map
                    ViewData["HDatum"] = 4; //WGS84
                }
                
                //get the list of states and counties
                ViewData["States"] = GetStates();
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                ViewData["Counties"] = serializer.Serialize(GetCounties());

                return View();
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        } 
        
        //
        // POST: /Sites/Create
        [HttpPost]
        [Authorize]
        public ActionResult Create(SiteModel newSite, string Create)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                //if landowner was populated, post it first and use id for site.landownercontact_id
                if (newSite.aLandOwner.FNAME != null)
                {
                    request.Resource = "/LandOwners";
                    request.RequestFormat = DataFormat.Xml;
                    request.AddHeader("Content-Type", "application/xml");
                    //serialize and post it
                    STNWebSerializer serializer = new STNWebSerializer();
                    request.AddParameter("application/xml", serializer.Serialize<LANDOWNERCONTACT>(newSite.aLandOwner), ParameterType.RequestBody);
                    LANDOWNERCONTACT createdLOC = serviceCaller.Execute<LANDOWNERCONTACT>(request);

                    newSite.aSite.LANDOWNERCONTACT_ID = createdLOC.LANDOWNERCONTACTID;
                }

                //get member logged in
                request = new RestRequest();
                request.Resource = "/Members?username={userName}";
                request.RootElement = "MEMBER";
                request.AddParameter("userName", User.Identity.Name, ParameterType.UrlSegment);
                MEMBER aMember = serviceCaller.Execute<MEMBER>(request);

                newSite.aSite.MEMBER_ID = aMember.MEMBER_ID;
                //make sure long is negative
                newSite.aSite.LONGITUDE_DD = newSite.aSite.LONGITUDE_DD < 0 ? newSite.aSite.LONGITUDE_DD : newSite.aSite.LONGITUDE_DD * (-1);
                //and that it's only 6 dec places
                newSite.aSite.LONGITUDE_DD = Math.Round(newSite.aSite.LONGITUDE_DD, 6);

                request = new RestRequest(Method.POST);
                request.Resource = "/Sites/";
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(newSite.aSite);
                SITE createdSite = serviceCaller.Execute<SITE>(request);
                
                //if housing types were added
                if (newSite.SiteHousings != null)
                {
                    if (newSite.SiteHousings.Count >= 1)
                    {
                        //check to see if they added an amount
                        foreach (SiteHousingModel sh in newSite.SiteHousings)
                        {
                            SITE_HOUSING newOne = new SITE_HOUSING();
                            newOne.HOUSING_TYPE_ID = sh.HOUSING_TYPE_ID;
                            newOne.LENGTH = sh.LENGTH;
                            newOne.MATERIAL = sh.MATERIAL;
                            newOne.NOTES = sh.NOTES;
                            newOne.AMOUNT = sh.AMOUNT;

                            if (sh.AMOUNT != null && sh.AMOUNT >= 1)
                            {
                                // post that one
                                request = new RestRequest(Method.POST);
                                request.Resource = "Site/{siteId}/AddSiteSiteHousing";
                                request.AddParameter("siteId", createdSite.SITE_ID, ParameterType.UrlSegment);
                                request.RequestFormat = DataFormat.Xml;
                                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                                request.AddBody(newOne);
                                serviceCaller.Execute<SITE_HOUSING>(request);
                            }
                        }
                    }
                }

                //foreach networktypeID in the [], get it, then post it with the siteID to "/sites/{siteId}/AddNetworkType"
                if (newSite.SiteNetworkTypes != null)
                {
                    List<NETWORK_TYPE> siteNetworkTypes = new List<NETWORK_TYPE>();
                    foreach (string nt in newSite.SiteNetworkTypes)
                    {
                        if (!string.IsNullOrWhiteSpace(nt))
                        {
                            //get it
                            NETWORK_TYPE thisNetworkType = getNetworkType(Convert.ToDecimal(nt));
                            //now post
                            request = new RestRequest(Method.POST);
                            request.Resource = "/sites/{siteId}/AddNetworkType";
                            request.AddParameter("siteId", createdSite.SITE_ID, ParameterType.UrlSegment);
                            request.RequestFormat = DataFormat.Xml;
                            request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                            request.AddBody(thisNetworkType);
                            siteNetworkTypes = serviceCaller.Execute<List<NETWORK_TYPE>>(request);
                        }
                    }
                }

                //foreach networknameID in the [], get it, then post it with the siteID to "/sites/{siteId}/AddNetworkName"
                if (newSite.SiteNetworkNames != null)
                {
                    List<NETWORK_NAME> siteNetworkNames = new List<NETWORK_NAME>();
                    foreach (string nt in newSite.SiteNetworkNames)
                    {
                        if (!string.IsNullOrWhiteSpace(nt))
                        {
                            //get it
                            NETWORK_NAME thisNetworkName = getNetworkName(Convert.ToDecimal(nt));
                            //now post
                            request = new RestRequest(Method.POST);
                            request.Resource = "/sites/{siteId}/AddNetworkName";
                            request.AddParameter("siteId", createdSite.SITE_ID, ParameterType.UrlSegment);
                            request.RequestFormat = DataFormat.Xml;
                            request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                            request.AddBody(thisNetworkName);
                            siteNetworkNames = serviceCaller.Execute<List<NETWORK_NAME>>(request);
                        }
                    }
                }

                //Proposed Sensors for each one checked, create it for this site with status of Proposed
                if (newSite.ProposedSiteSensors != null)
                {
                    foreach (string i in newSite.ProposedSiteSensors)
                    {
                        //make an instrument
                        INSTRUMENT proposedInst = new INSTRUMENT();
                        proposedInst.DEPLOYMENT_TYPE_ID = Convert.ToDecimal(i);
                        proposedInst.SITE_ID = createdSite.SITE_ID;

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
                        //get the member logged in to store in the collectionTeam id field
                        //will be member id only for proposed sensors
                        
                        proposedInsStat.COLLECTION_TEAM_ID = aMember.MEMBER_ID;
                        //now post it
                        request = new RestRequest(Method.POST);
                        request.Resource = "InstrumentStatus/";
                        request.RequestFormat = DataFormat.Xml;
                        request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                        request.AddBody(proposedInsStat);
                        serviceCaller.Execute<INSTRUMENT_STATUS>(request);
                    }
                }
                //determine which submit button was selected
                if (Create == "Submit\r\n& Add OP") //Submit & Add OP
                {
                    return RedirectToAction("ObjPointCreate", "ObjPoints", new { siteID = createdSite.SITE_ID });
                }
                else if (Create == "Submit\r\n& Add HWM") //Submit & Add HWM
                {
                    return RedirectToAction("Create", "HWMs", new { siteID = createdSite.SITE_ID });
                }
                else //Submit
                {
                    return RedirectToAction("Details", new { id = createdSite.SITE_ID });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //GET: page showing quickHwm 
        //create a quick site/op/hwm
        [Authorize]
        public ActionResult QuickHWM()
        {
            try
            {
                if (Session["TeamId"] == null)
                {
                    //grab the url to bring back there
                    string returnUrl = "../STNWeb/Sites/QuickHWM"; //Request.UrlReferrer.ToString(); 
                    return RedirectToAction("Index", "Home", new { returnUrl = returnUrl });
                }

                return View();
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //partial views for quickHWM
        public PartialViewResult QuickHWM_SITE()
        {
            try
            {

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //get HDatum for dropdown
                request.Resource = "/HorizontalDatums";
                request.RootElement = "ArrayOfHORIZONTAL_DATUMS";
                ViewData["HDatumList"] = serviceCaller.Execute<List<HORIZONTAL_DATUMS>>(request);

                //Get horizontal collection methods
                request = new RestRequest();
                request.Resource = "/HorizontalMethods";
                request.RootElement = "ArrayOfHORIZONTAL_COLLECT_METHODS";
                ViewData["hCollectMethodList"] = serviceCaller.Execute<List<HORIZONTAL_COLLECT_METHODS>>(request);

                //get the list of states and counties
                ViewData["States"] = GetStates();
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                ViewData["Counties"] = serializer.Serialize(GetCounties());

                return PartialView();
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //partial views for quickHWM
        public PartialViewResult QuickHWM_OP()
        {
            try
            {

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //get op types for dropdown
                request.Resource = "/OPTypes";
                request.RootElement = "ArrayOfOBJECTIVE_POINT_TYPE";
                ViewData["OPTypeList"] = serviceCaller.Execute<List<OBJECTIVE_POINT_TYPE>>(request);

                //Get vertical datum choices
                request = new RestRequest();
                request.Resource = "/VerticalDatums";
                request.RootElement = "ArrayOfVERTICAL_DATUMS";
                ViewData["vDatumList"] = serviceCaller.Execute<List<VERTICAL_DATUMS>>(request);

                //Get horizontal collection methods
                request = new RestRequest();
                request.Resource = "/HorizontalMethods";
                request.RootElement = "ArrayOfHORIZONTAL_COLLECT_METHODS";
                ViewData["hCollectMethodList"] = serviceCaller.Execute<List<HORIZONTAL_COLLECT_METHODS>>(request);

                //Get collection method choices
                request = new RestRequest();
                request.Resource = "/VerticalMethods";
                request.RootElement = "ArrayOfVERTICAL_COLLECT_METHODS";
                ViewData["VcollectMethodList"] = serviceCaller.Execute<List<VERTICAL_COLLECT_METHODS>>(request);

                //Get op qualities choices
                request = new RestRequest();
                request.Resource = "/ObjectivePointQualities";
                request.RootElement = "ArrayOfOP_QUALITY";
                ViewData["Qualities"] = serviceCaller.Execute<List<OP_QUALITY>>(request);

                return PartialView();
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //partial views for quickHWM
        public PartialViewResult QuickHWM_HWM()
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //Get HWM type choices
                request.Resource = "/HWMTypes";
                request.RootElement = "ArrayOfHWM_TYPES";
                ViewData["HWMTypesList"] = serviceCaller.Execute<List<HWM_TYPES>>(request);

                //Get collection method choices
                request = new RestRequest();
                request.Resource = "/VerticalMethods";
                request.RootElement = "ArrayOfVERTICAL_COLLECT_METHODS";
                ViewData["VcollectMethodList"] = serviceCaller.Execute<List<VERTICAL_COLLECT_METHODS>>(request);

                //Get HWM quality choices
                request = new RestRequest();
                request.Resource = "/HWMQualities";
                request.RootElement = "ArrayOfHWM_QUALITIES";
                ViewData["HWMQualitiesList"] = serviceCaller.Execute<List<HWM_QUALITIES>>(request);

                //Get vertical datum choices
                request = new RestRequest();
                request.Resource = "/VerticalDatums";
                request.RootElement = "ArrayOfVERTICAL_DATUMS";
                ViewData["vDatumList"] = serviceCaller.Execute<List<VERTICAL_DATUMS>>(request);

                //Get marker choices
                request = new RestRequest();
                request.Resource = "/Markers";
                request.RootElement = "ArrayOfMARKER";
                ViewData["markerList"] = serviceCaller.Execute<List<MARKER>>(request);


                //Get horizontal collection methods
                request = new RestRequest();
                request.Resource = "/HorizontalMethods";
                request.RootElement = "ArrayOfHORIZONTAL_COLLECT_METHODS";
                ViewData["hCollectMethodList"] = serviceCaller.Execute<List<HORIZONTAL_COLLECT_METHODS>>(request);

                return PartialView();
            }
            catch (Exception e)
            {
                string errorPage = System.Configuration.ConfigurationManager.AppSettings["ErrorPage"].ToString();
                    
                return PartialView(errorPage, e);
            }
        }


        //post create a quick site/op/hwm
        [HttpPost]
        [Authorize]
        public ActionResult CreateQuickHWM(QuickHWMModel newQuickHWM, string Create)
        {
            SITE newSite = newQuickHWM.Qsite;
            
            HWM newHWM = newQuickHWM.Qhwm;
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //make sure latitude is 6 decimal places
                newSite.LATITUDE_DD = Math.Round(newSite.LATITUDE_DD, 6);
                //make sure long is negative
                newSite.LONGITUDE_DD = newSite.LONGITUDE_DD < 0 ? newSite.LONGITUDE_DD : newSite.LONGITUDE_DD * (-1);
                //and that it's only 6 decimal places
                newSite.LONGITUDE_DD = Math.Round(newSite.LONGITUDE_DD, 6);

                //get member logged in
                request.Resource = "/Members?username={userName}";
                request.RootElement = "MEMBER";
                request.AddParameter("userName", User.Identity.Name, ParameterType.UrlSegment);
                newSite.MEMBER_ID = serviceCaller.Execute<MEMBER>(request).MEMBER_ID;

                request = new RestRequest(Method.POST);
                request.Resource = "/Sites/";
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(newSite);
                SITE createdSite = serviceCaller.Execute<SITE>(request);

                //build OP
                OBJECTIVE_POINT newOP = new OBJECTIVE_POINT();

                newOP.NAME = newQuickHWM.opmod.NAME;
                newOP.DESCRIPTION = newQuickHWM.opmod.DESCRIPTION;
                if (newQuickHWM.opmod.ELEV_FT > 0)
                {
                    var radio = Convert.ToString(Request.Form["ElevationUnit"]);
                    //elevation is stored in ft, do conversion if == meter
                    if (radio == "meter")
                    {
                        decimal converted = Convert.ToDecimal(newQuickHWM.opmod.ELEV_FT) * 3.2808M;
                        newOP.ELEV_FT = Math.Round(converted, 3);
                    }
                    else
                        newOP.ELEV_FT = newQuickHWM.opmod.ELEV_FT;
                }
                newOP.DATE_ESTABLISHED = newQuickHWM.opmod.DATE_ESTABLISHED;
                newOP.OP_NOTES = newQuickHWM.opmod.OP_NOTES;
                newOP.SITE_ID = createdSite.SITE_ID;
                newOP.VDATUM_ID = newQuickHWM.opmod.VDATUM_ID;
                newOP.LATITUDE_DD = createdSite.LATITUDE_DD;
                newOP.LONGITUDE_DD = createdSite.LONGITUDE_DD; 
                newOP.HDATUM_ID = createdSite.HDATUM_ID;
                newOP.HCOLLECT_METHOD_ID = createdSite.HCOLLECT_METHOD_ID;
                newOP.VCOLLECT_METHOD_ID = newQuickHWM.opmod.VCOLLECT_METHOD_ID;
                newOP.OP_TYPE_ID = newQuickHWM.opmod.OP_TYPE_ID;
                if (newQuickHWM.opmod.UNCERTAINTY > 0)
                {
                    var rad = Convert.ToString(Request.Form["UncertainUnit"]);
                    if (rad == "cm")
                    {
                        decimal convertedV = Convert.ToDecimal(newQuickHWM.opmod.UNCERTAINTY) / 30.48M;
                        newOP.UNCERTAINTY = Math.Round(convertedV, 3);
                    }
                    else
                    {
                        newOP.UNCERTAINTY = newQuickHWM.opmod.UNCERTAINTY;
                    }
                }
                newOP.UNQUANTIFIED = newQuickHWM.opmod.UNQUANTIFIED;
                newOP.OP_QUALITY_ID = newQuickHWM.opmod.OP_QUALITY_ID;
                
                request = new RestRequest(Method.POST);
                request.Resource = "/ObjectivePoints";
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<OBJECTIVE_POINT>(newOP), ParameterType.RequestBody);
                OBJECTIVE_POINT createdOP = serviceCaller.Execute<OBJECTIVE_POINT>(request);

                if (newQuickHWM.opmod.OPIdentifiers != null)
                {
                    if (newQuickHWM.opmod.OPIdentifiers.Count >= 1)
                    {
                        foreach( OP_CONTROL_IDENTIFIER opCI in newQuickHWM.opmod.OPIdentifiers)
                        {
                            //post each one for this OP
                            request = new RestRequest(Method.POST);
                            request.Resource = "ObjectivePoints/{objectivePointId}/AddOPControls";
                            request.AddParameter("objectivePointId", createdOP.OBJECTIVE_POINT_ID, ParameterType.UrlSegment);
                            request.RequestFormat = DataFormat.Xml;
                            request.AddHeader("Content-Type", "application/xml");
                            serializer = new STNWebSerializer();
                            request.AddParameter("application/xml", serializer.Serialize<OP_CONTROL_IDENTIFIER>(opCI), ParameterType.RequestBody);
                            serviceCaller.Execute<OP_CONTROL_IDENTIFIER>(request);
                        }
                    }
                }

                //post hwm
                newHWM.WATERBODY = createdSite.WATERBODY;
                newHWM.SITE_ID = createdSite.SITE_ID;                
                newHWM.EVENT_ID = Convert.ToInt32(Session["EventId"]);
                newHWM.LATITUDE_DD = createdSite.LATITUDE_DD;
                newHWM.LONGITUDE_DD = createdSite.LONGITUDE_DD;
                newHWM.FLAG_TEAM_ID = Convert.ToInt32(Session["TeamId"]);
                newHWM.HCOLLECT_METHOD_ID = createdSite.HCOLLECT_METHOD_ID;
                newHWM.HDATUM_ID = createdSite.HDATUM_ID;
                if (newHWM.SURVEY_DATE != null)
                {
                    newHWM.SURVEY_TEAM_ID = newHWM.FLAG_TEAM_ID;
                }

                request = new RestRequest(Method.POST);
                request.Resource = "HWMs/";
                request.RequestFormat = DataFormat.Xml;
                //Use extended serializer
                serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<HWM>(newHWM), ParameterType.RequestBody);
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                HWM createdHWM = serviceCaller.Execute<HWM>(request);

                //determine which submit button was selected
                if (Create == "Submit\r\n& Upload File") //Submit & Add file
                {
                    return RedirectToAction("FileCreate", "Files", new { Id = createdSite.SITE_ID, aPage = "SITE" });
                }
                else //Submit
                {
                    return RedirectToAction("Details", "Sites", new { id = createdSite.SITE_ID, evId = createdHWM.EVENT_ID });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        private OBJECTIVE_POINT BuildOP(OPModel oPModel)
        {
            OBJECTIVE_POINT newOP = new OBJECTIVE_POINT();

            newOP.NAME = oPModel.NAME;
            newOP.DESCRIPTION = oPModel.DESCRIPTION;
            newOP.DATE_ESTABLISHED = oPModel.DATE_ESTABLISHED;
            newOP.OP_NOTES = oPModel.OP_NOTES;
            newOP.VDATUM_ID = oPModel.VDATUM_ID;
            newOP.VCOLLECT_METHOD_ID = oPModel.VCOLLECT_METHOD_ID;
            newOP.OP_TYPE_ID = oPModel.OP_TYPE_ID;
           
            newOP.UNQUANTIFIED = oPModel.UNQUANTIFIED;
            newOP.OP_QUALITY_ID = oPModel.OP_QUALITY_ID;

            return newOP;
        }
        //
        // GET: /Sites/SiteEditPV/5
        [Authorize]
        public PartialViewResult SiteEditPV(int id)
        {
            try
            {
                SITE aSite = GetSite(id);

                string returned = GetDatum(aSite.HDATUM_ID);
                ViewData["aHorizontalDatum"] = returned != "null" ? returned : "not provided";

                //store site and landowner in model
                SiteModel siteModel = new SiteModel();
                LANDOWNERCONTACT aLandOwner = new LANDOWNERCONTACT();

                //make sure lat is only 6 dec places
                aSite.LATITUDE_DD = Math.Round(aSite.LATITUDE_DD, 6);

                siteModel.aSite = aSite;

                //get network types choices
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "NetworkTypes";
                request.RootElement = "ArrayOfNETWORK_TYPE";
                ViewData["NetworkTypeList"] = serviceCaller.Execute<List<NETWORK_TYPE>>(request);

                //get network names choices
                request = new RestRequest();
                request.Resource = "NetworkNames";
                request.RootElement = "ArrayOfNETWORK_NAME";
                ViewData["NetworkNameList"] = serviceCaller.Execute<List<NETWORK_NAME>>(request);

                //get housing types for dropdown
                request = new RestRequest();
                request.Resource = "HousingTypes";
                request.RootElement = "ArrayOfHOUSING_TYPE";
                List<HOUSING_TYPE> HouseTypeList = serviceCaller.Execute<List<HOUSING_TYPE>>(request);
                ViewData["HousingTypes"] = HouseTypeList.OrderBy(x => x.HOUSING_TYPE_ID).ToList();

                //get this Site's Network Types
                request = new RestRequest();
                request.Resource = "sites/{siteId}/networkTypes";
                request.RootElement = "ArrayOfNETWORK_TYPE";
                request.AddParameter("siteId", id, ParameterType.UrlSegment);
                ViewData["siteNets"] = serviceCaller.Execute<List<NETWORK_TYPE>>(request);

                //get this Site's Network Names
                request = new RestRequest();
                request.Resource = "sites/{siteId}/networkNames";
                request.RootElement = "ArrayOfNETWORK_NAME";
                request.AddParameter("siteId", id, ParameterType.UrlSegment);
                ViewData["siteNetNames"] = serviceCaller.Execute<List<NETWORK_NAME>>(request);

                //Get horizontal datum choices
                request = new RestRequest();
                request.Resource = "HorizontalDatums";
                request.RootElement = "ArrayOfHORIZONTAL_DATUMS";
                ViewData["hDatumList"] = serviceCaller.Execute<List<HORIZONTAL_DATUMS>>(request);

                //Get h collect method
                request = new RestRequest();
                request.Resource = "HorizontalMethods";
                request.RootElement = "ArrayOfHORIZONTAL_COLLECT_METHODS";
                ViewData["hCollectMethodList"] = serviceCaller.Execute<List<HORIZONTAL_COLLECT_METHODS>>(request);

                request = new RestRequest();
                request.Resource = "DeploymentPriorities";
                request.RootElement = "ArrayOfDEPLOYMENT_PRIORITY";
                ViewData["priorityList"] = serviceCaller.Execute<List<DEPLOYMENT_PRIORITY>>(request);

                //get creator
                request = new RestRequest();
                request.Resource = "Members/{entityId}";
                request.RootElement = "MEMBER";
                request.AddParameter("entityId", aSite.MEMBER_ID, ParameterType.UrlSegment);
                MEMBER creator = serviceCaller.Execute<MEMBER>(request);
                if (creator != null)
                    ViewData["creator"] = creator.FNAME + " " + creator.LNAME;

                //Get Landowner info
                request = new RestRequest();
                request.Resource = "LandOwners/{entityId}";
                request.RootElement = "LANDOWNERCONTACT";
                request.AddParameter("entityId", aSite.LANDOWNERCONTACT_ID, ParameterType.UrlSegment);
                aLandOwner = serviceCaller.Execute<LANDOWNERCONTACT>(request);

                //get site housings info if any
                request = new RestRequest();
                request.Resource = "/Sites/{siteId}/SiteHousings";
                request.RootElement = "ArrayOfSITE_HOUSING";
                request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
                List<SITE_HOUSING> siteHouses = serviceCaller.Execute<List<SITE_HOUSING>>(request);
                List<SiteHousingModel> SiteHouse_ModelList = new List<SiteHousingModel>();
                if (siteHouses != null)
                {
                    if (siteHouses.Count >= 1)
                    {
                        //add to comma separated string to populate hidden input
                        string houseTyID = string.Empty;
                        string trimmedHTID = string.Empty;

                        foreach (SITE_HOUSING rpm in siteHouses)
                        {
                            SiteHousingModel thisOne = new SiteHousingModel(
                                rpm.SITE_HOUSING_ID, rpm.SITE_ID, rpm.HOUSING_TYPE_ID,
                                GetHousingTypeName(rpm.HOUSING_TYPE_ID), rpm.LENGTH,
                                rpm.MATERIAL, rpm.NOTES, rpm.AMOUNT);

                            houseTyID += rpm.HOUSING_TYPE_ID + ",";

                            SiteHouse_ModelList.Add(thisOne);
                        }

                        ViewData["SiteHousings"] = SiteHouse_ModelList;

                        trimmedHTID = houseTyID.TrimEnd(',', ' ');
                        siteModel.houseTypeIDs = trimmedHTID;
                    }
                }

                siteModel.SiteHousings = SiteHouse_ModelList;
                siteModel.aLandOwner = aLandOwner;

                ViewData["States"] = GetStates();

                request = new RestRequest();
                request.Resource = "/States/Counties?StateAbbrev={stateAbbrev}";
                request.RootElement = "ArrayOfCOUNTIES";
                request.AddParameter("stateAbbrev", siteModel.aSite.STATE, ParameterType.UrlSegment);
                List<COUNTIES> stateCounties = serviceCaller.Execute<List<COUNTIES>>(request);
                ViewData["StateCounties"] = stateCounties;

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                ViewData["AllCounties"] = serializer.Serialize(GetCounties());               

                return PartialView("SiteEditPV", siteModel);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //
        // POST: /Sites/Edit/5
        [HttpPost]
        [Authorize]
        public ActionResult Edit(int id, SiteModel editedSite)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                
                LANDOWNERCONTACT updatedLOContact = new LANDOWNERCONTACT();

                //see if LandOwner was edited or added
                if (editedSite.aLandOwner.LANDOWNERCONTACTID != 0)
                {
                    //put it
                    request.Resource = "/LandOwners/{entityId}";
                    request.RequestFormat = DataFormat.Xml;
                    request.AddParameter("entityId", editedSite.aLandOwner.LANDOWNERCONTACTID, ParameterType.UrlSegment);
                    request.AddHeader("X-HTTP-Method-Override", "PUT");
                    request.AddHeader("Content-Type", "application/xml");
                    request.AddParameter("application/xml", request.XmlSerializer.Serialize(editedSite.aLandOwner), ParameterType.RequestBody);
                    updatedLOContact = serviceCaller.Execute<LANDOWNERCONTACT>(request);
                }
                else if (editedSite.aLandOwner.LNAME != null && (editedSite.aLandOwner.EMAIL != null || editedSite.aLandOwner.PRIMARYPHONE !=null || editedSite.aLandOwner.ADDRESS != null))
                {
                    //post it ..check to see if they added anything to post
                    request.Resource = "/LandOwners";
                    request.RequestFormat = DataFormat.Xml;
                    request.AddHeader("Content-Type", "application/xml");
                    //serialize and post it
                    STNWebSerializer serializer = new STNWebSerializer();
                    request.AddParameter("application/xml", serializer.Serialize<LANDOWNERCONTACT>(editedSite.aLandOwner), ParameterType.RequestBody);
                    updatedLOContact = serviceCaller.Execute<LANDOWNERCONTACT>(request);
                }
                if (updatedLOContact.LANDOWNERCONTACTID != 0)
                    editedSite.aSite.LANDOWNERCONTACT_ID = updatedLOContact.LANDOWNERCONTACTID;

                request = new RestRequest(Method.POST);
                request.Resource = "Sites/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                request.AddParameter("application/xml", request.XmlSerializer.Serialize(editedSite.aSite), ParameterType.RequestBody);
                SITE updatedSite = serviceCaller.Execute<SITE>(request);

                if (editedSite.SiteHousings != null && editedSite.SiteHousings.Count >= 1)
                {
                    //convert comma separated string to List<decimal> of original housing_type_ids
                    List<Decimal> start_HTIDs = new List<decimal>();

                    if (!string.IsNullOrEmpty(editedSite.houseTypeIDs))
                    {
                        start_HTIDs = editedSite.houseTypeIDs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToDecimal(x)).ToList();
                    }
                    
                    List<SiteHousingModel> PUTones = new List<SiteHousingModel>();

                    //if ones have been added
                    if (editedSite.SiteHousings.Count > start_HTIDs.Count)
                    {
                        //getting all the housing_type_ids
                        var excludedSiteList = editedSite.SiteHousings.Select(g => g.HOUSING_TYPE_ID.Value).ToList();
                        List<decimal> differences = excludedSiteList.Except(start_HTIDs).ToList();
                        // post the differences with these housing_type_ids
                        foreach (SiteHousingModel sHM in editedSite.SiteHousings)
                        {
                            if (differences.Contains(sHM.HOUSING_TYPE_ID.Value))
                            {
                                if (sHM.AMOUNT != null && sHM.AMOUNT > 0)
                                {
                                    SITE_HOUSING thisOne = new SITE_HOUSING();
                                    thisOne.HOUSING_TYPE_ID = sHM.HOUSING_TYPE_ID;
                                    thisOne.LENGTH = sHM.LENGTH;
                                    thisOne.MATERIAL = sHM.MATERIAL;
                                    thisOne.NOTES = sHM.NOTES;
                                    thisOne.AMOUNT = sHM.AMOUNT;

                                    request = new RestRequest(Method.POST);
                                    request.Resource = "Site/{siteId}/AddSiteSiteHousing";
                                    request.AddParameter("siteId", updatedSite.SITE_ID, ParameterType.UrlSegment);
                                    request.RequestFormat = DataFormat.Xml;
                                    request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                                    request.AddBody(thisOne);
                                    serviceCaller.Execute<SITE_HOUSING>(request);
                                }
                            }
                            else
                            {
                                //these are the ones that are the same.
                                if (sHM.AMOUNT == null || sHM.AMOUNT == 0)
                                {
                                    //delete it
                                    request = new RestRequest(Method.POST);
                                    request.Resource = "/SiteHousings/{entityId}";
                                    request.AddParameter("entityId", sHM.SITE_HOUSING_ID, ParameterType.UrlSegment);
                                    request.AddHeader("X-HTTP-Method-Override", "DELETE");
                                    request.AddHeader("Content-Type", "application/xml");
                                    serviceCaller.Execute<SITE_HOUSING>(request);
                                }
                                else
                                {
                                    //put the rest
                                    SITE_HOUSING editOne = new SITE_HOUSING();
                                    editOne.SITE_HOUSING_ID = sHM.SITE_HOUSING_ID;
                                    editOne.HOUSING_TYPE_ID = sHM.HOUSING_TYPE_ID;
                                    editOne.SITE_ID = sHM.SITE_ID;
                                    editOne.LENGTH = sHM.LENGTH;
                                    editOne.MATERIAL = sHM.MATERIAL;
                                    editOne.NOTES = sHM.NOTES;
                                    editOne.AMOUNT = sHM.AMOUNT;

                                    request = new RestRequest(Method.POST);
                                    request.Resource = "/SiteHousings/{entityId}";
                                    request.RequestFormat = DataFormat.Xml;
                                    request.AddParameter("entityId", sHM.SITE_HOUSING_ID, ParameterType.UrlSegment);
                                    request.AddHeader("X-HTTP-Method-Override", "PUT");
                                    request.AddHeader("Content-Type", "application/xml");
                                    request.AddParameter("application/xml", request.XmlSerializer.Serialize(editOne), ParameterType.RequestBody);
                                    serviceCaller.Execute<SITE_HOUSING>(request);
                                }
                            }
                        }
                    } //end if (editedSite.SiteHousings.Count >= 1)
                    else
                    {
                        //they are the same counts (none have been added, but may have changed)
                        //these are the ones that are the same.
                        foreach (SiteHousingModel sHM in editedSite.SiteHousings)
                        {
                            if (sHM.AMOUNT == null || sHM.AMOUNT == 0)
                            {
                                //delete it
                                request = new RestRequest(Method.POST);
                                request.Resource = "/SiteHousings/{entityId}";
                                request.AddParameter("entityId", sHM.SITE_HOUSING_ID, ParameterType.UrlSegment);
                                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                                request.AddHeader("Content-Type", "application/xml");
                                serviceCaller.Execute<SITE_HOUSING>(request);
                            }
                            else
                            {
                                //put the rest
                                SITE_HOUSING editOne = new SITE_HOUSING();
                                editOne.SITE_HOUSING_ID = sHM.SITE_HOUSING_ID;
                                editOne.HOUSING_TYPE_ID = sHM.HOUSING_TYPE_ID;
                                editOne.SITE_ID = sHM.SITE_ID;
                                editOne.LENGTH = sHM.LENGTH;
                                editOne.MATERIAL = sHM.MATERIAL;
                                editOne.NOTES = sHM.NOTES;
                                editOne.AMOUNT = sHM.AMOUNT;

                                request = new RestRequest(Method.POST);
                                request.Resource = "/SiteHousings/{entityId}";
                                request.RequestFormat = DataFormat.Xml;
                                request.AddParameter("entityId", sHM.SITE_HOUSING_ID, ParameterType.UrlSegment);
                                request.AddHeader("X-HTTP-Method-Override", "PUT");
                                request.AddHeader("Content-Type", "application/xml");
                                request.AddParameter("application/xml", request.XmlSerializer.Serialize(editOne), ParameterType.RequestBody);
                                serviceCaller.Execute<SITE_HOUSING>(request);
                            }
                        }
                    }
                }//end else of if (editedSite.SiteHousings.Count >= 1)

                //get original Site Network Types
                request = new RestRequest();
                request.Resource = "/sites/{siteId}/networkTypes";
                request.RootElement = "ArrayOfNETWORK_TYPE";
                request.AddParameter("siteId", updatedSite.SITE_ID, ParameterType.UrlSegment);
                List<NETWORK_TYPE> oldSiteNetworks = serviceCaller.Execute<List<NETWORK_TYPE>>(request);

                if (oldSiteNetworks != null)
                {
                    if (oldSiteNetworks.Count >= 1)
                    {
                        //remove all the old ones
                        foreach (NETWORK_TYPE oldSN in oldSiteNetworks)
                        {
                            request = new RestRequest(Method.POST);
                            request.Resource = "/sites/{siteId}/removeNetworkType";
                            request.AddParameter("siteId", editedSite.aSite.SITE_ID, ParameterType.UrlSegment);
                            request.AddHeader("X-HTTP-Method-Override", "DELETE");
                            request.AddHeader("Content-Type", "application/xml");
                            request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                            request.AddBody(oldSN);
                            serviceCaller.Execute<NETWORK_TYPE>(request);
                        }
                    }
                }
                //make new network types for the updated site
                if (editedSite.SiteNetworkTypes != null)
                {
                    List<NETWORK_TYPE> updatedSiteNets = new List<NETWORK_TYPE>();
                    foreach (string x in editedSite.SiteNetworkTypes)
                    {
                        NETWORK_TYPE editedSiteNet = getNetworkType(Convert.ToDecimal(x));
                        //now post
                        request = new RestRequest(Method.POST);
                        request.Resource = "/sites/{siteId}/AddNetworkType";
                        request.AddParameter("siteId", updatedSite.SITE_ID, ParameterType.UrlSegment);
                        request.RequestFormat = DataFormat.Xml;
                        request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                        request.AddBody(editedSiteNet);
                        updatedSiteNets = serviceCaller.Execute<List<NETWORK_TYPE>>(request);
                    }
                }

                //get original Site Network Names
                request = new RestRequest();
                request.Resource = "/sites/{siteId}/networkNames";
                request.RootElement = "ArrayOfNETWORK_NAME";
                request.AddParameter("siteId", updatedSite.SITE_ID, ParameterType.UrlSegment);
                List<NETWORK_NAME> oldSiteNetworkNames = serviceCaller.Execute<List<NETWORK_NAME>>(request);

                if (oldSiteNetworkNames != null)
                {
                    if (oldSiteNetworkNames.Count >= 1)
                    {
                        //remove all the old ones
                        foreach (NETWORK_NAME oldSNN in oldSiteNetworkNames)
                        {
                            request = new RestRequest(Method.POST);
                            request.Resource = "/sites/{siteId}/removeNetworkName";
                            request.AddParameter("siteId", editedSite.aSite.SITE_ID, ParameterType.UrlSegment);
                            request.AddHeader("X-HTTP-Method-Override", "DELETE");
                            request.AddHeader("Content-Type", "application/xml");
                            request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                            request.AddBody(oldSNN);
                            serviceCaller.Execute<NETWORK_NAME>(request);
                        }
                    }
                }
                //make new network names for the updated site
                if (editedSite.SiteNetworkNames != null)
                {
                    List<NETWORK_NAME> updatedSiteNetNames = new List<NETWORK_NAME>();
                    foreach (string x in editedSite.SiteNetworkNames)
                    {
                        NETWORK_NAME editedSiteNetName = getNetworkName(Convert.ToDecimal(x));
                        //now post
                        request = new RestRequest(Method.POST);
                        request.Resource = "/sites/{siteId}/AddNetworkName";
                        request.AddParameter("siteId", updatedSite.SITE_ID, ParameterType.UrlSegment);
                        request.RequestFormat = DataFormat.Xml;
                        request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                        request.AddBody(editedSiteNetName);
                        updatedSiteNetNames = serviceCaller.Execute<List<NETWORK_NAME>>(request);
                    }
                }
                return RedirectToAction("SiteDetailsPV", new { id = updatedSite.SITE_ID });
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }
        
        //
        // GET: /Sites/Delete/5
        [Authorize]
        public ActionResult Delete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "Sites/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<SITE>(request);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        #region Popup
        //
        // GET: /PopupSitesDetailsPV/

        public PartialViewResult PopupSitesDetailsPV(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                #region Site Details Tab
                //SiteDetails tab
                SITE aSite = GetSite(id);
                string returned = GetDatum(aSite.HDATUM_ID);
                ViewData["aHorizontalDatum"] = returned != "null" ? returned : "not provided";

                if (aSite.PRIORITY_ID != null && aSite.PRIORITY_ID > 0)
                {
                    request.Resource = "/Sites/{siteId}/DeploymentPriorities";
                    request.RootElement = "DEPLOYMENT_PRIORITY";
                    request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
                    ViewData["aPriority"] = serviceCaller.Execute<DEPLOYMENT_PRIORITY>(request).PRIORITY_NAME;
                }

                #endregion Site Details tab

                #region HWMs tab
                //HWMs Tab
                request = new RestRequest();
                request.Resource = "Sites/{siteId}/HWMs";
                request.RootElement = "ArrayOfHWM";
                request.AddParameter("siteId", id, ParameterType.UrlSegment);
                List<HWM> SiteHWMs = serviceCaller.Execute<List<HWM>>(request);
                if (SiteHWMs != null)
                {
                    if (SiteHWMs.Count >= 1)
                        ViewData["SiteHWMs"] = SiteHWMs;
                }
                #endregion HWMs tab

                #region Instruments tab
                //Instrument Tab
                List<InstrSensorType> siteInstList = new List<InstrSensorType>();
                request = new RestRequest();
                request.Resource = "Sites/{siteId}/Instruments";
                request.RootElement = "ArrayOfInstruments";
                request.AddParameter("siteId", id, ParameterType.UrlSegment);
                List<INSTRUMENT> siteInstrumentList = serviceCaller.Execute<List<INSTRUMENT>>(request);
                if (siteInstrumentList != null)
                {
                    if (siteInstrumentList.Count >= 1)
                    {
                        foreach (INSTRUMENT sI in siteInstrumentList)
                        {
                            InstrSensorType thisSiteIn = new InstrSensorType();
                            thisSiteIn.InstrID = sI.INSTRUMENT_ID.ToString();
                            thisSiteIn.SerialNumber = sI.SERIAL_NUMBER;
                            request = new RestRequest();
                            request.Resource = "SensorTypes/{entityId}";
                            request.RootElement = "SENSOR_TYPE";
                            request.AddParameter("entityId", sI.SENSOR_TYPE_ID, ParameterType.UrlSegment);
                            thisSiteIn.SensorType = serviceCaller.Execute<SENSOR_TYPE>(request).SENSOR;
                            siteInstList.Add(thisSiteIn);
                        }


                        ViewData["instrumentList"] = siteInstList;
                    }
                }
                #endregion Instruments tab

                #region Files Tab
                ////Site Files////
                request = new RestRequest();
                request.Resource = "Sites/{siteId}/Files";
                request.RootElement = "ArrayOfFile";
                request.AddParameter("siteId", id, ParameterType.UrlSegment);
                List<FILE> SfileList = serviceCaller.Execute<List<FILE>>(request);

                ViewData["photoSFList"] = SfileList.Where(sf => sf.FILETYPE_ID == 1).ToList();
                ViewData["dataSFList"] = SfileList.Where(df => df.FILETYPE_ID == 2).ToList();
                ViewData["otherSFList"] = SfileList.Where(df => df.FILETYPE_ID >= 3).ToList();

                #endregion Files Tab


                #region Peak Summary Tab
                GetPeakSummaries(id);
                #endregion Peak Summary Tab

                return PartialView("Popups/PopupSitesDetailsPV", aSite);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }
        
        public PartialViewResult CheckSitesBuffer(string latitude, string longitude, decimal buffer)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "Sites?Latitude={latitude}&Longitude={longitude}&Buffer={buffer}";
                request.RootElement = "ArrayOfSITE";
                request.AddParameter("latitude", latitude, ParameterType.UrlSegment);
                request.AddParameter("longitude", longitude, ParameterType.UrlSegment);
                request.AddParameter("buffer", buffer, ParameterType.UrlSegment);
                SiteList SitesList = serviceCaller.Execute<SiteList>(request);
                if (SitesList != null)
                {
                    ViewData["SiteList"] = SitesList.Sites;
                    ViewData["NumSites"] = SitesList.Sites.Count;
                }
                ViewData["lat"] = latitude;
                ViewData["long"] = longitude;


                return PartialView("Popups/PopupSiteBufferPV", SitesList);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        #endregion Popup

        //Method called from PopupSiteDetailsPV and SiteDetailsPV to create Peak table
       
        public void GetPeakSummaries(int id)
        {
            try
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
                if (SiteSummaries.Count >= 1 && SiteSummaries != null)
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
                        if (HWMSummaries != null)
                        {
                            foreach (HWM h in HWMSummaries)
                            {
                                PeakSum.PeakHWMDesc = h.HWM_LOCATIONDESCRIPTION;
                                PeakSum.eventId = h.EVENT_ID;
                                //get event 
                                if (h.EVENT_ID != null)
                                {
                                    request = new RestRequest();
                                    request.Resource = "/Events/{entityId}";
                                    request.RootElement = "EVENT";
                                    request.AddParameter("entityId", h.EVENT_ID, ParameterType.UrlSegment);
                                    EVENT HWMEvent = serviceCaller.Execute<EVENT>(request);
                                    if (HWMEvent != null)
                                    {
                                        PeakSum.EventName = HWMEvent.EVENT_NAME;
                                    }
                                }
                            }
                        }
                        // get datafile peaks
                        request = new RestRequest();
                        request.Resource = "/PeakSummaries/{peakSummaryId}/DataFiles";
                        request.RootElement = "ArrayOfDATA_FILE";
                        request.AddParameter("peakSummaryId", p.PEAK_SUMMARY_ID, ParameterType.UrlSegment);
                        DataFileSums = serviceCaller.Execute<List<DATA_FILE>>(request);

                        //if it is a datafile peak, get instrument for Description and event name
                        if (DataFileSums != null)
                        {
                            foreach (DATA_FILE df in DataFileSums)
                            {
                                //get event name and description via instrument
                                request = new RestRequest();
                                request.Resource = "Instruments/{entityId}";
                                request.RootElement = "INSTRUMENT";
                                request.AddParameter("entityId", df.INSTRUMENT_ID, ParameterType.UrlSegment);
                                INSTRUMENT thisInstr = serviceCaller.Execute<INSTRUMENT>(request);

                                PeakSum.PeakDataFileDesc = thisInstr.LOCATION_DESCRIPTION;
                                PeakSum.eventId = thisInstr.EVENT_ID;
                                request = new RestRequest();
                                request.Resource = "/Events/{entityId}";
                                request.RootElement = "EVENT";
                                request.AddParameter("entityId", thisInstr.EVENT_ID, ParameterType.UrlSegment);
                                EVENT DFEvent = serviceCaller.Execute<EVENT>(request);
                                if (DFEvent != null)
                                {
                                    PeakSum.EventName = DFEvent.EVENT_NAME;
                                }
                            }
                        }
                        allPeakSums.Add(PeakSum);
                    }
                }
                ViewData["Peaks"] = allPeakSums;
            }
            catch (Exception e)
            {
                e.ToString();
            }

        }

        //method called several places to request/receive aSite
        [Authorize]
        public SITE GetSite(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "Sites/{entityId}";
                request.RootElement = "SITE";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                SITE aSite = serviceCaller.Execute<SITE>(request);
                return aSite;
            }
            catch
            {
                SITE emptySite = null;
                return emptySite;
            }
        }

        //method called several places to request/receive HorizontalDatum.Datum_abbrev
        [Authorize]
        public string GetDatum(decimal id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/HorizontalDatums/{entityId}";
                request.RootElement = "HORIZONTAL_DATUMS";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                HORIZONTAL_DATUMS aDatum = serviceCaller.Execute<HORIZONTAL_DATUMS>(request);
                if (aDatum != null)
                {
                    return aDatum.DATUM_NAME;
                }
                else
                {
                    return "null";
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private string GetHousingTypeName(decimal? p)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/HousingTypes/{entityId}";
                request.RootElement = "HOUSING_TYPE";
                request.AddParameter("entityId", p, ParameterType.UrlSegment);
                HOUSING_TYPE htName = serviceCaller.Execute<HOUSING_TYPE>(request);
                if (htName != null)
                {
                    return htName.TYPE_NAME;
                }
                else
                {
                    return "Not Provided";
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private NETWORK_TYPE getNetworkType(decimal id)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/NetworkTypes/{entityId}";
            request.RootElement = "NETWORK_TYPE";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            NETWORK_TYPE thisNetworkType = serviceCaller.Execute<NETWORK_TYPE>(request);
            return thisNetworkType;
        }

        private NETWORK_NAME getNetworkName(decimal id)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/NetworkNames/{entityId}";
            request.RootElement = "NETWORK_NAME";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            NETWORK_NAME thisNetworkName = serviceCaller.Execute<NETWORK_NAME>(request);
            return thisNetworkName;
        }
        
        //site create page, get states to populate dropdown
        private List<STATES> GetStates()
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/States";
            request.RootElement = "STATES";
            List<STATES> AllStates = serviceCaller.Execute<List<STATES>>(request);
            return AllStates;
        }

        public List<COUNTIES> GetCounties()
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Counties";
            request.RootElement = "ArrayOfCOUNTIES";
            List<COUNTIES> Counties = serviceCaller.Execute<List<COUNTIES>>(request);
            return Counties;
        }

        private string convertState(string name)
        {
            string result = string.Empty;

            switch (name)
            {
                case "Alabama":
                    result = "AL";
                    break;
                case "Alaska":
                    result = "AK";
                    break;
                case "American Samoa":
                    result = "AS";
                    break;
                case "Arizona":
                    result = "AZ";
                    break;
                case "Arkansas":
                    result = "AR";
                    break;
                case "California":
                    result = "CA";
                    break;
                case "Colorado":
                    result = "CO";
                    break;
                case "Connecticut":
                    result = "CT";
                    break;
                case "Delaware":
                    result = "DE";
                    break;
                case "District of Columbia":
                    result = "DC";
                    break;
                case "Federated States of Micronesia":
                    result = "FM";
                    break;
                case "Florida":
                    result = "FL";
                    break;
                case "Georgia":
                    result = "GA";
                    break;
                case "Guam":
                    result = "GU";
                    break;
                case "Hawaii":
                    result = "HI";
                    break;
                case "Idaho":
                    result = "ID";
                    break;
                case "Illinois":
                    result = "IL";
                    break;
                case "Indiana":
                    result = "IN";
                    break;
                case "Iowa":
                    result = "IA";
                    break;
                case "Kansas":
                    result = "KS";
                    break;
                case "Kentucky":
                    result = "KY";
                    break;
                case "Louisiana":
                    result = "LA";
                    break;
                case "Maine":
                    result = "ME";
                    break;
                case "Marshall Islands":
                    result = "MH";
                    break;
                case "Maryland":
                    result = "MD";
                    break;
                case "Massachusetts":
                    result = "MA";
                    break;
                case "Michigan":
                    result = "MI";
                    break;
                case "Minnesota":
                    result = "MN";
                    break;
                case "Mississippi":
                    result = "MS";
                    break;
                case "Missouri":
                    result = "MO";
                    break;
                case "Montana":
                    result = "MT";
                    break;
                case "Nebraska":
                    result = "NE";
                    break;
                case "Nevada":
                    result = "NV";
                    break;
                case "New Hampshire":
                    result = "NH";
                    break;
                case "New Jersey":
                    result = "NJ";
                    break;
                case "New Mexico":
                    result = "NM";
                    break;
                case "New York":
                    result = "NY";
                    break;
                case "North Carolina":
                    result = "NC";
                    break;
                case "North Dakota":
                    result = "ND";
                    break;
                case "Northern Mariana Islands":
                    result = "MP";
                    break;
                case "Ohio":
                    result = "OH";
                    break;
                case "Oklahoma":
                    result = "OK";
                    break;
                case "Oregon":
                    result = "OR";
                    break;
                case "Pennsylvania":
                    result = "PA";
                    break;
                case "Puerto Rico":
                    result = "PR";
                    break;
                case "Rhode Island":
                    result = "RI";
                    break;
                case "South Carolina":
                    result = "SC";
                    break;
                case "South Dakota":
                    result = "SD";
                    break;
                case "Tennessee":
                    result = "TN";
                    break;
                case "Texas":
                    result = "TX";
                    break;
                case "Utah":
                    result = "UT";
                    break;
                case "Vermont":
                    result = "VT";
                    break;
                case "Virgin Islands":
                    result = "VI";
                    break;
                case "Virginia":
                    result = "VA";
                    break;
                case "Washington":
                    result = "WA";
                    break;
                case "West Virginia":
                    result = "WV";
                    break;
                case "Wisconsin":
                    result = "WI";
                    break;
                case "Wyoming":
                    result = "WY";
                    break;
                default:
                    result = string.Empty;
                    break;
            }
            return result;
        }
        
    }   
}
