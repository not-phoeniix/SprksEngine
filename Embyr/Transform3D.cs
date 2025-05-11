using Microsoft.Xna.Framework;

namespace Embyr;

/// <summary>
/// A sealed transform class holding 3D position, scaling, and rotation information
/// </summary>
public sealed class Transform3D {
    private Transform3D? parent;
    private readonly List<Transform3D> children;

    private Vector3 localPos;
    private Vector3 localScale;
    private Vector3 localRotation;

    bool parentGlobalsDirty;
    private Vector3 parentGlobalPos;
    private Vector3 parentGlobalScale;
    private Vector3 parentGlobalRotation;

    private bool matricesDirty;
    private Matrix worldMatrix;
    private Matrix worldInverseTranspose;

    private bool directionalsDirty;
    private Vector3 forward;
    private Vector3 right;
    private Vector3 up;

    /// <summary>
    /// Gets/sets the parent for this transform
    /// </summary>
    public Transform3D? Parent {
        get => parent;
        set {
            // apply offsets whenever changing what the parent is
            if (parentGlobalsDirty) RecalculateParentGlobals();
            localPos += parentGlobalPos;
            localRotation += parentGlobalRotation;
            localScale *= parentGlobalScale;

            // actually change parent, updating child refs
            //   of old and new parents
            parent?.children.Remove(this);
            parent = value;
            parent?.children.Add(this);
            RecalculateParentGlobals();

            // re-remove offsets after new parent has been set
            localPos -= parentGlobalPos;
            localRotation -= parentGlobalRotation;
            localScale /= parentGlobalScale;
        }
    }

    /// <summary>
    /// Gets the world transformation matrix for this transform
    /// </summary>
    public Matrix WorldMatrix {
        get {
            if (matricesDirty) RecalculateMatrices();
            return worldMatrix;
        }
    }

    /// <summary>
    /// Gets the world inverse transpose transformation matrix for this transform
    /// </summary>
    public Matrix WorldInverseTranspose {
        get {
            if (matricesDirty) RecalculateMatrices();
            return worldInverseTranspose;
        }
    }

    /// <summary>
    /// Gets/sets the position relative to the parent of this transform
    /// </summary>
    public Vector3 Position {
        get => localPos;
        set {
            localPos = value;
            MarkParentGlobalsDirty();
            MarkMatricesDirty();
        }
    }

    /// <summary>
    /// Gets/sets the global position of this transform
    /// </summary>
    public Vector3 GlobalPosition {
        get {
            if (parentGlobalsDirty) RecalculateParentGlobals();
            return localPos + parentGlobalPos;
        }

        set {
            if (parentGlobalsDirty) RecalculateParentGlobals();
            localPos = value - parentGlobalPos;
            MarkParentGlobalsDirty();
            MarkMatricesDirty();
        }
    }

    /// <summary>
    /// Gets/sets the scale relative to the parent of this transform
    /// </summary>
    public Vector3 Scale {
        get => localScale;
        set {
            localScale = value;
            MarkParentGlobalsDirty();
            MarkMatricesDirty();
        }
    }

    /// <summary>
    /// Gets/sets the global scale of this transform
    /// </summary>
    public Vector3 GlobalScale {
        get {
            if (parentGlobalsDirty) RecalculateParentGlobals();
            return parentGlobalScale * localScale;
        }

        set {
            if (parentGlobalsDirty) RecalculateParentGlobals();
            localScale = value / parentGlobalScale;
            MarkParentGlobalsDirty();
            MarkMatricesDirty();
        }
    }

    /// <summary>
    /// Gets/sets the rotation of this transform relative to the parent in PITCH/YAW/ROLL format
    /// </summary>
    public Vector3 Rotation {
        get => localRotation;
        set {
            localRotation = value;
            MarkParentGlobalsDirty();
            MarkMatricesDirty();
            MarkDirectionalsDirty();
        }
    }

    /// <summary>
    /// Gets/sets the global rotation of this transform in PITCH/YAW/ROLL format
    /// </summary>
    public Vector3 GlobalRotation {
        get {
            if (parentGlobalsDirty) RecalculateParentGlobals();
            return parentGlobalRotation + localRotation;
        }

        set {
            if (parentGlobalsDirty) RecalculateParentGlobals();
            localRotation = value - parentGlobalRotation;
            MarkParentGlobalsDirty();
            MarkMatricesDirty();
            MarkDirectionalsDirty();
        }
    }

    /// <summary>
    /// Gets the forward-facing direction vector
    /// </summary>
    public Vector3 Forward {
        get {
            if (directionalsDirty) RecalculateDirectionals();
            return forward;
        }
    }

    /// <summary>
    /// Gets the up-facing direction vector
    /// </summary>
    public Vector3 Up {
        get {
            if (directionalsDirty) RecalculateDirectionals();
            return up;
        }
    }

    /// <summary>
    /// Gets the right-facing direction vector
    /// </summary>
    public Vector3 Right {
        get {
            if (directionalsDirty) RecalculateDirectionals();
            return right;
        }
    }

    /// <summary>
    /// Creates a new Transform object
    /// </summary>
    /// <param name="position">Position of transform</param>
    /// <param name="scale">Scale of transform</param>
    /// <param name="rotation">Rotation of transform</param>
    /// <param name="parent">Reference to parent transform object</param>
    public Transform3D(Vector3 position, Vector3 scale, Vector3 rotation, Transform3D? parent = null) {
        this.localPos = position;
        this.localScale = scale;
        this.localRotation = rotation;
        this.parent = parent;
        this.children = new List<Transform3D>();
        RecalculateParentGlobals();
        RecalculateMatrices();
        RecalculateDirectionals();
    }

    /// <summary>
    /// Creates a new Transform object
    /// </summary>
    /// <param name="position">Position of transform</param>
    public Transform3D(Vector3 position) : this(position, Vector3.One, Vector3.Zero) { }

    /// <summary>
    /// Creates a new Transform object at (0, 0) with default scale and rotation
    /// </summary>
    public Transform3D() {
        this.localPos = Vector3.Zero;
        this.localScale = Vector3.One;
        this.localRotation = Vector3.Zero;
        this.parentGlobalPos = Vector3.Zero;
        this.parentGlobalScale = Vector3.One;
        this.parentGlobalRotation = Vector3.Zero;
        this.children = new List<Transform3D>();
        this.parent = null;
    }

    /// <summary>
    /// Adds a child to this transform
    /// </summary>
    /// <param name="child">Transform to add</param>
    public void AddChild(Transform3D child) {
        if (child != null) {
            // give me the child.
            children.Add(child);
            child.Parent = this;
        }
    }

    /// <summary>
    /// Removes a child from this transform
    /// </summary>
    /// <param name="child">Tranform to remove</param>
    public void RemoveChild(Transform3D child) {
        if (child != null) {
            children.Remove(child);
            child.Parent = null;
        }
    }

    /// <summary>
    /// Clears all children from this transform
    /// </summary>
    public void ClearChildren() {
        foreach (Transform3D child in children) {
            child.Parent = null;
        }

        children.Clear();
    }

    private void RecalculateParentGlobals() {
        if (parent != null) {
            // this will recursively go up the transform tree
            //   recalculating things if they're ever dirty
            parentGlobalPos = parent.GlobalPosition;
            parentGlobalScale = parent.GlobalScale;
            parentGlobalRotation = parent.GlobalRotation;
        } else {
            parentGlobalPos = Vector3.Zero;
            parentGlobalScale = Vector3.One;
            parentGlobalRotation = Vector3.Zero;
        }

        parentGlobalsDirty = false;
    }

    private void RecalculateMatrices() {
        if (parentGlobalsDirty) RecalculateParentGlobals();

        // calculate all transforms...
        Matrix transMat = Matrix.CreateTranslation(localPos);
        Vector3 pyr = localRotation;
        Matrix rotMat = Matrix.CreateFromYawPitchRoll(pyr.Y, pyr.X, pyr.Z);
        Matrix scaleMat = Matrix.CreateScale(localScale);

        // then combine to internal matrices !
        worldMatrix = scaleMat * rotMat * transMat;
        if (parent != null) worldMatrix *= parent.WorldMatrix;
        worldInverseTranspose = Matrix.Transpose(Matrix.Invert(worldMatrix));

        // after recalculating we're no longer dirty!
        matricesDirty = false;
    }

    private void RecalculateDirectionals() {
        Matrix rotation = Matrix.CreateFromYawPitchRoll(GlobalRotation.Y, GlobalRotation.X, GlobalRotation.Z);

        right = Vector3.Transform(new Vector3(1, 0, 0), rotation);
        up = Vector3.Transform(new Vector3(0, 1, 0), rotation);
        forward = Vector3.Transform(new Vector3(0, 0, 1), rotation);
        directionalsDirty = false;
    }

    private void MarkParentGlobalsDirty() {
        // recursively go DOWN the transform tree and
        //   mark all children dirty
        foreach (Transform3D child in children) {
            child.MarkParentGlobalsDirty();
        }

        // base case is when there are no children, so mark
        //   this transform as dirty then go back up!
        parentGlobalsDirty = true;
    }

    private void MarkMatricesDirty() {
        // recursively go DOWN the transform tree and
        //   mark all children dirty
        foreach (Transform3D child in children) {
            child.MarkMatricesDirty();
        }

        // base case is when there are no children, so mark
        //   this transform as dirty then go back up!
        matricesDirty = true;
    }

    private void MarkDirectionalsDirty() {
        // recursively go DOWN the transform tree and
        //   mark all children dirty
        foreach (Transform3D child in children) {
            child.MarkDirectionalsDirty();
        }

        // base case is when there are no children, so mark
        //   this transform as dirty then go back up!
        directionalsDirty = true;
    }
}
