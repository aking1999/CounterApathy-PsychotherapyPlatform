using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Helpers.ExtensionMethods
{
    public static class ViewDataExtensions
    {
        public static string GetTitle(this ViewDataDictionary viewData)
        {
            return viewData["Title"]?.ToString();
        }

        public static void SetTitle(this ViewDataDictionary viewData, string title)
        {
            viewData["Title"] = title;
        }

        public static string GetMetaDescription(this ViewDataDictionary viewData)
        {
            return viewData["MetaDescription"]?.ToString();
        }

        public static void SetMetaDescription(this ViewDataDictionary viewData, string metaDescription)
        {
            viewData["MetaDescription"] = metaDescription;
        }
    }
}
