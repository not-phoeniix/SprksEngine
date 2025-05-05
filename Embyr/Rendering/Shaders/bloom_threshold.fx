#include "frag_header.fxh"

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state {
    Texture = <SpriteTexture>;
};

float Threshold;

float4 MainPS(VSOutput input) : COLOR {
    float4 inputColor = tex2D(SpriteTextureSampler, input.UV);
    float luminance = dot(inputColor.rgb, float3(0.299, 0.587, 0.144));
    return luminance >= Threshold ? inputColor : float4(0.0, 0.0, 0.0, 1.0);
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
