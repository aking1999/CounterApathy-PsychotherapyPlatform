using Database.Models;
using Database.RepositoryImplementations;
using Framework.Helpers;
using Framework.Helpers.ExtensionMethods;
using Framework.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using UAParser;

namespace Framework.ActionFilters
{
    public class UserActivityLoggerAttribute : ActionFilterAttribute
    {
        private readonly IErrorLogger _errors;
        private readonly UnitOfWork _context;

        public UserActivityLoggerAttribute(IErrorLogger errors)
        {
            _errors = errors;
            _context = new UnitOfWork(new LajsnaProbaContext());
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                context.HttpContext.Items["actionExecutedTimer"] = Stopwatch.StartNew();
                context.HttpContext.Items["actionArguments"] = context.ActionArguments;
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "Framework", "UserActivityLoggerAttribute", "OnActionExecuting");
            }
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            try
            {
                context.HttpContext.Items["actionExecutedMilliseconds"] = ((Stopwatch)context.HttpContext.Items["actionExecutedTimer"]).ElapsedMilliseconds;
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "Framework", "UserActivityLoggerAttribute", "OnActionExecuted");
            }
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            try
            {
                var controller = context.HttpContext.GetRouteValue("controller")?.ToString();
                var action = context.HttpContext.GetRouteValue("action")?.ToString();
                var userAgentParser = Parser.GetDefault().Parse(context.HttpContext.Request.Headers["User-Agent"]);

                var activityLog = new UserActivityLogs
                {
                    Id = Helper.GenerateNumbersId(),
                    UserIdOrAnonymous = context.HttpContext.User.Identity.IsAuthenticated ? context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.TakeMax(450) : "Anonymous",
                    Area = context.HttpContext.GetRouteData()?.Values["area"]?.ToString().TakeMax(64),
                    Controller = controller.TakeMax(64),
                    Action = action.TakeMax(64),
                    QueryDataJson = !string.IsNullOrWhiteSpace(context.HttpContext.Request?.QueryString.Value) ?
                                context.HttpContext.Request?.QueryString.Value.TakeMax(2048) :
                                JsonConvert.SerializeObject((IDictionary<string, object>)context.HttpContext.Items["actionArguments"]).TakeMax(2048),
                    MethodType = context.HttpContext.Request.Method,
                    UserAgent = userAgentParser.ToString().TakeMax(1024),
                    IsCrawler = userAgentParser.Device.IsSpider,
                    IpAddress = context.HttpContext?.Connection?.RemoteIpAddress?.ToString().TakeMax(256),
                    ActivityDateTime = DateTime.UtcNow
                };

                var actionExecutedMilliseconds = context.HttpContext.Items["actionExecutedMilliseconds"];

                activityLog.ActionExecutedMilliseconds = actionExecutedMilliseconds != null ? Convert.ToInt64(actionExecutedMilliseconds) : 0; // null reference sometimes, temporary stupid fix

                var actionExecutedTimer = context.HttpContext.Items["actionExecutedTimer"];
                activityLog.ResultExecutedMilliseconds = actionExecutedTimer != null ? ((Stopwatch)actionExecutedTimer).ElapsedMilliseconds : 0; // null reference sometimes, temporary stupid fix

                _context.UserActivityLogs.Insert(activityLog);

                _context.Save();

                if (actionExecutedTimer != null)
                    ((Stopwatch)actionExecutedTimer).Stop(); // null reference sometimes, temporary fix
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "Framework", "UserActivityLoggerAttribute", "OnResultExecuted");
            }
        }
    }
}
