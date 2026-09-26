using CommunitySports.Plugins.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace CommunitySports.Plugins.BLL
{
    public class FacilityBLL
    {
        private readonly IOrganizationService _service;

        public FacilityBLL(IOrganizationService service)
        {
            _service = service;
        }

        public bool IsUnderMaintenance(Guid facilityId)
        {
            var facility = _service.Retrieve(
                SchemaNames.Facility,
                facilityId,
                new ColumnSet(
                    SchemaNames.FacilityFields.State,
                    SchemaNames.FacilityFields.StatusReason));

            var state =
                facility.GetAttributeValue<OptionSetValue>(
                    SchemaNames.FacilityFields.State);

            var statusReason =
                facility.GetAttributeValue<OptionSetValue>(
                    SchemaNames.FacilityFields.StatusReason);

            return
                state != null &&
                statusReason != null &&
                state.Value == SchemaNames.FacilityState.Inactive &&
                statusReason.Value ==
                    SchemaNames.FacilityStatusReason.UnderMaintenance;
        }
    }
}