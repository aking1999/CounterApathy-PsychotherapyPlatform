using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Framework.RouteMiddlewares
{
    public class IgnoreRouteMiddleware
    {
        private readonly RequestDelegate _next;

        // You can inject a dependency here that gives you access
        // to your ignored route configuration.
        public IgnoreRouteMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.HasValue &&
                context.Request.Path.Value.ToLower().Contains("base"))
            {
                context.Response.StatusCode = 404;
                return;
            }

            await _next.Invoke(context);
        }
    }
}
