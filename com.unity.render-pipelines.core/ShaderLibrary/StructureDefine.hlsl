
#ifndef UNITY_STRUCT_DEFINE_INCLUDED
#define UNITY_STRUCT_DEFINE_INCLUDED
struct DefaultStruct
 {
     float4 Test;
 };
struct GrassCalcStruct
{
    int CalcIdx;
    float Spring;
    float Damping;
    float3 PosWs;
    float3 UpNormal;
    float3 SpringVector;
    float3 Velocity;
};
#endif
