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

#define BLUR_RADIUS 5

float2 ScreenRes;

float4 MainPS(VSOutput input) : COLOR {
    float4 sum = float4(0.0, 0.0, 0.0, 0.0);
    float counter = 0.0;

    for (int y = -BLUR_RADIUS; y <= BLUR_RADIUS; y++) {
        for (int x = -BLUR_RADIUS; x < BLUR_RADIUS; x++) {
            float2 samplePos = input.UV + (float2(x, y) / ScreenRes);
            sum += tex2D(SpriteTextureSampler, samplePos);
            counter++;
        }
    }

    return sum / counter;
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
