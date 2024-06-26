﻿using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xbim.IDS.Validator.Core.Tests.Binders;
using Xunit.Abstractions;
using IdsLib;
using static IdsLib.Audit;

namespace Xbim.IDS.Validator.Core.Tests
{
    public class IdsValidatorTests : BaseBinderTests
    {
        public IdsValidatorTests(ITestOutputHelper output) : base(output)
        {
        }

        [InlineData(@"TestModels\IDS_wooden-windows.ids", Status.IdsContentError)]// On latest IDS. Should be OK
        [InlineData(@"TestModels\sample.ids", Status.Ok)]
        [InlineData(@"TestModels\BasicRequirements0-97.ids", Status.Ok)]
        [InlineData(@"TestModels\BasicRequirements1-0.ids", Status.Ok)]
        [InlineData(@"TestModels\Example.ids", Status.IdsStructureError | Status.IdsContentError)]
        [Theory]
        public void CanValidateIDS(string filename, Status expectedStatus)
        {
            var validator = new IdsValidator();
            var result = validator.ValidateIDS(filename, logger);

            result.Should().Be(expectedStatus);
        }

        [Fact]
        public void CanTestFolderInBatch()
        {
            var validator = new IdsValidator();
            var result = validator.ValidateIdsFolder(@"TestCases", Logger);
            result.Should().Be(Status.IdsStructureError | Status.IdsContentError);
        }


        ILogger<IdsValidator> Logger { get => GetLogger<IdsValidator>(); }
    }
}
