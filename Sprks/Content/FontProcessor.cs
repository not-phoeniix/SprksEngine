using System.Diagnostics;
using System.Text.Json.Nodes;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Sprks.Content;

[ContentProcessor(DisplayName = "Font Processor - Sprks Engine")]
public class FontProcessor : ContentProcessor<string, JsonProcessedResult> {
    public override JsonProcessedResult Process(string input, ContentProcessorContext context) {
        string readerClass = $"{typeof(FontReader).FullName}, Sprks";

        return new JsonProcessedResult() {
            ProcessedJson = JsonNode.Parse(input),
            ReaderClass = readerClass
        };
    }
}
