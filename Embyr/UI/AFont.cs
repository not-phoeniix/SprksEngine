using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Content.Processors;
using MonoGame.Aseprite.Sprites;

namespace Embyr.UI;

/// <summary>
/// A custom aseprite font, created from a loaded aseprite file
/// </summary>
public class AFont {
    private readonly Sprite[] letters;
    private readonly int spacing;

    /// <summary>
    /// Creates a new AFont
    /// </summary>
    /// <param name="file">Aseprite file to load characters from</param>
    /// <param name="spacing">Spacing (in pixels) between characters when drawn</param>
    /// <param name="gd">GraphicsDevice to process file characters with</param>
    public AFont(AsepriteFile file, int spacing, GraphicsDevice gd) {
        if (file.FrameCount != 96) {
            throw new System.Exception($"ERROR: input file \"{file.Name}\" has incorrect number of frames for 96 letters!");
        }

        letters = new Sprite[96];
        for (int i = 0; i < 96; i++) {
            letters[i] = SpriteProcessor.Process(gd, file, i);
        }

        this.spacing = spacing;
    }

    public void Draw(string text, Vector2 position, Color color, SpriteBatch sb) {
        // offset used for tracking character spacing and such
        Vector2 offset = Vector2.Zero;

        for (int i = 0; i < text.Length; i++) {
            // on a newline character, go down a line w/ offset and reset x to 0
            if (text[i] == '\n') {
                offset.Y += GetChar(' ').Height + spacing;
                offset.X = 0;
                continue;
            }

            if (text[i] == '\t') {
                offset.X += (GetChar(' ').Width + spacing) * 4;
            }

            // grab ref to sprite, set color, draw, then update offset for next character
            Sprite sprite = GetChar(text[i]);
            sprite.Color = color;
            sprite.Draw(sb, position + offset);
            offset += new Vector2(sprite.Width + spacing, 0);
        }
    }

    public Vector2 MeasureString(string text) {
        Vector2 size = new(0, GetChar(' ').Height);
        Vector2 tmpSize = Vector2.Zero;
        string stringSoFar = "";
        for (int i = 0; i < text.Length; i++) {
            char c = text[i];
            stringSoFar += c;
            Sprite letter = GetChar(c);

            if (text[i] == '\n') {
                tmpSize.X = 0;
                size.Y += letter.Height + spacing;
            } else if (text[i] == '\t') {
                tmpSize.X += (letter.Width + spacing) * 4;
            } else {
                tmpSize.X += letter.Width + spacing;
            }

            if (tmpSize.X > size.X) size.X = tmpSize.X;
            if (tmpSize.Y > size.Y) size.Y = tmpSize.Y;
        }

        return size;
    }

    private Sprite GetChar(char c) {
        // look at this website for a visual ascii conversion :]
        // https://www.asciitable.com/

        // converted character code, where index 0 is
        //   space and everything else follows suit
        int code = c - ' ';

        // return unrecognized character if out of bounds
        if (code < 0 || code > 94) return letters[95];

        // return calculated character regularly
        return letters[code];
    }
}
