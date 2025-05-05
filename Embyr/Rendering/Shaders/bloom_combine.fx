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

Texture2D InitialTexture;
sampler2D InitialTextureSampler = sampler_state {
    Texture = <InitialTexture>;
};

float4 MainPS(VSOutput input) : COLOR {
    float4 blurColor = tex2D(SpriteTextureSampler, input.UV);
    float4 initialColor = tex2D(InitialTextureSampler, input.UV);
    return float4(initialColor.rgb + blurColor.rgb, initialColor.a);
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
