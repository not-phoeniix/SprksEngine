using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr;

/// <summary>
/// Global helper class to load/manage assets in an Embyr game
/// </summary>
public static class Assets {
    private static ContentManager localContent;
    private static Game game;

    /// <summary>
    /// Gets the reference to the graphics device of the currently running game
    /// </summary>
    public static GraphicsDevice GraphicsDevice => game.GraphicsDevice;

    /// <summary>
    /// Initializes the Assets manager
    /// </summary>
    /// <param name="game">Game to initialize from</param>
    internal static void Init(Game game) {
        localContent = new ContentManager(game.Content.ServiceProvider, game.Content.RootDirectory);
        Assets.game = game;
    }

    /// <summary>
    /// Clears local content manager and cache
    /// </summary>
    internal static void ClearLocalContent() {
        localContent?.Unload();
        localContent?.Dispose();

        localContent = new ContentManager(game.Content.ServiceProvider, game.Content.RootDirectory);
    }

    /// <summary>
    /// Loads an asset that has been processed by the content pipeline
    /// </summary>
    /// <typeparam name="T">Type of content asset to load</typeparam>
    /// <param name="content">String path of the processed asset</param>
    /// <returns>Loaded asset, returns the same reference with repeated calls</returns>
    public static T Load<T>(string content) {
        return localContent.Load<T>(content);
    }

    /// <summary>
    /// Unloads an asset that has been previously loaded
    /// </summary>
    /// <param name="content">String path of the processed asset</param>
    public static void Unload(string content) {
        localContent.UnloadAsset(content);
    }

    /// <summary>
    /// Loads an asset that has been processed by the content pipeline, caches globally and is never unloaded automatically
    /// </summary>
    /// <typeparam name="T">Type of content asset to load</typeparam>
    /// <param name="content">String path of the processed asset</param>
    /// <returns>Loaded asset, returns the same reference with repeated calls</returns>
    public static T LoadGlobal<T>(string content) {
        return game.Content.Load<T>(content);
    }

    /// <summary>
    /// Unloads an asset that has been previously loaded globally
    /// </summary>
    /// <param name="content">String path of the processed asset</param>
    public static void UnloadGlobal(string content) {
        game.Content.UnloadAsset(content);
    }
}
