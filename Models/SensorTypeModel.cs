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
    public class SensorTypeModel
    {
        public SENSOR_TYPE aSensorType { get; set; }
        public string existingSenDepTyps { get; set; }
        public List<decimal> SensorDeploymentTypes { get; set; }
    }
    
}