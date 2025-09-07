using Microsoft.AspNetCore.Components;
using pdvApp.Service.Interface;

namespace pdvApp.Helps
{
    public class AuthenticatedComponentBase : ComponentBase, IDisposable
    {
        [Inject] protected IAuthService AuthService { get; set; }
        [Inject] protected NavigationManager Navigation { get; set; }

        protected bool IsAuthenticated { get; private set; }
        protected bool IsLoading { get; private set; } = true;

        protected override async Task OnInitializedAsync()
        {
            AuthService.OnAuthenticationChanged += HandleAuthenticationChanged;
            await CheckAuthentication();
        }

        private async void HandleAuthenticationChanged()
        {
            await CheckAuthentication();
        }

        private async Task CheckAuthentication()
        {
            IsLoading = true;
            IsAuthenticated = await AuthService.IsAuthenticatedAsync();

            if (!IsAuthenticated)
            {
                Navigation.NavigateTo("/login", true);
            }

            IsLoading = false;
            StateHasChanged();
        }

        public void Dispose()
        {
            AuthService.OnAuthenticationChanged -= HandleAuthenticationChanged;
        }
    }
}
