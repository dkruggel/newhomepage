using System;
using System.Text.Json.Serialization;

namespace newhomepage.Shared.Models
{
    public class AppSettings
    {
        // Default location (e.g., "San Francisco, US" or postal code)
        [JsonPropertyName("defaultLocation")]
        public string? DefaultLocation { get; set; }

        // Number of days to mark an event as urgent
        [JsonPropertyName("eventUrgentThresholdDays")]
        public int? EventUrgentThresholdDays { get; set; }

        // Month-day string (MM-DD) after which next-year holidays are also fetched
        [JsonPropertyName("nextYearHolidaysIncludeAfter")]
        public string? NextYearHolidaysIncludeAfter { get; set; }
    }
}
