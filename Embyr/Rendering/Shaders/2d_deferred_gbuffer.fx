#include "2d_defines.fxh"

struct PSOutput {
    float4 Albedo : COLOR0;  // render target 0
    float4 Normal : COLOR1;  // render target 1
    float4 Depth : COLOR2;   // render target 2
};

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state {
    Texture = <SpriteTexture>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    AddressW = CLAMP;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
};

Texture2D NormalTexture;
sampler2D NormalTextureSampler = sampler_state {
    Texture = <NormalTexture>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    AddressW = CLAMP;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
};

#define MAX_Z_INDEX 1000

int ZIndex;
bool ObstructsLight;
bool UseNormals;
float Gamma;

PSOutput MainPS(VSOutput input) {
    PSOutput output;

    output.Albedo = tex2D(SpriteTextureSampler, input.UV) * input.Color;
    output.Albedo.rgb = pow(abs(output.Albedo.rgb), Gamma);

    // if sampled color is black set default normal values
    float3 normal = tex2D(NormalTextureSampler, input.UV).xyz;
    output.Normal = UseNormals ? float4(normal, 1.0) : float4(0.5, 0.5, 1.0, 1.0);

    float depth = float(ZIndex) / float(MAX_Z_INDEX + 1);
    // black means obstructs light, white means doesn't
    float obstruct = 1.0f - (float)ObstructsLight;
    output.Depth = float4(depth, obstruct, 1.0, 1.0f);

    return output;
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
