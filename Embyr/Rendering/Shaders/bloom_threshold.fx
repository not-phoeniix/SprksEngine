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

float Threshold;

float4 MainPS(VSOutput input) : COLOR {
    float4 inputColor = tex2D(SpriteTextureSampler, input.UV);
    float luminance = dot(inputColor.rgb, float3(0.299, 0.587, 0.144));
    return luminance >= Threshold ? inputColor : float4(0.0, 0.0, 0.0, 1.0);
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
