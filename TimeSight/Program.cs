using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TimeSight;
using TimeSight.SupabaseClient;
using TimeSight.SupabaseClient.Auth;
using TimeSight.SupabaseClient.Options;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddAuthorizationCore();

builder.Services.AddSupabase().Configure(options =>
{
    builder.Configuration.Bind("Supabase", options);
});

var host = builder.Build();
await host.Services.GetRequiredService<Supabase.Client>().InitializeAsync();
await host.RunAsync();
