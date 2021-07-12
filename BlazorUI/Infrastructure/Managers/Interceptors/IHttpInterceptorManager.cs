using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbelt.Blazor;

namespace BlazorUI.Infrastructure.Managers.Interceptors
{
    public interface IHttpInterceptorManager : IManager
    {
        void RegisterEvent();

        Task InterceptBeforeHttpAsync(object sender, HttpClientInterceptorEventArgs e);

        void DisposeEvent();
    }
}
