namespace pdvApp.Service.Interface
{
    public interface IAuthService
    {
        Task<bool> IsAuthenticatedAsync();
        Task<string> GetTokenAsync();
        Task SetTokenAsync(string token);
        Task RemoveTokenAsync();
        Task<bool> ValidateTokenAsync();
        event Action OnAuthenticationChanged;
    }
}
