﻿using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xbim.IDS.Validator.Core.Binders;
using Xbim.Ifc4.Interfaces;
using Xbim.InformationSpecifications;
using Xunit.Abstractions;

namespace Xbim.IDS.Validator.Core.Tests.Binders
{
    public class AttributeFacetBinderTests : BaseModelTester
    {
        public AttributeFacetBinderTests(ITestOutputHelper output) : base(output)
        {
            Binder = new AttributeFacetBinder(BinderContext, Logger);
        }

        /// <summary>
        /// System under test
        /// </summary>
        AttributeFacetBinder Binder { get; }


        [InlineData(nameof(IIfcFurniture.ObjectType), "Chair - Dining", 6)]
        [InlineData(nameof(IIfcSite.CompositionType), "ELEMENT", 8)]
        [InlineData(nameof(IIfcSite.RefElevation), "0", 1)]
        [InlineData(nameof(IIfcRoot.GlobalId), "2ru7YPT4T9MuTpOS4FRzxX", 1)]    // A WallType
        [InlineData(nameof(IIfcObject.ObjectType), null, 68)]    // Any object with an ObjectType defined (any IfcObject)
        [InlineData(nameof(IIfcRoot.GlobalId), null, 95)]    // Any object with an GlobalID (any object or Type)
        [InlineData(nameof(IIfcRoot.Description), null, 1)] // Any Object with a description
        [Theory]
        public void Can_Query_By_Attributes(string attributeFieldName, string attributeValue, int expectedCount)
        {

            AttributeFacet attrFacet = new AttributeFacet
            {
                AttributeName = attributeFieldName,
                AttributeValue = attributeValue != null ? new ValueConstraint(attributeValue) : null
            };


            // Act
            var expression = Binder.BindSelectionExpression(query.InstancesExpression, attrFacet);

            // Assert

            var result = query.Execute(expression, Model);
            result.Should().HaveCount(expectedCount);

        }

        [InlineData(nameof(IIfcFurniture.ObjectType), "Chair.*", 6)]
        [Theory]
        public void Can_Query_By_Attributes_Patterns(string attributeFieldName, string attributeValue, int expectedCount)
        {

            AttributeFacet attrFacet = new AttributeFacet
            {
                AttributeName = attributeFieldName,
                AttributeValue = new ValueConstraint()
            };
            attrFacet.AttributeValue.AddAccepted(new PatternConstraint(attributeValue));


            // Act
            var expression = Binder.BindSelectionExpression(query.InstancesExpression, attrFacet);

            // Assert

            var result = query.Execute(expression, Model);
            result.Should().HaveCount(expectedCount);

        }


        [InlineData("IfcWall", "IfcOwnerHistory")]
        [InlineData("IfcWall", "NAME")]
        [InlineData("IfcWall", "OBJECTTYPE")]
        [InlineData("IfcWall", "IfcRelAggregates")]
        [InlineData("IfcWall", "*")]
        [InlineData("IfcWall", " ")]
        [InlineData("IfcWall", "")]
        [Theory]
        public void Invalid_Attributes_Handled(string ifcType, string attributeFieldName)
        {
            IfcTypeFacet ifcFacet = new IfcTypeFacet
            {
                IfcType = new ValueConstraint(ifcType),
            };

            AttributeFacet attrFacet = new AttributeFacet
            {
                AttributeName = attributeFieldName,
                AttributeValue = new ValueConstraint("not relevant")
            };
            var ifcBinder = new IfcTypeFacetBinder(new BinderContext { Model = Model}, GetLogger<IfcTypeFacetBinder>());


            var expression = ifcBinder.BindSelectionExpression(query.InstancesExpression, ifcFacet);
            // Act
            var ex = Record.Exception(() => Binder.BindSelectionExpression(expression, attrFacet));

            ex.Should().NotBeNull();
            ex.Should().BeOfType<InvalidOperationException>();
            ex.Message.Should().Be($"Attribute Facet '{attributeFieldName?.Trim()}' is not valid");

        }

        ILogger<AttributeFacetBinder> Logger { get => GetLogger<AttributeFacetBinder>(); }

    }
}
