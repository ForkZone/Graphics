


using UnityEngine;

namespace UnityEditor.ShaderGraph.Internal
{
    class StructuredBuffer
    {
        [SerializeField] private string m_StructName = "DefaultStruct";

        public string StructName
        {
            get => m_StructName;
            set => m_StructName = value;
        }
    }
}
