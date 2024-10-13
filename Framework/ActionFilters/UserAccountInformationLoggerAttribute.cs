using Framework.Implementations;
using Framework.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;

namespace Framework.ActionFilters
{
    public class UserAccountInformationLoggerAttribute : ActionFilterAttribute
    {
        private readonly IErrorLogger _errors;
        private readonly ICustomClientFunctionsProvider _clientFunctions;

        public UserAccountInformationLoggerAttribute(IHttpContextAccessor contextAccessor, IErrorLogger errors)
        {
            _errors = errors;
            _clientFunctions = new CustomClientFunctionsProvider(contextAccessor);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                if (context.HttpContext.User.Identity.IsAuthenticated)
                    _clientFunctions.UpdateUserAccountInformation(context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, isSigningIn: false);
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "Framework", "UserAccountInformationLoggerAttribute", "OnActionExecuting");
            }
        }

        //ovo ne radi
        //public override void OnActionExecuted(ActionExecutedContext context)
        //{
        //    if (context.HttpContext.User.Identity.IsAuthenticated)
        //    {
        //        var controller = context.HttpContext.GetRouteValue("controller")?.ToString();
        //        var action = context.HttpContext.GetRouteValue("action")?.ToString();

        //        if (controller.ToLower() == "authorization" && action.ToLower() == "signin")
        //            _clientFunctions.UpdateUserAccountInformation(context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, isSigningIn: true);
        //    }

        //}
    }
}
