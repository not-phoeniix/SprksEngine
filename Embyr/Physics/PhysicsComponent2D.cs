using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Embyr.Tools;
using Embyr.Scenes;
using System.Diagnostics;

namespace Embyr.Physics;

/// <summary>
/// Component that deals with physics handling, both with integration and collision
/// </summary>
public class PhysicsComponent2D : ActorComponent2D {
    /// <summary>
    /// The types of physics solvers used in position calculation
    /// </summary>
    public enum PhysicsSolver {
        Euler,
        Verlet
    }

    private readonly Collider2D? collider;
    private Vector2 prevTransformPos;
    private Vector2 prevPos;
    private Vector2 prevVerletPos;
    private Vector2 position;
    private Vector2 velocity;
    private Vector2 acceleration;
    private Vector2 gravityAccel;
    private Vector2 impulseAccel;
    private bool toChangeVelocity;
    private bool somethingCollidedPrev;

    /// <summary>
    /// Angle used for wandering behavior in <c>IAgent</c>, internal and restricted to <c>Embyr</c> namespace
    /// </summary>
    internal float WanderAngle;

    /// <summary>
    /// Whether or not gravity is enabled
    /// </summary>
    public bool EnableGravity { get; set; } = true;

    /// <summary>
    /// Whether or not object should collide with tiles
    /// </summary>
    public bool EnableCollisions { get; set; } = true;

    /// <summary>
    /// The type of solver used for this physics component, is Euler by default
    /// </summary>
    public PhysicsSolver Solver { get; set; } = PhysicsSolver.Euler;

    /// <summary>
    /// Boolean value whether or not object is colliding with the ground
    /// </summary>
    public bool OnGround { get; private set; }

    /// <summary>
    /// Mass of current physics object
    /// </summary>
    public float Mass { get; set; }

    /// <summary>
    /// Maximum speed of physics component before clamping, only applied with the Euler physics solver
    /// </summary>
    public float MaxSpeed { get; set; }

    /// <summary>
    /// Minimum speed of physics component before snapping to zero
    /// </summary>
    public float MinSpeed { get; set; }

    /// <summary>
    /// Current position aligned to top left corner of sprite
    /// </summary>
    public Vector2 Position {
        get { return Vector2.Lerp(prevPos, position, Performance.PhysicsLerpValue); }
        set {
            position = value;
            prevPos = value;
        }
    }

    /// <summary>
    /// Gets/sets the non-lerped position ignoring frame interpolation,
    /// useful for accessing current position during physics updates externally
    /// </summary>
    internal Vector2 NonLerpedPosition {
        get => position;
        set => position = value;
    }

    /// <summary>
    /// Gets the normalized direction vector of motion, or Vector2.Zero if stationary
    /// </summary>
    public Vector2 Direction { get; private set; }

    /// <summary>
    /// Current velocity vector of this object
    /// </summary>
    public Vector2 Velocity {
        get { return velocity; }
        set {
            velocity = value;
            toChangeVelocity = true;
        }
    }

    /// <summary>
    /// Magnitude of the velocity vector
    /// </summary>
    public float Speed {
        get { return velocity.Length(); }
    }

    /// <summary>
    /// Force of gravity (vector force) for this current component, applied every frame
    /// </summary>
    public Vector2 GravityForce { get; private set; }

    /// <summary>
    /// Scale of gravity applied to this object
    /// </summary>
    public float GravityScale { get; set; }

    /// <summary>
    /// Scale of friction applied to this object when on
    /// the ground (colliding with a tile vertically)
    /// </summary>
    public float GroundFrictionScale { get; set; }

    /// <summary>
    /// Callback executed when the component collides with a tile
    /// </summary>
    public event Action? OnCollide;

    /// <summary>
    /// Creates a new physics component
    /// </summary>
    /// <param name="actor">Actor to attach this component to</param>
    /// <param name="mass">Mass of object</param>
    /// <param name="maxSpeed">Maximum speed of object</param>
    internal PhysicsComponent2D(Actor2D actor) : base(actor) {
        this.prevTransformPos = actor.Transform.GlobalPosition;
        this.position = actor.Transform.GlobalPosition;
        this.prevPos = position;
        this.prevVerletPos = position;
        this.Mass = 1;
        this.MaxSpeed = 10_000;
        this.MinSpeed = 0.01f;
        this.GroundFrictionScale = 20;
        this.GravityScale = 1;
        this.collider = actor.GetComponent<Collider2D>();

        WanderAngle = Random.Shared.NextSingle(0, 2.0f * MathF.PI);
    }

    /// <summary>
    /// Updates physics simulation for this component. Should be called every PhysicsUpdate.
    /// </summary>
    /// <param name="scene">Scene that physics component exists in</param>
    /// <param name="deltaTime">Time passed since last PhysicsUpdate</param>
    public override void PhysicsUpdate(float deltaTime) {
        // update prev pos before anything changes first
        prevPos = position;

        // exit after updating prev pos to prevent jittering
        if (!Enabled) return;

        if (EnableCollisions && collider == null) {
            Debug.WriteLine("Warning: PhysicsComponent2D created without finding a Collider2D! Collisions will be permanently disabled for this actor! If this is intended then please set PhysicsComponent2D.EnableCollisions to FALSE :]");
            EnableCollisions = false;
        }

        // apply gravity if enabled
        if (EnableGravity) {
            GravityForce = new Vector2(0, Actor.Scene.Gravity);
            ApplyGravity(GravityForce * GravityScale);
        }

        // do physics solving algorithm
        switch (Solver) {
            case PhysicsSolver.Euler:
                EulerPosUpdate(deltaTime);
                break;
            case PhysicsSolver.Verlet:
                VerletPosUpdate(deltaTime);
                break;
        }

        // correct collisions
        if (EnableCollisions && Actor.Scene is Scene2D scene) {
            CollisionCorrection(scene);
        } else {
            OnGround = false;
        }

        // update direction at the end of update cycle
        Direction = Vector2.Zero;
        if (Velocity.LengthSquared() >= float.Epsilon) {
            // direction of motion equals normalized velocity vector
            Direction = Vector2.Normalize(Velocity);
        }
    }

    /// <inheritdoc/>
    public override void Update(float dt) {
        // TODO: this causes lots of rope instability, fix plz <3

        if (Actor.Transform.GlobalPosition != prevTransformPos) {
            // if transform position has changed since last
            //   update, set physics component's position
            position = Actor.Transform.GlobalPosition;
            prevPos = Actor.Transform.GlobalPosition;
        } else {
            // otherwise, just normally update transform
            //   position to current lerped position
            Actor.Transform.GlobalPosition = Position;
        }

        prevTransformPos = Actor.Transform.GlobalPosition;
        // actor.Transform.GlobalPosition = Position;
    }

    /// <inheritdoc/>
    public override void Draw(SpriteBatch sb) { }

    /// <inheritdoc/>
    public override void DebugDraw(SpriteBatch sb) { }

    private void EulerPosUpdate(float deltaTime) {
        velocity += acceleration * deltaTime;
        velocity += impulseAccel;
        velocity += gravityAccel * deltaTime;
        velocity = Utils.ClampMagnitude(velocity, MaxSpeed);

        // snap velocity to zero if below min threshold
        if (velocity.LengthSquared() < MinSpeed * MinSpeed) {
            velocity = Vector2.Zero;
        }

        position += velocity * deltaTime;

        // reset fields
        acceleration = Vector2.Zero;
        impulseAccel = Vector2.Zero;
        gravityAccel = Vector2.Zero;
        toChangeVelocity = false;
    }

    private void VerletPosUpdate(float deltaTime) {
        // if velocity is supposed to change,,, do a new inverse
        //   calculation to interpolate the value of prevPos
        if (toChangeVelocity) {
            // velocity = (position - prevPos) / (2 * deltaTime);
            // (2 * deltaTime) * velocity = position - prevPos
            // (2 * deltaTime) * velocity - position = -prevPos
            // position - (2 * deltaTime) * velocity = prevPos
            prevVerletPos = position - 2 * deltaTime * velocity;
        }

        // do verlet integration itself, apply impluse in addition to acceleration
        Vector2 tmpPos = position;
        position = 2 * position - prevVerletPos + ((acceleration + gravityAccel) * deltaTime + impulseAccel) * deltaTime;
        velocity = (position - prevVerletPos) / (2 * deltaTime);
        prevVerletPos = tmpPos;
        if (velocity.LengthSquared() < MinSpeed * MinSpeed) {
            // stop movement if velocity is below minimum threshold
            velocity = Vector2.Zero;
            prevVerletPos = position;
        }

        // reset fields
        acceleration = Vector2.Zero;
        impulseAccel = Vector2.Zero;
        gravityAccel = Vector2.Zero;
        toChangeVelocity = false;
    }

    /// <summary>
    /// Applies a force to the acceleration, takes mass into account
    /// </summary>
    /// <param name="force">Vector2 force to apply</param>
    public void ApplyForce(Vector2 force) {
        acceleration += force / Mass;
    }

    /// <summary>
    /// Applies an instantaneous impulse force to this physics object (ignores deltaTime)
    /// </summary>
    /// <param name="impulse">Vector2 impulse force to apply</param>
    public void ApplyImpulse(Vector2 impulse) {
        impulseAccel += impulse / Mass;
    }

    /// <summary>
    /// Applies a gravity force to the acceleration, does NOT take mass into account
    /// </summary>
    /// <param name="gravity">Vector2 gravity force to apply</param>
    public void ApplyGravity(Vector2 gravity) {
        gravityAccel += gravity;
    }

    /// <summary>
    /// Applies a friction force (opposite to velocity)
    /// </summary>
    /// <param name="coeff">Friction coefficient to scale velocity by</param>
    public void ApplyFriction(float coeff) {
        if (velocity != Vector2.Zero) {
            Vector2 friction = Vector2.Normalize(velocity) * coeff * -1;
            ApplyForce(friction);
        }
    }

    private void CollisionCorrection(Scene2D scene) {
        // this solution was used for simple AABB:
        //   https://c.har.li/e/2024/03/28/implementing-robust-2D-collision-resolution.html#fn:1

        bool somethingCollided = false;

        OnGround = false;

        if (collider == null) return;

        const int NumCollisionIterations = 3;
        for (int i = 0; i < NumCollisionIterations; i++) {
            bool anyCollisionsOccured = false;

            Vector2 max = collider.Max;
            Vector2 min = collider.Min;
            float collisionRadius = MathF.Max(max.X - min.X, max.Y - min.Y) + 1;

            float largestArea = 0;
            Vector2 displacement = Vector2.Zero;

            // find largest collision area
            foreach (Actor2D a in scene.GetActorsInRadius(Actor.Transform.GlobalPosition, collisionRadius)) {
                Collider2D? otherCollider = a.GetComponent<Collider2D>();

                // don't collide with yourself!
                if (otherCollider == this.collider) continue;

                // get most specific collider at first, will
                //   be null if it does not collide so we skip
                otherCollider = otherCollider?.GetMostSpecificCollidingChild(collider);
                if (otherCollider == null) continue;

                float area = otherCollider.GetOverlappingArea(collider);

                if (area > largestArea) {
                    largestArea = area;
                    displacement = collider.GetDisplacementVector(otherCollider);
                    anyCollisionsOccured = true;
                    somethingCollided = true;
                }
            }

            position += displacement;
            this.Actor.Transform.GlobalPosition = position;

            // we are on the ground if we are displacing up
            if (displacement.Y < 0) {
                OnGround = true;
            }

            // stop velocity in each axis if
            //   that axis has any displacement
            velocity.X *= displacement.X != 0 ? 0 : 1;
            if (displacement.X != 0) {
                velocity.X = 0;
                toChangeVelocity = true;
            }
            if (displacement.Y != 0) {
                velocity.Y = 0;
                toChangeVelocity = true;
            }

            // invoke collision callback when a collision occurs for the first frame
            if (somethingCollided && !somethingCollidedPrev) {
                OnCollide?.Invoke();
                somethingCollidedPrev = true;
            }

            // no need to do collision checks anymore if
            //   there aren't any collisions left
            if (!anyCollisionsOccured) break;
        }

        // update previous collision state
        somethingCollidedPrev = somethingCollided;

        // only apply ground friction if velocity isn't a really really small number
        float velLen = velocity.Length();
        if (OnGround && velLen > 0.1f) {
            ApplyFriction(velLen * Mass * GroundFrictionScale);
        }
    }
}
