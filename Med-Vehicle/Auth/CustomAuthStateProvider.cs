using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Med_Vehicle.Models;

namespace Med_Vehicle.Auth;

// Custom Authentication State Provider
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    // Session storage to persist user data
    private readonly ProtectedSessionStorage _sessionStorage;
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    // Constructor
    public CustomAuthStateProvider(ProtectedSessionStorage sessionStorage) 
    {
        _sessionStorage = sessionStorage;
    }

    // Get the current authentication state
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<User>("UserSession");
            var user = result.Success ? result.Value : null;

            if (user == null) 
                return new AuthenticationState(_anonymous);

            // Create claims principal
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            }, "CustomAuth"));

            return new AuthenticationState(claimsPrincipal);
        }
        catch 
        { 
            return new AuthenticationState(_anonymous); 
        }
    }

    // Update the authentication state
    public async Task UpdateAuthenticationState(User? user)
    {
        ClaimsPrincipal claimsPrincipal;

        if (user != null)
        {
            await _sessionStorage.SetAsync("UserSession", user);
            
            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            }, "CustomAuth"));
        }
        else
        {
            await _sessionStorage.DeleteAsync("UserSession");
            claimsPrincipal = _anonymous;
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }
}