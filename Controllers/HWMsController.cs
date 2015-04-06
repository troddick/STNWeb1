//------------------------------------------------------------------------------
//----- HWMsController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Populates HWM resource for the view
//
//discussion:   
//
//     

#region Comments
// 05.12.14 - TR - Changed GPS TYPE to HORIZONTAL COLLECT METHOD
// 11.11.13 - TR - Add in previous url grab to pass on in case the didn't choose collection team/event before creating hwm
// 05.16.13 - TR - Added "submit and add peak summary button, removed peak summary from form
// 05.13.13 - TR - Added new fields for HWM ENvironment and Flagged date
// 03.13.13 - TR - Added BANK radio buttons to create page
// 01.28.13 - TR - LandownerContact removed from HWM (now in Site)
// 01.26.13 - TR - Member signed in's collection team
// 11.30.12 - TR - Added partial views for details and edit, and approval.
// 11.06.12 - TR - Fixed some small things within edit and details so that all fields show. (Edit is not updating Marker and GPS Type)
// 10.29.12 - TR - Added Create HWM page
// 10.29.12 - TR - Details: Approval shows member Username (was: 'STNServices.MEMBER') & instrument request passes aHWM.SiteId as urlSegment (was: id) 
// 10.23.12 - JB - Fixed DateTime Serialization issue
// 10.10.12 - JB - Created from old web app
#endregion

using System;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using RestSharp;
using STNServices;
using STNServices.Authentication;
using STNServices.Resources;
using STNWeb.Utilities;
using STNWeb.Helpers;

namespace STNWeb.Controllers
{
    [RequireSSL]
    public class HWMsController : Controller
    {
        //
        // GET: /HWMs/
        [Authorize]
        public ActionResult ViewAll()
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "HWMs/";
                request.RootElement = "ArrayOfHWM";

                HWMList hwmList = serviceCaller.Execute<HWMList>(request);
                
                return View(hwmList);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }         
        }

        //
        // GET: /HWMs/HWMInfoBox/
        public PartialViewResult HWMInfoBox(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "Sites/{entityId}";
                request.RootElement = "SITE";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                ViewData["Site"] = serviceCaller.Execute<SITE>(request);

                request = new RestRequest();
                request.Resource = "Sites/{siteId}/HWMs";
                request.RootElement = "ArrayOfHWM";
                request.AddParameter("siteId", id, ParameterType.UrlSegment);
                ViewData["hwmList"] = serviceCaller.Execute<List<HWM>>(request);

                return PartialView();
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        public PartialViewResult HWMInfoBox4Ev(int evId, int siteId)
        {
            try
            {
                //returns the HWMs for this site and this event
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/Sites/{siteId}/EventHWMs?Event={eventId}";
                request.RootElement = "ArrayOfHWM";
                request.AddParameter("siteId", siteId, ParameterType.UrlSegment);
                request.AddParameter("eventId", evId, ParameterType.UrlSegment);
                List<HWM> SiteEvHWMs = serviceCaller.Execute<List<HWM>>(request);
                if (SiteEvHWMs != null && SiteEvHWMs.Count >= 1)
                { ViewData["SiteEvHWMs"] = SiteEvHWMs; }

                return PartialView();
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //
        // GET: /HWMs/Details/5
        [Authorize]
        public ActionResult Details(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "HWMs/{entityId}";
                request.RootElement = "HWM";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                HWM aHWM = serviceCaller.Execute<HWM>(request);

                //see if this hwm has a peak summary or approval (field can't delete then)
                if (aHWM.PEAK_SUMMARY_ID.HasValue || aHWM.APPROVAL_ID.HasValue)
                    ViewData["ManagerOnly"] = "true";

                //get Site number
                request = new RestRequest();
                request.Resource = "Sites/{entityId}";
                request.RootElement = "SITE";
                request.AddParameter("entityId", aHWM.SITE_ID, ParameterType.UrlSegment);
                ViewData["SiteNo"] = serviceCaller.Execute<SITE>(request).SITE_NO;

                //get member logged in's role
                request = new RestRequest();
                request.Resource = "/Members?username={userName}";
                request.RootElement = "MEMBER";
                request.AddParameter("userName", User.Identity.Name, ParameterType.UrlSegment);
                MEMBER thisMember = serviceCaller.Execute<MEMBER>(request);
                if (thisMember.ROLE_ID == 1) { ViewData["Role"] = "Admin"; }
                if (thisMember.ROLE_ID == 2) { ViewData["Role"] = "Manager"; }
                if (thisMember.ROLE_ID == 3) { ViewData["Role"] = "Field"; }

                //Get peak summary info
                request = new RestRequest();
                request.Resource = "/PeakSummaries/{entityId}";
                request.RootElement = "ArrayOfPEAK_SUMMARY";
                request.AddParameter("entityId", aHWM.PEAK_SUMMARY_ID, ParameterType.UrlSegment);
                ViewData["PeakSummary"] = serviceCaller.Execute<PEAK_SUMMARY>(request);

                return View(aHWM);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        // GET: /HWMs/HWMDetailsPV/5
        [Authorize]
        public PartialViewResult HWMDetailsPV(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "HWMs/{entityId}";
                request.RootElement = "HWM";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                HWM aHWM = serviceCaller.Execute<HWM>(request);

                //make sure latitude is only 6 dec places
                aHWM.LATITUDE_DD = Math.Round(aHWM.LATITUDE_DD.Value, 6);

                //get Site name and number if fields have values
                request = new RestRequest();
                request.Resource = "Sites/{entityId}";
                request.RootElement = "SITE";
                request.AddParameter("entityId", aHWM.SITE_ID, ParameterType.UrlSegment);
                ViewData["aSite"] = serviceCaller.Execute<SITE>(request);

                //get event
                request = new RestRequest();
                request.Resource = "/Events/{eventId}";
                request.RootElement = "EVENT";
                request.AddParameter("eventId", aHWM.EVENT_ID, ParameterType.UrlSegment);
                EVENT thisEvent = serviceCaller.Execute<EVENT>(request);
                if (thisEvent != null)
                { ViewData["Event"] = thisEvent.EVENT_NAME; }

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
                { ViewData["HWMQual"] = thisHWMQual.HWM_QUALITY; }

                //build hwm quality field based on Environment and quality
                if (aHWM.HWM_ENVIRONMENT == "Coastal")
                {
                    switch (Convert.ToInt32(aHWM.HWM_QUALITY_ID))
                    {
                        case 1:
                            ViewData["QualValue"] = "(+/- 0.05)"; //Excellent
                            break;
                        case 2:
                            ViewData["QualValue"] = "(+/- 0.1)"; //Good
                            break;
                        case 3:
                            ViewData["QualValue"] = "(+/- 0.2)"; //Fair
                            break;
                        case 4:
                            ViewData["QualValue"] = "(+/- 0.4)"; //Poor
                            break;
                        default:
                            ViewData["QualValue"] = "(>0.40)"; //VP
                            break;
                    }
                }
                else // "riverine"
                {
                    switch (Convert.ToInt32(aHWM.HWM_QUALITY_ID))
                    {
                        case 1:
                            ViewData["QualValue"] = "(+/- 0.02)"; //Excellent
                            break;
                        case 2:
                            ViewData["QualValue"] = "(+/- 0.05)"; //Good
                            break;
                        case 3:
                            ViewData["QualValue"] = "(+/- 0.1)"; //Fair
                            break;
                        case 4:
                            ViewData["QualValue"] = "(+/- 0.2)"; //Poor
                            break;
                        default:
                            ViewData["QualValue"] = "(>0.20)"; //VP
                            break;
                    }
                }

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
                if (aHWM.VCOLLECT_METHOD_ID != null && aHWM.VCOLLECT_METHOD_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/VerticalMethods/{entityId}";
                    request.RootElement = "VERTICAL_COLLECT_METHODS";
                    request.AddParameter("entityId", aHWM.VCOLLECT_METHOD_ID, ParameterType.UrlSegment);
                    VERTICAL_COLLECT_METHODS thisVCollMtd = serviceCaller.Execute<VERTICAL_COLLECT_METHODS>(request);
                    if (thisVCollMtd != null)
                    { ViewData["VCollectMethod"] = thisVCollMtd.VCOLLECT_METHOD; }
                }

                //Get Vertical Datum
                if (aHWM.VDATUM_ID != null && aHWM.VDATUM_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/VerticalDatums/{entityId}";
                    request.RootElement = "VERTICAL_DATUMS";
                    request.AddParameter("entityId", aHWM.VDATUM_ID, ParameterType.UrlSegment);
                    VERTICAL_DATUMS thisVD = serviceCaller.Execute<VERTICAL_DATUMS>(request);
                    if (thisVD != null)
                    { ViewData["VerticalDatum"] = thisVD.DATUM_NAME; }
                }

                //get horizontal datum
                if (aHWM.HDATUM_ID != null && aHWM.HDATUM_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/HorizontalDatums/{entityId}";
                    request.RootElement = "HORIZONTAL_DATUMS";
                    request.AddParameter("entityId", aHWM.HDATUM_ID, ParameterType.UrlSegment);
                    HORIZONTAL_DATUMS thisHD = serviceCaller.Execute<HORIZONTAL_DATUMS>(request);
                    if (thisHD != null)
                        ViewData["aHorizontalDatum"] = thisHD.DATUM_NAME;
                }

                //Get marker
                if (aHWM.MARKER_ID != null && aHWM.MARKER_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/Markers/{entityId}";
                    request.RootElement = "MARKER";
                    request.AddParameter("entityId", aHWM.MARKER_ID, ParameterType.UrlSegment);
                    MARKER thisMarker = serviceCaller.Execute<MARKER>(request);
                    if (thisMarker != null)
                    { ViewData["MarkerID"] = thisMarker.MARKER1; }
                }
                //get horizontal collection Method
                if (aHWM.HCOLLECT_METHOD_ID != null && aHWM.HCOLLECT_METHOD_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/HorizontalMethods/{entityId}";
                    request.RootElement = "HORIZONTAL_COLLECT_METHODS";
                    request.AddParameter("entityId", aHWM.HCOLLECT_METHOD_ID, ParameterType.UrlSegment);
                    HORIZONTAL_COLLECT_METHODS thisHCollectMethod = serviceCaller.Execute<HORIZONTAL_COLLECT_METHODS>(request);
                    if (thisHCollectMethod != null)
                    { ViewData["HCollectMethod"] = thisHCollectMethod.HCOLLECT_METHOD; }
                }
                return PartialView("HWMDetailsPV", aHWM);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //
        // GET: /HWMs/Create
        [Authorize]
        public ActionResult Create(int siteID)
        {
            //need to choose a collection team first
            if (Session["TeamId"] == null)
            {
                //grab the url to bring back there
                string returnUrl = Request.UrlReferrer.ToString();
                if (returnUrl.Contains("Sites/Create"))
                {
                    //then this is the site create page trying to create a hwm, change to hwm/create
                    //just replace Sites/Create with HWMs/Create
                    returnUrl = returnUrl.Replace("Sites/Create", string.Format("HWMs/Create?siteId={0}", siteID));
                }

                return RedirectToAction("Index", "Home", new { returnUrl = returnUrl });
            }
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //store siteID and waterbody
                request = new RestRequest();
                request.Resource = "Sites/{entityId}";
                request.RootElement = "SITE";
                request.AddParameter("entityId", siteID, ParameterType.UrlSegment);
                SITE aSite = serviceCaller.Execute<SITE>(request);

                //ensure latitude is only 6 dec places
                aSite.LATITUDE_DD = Math.Round(aSite.LATITUDE_DD, 6);

                ViewData["theSite"] = aSite;
            
                //Get event choices
                request = new RestRequest();
                request.Resource = "/Events";
                request.RootElement = "ArrayOfEvents";
                ViewData["EventList"] = serviceCaller.Execute<List<EVENT>>(request);

                //Get HWM type choices
                request = new RestRequest();
                request.Resource = "/HWMTypes";
                request.RootElement = "ArrayOfHWM_TYPES";
                ViewData["HWMTypesList"] = serviceCaller.Execute<List<HWM_TYPES>>(request);

                //Get HWM quality choices
                request = new RestRequest();
                request.Resource = "/HWMQualities";
                request.RootElement = "ArrayOfHWM_QUALITY";
                ViewData["HWMQualitiesList"] = serviceCaller.Execute<List<HWM_QUALITIES>>(request);

                //Get vertical datum choices
                request = new RestRequest();
                request.Resource = "/VerticalDatums";
                request.RootElement = "ArrayOfVERTICAL_DATUMS";
                ViewData["vDatumList"] = serviceCaller.Execute<List<VERTICAL_DATUMS>>(request);

                //get HDatum for dropdown
                request = new RestRequest();
                request.Resource = "/HorizontalDatums";
                request.RootElement = "ArrayOfHORIZONTAL_DATUMS";
                ViewData["HDatumList"] = serviceCaller.Execute<List<HORIZONTAL_DATUMS>>(request);
            
                //Get collection method choices
                request = new RestRequest();
                request.Resource = "/VerticalMethods";
                request.RootElement = "ArrayOfVERTICAL_COLLECT_METHODS";
                ViewData["VcollectMethodList"] = serviceCaller.Execute<List<VERTICAL_COLLECT_METHODS>>(request);

                //Get marker choices
                request = new RestRequest();
                request.Resource = "/Markers";
                request.RootElement = "ArrayOfMARKER";
                ViewData["markerList"] = serviceCaller.Execute<List<MARKER>>(request);

                //Get Horizontal Collection Methods
                request = new RestRequest();
                request.Resource = "/HorizontalMethods";
                request.RootElement = "ArrayOfHORIZONTAL_COLLECT_METHODS";
                ViewData["HCollectList"] = serviceCaller.Execute<List<HORIZONTAL_COLLECT_METHODS>>(request);
            
                return View();
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        // POST: /HWMs/Create
        [Authorize]
        [HttpPost]
        public ActionResult Create(HWM newHWM, string Create)
        {
            try
            {
                //see if they entered a survey date, if so, apply the survey_team_id
                if (newHWM.SURVEY_DATE != null)
                {
                    newHWM.SURVEY_TEAM_ID = newHWM.FLAG_TEAM_ID;
                }

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "HWMs/";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<HWM>(newHWM), ParameterType.RequestBody);

                string siteID = newHWM.SITE_ID.ToString();
                HWM createdHWM = serviceCaller.Execute<HWM>(request);

                //determine which button was clicked
                if (Create == "Submit & Add\r\nPeak Summary") //Submit & Add HWM
                {
                    return RedirectToAction("CreatePeakSumForm", "PeakSummary", new { id = createdHWM.HWM_ID, FROM = "HWM" });
                }
                else //Submit
                {
                    return RedirectToAction("Details", new { id = createdHWM.HWM_ID });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }
        
        //
        // GET: /HWMs/HWMEditPV/5
        [Authorize]
        public ActionResult HWMEditPV(int id)
        {
            //need to choose a collection team first
            if (Session["TeamId"] == null)
            {
                //grab the url to bring back there
                string returnUrl = Request.UrlReferrer.ToString();
                return RedirectToAction("Index", "Home", new { returnUrl = returnUrl });
            }
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "HWMs/{entityId}";
                request.RootElement = "HWM";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                HWM aHWM = serviceCaller.Execute<HWM>(request);

                //ensure latitude is 6 dec places long
                aHWM.LATITUDE_DD = Math.Round(aHWM.LATITUDE_DD.Value, 6);

                //get Site name and number
                request = new RestRequest();
                request.Resource = "Sites/{entityId}";
                request.RootElement = "SITE";
                request.AddParameter("entityId", aHWM.SITE_ID, ParameterType.UrlSegment);
                SITE aSite = serviceCaller.Execute<SITE>(request);
                ViewData["SiteNo"] = aSite.SITE_NO;

                //Get vertical datum choices
                request = new RestRequest();
                request.Resource = "/VerticalDatums";
                request.RootElement = "ArrayOfVERTICAL_DATUMS";
                ViewData["vDatumList"] = serviceCaller.Execute<List<VERTICAL_DATUMS>>(request);

                //Get horizontal datum choices
                request = new RestRequest();
                request.Resource = "/HorizontalDatums";
                request.RootElement = "ArrayOfHORIZONTAL_DATUMS";
                ViewData["hDatumList"] = serviceCaller.Execute<List<HORIZONTAL_DATUMS>>(request);

                //Get event choices
                request = new RestRequest();
                request.Resource = "/Events";
                request.RootElement = "ArrayOfEvents";
                ViewData["EventList"] = serviceCaller.Execute<List<EVENT>>(request);

                //Get HWM type choices
                request = new RestRequest();
                request.Resource = "/HWMTypes";
                request.RootElement = "ArrayOfHWM_TYPES";
                ViewData["HWMTypesList"] = serviceCaller.Execute<List<HWM_TYPES>>(request);

                //Get HWM quality choices
                request = new RestRequest();
                request.Resource = "/HWMQualities";
                request.RootElement = "ArrayOfHWM_QUALITIES";
                ViewData["HWMQualitiesList"] = serviceCaller.Execute<List<HWM_QUALITIES>>(request);

                //get FlagTeam
                if (aHWM.FLAG_TEAM_ID != null && aHWM.FLAG_TEAM_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "CollectionTeams/{entityId}";
                    request.RootElement = "COLLECT_TEAM";
                    request.AddParameter("entityId", aHWM.FLAG_TEAM_ID, ParameterType.UrlSegment);
                    ViewData["FlagTeam"] = serviceCaller.Execute<COLLECT_TEAM>(request).DESCRIPTION;
                }

                //get FlagTeam
                if (aHWM.SURVEY_TEAM_ID != null && aHWM.SURVEY_TEAM_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "CollectionTeams/{entityId}";
                    request.RootElement = "COLLECT_TEAM";
                    request.AddParameter("entityId", aHWM.SURVEY_TEAM_ID, ParameterType.UrlSegment);
                    ViewData["SurveyTeam"] = serviceCaller.Execute<COLLECT_TEAM>(request).DESCRIPTION;
                }

                //Get collection method choices
                request = new RestRequest();
                request.Resource = "/VerticalMethods";
                request.RootElement = "ArrayOfVERTICAL_COLLECT_METHODS";
                ViewData["VcollectMethodList"] = serviceCaller.Execute<List<VERTICAL_COLLECT_METHODS>>(request);

                //Get marker choices
                request = new RestRequest();
                request.Resource = "/Markers";
                request.RootElement = "ArrayOfMARKER";
                ViewData["markerList"] = serviceCaller.Execute<List<MARKER>>(request);

                //Get Horizontal Collection Methods
                request = new RestRequest();
                request.Resource = "/HorizontalMethods";
                request.RootElement = "ArrayOfHORIZONTAL_COLLECT_METHODS";
                ViewData["HCollectList"] = serviceCaller.Execute<List<HORIZONTAL_COLLECT_METHODS>>(request);

                //TODO: Get peak summary choices


                return PartialView("HWMEditPV", aHWM);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        // POST: /HWMs/Edit/5
        [Authorize]
        [HttpPost]
        public PartialViewResult Edit(int id, HWM aHWM)
        {
            try
            {
                if (aHWM.SURVEY_DATE != null)
                {
                    //they added a date, or there was one there already
                    if (aHWM.SURVEY_TEAM_ID == null)
                    {
                        //there's no survey team stored, so they are just now surveying it
                        aHWM.SURVEY_TEAM_ID = Convert.ToDecimal(Request.Form["UseThisSurveyID"]);
                    }
                }
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "HWMs/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<HWM>(aHWM), ParameterType.RequestBody);
                serviceCaller.Execute<HWM>(request);

                //**store info in ViewData for return to Partial View**//
                //get site name and number
                request = new RestRequest();
                request.Resource = "Sites/{entityId}";
                request.RootElement = "SITE";
                request.AddParameter("entityId", aHWM.SITE_ID, ParameterType.UrlSegment);
                SITE aSite = serviceCaller.Execute<SITE>(request);
                ViewData["aSite"] = aSite;
                ViewData["SiteNo"] = aSite.SITE_NO;

                //get event
                request = new RestRequest();
                request.Resource = "/Events/{entityId}";
                request.RootElement = "EVENT";
                request.AddParameter("entityId", aHWM.EVENT_ID, ParameterType.UrlSegment);
                EVENT thisEvent = serviceCaller.Execute<EVENT>(request);
                if (thisEvent != null)
                { ViewData["Event"] = thisEvent.EVENT_NAME; }

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
                { ViewData["HWMQual"] = thisHWMQual.HWM_QUALITY; }

                //build hwm quality field based on Environment and quality
                if (aHWM.HWM_ENVIRONMENT == "Coastal")
                {
                    switch (Convert.ToInt32(aHWM.HWM_QUALITY_ID))
                    {
                        case 1: //Excellent
                            ViewData["QualValue"] = "(+/- 0.05)"; break;
                        case 2: //Good
                            ViewData["QualValue"] = "(+/- 0.1)"; break;
                        case 3: //Fair
                            ViewData["QualValue"] = "(+/- 0.2)"; break;
                        case 4: //Poor
                            ViewData["QualValue"] = "(+/- 0.4)"; break;
                        default: //VP
                            ViewData["QualValue"] = "(>0.40)"; break;
                    }
                }
                else // "riverine"
                {
                    switch (Convert.ToInt32(aHWM.HWM_QUALITY_ID))
                    {
                        case 1: //Excellent
                            ViewData["QualValue"] = "(+/- 0.02)"; break;
                        case 2: //Good
                            ViewData["QualValue"] = "(+/- 0.05)"; break;
                        case 3: //Fair
                            ViewData["QualValue"] = "(+/- 0.1)"; break;                            
                        case 4: //Poor
                            ViewData["QualValue"] = "(+/- 0.2)"; break;                            
                        default: //VP
                            ViewData["QualValue"] = "(>0.20)"; break;
                    }
                }
                
                //get FlagTeam
                request = new RestRequest();
                request.Resource = "/CollectionTeams/{entityId}";
                request.RootElement = "COLLECT_TEAM";
                request.AddParameter("entityId", aHWM.FLAG_TEAM_ID, ParameterType.UrlSegment);
                COLLECT_TEAM thisCollTm = serviceCaller.Execute<COLLECT_TEAM>(request);
                if (thisCollTm != null)
                    ViewData["FlagTeam"] = thisCollTm.DESCRIPTION;

                if (aHWM.SURVEY_TEAM_ID != null)
                {
                    //get surveyTeam
                    request = new RestRequest();
                    request.Resource = "/CollectionTeams/{entityId}";
                    request.RootElement = "COLLECT_TEAM";
                    request.AddParameter("entityId", aHWM.SURVEY_TEAM_ID, ParameterType.UrlSegment);
                    COLLECT_TEAM surveyTeam = serviceCaller.Execute<COLLECT_TEAM>(request);
                    if (surveyTeam != null)
                        ViewData["SurveyTeam"] = surveyTeam.DESCRIPTION;

                }
                
                if (thisCollTm != null)
                { ViewData["FlagTeam"] = thisCollTm.DESCRIPTION; }


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
                { ViewData["MarkerID"] = thisMarker.MARKER1; }

                //get Horizontal Collection Methods
                request = new RestRequest();
                request.Resource = "/HorizontalMethods/{entityId}";
                request.RootElement = "HORIZONTAL_COLLECT_METHODS";
                request.AddParameter("entityId", aHWM.HCOLLECT_METHOD_ID, ParameterType.UrlSegment);
                HORIZONTAL_COLLECT_METHODS thisHcoll = serviceCaller.Execute<HORIZONTAL_COLLECT_METHODS>(request);
                if (thisHcoll != null)
                { ViewData["HCollectMethod"] = thisHcoll.HCOLLECT_METHOD; }

                return PartialView("HWMDetailsPV", aHWM);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }
        
        //
        // GET: /HWMs/Delete/5
        [Authorize]
        public ActionResult Delete(int id, int siteID)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                
                //delete of associated stuff handled in handler

                var request = new RestRequest(Method.POST);
                request.Resource = "HWMs/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<HWM>(request);
                return RedirectToAction("Details", "Sites", new { id = siteID });
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }
        

    }
}
