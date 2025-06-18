using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr;

/// <summary>
/// A parallax layer that auto scrolls with the scene's camera, inherits from <c>Actor2D</c>
/// </summary>
public class ParallaxLayer2D : Actor2D {
    private readonly SpriteComponent2D sprite;

    /// <summary>
    /// Gets/sets the size of repeat in the X/Y axes, doesn't repeat when set to 0
    /// </summary>
    public Point RepeatSize { get; set; }

    /// <summary>
    /// Gets/sets the scroll scale of this parallax in the X/Y axes, should be between 0-1
    /// </summary>
    public Vector2 ScrollScale { get; set; }

    /// <summary>
    /// Gets/sets anchor of texture relative to this actor's transform position
    /// </summary>
    public Vector2 TextureAnchor {
        get => sprite.Anchor;
        set => sprite.Anchor = value;
    }

    /// <summary>
    /// Creates a new ParallaxLayer2D
    /// </summary>
    /// <param name="texture">Texture of parallax layer</param>
    /// <param name="scrollScale">Scroll scale relative to camera in X/Y axes</param>
    /// <param name="scene"></param>
    public ParallaxLayer2D(Texture2D texture, Vector2 scrollScale, Scene2D scene)
    : base(Vector2.Zero, scene) {
        this.sprite = new SpriteComponent2D(this) {
            Texture = texture
        };
        this.ScrollScale = scrollScale;
        this.RepeatSize = Point.Zero;
    }

    /// <inheritdoc/>
    public override void Draw(SpriteBatch sb) {
        Vector2 offset = ((Scene2D)Scene).Camera.Position * ScrollScale;

        sprite.Draw(Transform.GlobalPosition + offset, sb);

        base.Draw(sb);
    }
}
