//------------------------------------------------------------------------------
//----- AuthorizeRoleAttribute.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Attribute to allow authorization and role requirement. (i.e. [Authorize("Admin")]
//
//discussion:   
//
//     

#region Comments
// 10.11.12 - JB - Created 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STNWeb.Providers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class AuthorizeRoleAttribute : Attribute
    {
        public void AuthorizeRole(string roleName) {
            //check multiple
            // add "all" option
        }
    }
}

