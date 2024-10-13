using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace SharedRepository.Authorization
{
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PermissionMiddleware> _logger;
        private readonly IAuthorizationService _authorizationService;

        public PermissionMiddleware(RequestDelegate next, ILogger<PermissionMiddleware> logger, IAuthorizationService authorizationService)
        {
            _next = next;
            _logger = logger;
            _authorizationService = authorizationService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("PermissionMiddleware invoked");

            var endpoint = context.GetEndpoint();
            if (endpoint == null)
            {
                await _next(context);
                return;
            }

            var authorizeAttribute = endpoint.Metadata.GetMetadata<AuthorizeAttribute>();
            if (authorizeAttribute == null)
            {
                await _next(context);
                return;
            }

            var policy = authorizeAttribute.Policy;
            if (string.IsNullOrEmpty(policy))
            {
                await _next(context);
                return;
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(context.User, policy);

            if (authorizationResult.Succeeded)
            {
                _logger.LogInformation($"Authorization succeeded for policy: {policy}");
                await _next(context);
            }
            else
            {
                _logger.LogWarning($"Authorization failed for policy: {policy}");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    StatusCode = 403,
                    Message = "You do not have permission to access this resource."
                });
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class PermissionMiddlewareExtensions
    {
        public static IApplicationBuilder UsePermissionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PermissionMiddleware>();
        }
    }
}