using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorUI.Infrastructure.Extensions
{
    public static class HubExtensions
    {
        public static HubConnection TryInitialize(this HubConnection hubConnection, string apiUri)
        {
            if (hubConnection == null)
            {
                hubConnection = new HubConnectionBuilder()
                                  .WithUrl(apiUri)
                                  .Build();
            }
            return hubConnection;
        }
    }
}
