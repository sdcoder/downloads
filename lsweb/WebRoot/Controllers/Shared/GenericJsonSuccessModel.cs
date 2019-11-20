using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Controllers.Shared
{
    public class GenericJsonSuccessModel
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorValue { get; set; }

        public static GenericJsonSuccessModel Fail(string errorMessage, string errorValue = null)
        {
            return new GenericJsonSuccessModel()
            {
                Success = false,
                ErrorMessage = errorMessage, 
                ErrorValue = errorValue
            };
        }

        public static GenericJsonSuccessModel Ok()
        {
            return new GenericJsonSuccessModel()
            {
                Success = true
            };
        }

    }
}