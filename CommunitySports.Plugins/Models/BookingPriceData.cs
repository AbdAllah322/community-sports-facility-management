using System;

namespace CommunitySports.Plugins.Models
{
    public class BookingPriceData
    {
        public Guid FacilityId { get; set; }

        public Guid MemberId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}