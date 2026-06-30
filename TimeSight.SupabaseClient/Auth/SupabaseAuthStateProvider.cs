using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace TimeSight.SupabaseClient.Auth;

/// <summary>
/// Link between Supabase Auth and Blazor Auth
/// </summary>
public class SupabaseAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private const string EmailStorageKey = "timesight.userEmail";

    private readonly Supabase.Client _supabase;
    private readonly IJSInProcessRuntime _js;

    public SupabaseAuthStateProvider(Supabase.Client supabase, IJSRuntime jsRuntime)
    {
        _supabase = supabase;
        // In Blazor WASM, IJSRuntime is WebAssemblyJSRuntime which implements IJSInProcessRuntime
        _js = (IJSInProcessRuntime)jsRuntime;
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

        // user_metadata is only populated at first sign-up and isn't refreshed on later
        // logins, so fall back to the OAuth identity's data (refreshed every sign-in).
        var identityData = user.Identities?.FirstOrDefault()?.IdentityData;

        // A token refresh can return a trimmed user payload (no identities, blank email),
        // so only trust candidates that actually look like an email, and persist the last
        // known-good one so the display name stays put across refreshes instead of going blank.
        var email = FirstValidEmail(
            user.Email,
            user.UserMetadata?.GetValueOrDefault("email")?.ToString(),
            identityData?.GetValueOrDefault("email")?.ToString(),
            identityData?.GetValueOrDefault("preferred_username")?.ToString());

        if (email is not null)
            _js.InvokeVoid("localStorage.setItem", EmailStorageKey, email);
        else
            email = _js.Invoke<string?>("localStorage.getItem", EmailStorageKey) ?? string.Empty;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, email),
        };

        var identity = new ClaimsIdentity(claims, "supabase");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public void Dispose()
    {
        _supabase.Auth.RemoveStateChangedListener(OnAuthStateChanged);
    }

    private static string? FirstValidEmail(params string?[] candidates) =>
        candidates.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c) && c.Contains('@'));
}
