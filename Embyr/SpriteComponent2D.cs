using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr;

public class SpriteComponent2D : IDrawable2D {
    private readonly IActor2D actor;
    private readonly Texture2D sprite;

    public Color Color { get; set; }

    // anchor for drawing, must be values (0 - 1)
    public Vector2 Anchor { get; set; }

    public SpriteEffects SpriteEffects { get; set; }

    public SpriteComponent2D(IActor2D actor, Texture2D sprite) {
        this.actor = actor;
        this.sprite = sprite;
        Color = Color.White;
        Anchor = new Vector2(0.5f, 0.5f);
        SpriteEffects = SpriteEffects.None;
    }

    public void Draw(SpriteBatch sb) {
        Vector2 spriteSize = new(sprite.Width, sprite.Height);
        spriteSize *= actor.Transform.GlobalScale;

        Vector2 drawPos = actor.Transform.GlobalPosition;

        Rectangle dest = new(
            Vector2.Floor(drawPos).ToPoint(),
            Vector2.Floor(spriteSize).ToPoint()
        );

        sb.Draw(
            sprite,
            dest,
            null,
            Color,
            actor.Transform.GlobalRotation,
            Anchor * spriteSize,
            SpriteEffects,
            0
        );
    }
}
