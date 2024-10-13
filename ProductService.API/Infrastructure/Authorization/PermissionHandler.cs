using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data.Common;
using Dapper;
namespace ProductService.API.Infrastructure.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly string dbconnection = "Host=dpg-crvsqllds78s738bvq40-a.oregon-postgres.render.com;Database=user_usergroupdatabase;Username=user_usergroupdatabase_user;Password=X01Sf7FT75kppHe46dnULUCpe52s69ag";
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User == null || !context.User.Identity.IsAuthenticated)
            {
                return;
            }

            var userGroupNoClaim = context.User.Claims.FirstOrDefault(c => c.Type == "UserGroupNo");
            if (userGroupNoClaim == null || !int.TryParse(userGroupNoClaim.Value.Trim(), out int userGroupNo))
            {
                // Handle parsing error
                Console.WriteLine($"Error parsing UserNo claim value: {userGroupNoClaim}");
                return;
            }

            var connection = new NpgsqlConnection(dbconnection);
            connection.Open();
            Console.WriteLine($"connection opened : {connection}");
            var result = await connection.QuerySingleAsync<int>($"SELECT \"UserGroupNo\" FROM public.\"UserGroups\" WHERE \"UserGroupNo\" = '{userGroupNoClaim.Value}'");
            var IsAdmin = await connection.QuerySingleAsync<bool>($"SELECT \"IsAdmin\" FROM public.\"UserGroups\"  WHERE \"UserGroupNo\" = '{userGroupNoClaim.Value}'");
            var AllowAddUser = await connection.QuerySingleAsync<bool>($"SELECT \"AllowAddUser\" FROM public.\"UserGroups\" WHERE \"UserGroupNo\" = '{userGroupNoClaim.Value}'");
            var AllowAddCustomer = await connection.QuerySingleAsync<bool>($"SELECT \"AllowAddCustomer\" FROM public.\"UserGroups\" WHERE \"UserGroupNo\" = '{userGroupNoClaim.Value}'");
            var AllowAddProducts = await connection.QuerySingleAsync<bool>($"SELECT \"AllowAddProducts\" FROM public.\"UserGroups\" WHERE \"UserGroupNo\" = '{userGroupNoClaim.Value}'");
            var AllowAddOrder = await connection.QuerySingleAsync<bool>($"SELECT \"AllowAddOrder\" FROM public.\"UserGroups\" WHERE \"UserGroupNo\" = '{userGroupNoClaim.Value}'");
            var AllowAddUserGroup = await connection.QuerySingleAsync<bool>($"SELECT \"AllowAddUserGroup\" FROM public.\"UserGroups\" WHERE \"UserGroupNo\" = '{userGroupNoClaim.Value}'");
            Console.WriteLine($"connection created : {result}");

            var user = result;

            if (user != null)
            {
                if (IsAdmin == true)
                {
                    context.Succeed(requirement);
                    return;
                }

                bool hasPermission = requirement.Permission switch
                {
                    Permissions.AddUser => AllowAddUser,
                    Permissions.AddUserGroup => AllowAddUserGroup,
                    Permissions.AddCustomer => AllowAddCustomer,
                    Permissions.AddProducts => AllowAddProducts,
                    Permissions.AddOrder => AllowAddOrder,
                    _ => false
                };

                if (hasPermission)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
