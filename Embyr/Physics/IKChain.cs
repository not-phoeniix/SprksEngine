using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Physics;

/// <summary>
/// Inverse Kinematics chain using the FABRIK solver
/// </summary>
public class IKChain : IDrawable2D, IDebugDrawable {
    //* WOWSA !! this was SO EASY to implement :D
    //*   many many thanks to the awesome video that helped me figure
    //*   out the FABRIK solver !!
    //*
    //*   https://www.youtube.com/watch?v=UNoX65PRehA   :]

    private readonly Vector2[] nodes;
    private readonly float[] lengths;
    private readonly float totalLength;
    private readonly float distThreshold = 0.05f;

    /// <summary>
    /// Gets/sets the target position for the IK solver to attempt to reach
    /// </summary>
    public Vector2 Target { get; set; }

    /// <summary>
    /// Get/sets the original anchor position the chain originates from
    /// </summary>
    public Vector2 Anchor { get; set; }

    /// <summary>
    /// Gets/sets the color to draw the segments in the chain
    /// </summary>
    public Color DrawColor { get; set; } = Color.Black;

    /// <summary>
    /// Gets/sets the thickness of the segments in the chain
    /// </summary>
    public float DrawThickness { get; set; } = 1;

    /// <summary>
    /// Gets the sum of all segment lengths, or rather "total chain length"
    /// </summary>
    public float Length => totalLength;

    /// <summary>
    /// Gets/sets the relative pull bias of the IK solver,
    /// where Vector2.Zero is zero bias whatsoever
    /// </summary>
    public Vector2 RelativePullBias { get; set; } = Vector2.Zero;

    /// <summary>
    /// Gets enumerable of all nodes within chain
    /// </summary>
    public IEnumerable<Vector2> Nodes {
        get {
            foreach (Vector2 node in nodes) {
                yield return node;
            }
        }
    }

    /// <summary>
    /// Creates a new IKChain
    /// </summary>
    /// <param name="segmentLengths">
    /// Array of segment lengths starting from anchor moving to
    /// target, also determines number of segments
    /// </param>
    /// <param name="anchor">Starting anchor of chain</param>
    /// <param name="initialTarget">Starting target of chain</param>
    public IKChain(float[] segmentLengths, Vector2 anchor, Vector2 initialTarget) {
        if (segmentLengths.Length <= 1) {
            throw new System.Exception("ERROR: Cannot create IK with one or less segments!");
        }

        // setting properties
        Anchor = anchor;
        Target = initialTarget;

        // create length/node arrays
        nodes = new Vector2[segmentLengths.Length + 1];
        lengths = new float[segmentLengths.Length];

        float totalLength = 0;
        for (int i = 0; i < lengths.Length; i++) {
            lengths[i] = segmentLengths[i];
            totalLength += segmentLengths[i];
        }

        this.totalLength = totalLength;

        OutOfReachAlign();
    }

    /// <summary>
    /// Creates an IK chain with both the target and anchor as Vector.Zero
    /// </summary>
    /// <param name="segmentLengths">
    /// Array of segment lengths starting from anchor moving to
    /// target, also determines number of segments
    /// </param>
    public IKChain(float[] segmentLengths)
    : this(segmentLengths, Vector2.Zero, Vector2.Zero) { }

    // starts at first node, sets it to the anchor, and pulls all
    //   other nodes along forwards
    private void Forward() {
        for (int i = 0; i < nodes.Length; i++) {
            if (i == 0) {
                nodes[i] = Anchor;
            } else {
                Vector2 prev = nodes[i - 1];
                Vector2 current = nodes[i];

                // calculate direction vec from previous to current nodes
                Vector2 dir = prev != current ?
                    Vector2.Normalize(current - prev) :
                    Vector2.UnitX;

                // make current node constrained to the line between
                //   two nodes, by its stored length
                nodes[i] = nodes[i - 1] + dir * lengths[i - 1];
            }
        }
    }

    // starts at last node, sets it to the target, and pulls all
    //   other nodes along backwards
    private void Backward() {
        for (int i = nodes.Length - 1; i >= 0; i--) {
            if (i == nodes.Length - 1) {
                nodes[i] = Target;
            } else {
                Vector2 prev = nodes[i + 1];
                Vector2 current = nodes[i];

                // calculate direction vec from previous to current nodes
                Vector2 dir = prev != current ?
                    Vector2.Normalize(current - prev) :
                    Vector2.UnitX;

                // make current node constrained to the line between
                //   two nodes, by its stored length
                nodes[i] = nodes[i + 1] + dir * lengths[i];
            }
        }
    }

    private void OutOfReachAlign() {
        // set up direction to target, zero vector if points are the same
        Vector2 dirToTarget = Anchor != Target ?
            Vector2.Normalize(Target - Anchor) :
            Vector2.UnitX;

        // align all nodes iteratively
        for (int i = 0; i < nodes.Length; i++) {
            if (i == 0) {
                nodes[i] = Anchor;
            } else {
                // position equals prev pos plus length between
                //   segments times direction
                nodes[i] = nodes[i - 1] + dirToTarget * lengths[i - 1];
            }
        }
    }

    private void ApplyPullBias() {
        for (int i = 0; i < nodes.Length; i++) {
            nodes[i] += RelativePullBias;
        }
    }

    /// <summary>
    /// Updates IK solver
    /// </summary>
    public void Update() {
        float dSqrToTarg = Vector2.DistanceSquared(Anchor, Target);
        if (dSqrToTarg >= totalLength * totalLength) {
            OutOfReachAlign();
        } else {
            // apply pull bias if bias isn't Vector2.Zero
            if (RelativePullBias != Vector2.Zero) {
                ApplyPullBias();
            }

            // loop upper conditional defines max num iterations
            for (int i = 0; i < 10; i++) {
                // yank back yank forward (FAB part of FABRIK)
                Backward();
                Forward();

                // exit loop early if distance to target is below a certain threshold
                float dSqr = Vector2.DistanceSquared(nodes[nodes.Length - 1], Target);
                if (dSqr <= (distThreshold * distThreshold)) {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Draws this IKChain as a singular solid connection of nodes
    /// (kinda like a rope but not a rope)
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public void Draw(SpriteBatch sb) {
        for (int i = 1; i < nodes.Length; i++) {
            sb.DrawLineCentered(
                nodes[i - 1],
                nodes[i],
                DrawThickness,
                DrawColor
            );
        }
    }

    /// <summary>
    /// Draws debug information for this IKChain, namely chain
    /// connections, node locations, and anchor/target positions
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public void DebugDraw(SpriteBatch sb) {
        for (int i = 0; i < nodes.Length; i++) {
            if (i > 0) {
                sb.DrawLineCentered(nodes[i - 1], nodes[i], 1, Color.White);
            }

            sb.DrawCircleFill(nodes[i], 2, 5, Color.DimGray);
        }

        sb.DrawCircleFill(Anchor, 2, 5, Color.Blue);
        sb.DrawCircleFill(Target, 2, 5, Color.Red);
    }
}
