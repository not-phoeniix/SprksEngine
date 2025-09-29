using Sprks.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sprks;

/// <summary>
/// A parallax layer that auto scrolls with the scene's camera, inherits from <c>Actor2D</c>
/// </summary>
public class ParallaxLayer2D : Actor2D {
    private readonly Texture2D texture;
    private readonly Vector2 textureSize;
    private SpriteComponent2D[,] sprites;
    private Point repeatSize;

    /// <summary>
    /// Gets/sets the size of repeat in the X/Y axes, doesn't repeat when set to 0
    /// </summary>
    public Point RepeatSize {
        get => repeatSize;
        set {
            value.X = Math.Max(value.X, 0);
            value.Y = Math.Max(value.Y, 0);

            if (repeatSize != value) {
                SetUpSprites(value);
            }

            repeatSize = value;
        }
    }

    /// <summary>
    /// Gets/sets the scroll scale of this parallax in the X/Y axes, should be between
    /// 0-1 where 0 represents no movement at all and 1 represents full speed, moving
    /// along with all other scene actors.
    /// </summary>
    public Vector2 ScrollScale { get; set; }

    /// <summary>
    /// Gets/sets anchor of texture relative to this actor's transform position,
    /// set to middle anchor (0.5, 0.5) by default.
    /// </summary>
    public Vector2 TextureAnchor { get; set; }

    /// <summary>
    /// Creates a new ParallaxLayer2D
    /// </summary>
    /// <param name="texture">Texture of parallax layer</param>
    /// <param name="scrollScale">Scroll scale relative to camera in X/Y axes</param>
    /// <param name="scene">Scene to place parallax layer into</param>
    public ParallaxLayer2D(Texture2D texture, Vector2 scrollScale, Scene2D scene)
    : base(Vector2.Zero, scene) {
        this.texture = texture;
        this.textureSize = new Vector2(texture.Width, texture.Height);
        this.ScrollScale = scrollScale;
        this.PreventCulling = true;
        this.TextureAnchor = new Vector2(0.5f);

        this.repeatSize = Point.Zero;
        this.sprites = new SpriteComponent2D[1, 1];

        SpriteComponent2D sprite = AddComponent<SpriteComponent2D>();
        sprite.Texture = texture;
        this.sprites[0, 0] = sprite;
    }

    /// <inheritdoc/>
    public override void Update(float deltaTime) {
        Vector2 camOffset = ((Scene2D)Scene).Camera.Position * (Vector2.One - ScrollScale);

        for (int x = 0; x < sprites.GetLength(0); x++) {
            for (int y = 0; y < sprites.GetLength(1); y++) {
                Vector2 halfOffset = new(
                    x - (sprites.GetLength(0) / 2),
                    y - (sprites.GetLength(1) / 2)
                );

                sprites[x, y].Offset = camOffset + (halfOffset * textureSize);
                sprites[x, y].Anchor = TextureAnchor;
            }
        }

        base.Update(deltaTime);
    }

    private void SetUpSprites(Point numRepeats) {
        // remove old sprites if they exist
        if (sprites != null) {
            foreach (SpriteComponent2D s in sprites) {
                RemoveComponent(s);
            }
        }

        // recreate sprites array and fill it up with new sprites!!
        sprites = new SpriteComponent2D[numRepeats.X + 1, numRepeats.Y + 1];
        for (int x = 0; x < numRepeats.X + 1; x++) {
            for (int y = 0; y < numRepeats.Y + 1; y++) {
                SpriteComponent2D sprite = AddComponent<SpriteComponent2D>();
                sprite.Texture = this.texture;
                sprites[x, y] = sprite;
            }
        }
    }
}
