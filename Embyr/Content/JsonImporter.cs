using Microsoft.Xna.Framework.Content.Pipeline;

namespace Embyr.Content;

[ContentImporter(".json", DisplayName = "JSON Importer - Embyr Engine", DefaultProcessor = nameof(JsonProcessor))]
public class JsonImporter : ContentImporter<string> {
    public override string Import(string filename, ContentImporterContext context) {
        return File.ReadAllText(filename);
    }
}
