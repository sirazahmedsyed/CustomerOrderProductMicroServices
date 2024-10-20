using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using AuthService.API.Infrastructure.DTOs;
using AuthService.API.Infrastructure.Entities;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AuthService.API.Infrastructure.Services;
using Npgsql;
using Dapper;

namespace AuthService.API.Infrastructure.Services
{
    public class AuthServices : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly string dbconnection = "Host=dpg-crvsqllds78s738bvq40-a.oregon-postgres.render.com;Database=user_usergroupdatabase;Username=user_usergroupdatabase_user;Password=X01Sf7FT75kppHe46dnULUCpe52s69ag";
        public AuthServices(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }
        
        public async Task<(bool Succeeded, string Token)> AuthenticateAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                string token = await GenerateJwtTokenAsync(user);
                return (true, token);
            }
            return (false, null);
        }
        
        public async Task<(bool Succeeded, string UserId, string[] Errors)> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = new ApplicationUser
            {
                UserName = createUserDto.Username,
                Email = createUserDto.Email,
                UserNo = createUserDto.UserNo
            };

            var result = await _userManager.CreateAsync(user, createUserDto.Password);
            if (result.Succeeded)
            {
                return (true, user.Id, null);
            }
            return (false, null, result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<(bool Succeeded, string[] Errors)> UpdateUserAsync(UpdateUserDto updateUserDto)
        {
            var user = await _userManager.FindByIdAsync(updateUserDto.Id);
            if (user == null)
            {
                return (false, new[] { "User not found" });
            }

            user.UserName = updateUserDto.Username;
            user.Email = updateUserDto.Email;
            user.UserNo = updateUserDto.UserNo;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return (true, null);
            }
            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<(bool Succeeded, string[] Errors)> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, new[] { "User not found" });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return (true, null);
            }
            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var connection = new NpgsqlConnection(dbconnection);
            connection.Open();
            Console.WriteLine($"connection opened : {connection}");
            var result = await connection.QuerySingleAsync<string>($"SELECT \"UserNo\" FROM public.\"AspNetUsers\" WHERE \"UserName\" = '{user.UserName}'");
            
            Console.WriteLine($"connection created : {result}");

            var user_group_no = result switch
            {
                "USR001" => "1",
                "USR002" => "2",
                _ => "0" // default or fallback value
            };

            var claims = new List<Claim>
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("user_group_no", user_group_no) // Add UserGroupNo as a claim
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer:   _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddDays(10),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
