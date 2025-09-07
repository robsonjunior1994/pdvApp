using Microsoft.JSInterop;
using pdvApp.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace pdvApp.Service
{
    public class AuthService : IAuthService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;

        public event Action OnAuthenticationChanged;

        public AuthService(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await GetTokenAsync();
            return !string.IsNullOrEmpty(token) && await ValidateTokenAsync();
        }

        public async Task<string> GetTokenAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            }
            catch
            {
                return null;
            }
        }

        public async Task SetTokenAsync(string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
            OnAuthenticationChanged?.Invoke();
        }

        public async Task RemoveTokenAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            OnAuthenticationChanged?.Invoke();
        }

        public async Task<bool> ValidateTokenAsync()
        {
            var token = await GetTokenAsync();

            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                // Verifica se o token é um JWT válido (verificação básica)
                var parts = token.Split('.');
                if (parts.Length != 3) return false;

                // Verifica expiração (se for JWT)
                var payload = parts[1];
                var jsonBytes = ParseBase64WithoutPadding(payload);
                var json = Encoding.UTF8.GetString(jsonBytes);
                var payloadData = JsonSerializer.Deserialize<JsonElement>(json);

                if (payloadData.TryGetProperty("exp", out var expProperty))
                {
                    var exp = expProperty.GetInt64();
                    var expiry = DateTimeOffset.FromUnixTimeSeconds(exp);
                    return expiry > DateTimeOffset.UtcNow;
                }

                return true; // Se não tem exp, assume válido
            }
            catch
            {
                return false;
            }
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
