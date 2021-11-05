#ifndef VISIBILITY_PASS_HLSL
#define VISIBILITY_PASS_HLSL

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Visibility/VisibilityCommon.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _DeferredMaterialInstanceData;
CBUFFER_END

#if defined(UNITY_DOTS_INSTANCING_ENABLED)
UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
    UNITY_DOTS_INSTANCED_PROP(float4, _DeferredMaterialInstanceData)
UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

#define _DeferredMaterialInstanceData UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _DeferredMaterialInstanceData)

#endif

struct VisibilityVtoP
{
    float4 pos : SV_Position;
    uint batchID : ATTRIBUTE0;

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

struct VisibilityDrawInput
{
    uint vertexIndex : SV_VertexID;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

VisibilityVtoP Vert(VisibilityDrawInput input)
{
    VisibilityVtoP v2p;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, v2p);

    GeoPoolMetadataEntry metadata = _GeoPoolGlobalMetadataBuffer[(int)_DeferredMaterialInstanceData.x];

    GeoPoolVertex vertexData = GeometryPool::LoadVertex(input.vertexIndex, metadata);

    float3 worldPos = TransformObjectToWorld(vertexData.pos);
    v2p.pos = TransformWorldToHClip(worldPos);
    v2p.batchID = (int)_DeferredMaterialInstanceData.y;
    return v2p;
}

void Frag(
    VisibilityVtoP packedInput,
    uint primitiveID : SV_PrimitiveID,
    out uint outVisibility0 : SV_Target0,
    out uint outVisibility1 : SV_Target1)
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
    UNITY_SETUP_INSTANCE_ID(packedInput);
    #ifdef DOTS_INSTANCING_ON
        Visibility::VisibilityData visData;
        visData.valid = true;
        visData.DOTSInstanceIndex = GetDOTSInstanceIndex();
        visData.primitiveID = primitiveID;
        visData.batchID = packedInput.batchID;
        Visibility::PackVisibilityData(visData, outVisibility0, outVisibility1);
    #else
        outVisibility0 = 0;
        outVisibility1 = 0;
    #endif
}

//TODO: make this follow the pretty pattern of materials.
void FragEmpty()
{
    //empty frag
}

#endif
