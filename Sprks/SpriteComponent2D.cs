using Sprks.Rendering;
using Sprks.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sprks;

/// <summary>
/// 2D actor component used for rendering sprites for actors
/// </summary>
public class SpriteComponent2D : ActorComponent2D {
    private Effect? shader;
    private EffectParameter zIndexParam;
    private EffectParameter normalTextureParam;
    private EffectParameter obstructsLightParam;
    private EffectParameter useNormalsParam;

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
    /// Gets/sets whether or not this sprite component obstructs light
    /// </summary>
    public bool ObstructsLight { get; set; }

    /// <summary>
    /// Gets/sets local source rectangle to use when drawing sprite
    /// </summary>
    public Rectangle? SourceRect { get; set; }

    /// <summary>
    /// Gets/sets the texture offset to use when drawing
    /// </summary>
    public Vector2 Offset { get; set; }

    /// <summary>
    /// Gets/sets the shader used when rendering this sprite
    /// </summary>
    public Effect? Shader {
        get => shader;
        set {
            if (shader != value && value != null) {
                zIndexParam = value.Parameters["ZIndex"];
                normalTextureParam = value.Parameters["NormalTexture"];
                obstructsLightParam = value.Parameters["ObstructsLight"];
                useNormalsParam = value.Parameters["UseNormals"];
            }

            shader = value;
        }
    }

    /// <summary>
    /// Creates a new SpriteComponent2D
    /// </summary>
    /// <param name="actor">Actor to attach component to</param>
    internal SpriteComponent2D(Actor2D actor) : base(actor) {
        Color = Color.White;
        Offset = Vector2.Zero;
        Anchor = new Vector2(0.5f, 0.5f);
        SpriteEffects = SpriteEffects.None;
        SourceRect = null;
    }

    /// <inheritdoc/>
    public override void Update(float deltaTime) { }

    /// <inheritdoc/>
    public override void PhysicsUpdate(float deltaTime) { }

    /// <inheritdoc/>
    public override void Draw(SpriteBatch sb) {
        if (Texture == null) return;

        // use either source rect size for drawing or whole
        //   texture size if source rect is null, then scale
        Vector2 spriteSize =
            SourceRect?.Size.ToVector2() ??
            new(Texture.Width, Texture.Height);
        spriteSize *= Actor.Transform.GlobalScale;

        Vector2 pos = Actor.Transform.GlobalPosition + Offset;

        Rectangle dest = new(
            Vector2.Floor(pos).ToPoint(),
            Vector2.Floor(spriteSize).ToPoint()
        );

        Shader ??= ShaderManager.CurrentActorEffect;
        if (Shader != null) {
            bool paramsChanged =
                zIndexParam.GetValueInt32() != Actor.Transform.GlobalZIndex ||
                normalTextureParam.GetValueTexture2D() != Normal ||
                obstructsLightParam.GetValueBoolean() != ObstructsLight ||
                useNormalsParam.GetValueBoolean() != (Normal != null);

            // if the parameters have changed, update the
            //   parameters and restart spritebatch
            if (paramsChanged) {
                //! NOTE: this isn't super modular for future 2D renderers...
                ((RendererDeferred2D)SceneManager.Renderer).RestartSpriteBatch(Actor.Scene as Scene2D);

                zIndexParam.SetValue(Actor.Transform.GlobalZIndex);
                normalTextureParam.SetValue(Normal);
                obstructsLightParam.SetValue(ObstructsLight);
                useNormalsParam.SetValue(Normal != null);
            }
        }

        sb.Draw(
            Texture,
            dest,
            SourceRect,
            Color,
            Actor.Transform.GlobalRotation,
            Vector2.Floor(Anchor * spriteSize),
            SpriteEffects,
            0
        );
    }

    /// <inheritdoc/>
    public override void DebugDraw(SpriteBatch sb) { }
}
