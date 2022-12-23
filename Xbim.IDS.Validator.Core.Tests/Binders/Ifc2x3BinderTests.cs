﻿using FluentAssertions;
using Xbim.Common.Step21;
using Xbim.IDS.Validator.Core.Binders;

using Xbim.Ifc4.Interfaces;
using Xbim.InformationSpecifications;
using Xunit.Abstractions;

namespace Xbim.IDS.Validator.Core.Tests.Binders
{
    public class Ifc2x3BinderTests : BaseModelTester
    {
        public Ifc2x3BinderTests(ITestOutputHelper output) : base(output, XbimSchemaVersion.Ifc2X3)
        {


        }
        // IfcFurniture

        [InlineData("IfcDoorStyle", nameof(IIfcDoorStyle.OperationType), "SINGLE_SWING_RIGHT", 9)]
        [InlineData("IfcDoorStyle", nameof(IIfcDoorStyle.Sizeable), "False", 19)]
        [InlineData("IfcFurnishingElement", nameof(IIfcFurniture.ObjectType), "430x480x800_Red", 10)]
        [InlineData("IfcWall", nameof(IIfcWall.GlobalId), "1iA8Dyn8X26BLTV11kuxmS", 1)]
        [InlineData("IfcDoor", nameof(IIfcDoor.Name), "Door_Single_Swinging_Internal-A:857x2185(762X2135)_Wood:216751", 1)]
        [InlineData("IfcSpace", nameof(IIfcSpace.Description), "External", 2)]
        // IfcElectricalCircuit Is only in IFC2x3
        [InlineData("IfcElectricalCircuit", nameof(Ifc2x3.ElectricalDomain.IfcElectricalCircuit.Description), "External", 0)]
        [Theory]
        public void Can_Query_By_Ifc_And_Attributes(string ifcType, string attributeFieldName, string attributeValue, int expectedCount)
        {
            IfcTypeFacet ifcFacet = new IfcTypeFacet
            {
                IfcType = new ValueConstraint(ifcType),
            };

            AttributeFacet attrFacet = new AttributeFacet
            {
                AttributeName = attributeFieldName,
                AttributeValue = new ValueConstraint(attributeValue)
            };
            var ifcbinder = new IfcTypeFacetBinder(Model);

            var attrbinder = new AttributeFacetBinder(Model);

            // Act
            var expression = ifcbinder.BindSelectionExpression(query.InstancesExpression, ifcFacet);
            expression = attrbinder.BindWhereExpression(expression, attrFacet);

            // Assert

            var result = query.Execute(expression, Model);
            result.Should().HaveCount(expectedCount);

        }

        //[InlineData("IfcWindowStyle", "Dimensions", "Frame Depth", 65, 1)]

        [InlineData("IfcDoor", "Pset_DoorCommon", "FireRating", "n/a", 56)]
        [InlineData("IfcWindow", "Pset_WindowCommon", "SecurityRating", "1123", 33)]
        [InlineData("IfcWindow", "Pset_WindowCommon", "ThermalTransmittance", 5.5617, 23)]
        [Theory]
        public void Can_Query_By_Ifc_And_Properties(string ifcType, string psetName, string propName, object value, int expectedCount)
        {
            IfcTypeFacet ifcFacet = new IfcTypeFacet
            {
                IfcType = new ValueConstraint(ifcType),
            };

            IfcPropertyFacet propertyFacet = new IfcPropertyFacet
            {
                PropertySetName = new ValueConstraint(NetTypeName.String),
                PropertyName = new ValueConstraint(NetTypeName.String),
                PropertyValue = new ValueConstraint(),
            };
            propertyFacet.PropertySetName.AddAccepted(new ExactConstraint(psetName));
            propertyFacet.PropertyName.AddAccepted(new ExactConstraint(propName));
            if (value != null)
                propertyFacet.PropertyValue.AddAccepted(new ExactConstraint(value?.ToString()));
            var ifcbinder = new IfcTypeFacetBinder(Model);

            var psetbinder = new PsetFacetBinder(Model);

            // Act
            var expression = ifcbinder.BindSelectionExpression(query.InstancesExpression, ifcFacet);
            expression = psetbinder.BindWhereExpression(expression, propertyFacet);

            // Assert

            var result = query.Execute(expression, Model);
            result.Should().HaveCount(expectedCount);

        }

        [InlineData("IfcWall", "Cement_Render_White", 12)]
        [InlineData("IfcDoor", "Metal_Steel_Stainless", 7)]
        [InlineData("IfcWall", "Vapor Retarder", 0)]
        [InlineData("IfcActor", "Vapor Retarder", 0)]
        [Theory]
        public void Can_Query_By_Ifc_And_Materials(string ifcType, string material, int expectedCount)
        {
            IfcTypeFacet ifcFacet = new IfcTypeFacet
            {
                IfcType = new ValueConstraint(ifcType),
            };

            MaterialFacet materialFacet = new MaterialFacet
            {
                Value = new ValueConstraint(NetTypeName.String),
            };
            materialFacet.Value = material;
            var ifcbinder = new IfcTypeFacetBinder(Model);

            var materialbinder = new MaterialFacetBinder(Model);

            // Act
            var expression = ifcbinder.BindSelectionExpression(query.InstancesExpression, ifcFacet);
            expression = materialbinder.BindWhereExpression(expression, materialFacet);

            // Assert

            var result = query.Execute(expression, Model);
            result.Should().HaveCount(expectedCount);

        }
    }
}