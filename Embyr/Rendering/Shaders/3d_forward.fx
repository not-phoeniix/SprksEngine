#include "3d_defines.fxh"

#define MAX_SPECULAR_EXP 256.0f

matrix World;
matrix View;
matrix Projection;
matrix WorldInverseTranspose;

float3 AmbientColor;
float3 SurfaceColor;
float3 CamWorldPos;
float Gamma;
float Roughness;

PSInput MainVS(in VSInput input) {
    PSInput output = (PSInput)0;

    matrix wvp = mul(World, mul(View, Projection));
    output.Position = mul(input.Position, wvp);
    output.WorldPosition = mul(input.Position, World).xyz;
    output.Normal = normalize(mul(input.Normal.xyz, (float3x3)WorldInverseTranspose));

    return output;
}

float3 LambertDiffuse(float3 lightDir, float lightIntensity, float3 lightColor, float3 surfaceNormal) {
    return max(dot(-lightDir, surfaceNormal), 0.0f) * lightIntensity * lightColor;
}

float3 PhongSpecular(float3 lightDir, float lightIntensity, float3 lightColor, float3 surfaceNormal, float3 fragWorldPos) {
    float specularExp = (1.0f - Roughness) * MAX_SPECULAR_EXP;
    float3 reflected = reflect(lightDir, surfaceNormal);
    float3 toCam = normalize(CamWorldPos - fragWorldPos);

    float3 specularTerm =
        pow(max(dot(reflected, toCam), 0), specularExp) *
        lightIntensity *
        lightColor;
    specularTerm *= specularExp > 0.001f;

    return specularTerm;
}

float3 DirectionalLight(float3 lightDir, float lightIntensity, float3 lightColor, float3 surfaceNormal, float3 fragWorldPos) {
    float3 diffuse = LambertDiffuse(lightDir, lightIntensity, lightColor, surfaceNormal);
    float3 specular = PhongSpecular(lightDir, lightIntensity, lightColor, surfaceNormal, fragWorldPos);

    // don't apply specular when diffuse is black
    specular *= any(diffuse);

    return (diffuse * SurfaceColor) + specular;
}

float Attenuate(float3 lightPos, float lightRange, float3 fragWorldPos) {
    float dist = distance(lightPos, fragWorldPos);
    float att = saturate(1.0f - (dist * dist / (lightRange * lightRange)));
    return att * att;
}

float4 MainPS(PSInput input) : COLOR {
    // re-normalize to fix any interpolation weirdness
    input.Normal = normalize(input.Normal);

    float3 lightSum = float3(0.0f, 0.0f, 0.0f);

    lightSum += AmbientColor * SurfaceColor;

    lightSum += DirectionalLight(
        normalize(float3(0.42f, -3.92f, 0.91f)),
        0.7f,
        float3(1.0f, 0.2f, 0.2f),
        input.Normal,
        input.WorldPosition);

    // gamma correction :]
    return float4(pow(lightSum, 1.0f / max(Gamma, 0.001f)), 1.0f);
}

technique MainShading {
    pass P0 {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
