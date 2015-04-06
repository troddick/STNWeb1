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
    public class SiteModel
    { 
        public SITE aSite { get; set; }
        public LANDOWNERCONTACT aLandOwner { get; set; }
        public string[] SiteNetworkTypes { get; set; }
        public string[] SiteNetworkNames { get; set; }
        public string[] ProposedSiteSensors { get; set; }
        public List<SiteHousingModel> SiteHousings { get; set; }
        public string houseTypeIDs { get; set; }
    }

    public class SiteCreateModel
    {
        public SITE aSite { get; set; }
        public int[] NetworkTypeIDs { get; set; }
        public int[] NetworkNameIDs { get; set; }
        public int[] AddNetTyes { get; set; }
        public int[] AddNetName { get; set; }
        public int[] RemoveNetTypes { get; set; }
        public int[] RemoveNetNames { get; set; }
    }

    public class SiteHousingModel :SITE_HOUSING
    {
        public string HousingTypeName { get; set; }
        public SiteHousingModel() { }
        public SiteHousingModel(decimal siteHousingID, decimal? siteID, decimal? housingTypeID, string housingTypeName, decimal? length, string material, string notes, decimal? amount) 
        {
            SITE_HOUSING_ID = siteHousingID;
            SITE_ID = siteID;
            HOUSING_TYPE_ID = housingTypeID;
            HousingTypeName = housingTypeName;
            LENGTH = length;
            MATERIAL = material;
            NOTES = notes;
            AMOUNT = amount;
            
        }
        
    }
   
}