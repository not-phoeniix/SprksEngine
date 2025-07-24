using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.UI;

public class Font {
    // number of letters across and upwards in the atlas texture
    private const int textureAtlasWidth = 16;
    private const int textureAtlasHeight = 8;

    private readonly Texture2D texture;
    private readonly int charWidth;
    private readonly int charHeight;
    private readonly int charHorizSeparation;
    private readonly int charVertSeparation;

    private readonly int atlasGaps;
    private readonly int atlasPadding;

    public Font(Texture2D texture, int charWidth, int charHeight, int atlasGaps, int atlasPadding, int charHorizSeparation, int charVertSeparation) {
        this.texture = texture;
        this.charWidth = charWidth;
        this.charHeight = charHeight;
        this.charHorizSeparation = charHorizSeparation;
        this.charVertSeparation = charVertSeparation;
        this.atlasGaps = atlasGaps;
        this.atlasPadding = atlasPadding;
    }

    public void DrawString(string text, Vector2 position, Color color, SpriteBatch sb) {
        Vector2 offset = Vector2.Zero;

        foreach (char c in text) {
            if (c == '\n') {
                offset.Y += charHeight + charVertSeparation;
                offset.X = 0;
            } else {
                sb.Draw(texture, Vector2.Floor(position + offset), GetSource(c), color);

                offset.X += charWidth + charHorizSeparation;
            }
        }
    }

    public Vector2 MeasureString(string text) {
        Vector2 size = new(0, charHeight);
        int tempWidth = 0;

        int i = 0;
        foreach (char c in text) {
            if (c == '\n') {
                size.Y += charHeight + charVertSeparation;
                tempWidth = 0;
            } else {
                tempWidth += charWidth;

                // only add separation amount if we're not
                //   at the end of the string
                if (i != text.Length - 1) {
                    tempWidth += charHorizSeparation;
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
