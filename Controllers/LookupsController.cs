//------------------------------------------------------------------------------
//----- LookupController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Display a master Lookups page and link to individual lookup pages 
//
//discussion:   
//
//     

#region Comments
// 04.28.14 - TR - add housing type
// 04.29.13 - TR - only admin can add or delete, admin and field can edit 
// 04.26.13 - TR - redoing all the lookups to be dropdowns rather than lists, easier edit/delete
// 10.25.12 - TR - Lookup Deletes complete
// 10.24.12 - JB - Added role authorization
// 10.19.12 - TR - Created

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
using System.Text.RegularExpressions;


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
    [Authorize]
    public class LookupsController : Controller
    {
        //get all my lookups to populate their dropdowns
        // GET: /Settings/Lookups/
        public ActionResult Index()
        { 
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //Get lists and add to ViewData
                ViewData["Role"] = GetMemberLoggedIn();        
                
                // get list of AGENCY
                #region AGENCY
                request = new RestRequest();
                request.Resource = "/agencies";
                request.RootElement = "ArrayOfAGENCY";
                List<AGENCY> agencies = serviceCaller.Execute<List<AGENCY>>(request);
                agencies = agencies.OrderBy(x => x.AGENCY_NAME).ToList();
                ViewData["Agencies"] = agencies;
                #endregion AGENCY

                //get list of CONTACT_TYPE
                #region CONTACT_TYPE
                request = new RestRequest();
                request.Resource = "/ContactTypes";
                request.RootElement = "ArrayOfCONTACT_TYPE";
                List<CONTACT_TYPE> contactTypes = serviceCaller.Execute<List<CONTACT_TYPE>>(request);
                contactTypes = contactTypes.OrderBy(x => x.TYPE).ToList();
                ViewData["ContactTypes"] = contactTypes;
                #endregion CONTACT_TYPE

                //get list of DEPLOYMENT_PRIORITY
                #region DEPLOYMENT_PRIORITY
                request = new RestRequest();
                request.Resource = "/DeploymentPriorities";
                request.RootElement = "ArrayOfDEPLOYMENT_PRIORITY";
                List<DEPLOYMENT_PRIORITY> DepPri = serviceCaller.Execute<List<DEPLOYMENT_PRIORITY>>(request);
                ViewData["DeploymentPriority"] = DepPri;
                #endregion DEPLOYMENT_PRIORITY

                //get list of DEPLOYMENT_TYPE
                #region DEPLOYMENT_TYPE
                request = new RestRequest();
                request.Resource = "/DeploymentTypes";
                request.RootElement = "ArrayOfDEPLOYMENT_TYPE";
                List<DEPLOYMENT_TYPE> DeploymentType = serviceCaller.Execute<List<DEPLOYMENT_TYPE>>(request);
                DeploymentType = DeploymentType.OrderBy(x => x.METHOD).ToList();
                ViewData["DeploymentTypes"] = DeploymentType;
                #endregion DEPLOYMENT_TYPE

                //get list of EVENT_STATUS
                #region EVENT_STATUS
                request = new RestRequest();
                request.Resource = "/EventStatus";
                request.RootElement = "ArrayOfEVENT_STATUS";
                List<EVENT_STATUS> EventStatus = serviceCaller.Execute<List<EVENT_STATUS>>(request);
                ViewData["EventStatus"] = EventStatus;
                #endregion EVENT_STATUS

                //get list of EVENT_TYPE
                #region EVENT_TYPE
                request = new RestRequest();
                request.Resource = "/EventTypes";
                request.RootElement = "ArrayOfEVENT_TYPE";
                List<EVENT_TYPE> EventType = serviceCaller.Execute<List<EVENT_TYPE>>(request);
                EventType = EventType.OrderBy(x => x.TYPE).ToList();
                ViewData["EventTypes"] = EventType;
                #endregion EVENT_TYPE

                //get list of FILE_TYPE
                #region FILE_TYPE
                request = new RestRequest();
                request.Resource = "/FileTypes";
                request.RootElement = "ArrayOfFILE_TYPE";
                List<FILE_TYPE> fileTypes = serviceCaller.Execute<List<FILE_TYPE>>(request);
                fileTypes = fileTypes.OrderBy(x => x.FILETYPE).ToList();
                ViewData["FileTypes"] = fileTypes;
                #endregion FILE_TYPES

                //get list of HORIZONTAL_COLLECT_METHOD
                #region HORIZONTAL_COLLECT_METHOD
                request = new RestRequest();
                request.Resource = "/HorizontalMethods";
                request.RootElement = "ArrayOfHORIZONTAL_COLLECT_METHODS";
                List<HORIZONTAL_COLLECT_METHODS> HCOllectMethods = serviceCaller.Execute<List<HORIZONTAL_COLLECT_METHODS>>(request);
                HCOllectMethods = HCOllectMethods.OrderBy(x => x.HCOLLECT_METHOD).ToList();
                ViewData["HCollectMethods"] = HCOllectMethods;
                #endregion HORIZONTAL_COLLECT_METHOD

                //get list of HorizontalDatum
                #region HORIZONTAL_DATUM
                request = new RestRequest();
                request.Resource = "/HorizontalDatums";
                request.RootElement = "ArrayOfHORIZONTAL_DATUMS";
                List<HORIZONTAL_DATUMS> HorDatums = serviceCaller.Execute<List<HORIZONTAL_DATUMS>>(request);
                HorDatums = HorDatums.OrderBy(x => x.DATUM_NAME).ToList();
                ViewData["HorizontalDatums"] = HorDatums;
                #endregion HORIZONTAL_DATUM

                //get list of HOUSING_TYPE
                #region HOUSING_TYPE
                request = new RestRequest();
                request.Resource = "/HousingTypes";
                request.RootElement = "ArrayOfHOUSING_TYPE";
                List<HOUSING_TYPE> HousTy = serviceCaller.Execute<List<HOUSING_TYPE>>(request);
                ViewData["HousingTypes"] = HousTy.OrderBy(x => x.HOUSING_TYPE_ID).ToList();
                #endregion HOUSING_TYPE

                //get list of HWM_QUALITIES
                #region HWM_QUALITIES
                request = new RestRequest();
                request.Resource = "/HWMQualities";
                request.RootElement = "ArrayOfHWM_QUALITIES";
                List<HWM_QUALITIES> HWMqualities = serviceCaller.Execute<List<HWM_QUALITIES>>(request);
                ViewData["HWMqualities"] = HWMqualities;
                #endregion HWM_QUALITIES

                //get list of HWM_TYPES
                #region HWM_TYPES
                request = new RestRequest();
                request.Resource = "/HWMTypes";
                request.RootElement = "ArrayOfHWM_TYPES";
                List<HWM_TYPES> HWMTypes = serviceCaller.Execute<List<HWM_TYPES>>(request);
                HWMTypes = HWMTypes.OrderBy(x => x.HWM_TYPE).ToList();
                ViewData["HWMTypes"] = HWMTypes;
                #endregion HWM_TYPES

                //get list of Instr_COLLECTION_CONDITIONS
                #region Instr_COLLECTION_CONDITIONS
                request = new RestRequest();
                request.Resource = "/InstrCollectConditions";
                request.RootElement = "ArrayOfINSTR_COLLECTION_CONDITIONS";
                List<INSTR_COLLECTION_CONDITIONS> instrColCond = serviceCaller.Execute<List<INSTR_COLLECTION_CONDITIONS>>(request);
                instrColCond = instrColCond.OrderBy(x => x.CONDITION).ToList();
                ViewData["InstrCollCond"] = instrColCond;
                #endregion Instr_COLLECTION_CONDITIONS

                //get list of MARKERS
                #region MARKERS
                request = new RestRequest();
                request.Resource = "/Markers";
                request.RootElement = "ArrayOfMARKER";
                List<MARKER> markers = serviceCaller.Execute<List<MARKER>>(request);
                markers = markers.OrderBy(x => x.MARKER1).ToList();
                ViewData["Markers"] = markers;
                #endregion MARKERS

                //get list of NETWORK_NAME
                #region NETWORK_NAME
                request = new RestRequest();
                request.Resource = "/NetworkNames";
                request.RootElement = "ArrayOfNETWORK_NAME";
                List<NETWORK_NAME> NetworkN = serviceCaller.Execute<List<NETWORK_NAME>>(request);
                ViewData["NetworkNames"] = NetworkN;
                #endregion NETWORK_NAME

                //get list of NETWORK_TYPE
                #region NETWORK_TYPE
                request = new RestRequest();
                request.Resource = "/NetworkTypes";
                request.RootElement = "ArrayOfNETWORK_TYPE";
                List<NETWORK_TYPE> Networks = serviceCaller.Execute<List<NETWORK_TYPE>>(request);
                ViewData["NetworkTypes"] = Networks;
                #endregion NETWORK_TYPE
  
                //get list of OBJECTIVE_POINT_TYPE
                #region OBJECTIVE_POINT_TYPE
                request = new RestRequest();
                request.Resource = "/OPTypes";
                request.RootElement = "ArrayOfOBJECTIVE_POINT_TYPE";
                List<OBJECTIVE_POINT_TYPE> OPTypes = serviceCaller.Execute<List<OBJECTIVE_POINT_TYPE>>(request);
                ViewData["OPTypes"] = OPTypes;
                #endregion OBJECTIVE_POINT_TYPES

                //get list of OP_QUALITY
                #region OP_QUALITY
                request = new RestRequest();
                request.Resource = "/ObjectivePointQualities";
                request.RootElement = "ArrayOfOP_QUALITY";
                List<OP_QUALITY> OPqualities = serviceCaller.Execute<List<OP_QUALITY>>(request);
                ViewData["OPQualities"] = OPqualities;
                #endregion OP_QUALITY

                //get list of SENSOR_BRAND
                #region SENSOR_BRAND
                request = new RestRequest();
                request.Resource = "/SensorBrands";
                request.RootElement = "ArrayOfSENSOR_BRAND";
                List<SENSOR_BRAND> sensBrands = serviceCaller.Execute<List<SENSOR_BRAND>>(request);
                sensBrands = sensBrands.OrderBy(x => x.BRAND_NAME).ToList();
                ViewData["SensorBrands"] = sensBrands;
                #endregion SENSOR_BRAND

                //get list of SENSOR_TYPES
                #region SENSOR_TYPE
                request = new RestRequest();
                request.Resource = "/SensorTypes";
                request.RootElement = "ArrayOfSENSOR_TYPE";
                List<SENSOR_TYPE> SensorType = serviceCaller.Execute<List<SENSOR_TYPE>>(request);
                SensorType = SensorType.OrderBy(x => x.SENSOR).ToList();
                ViewData["SensorTypes"] = SensorType;
                #endregion SENSOR_TYPE

                //get list of STATUS_TYPE
                #region STATUS_TYPE
                request = new RestRequest();
                request.Resource = "/StatusTypes";
                request.RootElement = "ArrayOfSTATUS_TYPE";
                List<STATUS_TYPE> StatusType = serviceCaller.Execute<List<STATUS_TYPE>>(request);
                ViewData["StatusTypes"] = StatusType;
                #endregion SENSOR_TYPE

                //get list of VERTICAL_COLLECT_METHODS
                #region VERTICAL_COLLECT_METHODS
                request = new RestRequest();
                request.Resource = "/VerticalMethods";
                request.RootElement = "ArrayOfVERTICAL_COLLECT_METHODS";
                List<VERTICAL_COLLECT_METHODS> VCollectMethods = serviceCaller.Execute<List<VERTICAL_COLLECT_METHODS>>(request);
                VCollectMethods = VCollectMethods.OrderBy(x => x.VCOLLECT_METHOD).ToList();
                ViewData["VCollectMethods"] = VCollectMethods;
                #endregion VERTICAL_COLLECT_METHODS

                //get list of VerticalDatum
                #region VERTICAL_DATUM
                request = new RestRequest();
                request.Resource = "/VerticalDatums";
                request.RootElement = "ArrayOfVERTICAL_DATUMS";
                List<VERTICAL_DATUMS> VertDatums = serviceCaller.Execute<List<VERTICAL_DATUMS>>(request);
                VertDatums = VertDatums.OrderBy(x => x.DATUM_NAME).ToList();
                ViewData["VerticalDatums"] = VertDatums;
                #endregion VERTICAL_DATUM

                return View("../Settings/Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        #region Agencies
        //need to redirect to get index if error and want to refresh Agency
        public ActionResult Agency()
        {
            return RedirectToAction("Index");
        }
        
        //a edit or add new button was clicked in the Agency infoxbox -- get AgencyEdit page or redirect to AgencyCreate
        //GET: /Settings/Lookups/LookupEdits/AgencyEdit/
        [HttpPost]
        public ActionResult Agency(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("AgencyCreate"); }
                else
                {//edit
                    int agencyId = Convert.ToInt32(fc["AGENCY_ID"]);
                    if (agencyId == 0)
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("AgencyEdit", new { id = agencyId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //agency edit page
        public ActionResult AgencyEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/Agencies/{entityId}";
                    request.RootElement = "AGENCY";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    AGENCY ag = serviceCaller.Execute<AGENCY>(request);

                    ViewData["States"] = GetStatesList();
                    return View("../Settings/Lookups/LookupEdits/AgencyEdit", ag);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to Agency, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/AgencyEdit/
        [HttpPost]
        public ActionResult AgencyEdit(int id, AGENCY anAgency)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/Agencies/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<AGENCY>(anAgency), ParameterType.RequestBody);

                serviceCaller.Execute<AGENCY>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from agency Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/AgencyCreate/
        public ActionResult AgencyCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/Agencies";
                    request.RootElement = "ArrayOfAGENCY";
                    List<AGENCY> Ags = serviceCaller.Execute<List<AGENCY>>(request);
                    Ags = Ags.OrderBy(x => x.AGENCY_NAME).ToList();

                    ViewData["States"] = GetStatesList();
                    return View("../Settings/Lookups/LookupCreates/AgencyCreate", Ags);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        private List<STATES> GetStatesList()
        {
           STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/States";
            request.RootElement = "STATES";
            List<STATES> AllStates = serviceCaller.Execute<List<STATES>>(request);
            return AllStates;
        }

        // new agency was created, post it
        //POST: /Settings/Lookups/LookupCreates/EventTypeCreate
        [HttpPost]
        public ActionResult AgencyCreate(AGENCY newAgency)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/Agencies";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<AGENCY>(newAgency), ParameterType.RequestBody);

                serviceCaller.Execute<AGENCY>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }
       
        //
        //GET: /Settings/Lookups/AgencyDelete/1
        public Boolean AgencyDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/Agencies/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<AGENCY>(request);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        #endregion Agencies

        #region Contact_Type
        //need to redirect to get index if error and want to refresh contactType
        public ActionResult ContactType()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the Contact Type infoxbox -- get ContactTypeEdit page or redirect to ContactTypeCreate
        //GET: /Settings/Lookups/LookupEdits/ContactTypeEdit/
        [HttpPost]
        public ActionResult ContactType(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("ContactTypeCreate"); }
                else
                {//edit
                    int contactTypeId = Convert.ToInt32(fc["CONTACT_TYPE_ID"]);
                    if (contactTypeId == 0) //they didn't choose one before hitting edit
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("ContactTypeEdit", new { id = contactTypeId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //Contact type edit page
        public ActionResult ContactTypeEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/ContactTypes/{entityId}";
                    request.RootElement = "CONTACT_TYPE";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    CONTACT_TYPE coType = serviceCaller.Execute<CONTACT_TYPE>(request);

                    return View("../Settings/Lookups/LookupEdits/ContactTypeEdit", coType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to contact type, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/ContactTypeEdit/
        [HttpPost]
        public ActionResult ContactTypeEdit(int id, CONTACT_TYPE aContactType)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/ContactTypes/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<CONTACT_TYPE>(aContactType), ParameterType.RequestBody);

                serviceCaller.Execute<CONTACT_TYPE>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from ContactType Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/ContactTypeCreate/
        public ActionResult ContactTypeCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/ContactTypes/";
                    request.RootElement = "ArrayOfCONTACT_TYPE";
                    List<CONTACT_TYPE> coType = serviceCaller.Execute<List<CONTACT_TYPE>>(request);
                    return View("../Settings/Lookups/LookupCreates/ContactTypeCreate", coType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new contact type was created, post it
        //POST: /Settings/Lookups/LookupCreates/ContactTypeCreate
        [HttpPost]
        public ActionResult ContactTypeCreate(CONTACT_TYPE newContactType)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/ContactTypes";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<CONTACT_TYPE>(newContactType), ParameterType.RequestBody);

                CONTACT_TYPE newCO = serviceCaller.Execute<CONTACT_TYPE>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/ContactTypeDelete/1
        public Boolean ContactTypeDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/ContactTypes/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<CONTACT_TYPE>(request);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Contact_Type

        #region Deployment_Priority
        //need to redirect to get index if error and want to refresh eventType
        public ActionResult DeploymentPriority()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the Event Type infoxbox -- get EventTypeEdit page or redirect to EventTypeCreate
        //GET: /Settings/Lookups/LookupEdits/DeploymentPriorityEdit/
        [HttpPost]
        public ActionResult DeploymentPriority(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("DeploymentPriorityCreate"); }
                else
                {//edit
                    int deploymentPriorityId = Convert.ToInt32(fc["PRIORITY_ID"]);
                    if (deploymentPriorityId == 0) //they didn't choose one before hitting edit
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("DeploymentPriorityEdit", new { id = deploymentPriorityId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //event type edit page
        public ActionResult DeploymentPriorityEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/DeploymentPriorities/{entityId}";
                    request.RootElement = "DEPLOYMENT_PRIORITY";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    DEPLOYMENT_PRIORITY netType = serviceCaller.Execute<DEPLOYMENT_PRIORITY>(request);

                    return View("../Settings/Lookups/LookupEdits/DeploymentPriorityEdit", netType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to event type, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/DeploymentPriorityEdit/
        [HttpPost]
        public ActionResult DeploymentPriorityEdit(int id, DEPLOYMENT_PRIORITY anDeploymentPriority)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/DeploymentPriorities/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<DEPLOYMENT_PRIORITY>(anDeploymentPriority), ParameterType.RequestBody);

                serviceCaller.Execute<DEPLOYMENT_PRIORITY>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from EventType Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/DeploymentPriorityCreate/
        public ActionResult DeploymentPriorityCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/DeploymentPriorities/";
                    request.RootElement = "ArrayOfDEPLOYMENT_PRIORITY";
                    List<DEPLOYMENT_PRIORITY> netType = serviceCaller.Execute<List<DEPLOYMENT_PRIORITY>>(request);
                    return View("../Settings/Lookups/LookupCreates/DeploymentPriorityCreate", netType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new event type was created, post it
        //POST: /Settings/Lookups/LookupCreates/DeploymentPriorityCreate
        [HttpPost]
        public ActionResult DeploymentPriorityCreate(DEPLOYMENT_PRIORITY deploymentPriorityType)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/DeploymentPriorities";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<DEPLOYMENT_PRIORITY>(deploymentPriorityType), ParameterType.RequestBody);

                DEPLOYMENT_PRIORITY newNet = serviceCaller.Execute<DEPLOYMENT_PRIORITY>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/EventTypeDelete/1
        public Boolean DeploymentPriorityDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/DeploymentPriorities/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<DEPLOYMENT_PRIORITY>(request);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Deployment_Priority

        #region Deployment_Type
        //need to redirect to get index if error and want to refresh DepType
        public ActionResult DepType()
        {
            return RedirectToAction("Index");
        }
        
        //a edit or add new button was clicked in the DepType infoxbox -- get DepTypeEdit page or redirect to DepTypeCreate
        //GET: /Settings/Lookups/LookupEdits/DepTypeEdit/
        [HttpPost]
        public ActionResult DepType(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("DepTypeCreate"); }
                else
                {//edit
                    int DepTypeId = Convert.ToInt32(fc["DEPLOYMENT_TYPE_ID"]);
                    if (DepTypeId == 0)
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with one to edit
                        return RedirectToAction("DepTypeEdit", new { id = DepTypeId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //deptype edit page
        public ActionResult DepTypeEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/DeploymentTypes/{entityId}";
                    request.RootElement = "DEPLOYMENT_TYPE";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    DEPLOYMENT_TYPE DepType = serviceCaller.Execute<DEPLOYMENT_TYPE>(request);
                    return View("../Settings/Lookups/LookupEdits/DepTypeEdit", DepType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to DepType, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/DepTypeEdit/
        [HttpPost]
        public ActionResult DepTypeEdit(int id, DEPLOYMENT_TYPE aDepType)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/DeploymentTypes/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<DEPLOYMENT_TYPE>(aDepType), ParameterType.RequestBody);

                serviceCaller.Execute<DEPLOYMENT_TYPE>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from DepType Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/DepTypeCreate/
        public ActionResult DepTypeCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/DeploymentTypes";
                    request.RootElement = "ArrayOfDEPLOYMENT_TYPE";
                    List<DEPLOYMENT_TYPE> depType = serviceCaller.Execute<List<DEPLOYMENT_TYPE>>(request);
                    return View("../Settings/Lookups/LookupCreates/DepTypeCreate", depType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new DepType was created, post it
        //POST: /Settings/Lookups/LookupCreates/DepTypeCreate
        [HttpPost]
        public ActionResult DepTypeCreate(DEPLOYMENT_TYPE newDepType)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/DeploymentTypes";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<DEPLOYMENT_TYPE>(newDepType), ParameterType.RequestBody);

                serviceCaller.Execute<DEPLOYMENT_TYPE>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/DepTypeDelete/1
        public Boolean DepTypeDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/DeploymentTypes/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<DEPLOYMENT_TYPE>(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Deployment_Type

        #region Event_Status       
        //need to redirect to get index if error and want to refresh EventStatus
        public ActionResult EventStatus()
        {
            return RedirectToAction("Index");
        }
        
        //a edit or add new button was clicked in the EventStatus infoxbox -- get EventStatusEdit page or redirect to EventStatusCreate
        //GET: /Settings/Lookups/LookupEdits/EventStatusEdit/
        [HttpPost]
        public ActionResult EventStatus(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("EventStatusCreate"); }
                else
                {//edit
                    int EventStatId = Convert.ToInt32(fc["EVENT_STATUS_ID"]);
                    if (EventStatId == 0)
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("EventStatusEdit", new { id = EventStatId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //event status edit page
        public ActionResult EventStatusEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/EventStatus/{entityId}";
                    request.RootElement = "EVENT_STATUS";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    EVENT_STATUS EventStat = serviceCaller.Execute<EVENT_STATUS>(request);
                    return View("../Settings/Lookups/LookupEdits/EventStatusEdit", EventStat);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to EventStatus, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/EventStatusEdit/
        [HttpPost]
        public ActionResult EventStatusEdit(int id, EVENT_STATUS anEventStatus)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/EventStatus/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<EVENT_STATUS>(anEventStatus), ParameterType.RequestBody);

                serviceCaller.Execute<EVENT_STATUS>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from EventStatus Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/EventStatusCreate/
        public ActionResult EventStatusCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/EventStatus";
                    request.RootElement = "ArrayOfEVENT_STATUS";
                    List<EVENT_STATUS> eventStat = serviceCaller.Execute<List<EVENT_STATUS>>(request);
                    return View("../Settings/Lookups/LookupCreates/EventStatusCreate", eventStat);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new EventStatus was created, post it
        //POST: /Settings/Lookups/LookupCreates/EventStatusCreate
        [HttpPost]
        public ActionResult EventStatusCreate(EVENT_STATUS newEventStat)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/EventStatus";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<EVENT_STATUS>(newEventStat), ParameterType.RequestBody);

                serviceCaller.Execute<EVENT_STATUS>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/EventStatusDelete/1
        public Boolean EventStatusDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/EventStatus/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<EVENT_STATUS>(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Event_Status

        #region Event_Type
        //need to redirect to get index if error and want to refresh eventType
        public ActionResult EventType()
        {
            return RedirectToAction("Index");
        }
        
        //a edit or add new button was clicked in the Event Type infoxbox -- get EventTypeEdit page or redirect to EventTypeCreate
        //GET: /Settings/Lookups/LookupEdits/EventTypeEdit/
        [HttpPost]
        public ActionResult EventType(FormCollection fc, string Create)
        {
            try
            {                
                if (Create == "Add New")
                { return RedirectToAction("EventTypeCreate"); }
                else
                {//edit
                    int eventTypeId = Convert.ToInt32(fc["EVENT_TYPE_ID"]);
                    if (eventTypeId == 0) //they didn't choose one before hitting edit
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("EventTypeEdit", new { id = eventTypeId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }
        
        //event type edit page
        public ActionResult EventTypeEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/EventTypes/{entityId}";
                    request.RootElement = "EVENT_TYPE";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    EVENT_TYPE evType = serviceCaller.Execute<EVENT_TYPE>(request);

                    return View("../Settings/Lookups/LookupEdits/EventTypeEdit", evType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to event type, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/EventTypeEdit/
        [HttpPost]
        public ActionResult EventTypeEdit(int id, EVENT_TYPE anEventType)
        {
            try 
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/EventTypes/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<EVENT_TYPE>(anEventType), ParameterType.RequestBody);
                       
                serviceCaller.Execute<EVENT_TYPE>(request);
            
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from EventType Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/EventTypeCreate/
        public ActionResult EventTypeCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/EventTypes/";
                    request.RootElement = "ArrayOfEVENT_TYPE";
                    List<EVENT_TYPE> evType = serviceCaller.Execute<List<EVENT_TYPE>>(request);
                    return View("../Settings/Lookups/LookupCreates/EventTypeCreate", evType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new event type was created, post it
        //POST: /Settings/Lookups/LookupCreates/EventTypeCreate
        [HttpPost]
        public ActionResult EventTypeCreate(EVENT_TYPE newEventType)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/EventTypes";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<EVENT_TYPE>(newEventType), ParameterType.RequestBody);

                EVENT_TYPE newEV = serviceCaller.Execute<EVENT_TYPE>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/EventTypeDelete/1
        public Boolean EventTypeDelete(int id)
        {
            try
            {                
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/EventTypes/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<EVENT_TYPE>(request);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Event_Type

        #region File_Type        
        //need to redirect to get index if error and want to refresh FileType
        public ActionResult FileType()
        {
            return RedirectToAction("Index");
        }
        
        //a edit or add new button was clicked in the FileType infoxbox -- get FileTypeEdit page or redirect to FileTypeCreate
        //GET: /Settings/Lookups/LookupEdits/FileTypeEdit/
        [HttpPost]
        public ActionResult FileType(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("FileTypeCreate"); }
                else
                {//edit
                    int FileTypeId = Convert.ToInt32(fc["FILETYPE_ID"]);
                    if (FileTypeId == 0)
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with one to edit
                        return RedirectToAction("FileTypeEdit", new { id = FileTypeId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //filetype edit page
        public ActionResult FileTypeEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/FileTypes/{entityId}";
                    request.RootElement = "FILE_TYPE";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    FILE_TYPE FileType = serviceCaller.Execute<FILE_TYPE>(request);
                    return View("../Settings/Lookups/LookupEdits/FileTypeEdit", FileType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to FileType, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/FileTypeEdit/
        [HttpPost]
        public ActionResult FileTypeEdit(int id, FILE_TYPE aFileType)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/FileTypes/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<FILE_TYPE>(aFileType), ParameterType.RequestBody);

                serviceCaller.Execute<FILE_TYPE>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }

        }

        //redirected here from File type Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/FileTypeCreate/
        public ActionResult FileTypeCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/FileTypes";
                    request.RootElement = "ArrayOfFILE_TYPE";
                    List<FILE_TYPE> fileType = serviceCaller.Execute<List<FILE_TYPE>>(request);
                    return View("../Settings/Lookups/LookupCreates/FileTypeCreate", fileType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new FileType was created, post it
        //POST: /Settings/Lookups/LookupCreates/FileTypeCreate
        [HttpPost]
        public ActionResult FileTypeCreate(FILE_TYPE newFileType)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/FileTypes";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<FILE_TYPE>(newFileType), ParameterType.RequestBody);

                serviceCaller.Execute<FILE_TYPE>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/FileTypeDelete/1
        public Boolean FileTypeDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/FileTypes/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<FILE_TYPE>(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion File_Type

        #region Horizontal_Collect_Method
        //need to redirect to get index if error and want to refresh HCollectMethod
        public ActionResult HCollectMethod()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the HCollectMethod infoxbox -- get HCollectMethod page or redirect to HCollectMethod
        //GET: /Settings/Lookups/LookupEdits/HCollectMethod/
        [HttpPost]
        public ActionResult HCollectMethod(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("HCollectMethodCreate"); }
                else
                {//edit
                    int HCollectMethId = Convert.ToInt32(fc["HCOLLECT_METHOD_ID"]);
                    if (HCollectMethId == 0)
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with one to edit
                        return RedirectToAction("HCollectMethodEdit", new { id = HCollectMethId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //horizontal collect methods edit page
        public ActionResult HCollectMethodEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/HorizontalMethods/{entityId}";
                    request.RootElement = "HORIZONTAL_COLLECT_METHODS";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    HORIZONTAL_COLLECT_METHODS hColl = serviceCaller.Execute<HORIZONTAL_COLLECT_METHODS>(request);
                    return View("../Settings/Lookups/LookupEdits/HCollectMethodEdit", hColl);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to HCollectMethod, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/HCollectMethodEdit/
        [HttpPost]
        public ActionResult HCollectMethodEdit(int id, HORIZONTAL_COLLECT_METHODS aHCollMethod)
        {
            try
            {

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/HorizontalMethods/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<HORIZONTAL_COLLECT_METHODS>(aHCollMethod), ParameterType.RequestBody);

                serviceCaller.Execute<HORIZONTAL_COLLECT_METHODS>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from HCollectMethod Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/HCollectMethodCreate/
        public ActionResult HCollectMethodCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/HorizontalMethods/";
                    request.RootElement = "ArrayOfHORIZONTAL_COLLECT_METHODS";
                    List<HORIZONTAL_COLLECT_METHODS> hColMethods = serviceCaller.Execute<List<HORIZONTAL_COLLECT_METHODS>>(request);
                    return View("../Settings/Lookups/LookupCreates/HCollectMethodCreate", hColMethods);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new Horizontal Collection MEthod was created, post it
        //POST: /Settings/Lookups/LookupCreates/HCollectMethodCreate
        [HttpPost]
        public ActionResult HCollectMethodCreate(HORIZONTAL_COLLECT_METHODS newHColMethod)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/HorizontalMethods";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<HORIZONTAL_COLLECT_METHODS>(newHColMethod), ParameterType.RequestBody);

                serviceCaller.Execute<HORIZONTAL_COLLECT_METHODS>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/HCollectMethodDelete/1
        public Boolean HCollectMethodDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/HorizontalMethods/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<HORIZONTAL_COLLECT_METHODS>(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Horizontal_Collect_Method

        #region HorizontalDatum
        //need to redirect to get index if error and want to refresh horizontalDatum
        public ActionResult HDatum()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the HDatum infoxbox -- get HDatum page or redirect to HDatumCreate
        //GET: /Settings/Lookups/LookupEdits/HDatumEdit/
        [HttpPost]
        public ActionResult HDatum(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("HDatumCreate"); }
                else
                {//edit
                    int HDatumId = Convert.ToInt32(fc["DATUM_ID"]);
                    if (HDatumId == 0) //they didn't choose one before hitting edit
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("HDatumEdit", new { id = HDatumId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //hdatum edit page
        public ActionResult HDatumEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/HorizontalDatums/{horizontalDatumId}";
                    request.RootElement = "HORIZONTAL_DATUMS";
                    request.AddParameter("horizontalDatumId", id, ParameterType.UrlSegment);
                    HORIZONTAL_DATUMS HDType = serviceCaller.Execute<HORIZONTAL_DATUMS>(request);

                    return View("../Settings/Lookups/LookupEdits/HDatumEdit", HDType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to hdatum, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/HDatumEdit/
        [HttpPost]
        public ActionResult HDatumEdit(int id, HORIZONTAL_DATUMS anHDatum)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/HorizontalDatums/{horizontalDatumId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("horizontalDatumId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<HORIZONTAL_DATUMS>(anHDatum), ParameterType.RequestBody);

                serviceCaller.Execute<HORIZONTAL_DATUMS>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from HDatum Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/HDatumCreate/
        public ActionResult HDatumCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/HorizontalDatums/";
                    request.RootElement = "ArrayOfHORIZONTAL_DATUMS";
                    List<HORIZONTAL_DATUMS> HDatums = serviceCaller.Execute<List<HORIZONTAL_DATUMS>>(request);
                    return View("../Settings/Lookups/LookupCreates/HDatumCreate", HDatums);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new Hdatum was created, post it
        //POST: /Settings/Lookups/LookupCreates/HDatumCreate
        [HttpPost]
        public ActionResult HDatumCreate(HORIZONTAL_DATUMS newHDatum)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/HorizontalDatums";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<HORIZONTAL_DATUMS>(newHDatum), ParameterType.RequestBody);

                HORIZONTAL_DATUMS newEV = serviceCaller.Execute<HORIZONTAL_DATUMS>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/HDatumDelete/1
        public Boolean HDatumDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/HorizontalDatums/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<HORIZONTAL_DATUMS>(request);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion HortizontalDatum

        #region Housing_Type
        //need to redirect to get index if error and want to refresh housingType
        public ActionResult HousingType()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the Housing Type infoxbox -- get HousingTypeEdit page or redirect to HousingTypeCreate
        //GET: /Settings/Lookups/LookupEdits/HousingTypeEdit/
        [HttpPost]
        public ActionResult HousingType(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("HousingTypeCreate"); }
                else
                {//edit
                    int housingTypeId = Convert.ToInt32(fc["HOUSING_TYPE_ID"]);
                    if (housingTypeId == 0) //they didn't choose one before hitting edit
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("HousingTypeEdit", new { id = housingTypeId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //Housing type edit page
        public ActionResult HousingTypeEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/HousingTypes/{entityId}";
                    request.RootElement = "HOUSING_TYPE";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    HOUSING_TYPE housType = serviceCaller.Execute<HOUSING_TYPE>(request);

                    return View("../Settings/Lookups/LookupEdits/HousingTypeEdit", housType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to event type, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/HousingTypeEdit/
        [HttpPost]
        public ActionResult HousingTypeEdit(int id, HOUSING_TYPE anHousingType)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/HousingTypes/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<HOUSING_TYPE>(anHousingType), ParameterType.RequestBody);

                serviceCaller.Execute<HOUSING_TYPE>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from EventType Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/NetworkTypeCreate/
        public ActionResult HousingTypeCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/HousingTypes/";
                    request.RootElement = "ArrayOfHOUSING_TYPE";
                    List<HOUSING_TYPE> houseType = serviceCaller.Execute<List<HOUSING_TYPE>>(request);
                    return View("../Settings/Lookups/LookupCreates/HousingTypeCreate", houseType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new event type was created, post it
        //POST: /Settings/Lookups/LookupCreates/HousingTypeCreate
        [HttpPost]
        public ActionResult HousingTypeCreate(HOUSING_TYPE newHousingType)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/HousingTypes";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<HOUSING_TYPE>(newHousingType), ParameterType.RequestBody);

                HOUSING_TYPE newHou = serviceCaller.Execute<HOUSING_TYPE>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/EventTypeDelete/1
        public Boolean HousingTypeDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/HousingTypes/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<HOUSING_TYPE>(request);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Housing_Type
        
        #region HWM_Quality
        //need to redirect to get index if error and want to refresh HWMQual
        public ActionResult HWMQual()
        {
            return RedirectToAction("Index");
        }
        
        //a edit or add new button was clicked in the HWM Quality infoxbox -- get HWMQualEdit page or redirect to HWMQualCreate
        //GET: /Settings/Lookups/LookupEdits/HWMQualEdit/
        [HttpPost]
        public ActionResult HWMQual(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("HWMQualCreate"); }
                else
                {//edit
                    int HWMQualId = Convert.ToInt32(fc["HWM_QUALITY_ID"]);
                    if (HWMQualId == 0) //they didn't choose one.
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with one to edit
                        return RedirectToAction("HWMQualEdit", new { id = HWMQualId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //hwmqual edit page
        public ActionResult HWMQualEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/HWMQualities/{entityId}";
                    request.RootElement = "HWM_QUALITIES";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    HWM_QUALITIES HWMQual = serviceCaller.Execute<HWM_QUALITIES>(request);
                    return View("../Settings/Lookups/LookupEdits/HWMQualEdit", HWMQual);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to HWM qual, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/HWMQualEdit/
        [HttpPost]
        public ActionResult HWMQualEdit(int id, HWM_QUALITIES aHWMQual)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/HWMQualities/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<HWM_QUALITIES>(aHWMQual), ParameterType.RequestBody);

                serviceCaller.Execute<HWM_QUALITIES>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }

        }

        //redirected here from HWM qual Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/HWMQualCreate/
        public ActionResult HWMQualCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/HWMQualities";
                    request.RootElement = "ArrayOfHWM_QUALITIES";
                    List<HWM_QUALITIES> hwmQual = serviceCaller.Execute<List<HWM_QUALITIES>>(request);
                    return View("../Settings/Lookups/LookupCreates/HWMQualCreate", hwmQual);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new HWM qual was created, post it
        //POST: /Settings/Lookups/LookupCreates/HWMQualCreate
        [HttpPost]
        public ActionResult HWMQualCreate(HWM_QUALITIES newHWMQual)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/HWMQualities";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<HWM_QUALITIES>(newHWMQual), ParameterType.RequestBody);

                serviceCaller.Execute<HWM_QUALITIES>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/HWMQualDelete/1
        public Boolean HWMQualDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/HWMQualities/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<HWM_QUALITIES>(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion HWM_Quality

        #region HWM_Type
        //need to redirect to get index if error and want to refresh HWMType
        public ActionResult HWMType()
        {
            return RedirectToAction("Index");
        }
        
        //a edit or add new button was clicked in the HWM Type infoxbox -- get HWMTypeEdit page or redirect to HWMTypeCreate
        //GET: /Settings/Lookups/LookupEdits/HWMTypeEdit/
        [HttpPost]
        public ActionResult HWMType(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("HWMTypeCreate"); }
                else
                {//edit
                    int HWMTypeId = Convert.ToInt32(fc["HWM_TYPE_ID"]);
                    if (HWMTypeId == 0)
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("HWMTypeEdit", new { id = HWMTypeId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //hwmtype edit page
        public ActionResult HWMTypeEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/HWMTypes/{entityId}";
                    request.RootElement = "HWM_TYPES";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    HWM_TYPES HWMType = serviceCaller.Execute<HWM_TYPES>(request);
                    return View("../Settings/Lookups/LookupEdits/HWMTypeEdit", HWMType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to HWM type, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/HWMTypeEdit/
        [HttpPost]
        public ActionResult HWMTypeEdit(int id, HWM_TYPES aHWMType)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/HWMTypes/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<HWM_TYPES>(aHWMType), ParameterType.RequestBody);

                serviceCaller.Execute<HWM_TYPES>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from HWM Type Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/HWMTypeCreate/
        public ActionResult HWMTypeCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/HWMTypes";
                    request.RootElement = "ArrayOfHWM_TYPES";
                    List<HWM_TYPES> hwmType = serviceCaller.Execute<List<HWM_TYPES>>(request);
                    return View("../Settings/Lookups/LookupCreates/HWMTypeCreate", hwmType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new HWM type was created, post it
        //POST: /Settings/Lookups/LookupCreates/HWMTypeCreate
        [HttpPost]
        public ActionResult HWMTypeCreate(HWM_TYPES newHWMType)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/HWMTypes";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<HWM_TYPES>(newHWMType), ParameterType.RequestBody);

                serviceCaller.Execute<HWM_TYPES>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/HWMTypeDelete/1
        public Boolean HWMTypeDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/HWMTypes/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<HWM_TYPES>(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion HWM_Type

        #region Instr_Collect_Conditions
        //need to redirect to get index if error and want to refresh InstrCollCond
        public ActionResult InstrCollCond()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the InstrCollCond infoxbox -- get InstrCollCondEdit page or redirect to InstrCollCondCreate
        //GET: /Settings/Lookups/LookupEdits/SensorTypeEdit/
        [HttpPost]
        public ActionResult InstrCollCond(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("InstrCollCondCreate"); }
                else
                {//edit
                    int InstrCollCondId = Convert.ToInt32(fc["ID"]);
                    if (InstrCollCondId == 0)
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with one to edit
                        return RedirectToAction("InstrCollCondEdit", new { id = InstrCollCondId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //InstrCollCond edit page
        public ActionResult InstrCollCondEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/InstrCollectConditions/{entityId}";
                    request.RootElement = "INSTR_COLLECTION_CONDITIONS";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    INSTR_COLLECTION_CONDITIONS InsColCond = serviceCaller.Execute<INSTR_COLLECTION_CONDITIONS>(request);
                    return View("../Settings/Lookups/LookupEdits/InstrCollCondEdit", InsColCond);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to InstrCollCond, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/InstrCollCondEdit/
        [HttpPost]
        public ActionResult InstrCollCondEdit(int id, INSTR_COLLECTION_CONDITIONS anInstColCond)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/InstrCollectConditions/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<INSTR_COLLECTION_CONDITIONS>(anInstColCond), ParameterType.RequestBody);

                serviceCaller.Execute<INSTR_COLLECTION_CONDITIONS>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from InstrCollCond Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/InstrCollCondCreate/
        public ActionResult InstrCollCondCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/InstrCollectConditions/";
                    request.RootElement = "ArrayOfINSTR_COLLECTION_CONDITIONS";
                    List<INSTR_COLLECTION_CONDITIONS> insColCond = serviceCaller.Execute<List<INSTR_COLLECTION_CONDITIONS>>(request);
                    return View("../Settings/Lookups/LookupCreates/InstrCollCondCreate", insColCond);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new InstrCollCond was created, post it
        //POST: /Settings/Lookups/LookupCreates/InstrCollCondCreate
        [HttpPost]
        public ActionResult InstrCollCondCreate(INSTR_COLLECTION_CONDITIONS newInstColCond)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/InstrCollectConditions";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<INSTR_COLLECTION_CONDITIONS>(newInstColCond), ParameterType.RequestBody);

                serviceCaller.Execute<INSTR_COLLECTION_CONDITIONS>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/InstrCollCondDelete/1
        public Boolean InstrCollCondDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/InstrCollectConditions/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<INSTR_COLLECTION_CONDITIONS>(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Instr_Collect_Conditions

        #region Markers
        //need to redirect to get index if error and want to refresh Markers
        public ActionResult Markers()
        {
            return RedirectToAction("Index");
        }
        
        //a edit or add new button was clicked in the Markers infoxbox -- get MarkersEdit page or redirect to MarkersCreate
        //GET: /Settings/Lookups/LookupEdits/MarkersEdit/
        [HttpPost]
        public ActionResult Markers(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("MarkersCreate"); }
                else
                {//edit
                    int markId = Convert.ToInt32(fc["MARKER_ID"]);
                    if (markId == 0)
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("MarkersEdit", new { id = markId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //markers edit page
        public ActionResult MarkersEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/Markers/{entityId}";
                    request.RootElement = "MARKER";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    MARKER marker = serviceCaller.Execute<MARKER>(request);
                    return View("../Settings/Lookups/LookupEdits/MarkersEdit", marker);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }
        
        //change was made to Markes, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/MarkersEdit/
        [HttpPost]
        public ActionResult MarkersEdit(int id, MARKER aMarker)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/Markers/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<MARKER>(aMarker), ParameterType.RequestBody);

                serviceCaller.Execute<MARKER>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from Markers Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/MarkersCreate/
        public ActionResult MarkersCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/Markers";
                    request.RootElement = "ArrayOfMARKER";
                    List<MARKER> mark = serviceCaller.Execute<List<MARKER>>(request);
                    return View("../Settings/Lookups/LookupCreates/MarkersCreate", mark);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new marker was created, post it
        //POST: /Settings/Lookups/LookupCreates/MarkersCreate
        [HttpPost]
        public ActionResult MarkersCreate(MARKER newMark)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/Markers";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<MARKER>(newMark), ParameterType.RequestBody);

                serviceCaller.Execute<MARKER>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/MarkersDelete/1
        public Boolean MarkersDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/Markers/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<MARKER>(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Markers

        #region Network_Name
        //need to redirect to get index if error and want to refresh NetworkName
        public ActionResult NetworkName()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the NetworkName infoxbox -- get NetworkNameEdit page or redirect to NetworkNameCreate
        //GET: /Settings/Lookups/LookupEdits/NetworkTypeEdit/
        [HttpPost]
        public ActionResult NetworkName(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("NetworkNameCreate"); }
                else
                {//edit
                    int networkNameId = Convert.ToInt32(fc["NETWORK_NAME_ID"]);
                    if (networkNameId == 0) //they didn't choose one before hitting edit
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("NetworkNameEdit", new { id = networkNameId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //event type edit page
        public ActionResult NetworkNameEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/NetworkNames/{entityId}";
                    request.RootElement = "NETWORK_NAME";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    NETWORK_NAME netName = serviceCaller.Execute<NETWORK_NAME>(request);

                    return View("../Settings/Lookups/LookupEdits/NetworkNameEdit", netName);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to NetworkName, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/NetworkNameEdit/
        [HttpPost]
        public ActionResult NetworkNameEdit(int id, NETWORK_NAME anNetworkName)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/NetworkNames/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<NETWORK_NAME>(anNetworkName), ParameterType.RequestBody);

                serviceCaller.Execute<NETWORK_NAME>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from NetworkName Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/NetworkNameCreate/
        public ActionResult NetworkNameCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/NetworkNames/";
                    request.RootElement = "ArrayOfNETWORK_NAME";
                    List<NETWORK_NAME> netType = serviceCaller.Execute<List<NETWORK_NAME>>(request);
                    return View("../Settings/Lookups/LookupCreates/NetworkNameCreate", netType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new network Name was created, post it
        //POST: /Settings/Lookups/LookupCreates/NetworkNameCreate
        [HttpPost]
        public ActionResult NetworkNameCreate(NETWORK_NAME newNetworkName)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/NetworkNames";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<NETWORK_NAME>(newNetworkName), ParameterType.RequestBody);

                NETWORK_NAME newNetName = serviceCaller.Execute<NETWORK_NAME>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/NetworkNameDelete/1
        public Boolean NetworkNameDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/NetworkNames/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<NETWORK_NAME>(request);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Network_Name

        #region Network_Type
        //need to redirect to get index if error and want to refresh NetworkName
        public ActionResult NetworkType()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the Network Types infoxbox -- get NetworkTypeEdit page or redirect to NetworkTypeCreate
        //GET: /Settings/Lookups/LookupEdits/NetworkTypeEdit/
        [HttpPost]
        public ActionResult NetworkType(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("NetworkTypeCreate"); }
                else
                {//edit
                    int networkTypeId = Convert.ToInt32(fc["NETWORK_TYPE_ID"]);
                    if (networkTypeId == 0) //they didn't choose one before hitting edit
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("NetworkTypeEdit", new { id = networkTypeId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //network type edit page
        public ActionResult NetworkTypeEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/NetworkTypes/{entityId}";
                    request.RootElement = "NETWORK_TYPE";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    NETWORK_TYPE netType = serviceCaller.Execute<NETWORK_TYPE>(request);

                    return View("../Settings/Lookups/LookupEdits/NetworkTypeEdit", netType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to network type, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/NetworkTypeEdit/
        [HttpPost]
        public ActionResult NetworkTypeEdit(int id, NETWORK_TYPE anNetworkType)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/NetworkTypes/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<NETWORK_TYPE>(anNetworkType), ParameterType.RequestBody);

                serviceCaller.Execute<NETWORK_TYPE>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from NetworkType Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/NetworkTypeCreate/
        public ActionResult NetworkTypeCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/NetworkTypes/";
                    request.RootElement = "ArrayOfNETWORK_TYPE";
                    List<NETWORK_TYPE> netType = serviceCaller.Execute<List<NETWORK_TYPE>>(request);
                    return View("../Settings/Lookups/LookupCreates/NetworkTypeCreate", netType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new network type was created, post it
        //POST: /Settings/Lookups/LookupCreates/NetworkTypeCreate
        [HttpPost]
        public ActionResult NetworkTypeCreate(NETWORK_TYPE newNetworkType)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/NetworkTypes";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<NETWORK_TYPE>(newNetworkType), ParameterType.RequestBody);

                NETWORK_TYPE newNet = serviceCaller.Execute<NETWORK_TYPE>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/NetworkTypeDelete/1
        public Boolean NetworkTypeDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/NetworkTypes/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<NETWORK_TYPE>(request);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Network_Type

        #region ObjectivePointTypes
        //need to redirect to get index if error and want to refresh OPTypes
        public ActionResult OPTypes()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the OPTypes infoxbox -- get OPTypesEdit page or redirect to OPTypesCreate
        //GET: /Settings/Lookups/LookupEdits/OPTypesEdit/
        [HttpPost]
        public ActionResult OPTypes(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("OPTypesCreate"); }
                else
                {//edit
                    int OPTId = Convert.ToInt32(fc["OBJECTIVE_POINT_TYPE_ID"]);
                    if (OPTId == 0)
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("OPTypesEdit", new { id = OPTId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //markers edit page
        public ActionResult OPTypesEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/OPTypes/{entityId}";
                    request.RootElement = "OBJECTIVE_POINT_TYPE";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    OBJECTIVE_POINT_TYPE opt = serviceCaller.Execute<OBJECTIVE_POINT_TYPE>(request);
                    return View("../Settings/Lookups/LookupEdits/OPTypesEdit", opt);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to OPTypes, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/OPTypesEdit/
        [HttpPost]
        public ActionResult OPTypesEdit(int id, OBJECTIVE_POINT_TYPE anOPT)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/OPTypes/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<OBJECTIVE_POINT_TYPE>(anOPT), ParameterType.RequestBody);

                serviceCaller.Execute<OBJECTIVE_POINT_TYPE>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from OPTypes Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/OPTypesCreate/
        public ActionResult OPTypesCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/OPTypes";
                    request.RootElement = "ArrayOfOBJECTIVE_POINT_TYPE";
                    List<OBJECTIVE_POINT_TYPE> anOPT = serviceCaller.Execute<List<OBJECTIVE_POINT_TYPE>>(request);
                    return View("../Settings/Lookups/LookupCreates/OPTypesCreate", anOPT);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new marker was created, post it
        //POST: /Settings/Lookups/LookupCreates/OPTypesCreate
        [HttpPost]
        public ActionResult OPTypesCreate(OBJECTIVE_POINT_TYPE newOPT)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/OPTypes";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<OBJECTIVE_POINT_TYPE>(newOPT), ParameterType.RequestBody);

                serviceCaller.Execute<OBJECTIVE_POINT_TYPE>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/MarkersDelete/1
        public Boolean OPTypesDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/OPTypes/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<OBJECTIVE_POINT_TYPE>(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion ObjectivePointTypes

        #region OP_QUALITY
        //need to redirect to get index if error and want to refresh OPQuality
        public ActionResult OPQuality()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the OPQuality infoxbox -- get OPQualityEdit page or redirect to OPQualityCreate
        //GET: /Settings/Lookups/LookupEdits/OPQualityEdit/
        [HttpPost]
        public ActionResult OPQuality(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("OPQualityCreate"); }
                else
                {//edit
                    int OPQualityId = Convert.ToInt32(fc["OP_QUALITY_ID"]);
                    if (OPQualityId == 0)
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with one to edit
                        return RedirectToAction("OPQualityEdit", new { id = OPQualityId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //OPQuality edit page
        public ActionResult OPQualityEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/ObjectivePointQualities/{entityId}";
                    request.RootElement = "OP_QUALITY";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    OP_QUALITY opQual = serviceCaller.Execute<OP_QUALITY>(request);
                    return View("../Settings/Lookups/LookupEdits/OPQualityEdit", opQual);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to OPQuality, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/OPQualityEdit/
        [HttpPost]
        public ActionResult OPQualityEdit(int id, OP_QUALITY anOPQual)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/ObjectivePointQualities/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<OP_QUALITY>(anOPQual), ParameterType.RequestBody);

                serviceCaller.Execute<OP_QUALITY>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }

        }

        //redirected here from OPQuality Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/OPQualityCreate/
        public ActionResult OPQualityCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/ObjectivePointQualities";
                    request.RootElement = "ArrayOfOP_QUALITY";
                    List<OP_QUALITY> OPQuals = serviceCaller.Execute<List<OP_QUALITY>>(request);
                    return View("../Settings/Lookups/LookupCreates/OPQualityCreate", OPQuals);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new OPQuality was created, post it
        //POST: /Settings/Lookups/LookupCreates/OPQualityCreate
        [HttpPost]
        public ActionResult OPQualityCreate(OP_QUALITY newOPQual)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/ObjectivePointQualities/";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<OP_QUALITY>(newOPQual), ParameterType.RequestBody);

                serviceCaller.Execute<OP_QUALITY>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/OPQualityDelete/1
        public Boolean OPQualityDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/ObjectivePointQualities/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<OP_QUALITY>(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion OP_QUALITY

        #region SensorBrand
        //need to redirect to get index if error and want to refresh SensorBrand
        public ActionResult SensorBrand()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the SensorBrand infoxbox -- get SensorBrandEdit page or redirect to SensorBrandCreate
        //GET: /Settings/Lookups/LookupEdits/SensorBrandEdit/
        [HttpPost]
        public ActionResult SensorBrand(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("SensorBrandCreate"); }
                else
                {//edit
                    int sensBrandId = Convert.ToInt32(fc["SENSOR_BRAND_ID"]);
                    if (sensBrandId == 0)
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("SensorBrandEdit", new { id = sensBrandId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //SensorBrand edit page
        public ActionResult SensorBrandEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/SensorBrands/{entityId}";
                    request.RootElement = "SENSOR_BRAND";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    SENSOR_BRAND senBrand = serviceCaller.Execute<SENSOR_BRAND>(request);
                    return View("../Settings/Lookups/LookupEdits/SensorBrandEdit", senBrand);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to SensorBrand, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/SensorBrandEdit/
        [HttpPost]
        public ActionResult SensorBrandEdit(int id, SENSOR_BRAND aSenBrand)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/SensorBrands/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<SENSOR_BRAND>(aSenBrand), ParameterType.RequestBody);

                serviceCaller.Execute<SENSOR_BRAND>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from SensorBrand Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/SensorBrandCreate/
        public ActionResult SensorBrandCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/SensorBrands";
                    request.RootElement = "ArrayOfSENSOR_BRAND";
                    List<SENSOR_BRAND> senBrand = serviceCaller.Execute<List<SENSOR_BRAND>>(request);
                    return View("../Settings/Lookups/LookupCreates/SensorBrandCreate", senBrand);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new marker was created, post it
        //POST: /Settings/Lookups/LookupCreates/MarkersCreate
        [HttpPost]
        public ActionResult SensorBrandCreate(SENSOR_BRAND newSenBrand)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/SensorBrands";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<SENSOR_BRAND>(newSenBrand), ParameterType.RequestBody);

                serviceCaller.Execute<SENSOR_BRAND>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/MarkersDelete/1
        public Boolean SensorBrandDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/SensorBrands/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<SENSOR_BRAND>(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion SensorBrand

        #region Sensor_type
        //need to redirect to get index if error and want to refresh eventType
        public ActionResult SensorType()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the Sensor Type infoxbox -- get SensorTypeEdit page or redirect to SensorTypeCreate
        //GET: /Settings/Lookups/LookupEdits/SensorTypeEdit/
        [HttpPost]
        public ActionResult SensorType(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("SensorTypeCreate"); }
                else
                {//edit
                    int sensorTypeId = Convert.ToInt32(fc["SENSOR_TYPE_ID"]);
                    if (sensorTypeId == 0)
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with one to edit
                        return RedirectToAction("SensorTypeEdit", new { id = sensorTypeId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //sensor type edit page
        public ActionResult SensorTypeEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/SensorTypes/{entityId}";
                    request.RootElement = "SENSOR_TYPE";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    SENSOR_TYPE seType = serviceCaller.Execute<SENSOR_TYPE>(request);

                    //get this sensor's deployment types (to check off)
                    request = new RestRequest();
                    request.Resource = "/SensorTypes/{sensorTypeID}/DeploymentTypes";
                    request.RootElement = "ArrayOfDEPLOYMENT_TYPE";
                    request.AddParameter("sensorTypeID", id, ParameterType.UrlSegment);
                    List<DEPLOYMENT_TYPE> sensorDeplTypes = serviceCaller.Execute<List<DEPLOYMENT_TYPE>>(request);

                    //put id's into string[]
                    List<decimal> decSensorDepIDs = sensorDeplTypes.Select(r => r.DEPLOYMENT_TYPE_ID).ToList<decimal>();
                    ViewData["decSenDepIDs"] = decSensorDepIDs; //decimal version so view can loop to determine whether or not to check box

                    //get all the deployment types to allow link to be made
                    request = new RestRequest();
                    request.Resource = "/DeploymentTypes";
                    request.RootElement = "ArrayOfDEPLOYMENT_TYPE";
                    ViewData["DeplTypeList"] = serviceCaller.Execute<List<DEPLOYMENT_TYPE>>(request);
                    SensorTypeModel thisSensor = new SensorTypeModel();

                    thisSensor.aSensorType = seType;

                    return View("../Settings/Lookups/LookupEdits/SensorTypeEdit", thisSensor);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to Sensor type, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/SensorTypeEdit/
        [HttpPost]
        public ActionResult SensorTypeEdit(int id, SensorTypeModel Sensor)
        {
            try
            {   
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/SensorTypes/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<SENSOR_TYPE>(Sensor.aSensorType), ParameterType.RequestBody);
                serviceCaller.Execute<SENSOR_TYPE>(request);

                //section to determine if Deployment Type was changed or chosen for adding relationship to sensor
                //convert string of Deployment Type IDs linked before edited to a List<decimal> 
                List<decimal> previousSensDepList = new List<decimal>();
                if (Sensor.existingSenDepTyps != null)
                {
                    string[] parsedlist = Regex.Split(Sensor.existingSenDepTyps, ",");
                    foreach (string x in parsedlist)
                    {
                        if (!string.IsNullOrWhiteSpace(x))
                            previousSensDepList.Add(Convert.ToDecimal(x));
                    }
                }
                List<decimal> NewOnes = new List<decimal>();
                List<decimal> RemoveOnes = new List<decimal>();
                
                //Any checked (these are the ones that are or should be linked
                if (Sensor.SensorDeploymentTypes != null)
                {                    
                    //if there are no ids, then we can just add the sensorDeploymentTypes
                    if (previousSensDepList.Count >= 1)
                    {                        //if both lists are the same (have all the same ids) = no change
                        if (!previousSensDepList.SequenceEqual(Sensor.SensorDeploymentTypes))
                        {                    //Not the same:            
                            //if sensordeploymenttypes has an id that is not in previoussensdeplist = new one
                            foreach (decimal x in Sensor.SensorDeploymentTypes.Except(previousSensDepList))
                            {
                                NewOnes.Add(x);
                            }
                            //if previoussensdeplist has an id that is not in sensordeploymenttypes = remove it
                            foreach (decimal y in previousSensDepList.Except(Sensor.SensorDeploymentTypes))
                            {
                                RemoveOnes.Add(y);
                            }
                        }//end if
                    }
                    else
                    {   //some are checked, but none in previousSensDepList, add them
                        foreach (decimal a in Sensor.SensorDeploymentTypes)
                        {
                            NewOnes.Add(a);
                        }
                    }
                }
                else
                {   
                    //none were checked. Does that mean they unchecked some? Look at previous list
                    if (previousSensDepList.Count >= 1)
                    {
                        //all the previous ones need to be removed
                        foreach (decimal i in previousSensDepList)
                        {
                            RemoveOnes.Add(i);
                        }
                    }
                }
                
                //now go through and add/remove them
                if (RemoveOnes.Count >= 1)
                {
                    foreach (decimal Rdt in RemoveOnes)
                    {
                        //get it
                        request = new RestRequest();
                        request.Resource = "/DeploymentTypes/{entityId}";
                        request.RootElement = "DEPLOYMENT_TYPE";
                        request.AddParameter("entityId", Rdt, ParameterType.UrlSegment);
                        DEPLOYMENT_TYPE DepType = serviceCaller.Execute<DEPLOYMENT_TYPE>(request);
                        //remove them
                        request = new RestRequest(Method.POST);
                        request.Resource = "SensorTypes/{sensorTypeId}/removeDeploymentType";
                        request.AddParameter("sensorTypeId", id, ParameterType.UrlSegment);
                        request.RequestFormat = DataFormat.Xml;
                        request.AddHeader("Content-Type", "application/xml");
                        //Use extended serializer
                        serializer = new STNWebSerializer();
                        request.AddParameter("application/xml", serializer.Serialize<SENSOR_TYPE>(DepType), ParameterType.RequestBody);
                        serviceCaller.Execute<DEPLOYMENT_TYPE>(request);
                    }
                }
                if (NewOnes.Count >= 1)
                {
                    foreach (decimal Ndt in NewOnes)
                    {
                        //get it
                        request = new RestRequest();
                        request.Resource = "/DeploymentTypes/{entityId}";
                        request.RootElement = "DEPLOYMENT_TYPE";
                        request.AddParameter("entityId", Ndt, ParameterType.UrlSegment);
                        DEPLOYMENT_TYPE DepType = serviceCaller.Execute<DEPLOYMENT_TYPE>(request);
                        //Add them
                        request = new RestRequest(Method.POST);
                        request.Resource = "/SensorTypes/{sensorTypeId}/addDeploymentType";
                        request.AddParameter("sensorTypeId", id, ParameterType.UrlSegment);
                        request.RequestFormat = DataFormat.Xml;
                        request.AddHeader("Content-Type", "application/xml");
                        //Use extended serializer
                        serializer = new STNWebSerializer();
                        request.AddParameter("application/xml", serializer.Serialize<DEPLOYMENT_TYPE>(DepType), ParameterType.RequestBody);
                        List<DEPLOYMENT_TYPE> addedSDT = serviceCaller.Execute<List<DEPLOYMENT_TYPE>>(request);
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from EventType Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/SensorTypeCreate/
        public ActionResult SensorTypeCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/SensorTypes/";
                    request.RootElement = "ArrayOfSENSOR_TYPE";
                    List<SENSOR_TYPE> seType = serviceCaller.Execute<List<SENSOR_TYPE>>(request);
                    return View("../Settings/Lookups/LookupCreates/SensorTypeCreate", seType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new SensorType was created, post it
        //POST: /Settings/Lookups/LookupCreates/SensorTypeCreate
        [HttpPost]
        public ActionResult SensorTypeCreate(SENSOR_TYPE newSensorType)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/SensorTypes";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<SENSOR_TYPE>(newSensorType), ParameterType.RequestBody);

                serviceCaller.Execute<SENSOR_TYPE>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/SensorTypeDelete/1
        public Boolean SensorTypeDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/SensorTypes/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<SENSOR_TYPE>(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Sensor_type

        #region Status_Type
        //need to redirect to get index if error and want to refresh statusTyp
        public ActionResult StatusType()
        {
            return RedirectToAction("Index");
        }
        
        //a edit or add new button was clicked in the Status Type infoxbox -- get StatusTypeEdit page or redirect to StatusTypeCreate
        //GET: /Settings/Lookups/LookupEdits/StatusTypeEdit/
        [HttpPost]
        public ActionResult StatusType(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("StatusTypeCreate"); }
                else
                {//edit
                    int statusTypeId = Convert.ToInt32(fc["STATUS_TYPE_ID"]);
                    if (statusTypeId == 0)
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with one to edit
                        return RedirectToAction("StatusTypeEdit", new { id = statusTypeId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //statustype edit page
        public ActionResult StatusTypeEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/StatusTypes/{entityId}";
                    request.RootElement = "STATUS_TYPE";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    STATUS_TYPE stType = serviceCaller.Execute<STATUS_TYPE>(request);
                    return View("../Settings/Lookups/LookupEdits/StatusTypeEdit", stType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to Status type, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/StatusTypeEdit/
        [HttpPost]
        public ActionResult StatusTypeEdit(int id, STATUS_TYPE aStatusType)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/StatusTypes/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<STATUS_TYPE>(aStatusType), ParameterType.RequestBody);

                serviceCaller.Execute<STATUS_TYPE>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }

        }

        //redirected here from StatusType Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/StatusTypeCreate/
        public ActionResult StatusTypeCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/StatusTypes";
                    request.RootElement = "ArrayOfSTATUS_TYPE";
                    List<STATUS_TYPE> stType = serviceCaller.Execute<List<STATUS_TYPE>>(request);
                    return View("../Settings/Lookups/LookupCreates/StatusTypeCreate", stType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new Status type was created, post it
        //POST: /Settings/Lookups/LookupCreates/StatusTypeCreate
        [HttpPost]
        public ActionResult StatusTypeCreate(STATUS_TYPE newStatusType)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/StatusTypes/";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<STATUS_TYPE>(newStatusType), ParameterType.RequestBody);

                serviceCaller.Execute<STATUS_TYPE>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/StatusTypeDelete/1
        public Boolean StatusTypeDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/StatusTypes/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<STATUS_TYPE>(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Status_Type
        
        #region Vertical_Collect_Method
        //need to redirect to get index if error and want to refresh CollectMethod
        public ActionResult VCollectMethod()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the CollectMethod infoxbox -- get CollectMethodEdit page or redirect to CollectMethodCreate
        //GET: /Settings/Lookups/LookupEdits/CollectMethodEdit/
        [HttpPost]
        public ActionResult VCollectMethod(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("VCollectMethodCreate"); }
                else
                {//edit
                    int VCollectMethodId = Convert.ToInt32(fc["VCOLLECT_METHOD_ID"]);
                    if (VCollectMethodId == 0)
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("VCollectMethodEdit", new { id = VCollectMethodId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //collectmethod edit page
        public ActionResult VCollectMethodEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/VerticalMethods/{entityId}";
                    request.RootElement = "VERTICAL_COLLECT_METHODS";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    VERTICAL_COLLECT_METHODS VcolMethod = serviceCaller.Execute<VERTICAL_COLLECT_METHODS>(request);
                    return View("../Settings/Lookups/LookupEdits/VCollectMethodEdit", VcolMethod);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }
        //change was made to CollectMethod, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/CollectMethodEdit/
        [HttpPost]
        public ActionResult VCollectMethodEdit(int id, VERTICAL_COLLECT_METHODS aVColMethod)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/VerticalMethods/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<VERTICAL_COLLECT_METHODS>(aVColMethod), ParameterType.RequestBody);

                serviceCaller.Execute<VERTICAL_COLLECT_METHODS>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from CollectMethod Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/CollectMethodCreate/
        public ActionResult VCollectMethodCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/VerticalMethods/";
                    request.RootElement = "ArrayOfVERTICAL_COLLECT_METHODS";
                    List<VERTICAL_COLLECT_METHODS> VcolMethod = serviceCaller.Execute<List<VERTICAL_COLLECT_METHODS>>(request);
                    return View("../Settings/Lookups/LookupCreates/VCollectMethodCreate", VcolMethod);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new collect method was created, post it
        //POST: /Settings/Lookups/LookupCreates/CollectMethodCreate
        [HttpPost]
        public ActionResult VCollectMethodCreate(VERTICAL_COLLECT_METHODS newVColMeth)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/VerticalMethods/";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<VERTICAL_COLLECT_METHODS>(newVColMeth), ParameterType.RequestBody);

                serviceCaller.Execute<VERTICAL_COLLECT_METHODS>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/CollectMethodDelete/1
        public Boolean VCollectMethodDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/VerticalMethods/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<VERTICAL_COLLECT_METHODS>(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Vertical_Collect_Method
        
        #region VerticalDatum
        //need to redirect to get index if error and want to refresh verticalDatum
        public ActionResult VDatum()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the VDatum infoxbox -- get VDatum page or redirect to VDatumCreate
        //GET: /Settings/Lookups/LookupEdits/VDatumEdit/
        [HttpPost]
        public ActionResult VDatum(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("VDatumCreate"); }
                else
                {//edit
                    int VDatumId = Convert.ToInt32(fc["DATUM_ID"]);
                    if (VDatumId == 0) //they didn't choose one before hitting edit
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("VDatumEdit", new { id = VDatumId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //hdatum edit page
        public ActionResult VDatumEdit(int id)
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/VerticalDatums/{entityId}";
                    request.RootElement = "VERTICAL_DATUMS";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    VERTICAL_DATUMS VDType = serviceCaller.Execute<VERTICAL_DATUMS>(request);

                    return View("../Settings/Lookups/LookupEdits/VDatumEdit", VDType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to vdatum, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/VDatumEdit/
        [HttpPost]
        public ActionResult VDatumEdit(int id, VERTICAL_DATUMS anVDatum)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/VerticalDatums/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<VERTICAL_DATUMS>(anVDatum), ParameterType.RequestBody);

                serviceCaller.Execute<VERTICAL_DATUMS>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from VDatum Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/VDatumCreate/
        public ActionResult VDatumCreate()
        {
            try
            {
                ViewData["Role"] = GetMemberLoggedIn();
                if (ViewData["Role"].ToString() == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/VerticalDatums/";
                    request.RootElement = "ArrayOfVERTICAL_DATUMS";
                    List<VERTICAL_DATUMS> VDatums = serviceCaller.Execute<List<VERTICAL_DATUMS>>(request);
                    return View("../Settings/Lookups/LookupCreates/VDatumCreate", VDatums);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new Vdatum was created, post it
        //POST: /Settings/Lookups/LookupCreates/VDatumCreate
        [HttpPost]
        public ActionResult VDatumCreate(VERTICAL_DATUMS newVDatum)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/VerticalDatums";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<VERTICAL_DATUMS>(newVDatum), ParameterType.RequestBody);

                VERTICAL_DATUMS newEV = serviceCaller.Execute<VERTICAL_DATUMS>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/VDatumDelete/1
        public Boolean VDatumDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/VerticalDatums/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<VERTICAL_DATUMS>(request);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion VerticalDatum
        
        //get member logged in to determine if they can add/edit/delete 
        public string GetMemberLoggedIn()
        {
            string role = string.Empty;
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Members?username={userName}";
            request.RootElement = "MEMBER";
            request.AddParameter("userName", User.Identity.Name, ParameterType.UrlSegment);
            MEMBER thisMember = serviceCaller.Execute<MEMBER>(request);
            if (thisMember.ROLE_ID == 1) { role = "Admin"; }
            if (thisMember.ROLE_ID == 2) { role = "Manager"; }
            if (thisMember.ROLE_ID == 3) { role = "Field"; }
            return role;
        }
    }
}
