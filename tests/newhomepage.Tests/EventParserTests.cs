using System;
using System.Collections.Generic;
using newhomepage.Shared.Services;
using Xunit;

namespace newhomepage.Tests
{
    public class EventParserTests
    {
        private readonly EventParser _parser = new EventParser();

        [Fact]
        public void ParseDateString_FullIso_ReturnsDate()
        {
            var today = new DateTime(2025, 11, 28);
            var result = _parser.ParseDateString("2026-01-28", today);
            Assert.NotNull(result);
            Assert.Equal(new DateTime(2026, 1, 28), result.Value);
        }

        [Fact]
        public void ParseDateString_MonthDay_PastRollsToNextYear()
        {
            var today = new DateTime(2025, 11, 28);
            var result = _parser.ParseDateString("01-01", today);
            Assert.NotNull(result);
            // Jan 1 next year
            Assert.Equal(new DateTime(2026, 1, 1), result.Value);
        }

        [Fact]
        public void ParseDateString_MonthDay_SameYearIfInFuture()
        {
            var today = new DateTime(2025, 6, 1);
            var result = _parser.ParseDateString("08-20", today);
            Assert.NotNull(result);
            Assert.Equal(new DateTime(2025, 8, 20), result.Value);
        }

        [Fact]
        public void TryParseMonthDay_ValidAndInvalid()
        {
            Assert.True(_parser.TryParseMonthDay("01-28", out var m1, out var d1));
            Assert.Equal(1, m1);
            Assert.Equal(28, d1);

            Assert.True(_parser.TryParseMonthDay("7/4", out var m2, out var d2));
            Assert.Equal(7, m2);
            Assert.Equal(4, d2);

            Assert.False(_parser.TryParseMonthDay("invalid", out _, out _));
        }

        [Fact]
        public void GetNextOccurrence_RollsCorrectly()
        {
            var today = new DateTime(2025, 12, 31);
            var next = _parser.GetNextOccurrence(1, 1, today);
            Assert.Equal(new DateTime(2026, 1, 1), next);
        }

        [Fact]
        public void ParsePersonalEvents_ParsesJsonWithMMDDAndIso()
        {
            var today = new DateTime(2025, 11, 28);
            var json =
                "[ { \"Event\": \"A\", \"Date\": \"01-28\" }, { \"Event\": \"B\", \"Date\": \"2026-02-07\" } ]";
            var list = _parser.ParsePersonalEvents(json, today);
            Assert.Equal(2, list.Count);
            Assert.Contains(list, e => e.Event == "A" && e.Date == new DateTime(2026, 1, 28));
            Assert.Contains(list, e => e.Event == "B" && e.Date == new DateTime(2026, 2, 7));
        }

        [Fact]
        public void ParsePersonalEvents_InvalidJson_ReturnsEmpty()
        {
            var today = DateTime.Today;
            var list = _parser.ParsePersonalEvents("not json", today);
            Assert.Empty(list);
        }
    }
}
