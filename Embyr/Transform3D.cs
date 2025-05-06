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
    private Quaternion localRotation;

    bool parentGlobalsDirty;
    private Vector3 parentGlobalPos;
    private Vector3 parentGlobalScale;
    private Quaternion parentGlobalRotation;

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
            if (matricesDirty) RecalculateMatrices();
            localPos += parentGlobalPos;
            localRotation *= parentGlobalRotation;
            localScale *= parentGlobalScale;

            // actually change parent, updating child refs
            //   of old and new parents
            parent?.children.Remove(this);
            parent = value;
            parent?.children.Add(this);
            RecalculateMatrices();

            // re-remove offsets after new parent has been set
            localPos -= parentGlobalPos;
            localRotation /= parentGlobalRotation;
            localScale /= parentGlobalScale;
        }
    }

    /// <summary>
    /// Gets/sets the position relative to the parent of this transform
    /// </summary>
    public Vector3 Position {
        get => localPos;
        set {
            localPos = value;
            MarkMatricesDirty();
        }
    }

    /// <summary>
    /// Gets/sets the global position of this transform
    /// </summary>
    public Vector3 GlobalPosition {
        get {
            if (matricesDirty) RecalculateMatrices();
            return localPos + parent?.GlobalPosition ?? Vector3.Zero;
        }

        set {
            if (matricesDirty) RecalculateMatrices();
            localPos = value - parent?.GlobalPosition ?? Vector3.Zero;
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
            MarkMatricesDirty();
        }
    }

    /// <summary>
    /// Gets/sets the global scale of this transform
    /// </summary>
    public Vector3 GlobalScale {
        get {
            if (matricesDirty) RecalculateMatrices();
            return localScale = parentGlobalScale;
        }

        set {
            if (matricesDirty) RecalculateMatrices();
            localScale = value - parentGlobalScale;
            MarkMatricesDirty();
        }
    }

    /// <summary>
    /// Gets/sets the rotation of this transform relative to the parent
    /// </summary>
    public Quaternion Rotation {
        get => localRotation;
        set {
            localRotation = value;
            MarkMatricesDirty();
            MarkDirectionalsDirty();
        }
    }

    /// <summary>
    /// Gets/sets the global rotation of this transform
    /// </summary>
    public Quaternion GlobalRotation {
        get {
            if (matricesDirty) RecalculateMatrices();
            return localRotation * parentGlobalRotation;
        }

        set {
            if (matricesDirty) RecalculateMatrices();
            localRotation = value / parentGlobalRotation;
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
    public Transform3D(Vector3 position, Vector3 scale, Quaternion rotation, Transform3D? parent = null) {
        this.localPos = position;
        this.localScale = scale;
        this.localRotation = rotation;
        this.parent = parent;
        this.children = new List<Transform3D>();
        RecalculateMatrices();
        RecalculateDirectionals();
    }

    /// <summary>
    /// Creates a new Transform object
    /// </summary>
    /// <param name="position">Position of transform</param>
    public Transform3D(Vector3 position) : this(position, Vector3.One, Quaternion.Identity) { }

    /// <summary>
    /// Creates a new Transform object at (0, 0) with default scale and rotation
    /// </summary>
    public Transform3D() {
        this.localPos = Vector3.Zero;
        this.localScale = Vector3.One;
        this.localRotation = Quaternion.Identity;
        this.parentGlobalPos = Vector3.Zero;
        this.parentGlobalScale = Vector3.One;
        this.parentGlobalRotation = Quaternion.Identity;
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
            parentGlobalRotation = Quaternion.Identity;
        }

        parentGlobalsDirty = false;
    }

    private void RecalculateMatrices() {
        if (parentGlobalsDirty) RecalculateParentGlobals();

        // calculate all transforms...
        Matrix transMat = Matrix.CreateTranslation(localPos + parentGlobalPos);
        Matrix rotMat = Matrix.CreateFromQuaternion(localRotation * parentGlobalRotation);
        Matrix scaleMat = Matrix.CreateScale(localScale + parentGlobalScale);

        // then combine to internal matrices !
        worldMatrix = scaleMat * rotMat * transMat;
        worldInverseTranspose = Matrix.Transpose(Matrix.Invert(worldMatrix));

        // after recalculating we're no longer dirty!
        matricesDirty = false;
    }

    private void RecalculateDirectionals() {
        right = Vector3.Transform(new Vector3(1, 0, 0), GlobalRotation);
        up = Vector3.Transform(new Vector3(0, 1, 0), GlobalRotation);
        forward = Vector3.Transform(new Vector3(0, 0, 1), GlobalRotation);
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
