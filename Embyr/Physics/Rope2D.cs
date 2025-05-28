using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Physics;

/// <summary>
/// Contains a collection of rope nodes, a segmented physics rope
/// </summary>
public class Rope2D : Actor2D {
    // try this !!
    // https://www.owlree.blog/posts/simulating-a-rope.html
    // hopefully it works :3
    //
    // HI UPDATE IT WORKS

    private class RopeNode : Actor2D {
        public PhysicsComponent2D Physics { get; }

        public RopeNode(Actor2D rope, Vector2 pos, Scene2D scene) : base("node", pos, scene) {
            Physics = AddComponent<PhysicsComponent2D>();
            Physics.MaxSpeed = 1000;
            Physics.Solver = PhysicsComponent2D.PhysicsSolver.Verlet;
            Transform.Parent = rope.Transform;
        }
    }

    private readonly RopeNode[] nodes;
    private float segmentLength;

    private PhysicsComponent2D? endEntity;

    /// <summary>
    /// Color to draw rope
    /// </summary>
    public Color DrawColor { get; set; }

    /// <summary>
    /// Thickness of rope
    /// </summary>
    public float DrawThickness { get; set; }

    /// <summary>
    /// Gets/sets whether or not to anchor or lock the start of rope to
    /// a position, should be true when start position is set manually
    /// </summary>
    public bool EnableStartAnchor { get; set; }

    /// <summary>
    /// Gets/sets whether or not to anchor or lock the end of rope to
    /// a position, should be true when end position is set manually
    /// </summary>
    public bool EnableEndAnchor { get; set; }

    /// <summary>
    /// Anchor position to stick the start of a rope to
    /// </summary>
    public Vector2 StartPos {
        get => Transform.GlobalPosition;
        set => Transform.GlobalPosition = value;
    }

    /// <summary>
    /// Anchor position to stick the end of a rope to
    /// </summary>
    public Vector2 EndPos { get; set; }

    /// <summary>
    /// Gets/sets the friction applied whenever an entity is
    //  attached to the end of the rope
    /// </summary>
    public float EntityAttachFriction { get; set; }

    /// <summary>
    /// Gets/sets the value of the "desired length" of this rope
    /// </summary>
    public float Length {
        get { return segmentLength * (nodes.Length - 2); }
        set {
            segmentLength = value / (nodes.Length - 2);
            if (segmentLength <= 0) {
                segmentLength = 0.01f;
            }
        }
    }

    /// <summary>
    /// Get/sets whether or not to enable gravity for this rope
    /// </summary>
    public bool EnableGravity { get; set; }

    /// <summary>
    /// Gets/sets the gravity scale for the rope
    /// </summary>
    public float GravityScale { get; set; }

    /// <summary>
    /// Gets/sets the mass of the rope
    /// </summary>
    public float Mass { get; set; }

    /// <summary>
    /// Gets/sets the number of iterations performed to perform rope physics constraint corrections
    /// </summary>
    public int CorrectionIterations { get; set; }

    /// <summary>
    /// Creates a new rope
    /// </summary>
    /// <param name="startPoint">Starting position of the rope</param>
    /// <param name="endPoint">Ending position of the rope</param>
    /// <param name="segments">Number of rope segments</param>
    public Rope2D(Vector2 startPoint, Vector2 endPoint, int segments, string name, Scene2D scene)
    : base(name, startPoint, scene) {
        if (segments <= 0) {
            throw new Exception("Error creating rope: segments cannot be less than one!");
        }

        StartPos = startPoint;
        EndPos = endPoint;
        EnableGravity = true;
        EnableEndAnchor = false;
        EnableStartAnchor = true;
        DrawColor = Color.White;
        DrawThickness = 1;
        GravityScale = 1;
        Mass = 0.5f;

        // when there's 0 subdivisions, there will be 2 nodes
        int numNodes = segments + 1;
        CorrectionIterations = numNodes * 2;

        // initialize arrays
        nodes = new RopeNode[numNodes];

        // calculate distances
        float totalLength = Vector2.Distance(startPoint, endPoint) + 10;
        segmentLength = totalLength / segments;

        // offset applied each iteration
        Vector2 diff = (endPoint - startPoint) / segments;

        // creating nodes/components
        for (int i = 0; i < numNodes; i++) {
            Vector2 position = startPoint + (diff * i);
            nodes[i] = new RopeNode(this, position, scene);
        }
    }

    /// <summary>
    /// Attaches an entity to the end of this rope
    /// </summary>
    /// <param name="entity">Entity to attach</param>
    public void AttachEnd(PhysicsComponent2D entity) {
        endEntity = entity;
    }

    /// <summary>
    /// Removes the item currently attached to the end of this rope
    /// </summary>
    public void ResetEndAttachment() {
        endEntity = null;
    }

    /// <summary>
    /// Applies a force to all segments of the rope
    /// </summary>
    /// <param name="force">Force vector to apply</param>
    public void ApplyForce(Vector2 force) {
        foreach (RopeNode node in nodes) {
            node.Physics.ApplyForce(force);
        }
    }

    /// <summary>
    /// Applies a friction force to all segments of the rope
    /// </summary>
    /// <param name="coeff">Coefficient of friction to apply</param>
    public void ApplyFriction(float coeff) {
        foreach (RopeNode node in nodes) {
            node.Physics.ApplyFriction(coeff);
        }
    }

    /// <inheritdoc/>
    public override void Update(float dt) {
        foreach (RopeNode node in nodes) {
            node.Update(dt);
        }

        if (EnableStartAnchor) {
            nodes[0].Physics.Enabled = false;
            nodes[0].Physics.Position = StartPos;
        } else {
            nodes[0].Physics.Enabled = true;
            StartPos = nodes[0].Physics.Position;
        }

        if (EnableEndAnchor) {
            nodes[nodes.Length - 1].Physics.Enabled = false;
            nodes[nodes.Length - 1].Physics.Position = EndPos;
        } else {
            nodes[nodes.Length - 1].Physics.Enabled = true;
            EndPos = nodes[nodes.Length - 1].Physics.Position;
        }
    }

    /// <inheritdoc/>
    public override void PhysicsUpdate(float deltaTime) {
        // update physics
        foreach (RopeNode node in nodes) {
            node.Physics.EnableGravity = EnableGravity;
            node.Physics.GravityScale = GravityScale;
            node.Physics.Mass = Mass;
            node.PhysicsUpdate(deltaTime);
        }

        // rope position correction
        for (int i = 0; i < CorrectionIterations; i++) {
            for (int j = 1; j < nodes.Length; j++) {
                RelaxConstraint(
                    nodes[j - 1].Physics,
                    nodes[j].Physics,
                    segmentLength
                );
            }
        }

        // draw entity and node together with a force
        if (endEntity != null) {
            PhysicsComponent2D node = nodes[nodes.Length - 1].Physics;
            Vector2 toNode = node.NonLerpedPosition - endEntity.NonLerpedPosition;
            Vector2 toEntity = endEntity.NonLerpedPosition - node.NonLerpedPosition;

            float scale = 2000;

            endEntity.ApplyForce(toNode * scale);
            endEntity.ApplyFriction(EntityAttachFriction);
            node.ApplyForce(toEntity * scale * endEntity.Mass / 1.8f);
        }
    }

    /// <inheritdoc/>
    public override void Draw(SpriteBatch sb) {
        // draw lines between nodes
        for (int i = 1; i < nodes.Length; i++) {
            sb.DrawLineCentered(
                nodes[i].Transform.GlobalPosition,
                nodes[i - 1].Transform.GlobalPosition,
                DrawThickness,
                DrawColor
            );
        }
    }

    /// <inheritdoc/>
    public override void DebugDraw(SpriteBatch sb) {
        // draw lines between nodes
        for (int i = 1; i < nodes.Length; i++) {
            sb.DrawLine(
                nodes[i].Transform.GlobalPosition,
                nodes[i - 1].Transform.GlobalPosition,
                1f,
                Color.DarkGray
            );
        }

        // draw nodes themselves
        foreach (RopeNode node in nodes) {
            sb.DrawCircleOutline(
                node.Transform.GlobalPosition,
                2f,
                10,
                1f,
                Color.White
            );
        }
    }

    private static void RelaxConstraint(
        PhysicsComponent2D phys1,
        PhysicsComponent2D phys2,
        float desiredDist
    ) {
        // direction vector
        Vector2 dir = phys2.NonLerpedPosition - phys1.NonLerpedPosition;
        if (dir != Vector2.Zero) dir.Normalize();

        // change in distance between current dist and desired dist
        float deltaDist = Vector2.Distance(phys1.NonLerpedPosition, phys2.NonLerpedPosition) - desiredDist;

        // apply half of each dist to each component
        if (phys1.Enabled) phys1.NonLerpedPosition += dir * deltaDist / 2;
        if (phys2.Enabled) phys2.NonLerpedPosition -= dir * deltaDist / 2;
    }
}
