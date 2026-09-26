using System;
using CommunitySports.Plugins.Constants;

namespace CommunitySports.Plugins.Business
{
    public static class BookingRules
    {
        public static bool IsCancelled(int? statusReason)
        {
            return statusReason == SchemaNames.BookingStatus.Cancelled;
        }

        public static bool HasInvalidDateRange(
            DateTime startTime,
            DateTime endTime)
        {
            return endTime < startTime;
        }
    }
}