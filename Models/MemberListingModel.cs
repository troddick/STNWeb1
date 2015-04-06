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
    public class MemberListingModel
    {
        [Required]
        public decimal MemberID { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name= "Member name")]
        public string MemberName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Agency")]
        public string AgencyName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Role")]
        public string RoleName { get; set; }
    }

    public class MemberCreateModel
    {
        public string FName { get; set; }
        public string LName { get; set; }
        public string UserName { get; set; }
        public Int32 Agency_ID { get; set; }
        public Int32 Role_ID { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string RSS_Feed { get; set; }
        public string Emergency_ContactName { get; set; }
        public string Emergency_ContactPhone { get; set; }
        [Required]
        public string Password { get; set; }
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
    
}