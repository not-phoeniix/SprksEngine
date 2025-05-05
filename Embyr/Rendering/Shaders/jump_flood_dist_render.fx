#include "frag_header.fxh"

//! jump flood algorithm conceptualized from this article:
//!   https://blog.demofox.org/2016/02/29/fast-voronoi-diagrams-and-distance-dield-textures-on-the-gpu-with-the-jump-flooding-algorithm/

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state {
    Texture = <SpriteTexture>;
};

//! input should be jump flood step result!

float4 MainPS(VSOutput input) : COLOR {
    float4 sampleColor = tex2D(SpriteTextureSampler, input.UV);
    float2 closestSeed = sampleColor.xy;
    float depth = sampleColor.z;

    if (closestSeed.x == 0.0 && closestSeed.y == 0.0) {
        return float4(1.0, 1.0, depth, 1.0);
    }

    float dist = saturate(length(closestSeed - input.UV));
    return float4(dist.xx, depth, 1.0);
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
