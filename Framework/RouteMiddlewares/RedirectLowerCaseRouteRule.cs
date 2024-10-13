using Framework.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Net;

namespace Framework.RouteMiddlewares
{
    public class RedirectLowerCaseRouteRule : IRule
    {
        private readonly ISystemErrorLogger _systemError;
        public int StatusCode { get; } = (int)HttpStatusCode.MovedPermanently;

        public RedirectLowerCaseRouteRule(ISystemErrorLogger systemError)
        {
            _systemError = systemError;
        }

        public void ApplyRule(RewriteContext context)
        {
            try
            {
                HttpRequest request = context.HttpContext.Request;
                PathString path = request.Path;
                HostString host = request.Host;

                if (path.HasValue && path.Value.Any(char.IsUpper) || host.HasValue && host.Value.Any(char.IsUpper))
                {
                    HttpResponse response = context.HttpContext.Response;
                    response.StatusCode = StatusCode;
                    var f = (request.Scheme + "://" + host.Value + request.PathBase.Value + request.Path.Value).ToLower() + request.QueryString; ;
                    response.Headers[HeaderNames.Location] = f;
                    context.Result = RuleResult.ContinueRules;
                }
            }
            catch(Exception e)
            {
                _systemError.SaveError(e, "Framework", "RedirectLowerCaseRouteRule", "ApplyRule");
            }
        }
    }
}
