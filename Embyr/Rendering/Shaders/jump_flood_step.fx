#include "2d_defines.fxh"

//! jump flood algorithm conceptualized from this article:
//!   https://blog.demofox.org/2016/02/29/fast-voronoi-diagrams-and-distance-dield-textures-on-the-gpu-with-the-jump-flooding-algorithm/

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state {
    Texture = <SpriteTexture>;
};

float Offset;
float2 ScreenRes;

float4 MainPS(VSOutput input) : COLOR {
    float2 uv = input.UV;
    float2 uvOffset = Offset.xx / ScreenRes;
    float depth = tex2D(SpriteTextureSampler, uv).z;

    float2 closestSeed = float2(0, 0);
    float closestDistSquared = 100.0;

    for (float x = -1; x <= 1; x++) {
        for (float y = -1; y <= 1; y++) {
            // calculate sample pos, skip if out of image bounds
            float2 samplePos = uv + float2(x * uvOffset.x, y * uvOffset.y);
            if (samplePos.x < 0.0 || samplePos.x > 1.0 || samplePos.y < 0.0 || samplePos.y > 1.0) {
                continue;
            }

            float4 sampleValue = tex2D(SpriteTextureSampler, samplePos);

            if (sampleValue.x != 0.0 && sampleValue.y != 0.0) {
                float2 diff = uv - sampleValue.xy;
                float dSqr = diff.x * diff.x + diff.y * diff.y;
                if (dSqr < closestDistSquared) {
                    closestDistSquared = dSqr;
                    closestSeed = sampleValue.xy;
                }
            }
        }
    }

    return float4(closestSeed, depth, 0.0);
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
