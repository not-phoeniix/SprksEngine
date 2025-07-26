using System.Text.Json.Nodes;

namespace Embyr.Content;

public class JsonProcessedResult {
   public JsonNode ProcessedJson { get; set; }
   public string ReaderClass { get; set; }
}
