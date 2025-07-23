using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.UI;

public class Font {
    // number of letters across and upwards in the atlas texture
    private const int textureAtlasWidth = 16;
    private const int textureAtlasHeight = 8;
    private const int textureAtlasSpacing = 2;

    private readonly Texture2D texture;
    private readonly int charWidth;
    private readonly int charHeight;
    private readonly int charHorizSeparation;
    private readonly int charVertSeparation;

    public Font(Texture2D texture, int charWidth, int charHeight, int charHorizSeparation, int charVertSeparation) {
        this.texture = texture;
        this.charWidth = charWidth;
        this.charHeight = charHeight;
        this.charHorizSeparation = charHorizSeparation;
        this.charVertSeparation = charVertSeparation;
    }

    public void DrawString(string str, Vector2 position, Color color, SpriteBatch sb) {
        Vector2 offset = Vector2.Zero;

        foreach (char c in str) {
            if (c == '\n') {
                offset.Y += charHeight + charVertSeparation;
                offset.X = 0;
            } else {
                sb.Draw(texture, position + offset, GetSource(c), color);

                offset.X += charWidth + charHorizSeparation;
            }
        }
    }

    private Rectangle GetSource(char c) {
        int xIndex = c % textureAtlasWidth;
        int yIndex = c / textureAtlasWidth;

        Point pos = new(
            1 + xIndex * (charWidth + textureAtlasSpacing),
            1 + yIndex * (charHeight + textureAtlasSpacing)
        );

        return new Rectangle(pos, new Point(charWidth, charHeight));
    }
}
