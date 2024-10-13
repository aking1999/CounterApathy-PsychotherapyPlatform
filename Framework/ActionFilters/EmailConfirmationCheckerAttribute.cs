using Database.Models;
using Framework.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Framework.ActionFilters
{
    public class EmailConfirmationCheckerAttribute : ActionFilterAttribute
    {
        private readonly IErrorLogger _errors;
        private readonly UserManager<CustomClient> _userManager;

        public EmailConfirmationCheckerAttribute(IErrorLogger errors, UserManager<CustomClient> userManager)
        {
            _errors = errors;
            _userManager = userManager;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                if (context.HttpContext.User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(context.HttpContext.User);
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        var controller = context.HttpContext.GetRouteValue("controller")?.ToString();
                        var action = context.HttpContext.GetRouteValue("action")?.ToString();
                        if(controller.ToLower() != "authorization" && action.ToLower() != "confirmemail")
                        {
                            context.Result = new RedirectToActionResult("ConfirmEmail", "Authorization", new { Area = "" }); //context.HttpContext.Response.Redirect("/Home/Index");
                            await context.Result.ExecuteResultAsync(context);
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "Framework", "EmailConfirmationCheckerAttribute", "OnActionExecutionAsync");
            }

            if (context.Result == null)
                await base.OnActionExecutionAsync(context, next);
        }
    }
}
