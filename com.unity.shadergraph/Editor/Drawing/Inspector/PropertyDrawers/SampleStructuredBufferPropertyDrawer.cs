using System;
using System.Reflection;
using UnityEditor.Graphing;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.ShaderGraph.Drawing.Inspector.PropertyDrawers
{
    [SGPropertyDrawer(typeof(SampleStructuredBufferNode))]
    public class SampleStructuredBufferPropertyDrawer: IPropertyDrawer, IGetNodePropertyDrawerPropertyData
    {
        Action m_setNodesAsDirtyCallback;
        Action m_updateNodeViewsCallback;

        void IGetNodePropertyDrawerPropertyData.GetPropertyData(Action setNodesAsDirtyCallback, Action updateNodeViewsCallback)
        {
            m_setNodesAsDirtyCallback = setNodesAsDirtyCallback;
            m_updateNodeViewsCallback = updateNodeViewsCallback;
        }
        public Action inspectorUpdateDelegate { get; set; }
        VisualElement CreateGUI(SampleStructuredBufferNode node, InspectableAttribute attribute,
            out VisualElement propertyVisualElement)
        {
            var propertySheet = new PropertySheet(PropertyDrawerUtils.CreateLabel("SampleStructuredBufferNode", 0, FontStyle.Bold));
            PropertyDrawerUtils.AddDefaultNodeProperties(propertySheet, node, m_setNodesAsDirtyCallback, m_updateNodeViewsCallback);
            var outputListView = new ReorderableSlotListView(node, SlotType.Output, true);
            outputListView.OnAddCallback += list => inspectorUpdateDelegate();
            outputListView.OnRemoveCallback += list => inspectorUpdateDelegate();
            outputListView.OnListRecreatedCallback += () => inspectorUpdateDelegate();
            propertySheet.Add(outputListView);
            propertyVisualElement = null;
            return propertySheet;
        }
        public VisualElement DrawProperty(PropertyInfo propertyInfo, object actualObject, InspectableAttribute attribute)
        {
            return this.CreateGUI(
                (SampleStructuredBufferNode)actualObject,
                attribute,
                out var propertyVisualElement);
        }


    }
}
