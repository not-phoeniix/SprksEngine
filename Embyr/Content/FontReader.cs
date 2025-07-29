using System.Text.Json.Nodes;
using Embyr.UI;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Content;

/// <summary>
/// Content reader for reading and loading <c>Embyr.Font</c> instances
/// </summary>
public class FontReader : ContentTypeReader<Font> {
    protected override Font Read(ContentReader input, Font existingInstance) {
        JsonNode? node = JsonNode.Parse(input.ReadString());

        string name = node["name"].GetValue<string>();
        string? url = node["url"]?.GetValue<string>();
        string? creator = node["creator"]?.GetValue<string>();
        string atlasTexture = node["atlasTexture"].GetValue<string>();
        int charWidth = node["charWidth"].GetValue<int>();
        int charHeight = node["charHeight"].GetValue<int>();
        int atlasPadding = node["atlasPadding"].GetValue<int>();
        int atlasGaps = node["atlasGaps"].GetValue<int>();

        Texture2D texture = Assets.Load<Texture2D>(atlasTexture);

        return new Font(
            texture,
            name,
            creator,
            url,
            charWidth,
            charHeight,
            atlasGaps,
            atlasPadding
        );
    }
}
