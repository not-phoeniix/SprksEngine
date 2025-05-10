#include "3d_defines.fxh"

matrix World;
matrix View;
matrix Projection;
matrix WorldInverseTranspose;

float3 AmbientColor;

PSInput MainVS(in VSInput input) {
    PSInput output = (PSInput)0;

    matrix wvp = mul(World, mul(View, Projection));
    output.Position = mul(input.Position, wvp);
    output.Color = input.Color;
    output.Normal = normalize(mul(input.Normal.xyz, (float3x3)WorldInverseTranspose));

    return output;
}

float4 MainPS(PSInput input) : COLOR {
    // re-normalize to fix any interpolation weirdness
    input.Normal = normalize(input.Normal);

    float3 lightSum = float3(0.0, 0.0, 0.0);

    lightSum += AmbientColor;

    float3 diffuse = saturate(dot(input.Normal, normalize(float3(-1.0, 1.0, -2.0)))) * float3(1.0, 0.2, 0.2);

    lightSum += diffuse;

    return float4(lightSum, 1.0);
}

technique MainShading {
    pass P0 {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
