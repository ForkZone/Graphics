using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

namespace UnityEditor.ShaderGraph
{
    [Title("Input", "StructuredBuffer", "Sample StructuredBuffer")]

    class SampleStructuredBufferNode : AbstractMaterialNode, IGeneratesBodyCode
    {
        public SampleStructuredBufferNode()
        {
            name = "Sample StructuredBuffer";
            UpdateNodeAfterDeserialization();
        }

        public StructuredBuffer buffer;

        // public AbstractShaderProperty AsShaderProperty()
        // {
        //     var prop = new StructuredBufferProperty() { value = buffer };
        //     if (buffer != null)
        //         prop.displayName = buffer.StructName;
        //     return prop;
        // }
        // public override void CollectShaderProperties(PropertyCollector properties, GenerationMode generationMode)
        // {
        //     properties.AddShaderProperty(new StructuredBufferProperty()
        //     {
        //         value = buffer
        //     });
        // }
        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Vector4MaterialSlot(0, "RGBA", "RGBA", SlotType.Output, Vector4.zero, ShaderStageCapability.All));
            AddSlot(new StructuredBufferSlot(1, "Buffer", "Buffer", SlotType.Input, new StructuredBuffer()));
            RemoveSlotsNameNotMatching(new[] {0, 1});
        }

        public override void Setup()
        {
            base.Setup();
        }

        public virtual void GenerateNodeCode(ShaderStringBuilder sb, GenerationMode generationMode)
        {
            //TODO:DELETE
            Debug.Log("////////////////////////////");
            Debug.Log(GetSlotValue(1, generationMode));

            sb.AppendLine(string.Format("float4 {0} = float4(0,1,0,0);" , GetVariableNameForSlot(0)));

        }
    }
}
