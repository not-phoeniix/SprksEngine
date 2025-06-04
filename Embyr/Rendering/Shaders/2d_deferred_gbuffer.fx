#include "2d_defines.fxh"

struct PSOutput {
    float4 Albedo : COLOR0;       // render target 0
    float4 NormalDepth : COLOR1;  // render target 1
};

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state {
    Texture = <SpriteTexture>;
};

Texture2D NormalTexture;
sampler2D NormalTextureSampler = sampler_state {
    Texture = <NormalTexture>;
};

#define MAX_Z_INDEX 1000

int ZIndex;

PSOutput MainPS(VSOutput input) {
    PSOutput output;

    output.Albedo = tex2D(SpriteTextureSampler, input.UV) * input.Color;

    float depth = 1.0f - float(ZIndex) / float(MAX_Z_INDEX);
    float2 normal = tex2D(NormalTextureSampler, input.UV).xy;

    output.NormalDepth = float4(normal.x, normal.y, depth, 1.0f);

    return output;
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
