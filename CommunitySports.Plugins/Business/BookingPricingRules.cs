using System;

namespace CommunitySports.Plugins.Business
{
    public static class BookingPricingRules
    {
        public static decimal CalculateTotalPrice(
            DateTime startTime,
            DateTime endTime,
            decimal hourlyRate,
            bool hasActiveMembership,
            decimal discountPercentage)
        {
            var duration = endTime - startTime;
            var durationHours = (decimal)duration.TotalHours;

            var basePrice = durationHours * hourlyRate;

            if (!hasActiveMembership)
                return basePrice;

            var discountAmount =
                basePrice * discountPercentage / 100m;

            return basePrice - discountAmount;
        }
    }
}