using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEngine;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Internal;

namespace UnityEditor.ShaderGraph
{
    [Serializable]
    [BlackboardInputInfo(80)]
    class StructuredBufferProperty : AbstractShaderProperty<StructuredBuffer>
    {
        public StructuredBufferProperty()
        {
            displayName = "MyStructuredBuffer";
            value = new StructuredBuffer();
        }
        internal override bool isExposable => false;
        internal override bool isRenamable => true;
        internal override ShaderInput Copy()
        {
            return new StructuredBufferProperty()
            {
                displayName = displayName,
                value = value
            };
        }

        public override PropertyType propertyType => PropertyType.StructuredBuffer;
        internal override void ForeachHLSLProperty(Action<HLSLProperty> action)
        {
            Action<ShaderStringBuilder> customDecl = (builder) =>
            {
                builder.AppendLine("StructuredBuffer<{1}> {0};", referenceName, value.StructName);
            };
            action(
                new HLSLProperty(HLSLType._CUSTOM, referenceName, HLSLDeclaration.Global, concretePrecision)
                {
                    customDeclaration = customDecl
                });
        }
        internal override string GetPropertyAsArgumentString(string precisionString)
        {
            return "StructuredBuffer<DefaultStruct> " + referenceName;
        }
        internal override string GetHLSLVariableName(bool isSubgraphProperty, GenerationMode mode)
        {
            return referenceName;
        }
        internal override AbstractMaterialNode ToConcreteNode()
        {
            return new StructuredBufferNode() { structuredBuffer = value };
        }

        internal override PreviewProperty GetPreviewMaterialProperty()
        {
            return new PreviewProperty(propertyType)
            {
                name = referenceName,
                structuredBufferValue = value
            };
        }
    }
}
