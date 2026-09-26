using System;
using CommunitySports.Plugins.BLL;
using CommunitySports.Plugins.Constants;
using Microsoft.Xrm.Sdk;

namespace CommunitySports.Plugins.Plugins.Booking
{
    public class CalculateBookingPricePlugin : IPlugin
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

            // Update only contains changed fields, so the pre-image provides the remaining booking values.
            if (context.PreEntityImages.Contains(PreImageName))
            {
                preImage =
                    context.PreEntityImages[PreImageName];
            }

            tracingService.Trace(
                "CalculateBookingPricePlugin started.");

            var bookingBLL =
                new BookingBLL(
                    service,
                    tracingService);

            bookingBLL.CalculatePrice(
                target,
                preImage);

            tracingService.Trace(
                "CalculateBookingPricePlugin completed.");
        }
    }
}