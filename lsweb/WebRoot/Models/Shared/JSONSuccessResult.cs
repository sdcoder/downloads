using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Models.Shared
{
    /// <summary>
    /// Wrapper to return JSON results in a format with a "Success" parameter - so response can be checked as:
    /// if (result && result.Success)
    /// </summary>
    public class JSONSuccessResult : JsonResult
    {
        public JSONSuccessResult()
        {
            Data = new
            {
                Success = true
            };
        }
        public JSONSuccessResult(bool success)
        {
            Data = new
            {
                Success = success
            };
        }
        public JSONSuccessResult(bool success, string errorMessage, bool isContactInformationConflict = false)
        {
            Data = new
            {
                Success = success,
                ErrorMessage = errorMessage,
                ContactInformationConflict = isContactInformationConflict
            };
        }
    }
}