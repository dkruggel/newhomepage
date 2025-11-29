using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

var app = builder.Build();

app.MapGet(
    "/api/weather",
    async (string location, IHttpClientFactory httpFactory) =>
    {
        var apiKey =
            Environment.GetEnvironmentVariable("WEATHER_API_KEY")
            ?? builder.Configuration["Weather:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            return Results.Problem("Server weather API key not configured.", statusCode: 500);
        }

        try
        {
            var client = httpFactory.CreateClient();
            var url =
                $"https://api.weatherapi.com/v1/forecast.json?key={WebUtility.UrlEncode(apiKey)}&q={WebUtility.UrlEncode(location)}&days=6&aqi=no&alerts=no";
            var resp = await client.GetStringAsync(url);
            return Results.Content(resp, "application/json");
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
);

app.Run();
