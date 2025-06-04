#include "2d_defines.fxh"

//! jump flood algorithm conceptualized from this article:
//!   https://blog.demofox.org/2016/02/29/fast-voronoi-diagrams-and-distance-dield-textures-on-the-gpu-with-the-jump-flooding-algorithm/

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state {
    Texture = <SpriteTexture>;
};

float TargetDepth;

//! input should be depth buffer!

float4 MainPS(VSOutput input) : COLOR {
    float depth = tex2D(SpriteTextureSampler, input.UV).g;

    float4 seed = float4(0.0, 0.0, depth, 0.0);

    if (depth > TargetDepth - 0.01 && depth < TargetDepth + 0.01) {
        seed.xy = input.UV;
    }

    return seed;
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
