using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using System.ComponentModel.DataAnnotations;
using STNServices;

namespace STNWeb.Models
{
    public class OPModel :OBJECTIVE_POINT
    {
        public List<OP_CONTROL_IDENTIFIER> OPIdentifiers { get; set; }
        public string IdentifiersIds { get; set; }
        public string IdentifiersToRemove { get; set; }
    }   
}