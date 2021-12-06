using System;
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
        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Vector1MaterialSlot(0, "Idx", "Idx", SlotType.Input, 0, ShaderStageCapability.All));
            AddSlot(new StructuredBufferSlot(1, "Buffer", "Buffer", SlotType.Input, new StructuredBuffer()));
            //RemoveSlotsNameNotMatching(new[] {0, 1});
        }

        public override void Setup()
        {
            base.Setup();
        }

        public virtual void GenerateNodeCode(ShaderStringBuilder sb, GenerationMode generationMode)
        {
            // //TODO:DELETE
            // Debug.Log("////////////////////////////");
            // Debug.Log(GetSlotValue(1, generationMode));
            // foreach (var VARIABLE in GetSlots())
            // {
            //
            // }
            // sb.AppendLine(string.Format("float4 {0} = float4(0,1,0,0);" , GetVariableNameForSlot(0)));
            using (var outputSlots = PooledList<MaterialSlot>.Get())
            {
                //GetInputSlots<MaterialSlot>(inputSlots);
                var sBName = GetSlotValue(1, generationMode);
                var idxName = GetSlotValue(0, generationMode);
                GetOutputSlots<MaterialSlot>(outputSlots);
                foreach (var slot in outputSlots)
                {
                    var outPutName = GetVariableNameForSlot(slot.id);
                    sb.AppendLine("{3} {0} = {1}[{4}].{2};", outPutName, sBName, slot.shaderOutputName, slot.concreteValueType.ToShaderString(), idxName);

                }
            }
        }
    }
}
