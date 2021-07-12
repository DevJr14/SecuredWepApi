using BlazorUI.Infrastructure.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BlazorUI.Shared
{
    public partial class MainLayout : IDisposable
    {
        private string CurrentUserId { get; set; }
        private string FirstName { get; set; }
        private string LastName { get; set; }
        private string Email { get; set; }
        private char FirstLetterOfName { get; set; }

        private async Task LoadDataAsync()
        {
            var state = await _stateProvider.GetAuthenticationStateAsync();
            var user = state.User;
            if (user == null) return;
            if (user.Identity.IsAuthenticated)
            {
                CurrentUserId = user.GetUserId();
                this.FirstName = user.GetFirstName();
                if (this.FirstName.Length > 0)
                {
                    FirstLetterOfName = FirstName[0];
                }
            }
        }

        private MudTheme currentTheme;
        private bool _drawerOpen = true;

        protected override async Task OnInitializedAsync()
        {
            _interceptor.RegisterEvent();
            //currentTheme = await _preferenceManager.GetCurrentThemeAsync();
            hubConnection = hubConnection.TryInitialize("https://localhost:44330/signalRHub");
            await hubConnection.StartAsync();
            //hubConnection.On<string, string, string>("ReceiveChatNotification", (message, receiverUserId, senderUserId) =>
            //{
            //    if (CurrentUserId == receiverUserId)
            //    {
            //        _jsRuntime.InvokeAsync<string>("PlayAudio", "notification");
            //        _snackBar.Add(message, Severity.Info, config =>
            //        {
            //            config.VisibleStateDuration = 10000;
            //            config.HideTransitionDuration = 500;
            //            config.ShowTransitionDuration = 500;
            //            config.Action = "Chat?";
            //            config.ActionColor = Color.Primary;
            //            config.Onclick = snackbar =>
            //            {
            //                _navigationManager.NavigateTo($"chat/{senderUserId}");
            //                return Task.CompletedTask;
            //            };
            //        });
            //    }
            //});
            hubConnection.On("RegenerateTokens", async () =>
            {
                try
                {
                    var token = await _authenticationManager.TryForceRefreshToken();
                    if (!string.IsNullOrEmpty(token))
                    {
                        _snackBar.Add("Refreshed Token.", Severity.Success);
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _snackBar.Add("You are Logged Out.", Severity.Error);
                    await _authenticationManager.Logout();
                    _navigationManager.NavigateTo("/");
                }
            });
        }

        private void Logout()
        {
            const string logoutConfirmationText = "Logout Confirmation";
            const string logoutText = "Logout";
            var parameters = new DialogParameters
            {
                { "ContentText", logoutConfirmationText },
                { "ButtonText", logoutText },
                { "Color", Color.Error }
            };

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };

            _dialogService.Show<Dialogs.Logout>("Logout", parameters, options);
        }

        private void DrawerToggle()
        {
            _drawerOpen = !_drawerOpen;
        }

        //private async Task DarkMode()
        //{
        //    bool isDarkMode = await _preferenceManager.ToggleDarkModeAsync();
        //    if (isDarkMode)
        //    {
        //        currentTheme = BlazorHeroTheme.DefaultTheme;
        //    }
        //    else
        //    {
        //        currentTheme = BlazorHeroTheme.DarkTheme;
        //    }
        //}

        public void Dispose()
        {
            _interceptor.DisposeEvent();
            //_ = hubConnection.DisposeAsync();
        }

        private HubConnection hubConnection;
        public bool IsConnected => hubConnection.State == HubConnectionState.Connected;
    }
}
