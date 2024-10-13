using Framework.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using System;
using System.Net;
using System.Text;

namespace Framework.RouteMiddlewares
{
    public class RedirectToNonWwwRouteRule : IRule
    {
        private readonly ISystemErrorLogger _systemError;
        public int StatusCode { get; } = (int)HttpStatusCode.MovedPermanently;

        public RedirectToNonWwwRouteRule(ISystemErrorLogger systemError)
        {
            _systemError = systemError;
        }

        public void ApplyRule(RewriteContext context)
        {
            try
            {
                HttpRequest request = context.HttpContext.Request;
                HostString host = request.Host;

                if (host.Host.StartsWith("www."))
                {
                    HttpResponse response = context.HttpContext.Response;
                    response.StatusCode = StatusCode;
                    var newHost = new HostString(host.Host.Substring(4), host.Port ?? 80);
                    var newUrl = new StringBuilder().Append("https://").Append(newHost).Append(request.PathBase).Append(request.Path).Append(request.QueryString);
                    response.Redirect(newUrl.ToString(), true);
                    context.Result = RuleResult.ContinueRules;
                }
            }
            catch(Exception e)
            {
                _systemError.SaveError(e, "Framework", "RedirectToNonWwwRouteRule", "ApplyRule");
            }
        }
    }
}
