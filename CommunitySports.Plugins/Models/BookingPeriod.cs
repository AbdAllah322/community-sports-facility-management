using System;

namespace CommunitySports.Plugins.Models
{
    public class BookingPeriod
    {
        public Guid BookingId { get; set; }

        public Guid FacilityId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int? StatusReason { get; set; }
    }
}