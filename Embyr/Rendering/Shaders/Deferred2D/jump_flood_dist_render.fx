#include "../2d_defines.fxh"

//! jump flood algorithm conceptualized from this article:
//!   https://blog.demofox.org/2016/02/29/fast-voronoi-diagrams-and-distance-dield-textures-on-the-gpu-with-the-jump-flooding-algorithm/

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

Texture2D DepthBuffer;
sampler2D DepthBufferSampler = sampler_state {
    Texture = <DepthBuffer>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    AddressW = CLAMP;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
};

float TargetDepth;

//! input should be final jump flood step result!

float4 MainPS(VSOutput input) : COLOR {
    float4 stepResult = tex2D(SpriteTextureSampler, input.UV);
    float2 closestSeed = stepResult.xy;
    float obstructorDepth = tex2D(DepthBufferSampler, input.UV).y;

    float dist = max(length(closestSeed - input.UV), 0.0);

    // obstructor fragments just get a distance of zero,
    //   this is an unsigned distance field
    if (obstructorDepth > TargetDepth - 0.001 && obstructorDepth < TargetDepth + 0.001) {
        dist = 0.0;
    }

    // non filled in seed fragments have a distance of 1
    if (stepResult.b < 0.01) {
        dist = 1.0;
    }

    return float4(dist.xx, obstructorDepth, 1.0);
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
