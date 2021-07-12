using Common.Models.Requests;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorUI.Pages.Authentication
{
    public partial class Login
    {
        private LoginRequest model = new LoginRequest();

        protected override async Task OnInitializedAsync()
        {
            var state = await _stateProvider.GetAuthenticationStateAsync();
            if (state != new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())))
            {
                _navigationManager.NavigateTo("/");
            }
        }

        private async Task SubmitAsync()
        {
            var result = await _authenticationManager.Login(model);
            if (result.Succeeded)
            {
                _snackBar.Add($"Welcome {model.Username}.", Severity.Success);
                _navigationManager.NavigateTo("/", true);
            }
            else
            {
                foreach (var message in result.Messages)
                {
                    _snackBar.Add(message, Severity.Error);
                }
            }
        }

        private void FillAdminstratorCredentials()
        {
            model.Username = "Junju";
            model.Password = "Admin@1234";
        }

        private void FillBasicUserCredentials()
        {
            model.Username = "JonBasic";
            model.Password = "Admin@1234";
        }
    }
}
