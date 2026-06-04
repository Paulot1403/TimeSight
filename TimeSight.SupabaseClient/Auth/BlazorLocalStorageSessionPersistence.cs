using Microsoft.JSInterop;
using Newtonsoft.Json;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace TimeSight.SupabaseClient.Auth;

public class BlazorLocalStorageSessionPersistence : IGotrueSessionPersistence<Session>
{
    private const string StorageKey = "timesight.session";
    private readonly IJSInProcessRuntime _js;

    public BlazorLocalStorageSessionPersistence(IJSRuntime jsRuntime)
    {
        // In Blazor WASM, IJSRuntime is WebAssemblyJSRuntime which implements IJSInProcessRuntime
        _js = (IJSInProcessRuntime)jsRuntime;
    }

    public void SaveSession(Session session)
    {
        var json = JsonConvert.SerializeObject(session);
        _js.InvokeVoid("localStorage.setItem", StorageKey, json);
    }

    public void DestroySession()
    {
        _js.InvokeVoid("localStorage.removeItem", StorageKey);
    }

    public Session? LoadSession()
    {
        var json = _js.Invoke<string?>("localStorage.getItem", StorageKey);
        if (string.IsNullOrEmpty(json)) return null;
        return JsonConvert.DeserializeObject<Session>(json);
    }
}
