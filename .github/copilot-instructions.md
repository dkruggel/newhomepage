## Quick orientation

This repository is a Blazor WebAssembly single-page app (TargetFramework: net8.0).

Key entry points

- `Program.cs` — root component registration and DI. `HttpClient` is registered here with BaseAddress set to `builder.HostEnvironment.BaseAddress`.
- `App.razor` — router and default layout.
- `Layout/MainLayout.razor` and `Layout/NavMenu.razor` — page shell and navigation patterns.
  - `Pages/Home.razor` — main app UI: weather, countdowns, JS interop. Shared models live in `Shared/Models`.
- `wwwroot/` — static assets (CSS under `wwwroot/css`, `index.html`, and `wwwroot/data/events.json`).

When editing UI/behavior, prefer `Pages/*.razor` and the layout files; when editing static content (images, JSON) update `wwwroot/` directly.

---

## Runtime keys, notification hooks, and contracts

- `app-settings` (localStorage key): JSON serialized `AppSettings` (see `Shared/Models/Settings.cs`).

  - Shape (fields you'll see):
    - `DefaultLocation` (string)
    - `EventUrgentThresholdDays` (int?)
    - `NextYearHolidaysIncludeAfter` (MM-DD string)
  - Usage: the app reads `app-settings` on startup and when signaled. To persist and immediately apply settings from JavaScript, use `dispatchAppSettings(json)` (defined in `wwwroot/index.html`).

- JS -> C# bridge (notification flow):
  - `dispatchAppSettings(json)` writes to localStorage and attempts to call the static JSInvokable `newhomepage.Shared.Services.SettingsBridge.OnAppSettingsChangedFromJs`.
  - On C#, `SettingsBridge.OnAppSettingsChangedFromJs` raises `SettingsChanged` (an event). Consumers (e.g., `Pages/Home.razor`) subscribe and rehydrate settings via `JsonSerializer.Deserialize<AppSettings>(json)` and re-run dependent loads.

Contract summary

- Caller: must pass a JSON string matching `AppSettings` to `dispatchAppSettings`.
- Consumer: `SettingsChanged` handlers should validate and apply settings defensively (guard against malformed JSON).

---

## How to make settings apply immediately (practical steps)

1. Serialize your settings object to JSON.
2. Call `dispatchAppSettings(json)` from JS or call it using `IJSRuntime` from C#.
   - This writes to `localStorage['app-settings']` and tries to notify Blazor.
3. The app subscribes to `SettingsBridge.SettingsChanged` and reloads settings and dependent data (weather, events) automatically.

If Blazor hasn't initialized, `dispatchAppSettings` still writes to localStorage so the app will pick up the changes on next load.

---

## Important patterns & examples

- Local JSON data: use `HttpClient` with relative paths. Example: `await Http.GetStringAsync("data/events.json")`.
- JS interop: small helpers are registered in `OnAfterRenderAsync` with `IJSRuntime` (see `Home.razor`). If you rename a helper, update both the JS and C# calls.
- External APIs (used by the client):
  - Weather: `https://api.weatherapi.com/v1/forecast.json?key={apiKey}&q={currentLocation}&days=6`
    - Client reads `Weather:ApiKey` in `wwwroot/appsettings.json`. For production, use a server-side proxy.
  - Holidays: `https://date.nager.at/api/v3/publicholidays/{year}/US`
  - Reverse geocode: `https://api.bigdatacloud.net/...`

---

## Build / run / debug (concrete)

- Local dev run (repo root):

```bash
dotnet run
```

- Fast-edit loop: `dotnet watch run`.
- Debugging: use browser DevTools for client errors and `Console.WriteLine` for logs. Attach VS Code/Visual Studio for client debugging.

---

## Optional server-side proxy (weather)

There is a minimal `Server/` project you can run separately to proxy the Weather API.

- Run server locally (if present):

```bash
dotnet run --project Server/Server.csproj
```

- The proxy expects `WEATHER_API_KEY` as an environment variable (or modify server config). It exposes `/api/weather?location=...` which the client will use when `Weather:ApiKey` is not configured.

Note: the Server project is optional and intentionally excluded from client build; run it separately when you need a secure key for development.

---

## Tests, CI and quality gates

- Unit tests: `tests/newhomepage.Tests` — run with:

```bash
dotnet test
```

- Pre-commit checks (recommended):
  1. `dotnet build`

2.  `dotnet test`
3.  Smoke-run the app (`dotnet run`) and verify the changed area in the browser.

- CI notes:
  - Do not store secrets in the repo. Put secrets in CI secret storage and inject them into any server runtime.
  - If you need to run integration tests that call external APIs, either mock the HTTP calls or restrict those tests to a gated pipeline that has the necessary credentials.

---

## Security & secrets (must follow)

- Do NOT commit API keys or secrets to `wwwroot/appsettings.json` or any source file.
- For development only you may use `wwwroot/appsettings.json` with an empty key; for production, configure keys server-side or in CI.

Example `wwwroot/appsettings.json` (safe example):

```json
{
  "Weather": { "ApiKey": "" }
}
```

---

## Project conventions and developer guidance

- Models: shared types should go in `Shared/Models/*.cs`. Small page-only DTOs may remain inline.
- Scoped CSS: component CSS goes in `Component.razor.css` beside the component. Global styles live in `wwwroot/css/app.css`.
- Nullability: project enables `<Nullable>enable</Nullable>` — follow nullable annotations and guard nulls.
- Accessibility: prefer `aria-live`, roles, and keyboard focus handling for dynamic UI; code already uses an `aria-live` region and accessible toasts.

---

## Troubleshooting & gotchas

- Razor parser issues: literal `<` characters or HTML-like text in Razor can break compilation. Escape or use `@Html.Raw` carefully.
- localStorage: guard every localStorage call — some environments (private mode) may throw.
- JS interop timing: `DotNet.invokeMethodAsync` fails if Blazor hasn't started; `dispatchAppSettings` writes to storage first to avoid lost updates.
- External API rate limits: implement caching on the client (weather cache exists) or use a server-side cache when scaling.

---

## PR checklist

- Run `dotnet build` and `dotnet test` locally.
- Add unit tests for parsing / business logic changes (see `EventParser` tests as an example).
- Avoid adding secrets. Document any env vars required to run the server proxy.

---

If you want this doc to include CI YAML snippets, publishing/publish profiles, or a coding style guide, tell me which area to expand and I will add it.

## Quick orientation

This repository is a small Blazor WebAssembly single-page app (TargetFramework: net8.0). Key entry points:

- `Program.cs` — root component registration and DI. HttpClient is registered here with BaseAddress set to `builder.HostEnvironment.BaseAddress`.
- `App.razor` — router and default layout.
- `Layout/MainLayout.razor` and `Layout/NavMenu.razor` — page shell and navigation patterns.
  -- `Pages/Home.razor` — main app UI: weather, countdowns, JS interop. Data models were moved to `Shared/Models/Models.cs` for reuse.
- `wwwroot/` — static assets (CSS under `wwwroot/css`, `index.html`, and `wwwroot/data/events.json`).

When editing UI/behavior, prefer `Pages/*.razor` and the layout files; when editing static content (images, JSON) update `wwwroot/` directly.

## Important runtime keys and notification hooks

- app-settings (localStorage key): JSON serialized AppSettings with the schema defined in `Shared/Models/Settings.cs`.
  - Fields: `DefaultLocation` (string), `EventUrgentThresholdDays` (int?), `NextYearHolidaysIncludeAfter` (MM-DD string).
  - The app saves and reads this key at runtime. To immediately apply settings from JavaScript, call the helper `dispatchAppSettings(json)` defined in `wwwroot/index.html`. This will both write the value to localStorage and invoke the static JSInvokable method `newhomepage.Shared.Services.SettingsBridge.OnAppSettingsChangedFromJs` which raises a C# event the app subscribes to.

## Quick tips for making settings apply immediately

- The client implements a small JS->C# bridge: `dispatchAppSettings(json)` will call into the Blazor runtime if available. When changing app-settings from code (tests or scripts), prefer calling that helper so the app updates without a full page reload.
- The C# side exposes `Shared.Services.SettingsBridge.SettingsChanged` as an event you can subscribe to. Handlers receive the raw JSON string and should rehydrate AppSettings via `JsonSerializer.Deserialize<AppSettings>(json)` then re-run any dependent loads (weather, events).

## Important patterns & examples (do this exactly)

- Relative data fetches use the app base address via `HttpClient` registered in `Program.cs`. Example: in `Home.razor` the personal events file is loaded with `await Http.GetStringAsync("data/events.json")` — editing `wwwroot/data/events.json` changes what the app reads at runtime.
- JS interop: `Home.razor` uses `IJSRuntime` and registers helper functions via `JSRuntime.InvokeVoidAsync("eval", "...")` in `OnAfterRenderAsync`. The functions used are `getUserLocation` and `promptForLocation`. If you change names or signatures, update both the JS snippet and the C# calls (`JSRuntime.InvokeAsync<T>(...)`).
- External API integrations shown in `Home.razor`:
  - Weather API: request built with `https://api.weatherapi.com/v1/forecast.json?key={apiKey}&q={currentLocation}&days=6`.
    The app now reads the key from configuration (`Weather:ApiKey`) in `wwwroot/appsettings.json`. Do NOT commit real secrets to the client; prefer a server-side proxy for private keys.
  - Holiday API: `https://date.nager.at/api/v3/publicholidays/{year}/US`
  - Reverse geocode: `https://api.bigdatacloud.net/...`
  - IP fallback: `https://ipapi.co/json/`

## Build / run / debug (concrete)

- Local dev run (recommended): from repo root run `dotnet run` (or `dotnet run --project newhomepage.csproj`). The dev server is provided by `Microsoft.AspNetCore.Components.WebAssembly.DevServer` and will print the app URL to the console. Open the URL it prints (typically https://localhost:5001 or http://localhost:5000).
- Fast-edit loop: you can use `dotnet watch run` in the project folder if you have the SDK tooling installed.
- Debugging: use browser DevTools for client-side exceptions and `Console.WriteLine`/console logs in `dotnet run` terminal output. You can also attach Visual Studio / VS Code to the Blazor WebAssembly debugging session.

## Running the optional server-side proxy (weather)

The repository includes a minimal `Server/` project that can proxy the Weather API so you don't have to embed a client key. It's optional and intentionally excluded from the client build to avoid conflicts, but you can run it separately during development:

- Build and run the server from the `Server` folder (if present): `dotnet run --project Server/Server.csproj`.
- The proxy expects a server-side environment variable `WEATHER_API_KEY` or you can modify `Program.cs` to read from appsettings. The proxy exposes `/api/weather?location=...` which the client will use automatically when no client `Weather:ApiKey` is configured.

Note: the Server project is optional. If you prefer not to use it, set `Weather:ApiKey` in `wwwroot/appsettings.json` for local dev (remember that shipping keys in client builds is insecure).
\*\*\* End Patch

## Project-specific conventions
