using Embyr.Rendering;
using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr;

/// <summary>
/// 2D actor component used for rendering sprites for actors
/// </summary>
public class SpriteComponent2D : ActorComponent2D {
    /// <summary>
    /// Gets/sets the drawing color tint
    /// </summary>
    public Color Color { get; set; }

    /// <summary>
    /// Gets/sets the anchor for drawing, values are normalized between 0-1
    /// </summary>
    public Vector2 Anchor { get; set; }

    /// <summary>
    /// Gets/sets sprite effects to apply to this sprite
    /// </summary>
    public SpriteEffects SpriteEffects { get; set; }

    /// <summary>
    /// Gets/sets the texture for this sprite component
    /// </summary>
    public Texture2D? Texture { get; set; }

    /// <summary>
    /// Gets/sets the normal map for this sprite component
    /// </summary>
    public Texture2D? Normal { get; set; }

    /// <summary>
    /// Creates a new SpriteComponent2D
    /// </summary>
    /// <param name="actor">Actor to attach component to</param>
    internal SpriteComponent2D(Actor2D actor) : base(actor) {
        Color = Color.White;
        Anchor = new Vector2(0.5f, 0.5f);
        SpriteEffects = SpriteEffects.None;
    }

    /// <inheritdoc/>
    public override void Update(float deltaTime) { }

    /// <inheritdoc/>
    public override void PhysicsUpdate(float deltaTime) { }

    /// <inheritdoc/>
    public override void Draw(SpriteBatch sb) {
        if (Texture == null) return;

        Vector2 spriteSize = new(Texture.Width, Texture.Height);
        spriteSize *= Actor.Transform.GlobalScale;

        Vector2 drawPos = Actor.Transform.GlobalPosition;

        Rectangle dest = new(
            Vector2.Floor(drawPos).ToPoint(),
            Vector2.Floor(spriteSize).ToPoint()
        );

        Effect? effect = ShaderManager.I.CurrentActorEffect;
        if (effect != null) {
            effect.Parameters["ZIndex"]?.SetValue(Math.Clamp(Actor.Transform.GlobalZIndex, 0, 1000));
            effect.Parameters["NormalTexture"]?.SetValue(Normal);
        }

        sb.Draw(
            Texture,
            dest,
            null,
            Color,
            Actor.Transform.GlobalRotation,
            Anchor * spriteSize,
            SpriteEffects,
            0
        );
    }

    /// <inheritdoc/>
    public override void DebugDraw(SpriteBatch sb) { }
}
