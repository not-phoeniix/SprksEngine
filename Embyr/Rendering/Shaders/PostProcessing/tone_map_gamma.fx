#include "../2d_defines.fxh"

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state {
    Texture = <SpriteTexture>;
};

float Gamma;
bool EnableTonemapping;

// https://knarkowicz.wordpress.com/2016/01/06/aces-filmic-tone-mapping-curve/
//   Written by Krzysztof Narkowicz under Public Domain CC0
float3 ACESFilm(float3 x) {
    float a = 2.51f;
    float b = 0.03f;
    float c = 2.43f;
    float d = 0.59f;
    float e = 0.14f;
    return saturate((x * (a * x + b)) / (x * (c * x + d) + e));
}

float4 MainPS(VSOutput input) : COLOR {
    float4 spriteColor = tex2D(SpriteTextureSampler, input.UV);

    // apply narkowicz ACES curve !!
    if (EnableTonemapping == true) {
        spriteColor.rgb = ACESFilm(spriteColor.rgb);
    }

    // gamma correction
    spriteColor.rgb = pow(abs(spriteColor.rgb), 1.0 / Gamma);

    return spriteColor;
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
