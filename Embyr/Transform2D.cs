using Microsoft.Xna.Framework;

namespace Embyr;

/// <summary>
/// A sealed transform class holding 2D position, scaling, and rotation information
/// </summary>
public sealed class Transform2D {
    private Transform2D? parent;
    private readonly List<Transform2D> children;
    private bool dirty;
    private Vector2 localPos;
    private Vector2 parentGlobalPos;
    private Vector2 localScale;
    private Vector2 parentGlobalScale;
    private float localRotation;
    private float parentGlobalRot;
    private int zIndex;
    private int parentGlobalZIndex;

    /// <summary>
    /// Gets/sets the parent for this transform
    /// </summary>
    public Transform2D? Parent {
        get => parent;
        set {
            // apply offsets whenever changing what the parent is
            if (dirty) Recalculate();
            localPos += parentGlobalPos;
            localRotation += parentGlobalRot;
            localScale *= parentGlobalScale;

            // actually change parent, updating child refs
            //   of old and new parents
            parent?.children.Remove(this);
            parent = value;
            parent?.children.Add(this);
            Recalculate();

            // re-remove offsets after new parent has been set
            localPos -= parentGlobalPos;
            localRotation -= parentGlobalRot;
            localScale /= parentGlobalScale;
        }
    }

    /// <summary>
    /// Gets/sets the position relative to the parent of this transform
    /// </summary>
    public Vector2 Position {
        get => localPos;
        set {
            localPos = value;
            MarkDirty();
        }
    }

    /// <summary>
    /// Gets/sets the global position of this transform
    /// </summary>
    public Vector2 GlobalPosition {
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
    public Vector2 Scale {
        get => localScale;
        set {
            localScale = value;
            MarkDirty();
        }
    }

    /// <summary>
    /// Gets/sets the global scale of this transform
    /// </summary>
    public Vector2 GlobalScale {
        get {
            if (dirty) Recalculate();
            return localScale * parentGlobalScale;
        }

        set {
            if (dirty) Recalculate();
            localScale = value / parentGlobalScale;
            MarkDirty();
        }
    }

    /// <summary>
    /// Gets/sets the rotation of this transform relative to the parent
    /// </summary>
    public float Rotation {
        get => localRotation;
        set {
            localRotation = value;
            MarkDirty();
        }
    }

    /// <summary>
    /// Gets/sets the global rotation of this transform
    /// </summary>
    public float GlobalRotation {
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
    /// Gets/sets the z-index of this transform relative to the parent
    /// </summary>
    public int ZIndex {
        get => zIndex;
        set {
            zIndex = value;
            MarkDirty();
        }
    }

    /// <summary>
    /// Gets/sets the global z-index of this transform
    /// </summary>
    public int GlobalZIndex {
        get {
            if (dirty) Recalculate();
            return zIndex + parentGlobalZIndex;
        }

        set {
            if (dirty) Recalculate();
            zIndex = value - parentGlobalZIndex;
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
    public Transform2D(Vector2 position, Vector2 scale, float rotation, Transform2D? parent = null) {
        this.localPos = position;
        this.localScale = scale;
        this.localRotation = rotation;
        this.parent = parent;
        this.children = new List<Transform2D>();
        Recalculate();
    }

    /// <summary>
    /// Creates a new Transform object
    /// </summary>
    /// <param name="position">Position of transform</param>
    public Transform2D(Vector2 position) : this(position, Vector2.One, 0) { }

    /// <summary>
    /// Creates a new Transform object at (0, 0) with default scale and rotation
    /// </summary>
    public Transform2D() {
        this.localPos = Vector2.Zero;
        this.localScale = Vector2.One;
        this.localRotation = 0;
        this.parentGlobalPos = Vector2.Zero;
        this.parentGlobalScale = Vector2.One;
        this.parentGlobalRot = 0;
        this.children = new List<Transform2D>();
        this.parent = null;
    }

    /// <summary>
    /// Adds a child to this transform
    /// </summary>
    /// <param name="child">Transform to add</param>
    public void AddChild(Transform2D child) {
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
    public void RemoveChild(Transform2D child) {
        if (child != null) {
            children.Remove(child);
            child.Parent = null;
        }
    }

    /// <summary>
    /// Clears all children from this transform
    /// </summary>
    public void ClearChildren() {
        foreach (Transform2D child in children) {
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
            parentGlobalZIndex = parent.GlobalZIndex;
        } else {
            parentGlobalPos = Vector2.Zero;
            parentGlobalScale = Vector2.One;
            parentGlobalRot = 0;
            parentGlobalZIndex = 0;
        }

        // after recalculating we're no longer dirty!
        dirty = false;
    }

    private void MarkDirty() {
        // recursively go DOWN the transform tree and
        //   mark all children dirty
        foreach (Transform2D child in children) {
            child.MarkDirty();
        }

        // base case is when there are no children, so mark
        //   this transform as dirty then go back up!
        dirty = true;
    }
}
