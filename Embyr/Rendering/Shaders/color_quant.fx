#include "frag_header.fxh"

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state {
    Texture = <SpriteTexture>;
};

//* ~~~ shader parameters ~~~

#define PAL_SIZE 46
#define BAYER_SIZE 4

// static const float DitherSpread = 0.3;
static const float DitherSpread = 0.0;
static const float DitherThreshold = 0.0001;
static const int Bayer4[4 * 4] = {
    0, 8, 2, 10,
    12, 4, 14, 6,
    3, 11, 1, 9,
    15, 7, 13, 5
};

float2 ScreenRes;
float4 Palette[PAL_SIZE];

//* ~~~ helpful functions ~~~

// calculates square euclidean distance between two colors (alpha ignored)
float ColorDist(float4 first, float4 second) {
    float rDiff = first.r - second.r;
    float gDiff = first.g - second.g;
    float bDiff = first.b - second.b;
    return rDiff * rDiff + gDiff * gDiff + bDiff * bDiff;
}

// iterates through palette and returns the nearest color using ColorDist
float4 NearestColor(float4 color) {
    float shortestDist = 1000.0;
    int index = -1;

    for (int i = 0; i < PAL_SIZE; i++) {
        float iterDist = ColorDist(color, Palette[i]);

        if (iterDist < shortestDist) {
            shortestDist = iterDist;
            index = i;
        }
    }

    return Palette[index];
}

//* ~~~ main shader things ~~~

// main function
float4 MainPS(VSOutput input) : COLOR {
    int2 pixelPos = input.UV * ScreenRes;

    // grab coordinate by mod-ing screenspace coord
    //   by array size and use that as index for array
    int2 bayerCoord = int2(pixelPos.x % 4, pixelPos.y % 4);
    float ditherValue = Bayer4[4 * bayerCoord.y + bayerCoord.x];

    // convert value to be from -0.5 to 0.5
    ditherValue *= 1.0 / (BAYER_SIZE * BAYER_SIZE);
    ditherValue -= 0.5;

    // calculate a noise-tweaked value by modifying the input color
    //   by the ditherValue, either adding or subtracting the value
    float4 inputColor = tex2D(SpriteTextureSampler, input.UV) * input.Color;
    float4 quantizedColor = NearestColor(inputColor);
    float4 noiseTweakedValue = inputColor + (ditherValue * DitherSpread);
    float4 quantDitherColor = NearestColor(noiseTweakedValue);

    // average both input and noise tweaked colors
    float inputQuantDist = ColorDist(inputColor, quantizedColor);

    // if distance is below threshold, return original
    //   color, otherwise return quantized/dithered color
    if (inputQuantDist <= DitherThreshold) {
        return inputColor;
    } else {
        return quantDitherColor;
    }
}

// shader technique with different passes
technique SpriteDrawing{
    pass P0 {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
