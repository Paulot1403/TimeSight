using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TimeSight;
using TimeSight.repositories;
using TimeSight.SupabaseClient;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddAuthorizationCore();

builder.Services.AddSingleton<IChoreRepository, SupabaseChoreRepository>();
builder.Services.AddSingleton<IDomainRepository, SupabaseDomainRepository>();
builder.Services.AddSingleton<IChoreDomainRepository, SupabaseChoreDomainRepository>();

builder.Services.AddSupabase().Configure(options =>
{
    builder.Configuration.Bind("Supabase", options);
});

var host = builder.Build();

await LoadSessionFromLocalStorage(host);

await host.RunAsync();

static async Task LoadSessionFromLocalStorage(WebAssemblyHost host)
{
    var supabase = host.Services.GetRequiredService<Supabase.Client>();
    await supabase.InitializeAsync();
    supabase.Auth.LoadSession();
    await supabase.Auth.RetrieveSessionAsync();
}