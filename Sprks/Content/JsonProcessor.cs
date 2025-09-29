using System.ComponentModel;
using System.Text.Json.Nodes;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Sprks.Content;

[ContentProcessor(DisplayName = "JSON Processor - Sprks Engine")]
public class JsonProcessor : ContentProcessor<string, JsonProcessedResult> {
    [DisplayName("Reader Class")]
    public string ReaderClass { get; set; } = "";

    public override JsonProcessedResult Process(string input, ContentProcessorContext context) {
        if (string.IsNullOrEmpty(ReaderClass)) {
            throw new Exception("ERROR: Cannot process JSON data without Reader Class specified!");
        }

        return new JsonProcessedResult() {
            ProcessedJson = JsonNode.Parse(input),
            ReaderClass = ReaderClass
        };
    }
}
