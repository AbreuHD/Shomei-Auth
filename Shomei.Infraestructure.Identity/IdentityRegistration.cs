﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Shomei.Infraestructure.Identity.BackgroundServices;
using Shomei.Infraestructure.Identity.Context;
using Shomei.Infraestructure.Identity.DTOs.Generic;
using Shomei.Infraestructure.Identity.Entities;
using Shomei.Infraestructure.Identity.Seeds;
using Shomei.Infraestructure.Identity.Settings;
using System.Reflection;
using System.Text;

namespace Shomei.Infraestructure.Identity
{
    public static class IdentityRegistration
    {
        public static void AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<IdentityContext>(options =>
            {
                options.EnableSensitiveDataLogging();
                options.UseMySql(Environment.GetEnvironmentVariable("IdentityConnection") ?? configuration.GetConnectionString("IdentityConnection"), new MySqlServerVersion(new Version(10, 6, 16)),
                    m => m.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName).SchemaBehavior(MySqlSchemaBehavior.Ignore));
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();


            services.ConfigureApplicationCookie(option =>
            {
                option.LoginPath = "/Account";
                option.AccessDeniedPath = "/User/AccessDenied";
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(int.Parse(configuration["JWTSettings:DefaultLockoutTimeSpan"] ?? "30"));
                options.Lockout.MaxFailedAccessAttempts = int.Parse(configuration["JWTSettings:MaxFailedAccessAttempts"] ?? "10");
                options.Lockout.AllowedForNewUsers = true;
            });

            if (Environment.GetEnvironmentVariable("SmtpPassword") != null)
            {
                services.Configure<MailSettings>(x =>
                {
                    x.EmailFrom = Environment.GetEnvironmentVariable("EmailFrom") ?? string.Empty;
                    x.SmtpHost = Environment.GetEnvironmentVariable("SmtpHost") ?? string.Empty;
                    x.SmtpPort = int.Parse(Environment.GetEnvironmentVariable("SmtpPort") ?? string.Empty);
                    x.DisplayName = Environment.GetEnvironmentVariable("DisplayName") ?? string.Empty;
                    x.SmtpUser = Environment.GetEnvironmentVariable("SmtpUser") ?? string.Empty;
                    x.SmtpPassword = Environment.GetEnvironmentVariable("SmtpPassword") ?? string.Empty;
                });
            }
            else
            {
                services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            }

            _ = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; //patrue
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = configuration["JWTSettings:Issuer"],
                    ValidAudience = configuration["JWTSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTSettings:Key"]))
                };
                options.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = async c =>
                    {
                        if (!c.Response.HasStarted)
                        {
                            c.NoResult();
                            c.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            c.Response.ContentType = "application/json";

                            var result = JsonConvert.SerializeObject(new GenericApiResponse<bool>
                            {
                                Message = "Your token is expired or it's not valid",
                                Success = false,
                                Statuscode = StatusCodes.Status401Unauthorized,
                            });
                            await c.Response.WriteAsync(result);
                            c.Response.CompleteAsync().Wait();
                        }
                    },
                    OnChallenge = async c =>
                    {
                        if (!c.Response.HasStarted)
                        {
                            c.HandleResponse();
                            c.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            c.Response.ContentType = "application/json";
                            var result = JsonConvert.SerializeObject(new GenericApiResponse<bool>
                            {
                                Message = "You're Not Authorized",
                                Success = false,
                                Statuscode = StatusCodes.Status401Unauthorized
                            });
                            await c.Response.WriteAsync(result);
                            c.Response.CompleteAsync().Wait();
                        }
                    },
                    OnForbidden = c =>
                    {
                        c.Response.StatusCode = StatusCodes.Status404NotFound;
                        c.Response.ContentType = "application/json";
                        var result = JsonConvert.SerializeObject(new GenericApiResponse<bool>
                        {
                            Message = "You're Not Authorized to access to this resource",
                            Success = false,
                            Statuscode = StatusCodes.Status404NotFound,
                        });
                        return c.Response.WriteAsync(result);
                    }

                };
            });
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<MailSettings>>().Value);
            services.AddHostedService<SessionCleanupService>();
        }

        public static async Task AddIdentityRolesAsync(this IServiceProvider services, IEnumerable<string> roles)
        {
            try
            {
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                await DefaultRoles.Seed(roleManager, roles);
                await DefaultOwner.Seed(userManager);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}