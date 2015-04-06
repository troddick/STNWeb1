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
    public class FileDataFileModel
    {
        public FILE FDFM_File { get; set; }
        public HttpPostedFileBase FileUpload { get; set; }
        public DATA_FILE FDFM_DataFile { get; set; }
        public PEAK_SUMMARY FDFM_Peak { get; set; }
    }
}