using Blazored.LocalStorage;
using BlazorUI.Infrastructure.Authentication;
using BlazorUI.Infrastructure.Managers;
using Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace BlazorUI
{
    public class Program
    {
        private const string ClientName = "SecuredWep.API";
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration.GetValue<string>("BaseAPIUrl")) });

            builder
                .Services
                .AddAuthorizationCore(options =>
                {
                    foreach (var permissionModule in PermissionModules.GetAllPermissionsModules())
                    {
                        RegisterPermissionClaimPolicyByModule(options, permissionModule);
                    }
                })
                .AddBlazoredLocalStorage()
                .AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies())
                .AddScoped<ApplicationAuthStateProvider>()
                .AddScoped<AuthenticationStateProvider, ApplicationAuthStateProvider>()
                .AddTransient<AuthenticationHeaderHandler>()
                .AddScoped(sp => sp
                    .GetRequiredService<IHttpClientFactory>()
                    .CreateClient(ClientName).EnableIntercept(sp))
                .AddHttpClient(ClientName, client => client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("BaseAPIUrl")))
                .AddHttpMessageHandler<AuthenticationHeaderHandler>();
            builder.Services.AddHttpClientInterceptor();

            builder.Services.AddMudServices();

            var managers = typeof(IManager);

            var types = managers
                .Assembly
                .GetExportedTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t => new
                {
                    Service = t.GetInterface($"I{t.Name}"),
                    Implementation = t
                })
                .Where(t => t.Service != null);

            foreach (var type in types)
            {
                if (managers.IsAssignableFrom(type.Service))
                {
                    builder.Services.AddTransient(type.Service, type.Implementation);
                }
            }

            //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            await builder.Build().RunAsync();
        }

        private static void RegisterPermissionClaimPolicyByModule(AuthorizationOptions options, string module)
        {
            var allPermissions = PermissionModules.GeneratePermissionsForModule(module);
            foreach (var permission in allPermissions)
            {
                options.AddPolicy(permission, policy => policy.RequireClaim(ApplicationClaimTypes.Permission, permission));
            }
        }
    }
}
