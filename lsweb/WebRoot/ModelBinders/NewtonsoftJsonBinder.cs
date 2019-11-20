using System.IO;
using System.Web.Mvc;
using FirstAgain.Common;

namespace LightStreamWeb.ModelBinders
{
    public class NewtonsoftJsonBinder<T> : IModelBinder where T : new()
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var stream = controllerContext.HttpContext.Request.InputStream;
            stream.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(stream))
            {
                string json = reader.ReadToEnd();
                return JsonUtility.CreateObject<T>(json);
            }
        }
    }
}