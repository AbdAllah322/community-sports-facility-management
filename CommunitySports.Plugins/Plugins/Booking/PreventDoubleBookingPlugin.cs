using System;
using CommunitySports.Plugins.BLL;
using CommunitySports.Plugins.Constants;
using Microsoft.Xrm.Sdk;

namespace CommunitySports.Plugins.Plugins.Booking
{
    public class PreventDoubleBookingPlugin : IPlugin
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

            // Update only contains changed fields, so the pre-image supplies the remaining values.
            if (context.PreEntityImages.Contains(PreImageName))
            {
                preImage = context.PreEntityImages[PreImageName];
            }

            tracingService.Trace(
                "PreventDoubleBookingPlugin started. Message: {0}",
                context.MessageName);

            var bookingBLL =
                new BookingBLL(
                    service,
                    tracingService);

            bookingBLL.ValidateNoOverlap(
                target,
                preImage);

            tracingService.Trace(
                "PreventDoubleBookingPlugin completed.");
        }
    }
}