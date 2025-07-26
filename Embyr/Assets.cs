using Microsoft.Xna.Framework.Content;

namespace Embyr;

public static class Assets {
    private static ContentManager localContent;
    private static Game game;

    internal static void Init(Game game) {
        localContent = new ContentManager(game.Content.ServiceProvider, game.Content.RootDirectory);
    }

    internal static void ClearLocalContent() {
        localContent?.Unload();
        localContent?.Dispose();

        localContent = new ContentManager(game.Content.ServiceProvider, game.Content.RootDirectory);
    }

    public static T Load<T>(string content) {
        return localContent.Load<T>(content);
    }

    public static void Unload(string content) {
        localContent.UnloadAsset(content);
    }

    public static T LoadGlobal<T>(string content) {
        return game.Content.Load<T>(content);
    }

    public static void UnloadGlobal(string content) {
        game.Content.UnloadAsset(content);
    }
}
