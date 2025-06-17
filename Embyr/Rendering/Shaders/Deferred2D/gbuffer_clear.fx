#include "../2d_defines.fxh"

struct PSOutput {
    float4 Albedo : COLOR0;         // render target 0
    float4 NormalDepth : COLOR1;    // render target 1
    float4 Obstructors : COLOR2;    // render target 2
};

float4 AlbedoClearColor;
float4 NormalDepthClearColor;
float4 ObstructorsClearColor;

PSOutput MainPS(VSOutput input) {
    PSOutput output;

    output.Albedo = AlbedoClearColor;
    output.NormalDepth = NormalDepthClearColor;
    output.Obstructors = ObstructorsClearColor;

    return output;
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
