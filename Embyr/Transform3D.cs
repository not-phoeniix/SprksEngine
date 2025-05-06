using Microsoft.Xna.Framework;

namespace Embyr;

/// <summary>
/// A sealed transform class holding 3D position, scaling, and rotation information
/// </summary>
public sealed class Transform3D {
    private readonly List<Transform3D> children;
    private Transform3D? parent;
    private bool dirty;
    private Vector3 localPos;
    private Vector3 parentGlobalPos;
    private Vector3 localScale;
    private Vector3 parentGlobalScale;
    private Quaternion localRotation;
    private Quaternion parentGlobalRot;

    /// <summary>
    /// Gets/sets the parent for this transform
    /// </summary>
    public Transform3D? Parent {
        get => parent;
        set {
            // apply offsets whenever changing what the parent is
            if (dirty) Recalculate();
            localPos += parentGlobalPos;
            localRotation *= parentGlobalRot;
            localScale += parentGlobalScale;

            // actually change parent, updating child refs
            //   of old and new parents
            parent?.children.Remove(this);
            parent = value;
            parent?.children.Add(this);
            Recalculate();

            // re-remove offsets after new parent has been set
            localPos -= parentGlobalPos;
            localRotation /= parentGlobalRot;
            localScale -= parentGlobalScale;
        }
    }

    /// <summary>
    /// Gets/sets the position relative to the parent of this transform
    /// </summary>
    public Vector3 Position {
        get => localPos;
        set {
            localPos = value;
            MarkDirty();
        }
    }

    /// <summary>
    /// Gets/sets the global position of this transform
    /// </summary>
    public Vector3 GlobalPosition {
        get {
            if (dirty) Recalculate();
            return localPos + parentGlobalPos;
        }

        set {
            if (dirty) Recalculate();
            localPos = value - parentGlobalPos;
            MarkDirty();
        }
    }

    /// <summary>
    /// Gets/sets the scale relative to the parent of this transform
    /// </summary>
    public Vector3 Scale {
        get => localScale;
        set {
            localScale = value;
            MarkDirty();
        }
    }

    /// <summary>
    /// Gets/sets the global scale of this transform
    /// </summary>
    public Vector3 GlobalScale {
        get {
            if (dirty) Recalculate();
            return localScale = parentGlobalScale;
        }

        set {
            if (dirty) Recalculate();
            localScale = value - parentGlobalScale;
            MarkDirty();
        }
    }

    /// <summary>
    /// Gets/sets the rotation of this transform relative to the parent
    /// </summary>
    public Quaternion Rotation {
        get => localRotation;
        set {
            localRotation = value;
            MarkDirty();
        }
    }

    /// <summary>
    /// Gets/sets the global rotation of this transform
    /// </summary>
    public Quaternion GlobalRotation {
        get {
            if (dirty) Recalculate();
            return localRotation + parentGlobalRot;
        }

        set {
            if (dirty) Recalculate();
            localRotation = value - parentGlobalRot;
            MarkDirty();
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
        Recalculate();
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
        this.parentGlobalRot = Quaternion.Identity;
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

    private void Recalculate() {
        if (parent != null) {
            // this will recursively go up the transform tree
            //   recalculating things if they're ever dirty
            parentGlobalPos = parent.GlobalPosition;
            parentGlobalScale = parent.GlobalScale;
            parentGlobalRot = parent.GlobalRotation;
        } else {
            parentGlobalPos = Vector3.Zero;
            parentGlobalScale = Vector3.One;
            parentGlobalRot = Quaternion.Identity;
        }

        // after recalculating we're no longer dirty!
        dirty = false;
    }

    private void MarkDirty() {
        // recursively go DOWN the transform tree and
        //   mark all children dirty
        foreach (Transform3D child in children) {
            child.MarkDirty();
        }

        // base case is when there are no children, so mark
        //   this transform as dirty then go back up!
        dirty = true;
    }
}
