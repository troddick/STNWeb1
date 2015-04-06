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
    public class QuickHWMModel
    {
        public SITE Qsite { get; set; }
        public HWM Qhwm { get; set; }
        public OBJECTIVE_POINT Qop { get; set; }
        public OPModel opmod { get; set; }
//        public List<OP_CONTROL_IDENTIFIER> OPIdentifiers { get; set; }
  //      public string IdentifiersIds { get; set; }
    }
}