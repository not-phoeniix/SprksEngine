using Embyr.Scenes;
using Embyr.Tools;
using Microsoft.Xna.Framework;

namespace Embyr;

/// <summary>
/// Class that holds information about a "game camera," contains
/// a transformation matrix to be passed into a spritebatch begin
/// call, and methods/properties that can tweak the aspects of
/// the camera itself.
/// </summary>
public class Camera2D {
    private Matrix matrix;
    private Matrix flooredMatrix;
    private Matrix invertedMatrix;
    private Matrix invertedFlooredMatrix;
    private Vector2 position;
    private float zoom;
    private float rotation;
    private bool dirty;
    private readonly int width;
    private readonly int height;

    /// <summary>
    /// Gets the precalculated transform matrix of this camera
    /// </summary>
    public Matrix Matrix {
        get {
            if (dirty) CalculateMatrices();
            return matrix;
        }
    }

    /// <summary>
    /// Gets the precalculated inverted transform matrix of this camera
    /// </summary>
    public Matrix InvertedMatrix {
        get {
            if (dirty) CalculateMatrices();
            return invertedMatrix;
        }
    }

    /// <summary>
    /// Gets precalculated floored position transform matrix of this camera
    /// </summary>
    public Matrix FlooredMatrix {
        get {
            if (dirty) CalculateMatrices();
            return flooredMatrix;
        }
    }

    /// <summary>
    /// Gets the precalculated inverted matrix of the floored transform matrix of this camera
    /// </summary>
    public Matrix InvertedFlooredMatrix {
        get {
            if (dirty) CalculateMatrices();
            return invertedFlooredMatrix;
        }
    }

    /// <summary>
    /// Gets/sets the center-aligned position of camera in world
    /// </summary>
    public Vector2 Position {
        get => position;
        set {
            position = value;
            dirty = true;
        }
    }

    /// <summary>
    /// Gets/sets camera zoom level
    /// </summary>
    public float Zoom {
        get => zoom;
        set {
            zoom = value;
            dirty = true;
        }
    }

    /// <summary>
    /// Gets/sets the rotation of the camera in radians
    /// </summary>
    public float Rotation {
        get => rotation;
        set {
            rotation = value;
            dirty = true;
        }
    }

    /// <summary>
    /// Returns the bounds of the camera view (in world space)
    /// </summary>
    public Rectangle ViewBounds {
        get {
            Rectangle worldView = new(0, 0, width, height);
            return Utils.TransformRect(worldView, invertedMatrix);
        }
    }

    /// <summary>
    /// Creates a new camera object at (0, 0) with zoom of 1
    /// </summary>
    /// <param name="width">Width of camera viewport in pixels</param>
    /// <param name="height">Height of camera viewport in pixels</param>
    public Camera2D(int width, int height) {
        this.width = width;
        this.height = height;
        matrix = Matrix.Identity;
        invertedMatrix = Matrix.Identity;
        Position = Vector2.Zero;
        Zoom = 1f;
        dirty = false;
    }

    /// <summary>
    /// Creates a new camera object at (0, 0) with zoom of 1
    /// </summary>
    /// <param name="resolution">X/Y resolution of camera viewport in pixels</param>
    public Camera2D(Point resolution) : this(resolution.X, resolution.Y) { }

    /// <summary>
    /// Transforms local camera-space position to a world-space position
    /// </summary>
    /// <param name="position">Position to transform</param>
    /// <returns>Position in the world</returns>
    public Vector2 CameraToWorldPos(Vector2 position) {
        return Vector2.Transform(position, invertedMatrix);
    }

    /// <summary>
    /// Updates smooth follow logic, linearly interpolating between
    /// current position and target position according to lerp scale
    /// </summary>
    /// <param name="target">Target position to follow</param>
    /// <param name="lerpAmt">Speed/amount of linear interpolation to apply, must be >= 0</param>
    /// <param name="dt">Time passed since last frame</param>
    public void SmoothFollow(Vector2 target, float lerpAmt, float dt) {
        if (lerpAmt < 0) lerpAmt = 0;
        Vector2 diff = target - Position;
        Position += diff * lerpAmt * dt;
    }

    /// <summary>
    /// Updates smooth follow logic, linearly interpolating between
    /// current position and target position according to lerp scale
    /// </summary>
    /// <param name="target">Target actor to follow</param>
    /// <param name="lerpAmt">Speed/amount of linear interpolation to apply, must be >= 0</param>
    /// <param name="dt">Time passed since last frame</param>
    public void SmoothFollow(IActor2D target, float lerpAmt, float dt) {
        SmoothFollow(target.Transform.Position, lerpAmt, dt);
    }

    private void CalculateMatrices() {
        Vector2 halfSize = new(width / 2, height / 2);
        Vector3 pos3 = new((-position + halfSize) * zoom, 0);

        matrix = Matrix.CreateScale(zoom) * Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(pos3);
        invertedMatrix = Matrix.Invert(matrix);

        flooredMatrix = Matrix.CreateScale(zoom) * Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(Vector3.Floor(pos3));
        invertedFlooredMatrix = Matrix.Invert(flooredMatrix);
    }
}
