using System;
using newhomepage.Shared.Services;
using Xunit;

namespace newhomepage.Tests
{
    public class EventParserEdgeTests
    {
        private readonly EventParser _parser = new EventParser();

        [Fact]
        public void ParseDateString_WithSlashes_InterpretedAsMonthDay()
        {
            var today = new DateTime(2025, 6, 1);
            var result = _parser.ParseDateString("8/20", today);
            Assert.NotNull(result);
            Assert.Equal(new DateTime(2025, 8, 20), result.Value);
        }

        [Fact]
        public void ParseDateString_LeapDay_HandlesNonLeapYear_ByReturningNull()
        {
            var today = new DateTime(2025, 1, 1); // 2025 is not a leap year
            var result = _parser.ParseDateString("02-29", today);
            Assert.Null(result);
        }

        [Fact]
        public void ParseDateString_LeapDay_InLeapYear_ReturnsDate()
        {
            var today = new DateTime(2024, 1, 1); // 2024 is leap year
            var result = _parser.ParseDateString("02-29", today);
            Assert.NotNull(result);
            Assert.Equal(new DateTime(2024, 2, 29), result.Value);
        }

        [Fact]
        public void ParseDateString_Empty_ReturnsNull()
        {
            var today = DateTime.Today;
            Assert.Null(_parser.ParseDateString("", today));
            Assert.Null(_parser.ParseDateString(null!, today));
        }

        [Fact]
        public void GetNextOccurrence_Today_ReturnsTodayIfNotPast()
        {
            var today = new DateTime(2025, 8, 20);
            var next = _parser.GetNextOccurrence(8, 20, today);
            Assert.Equal(new DateTime(2025, 8, 20), next);
        }

        [Fact]
        public void TryParseMonthDay_HandlesWhitespace()
        {
            Assert.True(_parser.TryParseMonthDay(" 07-04 ", out var m, out var d));
            Assert.Equal(7, m);
            Assert.Equal(4, d);
        }

        [Fact]
        public void ParsePersonalEvents_HandlesEmptyArray()
        {
            var today = new DateTime(2025, 11, 28);
            var list = _parser.ParsePersonalEvents("[]", today);
            Assert.Empty(list);
        }

        [Fact]
        public void ParsePersonalEvents_SkipsInvalidEntries()
        {
            var today = new DateTime(2025, 11, 28);
            var json =
                "[ { \"Event\": \"X\", \"Date\": \"invalid\" }, { \"Event\": \"Y\", \"Date\": \"12-31\" } ]";
            var list = _parser.ParsePersonalEvents(json, today);
            Assert.Single(list);
            Assert.Equal("Y", list[0].Event);
        }
    }
}
