using System;
using CommunitySports.Plugins.BLL;
using CommunitySports.Plugins.Constants;
using Microsoft.Xrm.Sdk;

namespace CommunitySports.Plugins.Plugins.Booking
{
    public class ValidateBookingDatesPlugin : IPlugin
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

            var serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(
                    typeof(IOrganizationServiceFactory));

            var service =
                serviceFactory.CreateOrganizationService(
                    context.UserId);

            Entity preImage = null;

            // Update may contain only one of the date fields.
            if (context.PreEntityImages.Contains(PreImageName))
            {
                preImage =
                    context.PreEntityImages[PreImageName];
            }

            tracingService.Trace(
                "ValidateBookingDatesPlugin started.");

            var bookingBLL =
                new BookingBLL(
                    service,
                    tracingService);

            bookingBLL.ValidateBookingDates(
                target,
                preImage);

            tracingService.Trace(
                "ValidateBookingDatesPlugin completed.");
        }
    }
}