using FirstAgain.Common.Data;
using FirstAgain.Domain.Lookups.FirstLook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace LightStreamWeb.Helpers
{
    public static class EnumHelper
    {
        /// <summary>
        /// Creates a drop-down list from an enum's ValueList.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="ngModel"></param>
        /// <param name="name">name and Id attribute of drop-down list</param>
        /// <param name="valueList">IEnumerable<dynamic> - any list with an Enumeration and a Caption. A BindingSource or GetFilteredList() result may be used.</param>
        /// <param name="ngrequired"></param>
        /// <returns></returns>
        public static MvcHtmlString NgDropDownSelectList(this HtmlHelper htmlHelper,
                                                             string ngModel,
                                                             string name,
                                                             string ariaLabel,
                                                             IEnumerable<dynamic> valueList,
                                                             string ngRequired,
                                                             object attributes = null)
        {
            IDictionary<string, object> htmlAttributes;
            if (attributes == null)
            {
                htmlAttributes = new Dictionary<string, object>();
            }
            else
            {
                htmlAttributes = new RouteValueDictionary(attributes);
            }

            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var a in valueList)
            {
                if (a.Enumeration.ToString() == "NotSelected")
                {
                    items.Add(new SelectListItem()
                    {
                        Value = "",
                        Text = ""
                    });
                }
                else
                {
                    items.Add(new SelectListItem()
                    {
                        Value = a.Enumeration.ToString(),
                        Text = a.Caption.ToString()
                    });
                }
            }


            htmlAttributes["tabindex"] = "0";
            htmlAttributes["ng-model"] = ngModel;
            htmlAttributes["ls_drop_down_with_not_selected"] = "ls_drop_down_with_not_selected";
            htmlAttributes["ng_required"] = ngRequired;
            if (ariaLabel != null && ariaLabel != "")
            {
                htmlAttributes["aria-label"] = ariaLabel;
            }
            return htmlHelper.DropDownList(
                name,
                items,
                htmlAttributes
                );
        }
        
        /// <summary>
        /// Creates a drop-down list from an enum's ValueList.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="ngModel"></param>
        /// <param name="name">name and Id attribute of drop-down list</param>
        /// <param name="valueList">IEnumerable<dynamic> - any list with an Enumeration and a Caption. A BindingSource or GetFilteredList() result may be used.</param>
        /// <param name="required"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static MvcHtmlString NgDropDownList(this HtmlHelper htmlHelper,
                                                             string ngModel,
                                                             string name,
                                                             string ariaLabel,
                                                             IEnumerable<SelectListItem> items,
                                                             bool? required = true,
                                                             object attributes = null)
        {
            IDictionary<string, object> htmlAttributes;
            if (attributes == null)
            {
                htmlAttributes = new Dictionary<string, object>();
            }
            else
            {
                htmlAttributes = new RouteValueDictionary(attributes);
            }

            if (htmlAttributes.ContainsKey("ng_init") && htmlAttributes["ng_init"] == null)
            {
                htmlAttributes.Remove("ng_init");
            }

            if (!htmlAttributes.ContainsKey("ng_init") && items.Any(i => i.Value == "NotSelected"))
            {
                // This will set the default to 'NotSelected' - which is, by convention, the default for all FirstAgain enums.
                htmlAttributes["ng-init"] = string.Format("{0} = {0} || 'NotSelected'", ngModel);
            }

            htmlAttributes["ng-model"] = ngModel;
            if (required.GetValueOrDefault(true))
            {
                // replace the default "required" directive with our own, that prohibits "NotSelected"
                if (items.Any(i => i.Value == "NotSelected"))
                {
                    htmlAttributes.Remove("required");
                    htmlAttributes["ls_prohibit_not_selected"] = "ls_prohibit_not_selected";
                }
                else
                {
                    htmlAttributes["required"] = "required";
                    htmlAttributes["aria-required"] = "true";
                }
            }
            if (ariaLabel != null && ariaLabel != "")
            {
                htmlAttributes["aria-label"] = ariaLabel;
            }

            return htmlHelper.DropDownList(
                name,
                items,
                htmlAttributes
                );
        }

        /// <summary>
        /// Creates a drop-down list from an enum's ValueList.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="ngModel"></param>
        /// <param name="name">name and Id attribute of drop-down list</param>
        /// <param name="valueList">IEnumerable<dynamic> - any list with an Enumeration and a Caption. A BindingSource or GetFilteredList() result may be used.</param>
        /// <param name="required"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static MvcHtmlString NgDropDownDefinitionList(this HtmlHelper htmlHelper,
                                                             string ngModel,
                                                             string name,
                                                             string ariaLabel,
                                                             dynamic lookup,
                                                             bool? required = true,
                                                             object attributes = null)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in lookup)
            {
                items.Add(new SelectListItem()
                {
                    Value = item.Enumeration.ToString(),
                    Text = item.Definition
                });
            }

            return NgDropDownList(htmlHelper, ngModel, name, ariaLabel, items, required, attributes);
        }

        /// <summary>
        /// Creates a drop-down list from an enum's ValueList.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="ngModel"></param>
        /// <param name="name">name and Id attribute of drop-down list</param>
        /// <param name="valueList">IEnumerable<dynamic> - any list with an Enumeration, Caption and IsHidden. A BindingSource or GetFilteredList() result may be used.</param>
        /// <param name="required"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static MvcHtmlString NgDropDownEnumList(this HtmlHelper htmlHelper,
                                                             string ngModel,
                                                             string name,
                                                             string ariaLabel,
                                                             IEnumerable<dynamic> valueList,
                                                             bool? required = true,
                                                             object attributes = null,
                                                             Func<string, string> captionFilter = null)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var a in valueList.Where(x => x.IsHidden == false))
            {
                items.Add(new SelectListItem()
                {
                    Value = a.Enumeration.ToString(),
                    Text = (captionFilter == null) ? a.Caption.ToString() : captionFilter(a.Caption.ToString())
                });
            }

            return NgDropDownList(htmlHelper, ngModel, name, ariaLabel, items, required, attributes);
        }

        /// <summary>
        /// Creates a drop-down list from an enum's ValueList.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="ngModelAndName"></param>
        /// <param name="valueList"></param>
        /// <param name="selectedValue"></param>
        /// <param name="required"></param>
        /// <returns></returns>
        public static MvcHtmlString DropDownValueList<TEnum>(this HtmlHelper htmlHelper, 
                                                             string ngModelAndName, 
                                                             string ariaLabel,
                                                             LookupTable<TEnum>.ValueList valueList, 
                                                             TEnum selectedValue,
                                                             bool? required = true)
        {
            IEnumerable<SelectListItem> items =
                from value in valueList
                select new SelectListItem
                {
                    Text = value.Caption.ToString(),
                    Value = value.Enumeration.ToString(),
                    Selected = (value.Enumeration.Equals(selectedValue))
                };

            Dictionary<string, object> attributes = new Dictionary<string, object>();
            attributes["ng_model"] = ngModelAndName;
            if (ariaLabel != null && ariaLabel != "")
            {
                attributes["aria-label"] = ariaLabel;
            }
            if (required.GetValueOrDefault(true))
            {
                attributes["required"] = "required";
            }
            if (selectedValue != null)
            {
                attributes["ng_init"] = ngModelAndName + "='" + selectedValue.ToString() + "'";
            }
            return htmlHelper.DropDownList(
                ngModelAndName,
                items,
                attributes
                );
        }

        /// <summary>
        /// Creates a drop-down list from an enum's ValueList.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="ngModelAndName"></param>
        /// <param name="valueList"></param>
        /// <param name="selectedValue"></param>
        /// <param name="required"></param>
        /// <returns></returns>
        public static MvcHtmlString DropDownValueList<TEnum>(this HtmlHelper htmlHelper,
                                                             string ngModelAndName,
                                                             LookupTable<TEnum>.ValueList valueList,
                                                             TEnum selectedValue,
                                                             string placeholder,
                                                             bool? required = true)
        {
            IEnumerable<SelectListItem> items =
                from value in valueList
                select new SelectListItem
                {
                    Text = (value.Enumeration.ToString() == "NotSelected") ? placeholder : value.Caption.ToString(),
                    Value = value.Enumeration.ToString(),
                    Selected = (value.Enumeration.Equals(selectedValue))
                };

            Dictionary<string, object> attributes = new Dictionary<string, object>();
            attributes["ng_model"] = ngModelAndName;
            attributes["placeholder"] = placeholder;
            if (required.GetValueOrDefault(true))
            {
                attributes["required"] = "required";
            }
            if (selectedValue != null)
            {
                attributes["ng_init"] = ngModelAndName + "='" + selectedValue.ToString() + "'";
            }
            return htmlHelper.DropDownList(
                ngModelAndName,
                items,
                attributes
                );
        }

        /// <summary>
        /// Creates a drop-down list from an enum's ValueList, with a value to exclude from the list.
        /// Most useful for hiding the "NotSelected" value
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="ngModelAndName"></param>
        /// <param name="valueList"></param>
        /// <param name="selectedValue"></param>
        /// <param name="excludeValue"></param>
        /// <param name="required"></param>
        /// <returns></returns>
        public static MvcHtmlString DropDownValueList<TEnum>(this HtmlHelper htmlHelper,
                                                             string ngModelAndName,
                                                             string ariaLabel,
                                                             LookupTable<TEnum>.ValueList valueList,
                                                             TEnum selectedValue,
                                                             TEnum excludeValue,
                                                             bool? required = true)
        {
            IEnumerable<SelectListItem> items =
                from value in valueList
                where !value.Enumeration.Equals(excludeValue)
                select new SelectListItem
                {
                    Text = value.Caption.ToString(),
                    Value = value.Enumeration.ToString(),
                    Selected = (value.Enumeration.Equals(selectedValue))
                };

            Dictionary<string, object> attributes = new Dictionary<string, object>();
            attributes["ng_model"] = ngModelAndName;
            attributes["aria-label"] = ariaLabel;
            if (required.GetValueOrDefault(true))
            {
                attributes["required"] = "required";
            }
            if (selectedValue != null)
            {
                attributes["ng_init"] = ngModelAndName + "='" + selectedValue.ToString() + "'";
            }
            return htmlHelper.DropDownList(
                ngModelAndName,
                items,
                attributes
                );
        }

        public static MvcHtmlString EnumDropDownList<TEnum>(this HtmlHelper htmlHelper, string ngModelAndName, TEnum selectedValue, bool? required = true)
        {
            IEnumerable<TEnum> values = Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>();

            IEnumerable<SelectListItem> items =
                from value in values
                select new SelectListItem
                {
                    Text = value.ToString(),
                    Value = value.ToString(),
                    Selected = (value.Equals(selectedValue))
                };

            Dictionary<string, object> attributes = new Dictionary<string, object>();
            attributes["ng_model"] = ngModelAndName;
            if (required.GetValueOrDefault(true)) 
            {
                attributes["required"] = "required";
            }
            if (selectedValue != null)
            {
                attributes["ng_init"] = ngModelAndName + "='" + selectedValue.ToString() + "'";
            }
            return htmlHelper.DropDownList(
                ngModelAndName,
                items,
                attributes
                );
        }
    }
}