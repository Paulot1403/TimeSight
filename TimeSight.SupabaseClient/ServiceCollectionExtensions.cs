using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Supabase.Gotrue.Interfaces;
using Supabase.Gotrue;
using TimeSight.SupabaseClient.Auth;
using TimeSight.SupabaseClient.Options;
using TimeSight.SupabaseClient.Services;

namespace TimeSight.SupabaseClient;

public static class ServiceCollectionExtensions
{
    public static OptionsBuilder<SupabaseClientOptions> AddSupabase(this IServiceCollection services)
    {
        services.AddSingleton<IGotrueSessionPersistence<Session>, BlazorLocalStorageSessionPersistence>();

        services.AddSingleton(sp =>
        {
            SupabaseClientOptions options = sp.GetRequiredService<IOptions<SupabaseClientOptions>>().Value;
            var client = new Supabase.Client(options.Url, options.AnonKey, new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true
            });
            var persistence = sp.GetRequiredService<IGotrueSessionPersistence<Session>>();
            client.Auth.SetPersistence(persistence);
            return client;
        });
        services.AddSingleton<SupabaseAuthService>();
        services.AddSingleton<SupabaseChoreService>();
        services.AddSingleton<SupabaseDomainService>();
        services.AddSingleton<SupabaseChoreDomainService>();

        services.AddScoped<AuthenticationStateProvider, SupabaseAuthStateProvider>();

        return services.AddOptions<SupabaseClientOptions>().ValidateDataAnnotations();
    }
}
