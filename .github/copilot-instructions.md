## Quick orientation

This repository is a small Blazor WebAssembly single-page app (TargetFramework: net8.0). Key entry points:

- `Program.cs` — root component registration and DI. HttpClient is registered here with BaseAddress set to `builder.HostEnvironment.BaseAddress`.
- `App.razor` — router and default layout.
- `Layout/MainLayout.razor` and `Layout/NavMenu.razor` — page shell and navigation patterns.
  -- `Pages/Home.razor` — main app UI: weather, countdowns, JS interop. Data models were moved to `Shared/Models/Models.cs` for reuse.
- `wwwroot/` — static assets (CSS under `wwwroot/css`, `index.html`, and `wwwroot/data/events.json`).

When editing UI/behavior, prefer `Pages/*.razor` and the layout files; when editing static content (images, JSON) update `wwwroot/` directly.

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

## Project-specific conventions

- Models are often declared inline in `Pages/Home.razor` (see classes like `WeatherData`, `CountdownEvent`, etc.). When adding or reusing models prefer creating a new `Shared/Models` or `Models` file only if the type will be reused across pages — otherwise keeping them next to consumers keeps the project simple.
- Scoped CSS: component css lives as `.razor.css` next to components (see `Layout/MainLayout.razor.css` and `Layout/NavMenu.razor.css`). Build output includes scoped css in `obj/` during compilation.
- Nullability: project uses `<Nullable>enable</Nullable>` — follow nullable reference conventions when editing (use `?` and null checks where appropriate).

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
