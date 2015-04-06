//------------------------------------------------------------------------------
//----- STNServiceCaller.cs ----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Helper to call services with REST sharp
//
//   discussion:   
//
//     

#region Comments
// 10.24.12 - JB - Added role tracking
// 08.13.12 - JB - Made a singelton to allow persistant authentication
// 07.20.12 - JB - Created
#endregion

using System;
using System.Configuration;
using System.Collections.Generic;

using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml.Serialization;
using System.IO;

using RestSharp;
using STNServices;
//using STNServices.Authentication;

namespace STNWeb.Utilities
{
    public sealed class STNServiceCaller
    {

        private static readonly STNServiceCaller _instance = new STNServiceCaller();
        public static STNServiceCaller Instance {
            get {
                return _instance;
            }
        }

        private RestClient client = new RestClient();
        public MEMBER CurrentUser;
        public ROLE CurrentRole;
        
        //Singleton
        private STNServiceCaller() {                         
            client.BaseUrl = ConfigurationManager.AppSettings["STNServicesBase"];
        }



        //Sets the base url for the service
        public void setBaseUrl(string baseUrl)
        {
            client.BaseUrl = baseUrl;
        }


        //Sets Username and Password to send with request
        public bool setAuthentication(String username, String password) 
        {
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            //Check login
            RestRequest request = new RestRequest();
            request.Resource = ConfigurationManager.AppSettings["STNServicesLoginEndpoint"];
            request.RootElement = "boolean";
            
            CurrentUser = _instance.Execute<MEMBER>(request);

            if (CurrentUser != null) {

                //get the role
                if ((CurrentUser.ROLE_ID != null) || (CurrentUser.ROLE_ID != 0))
                {
                    request = new RestRequest();
                    request.Resource = "/Roles/{roleId}";
                    request.RootElement = "ROLE";
                    request.AddParameter("roleId", CurrentUser.ROLE_ID, ParameterType.UrlSegment);
                    CurrentRole = _instance.Execute<ROLE>(request);                    
                }

            }
            else
            {
                clearAuthentication();
            }

            return (CurrentUser != null);

        }


        public void clearAuthentication() {
            client.Authenticator = new HttpBasicAuthenticator("", "");
            FormsAuthentication.SignOut();
        }



        //Generic Execute, allows request to be deserialized in any type
        public T Execute<T>(RestRequest request) where T : new()
        {
            RestResponse response = client.Execute(request) as RestResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {                
                if (response.ContentLength > 0)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    using (TextReader reader = new StringReader(response.Content))
                    {
                        return (T)serializer.Deserialize(reader);
                    }
                }                     
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                clearAuthentication();
            }
            
            return default(T);
        }


        //Generic Execute, allows request to be deserialized in any type 
        public T ExecuteExtraTypes<T>(RestRequest request, Type[] extraTypes) where T : new()
        {
            RestResponse response = client.Execute(request) as RestResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T), extraTypes);
                using (TextReader reader = new StringReader(response.Content))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                //clearAuthentication();
            }
            return default(T);
        }

        //download Execute
        public FileStreamResult ExecuteDownload(RestRequest request)
        {
            byte[] newByte;
            RestResponse response = client.Execute(request) as RestResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                
                newByte = response.RawBytes;
                return new FileStreamResult(new MemoryStream(newByte), response.ContentType);
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                //clearAuthentication();
            }

            return null;
        }
            
    }
}