using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace newhomepage.Shared.Models
{
    public class WeatherData
    {
        [JsonPropertyName("location")]
        public Location Location { get; set; } = new();

        [JsonPropertyName("current")]
        public Current Current { get; set; } = new();

        [JsonPropertyName("forecast")]
        public Forecast Forecast { get; set; } = new();
    }

    public class Location
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("region")]
        public string Region { get; set; } = "";
    }

    public class Current
    {
        [JsonPropertyName("temp_f")]
        public double Temp_F { get; set; } = 0.0;

        [JsonPropertyName("condition")]
        public Condition Condition { get; set; } = new();
    }

    public class Forecast
    {
        [JsonPropertyName("forecastday")]
        public List<ForecastDay> ForecastDays { get; set; } = new();
    }

    public class Condition
    {
        [JsonPropertyName("text")]
        public string Description { get; set; } = "";

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = "";
    }

    public class ForecastDay
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = "";

        [JsonPropertyName("day")]
        public Day Day { get; set; } = new();
    }

    public class Day
    {
        [JsonPropertyName("maxtemp_f")]
        public double High { get; set; }

        [JsonPropertyName("mintemp_f")]
        public double Low { get; set; }

        [JsonPropertyName("condition")]
        public Condition Condition { get; set; } = new();
    }

    public class CountdownEvent
    {
        public string Event { get; set; } = "";
        public DateTime Date { get; set; }
        public bool IsHoliday { get; set; } = false;
    }

    public class HolidayData
    {
        public DateTime Date { get; set; }
        public string LocalName { get; set; } = "";

        [JsonPropertyName("name")]
        public string Event { get; set; } = "";
        public string CountryCode { get; set; } = "";
        public bool Fixed { get; set; }
        public bool Global { get; set; }
    }

    public class LocationCoords
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class GeocodeResult
    {
        public string City { get; set; } = "";
        public string CountryCode { get; set; } = "";
        public string CountryName { get; set; } = "";
    }

    public class IPLocationResult
    {
        public string City { get; set; } = "";
        public string CountryCode { get; set; } = "";
        public string CountryName { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Postal { get; set; } = "";
    }

    // Helper type for deserializing personal events which may omit the year (MM-DD)
    public class PersonalEvent
    {
        public string Event { get; set; } = "";
        public string Date { get; set; } = ""; // e.g. "01-28" or full ISO "2026-01-28"
    }
}
