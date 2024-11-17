using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedRepository.Repositories
{   
    public static class AddSharedServices
    {
        public static void AddAuthenticationSharedServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Jwt:SecretKey"]))
                };

                var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("JwtBearerEvents");

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = async context =>
                    {
                        logger.LogWarning("Authentication failed: {Message}", context.Exception.Message);
                        context.NoResult();
                        context.Response.Clear();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var response = new { message = "Unauthorized. Bearer token is missing or invalid." };
                        await context.Response.WriteAsJsonAsync(response);
                    },

                    OnChallenge = async context =>
                    {
                        logger.LogWarning("Authorization challenge triggered");
                        context.HandleResponse(); // Prevent default challenge response

                        context.Response.Clear();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var response = new { message = "Unauthorized. Bearer token is missing or invalid." };
                        await context.Response.WriteAsJsonAsync(response);
                    },

                    OnMessageReceived = context =>
                    {
                        logger.LogInformation("JWT token received for validation");
                        return Task.CompletedTask;
                    },

                    OnTokenValidated = context =>
                    {
                        logger.LogInformation("Token has been validated");
                        return Task.CompletedTask;
                    }
                };


                //var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
                //var logger = loggerFactory.CreateLogger("JwtBearerEvents");

                //    options.Events = new JwtBearerEvents
                //    {
                //        OnAuthenticationFailed = context =>
                //        {
                //            if (!context.Response.HasStarted)
                //            {
                //                logger.LogWarning("Unauthorized access attempt. Bearer token is missing or invalid.");
                //                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                //                context.Response.ContentType = "application/json";
                //                return context.Response.WriteAsync("{\"message\":\"Unauthorized. Bearer token is missing or invalid.\"}");
                //            }
                //            logger.LogWarning("Unauthorized access attempt. Bearer token is missing or invalid, but the response has already started.");
                //            return Task.CompletedTask;
                //        },
                //        OnChallenge = context =>
                //        {
                //            if (!context.Response.HasStarted)
                //            {
                //                logger.LogWarning("Unauthorized access attempt. Bearer token is missing or invalid.");
                //                context.HandleResponse();
                //                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                //                context.Response.ContentType = "application/json";
                //                return context.Response.WriteAsync("{\"message\":\"Unauthorized. Bearer token is missing or invalid.\"}");
                //            }
                //            logger.LogWarning("Unauthorized access attempt. Bearer token is missing or invalid, but the response has already started.");
                //            return Task.CompletedTask;
                //        }
                //    };


            });
        }
        public static void AddSwaggerGenSharedServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });
        }
    }
}
