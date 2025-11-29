using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using newhomepage.Shared.Models;

namespace newhomepage.Shared.Services
{
    // Small helper service to parse personal events (MM-DD or YYYY-MM-DD) into CountdownEvent instances.
    // Public methods are intentionally small and unit-testable.
    public class EventParser
    {
        // Parse the JSON array of PersonalEvent entries and resolve each to its next occurrence relative to 'today'.
        public List<newhomepage.Shared.Models.CountdownEvent> ParsePersonalEvents(
            string json,
            DateTime today
        )
        {
            var results = new List<newhomepage.Shared.Models.CountdownEvent>();

            if (string.IsNullOrWhiteSpace(json))
                return results;

            List<PersonalEvent>? personalEventsRaw;
            try
            {
                personalEventsRaw = JsonSerializer.Deserialize<List<PersonalEvent>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch
            {
                return results;
            }

            if (personalEventsRaw == null)
                return results;

            foreach (var pe in personalEventsRaw)
            {
                try
                {
                    var resolved = ParseDateString(pe.Date, today);
                    if (resolved != null)
                    {
                        results.Add(
                            new newhomepage.Shared.Models.CountdownEvent
                            {
                                Event = pe.Event,
                                Date = resolved.Value,
                                IsHoliday = false,
                            }
                        );
                    }
                }
                catch
                {
                    // skip invalid entries
                }
            }

            return results;
        }

        // Parse a single date string which may be full ISO (YYYY-MM-DD) or month-day (MM-DD or MM/DD).
        // Returns next occurrence DateTime relative to 'today', or null if parse fails.
        public DateTime? ParseDateString(string dateString, DateTime today)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            // If the string looks like a month-day (e.g. "MM-DD" or "M/D"), prefer month-day parsing
            char sep = dateString.Contains('-') ? '-' : (dateString.Contains('/') ? '/' : '\0');
            if (sep != '\0')
            {
                var parts = dateString.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    if (
                        !int.TryParse(parts[0], out var month)
                        || !int.TryParse(parts[1], out var day)
                    )
                        return null;

                    try
                    {
                        var candidate = new DateTime(today.Year, month, day);
                        if (candidate < today)
                            candidate = candidate.AddYears(1);
                        return candidate;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            // Otherwise try a full date parse (ISO or other), which includes a year
            if (DateTime.TryParse(dateString, out var dt))
            {
                return dt.Date;
            }
            return null;
        }

        // Try parse month and day out of string (MM-DD or MM/DD), returns false on failure.
        public bool TryParseMonthDay(string s, out int month, out int day)
        {
            month = 0;
            day = 0;
            if (string.IsNullOrWhiteSpace(s))
                return false;
            char sep = s.Contains('-') ? '-' : (s.Contains('/') ? '/' : '\0');
            if (sep == '\0')
                return false;
            var parts = s.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                return false;
            return int.TryParse(parts[0], out month) && int.TryParse(parts[1], out day);
        }

        // Utility to compute the next occurrence (this year or next) for a given month/day.
        public DateTime GetNextOccurrence(int month, int day, DateTime today)
        {
            var candidate = new DateTime(today.Year, month, day);
            if (candidate < today)
                candidate = candidate.AddYears(1);
            return candidate;
        }
    }
}
