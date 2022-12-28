﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.IDS.Validator.Core.Binders;
using Xbim.Ifc;
using Xunit.Abstractions;

namespace Xbim.IDS.Validator.Core.Tests.Binders
{
    public abstract class BaseModelTester
    {

        private static Lazy<IModel> lazyIfc4Model = new Lazy<IModel>(()=> BuildIfc4Model());
        private static Lazy<IModel> lazyIfc2x3Model = new Lazy<IModel>(() => BuildIfc2x3Model());

        private BinderContext _context = new BinderContext();
        private readonly IServiceProvider provider;
       


        public IModel Model
        {
            get
            {
                if (_schema == XbimSchemaVersion.Ifc2X3)
                {
                    return lazyIfc2x3Model.Value;
                }
                else
                {
                    return lazyIfc4Model.Value;
                }
            }
        }

        public BinderContext BinderContext
        {
            get
            {
                _context.Model = Model;
                return _context;
            }
        }

        protected IfcQuery query;

        private readonly ITestOutputHelper output;
        private XbimSchemaVersion _schema;
        protected readonly ILogger logger;



        public BaseModelTester(ITestOutputHelper output, XbimSchemaVersion schema = XbimSchemaVersion.Ifc4)
        {
            this.output = output;
            _schema = schema;
            query = new IfcQuery();
            var services = new ServiceCollection()
                        .AddLogging((builder) => builder.AddXunit(output,
                        new Divergic.Logging.Xunit.LoggingConfig { LogLevel = LogLevel.Debug }));
            provider = services.BuildServiceProvider();
            logger = GetXunitLogger();
        }

       

        private static IModel BuildIfc4Model()
        {
            var filename = @"TestModels\SampleHouse4.ifc";
            return IfcStore.Open(filename);
        }

        private static IModel BuildIfc2x3Model()
        {
            var filename = @"TestModels\Dormitory-ARC.ifczip";
            return IfcStore.Open(filename);
        }

        internal ILogger<IdsModelBinderTests> GetXunitLogger()
        {
            var logger = GetLogger<IdsModelBinderTests>();
            Assert.NotNull(logger);
            return logger;
        }

        internal ILogger<T> GetLogger<T>()
        {
            return provider.GetRequiredService<ILogger<T>>();
        }


        public enum ConstraintType
        {
            Exact,
            Pattern,
            Range,
            Structure
        }
        
    }
}
