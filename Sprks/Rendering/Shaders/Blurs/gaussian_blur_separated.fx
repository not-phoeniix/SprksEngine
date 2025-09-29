#include "../2d_defines.fxh"

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state {
    Texture = <SpriteTexture>;
};

// weights gotten from: http://www.demofox.org/gauss.html
// blur radius of 5 <3
static const float GaussWeights[5] = {0.3829, 0.2417, 0.0606, 0.0060, 0.0002};

float2 ScreenRes;
bool IsVertical;

float4 MainPS(VSOutput input) : COLOR {
    float4 sum = tex2D(SpriteTextureSampler, input.UV) * GaussWeights[0];

    if (IsVertical == true) {
        for (int y = 1; y < 5; y++) {
            float2 offset = float2(0.0, float(y) / ScreenRes.y);

            sum += tex2D(SpriteTextureSampler, input.UV + offset) * GaussWeights[y];
            sum += tex2D(SpriteTextureSampler, input.UV - offset) * GaussWeights[y];
        }
    } else {
        for (int x = 1; x < 5; x++) {
            float2 offset = float2(float(x) / ScreenRes.x, 0.0);

            sum += tex2D(SpriteTextureSampler, input.UV + offset) * GaussWeights[x];
            sum += tex2D(SpriteTextureSampler, input.UV - offset) * GaussWeights[x];
        }
    }

    return sum;
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
