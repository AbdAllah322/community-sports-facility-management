namespace CommunitySports.Plugins.Constants
{
    public static class SchemaNames
    {
        public const string Booking = "as_booking";
        public const string Facility = "as_facility";
        public const string Contact = "contact";

        public static class BookingFields
        {
            public const string Facility = "as_facility";
            public const string Member = "as_member";
            public const string StartTime = "as_starttime";
            public const string EndTime = "as_endtime";
            public const string TotalPrice = "as_totalprice";
            public const string BookingStatus = "statuscode";
        }

        public static class FacilityFields
        {
            public const string HourlyRate = "as_hourlyrate";
            public const string Capacity = "as_capacity";
            public const string State = "statecode";
            public const string StatusReason = "statuscode";
        }

        public static class FacilityState
        {
            public const int Active = 0;
            public const int Inactive = 1;
        }

        public static class FacilityStatusReason
        {
            public const int UnderMaintenance = 2;
        }

        public static class ContactFields
        {
            public const string ActiveMembership = "as_activemembership";
        }

        public static class BookingStatus
        {
            public const int Draft = 1;
            public const int Cancelled = 120980001;
            public const int Booked = 120980002;
        }

        public static class EnvironmentVariables
        {
            public const string MembershipDiscountPercentage =
                "as_MembershipDiscountPercentage";
        }
    }
}