using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using System.ComponentModel.DataAnnotations;

namespace STNWeb.Models
{
    public class FilteredApprovals
    {
        public class hwmApproval
        {
            public string SiteNo { get; set; }
            public decimal HWM_ID { get; set; }
            public decimal? ELEV_FT { get; set; }
        }
        public class dataFileApproval
        {
            public string SiteNo { get; set; }
            public decimal? DF_ID { get; set; }
            public decimal? Inst_ID { get; set; }
            public string SensorType { get; set; }
        }
    }
}