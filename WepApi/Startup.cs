using AutoMapper.Configuration;
using Common.Constants;
using Common.Wrappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WepApi.Context;
using WepApi.Hubs;
using WepApi.Permission;
using WepApi.Services;
using WepApi.Settings;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace WepApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddSignalR();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "WepApi",
                    License = new OpenApiLicense()
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Input your Bearer token in this format - Bearer {your token here} to access this API",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                            Scheme = "Bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        }, new List<string>()
                    },
                });
            });

            var appConfigurationSection = Configuration.GetSection("AppConfiguration");
            services.Configure<AppConfiguration>(appConfigurationSection);

            var apiSettings = appConfigurationSection.Get<AppConfiguration>();
            var key = Encoding.ASCII.GetBytes(apiSettings.Secret);

            services.AddCors(o => o.AddPolicy("BlazorUI", builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            }));

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(bearer =>
                {
                    bearer.RequireHttpsMetadata = false;
                    bearer.SaveToken = true;
                    bearer.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        RoleClaimType = ClaimTypes.Role,
                        ClockSkew = TimeSpan.Zero
                    };
                    bearer.Events = new JwtBearerEvents()
                    {
                        OnAuthenticationFailed = c =>
                        {
                            c.NoResult();
                            c.Response.StatusCode = 500;
                            c.Response.ContentType = "text/plain";
                            return c.Response.WriteAsync(c.Exception.ToString());
                        },
                        //OnChallenge = context =>
                        //{
                        //    context.HandleResponse();
                        //    //context.Response.StatusCode = context.Response == null ? context.Response.StatusCode = 401 : context.Response.StatusCode;
                        //    context.Response.ContentType = "application/json";
                        //    var result = JsonConvert.SerializeObject(Result.Fail("You are not Authorized."));
                        //    return context.Response.WriteAsync(result);
                        //},
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = 403;
                            context.Response.ContentType = "application/json";
                            var result = JsonConvert.SerializeObject(Result.Fail("You are not authorized to access this resource."));
                            return context.Response.WriteAsync(result);
                        },
                    };

                    bearer.Events = new JwtBearerEvents()
                    {
                        OnAuthenticationFailed = c =>
                        {
                            c.NoResult();
                            c.Response.StatusCode = 500;
                            c.Response.ContentType = "text/plain";
                            return c.Response.WriteAsync(c.Exception.ToString());
                        },
                        //OnChallenge = context =>
                        //{
                        //    context.HandleResponse();
                        //    //context.Response.StatusCode = context.Response == null ? context.Response.StatusCode = 401 : context.Response.StatusCode;
                        //    context.Response.ContentType = "application/json";
                        //    var result = JsonConvert.SerializeObject(Result.Fail("You are not Authorized."));
                        //    return context.Response.WriteAsync(result);
                        //},
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = 403;
                            context.Response.ContentType = "application/json";
                            var result = JsonConvert.SerializeObject(Result.Fail("You are not authorized to access this resource."));
                            return context.Response.WriteAsync(result);
                        },
                    };
                });

            services.AddAuthorization(options =>
            {
                // Here I stored necessary permissions/roles in a constant
                foreach (var prop in typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                {
                    options.AddPolicy(prop.GetValue(null).ToString(), policy => policy.RequireClaim(ApplicationClaimTypes.Permission, prop.GetValue(null).ToString()));
                }
            });

            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddDbContext<ApplicationDbContext>(options => options
                .UseSqlServer(Configuration.GetConnectionString("DefaultConnection")))
                .AddTransient<IDatabaseSeeder, DatabaseSeeder>();

            services
                .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
                .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>()
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "WepApi v1");
                    options.RoutePrefix = "swagger";
                    options.DisplayRequestDuration();
                });
            }

            app.UseHttpsRedirection();
            app.UseCors("BlazorUI");
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
                endpoints.MapHub<SignalRHub>("/signalRHub");
            });
            var serviceScope = app.ApplicationServices.CreateScope();

            var initializers = serviceScope.ServiceProvider.GetServices<IDatabaseSeeder>();

            foreach (var initializer in initializers)
            {
                initializer.Seed();
            }

        }
    }
}
