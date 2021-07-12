using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WepApi.Hubs
{
    public class SignalRHub : Hub
    {
        public async Task UpdateDashboardAsync()
        {
            await Clients.All.SendAsync("UpdateDashboard");
        }

        public async Task RegenerateTokensAsync()
        {
            await Clients.All.SendAsync("RegenerateTokens");
        }
    }
}
