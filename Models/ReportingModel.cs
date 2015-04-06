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
    public class ReportingModel
    {
        public REPORTING_METRICS aReport { get; set; }
        public CONTACT DeplContact { get; set; }
        public CONTACT GenContact { get; set; }
        public CONTACT InlandContact { get; set; }
        public CONTACT CoastContact { get; set; }
        public CONTACT WaterContact { get; set; }
        public ReportMember submitter { get; set; }
        public EVENT anEvent { get; set; }
        public decimal FieldPers_YesterdaySWTotals { get; set; }       
        public decimal FieldPers_YesterdayWQTotals { get; set; }
        public decimal OfficePers_YesterdaySWTotals { get; set; } 
        public decimal OfficePers_YesterdayWQTotals { get; set; }
    }

    public class ReportMember
    {
        [Required]
        public decimal MemberID { get; set; }

        [DataType(DataType.Text)]
        [Display(Name= "Member name")]
        public string MemberName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "email")]
        public string Email { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Agency")]
        public string OfficeName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "OfficeAddress1")]
        public string OfficeAddress1 { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "OfficeAddress2")]
        public string OfficeAddress2 { get; set; }

    }


}