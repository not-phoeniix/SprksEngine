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

Texture2D BloomTexture;
sampler2D BloomTextureSampler = sampler_state {
    Texture = <BloomTexture>;
};

float4 MainPS(VSOutput input) : COLOR {
    float4 inputColor = tex2D(SpriteTextureSampler, input.UV);
    float3 bloomBlur = tex2D(BloomTextureSampler, input.UV).rgb;
    return float4(inputColor.rgb + bloomBlur, inputColor.a);
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
