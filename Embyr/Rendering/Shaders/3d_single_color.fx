#include "3d_defines.fxh"

#define MAX_SPECULAR_EXP 256.0f
#define MAX_LIGHTS 32

matrix World;
matrix View;
matrix Projection;
matrix WorldInverseTranspose;

float3 Color;

PSInput MainVS(in VSInput input) {
    PSInput output = (PSInput)0;

    matrix wvp = mul(World, mul(View, Projection));
    output.Position = mul(input.Position, wvp);
    output.WorldPosition = mul(input.Position, World).xyz;
    output.Normal = normalize(mul(input.Normal.xyz, (float3x3)WorldInverseTranspose));

    return output;
}

float4 MainPS(PSInput input) : COLOR {
    return float4(Color, 1.0);
}

technique MainShading {
    pass P0 {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
