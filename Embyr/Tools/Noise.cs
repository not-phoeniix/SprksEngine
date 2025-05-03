using System;
using Microsoft.Xna.Framework;

namespace Embyr.Tools;

/// <summary>
/// Custom static noise library for perlin-like and FBM noise
/// </summary>
public static class Noise {
    // DISCLAIMER:
    //   algorithm from https://rtouti.github.io/graphics/perlin-noise-algorithm
    //   this is a cool algorithm but i did not make it myself

    /// <summary>
    /// 2 dimensional Perlin-like noise
    /// </summary>
    /// <param name="x">X coordinate of noise (use non-integers)</param>
    /// <param name="y">Y coordinate of noise (use non-integers)</param>
    /// <param name="seed">Seed of random function</param>
    /// <returns>Single float value between -1.0 and 1.0 at coordinate x/y</returns>
    public static float Noise2D(float x, float y, int seed) {
        // floored x/y values, repeats every 256 values (0-255)
        int xFloored = (int)Math.Floor(x) % 256;
        int yFloored = (int)Math.Floor(y) % 256;

        // negative number wraparound fix
        if (xFloored < 0) xFloored += 256;
        if (yFloored < 0) yFloored += 256;

        // will be from 0-1, x/y offset values from the floored integers
        float localX = x - MathF.Floor(x);
        float localY = y - MathF.Floor(y);

        // position of current point, relative to each corner
        // values you'd need to add to each corner to reach inputted value
        Vector2 fromTopRight = new(localX - 1, localY - 1);
        Vector2 fromTopLeft = new(localX, localY - 1);
        Vector2 fromBottomRight = new(localX - 1, localY);
        Vector2 fromBottomLeft = new(localX, localY);

        // array from 0-255, shuffled and doubled
        int[] permutation = GeneratePermutation(seed);

        // corner values (all should be the same)
        int valueTopRight = permutation[permutation[xFloored + 1] + yFloored + 1];
        int valueTopLeft = permutation[permutation[xFloored] + yFloored + 1];
        int valueBottomRight = permutation[permutation[xFloored + 1] + yFloored];
        int valueBottomLeft = permutation[permutation[xFloored] + yFloored];

        // dot product of constant vectors and vectors pointing inward to input point
        float dotTopRight = Vector2.Dot(fromTopRight, GetConstantVector(valueTopRight));
        float dotTopLeft = Vector2.Dot(fromTopLeft, GetConstantVector(valueTopLeft));
        float dotBottomRight = Vector2.Dot(fromBottomRight, GetConstantVector(valueBottomRight));
        float dotBottomLeft = Vector2.Dot(fromBottomLeft, GetConstantVector(valueBottomLeft));

        // ease curve calculations
        float easeX = Fade(localX);
        float easeY = Fade(localY);

        // double linear interpolation in the 4 axes using the ease curves
        return MathHelper.Lerp(
            MathHelper.Lerp(dotBottomLeft, dotTopLeft, easeY),
            MathHelper.Lerp(dotBottomRight, dotTopRight, easeY),
            easeX
        );
    }

    /// <summary>
    /// 1 dimensional Perlin-like noise
    /// </summary>
    /// <param name="x">X coordinate of noise (use non-integers)</param>
    /// <param name="seed">Seed of random function</param>
    /// <returns>Single float value between -1.0 and 1.0 at coordinate x</returns>
    public static float Noise1D(float x, int seed) {
        // the non-zero Y value here ensures negative values are possible
        return Noise2D(x, 0.4f, seed);
    }

    /// <summary>
    /// 2 dimensional Fractal Brownian Motion noise function,
    /// adds roughness to perlin-like noise algorithm
    /// </summary>
    /// <param name="x">X coordinate of noise</param>
    /// <param name="y">Y coordinate of noise</param>
    /// <param name="octaves">
    /// Number of octave passes added to roughen noise
    /// (higher num == more jaggedy noise)
    /// </param>
    /// <param name="seed">Seed of random function</param>
    /// <returns>Single float value between -1.0 and 1.0 at coordinate x/y</returns>
    public static float FBM2D(float x, float y, int octaves, int seed) {
        float result = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int o = 0; o < octaves; o++) {
            float noiseValue = amplitude * Noise2D(x * frequency, y * frequency, seed);
            result += noiseValue;

            amplitude *= 0.5f;
            frequency *= 2;
        }

        return result;
    }

    /// <summary>
    /// 1 dimensional Fractal Brownian Motion noise function,
    /// adds roughness to perlin-like noise algorithm
    /// </summary>
    /// <param name="x">X coordinate of noise</param>
    /// <param name="octaves">
    /// Number of octave passes added to roughen noise
    /// (higher num == more jaggedy noise)
    /// </param>
    /// <param name="seed">Seed of random function</param>
    /// <returns>Single float value between -1.0 and 1.0 at coordinate x</returns>
    public static float FBM1D(float x, int octaves, int seed) {
        return FBM2D(x, 0.1f, octaves, seed);
    }

    /// <summary>
    /// Generates a random 1D Gaussian value
    /// </summary>
    /// <param name="mean">Mean/average value in normal distribution</param>
    /// <param name="stdDev">Standard deviation or "spread" of gaussian values</param>
    /// <returns>Float of gaussian noise value</returns>
    public static float Gaussian(float mean, float stdDev) {
        // stolen from IGME 202 #slay, using the "Box-Muller Transform"
        //   https://mathworld.wolfram.com/Box-MullerTransformation.html

        float val1 = Random.Shared.NextSingle();
        float val2 = Random.Shared.NextSingle();

        float gaussValue =
            MathF.Sqrt(-2.0f * MathF.Log(val1)) *
            MathF.Sin(2.0f * MathF.PI * val2);

        return mean + stdDev * gaussValue;
    }

    /// <summary>
    /// Generates a random float between a min and max bound
    /// </summary>
    /// <param name="min">Inclusive lower bound</param>
    /// <param name="max">Exclusive upper bound</param>
    /// <returns>Random float value inside a range</returns>
    public static float NextSingle(float min, float max) {
        return MathHelper.Lerp(min, max, Random.Shared.NextSingle());
    }

    /// <summary>
    /// Generates a random float between a min and max bound
    /// </summary>
    /// <param name="rng">Random object to generate noise with</param>
    /// <param name="min">Inclusive lower bound</param>
    /// <param name="max">Exclusive upper bound</param>
    /// <returns>Random float value inside a range</returns>
    public static float NextSingle(this Random rng, float min, float max) {
        return MathHelper.Lerp(min, max, rng.NextSingle());
    }

    #region // Helper methods

    private static int[] GeneratePermutation(int seed) {
        // creates and shuffles an int array from 0-255
        int[] tmp = new int[256];
        for (int i = 0; i < 256; i++) {
            tmp[i] = i;
        }
        tmp = Utils.ShuffleArray(tmp, seed);

        // double the array??
        int[] permutation = new int[tmp.Length * 2];
        for (int i = 0; i < 512; i++) {
            int index = i % 256;
            permutation[i] = tmp[index];
        }

        return permutation;
    }

    // return 1 of 4 vectors depending on input int
    private static Vector2 GetConstantVector(int value) {
        int h = value % 4;

        return h switch {
            0 => new Vector2(1, 1),
            1 => new Vector2(-1, 1),
            2 => new Vector2(-1, -1),
            _ => new Vector2(1, -1)
        };
    }

    // ease curve
    private static float Fade(float value) {
        return ((6 * value - 15) * value + 10) * value * value * value;
    }

    #endregion
}
