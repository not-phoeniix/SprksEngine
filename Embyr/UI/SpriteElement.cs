using Embyr.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite.Sprites;

namespace Embyr.UI;

/// <summary>
/// MenuElement that displays a sprite to the screen
/// </summary>
public class SpriteElement : MenuElement {
    private readonly Sprite sprite;

    /// <summary>
    /// Gets the width of the sprite in pixels
    /// </summary>
    public int SpriteWidth => sprite.Width;

    /// <summary>
    /// Gets the height of the sprite in pixels
    /// </summary>
    public int SpriteHeight => sprite.Height;

    /// <summary>
    /// Creates a new SpriteElement
    /// </summary>
    /// <param name="sprite">Sprite of this element to display</param>
    /// <param name="style">Style of this element</param>
    public SpriteElement(Sprite sprite, ElementStyle style)
    : base(new Rectangle(0, 0, sprite.Width, sprite.Height), style) {
        this.sprite = sprite;
        Position = Vector2.Zero;
    }

    /// <summary>
    /// Updates this SpriteElement
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    public override void Update(float dt) { }

    /// <summary>
    /// Draws this SpriteElement to the screen
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public override void Draw(SpriteBatch sb) {
        Rectangle borderBounds = Utils.ExpandRect(MarginlessBounds, Style.BorderSize);
        sb.DrawRectFill(borderBounds, Style.BorderColor);
        sprite.Draw(sb, MarginlessBounds.Location.ToVector2());
    }
}
