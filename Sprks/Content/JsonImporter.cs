using Microsoft.Xna.Framework.Content.Pipeline;

namespace Sprks.Content;

[ContentImporter(".json", DisplayName = "JSON Importer - Sprks Engine", DefaultProcessor = nameof(JsonProcessor))]
public class JsonImporter : ContentImporter<string> {
    public override string Import(string filename, ContentImporterContext context) {
        return File.ReadAllText(filename);
    }
}
