using System.Text.Json.Nodes;

namespace Embyr.Data;

public interface IDataLoadable<T> {
    public static abstract T FromJson(JsonNode data);
}
