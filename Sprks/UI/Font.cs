using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sprks.UI;

/// <summary>
/// Describes a bitmap-style font, includes drawing instructions
/// </summary>
public class Font {
    // number of letters across and upwards in the atlas texture
    private const int textureAtlasWidth = 16;
    private const int textureAtlasHeight = 8;

    private readonly Texture2D texture;
    private readonly int charWidth;
    private readonly int charHeight;
    private readonly int atlasGaps;
    private readonly int atlasPadding;

    /// <summary>
    /// Gets/sets the number of pixels separating characters when drawing, defaults to 1
    /// </summary>
    public int CharSpacing { get; set; }

    /// <summary>
    /// Gets/sets the number of pixels separating text lines when drawing, defaults to 1
    /// </summary>
    public int LineSpacing { get; set; }

    /// <summary>
    /// Gets the display name of this font
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the creator of this font, can be null
    /// </summary>
    public string? Creator { get; }

    /// <summary>
    /// Gets the source url for the font download, can be null
    /// </summary>
    public string? Url { get; }

    internal Font(Texture2D texture, string name, string? creator, string? url, int charWidth, int charHeight, int atlasGaps, int atlasPadding) {
        this.texture = texture;
        this.charWidth = charWidth;
        this.charHeight = charHeight;
        this.atlasGaps = atlasGaps;
        this.atlasPadding = atlasPadding;
        this.CharSpacing = 1;
        this.LineSpacing = 1;
        this.Name = name;
        this.Creator = creator;
        this.Url = url;
    }

    /// <summary>
    /// Draws text in this font to the screen
    /// </summary>
    /// <param name="text">Text to draw</param>
    /// <param name="position">Top-left-aligned position of text to draw at</param>
    /// <param name="color">Color of text to draw</param>
    /// <param name="sb">SpriteBatch to draw with</param>
    public void DrawString(string text, Vector2 position, Color color, SpriteBatch sb) {
        Vector2 offset = Vector2.Zero;

        foreach (char c in text) {
            if (c == '\n') {
                offset.Y += charHeight + LineSpacing;
                offset.X = 0;
            } else {
                sb.Draw(texture, Vector2.Floor(position + offset), GetSource(c), color);

                offset.X += charWidth + CharSpacing;
            }
        }
    }

    /// <summary>
    /// Measures the size of a string when drawn with this font
    /// </summary>
    /// <param name="text">Text to measure size of</param>
    /// <returns>A new Vector2 where the X/Y components represent width/height of string</returns>
    public Vector2 MeasureString(string text) {
        Vector2 size = new(0, charHeight);
        int tempWidth = 0;

        int i = 0;
        foreach (char c in text) {
            if (c == '\n') {
                size.Y += charHeight + LineSpacing;
                tempWidth = 0;
            } else {
                tempWidth += charWidth;

                // only add separation amount if we're not
                //   at the end of the string
                if (i != text.Length - 1) {
                    tempWidth += CharSpacing;
                }

                size.X = Math.Max(size.X, tempWidth);
            }

            i++;
        }

        return size;
    }

    private Rectangle GetSource(char c) {
        int xIndex = c % textureAtlasWidth;
        int yIndex = c / textureAtlasWidth;

        Point pos = new(
            atlasPadding + (xIndex * (charWidth + atlasGaps)),
            atlasPadding + (yIndex * (charHeight + atlasGaps))
        );

        return new Rectangle(pos, new Point(charWidth, charHeight));
    }
}
