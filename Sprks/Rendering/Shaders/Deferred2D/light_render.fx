#include "../2d_defines.fxh"

// input should be distance field where each pixel represents the
//   distance to the closest light-obstructing object
Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state {
    Texture = <SpriteTexture>;
};

Texture2D NormalMap;
sampler2D NormalMapSampler = sampler_state {
    Texture = <NormalMap>;
};

Texture2D DepthBuffer;
sampler2D DepthBufferSampler = sampler_state {
    Texture = <DepthBuffer>;
};

//* ~~~ parameters ~~~

#define MAX_LIGHTS 8
#define MAX_RAYMARCHES 16
#define M_PI 3.14159265f
#define RAYMARCH_DIST_THRESHOLD 0.0001
#define MAIN_TILE_DEPTH 0.25

int NumLights;
float2 ScreenRes;
float4 Positions[MAX_LIGHTS];
float3 Colors[MAX_LIGHTS];
float Intensities[MAX_LIGHTS];
float Rotations[MAX_LIGHTS];
float4 SizeParams[MAX_LIGHTS];
float CastsShadow[MAX_LIGHTS];
float Depth3DScalar;

//* ~~~ functions ~~~

// creates a pixel-relative circle float value, where 0.0-1.0 signifies circle-nocircle
float pixelCircle(float2 centerPixel, float2 fragCoordPixel, float pixelRadius, float smoothFactor) {
    float len = length(centerPixel - fragCoordPixel);

    // tweak length by smooth factor so that the circle
    //   doesn't get bigger with bigger smooth factors
    return smoothstep(
        pixelRadius - smoothFactor,
        pixelRadius + smoothFactor,
        len + (smoothFactor * 0.25));
}

// uses distance field to march rays for collision checking
//   returns a scaling factor, either 0 or 1,
//   to multiply a light value by
float hardShadow(float2 lightPos, float2 texCoord) {
    float distToLight = length(lightPos - texCoord);
    float2 dir = (lightPos - texCoord) / distToLight;
    float distSum = 0.01;

    [unroll(MAX_RAYMARCHES)] for (float i = 0.0; i < MAX_RAYMARCHES; i++) {
        float2 samplePos = texCoord + dir * distSum;

        // dist represents the closest distance of any object in the entire scene
        float dist = tex2D(SpriteTextureSampler, samplePos).r;

        // exit loop if distance travelled so far surpasses target distance to light
        if (distSum >= distToLight) {
            break;
        }

        // if distance is real short, we know an intersection has occured
        if (dist <= RAYMARCH_DIST_THRESHOLD) {
            return 0.0;
        }

        // if nothing has exited yet, march the sample pos forward!
        distSum += dist;
    }

    return 1.0;
}

// uses distance field to march rays for collision checking
//   returns a scaling factor, either 0 or 1,
//   to multiply a light value by
// https://iquilezles.org/articles/rmshadows/
float softShadow(float2 lightPos, float2 texCoord, float k) {
    float distToLight = length(lightPos - texCoord);
    float2 dir = (lightPos - texCoord) / distToLight;
    float distSum = 0.0;
    float penumbraVal = 1.0;

    // if depth indicates this pixel is an obstructor, just exit early and don't raymarch
    // float obstructorDepth = tex2D(DepthBufferSampler, texCoord).y;
    // if (obstructorDepth <= 0.001) {
    //     return 0.0;
    // }

    [unroll(MAX_RAYMARCHES)] for (float i = 0.0; i < MAX_RAYMARCHES; i++) {
        // dist represents the closest distance to any light-obstructing
        //   object in the entire scene
        float dist = tex2D(SpriteTextureSampler, texCoord + dir * distSum).r;

        // exit loop if distance travelled so far surpasses target distance to light
        if (distSum >= distToLight) {
            break;
        }

        // if distance is real short, we know an intersection has occured
        if (dist <= RAYMARCH_DIST_THRESHOLD) {
            return 0.0;
        }

        // if nothing has exited yet, update penumbra value
        //   based on distance and march the sample pos forward!
        penumbraVal = min(penumbraVal, k * dist / max(distSum, 0.00001));
        distSum += dist;
    }

    return penumbraVal;
}

//! NOTE: function returns a float4 so we can use alpha
//!   channel to blend with previous pass

float4 LocalLight(float2 uv, float3 normal, float4 depth, float2 position, float zIndex, float rotation, float3 color, float radius, float linearFalloff, float innerAngle, float outerAngle, float intensity, bool castsShadow) {
    float2 fragCoordPixel = uv * ScreenRes;
    float2 centerPixelPos = position * ScreenRes;
    float aspect = ScreenRes.x / ScreenRes.y;

    // ~~~ linear scalar calculations ~~~

    float linearScalar = saturate(1.0 - pixelCircle(centerPixelPos, fragCoordPixel, radius, linearFalloff));

    // ~~~ angular scalar calculations ~~~

    float2 toFrag = normalize(uv - position);
    float2 lightDir = float2(cos(rotation), sin(rotation));

    // get saturated cosine of angle between light and frag direction
    float cosDirFrag = saturate(dot(toFrag, lightDir));

	// outer is restricted so lights aren't inverted, set to slightly larget when equal or less
    float cosInner = cos(innerAngle);
    float cosOuter = cos(max(outerAngle, innerAngle + 0.0001));

    float falloffRange = cosOuter - cosInner;
    float angleScalar = saturate((cosOuter - cosDirFrag) / falloffRange);

    // ~~~ normal calculations ~~~
    float3 lightPos = float3(position, zIndex * Depth3DScalar);
    float3 surfacePos = float3(uv, depth.x * Depth3DScalar);
    float3 dir = normalize(lightPos - surfacePos);
    float normalTerm = saturate(dot(normal, dir));

    // calculate color before shadow scaling
    float finalIntensity = linearScalar * angleScalar * normalTerm * intensity;
    float4 lightColor = float4(color, 1.0) * finalIntensity;

    // do raymarch if shadowcasting is enabled, multiply output scalar
    //   to mask lights when in shadow
    lightColor *= castsShadow ? softShadow(position, uv, 8.0) : 1.0;

    return lightColor;
}

//! NOTE: function returns a float4 so we can use alpha
//!   channel to blend with previous pass

float4 GlobalLight(float2 uv, float3 normal, float4 depth, float zIndex, float rotation, float3 color, float intensity, bool castsShadow) {
    float2 flatDir = float2(cos(rotation), sin(rotation));
    float3 dir = normalize(float3(flatDir, -zIndex * Depth3DScalar));
    float normalTerm = saturate(dot(normal, -dir));

    float4 globalLightColor = float4(color, 1.0) * intensity * normalTerm;

    // position to light is relative to direction per frag
    float2 globalPos = uv + flatDir * 3.0;
    globalLightColor *= castsShadow ? softShadow(globalPos, uv, 512.0) : 1.0;

    return globalLightColor;
}

//* ~~~ main stuff ~~~

//! input drawn texture should be distance field!

float4 MainPS(VSOutput input) : COLOR {
    float4 additiveTotalColor = float4(0.0, 0.0, 0.0, 0.0);

    // unpack normals, get in range of -1 to 1
    float3 normal = tex2D(NormalMapSampler, input.UV).xyz;
    normal.y = 1.0f - normal.y;
    normal = normalize((normal * 2.0) - float3(1.0, 1.0, 1.0));

    // get depth color
    float4 depth = tex2D(DepthBufferSampler, input.UV);

    // iterate across all lights and sum all light colors
    [unroll(MAX_LIGHTS)] for (int i = 0; i < NumLights; i++) {
        // ~~~ initial light calculations ~~~
        float radius = SizeParams[i].x;
        float linearFalloff = SizeParams[i].y;
        float innerAngle = SizeParams[i].z;
        float outerAngle = SizeParams[i].w;
        bool isGlobal = Positions[i].w > 0.0;
        float2 position = Positions[i].xy;
        float zIndex = Positions[i].z;
        bool castsShadow = CastsShadow[i] > 0.0;

        float4 lightColor = float4(0.0, 0.0, 0.0, 0.0);

        if (isGlobal) {
            lightColor = GlobalLight(
                input.UV,
                normal,
                depth,
                zIndex,
                Rotations[i],
                Colors[i],
                Intensities[i],
                castsShadow);
        } else {
            lightColor = LocalLight(
                input.UV,
                normal,
                depth,
                position,
                zIndex,
                Rotations[i],
                Colors[i],
                radius,
                linearFalloff,
                innerAngle,
                outerAngle,
                Intensities[i],
                castsShadow);
        }

        additiveTotalColor += lightColor;
    }

    return additiveTotalColor;
}

technique SpriteDrawing {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
