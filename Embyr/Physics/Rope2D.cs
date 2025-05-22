using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Physics;

/// <summary>
/// Contains a collection of rope nodes, a segmented physics rope
/// </summary>
public class Rope2D : IDrawable2D, IDebugDrawable2D {
    // try this !!
    // https://www.owlree.blog/posts/simulating-a-rope.html
    // hopefully it works :3
    //
    // HI UPDATE IT WORKS

    private PhysicsComponent2D[] nodes;
    private float segmentLength;

    private PhysicsComponent2D endEntity;
    private PhysicsComponent2D startEntity;

    /// <summary>
    /// Color to draw rope
    /// </summary>
    public Color DrawColor { get; set; } = Color.White;

    /// <summary>
    /// Thickness of rope
    /// </summary>
    public float DrawThickness { get; set; } = 1f;

    /// <summary>
    /// Gets/sets whether or not to anchor or lock the start of rope to
    /// a position, should be true when start position is set manually
    /// </summary>
    public bool EnableStartAnchor { get; set; } = true;

    /// <summary>
    /// Gets/sets whether or not to anchor or lock the end of rope to
    /// a position, should be true when end position is set manually
    /// </summary>
    public bool EnableEndAnchor { get; set; } = false;

    /// <summary>
    /// Anchor position to stick the start of a rope to
    /// </summary>
    public Vector2 StartPos { get; set; }

    /// <summary>
    /// Anchor position to stick the end of a rope to
    /// </summary>
    public Vector2 EndPos { get; set; }

    /// <summary>
    /// Gets/sets the friction applied whenever an entity is
    //  attached to the end of the rope
    /// </summary>
    public float EntityAttachFriction { get; set; } = 0;

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
    public float GravityScale {
        get { return nodes[0].GravityScale; }
        set {
            foreach (PhysicsComponent2D node in nodes) {
                node.GravityScale = value;
            }
        }
    }

    /// <summary>
    /// Creates a new rope
    /// </summary>
    /// <param name="startPoint">Starting position of the rope</param>
    /// <param name="endPoint">Ending position of the rope</param>
    /// <param name="segments">Number of rope segments</param>
    public Rope2D(Vector2 startPoint, Vector2 endPoint, int segments) {
        if (segments <= 0) {
            throw new Exception("Error creating rope: segments cannot be less than one!");
        }

        StartPos = startPoint;
        EndPos = endPoint;

        // when there's 0 subdivisions, there will be 2 nodes
        int numNodes = segments + 1;

        // initialize arrays
        nodes = new PhysicsComponent2D[numNodes];

        // calculate distances
        float totalLength = Vector2.Distance(startPoint, endPoint) + 10;
        segmentLength = totalLength / segments;

        // offset applied each iteration
        Vector2 diff = (endPoint - startPoint) / segments;

        // creating nodes/components
        for (int i = 0; i < numNodes; i++) {
            Vector2 position = startPoint + (diff * i);

            // TODO: fix rope physics component initialization!

            // nodes[i] = new PhysicsComponent2D(
            //     new Transform2D(position),
            //     new Rectangle(0, 0, 1, 1),
            //     1f,
            //     1000
            // ) {
            //     Solver = PhysicsComponent2D.PhysicsSolver.Verlet
            // };
        }
    }

    /// <summary>
    /// Creates a new rope
    /// </summary>
    /// <param name="nodes">Pre-existing array of nodes</param>
    /// <param name="segmentLength">Length of each rope segment</param>
    public Rope2D(PhysicsComponent2D[] nodes, float segmentLength) {
        this.nodes = nodes;
        this.segmentLength = segmentLength;
        foreach (PhysicsComponent2D node in nodes) {
            node.Solver = PhysicsComponent2D.PhysicsSolver.Verlet;
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
        foreach (PhysicsComponent2D node in nodes) {
            node.ApplyForce(force);
        }
    }

    /// <summary>
    /// Applies a friction force to all segments of the rope
    /// </summary>
    /// <param name="coeff">Coefficient of friction to apply</param>
    public void ApplyFriction(float coeff) {
        foreach (PhysicsComponent2D node in nodes) {
            node.ApplyFriction(coeff);
        }
    }

    /// <summary>
    /// Updates general logic of this rope
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    public void Update(float dt) {
        foreach (PhysicsComponent2D node in nodes) {
            node.EnableGravity = EnableGravity;
            node.UpdateTransform();
        }

        if (EnableStartAnchor) {
            nodes[0].Enabled = false;
            nodes[0].Position = StartPos;
        } else {
            nodes[0].Enabled = true;
            StartPos = nodes[0].Position;
        }

        if (EnableEndAnchor) {
            nodes[nodes.Length - 1].Enabled = false;
            nodes[nodes.Length - 1].Position = EndPos;
        } else {
            nodes[nodes.Length - 1].Enabled = true;
            EndPos = nodes[nodes.Length - 1].Position;
        }
    }

    /// <summary>
    /// Updates rope movement logic/physics
    /// </summary>
    /// <param name="deltaTime">Time passed since last frame</param>
    public void PhysicsUpdate(Scene2D scene, float deltaTime) {
        // update physics
        foreach (PhysicsComponent2D node in nodes) {
            node.Update(scene, deltaTime);
        }

        // rope position correction
        int iterations = nodes.Length;
        for (int i = 0; i < iterations; i++) {
            for (int j = 1; j < nodes.Length; j++) {
                RelaxConstraint(
                    nodes[j - 1],
                    nodes[j],
                    segmentLength
                );
            }
        }

        // draw entity and node together with a force
        if (endEntity != null) {
            PhysicsComponent2D node = nodes[nodes.Length - 1];
            Vector2 toNode = node.NonLerpedPosition - endEntity.NonLerpedPosition;
            Vector2 toEntity = endEntity.NonLerpedPosition - node.NonLerpedPosition;

            float scale = 2000;

            endEntity.ApplyForce(toNode * scale);
            endEntity.ApplyFriction(EntityAttachFriction);
            node.ApplyForce(toEntity * scale * endEntity.Mass / 1.8f);
        }
    }

    /// <summary>
    /// Draws this rope (all internal segments)
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public void Draw(SpriteBatch sb) {
        // draw lines between nodes
        for (int i = 1; i < nodes.Length; i++) {
            sb.DrawLineCentered(
                nodes[i].Position,
                nodes[i - 1].Position,
                DrawThickness,
                DrawColor
            );
        }
    }

    /// <summary>
    /// Draws rope debug info (node location and connections)
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public void DebugDraw(SpriteBatch sb) {
        // draw lines between nodes
        for (int i = 1; i < nodes.Length; i++) {
            sb.DrawLine(
                nodes[i].Position,
                nodes[i - 1].Position,
                1f,
                Color.DarkGray
            );
        }

        // draw nodes themselves
        foreach (PhysicsComponent2D node in nodes) {
            sb.DrawCircleOutline(
                node.Position,
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
