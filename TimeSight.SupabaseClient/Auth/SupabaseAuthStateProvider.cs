using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace TimeSight.SupabaseClient.Auth;

/// <summary>
/// Link between Supabase Auth and Blazor Auth
/// </summary>
public class SupabaseAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly Supabase.Client _supabase;

    public SupabaseAuthStateProvider(Supabase.Client supabase)
    {
        _supabase = supabase;
        _supabase.Auth.AddStateChangedListener(OnAuthStateChanged);
    }

    private void OnAuthStateChanged(IGotrueClient<User, Session> sender, Constants.AuthState state)
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        User? user = _supabase.Auth.CurrentUser;

        if (user is null)
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.Email ?? string.Empty),
        };

        var identity = new ClaimsIdentity(claims, "supabase");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public void Dispose()
    {
        _supabase.Auth.RemoveStateChangedListener(OnAuthStateChanged);
    }
}
