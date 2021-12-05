using System;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Internal;

namespace UnityEditor.ShaderGraph
{
    [Serializable]
    class StructuredBufferSlot : MaterialSlot
    {
        public StructuredBufferSlot()
        { }

        public StructuredBufferSlot(
            int slotId,
            string displayName,
            string shaderOutputName,
            SlotType slotType,
            StructuredBuffer inValue,
            ShaderStageCapability stageCapability = ShaderStageCapability.All,
            bool hidden = false)
            : base(slotId, displayName, shaderOutputName, slotType, stageCapability, hidden)
        {
            value = inValue;
        }

        public StructuredBuffer value;
        public override SlotValueType valueType { get { return SlotValueType.StructuredBuffer; } }
        public override ConcreteSlotValueType concreteValueType { get { return ConcreteSlotValueType.StructuredBuffer; } }

        public override void AddDefaultProperty(PropertyCollector properties, GenerationMode generationMode)
        {
            // if (value == null)
            // {
            //     value = new StructuredBuffer();
            // }
            // var property = new StructuredBufferProperty()
            // {
            //
            //     value = value
            // };
            // properties.AddShaderProperty(property);
        }
        protected override string ConcreteSlotValueAsVariable()
        {
            return string.Format("StructuredBuffer<DDDD> {0}"
                , "DDDDDDD");
        }
        public override void CopyValuesFrom(MaterialSlot foundSlot)
        {
        }

        public override bool isDefaultValue => throw new Exception();
    }
}
