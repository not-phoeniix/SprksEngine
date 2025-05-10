#include "2d_defines.fxh"

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state {
    Texture = <SpriteTexture>;
};

Texture2D InitialTexture;
sampler2D InitialTextureSampler = sampler_state {
    Texture = <InitialTexture>;
    MagFilter = POINT;
    MinFilter = POINT;
    MipFilter = POINT;
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
