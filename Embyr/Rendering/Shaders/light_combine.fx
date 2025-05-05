#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

struct VSOutput {
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 UV : TEXCOORD0;
};

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state {
    Texture = <SpriteTexture>;
};

Texture2D LightBuffer;
sampler2D LightBufferSampler = sampler_state {
    Texture = <LightBuffer>;
};

Texture2D DistanceField;
sampler2D DistanceFieldSampler = sampler_state {
    Texture = <DistanceField>;
};

float VolumetricScalar = 0.0;
float3 AmbientColor;

float4 MainPS(VSOutput input) : COLOR {
    float4 albedo = tex2D(SpriteTextureSampler, input.UV);
    float3 lightColor = tex2D(LightBufferSampler, input.UV).rgb;

    // get final color with multiplication, add volumetrics
    float4 finalColor = albedo * float4(AmbientColor + lightColor, 1.0);
    finalColor += float4(lightColor, 1.0) * VolumetricScalar;

    return finalColor;
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
