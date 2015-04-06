//------------------------------------------------------------------------------
//----- SettingsController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Display a master settings page and link to individual setting pages 
//
//discussion:   
//
//     

#region Comments
// 10.19.12 - TR - Moved Lookups to own controller
// 10.18.12 - TR - Moved Members and Events to their own controllers 
// 10.18.12 - TR - Events Edit and Create working. Working on Members Edit and Create
// 10.17.12 - TR - Converted remaining Lookup Edits to use EntityObject for Serialization
// 10.17.12 - JB - Converted EVENT_TYPE PUT to use EntityObject for Serialization
// 10.12.12 - TR - Created Memeber view, edit, and create pages
// 10.09.12 - TR - Lookup Edits and Creates are visible with correct info
// 10.04.12 - TR - Working on Lookup Edit pages
// 10.03.12 - TR - Completed Lookup, working on LookupEdit pages
// 10.02.12 - TR - Working on Lookups
// 10.02.12 - TR - Worked on EventCreate(newEvent). Incomplete
// 10.02.12 - TR - Created EventEdit(int id)
// 10.01.12 - TR - Created EventDetails(int id)
// 10.01.12 - TR - Created Events()
// 09.28.12 - JB - Created from old web app
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
    public class SettingsController : Controller
    {
        //
        // GET: /Settings/

        public ActionResult Index()
        {
            ViewBag.CurrentPage = "SETTINGS";
            return View();
        }


    }
}
