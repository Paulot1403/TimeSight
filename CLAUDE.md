# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run the app (from repo root or TimeSight/ directory)
dotnet run --project TimeSight

# Build only
dotnet build

# Build a specific project
dotnet build TimeSight.SupabaseClient
```

There are no tests in this project.

## Architecture

Two projects:

**`TimeSight/`** — Blazor WebAssembly frontend (net10.0)
**`TimeSight.SupabaseClient/`** — Class library wrapping the Supabase SDK

### Authentication flow

`SupabaseAuthStateProvider` (in `TimeSight.SupabaseClient/Auth/`) implements Blazor's `AuthenticationStateProvider`. Session is persisted to `localStorage` via `BlazorLocalStorageSessionPersistence`. On startup, `Program.cs` manually calls `supabase.Auth.LoadSession()` and `RetrieveSessionAsync()` to restore the session before the app starts.

The auth routes live under `/authentication/{action}` (`login`, `logout`, `callback`, `logged-out`). GitHub OAuth uses PKCE — the verifier is stored in `sessionStorage` and consumed in the `callback` action.

Unauthenticated users are redirected by `RedirectToLogin` (rendered from `App.razor`'s `<NotAuthorized>` slot). `CascadingUserId` injects the authenticated user's `Guid` into the component tree as `[CascadingParameter(Name = "UserId")]`.

### Data layer

Repository pattern: `IChoreRepository`, `IDomainRepository`, `IChoreDomainRepository` are registered as singletons in `Program.cs`, with Supabase implementations in `TimeSight/repositories/`.

Domain models (`TimeSight/Models/`) are separate from Supabase models (`TimeSight.SupabaseClient/Models/`). Extension methods in `TimeSight/Extensions/` convert between them (e.g. `SupabaseChoreExtensions`, `DomainExtensions`).

### Styling

Bootstrap 5 is the primary styling framework (`wwwroot/lib/bootstrap/`, loaded in `index.html`). Dark mode is enabled via `data-bs-theme="dark"` on `<html>`. Bootstrap's dark theme variables are overridden in `wwwroot/css/app.css` to match the custom "Midnight Precision" palette (e.g. `--bs-card-bg`, `--bs-dropdown-bg`, `--bs-primary`). Button variants use Bootstrap's `--bs-btn-*` CSS variable overrides rather than custom classes — `btn-outline-secondary` is the ghost/secondary button, `btn-outline-danger` is the delete button.

Barlow / Barlow Semi Condensed fonts (loaded from Google Fonts) are applied on headings and labels via CSS overrides.

**What Bootstrap handles:** buttons, cards (`.card .card-body`), forms (`.form-control`, `.form-label`, `.mb-3`), dropdowns, flex layout utilities (`d-flex`, `gap-*`, `min-vh-100`, etc.), alerts (`.alert-danger` for login errors).

**What stays custom in `app.css`:** chore cards (`.chore-card` with hover animation and accent bar), filter pills (`.filter-pill`), domain tags (`.domain-tag`), the circular checkbox style on `.form-check-input`, the custom range slider, the nav brand mark, and the login page background gradient.

Scoped CSS files `MainLayout.razor.css` and `NavMenu.razor.css` handle the sidebar layout, sticky top bar, and nav link active states. The desktop collapse override (`display: block !important` at ≥641px) is necessary because Bootstrap's `.collapse` uses `!important` — Blazor applies/removes it conditionally for mobile toggle.

`EmptyLayout.razor` (bare `@Body`) is used by the authentication pages to skip the sidebar.

### Navigation

`NavigationManagerExtensions.NavigateToRelative()` (in `TimeSight/Extensions/`) builds relative URLs from `Navigation.BaseUri` and is used throughout pages instead of hardcoded paths.
