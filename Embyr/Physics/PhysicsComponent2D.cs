using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Embyr.Tools;
using Embyr.Scenes;

namespace Embyr.Physics;

/// <summary>
/// Component that deals with physics handling, both with integration and collision
/// </summary>
public class PhysicsComponent2D : IDebugDrawable2D {
    /// <summary>
    /// The types of physics solvers used in position calculation
    /// </summary>
    public enum PhysicsSolver {
        Euler,
        Verlet
    }

    #region // Fields & Properties

    private readonly IActor2D actor;
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
    /// Whether this component is enabled overall
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Whether or not gravity is enabled
    /// </summary>
    public bool EnableGravity { get; set; } = true;

    /// <summary>
    /// Whether or not object should collide with tiles
    /// </summary>
    public bool EnableCollisions { get; set; } = true;

    /// <summary>
    /// Gets/sets whether or not this entity will collide with the borders of null chunks
    /// </summary>
    public bool EnableNullChunkCollision { get; set; } = true;

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
    public float Mass { get; private set; }

    /// <summary>
    /// Maximum speed of physics component before clamping, only applied with the Euler physics solver
    /// </summary>
    public float MaxSpeed { get; private set; }

    /// <summary>
    /// Minimum speed of physics component before snapping to zero
    /// </summary>
    public float MinSpeed { get; private set; }

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
    public float GravityScale { get; set; } = 1f;

    /// <summary>
    /// Scale of friction applied to this object when on
    /// the ground (colliding with a tile vertically)
    /// </summary>
    public float GroundFrictionScale { get; set; } = 20f;

    /// <summary>
    /// Callback executed when the component collides with a tile
    /// </summary>
    public event Action? OnCollide;

    #endregion

    #region // Constructors

    /// <summary>
    /// Creates a new physics component
    /// </summary>
    /// <param name="actor">Actor to attach this component to</param>
    /// <param name="mass">Mass of object</param>
    /// <param name="maxSpeed">Maximum speed of object</param>
    public PhysicsComponent2D(
        IActor2D actor,
        float mass,
        float maxSpeed,
        float minSpeed
    ) {
        this.actor = actor;
        this.prevTransformPos = actor.Transform.GlobalPosition;
        this.position = actor.Transform.GlobalPosition;
        this.prevPos = position;
        this.prevVerletPos = position;
        this.Mass = mass;
        this.MaxSpeed = maxSpeed;
        this.MinSpeed = minSpeed;

        WanderAngle = Random.Shared.NextSingle(0, 2.0f * MathF.PI);
    }

    #endregion

    #region // Methods

    /// <summary>
    /// Updates physics simulation for this component. Should be called every PhysicsUpdate.
    /// </summary>
    /// <param name="scene">Scene that physics component exists in</param>
    /// <param name="deltaTime">Time passed since last PhysicsUpdate</param>
    public void Update(Scene2D scene, float deltaTime) {
        // update prev pos before anything changes first
        prevPos = position;

        // exit after updating prev pos to prevent jittering
        if (!Enabled) return;

        // apply gravity if enabled
        if (EnableGravity && !OnGround) {
            GravityForce = new Vector2(0, scene.Gravity);
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
        if (EnableCollisions) {
            CollisionCorrection(scene);
        } else {
            OnGround = false;
        }
        // if (chunk != null && EnableCollisions) {
        //     CollisionCorrection(chunk);
        // } else {
        //     OnGround = false;
        // }

        // update direction at the end of update cycle
        Direction = Vector2.Zero;
        if (Velocity.LengthSquared() >= float.Epsilon) {
            // direction of motion equals normalized velocity vector
            Direction = Vector2.Normalize(Velocity);
        }

        // updates the value of the transform every update
        actor.Transform.GlobalPosition = position;
    }

    /// <summary>
    /// Updates the attached transform object, must be run every frame
    /// </summary>
    public void UpdateTransform() {
        // TODO: this causes lots of rope instability, fix plz <3

        // if (transform.GlobalPosition != prevTransformPos) {
        //     // if transform position has changed since last
        //     //   update, set physics component's position
        //     position = transform.GlobalPosition;
        //     prevPos = transform.GlobalPosition;
        // } else {
        //     // otherwise, just normally update transform
        //     //   position to current lerped position
        //     transform.GlobalPosition = Position;
        // }

        prevTransformPos = actor.Transform.GlobalPosition;
        actor.Transform.GlobalPosition = Position;
    }

    /// <summary>
    /// Draws debug information about this physics component,
    /// mainly collision boxes for vertical, horizontal, and combined
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public void DebugDraw(SpriteBatch sb) {
        actor.Collider.DebugDraw(sb);
    }

    private void EulerPosUpdate(float deltaTime) {
        velocity += acceleration * deltaTime;
        velocity += impulseAccel;
        velocity = Utils.ClampMagnitude(velocity, MaxSpeed);
        // apply gravity AFTER speed limiting
        velocity += gravityAccel * deltaTime;

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
        if (velocity.LengthSquared() < MinSpeed * MinSpeed) {
            // stop movement if velocity is below minimum threshold
            velocity = Vector2.Zero;
            position = tmpPos;
        }
        prevVerletPos = tmpPos;

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

    #region // Collisions !!

    private void CollisionCorrection(Scene2D scene) {
        OnGround = false;
        bool somethingCollided = false;

        int numCollisionIterations = 4;
        for (int i = 0; i < numCollisionIterations; i++) {
            bool anyCollisionsOccured = false;

            Vector2 max = actor.Collider.Max;
            Vector2 min = actor.Collider.Min;
            float collisionRadius = MathF.Max(max.X - min.X, max.Y - min.Y);

            foreach (IActor2D a in scene.GetActorsInRadius(actor.Transform.GlobalPosition, collisionRadius)) {
                // don't collide with yourself!
                if (a.Collider == this.actor.Collider) continue;

                // resolve collisions, if any collisions still
                //   occur during resolution, mark that things
                //   are colliding this frame !!
                if (this.actor.Collider.Intersects(a.Collider)) {
                    Vector2 displacement = this.actor.Collider.GetDisplacementVector(a.Collider);

                    position += displacement;

                    somethingCollided = true;
                    anyCollisionsOccured = true;

                    // stop velocity in each axis if
                    //   that axis has any displacement
                    velocity.X *= displacement.X != 0 ? 0 : 1;
                    velocity.Y *= displacement.Y != 0 ? 0 : 1;

                    // we are on the ground if we are displacing up
                    if (displacement.Y < 0) {
                        OnGround = true;
                    }
                }

                this.actor.Transform.GlobalPosition = position;
            }

            // no need to do collision checks anymore if
            //   there aren't any collisions left
            if (!anyCollisionsOccured) break;
        }

        // invoke collision callback when a collision occurs for the first frame
        if (somethingCollided && !somethingCollidedPrev) {
            OnCollide?.Invoke();
        }

        // update previous collision state
        somethingCollidedPrev = somethingCollided;

        // only apply ground friction if velocity isn't a really really small number
        float velLen = velocity.Length();
        if (OnGround && velLen > 0.01f) {
            ApplyFriction(velLen * Mass * GroundFrictionScale);
        }
    }

    /*

    private void CollisionCorrection(Chunk chunk) {
        // world-space locations for top left and bottom right for iteration
        Vector2 startWorldPos = new(Bounds.X, Bounds.Y);
        Vector2 endWorldPos = new(Bounds.X + Bounds.Width, Bounds.Y + Bounds.Height);

        // array/tile-space locations for TL/BR iteration (used in loops)
        Point min = chunk.GetArrayCoords(startWorldPos) - new Point(2);
        Point max = chunk.GetArrayCoords(endWorldPos) + new Point(2);

        OnGround = false;
        bool somethingCollided = false;

        int numCollisionIterations = 4;
        for (int i = 0; i < numCollisionIterations; i++) {
            // track whether or not any object is colliding
            // at the end of collision loop
            bool anythingColliding = false;

            // collision loop checking itself
            for (int y = min.Y; y <= max.Y; y++) {
                for (int x = min.X; x <= max.X; x++) {

                    // ~~ grab reference to tile ~~

                    // difference offset in entire chunk indices... -1 when x/y
                    //   negative, +1 when x/y too big, 0 if in bounds
                    Point diff = new(
                        x < 0 ? -1 : x >= Chunk.Width ? 1 : 0,
                        y < 0 ? -1 : y >= Chunk.Height ? 1 : 0
                    );

                    // check if position is in bounds beforehand
                    //   to prevent out of range exception in
                    //   NegativeCollection
                    Chunk adjChunk = null;
                    Point adjPos = chunk.Position + diff;
                    if (chunk.Container.InBounds(adjPos.X, adjPos.Y)) {
                        // get reference to the chunk this iteration's tile is in
                        adjChunk = chunk.Container[adjPos.X, adjPos.Y];
                    }

                    // resolve collisions with the entire hypothetical
                    //   chunk bounds if one is not yet generated
                    if (adjChunk == null) {
                        if (EnableNullChunkCollision) {
                            Rectangle nullChunkBounds = new(
                                adjPos * Chunk.PixelSize,
                                Chunk.PixelSize
                            );

                            if (ResolveCollision(nullChunkBounds)) {
                                anythingColliding = true;
                            }
                        }

                        continue;
                    }

                    // get reference to tile in the chunk outside of this one,
                    //    use modulo to "wrap around" the index, where...
                    //    16 becomes 0,
                    //    17 becomes 1,
                    //    the negative checks make -1 become 15,
                    //    -2 becomes 14,
                    //    etc.
                    int xWrap = x % Chunk.Width;
                    int yWrap = y % Chunk.Height;
                    if (xWrap < 0) xWrap += Chunk.Width;
                    if (yWrap < 0) yWrap += Chunk.Height;

                    Tile tile = adjChunk[xWrap, yWrap, WorldLayer.Main];

                    // ~~ actual collision resolution ~~

                    // skip if tile data is null or tile isn't collidable
                    if (!tile.Data.Collidable) continue;

                    // resolve collisions, if any collisions still
                    //   occur during resolution, mark that things
                    //   are colliding this frame !!
                    if (ResolveCollision(tile.Bounds)) {
                        anythingColliding = true;
                    }
                }
            }

            // update flag if anything collided at all this iteration
            if (anythingColliding) somethingCollided = true;

            // end loop if no collisions occur this frame for efficiency
            if (!anythingColliding) break;
        }

        // invoke collision callback when a collision occurs for the first frame
        if (somethingCollided && !somethingCollidedPrev) {
            OnCollide?.Invoke();
        }

        // update previous collision state
        somethingCollidedPrev = somethingCollided;

        // only apply ground friction if velocity isn't a really really small number
        float velLen = velocity.Length();
        if (OnGround && velLen > 0.01f) {
            ApplyFriction(velLen * Mass * GroundFrictionScale);
        }
    }

    */

    // returns whether or not a collision exists
    // private bool ResolveCollision(Rectangle target) {
    //     bool collisionExists = false;

    //     Rectangle currentVertBounds = new(
    //         VerticalCollisionBox.X + (int)position.X,
    //         VerticalCollisionBox.Y + (int)position.Y,
    //         VerticalCollisionBox.Width,
    //         VerticalCollisionBox.Height
    //     );

    //     Rectangle currentHorizBounds = new(
    //         HorizontalCollisionBox.X + (int)position.X,
    //         HorizontalCollisionBox.Y + (int)position.Y,
    //         HorizontalCollisionBox.Width,
    //         HorizontalCollisionBox.Height
    //     );

    //     // detect by expanding bottom bounds by 1 pixel
    //     //   when object is colliding with ground
    //     bool isBelow = Utils.IsBelow(target, currentVertBounds);
    //     bool verticalCollision = target.Top <= currentVertBounds.Bottom + 1;
    //     if (isBelow && verticalCollision) {
    //         OnGround = true;
    //     }

    //     // vertical collision resolution
    //     if (target.Intersects(currentVertBounds)) {
    //         collisionExists = true;

    //         int displacement;
    //         bool tileBelow = target.Top >= currentVertBounds.Top;

    //         if (tileBelow) {
    //             // for when object/entity is above the object
    //             displacement = currentVertBounds.Bottom - target.Top;
    //         } else {
    //             // for when object/entity is below the object
    //             displacement = currentVertBounds.Top - target.Bottom;
    //         }

    //         position.Y -= displacement;
    //         velocity.Y = 0;
    //         toChangeVelocity = true;

    //         position.Y = (int)position.Y;
    //     }

    //     // horizontal collision resolution
    //     if (target.Intersects(currentHorizBounds)) {
    //         collisionExists = true;

    //         int displacement;
    //         bool tileToRight = target.Left >= currentHorizBounds.Left;

    //         if (tileToRight) {
    //             // for when object/entity is to the left of the object
    //             displacement = currentHorizBounds.Right - target.Left;
    //         } else {
    //             // for when object/entity is to the right of the object
    //             displacement = currentHorizBounds.Left - target.Right;
    //         }

    //         position.X -= displacement;
    //         velocity.X = 0;
    //         toChangeVelocity = true;

    //         position.X = (int)position.X;
    //     }

    //     return collisionExists;
    // }

    #endregion

    #endregion
}
