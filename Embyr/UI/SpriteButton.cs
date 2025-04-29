using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Content.Processors;
using MonoGame.Aseprite.Sprites;

namespace Embyr.UI;

/// <summary>
/// A button class that uses sprites to be drawn
/// </summary>
public class SpriteButton : Button {
    private readonly Sprite[] sprites;

    /// <summary>
    /// Creates a new SpriteButton
    /// </summary>
    /// <param name="sprites">Array of 3 sprites, where index 0 is normal, 1 is hover, and 2 is pressed</param>
    /// <param name="toggleable">Whether or not to make button toggle when pressed</param>
    /// <param name="style">ElementStyle to style this button with</param>
    public SpriteButton(Sprite[] sprites, bool toggleable, ElementStyle style)
    : base("", toggleable, new Rectangle(0, 0, sprites[0].Width, sprites[0].Height), style) {
        // setting position manually so alignment adjusts the internal bounds
        Position = Vector2.Zero;
        this.sprites = sprites;
    }

    /// <summary>
    /// Creates a new SpriteButton
    /// </summary>
    /// <param name="file">File to load and process first three sprites from, frame 0 is normal, 1 is hover, 2 is pressed</param>
    /// <param name="toggleable">Whether or not to make button toggle when pressed</param>
    /// <param name="style">ElementStyle to style this button with</param>
    public SpriteButton(AsepriteFile file, bool toggleable, ElementStyle style)
    : base("", toggleable, new Rectangle(0, 0, file.CanvasWidth, file.CanvasHeight), style) {
        // setting position manually so alignment adjusts the internal bounds
        Position = Vector2.Zero;
        sprites = new Sprite[3];
        for (int i = 0; i < 3; i++) {
            sprites[i] = SpriteProcessor.Process(
                SceneManager.I.GraphicsDevice,
                file,
                i
            );
        }
    }

    /// <summary>
    /// Draws this SpriteButton to the screen
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public override void Draw(SpriteBatch sb) {
        int i = Pressed || Toggled ? 2 : Hovered ? 1 : 0;
        sprites[i].Draw(sb, MarginlessBounds.Location.ToVector2());
    }
}
