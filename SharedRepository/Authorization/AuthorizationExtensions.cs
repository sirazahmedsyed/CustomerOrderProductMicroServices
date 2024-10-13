using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Dapper;
using Npgsql;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SharedRepository.Authorization
{
    public static class AuthorizationExtensions
    {
        public static void AddSharedAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Permissions.AddUser, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.AddUser)));
                options.AddPolicy(Permissions.AddUserGroup, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.AddUserGroup)));
                options.AddPolicy(Permissions.AddCustomer, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.AddCustomer)));
                options.AddPolicy(Permissions.AddProducts, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.AddProducts)));
                options.AddPolicy(Permissions.AddOrder, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.AddOrder)));
            });
            //services.AddScoped<IAuthorizationFilter, PermissionAuthorizationFilter>();
            //services.AddControllers(options =>
            //{
            //    options.Filters.Add<PermissionAuthorizationFilter>();
            //});

            //services.AddScoped<PermissionAuthorizationFilter>();

            //services.AddControllers(options =>
            //{
            //    options.Filters.Add<PermissionAuthorizationFilter>();
            //});

        }
    }

    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly string _dbconnection;

        public PermissionHandler(IConfiguration configuration)
        {
            _dbconnection = configuration.GetConnectionString("DefaultConnection");
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User == null || !context.User.Identity.IsAuthenticated)
            {
                return;
            }

            var userGroupNoClaim = context.User.Claims.FirstOrDefault(c => c.Type == "UserGroupNo");
            if (userGroupNoClaim == null || !int.TryParse(userGroupNoClaim.Value.Trim(), out int userGroupNo))
            {
                Console.WriteLine($"Error parsing UserNo claim value: {userGroupNoClaim?.Value}");
                return;
            }

            using var connection = new NpgsqlConnection(_dbconnection);
            await connection.OpenAsync();

            var userGroup = await connection.QueryFirstOrDefaultAsync<UserGroup>(
                "SELECT * FROM public.\"UserGroups\" WHERE \"UserGroupNo\" = @UserGroupNo",
                new { UserGroupNo = userGroupNo });

            if (userGroup != null)
            {
                if (userGroup.IsAdmin)
                {
                    context.Succeed(requirement);
                    return;
                }

                bool hasPermission = requirement.Permission switch
                {
                    Permissions.AddUser => userGroup.AllowAddUser,
                    Permissions.AddUserGroup => userGroup.AllowAddUserGroup,
                    Permissions.AddCustomer => userGroup.AllowAddCustomer,
                    Permissions.AddProducts => userGroup.AllowAddProducts,
                    Permissions.AddOrder => userGroup.AllowAddOrder,
                    _ => false
                };

                if (hasPermission)
                {
                    context.Succeed(requirement);
                }
                //else
                //{
                //    // If not, fail the authorization
                //    context.Fail();

                //    var httpContext = context.Resource as Microsoft.AspNetCore.Http.HttpContext;
                //    if (httpContext != null)
                //    {
                //        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                //            await httpContext.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                //            {
                //                StatusCode = 403,
                //                Message = "You do not have permission to access this resource."
                //            }));
                //    }
                //}
            }
        }
    }

    //public class PermissionAuthorizationFilter : IAuthorizationFilter
    //{
    //    private readonly IAuthorizationService _authorizationService;
    //    private readonly ILogger<PermissionAuthorizationFilter> _logger;

    //    public PermissionAuthorizationFilter(IAuthorizationService authorizationService, ILogger<PermissionAuthorizationFilter> logger)
    //    {
    //        _authorizationService = authorizationService;
    //        _logger = logger;
    //    }

    //    public async void OnAuthorization(AuthorizationFilterContext context)
    //    {
    //        _logger.LogInformation("PermissionAuthorizationFilter.OnAuthorization method called");

    //        var authorizeAttribute = context.ActionDescriptor.EndpointMetadata
    //            .OfType<AuthorizeAttribute>()
    //            .FirstOrDefault();

    //        if (authorizeAttribute == null)
    //        {
    //            _logger.LogWarning("No AuthorizeAttribute found on the endpoint");
    //            return;
    //        }

    //        var user = context.HttpContext.User;
    //        var policy = authorizeAttribute.Policy;

    //        _logger.LogInformation($"Authorizing user for policy: {policy}");

    //        if (!string.IsNullOrEmpty(policy))
    //        {
    //            var authorizationResult = await _authorizationService.AuthorizeAsync(user, policy);

    //            if (!authorizationResult.Succeeded)
    //            {
    //                _logger.LogWarning($"Authorization failed for policy: {policy}");
    //                context.Result = new ObjectResult(new
    //                {
    //                    StatusCode = 403,
    //                    Message = "You do not have permission to access this resource."
    //                })
    //                {
    //                    StatusCode = StatusCodes.Status403Forbidden
    //                };
    //            }
    //            else
    //            {
    //                _logger.LogInformation($"Authorization succeeded for policy: {policy}");
    //            }
    //        }
    //    }
    //}

    //public async void OnAuthorization(AuthorizationFilterContext context)
    //{
    //    var authorizeAttribute = context.ActionDescriptor.EndpointMetadata
    //        .OfType<AuthorizeAttribute>()
    //        .FirstOrDefault();

    //    if (authorizeAttribute == null)
    //    {
    //        return;
    //    }

    //    var user = context.HttpContext.User;
    //    var policy = authorizeAttribute.Policy;

    //    if (!string.IsNullOrEmpty(policy))
    //    {
    //        var authorizationResult = await _authorizationService.AuthorizeAsync(user, policy);

    //        if (!authorizationResult.Succeeded)
    //        {
    //            context.Result = new ForbidResult();
    //        }
    //    }
    //}
    //}

    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }

    public static class Permissions
    {
        public const string AddUser = "AddUser";
        public const string AddUserGroup = "AddUserGroup";
        public const string AddCustomer = "AddCustomer";
        public const string AddProducts = "AddProducts";
        public const string AddOrder = "AddOrder";
    }

    public class UserGroup
    {
        public int UserGroupNo { get; set; }
        public bool IsAdmin { get; set; }
        public bool AllowAddUser { get; set; }
        public bool AllowAddUserGroup { get; set; }
        public bool AllowAddCustomer { get; set; }
        public bool AllowAddProducts { get; set; }
        public bool AllowAddOrder { get; set; }
    }
}