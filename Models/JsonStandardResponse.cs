using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STNWeb.Models
{
    /// <summary>
    /// Note: I am using dynamic objects mainly for efficiency. 
    ///        This way I only send back to he client what I really need.
    /// </summary>
    public class JsonResponseFactory
    {
        public static object ErrorResponse(string error) {
            return new { Success = false, ErrorMessage = error };
        }

        public static object SuccessResponse() {
            return new { Success = true};
        }

        public static object SuccessResponse(object referenceObject)
        {
            return new { Success = true, Object = referenceObject };
        }

    }
}