//------------------------------------------------------------------------------
//----- EventsController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Display a master Events page and link to individual events pages 
//
//discussion:   
//
//     

#region Comments
// 04.29.13 - TR - Only admin can create and delete events. see who's logged in
// 01.30.13 - TR - Event_Coordinator is list of Members with Manager Role
// 12.11.12 - TR - Partial Views for Details/Edit pages
// 10.24.12 - TR - Delete working now.
// 10.24.12 - TR - Added calls for Event Type and Event Status for dropdowns
// 10.18.12 - TR - Created

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
using STNWeb.Helpers;

using RestSharp;
using STNServices;
using STNServices.Authentication;
using STNServices.Resources;
using STNWeb.Utilities;
using STNWeb.Models;

namespace STNWeb.Controllers
{
    [RequireSSL]
    [Authorize]
    public class EventsController : Controller
    {        
        // List of Events. Can go to Details/Edit or Create new
        // GET: Settings/Events/
        public ActionResult Index()
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                //get member logged in to determine if they can add/edit/delete 
                ViewData["Role"] = GetLoggedInMember();

                //get the list of events
                request = new RestRequest();
                request.Resource = "/Events/";
                request.RootElement = "ArrayOfEvent";
                List<EVENT> anEventList = serviceCaller.Execute<List<EVENT>>(request);

                return View("../Settings/Events/Index", anEventList);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // Container page for Details/Edit Partial Views
        // GET: Settings/Events/EventDetails/1
        public ActionResult EventDetails(int id)
        {
            try
            {
                //get this event
                EVENT anEvent = GetEvent(id);

                //get member logged in to determine if they can add/edit/delete 
                ViewData["Role"] = GetLoggedInMember();

                return View("../Settings/Events/EventDetails", anEvent);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // Partial View for the EventDetails
        // GET: Settings/Events/EventDetailsPV/1
        public PartialViewResult EventDetailsPV(int id)
        {
            try
            {
                EVENT anEvent = GetEvent(id);

                //get EVENT_TYPE.TYPE
                if (anEvent.EVENT_TYPE_ID != null && anEvent.EVENT_TYPE_ID != 0)
                {
                    ViewData["EventType"] = GetEventType(anEvent.EVENT_TYPE_ID);
                }

                //get EVENT_STATUS.STATUS
                if (anEvent.EVENT_STATUS_ID != null && anEvent.EVENT_STATUS_ID != 0)
                {
                    ViewData["EventStatus"] = GetEventStatus(anEvent.EVENT_STATUS_ID);
                }

                //get coordinator username.USERNAME
                if (anEvent.EVENT_COORDINATOR != null && anEvent.EVENT_COORDINATOR != 0)
                {
                    ViewData["MemUsername"] = GetMember(anEvent.EVENT_COORDINATOR);
                }

                return PartialView("../Settings/Events/EventDetailsPV", anEvent);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        // Get the Edit Event Partial View
        //GET: /Settings/Events/EventEditPV/1
        public PartialViewResult EventEditPV(int id)
        {
            try
            {
                string thisRole = GetLoggedInMember();

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                EVENT anEvent = GetEvent(id);

                if (thisRole == "Admin")
                {
                    //get list of EVENT_TYPE
                    request = new RestRequest();
                    request.Resource = "EventTypes/";
                    request.RootElement = "ArrayOfEVENT_TYPE";
                    ViewData["EventType"] = serviceCaller.Execute<List<EVENT_TYPE>>(request);

                    //get list of EVENT_STATUS
                    request = new RestRequest();
                    request.Resource = "/EventStatus/";
                    request.RootElement = "ArrayOfEVENT_STATUS";
                    ViewData["EventStatus"] = serviceCaller.Execute<List<EVENT_STATUS>>(request);

                    //get Manager Members
                    request = new RestRequest();
                    request.Resource = "/MEMBERS/";
                    request.RootElement = "ArrayOfMEMBER";
                    List<MEMBER> MembersList = serviceCaller.Execute<List<MEMBER>>(request);
                    //get the admin members
                    List<MEMBER> AdminMembers = MembersList.Where(m => m.ROLE_ID == 1).ToList();
                    List<MEMBER> SortedAdminList = AdminMembers.OrderBy(m => m.LNAME).ToList();
                    ViewData["ManagerMemberList"] = SortedAdminList;
                }
                else
                {
                    ViewData["Role"] = thisRole;
                }

                return PartialView("../Settings/Events/EventEditPV", anEvent);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        // Post the edit and return to the details PV
        //POST: /Settings/Events/EventEdit/1
        [HttpPost]
        public PartialViewResult EventEdit(int id, EVENT anEvent)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/Events/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<EVENT>(anEvent), ParameterType.RequestBody);

                serviceCaller.Execute<EVENT>(request);

                //populate viewData for DetailsPV
                //get EVENT_TYPE.TYPE
                if (anEvent.EVENT_TYPE_ID != null && anEvent.EVENT_TYPE_ID != 0) 
                {
                    ViewData["EventType"] = GetEventType(anEvent.EVENT_TYPE_ID);
                }
                
                //get EVENT_STATUS.STATUS
                if (anEvent.EVENT_STATUS_ID != null && anEvent.EVENT_STATUS_ID != 0) 
                {
                    ViewData["EventStatus"] = GetEventStatus(anEvent.EVENT_STATUS_ID);
                }

                //get coordinator username
                if (anEvent.EVENT_COORDINATOR != null && anEvent.EVENT_COORDINATOR != 0)
                {
                    ViewData["MemUsername"] = GetMember(anEvent.EVENT_COORDINATOR);
                }

                return PartialView("../Settings/Events/EventDetailsPV", anEvent);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Events/EventCreate
        public ActionResult EventCreate()
        {
            try
            {
                string thisRole = GetLoggedInMember();
                if (thisRole == "Admin")
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();

                    //get list of EVENT_TYPE
                    request.Resource = "/EventTypes/";
                    request.RootElement = "ArrayOfEVENT_TYPE";
                    ViewData["EventType"] = serviceCaller.Execute<List<EVENT_TYPE>>(request);

                    //get list of EVENT_STATUS
                    request = new RestRequest();
                    //get list of eventStatus
                    request.Resource = "/EventStatus/";
                    request.RootElement = "ArrayOfEVENT_STATUS";
                    ViewData["EventStatus"] = serviceCaller.Execute<List<EVENT_STATUS>>(request);

                    //get Manager Members
                    request = new RestRequest();
                    request.Resource = "/MEMBERS/";
                    request.RootElement = "ArrayOfMEMBER";
                    List<MEMBER> MembersList = serviceCaller.Execute<List<MEMBER>>(request);
                    //get the admin members
                    List<MEMBER> SortedAdminList = MembersList.Where(m => m.ROLE_ID == 1).OrderBy(m => m.LNAME).ToList();
                   
                    ViewData["ManagerMemberList"] = SortedAdminList;
                    return View("../Settings/Events/EventCreate");
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Event" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //POST: /Settings/Events/EventCreate
        [HttpPost]
        public ActionResult EventCreate(EVENT newEvent)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/Events/";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<EVENT>(newEvent), ParameterType.RequestBody);

                EVENT createdEvent = serviceCaller.Execute<EVENT>(request);

                return RedirectToAction("EventDetails", new { id = createdEvent.EVENT_ID });
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // GET: /Settings/Events/EventDelete/1
        public ActionResult EventDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/Events/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");

                serviceCaller.Execute<EVENT>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }
        
        //method called several places to request/receive anEvent
        public EVENT GetEvent(int id)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Events/{entityId}";
            request.RootElement = "EVENT";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            EVENT anEvent = serviceCaller.Execute<EVENT>(request);
            return anEvent;
        }

        //method called several places to request/receive anEventType
        public string GetEventType(decimal? id)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/EventTypes/{entityId}";
            request.RootElement = "EVENT_TYPE";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            string EventType = serviceCaller.Execute<EVENT_TYPE>(request).TYPE;
            return EventType;
        }

        //method called several places to request/receive anEventStatus
        public string GetEventStatus(decimal? id)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/EventStatus/{entityId}";
            request.RootElement = "EVENT_STATUS";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            string EventStatus = serviceCaller.Execute<EVENT_STATUS>(request).STATUS;
            return EventStatus;
        }
        //method called several places to request/receive aMember
        public string GetMember(decimal? id)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/MEMBERS/{entityId}";
            request.RootElement = "MEMBER";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            string aMember = serviceCaller.Execute<MEMBER>(request).USERNAME;
            return aMember;
        }

        //call for who the member logged in is 
        public string GetLoggedInMember()
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Members?username={userName}";
            request.RootElement = "Member";
            request.AddParameter("userName", User.Identity.Name, ParameterType.UrlSegment);
            MEMBER thisMember = serviceCaller.Execute<MEMBER>(request);
            int loggedInMember = Convert.ToInt32(thisMember.ROLE_ID);
            string Role = string.Empty;
            switch (loggedInMember)
            {
                case 1: Role = "Admin"; break;
                case 2: Role = "Manager"; break;
                case 3: Role = "Field"; break;
                default: Role = "error"; break;
            }

            return Role;
        }
    }
}
