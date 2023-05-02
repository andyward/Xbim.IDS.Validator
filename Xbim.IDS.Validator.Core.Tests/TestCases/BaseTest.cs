﻿using Divergic.Logging.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xbim.Common.Step21;
using Xbim.IDS.Validator.Core.Interfaces;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xunit.Abstractions;

namespace Xbim.IDS.Validator.Core.Tests.TestCases
{
    [Collection(nameof(TestEnvironment))]
    public abstract class BaseTest
    {
        protected readonly ITestOutputHelper output;

        protected readonly ILogger logger;

        private readonly IServiceProvider provider;

        


        public BaseTest(ITestOutputHelper output)
        {
            this.output = output;
            logger = GetXunitLogger();
            provider = TestEnvironment.ServiceProvider;
            TestEnvironment.InitialiseXunitLogger(output);
        }

        
        internal ILogger GetXunitLogger()
        {
            return TestEnvironment.GetXunitLogger<BaseTest>(output);
        }

        protected XbimSchemaVersion[] GetSchemas(XbimSchemaVersion[] schemaVersions)
        {
            if (schemaVersions.Length == 0)
            {
                return new[] { XbimSchemaVersion.Ifc4, XbimSchemaVersion.Ifc2X3 };
            }
            var except = new[] { XbimSchemaVersion.Ifc2X3 };
            return schemaVersions;//.Except(except).ToArray();
        }

        protected ValidationOutcome VerifyIdsFile(string idsFile, bool spotfix = false, XbimSchemaVersion schemaVersion = XbimSchemaVersion.Ifc4,
            VerificationOptions options = default
            )
        {
            IfcStore model = null;
            try
            {
                string ifcFile = Path.ChangeExtension(idsFile, "ifc");
                logger.LogInformation("Verifying {schema} model {model}", schemaVersion, ifcFile);
                if (schemaVersion == XbimSchemaVersion.Ifc4)
                {
                    model = IfcStore.Open(ifcFile);
                }
                else if (schemaVersion == XbimSchemaVersion.Ifc4x3)
                {
                    model = IfcStore.Open(ifcFile);
                }
                else
                {

                    // Very crude attempt for fake an IFC2x3 from IFC4. Because of breaking changes between schemas
                    // this should be treated as suspect - but does allow quick & dirty testing of our schema switching logic
                    var stepText = File.ReadAllText(ifcFile);
                    stepText = stepText.Replace("FILE_SCHEMA(('IFC4'));", "FILE_SCHEMA(('IFC2x3'));");
                    using (var stream = new MemoryStream())
                    using (var sw = new StreamWriter(stream))
                    {
                        sw.Write(stepText);
                        sw.Flush();
                        stream.Seek(0, SeekOrigin.Begin);

                        model = IfcStore.Open(stream, IO.StorageType.Ifc, schemaVersion, IO.XbimModelType.MemoryModel);
                    }
                }

                if (spotfix)
                {
                    // These models appear invalid for the test case by containing an additional wall. rather than fix the IFC, we spotfix
                    // So we can continue to add the standard test files without manually fixing in future.
                    // e.g. classification/pass-occurrences_override_the_type_classification_per_system_1_3.ifc
                    var rogue = model.Instances[4];
                    if (rogue is IIfcWall w && w.GlobalId == "3Agm079vPIYBL4JExVrhD5")
                    {
                        using (var tran = model.BeginTransaction("Patch"))
                        {
                            model.Delete(rogue);
                            tran.Commit();
                        }
                    }
                    else
                    {
                        // Maybe the test files got fixed?
                        logger.LogWarning("Spotfix failed. Check if this code can be removed");
                    }
                }

                var validator = provider.GetRequiredService<IIdsModelValidator>();
                var outcome = validator.ValidateAgainstIds(model, idsFile, logger, options);

                return outcome;
            }
            finally
            {
                if(model != null)
                {
                    model.Dispose();
                }
            }
        }
    }
}
