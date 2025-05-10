#include "2d_defines.fxh"

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state {
    Texture = <SpriteTexture>;
};

float4 Color;

float4 MainPS(VSOutput input) : COLOR {
    float4 inputColor = tex2D(SpriteTextureSampler, input.UV);
    return inputColor.a > 0.01 ? Color : float4(0.0, 0.0, 0.0, 0.0);
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
