using System;
using System.Reflection;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Drawing;
using UnityEditor.Graphing.Util;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine.UIElements;

namespace UnityEditor.ShaderGraph.Drawing.Inspector.PropertyDrawers
{
    [SGPropertyDrawer(typeof(SampleStructuredBufferNode))]
    class SampleStructuredBufferDrawer : AbstractMaterialNodePropertyDrawer
    {
        internal override void AddCustomNodeProperties(VisualElement parentElement, AbstractMaterialNode nodeBase,
            Action setNodesAsDirtyCallback, Action updateNodeViewsCallback)
        {
            var node = nodeBase as SampleStructuredBufferNode;
            // PropertyDrawerUtils.AddCustomCheckboxProperty(
            //     parentElement, nodeBase, setNodesAsDirtyCallback, updateNodeViewsCallback,
            //     "Use Global Mip Bias", "Change Enable Global Mip Bias",
            //     () => node.enableGlobalMipBias, (val) => node.enableGlobalMipBias = val);
        }
    }
}
