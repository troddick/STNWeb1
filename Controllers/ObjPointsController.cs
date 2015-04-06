//------------------------------------------------------------------------------
//----- ObjPointsController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Display a master Objective Points page and link to individual Objective Point pages 
//
//discussion:   
//
//     

#region Comments
 //05.14.14 - TR - Changed to OBJECTIVE_POINT
 //04.29.14 - TR - added Collect_Method
 //11.30.12 - TR - partial views for details and edit pages
 //11.29.12 - TR - added partial view for info box of ref points on sites and hwm detail pages 
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
    public class ObjPointsController : Controller
    {
         //
        // GET: /ObjPointInfoBox/
        public PartialViewResult ObjPointInfoBox(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "Sites/{siteId}/ObjectivePoints";
                request.RootElement = "ArrayOfObjectivePoints";
                request.AddParameter("siteId", id, ParameterType.UrlSegment);
                ViewData["objectivePointList"] = serviceCaller.Execute<List<OBJECTIVE_POINT>>(request);
                ViewData["SiteId"] = id;

                return PartialView();
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //
        // GET: /ObjPointsDetails/1
        public ActionResult ObjPointDetails(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "/ObjectivePoints/{entityId}";
                request.RootElement = "OBJECTIVE_POINT";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                OBJECTIVE_POINT anObjPoint = serviceCaller.Execute<OBJECTIVE_POINT>(request);

                //get any opMeasurements that have this op attached
                request = new RestRequest();
                request.Resource = "ObjectivePoints/{objectivePointId}/OPMeasurements";
                request.RootElement = "ArrayOfOP_MEASUREMENTS";
                request.AddParameter("objectivePointId", id, ParameterType.UrlSegment);
                List<OP_MEASUREMENTS> OPopMeasList = serviceCaller.Execute<List<OP_MEASUREMENTS>>(request);

                if (OPopMeasList.Count >= 1)
                    ViewData["OPMeas_Linked"] = "true";

                request = new RestRequest();
                request.Resource = "Sites/{entityId}";
                request.RootElement = "SITE";
                request.AddParameter("entityId", anObjPoint.SITE_ID, ParameterType.UrlSegment);
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

                return View("../ObjPoints/ObjPointDetails", anObjPoint);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        // GET: /ObjPointDetailsPV/1
        public PartialViewResult ObjPointDetailsPV(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "/ObjectivePoints/{entityId}";
                request.RootElement = "OBJECTIVE_POINT";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                OBJECTIVE_POINT anObjPoint = serviceCaller.Execute<OBJECTIVE_POINT>(request);

                //make sure latitude is only 6 dec places
                if (anObjPoint.LATITUDE_DD.HasValue)
                {
                    anObjPoint.LATITUDE_DD = Math.Round(anObjPoint.LATITUDE_DD.Value, 6);
                }

                //get site number
                request = new RestRequest();
                request.Resource = "Sites/{entityId}";
                request.RootElement = "SITE";
                request.AddParameter("entityId", anObjPoint.SITE_ID, ParameterType.UrlSegment);
                ViewData["SiteNo"] = serviceCaller.Execute<SITE>(request).SITE_NO;

                //get VDATUM_ID
                if (anObjPoint.VDATUM_ID != null && anObjPoint.VDATUM_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/VerticalDatums/{entityId}";
                    request.RootElement = "VERTICAL_DATUMS";
                    request.AddParameter("entityId", anObjPoint.VDATUM_ID, ParameterType.UrlSegment);
                    ViewData["aVerticalDatum"] = serviceCaller.Execute<VERTICAL_DATUMS>(request).DATUM_NAME;
                }

                //Get OP Types
                if (anObjPoint.OP_TYPE_ID != null && anObjPoint.OP_TYPE_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/OPTypes/{entityId}";
                    request.RootElement = "OBJECTIVE_POINT_TYPE";
                    request.AddParameter("entityId", anObjPoint.OP_TYPE_ID, ParameterType.UrlSegment);
                    ViewData["OPType"] = serviceCaller.Execute<OBJECTIVE_POINT_TYPE>(request).OP_TYPE;
                }

                //get HCollection Method 
                if (anObjPoint.HCOLLECT_METHOD_ID != null && anObjPoint.HCOLLECT_METHOD_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/HorizontalMethods/{entityId}";
                    request.RootElement = "HORIZONTAL_COLLECT_METHODS";
                    request.AddParameter("entityId", anObjPoint.HCOLLECT_METHOD_ID, ParameterType.UrlSegment);
                    ViewData["aHColMethod"] = serviceCaller.Execute<HORIZONTAL_COLLECT_METHODS>(request).HCOLLECT_METHOD;
                }

                //get HDatumID
                if (anObjPoint.HDATUM_ID != null && anObjPoint.HDATUM_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/HorizontalDatums/{entityId}";
                    request.RootElement = "HORIZONTAL_DATUMS";
                    request.AddParameter("entityId", anObjPoint.HDATUM_ID, ParameterType.UrlSegment);
                    ViewData["aHorizontalDatum"] = serviceCaller.Execute<HORIZONTAL_DATUMS>(request).DATUM_NAME;
                }

                //get collectionMethod
                if (anObjPoint.VCOLLECT_METHOD_ID != null && anObjPoint.VCOLLECT_METHOD_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/VerticalMethods/{entityId}";
                    request.RootElement = "VERTICAL_COLLECT_METHODS";
                    request.AddParameter("entityId", anObjPoint.VCOLLECT_METHOD_ID, ParameterType.UrlSegment);
                    VERTICAL_COLLECT_METHODS thisVCollMtd = serviceCaller.Execute<VERTICAL_COLLECT_METHODS>(request);
                    if (thisVCollMtd != null)
                    { ViewData["VCollectMethod"] = thisVCollMtd.VCOLLECT_METHOD; }
                }

                //get OP_Control Identifiers (if any)
                request = new RestRequest();
                request.Resource = "ObjectivePoints/{objectivePointId}/OPControls";
                request.RootElement = "ArrayOfOP_CONTROL_IDENTIFIER";
                request.AddParameter("objectivePointId", anObjPoint.OBJECTIVE_POINT_ID, ParameterType.UrlSegment);
                List<OP_CONTROL_IDENTIFIER> controlIds = serviceCaller.Execute<List<OP_CONTROL_IDENTIFIER>>(request);
                if (controlIds != null && controlIds.Count >= 1)
                    ViewData["ControlIDs"] = controlIds;

                //get quality
                request = new RestRequest();
                request.Resource = "/ObjectivePoints/{objectivePointId}/Quality";
                request.RootElement = "OP_QUALITY";
                request.AddParameter("objectivePointId", anObjPoint.OBJECTIVE_POINT_ID, ParameterType.UrlSegment);
                OP_QUALITY thisOPQuality = serviceCaller.Execute<OP_QUALITY>(request);
                if (thisOPQuality != null)
                    ViewData["thisQuality"] = thisOPQuality.QUALITY;

                return PartialView("ObjPointDetailsPV", anObjPoint);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //
        // GET: /ObjPointEditPV/1
        public PartialViewResult ObjPointEditPV(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "/ObjectivePoints/{entityId}";
                request.RootElement = "OBJECTIVE_POINT";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                OBJECTIVE_POINT anObjPt = serviceCaller.Execute<OBJECTIVE_POINT>(request);

                //make sure latitude is only 6 dec places
                if (anObjPt.LATITUDE_DD.HasValue)
                {
                    anObjPt.LATITUDE_DD = Math.Round(anObjPt.LATITUDE_DD.Value, 6);
                }

                //get OP_Control Identifiers (if any)
                request = new RestRequest();
                request.Resource = "ObjectivePoints/{objectivePointId}/OPControls";
                request.RootElement = "ArrayOfOP_CONTROL_IDENTIFIER";
                request.AddParameter("objectivePointId", anObjPt.OBJECTIVE_POINT_ID, ParameterType.UrlSegment);
                List<OP_CONTROL_IDENTIFIER> controlIds = serviceCaller.Execute<List<OP_CONTROL_IDENTIFIER>>(request);
                if (controlIds != null && controlIds.Count >= 1)
                    ViewData["ControlIDs"] = controlIds;

                //get VDATUM_ID
                request = new RestRequest();
                request.Resource = "/VerticalDatums";
                request.RootElement = "ArrayOfVERTICAL_DATUMS";
                List<VERTICAL_DATUMS> vDatums = serviceCaller.Execute<List<VERTICAL_DATUMS>>(request);
                ViewData["VerticalDatum"] = vDatums;

                //Get Horizontal Collection Methods
                request = new RestRequest();
                request.Resource = "/HorizontalMethods";
                request.RootElement = "ArrayOfHORIZONTAL_COLLECT_METHODS";
                ViewData["hColMethodList"] = serviceCaller.Execute<List<HORIZONTAL_COLLECT_METHODS>>(request);

                //get hDatumID
                request = new RestRequest();
                request.Resource = "/HorizontalDatums";
                request.RootElement = "ArrayOfHORIZONTAL_DATUMS";
                List<HORIZONTAL_DATUMS> hDatums = serviceCaller.Execute<List<HORIZONTAL_DATUMS>>(request);
                ViewData["HorizontalDatum"] = hDatums;

                //Get OP Types
                request = new RestRequest();
                request.Resource = "/OPTypes";
                request.RootElement = "ArrayOfOBJECTIVE_POINT_TYPE";
                ViewData["OPTypes"] = serviceCaller.Execute<List<OBJECTIVE_POINT_TYPE>>(request);

                //get qualities
                request = new RestRequest();
                request.Resource = "/ObjectivePointQualities";
                request.RootElement = "ArrayOfOP_QUALITY";
                ViewData["Qualities"] = serviceCaller.Execute<List<OP_QUALITY>>(request);

                //Get collection method choices
                request = new RestRequest();
                request.Resource = "/VerticalMethods";
                request.RootElement = "ArrayOfVERTICAL_COLLECT_METHODS";
                ViewData["VcollectMethodList"] = serviceCaller.Execute<List<VERTICAL_COLLECT_METHODS>>(request);

                //get siteNumber 
                request = new RestRequest();
                request.Resource = "Sites/{entityId}";
                request.RootElement = "SITE";
                request.AddParameter("entityId", anObjPt.SITE_ID, ParameterType.UrlSegment);
                ViewData["aSiteNum"] = serviceCaller.Execute<SITE>(request).SITE_NO;

                return PartialView("ObjPointEditPV", anObjPt);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //
        //POST: /ObjPointEdit/1
        [HttpPost]
        public ActionResult ObjPointEdit(int id, OPModel anObjPtM)
        {
            try
            {
                OBJECTIVE_POINT editedOP = BuildOP(anObjPtM);

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/ObjectivePoints/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<OBJECTIVE_POINT>(editedOP), ParameterType.RequestBody);
                OBJECTIVE_POINT anObjPoint = serviceCaller.Execute<OBJECTIVE_POINT>(request);

                if (anObjPtM.OPIdentifiers != null)
                {                    
                    foreach (OP_CONTROL_IDENTIFIER opi in anObjPtM.OPIdentifiers)
                    {
                        //if it has an id, it's a PUT
                        if (opi.OP_CONTROL_IDENTIFIER_ID == 0)
                        {
                            //POST it
                            request = new RestRequest(Method.POST);
                            request.Resource = "ObjectivePoints/{objectivePointId}/AddOPControls";
                            request.AddParameter("objectivePointId", editedOP.OBJECTIVE_POINT_ID, ParameterType.UrlSegment);
                            request.RequestFormat = DataFormat.Xml;
                            request.AddHeader("Content-Type", "application/xml");
                            serializer = new STNWebSerializer();
                            request.AddParameter("application/xml", serializer.Serialize<OP_CONTROL_IDENTIFIER>(opi), ParameterType.RequestBody);
                            serviceCaller.Execute<OP_CONTROL_IDENTIFIER>(request);
                        }
                        else
                        {
                            //PUT it
                            opi.OBJECTIVE_POINT_ID = editedOP.OBJECTIVE_POINT_ID;
                            request = new RestRequest(Method.POST);
                            request.Resource = "/OPControlIdentifiers/{entityId}";
                            request.RequestFormat = DataFormat.Xml;
                            request.AddParameter("entityId", opi.OP_CONTROL_IDENTIFIER_ID, ParameterType.UrlSegment);
                            request.AddHeader("X-HTTP-Method-Override", "PUT");
                            request.AddHeader("Content-Type", "application/xml");
                            //Use extended serializer
                            serializer = new STNWebSerializer();
                            request.AddParameter("application/xml", serializer.Serialize<OP_CONTROL_IDENTIFIER>(opi), ParameterType.RequestBody);
                            serviceCaller.Execute<OP_CONTROL_IDENTIFIER>(request);
                        }
                    }
//                    List<decimal> identifiersExisted = anObjPtM.IdentifiersIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToDecimal(x)).ToList();
                }

                //remove these
                if (!string.IsNullOrEmpty(anObjPtM.IdentifiersToRemove))
                {
                    List<decimal> identifiersRemoving = anObjPtM.IdentifiersToRemove.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToDecimal(x)).ToList();
                    foreach (decimal identifierID in identifiersRemoving)
                    {
                        //DELETE
                        request = new RestRequest(Method.POST);
                        request.Resource = "/OPControlIdentifiers/{entityId}";
                        request.AddParameter("entityId", identifierID, ParameterType.UrlSegment);
                        request.AddHeader("X-HTTP-Method-Override", "DELETE");
                        request.AddHeader("Content-Type", "application/xml");

                        serviceCaller.Execute<OP_CONTROL_IDENTIFIER>(request);
                    }
                }

                return RedirectToAction("ObjPointDetailsPV", new { id = editedOP.OBJECTIVE_POINT_ID });

            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /ObjPointCreate
        public ActionResult ObjPointCreate(int siteID)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //get VDatum for dropdown
                request.Resource = "/VerticalDatums";
                request.RootElement = "ArrayOfVERTICAL_DATUMS";
                List<VERTICAL_DATUMS> vDatums = serviceCaller.Execute<List<VERTICAL_DATUMS>>(request);
                ViewData["VerticalDatum"] = vDatums;

                //Get Horizontal Collection Methods
                request = new RestRequest();
                request.Resource = "/HorizontalMethods";
                request.RootElement = "ArrayOfHORIZONTAL_COLLECT_METHODS";
                ViewData["hColMethodList"] = serviceCaller.Execute<List<HORIZONTAL_COLLECT_METHODS>>(request);

                //get HDatum for dropdown
                request = new RestRequest();
                request.Resource = "/HorizontalDatums";
                request.RootElement = "ArrayOfHORIZONTAL_DATUMS";
                List<HORIZONTAL_DATUMS> hDatums = serviceCaller.Execute<List<HORIZONTAL_DATUMS>>(request);
                ViewData["HorizontalDatum"] = hDatums;

                //Get collection method choices
                request = new RestRequest();
                request.Resource = "/VerticalMethods";
                request.RootElement = "ArrayOfVERTICAL_COLLECT_METHODS";
                ViewData["VcollectMethodList"] = serviceCaller.Execute<List<VERTICAL_COLLECT_METHODS>>(request);

                //get qualities
                request = new RestRequest();
                request.Resource = "/ObjectivePointQualities";
                request.RootElement = "ArrayOfOP_QUALITY";
                ViewData["Qualities"] = serviceCaller.Execute<List<OP_QUALITY>>(request);

                //Get OP Types
                request = new RestRequest();
                request.Resource = "/OPTypes";
                request.RootElement = "ArrayOfOBJECTIVE_POINT_TYPE";
                ViewData["OPTypes"] = serviceCaller.Execute<List<OBJECTIVE_POINT_TYPE>>(request);

                //get the site
                request = new RestRequest();
                request.Resource = "Sites/{entityId}";
                request.RootElement = "SITE";
                request.AddParameter("entityId", siteID, ParameterType.UrlSegment);
                ViewData["aSite"] = serviceCaller.Execute<SITE>(request);

                return View();
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //POST: /ObjPointCreate
        [HttpPost]
        public ActionResult ObjPointCreate(OPModel newObjPt, string Create)
        {
            try
            {
                OBJECTIVE_POINT newOP = BuildOP(newObjPt);

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/ObjectivePoints";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                
                //use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<OBJECTIVE_POINT>(newOP), ParameterType.RequestBody);
                OBJECTIVE_POINT createdOP = serviceCaller.Execute<OBJECTIVE_POINT>(request);
                
                //post ControlIdentifiers if some were chosen
                if (newObjPt.OPIdentifiers != null) 
                {
                    if (newObjPt.OPIdentifiers.Count >= 1)
                    {
                        foreach (OP_CONTROL_IDENTIFIER opCI in newObjPt.OPIdentifiers)
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
                string siteId = newObjPt.SITE_ID.ToString();
                //determine which submit button was selected
                if (Create == "Submit &\r\nAdd Another OP") //Submit & Add Another OP
                {
                    return RedirectToAction("ObjPointCreate", "ObjPoints", new { siteID = siteId });
                }
                else if (Create == "Submit &\r\nAdd Upload File")
                {
                    return RedirectToAction("FileCreate", "Files", new { Id = createdOP.OBJECTIVE_POINT_ID, aPage = "OP" });
                }
                else if (Create == "Submit &\r\nDeploy Sensor") // Submit & Deploy Sensor
                {
                    return RedirectToAction("Create", "Instruments", new { siteID = siteId });
                }
                else if (Create == "Submit\r\n& Add HWM") //Submit & Add HWM
                {
                    return RedirectToAction("Create", "HWMs", new { siteID = siteId });
                }
                else //just Submit
                {
                    return RedirectToAction("Details", "Sites", new { id = siteId });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        private OBJECTIVE_POINT BuildOP(OPModel newObjPt)
        {
            OBJECTIVE_POINT newOP = new OBJECTIVE_POINT();

            if (newObjPt.OBJECTIVE_POINT_ID > 0)
                newOP.OBJECTIVE_POINT_ID = newObjPt.OBJECTIVE_POINT_ID;

            newOP.NAME = newObjPt.NAME;
            newOP.DESCRIPTION = newObjPt.DESCRIPTION;
            if (newObjPt.ELEV_FT.HasValue)
            {
                var radio = Convert.ToString(Request.Form["ElevationUnit"]);
                //elevation is stored in ft, do conversion if == meter
                if (radio == "meter")
                {
                    decimal converted = Convert.ToDecimal(newObjPt.ELEV_FT) * 3.2808M;
                    newOP.ELEV_FT = Math.Round(converted, 3);
                }
                else
                    newOP.ELEV_FT = newObjPt.ELEV_FT;
            }
            newOP.DATE_ESTABLISHED = newObjPt.DATE_ESTABLISHED;
            newOP.OP_IS_DESTROYED = newObjPt.OP_IS_DESTROYED == null ? 0 : newObjPt.OP_IS_DESTROYED;
            newOP.OP_NOTES = newObjPt.OP_NOTES;
            newOP.SITE_ID = newObjPt.SITE_ID;
            newOP.VDATUM_ID = newObjPt.VDATUM_ID;
            newOP.LATITUDE_DD = newObjPt.LATITUDE_DD;
            //make sure it's neg
            newOP.LONGITUDE_DD = newObjPt.LONGITUDE_DD.Value < 0 ? newObjPt.LONGITUDE_DD.Value : newObjPt.LONGITUDE_DD.Value * (-1);
            //and that it's only 6 dec places
            newOP.LONGITUDE_DD = Math.Round(newOP.LONGITUDE_DD.Value, 6);
            newOP.HDATUM_ID = newObjPt.HDATUM_ID;
            newOP.HCOLLECT_METHOD_ID = newObjPt.HCOLLECT_METHOD_ID;
            newOP.VCOLLECT_METHOD_ID = newObjPt.VCOLLECT_METHOD_ID;
            newOP.OP_TYPE_ID = newObjPt.OP_TYPE_ID;
            newOP.DATE_RECOVERED = newObjPt.DATE_RECOVERED;
            if (newObjPt.UNCERTAINTY > 0)
            {
                var rad = Convert.ToString(Request.Form["UncertainUnit"]);
                if (rad == "cm")
                {
                    decimal convertedV = Convert.ToDecimal(newObjPt.UNCERTAINTY) / 30.48M;
                    newOP.UNCERTAINTY = Math.Round(convertedV, 3);
                }
                else
                    newOP.UNCERTAINTY = newObjPt.UNCERTAINTY;
            }

            newOP.UNQUANTIFIED = newObjPt.UNQUANTIFIED;
            newOP.OP_QUALITY_ID = newObjPt.OP_QUALITY_ID;

            return newOP;
        }
        
        //GET
        //GET: /ObjPointDelete/11
        public ActionResult ObjPointDelete(int id, int siteID)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/ObjectivePoints/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");

                serviceCaller.Execute<OBJECTIVE_POINT>(request);

                return RedirectToAction("Details", "Sites", new { id = siteID });
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }


    }
}
