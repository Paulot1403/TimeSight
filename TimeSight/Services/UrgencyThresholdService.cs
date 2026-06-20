using Microsoft.JSInterop;
using TimeSight.Models;

namespace TimeSight.Services;

/// <summary>
/// Persiste le seuil d'urgence (<see cref="Chore.DaysBeforeEmergencyStart"/>) choisi par l'utilisateur dans le localStorage du navigateur.
/// </summary>
public class UrgencyThresholdService
{
    private const string StorageKey = "timesight.urgencyThresholdDays";
    private readonly IJSInProcessRuntime _js;

    public UrgencyThresholdService(IJSRuntime jsRuntime)
    {
        // In Blazor WASM, IJSRuntime is WebAssemblyJSRuntime which implements IJSInProcessRuntime
        _js = (IJSInProcessRuntime)jsRuntime;
    }

    public void LoadFromLocalStorage()
    {
        var stored = _js.Invoke<string?>("localStorage.getItem", StorageKey);
        if (int.TryParse(stored, out var days) && Chore.EmergencyThresholdOptionsDays.Contains(days))
            Chore.DaysBeforeEmergencyStart = days;
    }

    public void SetThresholdDays(int days)
    {
        Chore.DaysBeforeEmergencyStart = days;
        _js.InvokeVoid("localStorage.setItem", StorageKey, days.ToString());
    }
}
