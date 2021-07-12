using BlazorUI.Infrastructure.Extensions;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace BlazorUI.Shared.Components
{
    public partial class UserCard
    {
        [Parameter] public string Class { get; set; }
        private string FirstName { get; set; }
        private string LastName { get; set; }
        private string Email { get; set; }
        private char FirstLetterOfName { get; set; }

        [Parameter]
        public string ImageDataUrl { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var state = await _stateProvider.GetAuthenticationStateAsync();
            var user = state.User;

            Email = user.GetEmail().Replace(".com", string.Empty);
            FirstName = user.GetFirstName();
            LastName = user.GetLastName();
            if (FirstName.Length > 0)
            {
                FirstLetterOfName = FirstName[0];
            }
            var UserId = user.GetUserId();
            //var imageResponse = await _accountManager.GetProfilePictureAsync(UserId);
            //if (imageResponse.Succeeded)
            //{
            //    ImageDataUrl = imageResponse.Data;
            //}
        }
    }
}