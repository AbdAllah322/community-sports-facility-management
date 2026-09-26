using System;
using CommunitySports.Plugins.Business;
using CommunitySports.Plugins.Constants;
using CommunitySports.Plugins.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;



namespace CommunitySports.Plugins.BLL
{
    public class BookingBLL
    {
        private readonly IOrganizationService _service;
        private readonly ITracingService _tracingService;

        public BookingBLL(
            IOrganizationService service,
            ITracingService tracingService)
        {
            _service = service;
            _tracingService = tracingService;
        }

        public void ValidateNoOverlap(
            Entity target,
            Entity preImage)
        {
            var booking = BuildBookingPeriod(target, preImage);

            if (booking == null)
                return;

            if (BookingRules.IsCancelled(booking.StatusReason))
                return;

            if (HasOverlappingBooking(booking))
            {
                throw new InvalidPluginExecutionException(
                    "The selected facility already has a booking that overlaps with this time period.");
            }
        }

        private BookingPeriod BuildBookingPeriod(
            Entity target,
            Entity preImage)
        {
            var facility = GetValue<EntityReference>(
                target,
                preImage,
                SchemaNames.BookingFields.Facility);

            var startTime = GetValue<DateTime?>(
                target,
                preImage,
                SchemaNames.BookingFields.StartTime);

            var endTime = GetValue<DateTime?>(
                target,
                preImage,
                SchemaNames.BookingFields.EndTime);

            var status = GetValue<OptionSetValue>(
                target,
                preImage,
                SchemaNames.BookingFields.BookingStatus);

            if (facility == null ||
                !startTime.HasValue ||
                !endTime.HasValue)
            {
                return null;
            }

            return new BookingPeriod
            {
                BookingId = target.Id,
                FacilityId = facility.Id,
                StartTime = startTime.Value,
                EndTime = endTime.Value,
                StatusReason = status?.Value
            };
        }

        private bool HasOverlappingBooking(BookingPeriod booking)
        {
            var query = new QueryExpression(SchemaNames.Booking)
            {
                ColumnSet = new ColumnSet(false),
                TopCount = 1
            };

            query.Criteria.AddCondition(
                SchemaNames.BookingFields.Facility,
                ConditionOperator.Equal,
                booking.FacilityId);

            query.Criteria.AddCondition(
                SchemaNames.BookingFields.StartTime,
                ConditionOperator.LessThan,
                booking.EndTime);

            query.Criteria.AddCondition(
                SchemaNames.BookingFields.EndTime,
                ConditionOperator.GreaterThan,
                booking.StartTime);

            query.Criteria.AddCondition(
                SchemaNames.BookingFields.BookingStatus,
                ConditionOperator.NotEqual,
                SchemaNames.BookingStatus.Cancelled);

            // The current record must not be compared with itself during an update.
            if (booking.BookingId != Guid.Empty)
            {
                query.Criteria.AddCondition(
                    SchemaNames.Booking + "id",
                    ConditionOperator.NotEqual,
                    booking.BookingId);
            }

            _tracingService.Trace(
                "Checking booking overlap for facility {0}.",
                booking.FacilityId);

            var result = _service.RetrieveMultiple(query);

            return result.Entities.Count > 0;
        }

        private T GetValue<T>(
            Entity target,
            Entity preImage,
            string attributeName)
        {
            if (target != null &&
                target.Attributes.Contains(attributeName))
            {
                return target.GetAttributeValue<T>(attributeName);
            }

            if (preImage != null &&
                preImage.Attributes.Contains(attributeName))
            {
                return preImage.GetAttributeValue<T>(attributeName);
            }

            return default(T);
        }

        private BookingPriceData BuildBookingPriceData(
    Entity target,
    Entity preImage)
        {
            var facility =
                GetValue<EntityReference>(
                    target,
                    preImage,
                    SchemaNames.BookingFields.Facility);

            var member =
                GetValue<EntityReference>(
                    target,
                    preImage,
                    SchemaNames.BookingFields.Member);

            var startTime =
                GetValue<DateTime?>(
                    target,
                    preImage,
                    SchemaNames.BookingFields.StartTime);

            var endTime =
                GetValue<DateTime?>(
                    target,
                    preImage,
                    SchemaNames.BookingFields.EndTime);

            if (facility == null ||
                member == null ||
                !startTime.HasValue ||
                !endTime.HasValue)
            {
                return null;
            }

            return new BookingPriceData
            {
                FacilityId = facility.Id,
                MemberId = member.Id,
                StartTime = startTime.Value,
                EndTime = endTime.Value
            };
        }


        public void CalculatePrice(
        Entity target,
        Entity preImage)
        {
            var booking =
                BuildBookingPriceData(
                    target,
                    preImage);

            if (booking == null)
                return;

            // Date validation is handled separately by the booking form requirement.
            if (booking.EndTime <= booking.StartTime)
                return;

            var facility =
                _service.Retrieve(
                    SchemaNames.Facility,
                    booking.FacilityId,
                    new ColumnSet(
                        SchemaNames.FacilityFields.HourlyRate));

            var hourlyRate =
                facility.GetAttributeValue<Money>(
                    SchemaNames.FacilityFields.HourlyRate);

            if (hourlyRate == null)
            {
                throw new InvalidPluginExecutionException(
                    "The selected facility does not have an hourly rate.");
            }

            var member =
                _service.Retrieve(
                    SchemaNames.Contact,
                    booking.MemberId,
                    new ColumnSet(
                        SchemaNames.ContactFields.ActiveMembership));

            var hasActiveMembership =
                member.GetAttributeValue<bool>(
                    SchemaNames.ContactFields.ActiveMembership);

            decimal discountPercentage = 0m;

            if (hasActiveMembership)
            {
                var configurationBLL =
                    new ConfigurationBLL(_service);

                discountPercentage =
                    configurationBLL.GetDecimalEnvironmentVariable(
                        SchemaNames.EnvironmentVariables
                            .MembershipDiscountPercentage);
            }

            var totalPrice =
                BookingPricingRules.CalculateTotalPrice(
                    booking.StartTime,
                    booking.EndTime,
                    hourlyRate.Value,
                    hasActiveMembership,
                    discountPercentage);

            target[SchemaNames.BookingFields.TotalPrice] =
                new Money(totalPrice);

            _tracingService.Trace(
                "Booking price calculated. Total: {0}",
                totalPrice);
        }

        public void ValidateBookingDates(
        Entity target,
        Entity preImage)
        {
            var startTime =
                GetValue<DateTime?>(
                    target,
                    preImage,
                    SchemaNames.BookingFields.StartTime);

            var endTime =
                GetValue<DateTime?>(
                    target,
                    preImage,
                    SchemaNames.BookingFields.EndTime);

            if (!startTime.HasValue || !endTime.HasValue)
                return;

            if (BookingRules.HasInvalidDateRange(
                    startTime.Value,
                    endTime.Value))
            {
                throw new InvalidPluginExecutionException(
                    "The booking end time cannot be earlier than the start time.");
            }
        }
    }
}