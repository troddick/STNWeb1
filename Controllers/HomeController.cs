//------------------------------------------------------------------------------
//----- HomeController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Display master pages
//
//discussion:   
//
//     

#region Comments
// 05.01.13 - TR - Added a NotAuthorized page
// 03.12.13 - TR - added Create Quick HWM 
// 11.16.12 - TR - Changed ShowEventProperties from ActionResult to PartialViewResult, passing in Ajax EVENT_ID (Still need 'thinking' icon)
// 11.07.12 - TR - Created ShowEventProperties and related PartialView containing info related to Event chosen
// 11.05.12 - TR - Change index to show events dropdown with sensors, hwm, and people
// 09.28.12 - JB - Created from old web app
#endregion

using System;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using STNWeb.Utilities;
using RestSharp;

using STNServices;
using STNServices.Resources;
using STNWeb.Models;
using STNWeb.Helpers;

namespace STNWeb.Controllers
{
    [RequireSSL]
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Mapper()
        {
            ViewBag.CurrentPage = "MAP";
            return View();
        }

        //"Go To this site on the mapper" button was clicked from Site details page
        // GET: Mapper/ zoomed to this site
        public ActionResult ZoomToOnMap(int id)
        {
            ViewData["SiteId"] = id;
            return View("Mapper");
        }
        
        // Gets safety info: Current Event: 
        // GET: /Safety/
        public ActionResult Index(string returnUrl)
        {
            try
            {
                ViewBag.CurrentPage = "HOME";
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                //get list of events
                request = new RestRequest();
                request.Resource = "/Events/";
                request.RootElement = "ArrayOfEvent";
                List<EVENT> eventList = serviceCaller.Execute<List<EVENT>>(request);
                ViewData["EventList"] = eventList;

                if (returnUrl != null)
                    ViewData["returnUrl"] = returnUrl;
                
                return View();
            }

            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }

           
        }

        #region Collection Team

        //TeamDetails
        //GET: /CollectTeam/TeamDetails/1
        public ActionResult TeamDetails(int id)
        {
            try
            {
                //get the members on this team
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/CollectionTeams/{teamId}/Members";
                request.RootElement = "ArrayOfMEMBER";
                request.AddParameter("teamId", id, ParameterType.UrlSegment);
                List<MEMBER> MemberTeam = serviceCaller.Execute<List<MEMBER>>(request);

                int i = 0;
                foreach (MEMBER memt in MemberTeam)
                {
                    if (memt.AGENCY_ID != null && memt.AGENCY_ID != 0)
                    {
                        request = new RestRequest();
                        request.Resource = "/Agencies/{entityId}";
                        request.RootElement = "AGENCY";
                        request.AddParameter("entityId", memt.AGENCY_ID, ParameterType.UrlSegment);
                        string ag = "ag_" + i.ToString();
                        ViewData[ag] = serviceCaller.Execute<AGENCY>(request).AGENCY_NAME;
                    }
                    if (memt.ROLE_ID != null && memt.ROLE_ID != 0)
                    {
                        request = new RestRequest();
                        request.Resource = "/Roles/{entityId}";
                        request.RootElement = "ROLE";
                        request.AddParameter("entityId", memt.ROLE_ID, ParameterType.UrlSegment);
                        string ro = "ro_" + i.ToString();
                        ViewData[ro] = serviceCaller.Execute<ROLE>(request).ROLE_NAME;
                    }
                    i++;
                }
                if (Request.UrlReferrer != null)
                    ViewData["RedirectURL"] = Request.UrlReferrer.ToString();

                return View("CollectTeam/TeamDetails", MemberTeam);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //CreateCollectTeam
        //GET: /Settings/Members/CreateCollectTeam
        public ActionResult CreateCollectTeam(string rdURL)
        {

            if (!string.IsNullOrEmpty(rdURL))
                ViewData["RedirectURL"] = rdURL;
            return View("CollectTeam/CreateCollectTeam");
        }

        //POST: CreateCollectTeam
        [HttpPost]
        public ActionResult CreateCollectTeam(FormCollection thisFC)
        {
            try
            {
                //create collectTeam
                COLLECT_TEAM aColTeam = new COLLECT_TEAM();
                aColTeam.DESCRIPTION = thisFC["TeamName"];
                
                //make sure there's a description - was getting empty ones due to log off issue

                if (string.IsNullOrEmpty(aColTeam.DESCRIPTION))
                   aColTeam.DESCRIPTION = "Left Empty";
                    
                //create the collection team first
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/CollectionTeams";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<COLLECT_TEAM>(aColTeam), ParameterType.RequestBody);
                COLLECT_TEAM newTeam = serviceCaller.Execute<COLLECT_TEAM>(request);

                Session["TeamId"] = newTeam.COLLECT_TEAM_ID;
                Session["TeamName"] = newTeam.DESCRIPTION;

                //then add the members.
                MEMBER member1 = new MEMBER();
                MEMBER member2 = new MEMBER();
                List<MEMBER> theseMembers = new List<MEMBER>();

                string m1Username = thisFC["thisUserName"];
                string m2Username = thisFC["WhoRUwith"];

                if (m1Username != "")
                {
                    request = new RestRequest();
                    request.Resource = "/Members?username={userName}";
                    request.RootElement = "MEMBER";
                    request.AddParameter("userName", m1Username, ParameterType.UrlSegment);
                    member1 = serviceCaller.Execute<MEMBER>(request);
                    theseMembers.Add(member1);
                }

                if (m2Username != "")
                {
                    request = new RestRequest();
                    request.Resource = "/Members?username={userName}";
                    request.RootElement = "MEMBER";
                    request.AddParameter("userName", m2Username, ParameterType.UrlSegment);
                    member2 = serviceCaller.Execute<MEMBER>(request);
                    theseMembers.Add(member2);
                }
                 
                List<MEMBER> listMembers = new List<MEMBER>();

                foreach (MEMBER aMember in theseMembers)
                {
                    request = new RestRequest();
                    request.Method = Method.POST;
                    request.Resource = "/CollectionTeams/{teamId}/AddMember";
                    request.AddParameter("teamId", newTeam.COLLECT_TEAM_ID, ParameterType.UrlSegment);
                    request.RequestFormat = DataFormat.Xml;
                    request.AddHeader("Content-Type", "application/xml");
                    request.AddParameter("application/xml", request.XmlSerializer.Serialize(aMember), ParameterType.RequestBody);
                    listMembers = serviceCaller.Execute<List<MEMBER>>(request);
                }
                string rdURL = thisFC["RedirectURL"];

                return RedirectToAction("Index", new { returnUrl = rdURL });
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        public PartialViewResult CollectionTeams(int id, string RDurl)
        {
            try
            {
                //"/Events/{eventId}/Members" --for each member, get memberTeam , then get collect team = collect teams for this event
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/EVENTS/{eventId}/TEAMS";
                request.RootElement = "ArrayOfCOLLECT_TEAM";
                request.AddParameter("eventId", id, ParameterType.UrlSegment);
                List<COLLECT_TEAM> teams = serviceCaller.Execute<List<COLLECT_TEAM>>(request);
                if (teams.Count != 0 && teams != null)
                { ViewData["TESTCollTeam"] = teams.OrderBy(x => x.DESCRIPTION).ToList(); }
                if (!string.IsNullOrEmpty(RDurl))
                    ViewData["RedirectURL"] = RDurl;

                return PartialView("CollectTeam/TeamsPV");
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        public JsonResult SetCollectTeam(int teamId)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/CollectionTeams/{entityId}";
                request.RootElement = "COLLECT_TEAM";
                request.AddParameter("entityId", teamId, ParameterType.UrlSegment);
                COLLECT_TEAM ct = serviceCaller.Execute<COLLECT_TEAM>(request);

                Session["TeamId"] = ct.COLLECT_TEAM_ID;
                Session["TeamName"] = ct.DESCRIPTION;
                return Json(ct.DESCRIPTION, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(e.ToString(),JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Collection Team

        //partial view showing people, hwms and sensors for this event
        //GET: /HomeController/ShowEventProperties/{id}
        public PartialViewResult ShowEventProperties(int evId)
        {

            Session["EventId"] = evId;
            ViewData["EventID"] = evId;
            try
            {
                if (evId != 0)
                {
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/Events/{entityId}";
                    request.RootElement = "EVENT";
                    request.AddParameter("entityId", evId, ParameterType.UrlSegment);
                    EVENT anEvent = serviceCaller.Execute<EVENT>(request);
                    Session["EventName"] = anEvent.EVENT_NAME;

                    //sensors
                    request = new RestRequest();
                    request.Resource = "/Instruments?Event={EventId}&CurrentStatus=1";
                    request.RootElement = "ArrayOfINSTRUMENT";
                    request.AddParameter("EventId", evId, ParameterType.UrlSegment);
                    List<INSTRUMENT> DeployedInstruments = serviceCaller.Execute<List<INSTRUMENT>>(request);

                    request = new RestRequest();
                    request.Resource = "/Instruments?Event={EventId}&CurrentStatus=2";
                    request.RootElement = "ArrayOfINSTRUMENT";
                    request.AddParameter("EventId", evId, ParameterType.UrlSegment);
                    List<INSTRUMENT> RetrievedInstruments = serviceCaller.Execute<List<INSTRUMENT>>(request);

                    request = new RestRequest();
                    request.Resource = "/Instruments?Event={EventId}&CurrentStatus=3";
                    request.RootElement = "ArrayOfINSTRUMENT";
                    request.AddParameter("EventId", evId, ParameterType.UrlSegment);
                    List<INSTRUMENT> LostInstruments = serviceCaller.Execute<List<INSTRUMENT>>(request);


                    ViewData["TotSensors"] = DeployedInstruments != null ? DeployedInstruments.Count + RetrievedInstruments.Count + LostInstruments.Count : 0;
                    ViewData["Deployed"] = DeployedInstruments != null ? DeployedInstruments.Count : 0;
                    ViewData["Retrieved"] = RetrievedInstruments != null ? RetrievedInstruments.Count : 0;
                    ViewData["Lost"] = LostInstruments != null ? LostInstruments.Count: 0;

                    //hwms
                    request = new RestRequest();
                    request.Resource = "Events/{eventId}/HWMs";
                    request.RootElement = "ArrayOfHWM";
                    request.AddParameter("eventId", evId, ParameterType.UrlSegment);
                    List<HWM> HWMsList = serviceCaller.Execute<List<HWM>>(request);

                    int totalHWM = HWMsList != null ? HWMsList.Count : 0;
                    //add hwm info to viewData
                    ViewData["TotHWM"] = totalHWM;
                    ////end hwms

                    //people            
                    request = new RestRequest();
                    request.Resource = "/Events/{eventId}/Members";
                    request.RootElement = "ArrayOfMEMBER";
                    request.AddParameter("eventId", evId, ParameterType.UrlSegment);
                    List<MEMBER> memberList = serviceCaller.Execute<List<MEMBER>>(request);

                    ViewData["TotPeople"] = memberList != null ? memberList.Count : 0;
                    //end teams/people
                }
                return PartialView("ShowEventProperties");
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //get sent here from other places around the app when the user is not authorized to perform action (such as create or edit)
        public ActionResult NotAuthorized(string from)
        {
            //from = "MemberCreate"
            //from = "Event"
            //from = "Lookups"

            return View();
        }
    }
}
