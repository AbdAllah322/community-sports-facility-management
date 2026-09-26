using System.Globalization;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CommunitySports.Plugins.BLL
{
    public class ConfigurationBLL
    {
        private readonly IOrganizationService _service;

        public ConfigurationBLL(IOrganizationService service)
        {
            _service = service;
        }

        public decimal GetDecimalEnvironmentVariable(string schemaName)
        {
            var definitionQuery =
                new QueryExpression("environmentvariabledefinition")
                {
                    ColumnSet = new ColumnSet(
                        "defaultvalue"),

                    TopCount = 1
                };

            definitionQuery.Criteria.AddCondition(
                "schemaname",
                ConditionOperator.Equal,
                schemaName);

            var definition =
                _service
                    .RetrieveMultiple(definitionQuery)
                    .Entities
                    .FirstOrDefault();

            if (definition == null)
            {
                throw new InvalidPluginExecutionException(
                    $"Environment variable '{schemaName}' was not found.");
            }

            var valueQuery =
                new QueryExpression("environmentvariablevalue")
                {
                    ColumnSet = new ColumnSet("value"),
                    TopCount = 1
                };

            valueQuery.Criteria.AddCondition(
                "environmentvariabledefinitionid",
                ConditionOperator.Equal,
                definition.Id);

            valueQuery.Criteria.AddCondition(
                "statecode",
                ConditionOperator.Equal,
                0);

            valueQuery.AddOrder(
                "modifiedon",
                OrderType.Descending);

            var currentValue =
                _service
                    .RetrieveMultiple(valueQuery)
                    .Entities
                    .FirstOrDefault();

            var value =
                currentValue?.GetAttributeValue<string>("value");

            if (string.IsNullOrWhiteSpace(value))
            {
                value =
                    definition.GetAttributeValue<string>(
                        "defaultvalue");
            }

            if (!decimal.TryParse(
                    value,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out decimal result))
            {
                throw new InvalidPluginExecutionException(
                    $"Environment variable '{schemaName}' does not contain a valid decimal value.");
            }

            return result;
        }
    }
}