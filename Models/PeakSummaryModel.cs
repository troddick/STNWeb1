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
    public class PeakSummaryModel
    {
        public int peakID { get; set; }
        public int memberID { get; set; }
        public string PeakDate { get; set; }
        public decimal DateEstimated { get; set; }
        public string PeakTime { get; set; }
        public decimal TimeEstimated { get; set; }
        public decimal? PeakStage { get; set; }
        public decimal StageEstimated { get; set; }
        public decimal PeakDischarge { get; set; }
        public decimal DischargeEstimated { get; set; }
        public int VdatumID { get; set; }
        public int HeightAboveGround { get; set; }
        public decimal HAGEstimated { get; set; }
        public string EventName { get; set; }
        public decimal? eventId { get; set; }
        public string PeakHWMDesc { get; set; }
        public string PeakDataFileDesc { get; set; }
        public int Link2Details { get; set; }
        public string TimeZone { get; set; }
        public string From { get; set; }
        public decimal objID { get; set; }
    }
}