using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Supabase.Gotrue;
using TimeSight.SupabaseClient.Models;
namespace TimeSight.SupabaseClient.Services;

public class SupabaseAuthService(Supabase.Client supabase)
{
    public async Task SignOut()
    {
        await supabase.Auth.SignOut();
    }
    public async Task SignIn(string email, string password)
    {
        await supabase.Auth.SignIn(email, password);
    }

    public async Task<SignInInfo> GetSignInInfoForProvider(Constants.Provider provider, string redirectUri)
    {
        var info = await supabase.Auth.SignIn(provider,
            new SignInOptions
            {
                RedirectTo = redirectUri,
                FlowType = Constants.OAuthFlowType.PKCE
            });

        return new SignInInfo(info.Uri, info.PKCEVerifier!);
    }

    public async Task SignInWithCode(string pkceVerifier, string code)
    {
        await supabase.Auth.ExchangeCodeForSession(pkceVerifier, code);
    }
}
