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
public class Camera3D {
    /// <summary>
    /// Enumeration of all possible camera projection types
    /// </summary>
    public enum Projection {
        Orthographic,
        Perspective
    }

    private Matrix viewProjMat;
    private Projection projectionType;
    private float prevAspectRatio;
    private bool projectionDirty;
    private float fov;
    private float orthoWidth;
    private float orthoHeight;
    private float nearPlaneDist;
    private float farPlaneDist;

    /// <summary>
    /// Gets the transform of this camera
    /// </summary>
    public Transform3D Transform { get; }

    /// <summary>
    /// Gets/sets projection type for this camera
    /// </summary>
    public Projection ProjectionType {
        get => projectionType;
        set {
            projectionType = value;
            projectionDirty = true;
        }
    }

    /// <summary>
    /// Gets the view matrix of this camera
    /// </summary>
    public Matrix ViewMatrix { get; private set; }

    /// <summary>
    /// Gets the projection matrix of this camera
    /// </summary>
    public Matrix ProjectionMatrix { get; private set; }

    /// <summary>
    /// Gets the bounds of the camera view (in world space)
    /// </summary>
    public BoundingFrustum ViewBounds { get; private set; }

    /// <summary>
    /// Gets/sets the field of view of this camera when in perspective mode
    /// </summary>
    public float PerspectiveFOV {
        get => fov;
        set {
            fov = value;
            projectionDirty = true;
        }
    }

    /// <summary>
    /// Gets/sets the width of this camera when in orthographic mode
    /// </summary>
    public float OrthoWidth {
        get => orthoWidth;
        set {
            orthoWidth = value;
            projectionDirty = true;
        }
    }

    /// <summary>
    /// Gets/sets the height of this camera when in orthographic mode
    /// </summary>
    public float OrthoHeight {
        get => orthoHeight;
        set {
            orthoHeight = value;
            projectionDirty = true;
        }
    }

    /// <summary>
    /// Gets/sets the near plane distance of this camera
    /// </summary>
    public float NearPlaneDist {
        get => nearPlaneDist;
        set {
            nearPlaneDist = value;
            projectionDirty = true;
        }
    }

    /// <summary>
    /// Gets/sets the far plane distance of this camera
    /// </summary>
    public float FarPlaneDist {
        get => farPlaneDist;
        set {
            farPlaneDist = value;
            projectionDirty = true;
        }
    }

    /// <summary>
    /// Creates a new Camera3D object
    /// </summary>
    /// <param name="position">Position of camera in the world</param>
    /// <param name="nearPlane">Near plane distance</param>
    /// <param name="farPlane">Far plane distance</param>
    public Camera3D(Vector3 position, float nearPlane, float farPlane) {
        ProjectionType = Projection.Perspective;
        Transform = new Transform3D(position);
        LookAt(position + new Vector3(0, 0, 1));
        this.NearPlaneDist = nearPlane;
        this.FarPlaneDist = farPlane;
        this.PerspectiveFOV = MathHelper.ToRadians(65);
        this.ProjectionType = Projection.Perspective;
        this.OrthoWidth = 16;
        this.OrthoHeight = 9;
    }

    /// <summary>
    /// Updates camera matrices
    /// </summary>
    public void Update(float aspectRatio) {
        // clamp looking rotation to prevent upside down looking!
        Transform.GlobalRotation = new Vector3(
            Math.Clamp(Transform.GlobalRotation.X, MathHelper.ToRadians(-89), MathHelper.ToRadians(89)),
            Transform.GlobalRotation.Y,
            Transform.GlobalRotation.Z
        );

        UpdateView();
        if (projectionDirty || aspectRatio != prevAspectRatio) {
            UpdateProjection(aspectRatio);
        }

        viewProjMat = ViewMatrix * ProjectionMatrix;
        ViewBounds = new BoundingFrustum(viewProjMat);
        prevAspectRatio = aspectRatio;
    }

    /// <summary>
    /// Rotates camera to look at a world position, locks roll to zero
    /// </summary>
    /// <param name="targetPosition">Position to look at</param>
    public void LookAt(Vector3 targetPosition) {
        Vector3 delta = targetPosition - Transform.GlobalPosition;
        if (delta.LengthSquared() > float.Epsilon) delta.Normalize();

        float yaw = MathF.Atan2(delta.X, delta.Z);
        float pitch = MathF.Asin(-delta.Y);
        Transform.GlobalRotation = new Vector3(pitch, yaw, 0);
    }

    // /// <summary>
    // /// Updates smooth follow logic, linearly interpolating between
    // /// current position and target position according to lerp scale
    // /// </summary>
    // /// <param name="target">Target position to follow</param>
    // /// <param name="lerpAmt">Speed/amount of linear interpolation to apply, must be >= 0</param>
    // /// <param name="dt">Time passed since last frame</param>
    // public void SmoothFollow(Vector3 target, float lerpAmt, float dt) {
    //     if (lerpAmt < 0) lerpAmt = 0;
    //     Vector2 diff = target - Position;
    //     Position += diff * lerpAmt * dt;
    // }

    // /// <summary>
    // /// Updates smooth follow logic, linearly interpolating between
    // /// current position and target position according to lerp scale
    // /// </summary>
    // /// <param name="target">Target actor to follow</param>
    // /// <param name="lerpAmt">Speed/amount of linear interpolation to apply, must be >= 0</param>
    // /// <param name="dt">Time passed since last frame</param>
    // public void SmoothFollow(IActor3D target, float lerpAmt, float dt) {
    //     SmoothFollow(target.Transform.Position, lerpAmt, dt);
    // }

    private void UpdateView() {
        ViewMatrix = Matrix.CreateLookAt(
            Transform.GlobalPosition,
            Transform.GlobalPosition + Transform.Forward,
            Vector3.Up
        );
    }

    private void UpdateProjection(float aspectRatio) {
        switch (ProjectionType) {
            case Projection.Perspective:
                ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                    PerspectiveFOV,
                    aspectRatio,
                    NearPlaneDist,
                    FarPlaneDist
                );
                break;

            case Projection.Orthographic:
                ProjectionMatrix = Matrix.CreateOrthographic(
                    OrthoWidth,
                    OrthoHeight,
                    NearPlaneDist,
                    FarPlaneDist
                );
                break;
        }

        projectionDirty = false;
    }
}
