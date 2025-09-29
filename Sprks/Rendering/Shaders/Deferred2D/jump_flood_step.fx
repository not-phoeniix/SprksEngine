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

float Offset;
float2 ScreenRes;

float4 MainPS(VSOutput input) : COLOR {
    float2 uv = input.UV;
    float2 uvOffset = Offset.xx / ScreenRes;

    float2 closestSeed = float2(0.0, 0.0);
    float closestDistSquared = 100.0;
    bool foundSeed = false;

    for (float x = -1; x <= 1; x++) {
        for (float y = -1; y <= 1; y++) {
            // calculate sample pos, skip if out of image bounds
            float2 samplePos = uv + float2(x * uvOffset.x, y * uvOffset.y);
            if (samplePos.x < 0.0 || samplePos.x > 1.0 || samplePos.y < 0.0 || samplePos.y > 1.0) {
                continue;
            }

            float4 sampleValue = tex2D(SpriteTextureSampler, samplePos);
            float2 diff = uv - sampleValue.xy;
            float dSqr = diff.x * diff.x + diff.y * diff.y;

            // only update closest distance if distance is closer AND
            //   blue channel exists! blue values of zero indicate
            //   that no seed has been located there
            if (dSqr < closestDistSquared && sampleValue.b > 0.01) {
                closestDistSquared = dSqr;
                closestSeed = sampleValue.xy;
                foundSeed = true;
            }
        }
    }

    return float4(closestSeed, (float)foundSeed, (float)foundSeed);
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
