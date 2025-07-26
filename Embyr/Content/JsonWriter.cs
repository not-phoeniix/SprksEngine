using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Embyr.Content;

[ContentTypeWriter]
public class JsonWriter : ContentTypeWriter<JsonProcessedResult> {
    private string readerClass;

    protected override void Write(ContentWriter output, JsonProcessedResult value) {
        output.Write(value.ProcessedJson.ToJsonString());
        readerClass = value.ReaderClass;
    }

    public override string GetRuntimeReader(TargetPlatform targetPlatform) {
        return readerClass;
    }
}
