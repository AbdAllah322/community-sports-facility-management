using System;
using CommunitySports.Plugins.BLL;
using CommunitySports.Plugins.Constants;
using Microsoft.Xrm.Sdk;

namespace CommunitySports.Plugins.Plugins.Booking
{
    public class ValidateFacilityAvailabilityPlugin : IPlugin
    {
        private const string PreImageName = "PreImage";

        public void Execute(IServiceProvider serviceProvider)
        {
            var context =
                (IPluginExecutionContext)serviceProvider.GetService(
                    typeof(IPluginExecutionContext));

            var tracingService =
                (ITracingService)serviceProvider.GetService(
                    typeof(ITracingService));

            if (!context.InputParameters.Contains("Target"))
                return;

            if (!(context.InputParameters["Target"] is Entity target))
                return;

            if (target.LogicalName != SchemaNames.Booking)
                return;

            Entity preImage = null;

            if (context.PreEntityImages.Contains(PreImageName))
            {
                preImage =
                    context.PreEntityImages[PreImageName];
            }

            var facility =
                GetValue<EntityReference>(
                    target,
                    preImage,
                    SchemaNames.BookingFields.Facility);

            // Draft bookings created from the Opportunity may not have a facility yet.
            if (facility == null)
                return;

            var bookingStatus =
                GetValue<OptionSetValue>(
                    target,
                    preImage,
                    SchemaNames.BookingFields.BookingStatus);

            // Cancellation must remain possible even when the facility is under maintenance.
            if (bookingStatus != null &&
                bookingStatus.Value ==
                    SchemaNames.BookingStatus.Cancelled)
            {
                return;
            }

            var serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(
                    typeof(IOrganizationServiceFactory));

            var service =
                serviceFactory.CreateOrganizationService(
                    context.UserId);

            var facilityBLL =
                new FacilityBLL(service);

            tracingService.Trace(
                "Checking facility availability for {0}.",
                facility.Id);

            if (facilityBLL.IsUnderMaintenance(facility.Id))
            {
                throw new InvalidPluginExecutionException(
                    "The selected facility is currently under maintenance and cannot be booked.");
            }

            tracingService.Trace(
                "Facility is available for booking.");
        }

        private T GetValue<T>(
            Entity target,
            Entity preImage,
            string attributeName)
        {
            if (target != null &&
                target.Attributes.Contains(attributeName))
            {
                return target.GetAttributeValue<T>(
                    attributeName);
            }

            if (preImage != null &&
                preImage.Attributes.Contains(attributeName))
            {
                return preImage.GetAttributeValue<T>(
                    attributeName);
            }

            return default(T);
        }
    }
}