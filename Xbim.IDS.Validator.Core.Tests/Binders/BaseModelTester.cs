﻿using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.IDS.Validator.Core.Binders;
using Xbim.Ifc;
using Xbim.InformationSpecifications;
using Xunit.Abstractions;

namespace Xbim.IDS.Validator.Core.Tests.Binders
{
    [Collection(nameof(TestEnvironment))]
    public abstract class BaseModelTester
    {

        private static Lazy<IModel> lazyIfc4Model = new Lazy<IModel>(()=> BuildIfc4Model());
        private static Lazy<IModel> lazyIfc2x3Model = new Lazy<IModel>(() => BuildIfc2x3Model());

        private BinderContext _context = new BinderContext();
        
      

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
            
            logger = TestEnvironment.GetXunitLogger<BaseModelTester>(output);
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


        internal ILogger<T> GetLogger<T>()
        {
            return TestEnvironment.GetXunitLogger<T>(output);
        }

        protected static FacetGroup BuildGroup(IFacet facet)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var group = new FacetGroup();
#pragma warning restore CS0618 // Type or member is obsolete
            group.RequirementOptions = new System.Collections.ObjectModel.ObservableCollection<RequirementCardinalityOptions>();
            group.RequirementOptions.Add(RequirementCardinalityOptions.Expected);
            group.Facets.Add(facet);
            return group;
        }


        public enum ConstraintType
        {
            Exact,
            Pattern,
            Range,
            Structure
        }


        protected void AssertIfcTypeFacetQuery(IfcTypeFacetBinder typeFacetBinder, string ifcType, int expectedCount, Type[] expectedTypes, string predefinedType = "",
            ConstraintType ifcTypeConstraint = ConstraintType.Exact, ConstraintType preConstraint = ConstraintType.Exact, bool includeSubTypes = true)
        {
            IfcTypeFacet facet = BuildIfcTypeFacetFromCsv(ifcType, predefinedType, includeSubTypes, ifcTypeConstraint, preDefConstraint: preConstraint);

            // Act
            var expression = typeFacetBinder.BindSelectionExpression(query.InstancesExpression, facet);

            // Assert

            var result = query.Execute(expression, Model);

            result.Should().HaveCount(expectedCount);

            if (expectedCount > 0)
            {
                result.Should().AllSatisfy(t =>
                    expectedTypes.Where(e => e.IsAssignableFrom(t.GetType()))
                    .Should().ContainSingle($"Found {t.GetType().Name}, and expected one of {string.Join(',', expectedTypes.Select(t => t.Name))}"));

            }
        }


        private static IfcTypeFacet BuildIfcTypeFacetFromCsv(string ifcTypeCsv, string predefinedTypeCsv = "", bool includeSubTypes = false,
            ConstraintType ifcConstraint = ConstraintType.Exact, ConstraintType preDefConstraint = ConstraintType.Exact)
        {
            IfcTypeFacet facet = new IfcTypeFacet
            {
                IfcType = new ValueConstraint(NetTypeName.String),
                PredefinedType = new ValueConstraint(NetTypeName.String),
                IncludeSubtypes = includeSubTypes,
            };

            var ifcValues = ifcTypeCsv.Split(',');
            foreach (var ifcVal in ifcValues)
            {
                if (string.IsNullOrEmpty(ifcVal)) continue;
                if (ifcConstraint == ConstraintType.Pattern)
                    facet.IfcType.AddAccepted(new PatternConstraint(ifcVal));
                else
                    facet.IfcType.AddAccepted(new ExactConstraint(ifcVal));
            }

            var pdTypes = predefinedTypeCsv.Split(',');
            foreach (var predef in pdTypes)
            {
                if (string.IsNullOrEmpty(predef)) continue;
                if (preDefConstraint == ConstraintType.Pattern)
                    facet.PredefinedType.AddAccepted(new PatternConstraint(predef));
                else
                    facet.PredefinedType.AddAccepted(new ExactConstraint(predef));
            }
            return facet;
        }

    }
}
