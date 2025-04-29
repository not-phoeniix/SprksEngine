using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Content.Processors;
using MonoGame.Aseprite.Sprites;

namespace Embyr.Scenes;

/// <summary>
/// A layer that does parallax scrolling, grabs current camera and
/// uses inputted speed to create depth effect
/// </summary>
public class ParallaxLayer : IDrawable {
    private readonly Sprite sprite;
    private readonly float speed;
    private readonly bool hRepeat;
    private readonly bool vRepeat;

    /// <summary>
    /// Gets/sets the pixel offset of this parallax layer
    /// </summary>
    public Vector2 Offset { get; set; }

    /// <summary>
    /// Gets the world position of the first-drawn sprite for parallax layer
    /// </summary>
    public Vector2 WorldLocation { get; private set; }

    /// <summary>
    /// Creates a new ParallaxLayer
    /// </summary>
    /// <param name="file">AsepriteFile to grab frames from</param>
    /// <param name="frameIndex">Index of frame to grab sprite from</param>
    /// <param name="speed">
    /// Speed percentage to apply parallax with relative to camera
    /// (usually between 0 and 1 but can be anything lol)
    /// </param>
    /// <param name="offset">Offset to original position</param>
    /// <param name="hRepeat">Whether or not to repeat sprite horizontally</param>
    /// <param name="vRepeat">Whether or not to repeat sprite vertically</param>
    public ParallaxLayer(
        AsepriteFile file,
        int frameIndex,
        float speed,
        Vector2 offset,
        bool hRepeat = true,
        bool vRepeat = true
    ) {
        sprite = SpriteProcessor.Process(SceneManager.I.GraphicsDevice, file, frameIndex);
        this.speed = speed;
        this.Offset = offset;
        this.hRepeat = hRepeat;
        this.vRepeat = vRepeat;
    }

    /// <summary>
    /// Creates a ParallaxLayer
    /// </summary>
    /// <param name="file">AsepriteFile to grab frames from</param>
    /// <param name="frameIndex">Index of frame to grab sprite from</param>
    /// <param name="speed">
    /// Speed percentage to apply parallax with relative to camera
    /// (usually between 0 and 1 but can be anything lol)
    /// </param>
    public ParallaxLayer(
        AsepriteFile file,
        int frameIndex,
        float speed
    ) : this(file, frameIndex, speed, Vector2.Zero) { }

    /// <summary>
    /// Creates a new ParallaxLayer
    /// </summary>
    /// <param name="sprite">Sprite to use as parallax image</param>
    /// <param name="speed">
    /// Speed percentage to apply parallax with relative to camera
    /// (usually between 0 and 1 but can be anything lol)
    /// </param>
    /// <param name="offset">Offset to original position</param>
    /// <param name="hRepeat">Whether or not to repeat sprite horizontally</param>
    /// <param name="vRepeat">Whether or not to repeat sprite vertically</param>
    public ParallaxLayer(
        Sprite sprite,
        float speed,
        Vector2 offset,
        bool hRepeat = true,
        bool vRepeat = true
    ) {
        this.sprite = sprite;
        this.speed = speed;
        this.Offset = offset;
        this.hRepeat = hRepeat;
        this.vRepeat = vRepeat;
    }

    /// <summary>
    /// Draws this parallax layer to the screen (auto tiled)
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public void Draw(SpriteBatch sb) {
        // don't draw parallax layer if camera cannot be found
        Camera camera = SceneManager.I.Camera;
        if (camera == null) return;

        // number of sprites to draw in the x/y direction
        Point repeatSize = new(
            (int)Math.Ceiling((float)camera.ViewBounds.Width / sprite.Width),
            (int)Math.Ceiling((float)camera.ViewBounds.Height / sprite.Height)
        );

        // offset to apply to camera position that allows scrolling via speed ratio
        Vector2 unitOffset = new(
            (camera.Position.X * speed) % sprite.Width,
            (camera.Position.Y * speed) % sprite.Height
        );

        bool foundWorldLocation = false;

        for (int y = -repeatSize.Y; y <= repeatSize.Y; y++) {
            for (int x = -repeatSize.X; x <= repeatSize.X; x++) {
                Vector2 iterOffset = new(x * sprite.Width, y * sprite.Height);
                Vector2 location = camera.Position - unitOffset + iterOffset + Offset;

                if (!hRepeat) {
                    location.X = camera.Position.X - (camera.Position.X * speed) + Offset.X;
                }

                if (!vRepeat) {
                    location.Y = camera.Position.Y - (camera.Position.Y * speed) + Offset.Y;
                }

                if (!foundWorldLocation) {
                    WorldLocation = location;
                    foundWorldLocation = true;
                }

                sb.Draw(sprite, Vector2.Floor(location));

                if (!hRepeat) break;
            }

            if (!vRepeat) break;
        }
    }
}
