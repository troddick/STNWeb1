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
    public class InstrumentModel
    {

        public INSTRUMENT Instr { get; set; }
        public INSTRUMENT_STATUS PropInstrStatus { get; set; }
        public INSTRUMENT_STATUS DeplInstrStatus { get; set; }
        public INSTRUMENT_STATUS RetrInstrStatus { get; set; }
        public INSTRUMENT_STATUS LostInstrStatus { get; set; }
        public List<INSTRUMENT_STATUS> InstrStatusList { get; set; }
        public OP_MEASUREMENTS OPMeas { get; set; }
       
    }

    public class InstrSensorType
    {
        [DataType(DataType.Text)]
        [Display(Name = "InstrID")]
        public string InstrID { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "SerialNumber")]
        public string SerialNumber { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "SensorType")]
        public string SensorType { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "DepType")]
        public string DepType { get; set; }
    }

    public class OPMeasModel
    {
        public decimal OPMeasure_ID { get; set; }
        public decimal OP_ID { get; set; }
        public string ObjPointName { get; set; }
        public decimal InstStat_ID { get; set; }
        public string Type { get; set; }
        public decimal? FromRP { get; set; }
        public decimal? HangingLength { get; set; }
        public decimal? WaterSurface { get; set; }
        public decimal? GroundSurface { get; set; }
    }

}