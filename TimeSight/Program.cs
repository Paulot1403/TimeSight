using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using TimeSight;
using TimeSight.repositories;
using TimeSight.Services;
using TimeSight.SupabaseClient;
using TimeSight.SupabaseClient.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddHttpClient("MicrosoftGraph", client =>
{
    client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
});
builder.Services.AddAuthorizationCore();
builder.Services.AddRadzenComponents();

builder.Services.AddSingleton<IChoreRepository, SupabaseChoreRepository>();
builder.Services.AddSingleton<IDomainRepository, SupabaseDomainRepository>();
builder.Services.AddSingleton<IChoreDomainRepository, SupabaseChoreDomainRepository>();
builder.Services.AddSingleton<IWorkspaceRepository, SupabaseWorkspaceRepository>();
builder.Services.AddSingleton<OrderChoresService>();
builder.Services.AddSingleton<ChoreDomainService>();
builder.Services.AddSingleton<MicrosoftGraphService>();
builder.Services.AddSingleton<OutlookSyncService>();
builder.Services.AddSingleton<ChoreService>();
builder.Services.AddSingleton<WorkspaceState>();

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