//------------------------------------------------------------------------------
//----- MembersController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Display a master Members page and link to individual members pages 
//
//discussion:   
//
//     

#region Comments
//04.29.13 - TR - only admin can create/delete
//02.09.13 - TR - Added TeamDetails
//11.21.12 - TR - Added Partial views for Details and Edit pages to be shown on MemberDE page
//10.25.12 - TR - Added Delete
//10.24.12 - TR - Checked for null or zero value in dropdowns
//10.23.12 - TR - Condensed Details code by removing loops and calling just value wanted 
//10.18.12 - TR - Created

#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using System.Xml;

using RestSharp;
using STNServices;
using STNServices.Authentication;
using STNServices.Resources;
using STNWeb.Utilities;
using STNWeb.Models;
using STNWeb.Helpers;
using STNWeb.Providers;

namespace STNWeb.Controllers
{
    [RequireSSL]
    [Authorize]
    public class MembersController : Controller
    {
        //
        // GET: /Settings/Members/
        public ActionResult Index()
        {
            try
            {
                //get the logged in member for authorization
                ViewData["Role"] = GetLoggedInMember();

                List<MemberListingModel> allMemberModel = new List<MemberListingModel>();
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/Members";
                request.RootElement = "ArrayOfMEMBER";
                List<MEMBER> MembersList = serviceCaller.Execute<List<MEMBER>>(request);
                List<MEMBER> SortedMembers = MembersList.OrderBy(x => x.LNAME).ToList();

                //request agencies
                request = new RestRequest();
                request.Resource = "/Agencies";
                request.RootElement = "ArrayOfAGENCY";
                List<AGENCY> theAgencies = serviceCaller.Execute<List<AGENCY>>(request);

                //request roles
                request = new RestRequest();
                request.Resource = "/Roles";
                request.RootElement = "ArrayOfROLE";
                List<ROLE> theRoles = serviceCaller.Execute<List<ROLE>>(request);

                //loop through members to get each member and their agency and role
                foreach (MEMBER mem in SortedMembers)
                {
                    MemberListingModel aMember = new MemberListingModel();

                    aMember.MemberID = mem.MEMBER_ID;
                    aMember.MemberName = mem.LNAME + ", " + mem.FNAME;

                    //loop through agencies to get agencyname that matches mem.agencyid
                    if (mem.AGENCY_ID != null)
                    {
                        AGENCY anAg = theAgencies.FirstOrDefault(b => b.AGENCY_ID == mem.AGENCY_ID);
                        if (anAg != null)
                            aMember.AgencyName = anAg.AGENCY_NAME; 
                    }
                    //loop through roles to get role name that matches mem.roleid
                    if (mem.ROLE_ID != null)
                    {
                        ROLE aRole = theRoles.FirstOrDefault(r => r.ROLE_ID == mem.ROLE_ID);
                        if (aRole != null)
                            aMember.RoleName = aRole.ROLE_NAME; 
                    }

                    allMemberModel.Add(aMember);
                }

                return View("../Settings/Members/Index", allMemberModel);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //Member Details container page holds Partial views for details/edits
        //GET: /Settings/Members/MemberDE/1
        public ActionResult MemberDE(int id)
        {
            try
            {
                //get the logged in member for authorization
                ViewData["Role"] = GetLoggedInMember();

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "MEMBERS/{entityId}";
                request.RootElement = "MEMBER";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);

                MEMBER aMem = serviceCaller.Execute<MEMBER>(request);

                return View("../Settings/Members/MemberDE", aMem);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //Member Details container page holds Partial views for details/edits
        //GET: /Settings/Members/MemberDE/1
        public ActionResult MemberDEbyName(string name)
        {
            try
            {
                //get the logged in member for authorization
                ViewData["Role"] = GetLoggedInMember();

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "/Members?username={userName}";
                request.RootElement = "Member";
                request.AddParameter("userName", User.Identity.Name, ParameterType.UrlSegment);
                MEMBER aMem = serviceCaller.Execute<MEMBER>(request);

                return RedirectToAction("MemberDE", new { id = aMem.MEMBER_ID });
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //Details partial view. rendered from memberDE
        // GET: /Settings/Members/MemberDetails/1
        public PartialViewResult MemberDetailsPV(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "MEMBERS/{entityId}";
                request.RootElement = "MEMBER";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);

                MEMBER aMem = serviceCaller.Execute<MEMBER>(request);

                //get the agencies and roles (if any)
                if (aMem.AGENCY_ID != null && aMem.AGENCY_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/Agencies/{entityId}";
                    request.RootElement = "AGENCY";
                    request.AddParameter("entityId", aMem.AGENCY_ID, ParameterType.UrlSegment);
                    ViewData["Agency"] = serviceCaller.Execute<AGENCY>(request).AGENCY_NAME;
                }

                if (aMem.ROLE_ID != null && aMem.ROLE_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/Roles/{entityId}";
                    request.RootElement = "ROLE";
                    request.AddParameter("entityId", aMem.ROLE_ID, ParameterType.UrlSegment);
                    ViewData["Role"] = serviceCaller.Execute<ROLE>(request).ROLE_NAME;
                }

                return PartialView("../Settings/Members/MemberDetailsPV", aMem);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //Edit partial view. called from memberDE
        //GET: /Settings/Members/MemberEdit/1
        public PartialViewResult MemberEditPV(int id)
        {
            try
            {
                //get the logged in member for authorization
                ViewData["Role"] = GetLoggedInMember();

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "MEMBERS/{entityId}";
                request.RootElement = "MEMBER";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);

                MEMBER aMember = serviceCaller.Execute<MEMBER>(request);

                //get lists of agencies and roles
                request = new RestRequest();
                request.Resource = "/Agencies";
                request.RootElement = "ArrayOfAGENCY";
                List<AGENCY> agList = serviceCaller.Execute<List<AGENCY>>(request);
                agList = agList.OrderBy(x => x.AGENCY_NAME).ToList();
                ViewData["AllAgencies"] = agList;

                if (aMember.ROLE_ID != null && aMember.ROLE_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/Roles/{entityId}";
                    request.RootElement = "ROLE";
                    request.AddParameter("entityId", aMember.ROLE_ID, ParameterType.UrlSegment);
                    ViewData["MemberRole"] = serviceCaller.Execute<ROLE>(request).ROLE_NAME;
                }

                request = new RestRequest();
                request.Resource = "/Roles";
                request.RootElement = "ArrayOfROLE";
                ViewData["AllRoles"] = serviceCaller.Execute<List<ROLE>>(request);

                return PartialView("../Settings/Members/MemberEditPV", aMember);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //Edit partial view. called from memberDE
        //GET: /Settings/Members/MemberEdit/1
        public PartialViewResult MemberPasswordPV(int id)
        {
            try
            {
                //get the logged in member for authorization
                ViewData["Role"] = GetLoggedInMember();

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "MEMBERS/{entityId}";
                request.RootElement = "MEMBER";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);

                MEMBER aMember = serviceCaller.Execute<MEMBER>(request);

                return PartialView("../Settings/Members/MemberPasswordPV", aMember);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //
        //POST: /Settings/Members/MemberEdit/1
        [HttpPost]
        public PartialViewResult MemberPassword(FormCollection fc)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "/Members?username={userName}&new={newPassword}";
                request.RootElement = "MEMBER";
                request.AddParameter("userName", fc["USERNAME"], ParameterType.UrlSegment);
                //request.AddParameter("oldPassword", fc["Old_Password"], ParameterType.UrlSegment);
                request.AddParameter("newPassword", fc["New_Password"], ParameterType.UrlSegment);

                MEMBER updatedMember = new MEMBER();
                updatedMember = serviceCaller.Execute<MEMBER>(request);

                if (updatedMember != null)
                {
                    //populate viewdata info for agency and role              
                    if (updatedMember.AGENCY_ID != null && updatedMember.AGENCY_ID != 0)
                    {
                        request = new RestRequest();
                        request.Resource = "/Agencies/{entityId}";
                        request.RootElement = "AGENCY";
                        request.AddParameter("entityId", updatedMember.AGENCY_ID, ParameterType.UrlSegment);
                        ViewData["Agency"] = serviceCaller.Execute<AGENCY>(request).AGENCY_NAME;
                    }

                    if (updatedMember.ROLE_ID != null && updatedMember.ROLE_ID != 0)
                    {
                        request = new RestRequest();
                        request.Resource = "/Roles/{entityId}";
                        request.RootElement = "ROLE";
                        request.AddParameter("entityId", updatedMember.ROLE_ID, ParameterType.UrlSegment);
                        ViewData["Role"] = serviceCaller.Execute<ROLE>(request).ROLE_NAME;
                    }

                    return PartialView("../Settings/Members/MemberDetailsPV", updatedMember);
                }
                else
                {
                    request = new RestRequest();
                    request.Resource = "/Members?username={userName}";
                    request.RootElement = "MEMBER";
                    request.AddParameter("userName", fc["USERNAME"], ParameterType.UrlSegment);
                    MEMBER goTo = serviceCaller.Execute<MEMBER>(request);
                    ViewData["Error"] = true;
                    return PartialView("..Settings/Members/MemberDetailsPV", goTo); 
                }
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }


        //
        //POST: /Settings/Members/MemberEdit/1
        [HttpPost]
        public PartialViewResult MemberEdit(int id, MEMBER aMember)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/Members/{entityId}/Edit";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(aMember);
                MEMBER updatedMember = serviceCaller.Execute<MEMBER>(request);

                //populate viewdata info for agency and role              
                if (updatedMember.AGENCY_ID != null && updatedMember.AGENCY_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/Agencies/{entityId}";
                    request.RootElement = "AGENCY";
                    request.AddParameter("entityId", updatedMember.AGENCY_ID, ParameterType.UrlSegment);
                    ViewData["Agency"] = serviceCaller.Execute<AGENCY>(request).AGENCY_NAME;
                }

                if (updatedMember.ROLE_ID != null && updatedMember.ROLE_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/Roles/{entityId}";
                    request.RootElement = "ROLE";
                    request.AddParameter("entityId", updatedMember.ROLE_ID, ParameterType.UrlSegment);
                    ViewData["Role"] = serviceCaller.Execute<ROLE>(request).ROLE_NAME;
                }

                return PartialView("../Settings/Members/MemberDetailsPV", updatedMember); 
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Members/MemberCreate
        public ActionResult MemberCreate()
        {
            try
            {
                //get logged in user's role
                ViewData["Role"] = GetLoggedInMember();
                if (ViewData["Role"].ToString() == "Field")
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Member" });
                }
                else
                {
                    //get the agencies and roles to populate dropdowns
                    STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/Agencies";
                    request.RootElement = "ArrayOfAGENCY";
                    List<AGENCY> agList = serviceCaller.Execute<List<AGENCY>>(request);
                    agList = agList.OrderBy(x => x.AGENCY_NAME).ToList();
                    ViewData["AllAgencies"] = agList;

                    request = new RestRequest();
                    request.Resource = "/Roles";
                    request.RootElement = "ArrayOfROLE";
                    ViewData["AllRoles"] = serviceCaller.Execute<List<ROLE>>(request);

                    return View("../Settings/Members/MemberCreate");
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //POST: /Settings/MemberCreate
        [HttpPost]
        public ActionResult MemberCreate(MemberCreateModel fc)
        {
            try
            {
                MEMBER newMember = new MEMBER();
                newMember.FNAME = fc.FName;
                newMember.LNAME = fc.LName;
                newMember.AGENCY_ID = fc.Agency_ID;
                newMember.PHONE = fc.Phone;
                newMember.EMAIL = fc.Email;
                newMember.RSSFEED = fc.RSS_Feed;
                newMember.EMERGENCY_CONTACT_NAME = fc.Emergency_ContactName;
                newMember.EMERGENCY_CONTACT_PHONE = fc.Emergency_ContactPhone;
                newMember.ROLE_ID = fc.Role_ID;
                newMember.USERNAME = fc.UserName;

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "Members/{pass}/addMember";
                request.AddParameter("pass", fc.Password, ParameterType.UrlSegment);
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                STNWebSerializer serializer = new STNWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<MEMBER>(newMember), ParameterType.RequestBody);

                MEMBER createdMember = serviceCaller.Execute<MEMBER>(request);       
                
                return RedirectToAction("MemberDE", new { id = createdMember.MEMBER_ID });
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // GET: /Settings/Members/MemberDelete/1
        public ActionResult MemberDelete(int id)
        {
            try
            {
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "MEMBERS/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<MEMBER>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // GET: /Member username exists
        public string CheckMember(string userName)
        {
            try
            {
                string memberExists = string.Empty;
                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/Members?username={userName}";
                request.RootElement = "MEMBER";
                request.AddParameter("userName", userName, ParameterType.UrlSegment);
                MEMBER aMember = serviceCaller.Execute<MEMBER>(request);
                if (aMember != null)
                { memberExists = aMember.FNAME + " " + aMember.LNAME; }

                return memberExists;
            }
            catch
            {
                return "";
            }
        }
       
        //call for who the member logged in is 
        private string GetLoggedInMember()
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
