using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STNWeb.Models
{
    public class reversegeocode
    {
        public string result { get; set; }
        public address addressparts { get; set; }
    }
    public class address
    {
        public string road { get; set; }
        public string city { get; set; }
        public string county { get; set; }
        public string state { get; set; }
        public int postcode { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
    }
}
