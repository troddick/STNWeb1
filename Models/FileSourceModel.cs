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
    public class FileSourceModel
    {
        public FILE FSM_File { get; set; }
        public HttpPostedFileBase FileUpload { get; set; }
        public SOURCE FSM_Source { get; set; }
    }

    public class PhotoFileCaption
    {
        [DataType(DataType.Text)]
        [Display(Name = "FileID")]
        public string FileID { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "FileDesc")]
        public string FileDesc { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "SiteDescription")]
        public string SiteDescription { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "County")]
        public string County { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "State")]
        public string State { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Date")]
        public DateTime? Date { get; set; }
    
        [DataType(DataType.Text)]
        [Display(Name = "MemberName")]
        public string MemberName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "MemberAgency")]
        public string MemberAgency { get; set; }
    
    }
   
}