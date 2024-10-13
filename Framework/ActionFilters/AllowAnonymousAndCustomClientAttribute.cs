using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Framework.Providers;
using System;
using Framework.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Framework.ActionFilters
{
    public class AllowAnonymousAndCustomClientAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                if (context.HttpContext.User.Identity.IsAuthenticated && !context.HttpContext.User.IsInRole(UserRoles.Client))
                {
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    context.Result = new RedirectToActionResult("Index", "Home", new { Area = "" }); //context.HttpContext.Response.Redirect("/Home/Index");
                }  
            }
            catch(Exception e)
            {
                context.HttpContext.RequestServices.GetRequiredService<IErrorLogger>().SaveError(e, "Framework", "AllowAnonymousAndCustomClientAttribute", "OnActionExecuting");
            }
        }
    }
}
