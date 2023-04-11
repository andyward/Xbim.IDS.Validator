﻿using FluentAssertions;
using Xbim.Common.Step21;
using Xunit.Abstractions;

namespace Xbim.IDS.Validator.Core.Tests.TestCases
{
    public class IfcClassificationTestCases : BaseTest
    {
        public IfcClassificationTestCases(ITestOutputHelper output) : base(output)
        {
        }
        // IFC2x3: IFCEXTERNALREFERENCERELATIONSHIP is not implemented, ExternalReference.Identifier changed
        // Also change to IfcClassificationNotationSelect
        [InlineData("TestCases/class/pass-a_classification_facet_with_no_data_matches_any_classification_2_2.ids")]
        [InlineData("TestCases/class/pass-a_required_facet_checks_all_parameters_as_normal.ids")]
        [InlineData("TestCases/class/pass-an_optional_facet_always_passes_regardless_of_outcome_1_2.ids")]
        [InlineData("TestCases/class/pass-an_optional_facet_always_passes_regardless_of_outcome_2_2.ids")]
        [InlineData("TestCases/class/pass-both_system_and_value_must_match__all__not_any__if_specified_1_2.ids")]
        [InlineData("TestCases/class/pass-non_rooted_resources_that_have_external_classification_references_should_also_pass.ids", false)]
        [InlineData("TestCases/class/pass-occurrences_override_the_type_classification_per_system_1_3.ids", true)]
        [InlineData("TestCases/class/pass-occurrences_override_the_type_classification_per_system_3_3.ids", true)]
        [InlineData("TestCases/class/pass-restrictions_can_be_used_for_systems_2_2.ids")]
        [InlineData("TestCases/class/pass-restrictions_can_be_used_for_values_1_3.ids")]
        [InlineData("TestCases/class/pass-restrictions_can_be_used_for_values_2_3.ids")]
        [InlineData("TestCases/class/pass-systems_should_match_exactly_1_5.ids", false)]
        [InlineData("TestCases/class/pass-systems_should_match_exactly_3_5.ids")]
        [InlineData("TestCases/class/pass-systems_should_match_exactly_4_5.ids")]
        [InlineData("TestCases/class/pass-systems_should_match_exactly_5_5.ids", false)]
        [InlineData("TestCases/class/pass-values_match_subreferences_if_full_classifications_are_used__e_g__ef_25_10_should_match_ef_25_10_25__ef_25_10_30__etc_.ids", false)]
        [InlineData("TestCases/class/pass-values_should_match_exactly_if_lightweight_classifications_are_used.ids")]
        [Theory]
        public void EntityTestPass(string idsFile, bool specialCase = false)
        {
            // Can't test IFC2x3 Classifications in our harness due to schema changes
            var outcome = VerifyIdsFile(idsFile, specialCase, XbimSchemaVersion.Ifc4);

            outcome.Status.Should().Be(ValidationStatus.Pass);
            
        }

        [InlineData("TestCases/class/fail-a_classification_facet_with_no_data_matches_any_classification_1_2.ids")]
        [InlineData("TestCases/class/fail-a_prohibited_facet_returns_the_opposite_of_a_required_facet.ids")]
        [InlineData("TestCases/class/fail-both_system_and_value_must_match__all__not_any__if_specified_2_2.ids")]
        [InlineData("TestCases/class/fail-occurrences_override_the_type_classification_per_system_2_3.ids")]
        [InlineData("TestCases/class/fail-restrictions_can_be_used_for_systems_1_2.ids")]
        [InlineData("TestCases/class/fail-restrictions_can_be_used_for_values_3_3.ids")]
        [InlineData("TestCases/class/fail-systems_should_match_exactly_2_5.ids")]
        [Theory]
        public void EntityTestFailures(string idsFile)
        {
            
            var outcome = VerifyIdsFile(idsFile);

            outcome.Status.Should().Be(ValidationStatus.Fail);
            
        }
    }
}
