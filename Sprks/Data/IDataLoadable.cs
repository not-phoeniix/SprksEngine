using System.Text.Json.Nodes;

namespace Sprks.Data;

public interface IDataLoadable<T> {
    public static abstract T FromJson(JsonNode data);
}
