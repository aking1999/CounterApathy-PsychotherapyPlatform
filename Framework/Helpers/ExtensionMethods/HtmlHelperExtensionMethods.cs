using Database.Models;
using Framework.Implementations;
using Framework.Interfaces;
using Framework.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Framework.Helpers.ExtensionMethods
{
    public static class HtmlHelperExtensionMethods
    {
        private const string _partialViewResourceItemPrefix = "resources_";
        private const string _partialViewScriptItemPrefix = "scripts_";
        private static IErrorLogger _errors => new HttpContextAccessor().HttpContext?.RequestServices?.GetRequiredService<IErrorLogger>();
        private static UserManager<CustomClient> _userManager => new HttpContextAccessor().HttpContext?.RequestServices?.GetRequiredService<UserManager<CustomClient>>();
        private static ClaimsPrincipal User => new HttpContextAccessor().HttpContext?.User;

        public static async Task RenderMenuBasedOnUserRole(this IHtmlHelper html)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                //Each user can have only 1 role
                var userRole = (await _userManager.GetRolesAsync(user)).SingleOrDefault();

                if (userRole == default)
                    throw new Exception("User does not have a role or has multiple.");

                await html.RenderPartialAsync("Menues/_" + userRole + "Menu");
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "Framework", "HtmlHelperExtensionMethods", "RenderMenuBasedOnUserRole");
            }
        }

        public static bool ViewExists(this IHtmlHelper html, string viewName, bool isPartialView = false)
        {
            try
            {
                var viewEngine = html.ViewContext.HttpContext.RequestServices.GetService<ICompositeViewEngine>();

                if (viewEngine.GetView(html.ViewContext.ExecutingFilePath, viewName, isMainPage: !isPartialView).Success) return true;

                if (viewEngine.FindView(html.ViewContext, viewName, isMainPage: true).Success) return true;

                return false;
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "Framework", "HtmlHelperExtensionMethods", "ViewExists");
                return false;
            }
        }

        public static async Task<IHtmlContent> TryPartialAsync(this IHtmlHelper html, string partialViewName)
        {
            try
            {
                return ViewExists(html, partialViewName, isPartialView: true) ?
                    await html.PartialAsync(partialViewName) : throw new GeneralException($"View '{partialViewName}' does not exist.");
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "Framework", "HtmlHelperExtensionMethods", "TryPartialAsync");
                return new HtmlString(string.Empty);
            }
        }

        public static async Task<IHtmlContent> TryPartialAsync(this IHtmlHelper html, string partialViewName, object model)
        {
            try
            {
                return ViewExists(html, partialViewName, isPartialView: true) ?
                    await html.PartialAsync(partialViewName, model) : throw new GeneralException($"View '{partialViewName}' does not exist.");
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "Framework", "HtmlHelperExtensionMethods", "TryPartialAsync");
                return new HtmlString(string.Empty);
            }
        }

        public static string KeyWords(this IHtmlHelper html)
        {
            return "psihoterapija, psihoterapija beograd, psihoterapija novi sad, psihoterapija nis, " +
                "psihoterapija online, rebt psihoterapija, psihoterapija cena, online psihoterapija srbija, " +
                "besplatna psihoterapija, psihoterapija online besplatno, gestalt psihoterapija, " +
                "onlajn psihoterapija, psihoterapeut, licencirani psihoterapeuti, psihoterapeut beograd, " +
                "psihoterapeut novi beograd, psihoterapeut novi sad, psihoterapeut nis, psiholog, " +
                "psiholog beograd, psiholog novi sad, psiholog nis, psihoterapeut cenovnik, " +
                "psihoterapija seasa uzivo, psihoterapija seansa licno, psihoterapija seansa, gestalt studio, " +
                "asertivna komunikacija, granicnni poremecaj licnosti, mucnina, bipolarni poremecaj, anksioznost, " +
                "anxioznost, depresija, psiholog beograd iskustva, psiholog beograd preporuka, " +
                "psiholog novi sad iskustva, psiholog novi sad preporuka, psiholog nis iskustva, " +
                "psiholog nis preporuka, psihoterapeut beograd iskustva, psihoterapeut beograd preporuka, " +
                "psihoterapeut novi sad iskustva, psihoterapeut novi sad preporuka, psihoterapeut nis iskustva, " +
                "psihoterapeut nis preporuka, psihoterapija beograd iskustva, psihoterapija beograd preporuka, " +
                "psihoterapija novi sad iskustva, psihoterapija novi sad preporuka, psihoterapija nis iskustva, " +
                "psihoterapija nis preporuka, epsihoterapija, psihocentrala";
        }

        public static IHtmlContent Resource(this IHtmlHelper htmlHelper, Func<object, HelperResult> template, string type = "css")
        {
            if (htmlHelper.ViewContext.HttpContext.Items[type] != null) 
                ((List<Func<object, HelperResult>>)htmlHelper.ViewContext.HttpContext.Items[type]).Add(template);
            else 
                htmlHelper.ViewContext.HttpContext.Items[type] = new List<Func<object, HelperResult>>() { template };

            return new HtmlContentBuilder();
        }

        public static IHtmlContent RenderResources(this IHtmlHelper htmlHelper, string type = "css")
        {
            if (htmlHelper.ViewContext.HttpContext.Items[type] != null)
            {
                List<Func<object, HelperResult>> Resources = (List<Func<object, HelperResult>>)htmlHelper.ViewContext.HttpContext.Items[type];

                foreach (var Resource in Resources)
                {
                    if (Resource != null) htmlHelper.ViewContext.Writer.Write(Resource(null));
                }
            }

            return new HtmlContentBuilder();
        }

        public static IHtmlContent PartialSectionScripts(this IHtmlHelper htmlHelper, Func<object, HelperResult> template)
        {
            try
            {
                htmlHelper.ViewContext.HttpContext.Items[_partialViewScriptItemPrefix + Guid.NewGuid()] = template;
                return new HtmlContentBuilder();
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "Framework", "HtmlHelperExtensionMethods", "PartialSectionScripts");
                return new HtmlContentBuilder();
            }
        }

        public static IHtmlContent RenderPartialSectionScripts(this IHtmlHelper htmlHelper)
        {
            try
            {
                var partialSectionScripts = htmlHelper.ViewContext.HttpContext.Items.Keys
                    .Where(k => Regex.IsMatch(
                        k.ToString(),
                        "^" + _partialViewScriptItemPrefix + "([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$"));
                var contentBuilder = new HtmlContentBuilder();
                foreach (var key in partialSectionScripts)
                {
                    var template = htmlHelper.ViewContext.HttpContext.Items[key] as Func<object, HelperResult>;
                    if (template != null)
                    {
                        var writer = new StringWriter();
                        template(null).WriteTo(writer, HtmlEncoder.Default);
                        contentBuilder.AppendHtml(writer.ToString());
                    }
                }
                return contentBuilder;
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "Framework", "HtmlHelperExtensionMethods", "RenderPartialSectionScripts");
                return new HtmlContentBuilder();
            }
        }
    }
}