#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

struct VSInput {
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
};

struct PSInput {
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD1;
    float3 WorldPosition : TEXCOORD2;
};
