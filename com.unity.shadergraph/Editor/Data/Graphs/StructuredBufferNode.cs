using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

namespace UnityEditor.ShaderGraph
{
    class StructuredBufferNode: AbstractMaterialNode, IPropertyFromNode
    {
        public AbstractShaderProperty AsShaderProperty()
        {
            var prop = new StructuredBufferProperty() { value = m_Buffer};
            return prop;
        }

        [SerializeField] private StructuredBuffer m_Buffer;

        public StructuredBuffer structuredBuffer
        {
            get => m_Buffer;
            set => m_Buffer = value;
        }
        public int outputSlotId => 0;

        public override string GetVariableNameForSlot(int slotId)
        {
            //TODO:DELETE
            Debug.Log("////////////////////////////");
            Debug.Log("VAR");

            return $"{GetVariableNameForSlot(0)}";
        }
    }
}
