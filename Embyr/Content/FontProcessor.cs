using System.Diagnostics;
using System.Text.Json.Nodes;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Embyr.Content;

[ContentProcessor(DisplayName = "Font Processor - Embyr Engine")]
public class FontProcessor : ContentProcessor<string, JsonProcessedResult> {
    public override JsonProcessedResult Process(string input, ContentProcessorContext context) {
        string readerClass = $"Embyr.{typeof(FontReader).FullName}, Embyr";

        context.Logger.LogMessage("hiii... reader class: \"" + readerClass + "\"");
        Debug.WriteLine("HELOOOOO !!! HAYO !!!!!!");

        return new JsonProcessedResult() {
            ProcessedJson = JsonNode.Parse(input),
            ReaderClass = readerClass
        };
    }
}
