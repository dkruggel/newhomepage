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

## Project-specific conventions

- Models are often declared inline in `Pages/Home.razor` (see classes like `WeatherData`, `CountdownEvent`, etc.). When adding or reusing models prefer creating a new `Shared/Models` or `Models` file only if the type will be reused across pages — otherwise keeping them next to consumers keeps the project simple.
- Scoped CSS: component css lives as `.razor.css` next to components (see `Layout/MainLayout.razor.css` and `Layout/NavMenu.razor.css`). Build output includes scoped css in `obj/` during compilation.
- Nullability: project uses `<Nullable>enable</Nullable>` — follow nullable reference conventions when editing (use `?` and null checks where appropriate).

## Tests and code quality

- Unit tests live in `tests/newhomepage.Tests`. Run them with `dotnet test` from the repo root. The `EventParser` service has focused tests that validate parsing of MM-DD and ISO dates.
- Quick checks: run `dotnet build` before pushing changes. If you change shared models, update any affected components and run `dotnet test`.

## Making small interactive changes (apply immediately)

- When adding or editing settings in `Pages/Settings.razor`, call `dispatchAppSettings(json)` (JS) or write to `localStorage` and then invoke the static JSInvokable `OnAppSettingsChangedFromJs` from JS to make the client apply changes immediately.
- The client will re-run weather and events loads when it receives the settings-changed notification. This avoids a full page reload during interactive edits.

## Security & secrets (must fix)

- The app previously contained a hard-coded weather API key. The code now reads `Weather:ApiKey` from `wwwroot/appsettings.json` (sample file added).
- IMPORTANT: a client-side app cannot keep secrets safe. For production, prefer a server-side proxy or API that keeps secrets off the client. Use the client key only for public / limited-scope keys and rotate regularly.

Example `wwwroot/appsettings.json` (already present in the repo):

```json
{
  "Weather": { "ApiKey": "" }
}
```

Set the key locally for development only and do not commit secrets. For CI or deploy, wire the key into a secure server-side configuration or secret store and avoid shipping it with the WebAssembly bundle.

## Common maintenance tasks

- Update static events: edit `wwwroot/data/events.json` (JSON array of objects {Event, Date}). The app reads this at runtime via relative path.
- Update static events: edit `wwwroot/data/events.json` (JSON array of objects {Event, Date}). Dates may be month-day only ("MM-DD") for recurring annual events; the app will compute the next occurrence automatically. The app also accepts full ISO dates (`YYYY-MM-DD`) if you need a one-off date.
- Change base paths / static assets: edit `wwwroot/index.html` and `wwwroot/css/app.css`.
- Add new pages: add a `.razor` under `Pages/` and add routes with `@page "/yourroute"`. The router uses `App.razor`.

## When making changes, check these quick validations

1. Run `dotnet build` — compilation should succeed.
2. Run `dotnet run` and confirm the app loads in the browser and the console shows no unhandled exceptions for the changed area.
3. For data changes (events.json), verify the UI shows updated items without a rebuild by refreshing the browser (dev server serves static files).

## Do not assume

- Do not assume backend APIs are available in tests or CI — they are live public endpoints. If you need to make the app testable offline, mock `HttpClient` responses or isolate API calls behind an interface so tests can inject a fake.

## Example snippets to search for when editing

- `HttpClient { BaseAddress }` — locate how requests are formed (`Program.cs`).
- `Http.GetStringAsync("data/events.json")` — locate local JSON data loading (`Pages/Home.razor`).
- `JSRuntime.InvokeAsync` / `eval` — find JS interop usage (`Pages/Home.razor`).

---

If anything above is unclear or you want this file to cover more (CI, publish, or a style guide), tell me which area to expand and I'll update the instructions.
