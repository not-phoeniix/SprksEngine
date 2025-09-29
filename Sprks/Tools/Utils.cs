using System;
using Microsoft.Xna.Framework;

namespace Sprks.Tools;

/// <summary>
/// A static collection of general functions/utilities I use a bunch
/// </summary>
public static class Utils {
    /// <summary>
    /// Checks for whether two rectangles are above/below each other. (Y is above, and X aligns somewhat)
    /// </summary>
    /// <param name="belowRect">Rect to check if below aboveRect</param>
    /// <param name="aboveRect">Rectangle for reference</param>
    /// <returns>true/false whether or not rectangles are below/above each other</returns>
    public static bool IsBelow(Rectangle belowRect, Rectangle aboveRect) {
        // AABB with only three checks
        return aboveRect.Bottom <= belowRect.Bottom &&
               aboveRect.Left <= belowRect.Right &&
               aboveRect.Right >= belowRect.Left;
    }

    /// <summary>
    /// Checks for whether two rectangles are below/above each other. (Y is below, and X aligns somewhat)
    /// </summary>
    /// <param name="aboveRect">Rect to check if above belowRect</param>
    /// <param name="belowRect">Rectangle for reference</param>
    /// <returns>true/false whether or not rectangles are above/below each other</returns>
    public static bool IsAbove(Rectangle aboveRect, Rectangle belowRect) {
        // AABB with only three checks
        return belowRect.Top >= aboveRect.Top &&
               belowRect.Left <= aboveRect.Right &&
               belowRect.Right >= aboveRect.Left;
    }

    /// <summary>
    /// Checks for whether two rectangles are to the right/left of each othereach other.
    /// (X is to right, and Y aligns somewhat)
    /// </summary>
    /// <param name="rightRect">Rect to check if to right of leftRect</param>
    /// <param name="leftRect">Rectangle for reference</param>
    /// <returns>true/false whether or not rectangles are to right/left of each other</returns>
    public static bool IsToRight(Rectangle rightRect, Rectangle leftRect) {
        // AABB with only three checks
        return rightRect.Left >= leftRect.Left &&
               rightRect.Top <= leftRect.Bottom &&
               rightRect.Bottom >= leftRect.Top;
    }

    /// <summary>
    /// Checks for whether two rectangles are to the left/right of each othereach other.
    /// (X is to right, and Y aligns somewhat)
    /// </summary>
    /// <param name="rightRect">Rectangle for reference</param>
    /// <param name="leftRect">Rect to check if to left of rightRect</param>
    /// <returns>true/false whether or not rectangles are to left/right of each other</returns>
    public static bool IsToLeft(Rectangle leftRect, Rectangle rightRect) {
        // AABB with only three checks
        return leftRect.Right <= rightRect.Right &&
               leftRect.Top <= rightRect.Bottom &&
               leftRect.Bottom >= rightRect.Top;
    }

    /// <summary>
    /// Calculates an expanded rectangle, expanding all sides by the inputted integer value.
    /// </summary>
    /// <param name="rect">Input rectangle</param>
    /// <param name="sizeAdjust">Integer to increase all sides by (can be positive or negative)</param>
    /// <returns>New rectangle, size-adjusted by inputted integer</returns>
    public static Rectangle ExpandRect(Rectangle rect, int sizeAdjust) {
        return new Rectangle(
            rect.X - sizeAdjust,
            rect.Y - sizeAdjust,
            rect.Width + sizeAdjust * 2,
            rect.Height + sizeAdjust * 2);
    }

    /// <summary>
    /// Clamps the magnitude of a vector, preventing magnitude of vector from going past a desired max length.
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="magnitude"></param>
    /// <returns></returns>
    public static Vector2 ClampMagnitude(Vector2 vector, float magnitude) {
        if (vector.LengthSquared() > magnitude * magnitude) {
            return Vector2.Normalize(vector) * magnitude;
        }

        return vector;
    }

    /// <summary>
    /// Transforms an inputted rectangle by a matrix
    /// </summary>
    /// <param name="rect">Rect to transform</param>
    /// <param name="matrix">Matrix to transform by</param>
    /// <returns>Transformed rectangle</returns>
    public static Rectangle TransformRect(Rectangle rect, Matrix matrix) {
        // corner values
        Vector2 topLeft = Vector2.Transform(new Vector2(rect.X, rect.Y), matrix);
        Vector2 topRight = Vector2.Transform(new Vector2(rect.X + rect.Width, rect.Y), matrix);
        Vector2 bottomLeft = Vector2.Transform(new Vector2(rect.X, rect.Y + rect.Height), matrix);
        Vector2 bottomRight = Vector2.Transform(new Vector2(rect.X + rect.Width, rect.Y + rect.Height), matrix);

        // max and min vector points
        Vector2 min = new(
            MathF.Min(topLeft.X, Math.Min(topRight.X, Math.Min(bottomLeft.X, bottomRight.X))),
            MathF.Min(topLeft.Y, Math.Min(topRight.Y, Math.Min(bottomLeft.Y, bottomRight.Y)))
        );
        Vector2 max = new(
            MathF.Max(topLeft.X, Math.Max(topRight.X, Math.Max(bottomLeft.X, bottomRight.X))),
            MathF.Max(topLeft.Y, Math.Max(topRight.Y, Math.Max(bottomLeft.Y, bottomRight.Y)))
        );

        // new assembled rectangle
        return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
    }

    /// <summary>
    /// Shuffles an array using Fisher-Yates shuffle method
    /// </summary>
    /// <param name="input">Input array</param>
    /// <param name="seed">Seed to use with random shuffling</param>
    /// <returns>Shuffled version of inputted array</returns>
    public static int[] ShuffleArray(int[] input, int seed) {
        Random rng = new(seed);

        // iterate backwards
        for (int i = input.Length - 1; i > 0; i--) {
            // random index which hasn't been shuffled yet
            int j = rng.Next(i + 1);

            // swap data at indices
            int tmp = input[i];
            input[i] = input[j];
            input[j] = tmp;
        }

        return input;
    }

    /// <summary>
    /// Converts a "CamelCaseString" into a "snake_case_string"
    /// </summary>
    /// <param name="input">String to convert</param>
    /// <returns>New converted string</returns>
    public static string CamelToSnakeCase(string input) {
        string output = "";

        for (int i = 0; i < input.Length; i++) {
            // place an underscore before every capital letter
            char letter = input[i];
            if (i != 0 && char.IsUpper(letter)) {
                output += $"_{letter}";
            } else {
                output += letter;
            }

        }

        return output.ToLower();
    }

    /// <summary>
    /// Converts a "snake_case_string" into a "CamelCaseString"
    /// </summary>
    /// <param name="input">String to convert</param>
    /// <returns>New converted string</returns>
    public static string SnakeToCamelCase(string input) {
        string output = "";
        input = input.ToLower();

        for (int i = 0; i < input.Length; i++) {
            // place an uppercase letter wherever an underscore is (or first letter)
            char letter = input[i];

            if (i == 0) {
                output += char.ToUpper(letter);
            } else if (letter == '_' && i != input.Length - 1) {
                // skip forward an extra letter to skip the underscore
                output += char.ToUpper(input[i + 1]);
                i++;
            } else {
                output += letter;
            }
        }

        return output;
    }


    /// <summary>
    /// Calculates shortest distance between a 2D vector point and a rectangle
    /// </summary>
    /// <param name="p">Point to check for distance</param>
    /// <param name="r">Rectangle to check for distance</param>
    /// <returns>Closest distance between point and rectangle</returns>
    public static float Distance(Vector2 p, Rectangle r) {
        // https://stackoverflow.com/questions/5254838/calculating-distance-between-a-point-and-a-rectangular-box-nearest-point#18157551

        float dx = MathF.Max(r.Left - p.X, MathF.Max(0, p.X - r.Right));
        float dy = MathF.Max(r.Top - p.Y, MathF.Max(0, p.Y - r.Bottom));
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Calculates shortest distance between a 2D point and a rectangle
    /// </summary>
    /// <param name="p">Point to check for distance</param>
    /// <param name="r">Rectangle to check for distance</param>
    /// <returns>Closest distance between point and rectangle</returns>
    public static float Distance(Point p, Rectangle r) {
        return Distance(p.ToVector2(), r);
    }

    /// <summary>
    /// Calculates shortest squre distance between a 2D vector point and a rectangle, avoids square roots
    /// </summary>
    /// <param name="p">Point to check for distance</param>
    /// <param name="r">Rectangle to check for distance</param>
    /// <returns>Closest square distance between point and rectangle</returns>
    public static float DistanceSquared(Vector2 p, Rectangle r) {
        // https://stackoverflow.com/questions/5254838/calculating-distance-between-a-point-and-a-rectangular-box-nearest-point#18157551

        float dx = MathF.Max(r.Left - p.X, MathF.Max(0, p.X - r.Right));
        float dy = MathF.Max(r.Top - p.Y, MathF.Max(0, p.Y - r.Bottom));
        return dx * dx + dy * dy;
    }

    /// <summary>
    /// Calculates shortest squre distance between a 3D vector point and a bounding box, avoids square roots
    /// </summary>
    /// <param name="p">Point to check for distance</param>
    /// <param name="b">Bounding box to check for distance</param>
    /// <returns>Closest square distance between point and bounding box</returns>
    public static float DistanceSquared(Vector3 p, BoundingBox b) {
        // https://stackoverflow.com/questions/5254838/calculating-distance-between-a-point-and-a-rectangular-box-nearest-point#18157551

        float dx = MathF.Max(b.Min.X - p.X, MathF.Max(0, p.X - b.Max.X));
        float dy = MathF.Max(b.Min.Y - p.Y, MathF.Max(0, p.Y - b.Max.Y));
        float dz = MathF.Max(b.Min.Z - p.Z, MathF.Max(0, p.Z - b.Max.Z));
        return dx * dx + dy * dy + dz * dz;
    }

    /// <summary>
    /// Calculates shortest squre distance between a 2D point and a rectangle, avoids square roots
    /// </summary>
    /// <param name="p">Point to check for distance</param>
    /// <param name="r">Rectangle to check for distance</param>
    /// <returns>Closest square distance between point and rectangle</returns>
    public static float DistanceSquared(Point p, Rectangle r) {
        return DistanceSquared(p.ToVector2(), r);
    }
}
