using LightStreamWeb.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers
{
    public class SessionController : Controller
    {
        //
        // GET: /Session/Get
        public ActionResult Get(string key)
        {
            return new JsonNetResult()
            {
                JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet,
                Data = Session[key]
            };
        }

        //
        // GET: /Session/Set
        public ActionResult Set(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Session[key] = value;
            }
            
            return new EmptyResult();
        }

        //
        // GET: /Session/Delete
        public ActionResult Delete(string key)
        {
            Session.Remove(key);
            return new EmptyResult();
        }

    }
}
