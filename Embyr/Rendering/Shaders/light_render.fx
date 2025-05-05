#include "frag_header.fxh"

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state {
    Texture = <SpriteTexture>;
};

Texture2D SkyDistanceField;
sampler2D SkyDistanceFieldSampler = sampler_state {
    Texture = <SkyDistanceField>;
};

//* ~~~ parameters ~~~

#define MAX_LIGHTS 8
#define MAX_RAYMARCHES 32
#define M_PI 3.14159265f
#define RAYMARCH_DIST_THRESHOLD 0.0001
#define MAIN_TILE_DEPTH 0.25
#define SKY_DEPTH 1.0
#define SKY_EXP 25.0
#define SKY_BUMP_VALUE 0.01

int NumLights;
float2 ScreenRes;
float3 Positions[MAX_LIGHTS];
float3 Colors[MAX_LIGHTS];
float Intensities[MAX_LIGHTS];
float Rotations[MAX_LIGHTS];
float4 SizeParams[MAX_LIGHTS];

//* ~~~ functions ~~~

float makePositiveAngle(float angle) {
    // get sign and integer number of times the angle goes into 360 deg
    float signValue = sign(angle);
    float wholeDivisions = floor(abs(angle) / (M_PI * 2.0));

    // we have one more division to add if it's negative
    if (signValue == -1.0) {
        wholeDivisions += 1.0;
    }

    // calculate rotated angle and return
    return angle - (M_PI * 2.0 * wholeDivisions * signValue);
}

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

// creates an angular scalar for lighting
float angleDist(float fragAngle, float facingAngle, float angularWidth, float smoothFactor) {
    float fragAngleAdjusted = makePositiveAngle(fragAngle);
    float facingAngleAdjusted = makePositiveAngle(facingAngle);

    float distOne = abs(fragAngle - facingAngle);
    float distTwo = abs(fragAngleAdjusted - facingAngle);
    float distThree = abs(fragAngle - facingAngleAdjusted);
    float distFour = abs(fragAngleAdjusted - facingAngleAdjusted);
    float dist = min(distOne, min(distTwo, min(distThree, distFour)));

    return smoothstep(
        (angularWidth / 2.0) - smoothFactor,
        (angularWidth / 2.0) + smoothFactor,
        dist + (smoothFactor * 0.25));
}

// quantizes a continuous float value from 0.0-1.0 to be in even discreet steps
float quantize(float value, uint numSteps) {
    value *= float(numSteps);
    value = floor(value);
    return value / float(numSteps);
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

    // if depth indicates this pixel is a tile, just exit early and don't raymarch
    float depth = tex2D(SpriteTextureSampler, texCoord + dir * distSum).z;
    if (depth > MAIN_TILE_DEPTH - 0.01 && depth < MAIN_TILE_DEPTH + 0.01) {
        return 0.0;
    }

    [unroll(MAX_RAYMARCHES)] for (float i = 0.0; i < MAX_RAYMARCHES; i++) {
        // dist represents the closest distance of any object in the entire scene
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

float4 LocalLight(float2 uv, float2 position, float rotation, float3 color, float radius, float angularWidth, float linearFalloff, float angularFalloff, float intensity) {
    float2 fragCoordPixel = uv * ScreenRes;
    float2 centerPixelPos = position * ScreenRes;
    float aspect = ScreenRes.x / ScreenRes.y;

    // value from 0-1 on brightness of a light circle
    float linearScalar = saturate(1.0 - pixelCircle(centerPixelPos, fragCoordPixel, radius, linearFalloff));

    // ~~~ angular calculations ~~~

    float2 centerToFrag = uv - position;
    centerToFrag.x *= aspect;
    float angleToFrag = atan2(centerToFrag.y, centerToFrag.x);

    float angleScalar = clamp(1.0 - angleDist(angleToFrag, rotation, angularWidth, angularFalloff), 0.0, 1.0);

    float finalIntensity = linearScalar * angleScalar * intensity;
    float4 lightColor = float4(color, 1.0) * finalIntensity;

    // do raymarch, multiply output scalar to mask lights when in shadow
    lightColor *= softShadow(position, uv, 8.0);

    return lightColor;
}

float4 GlobalLight(float2 uv, float rotation, float3 color, float intensity) {
    float4 globalColor = float4(color, 1.0) * intensity;

    float4 skySample = tex2D(SkyDistanceFieldSampler, uv);
    float distToSky = skySample.x;
    float depth = skySample.z;

    float expScalar = depth <= 0.26 ? 1.0 : 0.2;
    float exp = SKY_EXP * expScalar;
    float skyTerm = pow(saturate(1.0 - distToSky + SKY_BUMP_VALUE), exp);

    globalColor *= skyTerm;

    // position to light is relative to direction per frag
    // float2 dir = float2(cos(rotation), sin(rotation));
    // float2 globalPos = uv + dir * 3.0;

    // globalColor *= softShadow(globalPos, uv, 512.0);

    return globalColor;
}

//* ~~~ main stuff ~~~

//! input drawn texture should be distance field!

float4 MainPS(VSOutput input) : COLOR {
    float2 fragCoordPixel = input.UV * ScreenRes;
    float4 additiveTotalColor = float4(0.0, 0.0, 0.0, 0.0);

    // iterate across all lights and sum all light colors
    [unroll(MAX_LIGHTS)] for (int i = 0; i < MAX_LIGHTS; i++) {
        // break loop when number of lights
        //   in scene has been surpassed
        if (i >= NumLights) {
            break;
        }

        // ~~~ initial light calculations ~~~
        float radius = SizeParams[i].x;
        float angularWidth = SizeParams[i].y;
        float linearFalloff = SizeParams[i].z;
        float angularFalloff = SizeParams[i].w;
        bool isGlobal = Positions[i].z > 0.0;
        float2 position = Positions[i].xy;

        float4 lightColor = float4(0.0, 0.0, 0.0, 0.0);

        if (isGlobal) {
            lightColor = GlobalLight(
                input.UV,
                Rotations[i],
                Colors[i],
                Intensities[i]);
        } else {
            lightColor = LocalLight(
                input.UV,
                position,
                Rotations[i],
                Colors[i],
                radius,
                angularWidth,
                linearFalloff,
                angularFalloff,
                Intensities[i]);
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
