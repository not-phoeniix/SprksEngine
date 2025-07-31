using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr;

/// <summary>
/// Describes a method that takes a loaded asset as input and processes it into another asset type
/// </summary>
/// <param name="input">Input asset loaded by the pipeline</param>
/// <returns>A processed/transformed new asset based on input asset</returns>
public delegate object ProcessCustomAssetDelegate(object input);

/// <summary>
/// Global asset manager class to load/manage assets in an Embyr game
/// </summary>
public static class Assets {
    private static ContentManager localContent;
    private static Game game;

    // maps string content name to loaded content itself
    private static readonly Dictionary<string, object> customLocalContent = new();
    private static readonly Dictionary<string, object> customGlobalContent = new();

    // maps output type to function that loads the input content
    private static readonly Dictionary<Type, Func<string, bool, object>> loadInputFuncs = new();

    // maps output type to function used to create custom content from input content
    private static readonly Dictionary<Type, ProcessCustomAssetDelegate> processInputFuncs = new();

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
    /// Clears local content manager and cache, calling dispose on all disposable content
    /// </summary>
    internal static void ClearLocalContent() {
        localContent?.Unload();
        localContent?.Dispose();

        localContent = new ContentManager(game.Content.ServiceProvider, game.Content.RootDirectory);

        foreach (object obj in customLocalContent.Values) {
            if (obj is IDisposable d) {
                d.Dispose();
            }
        }
        customLocalContent.Clear();
    }

    /// <summary>
    /// Loads an asset that has been processed by the content pipeline
    /// </summary>
    /// <typeparam name="T">Type of content asset to load</typeparam>
    /// <param name="content">String path of the processed asset</param>
    /// <returns>Loaded asset, returns the same reference with repeated calls</returns>
    public static T Load<T>(string content) {
        if (IsCustomType(typeof(T))) {
            return GetCustomContent<T>(content, isGlobal: false);
        }

        return localContent.Load<T>(content);
    }

    /// <summary>
    /// Unloads an asset that has been previously loaded, disposing it if necessary
    /// </summary>
    /// <param name="content">String path of the processed asset</param>
    public static void Unload(string content) {
        localContent.UnloadAsset(content);

        object asset = customLocalContent[content];
        if (asset is IDisposable d) {
            d.Dispose();
        }
        customLocalContent.Remove(content);
    }

    /// <summary>
    /// Loads an asset that has been processed by the content pipeline, caches globally and is never unloaded automatically
    /// </summary>
    /// <typeparam name="T">Type of content asset to load</typeparam>
    /// <param name="content">String path of the processed asset</param>
    /// <returns>Loaded asset, returns the same reference with repeated calls</returns>
    public static T LoadGlobal<T>(string content) {
        if (IsCustomType(typeof(T))) {
            return GetCustomContent<T>(content, isGlobal: true);
        }

        return game.Content.Load<T>(content);
    }

    /// <summary>
    /// Unloads an asset that has been previously loaded globally, disposing it if necessary
    /// </summary>
    /// <param name="content">String path of the processed asset</param>
    public static void UnloadGlobal(string content) {
        game.Content.UnloadAsset(content);

        object asset = customGlobalContent[content];
        if (asset is IDisposable d) {
            d.Dispose();
        }
        customGlobalContent.Remove(content);
    }

    /// <summary>
    /// Adds a new custom 2-stage asset type to the asset pipeline
    /// </summary>
    /// <typeparam name="I">Type of input asset to be processed initially, must be loadable already</typeparam>
    /// <typeparam name="O">Type of output asset, the type that you will be able to load anywhere</typeparam>
    /// <param name="processInstructions">
    /// Process instructions to invoke that takes in an input loaded
    /// asset and processes and returns the new processed asset
    /// </param>
    public static void AddAssetType<I, O>(ProcessCustomAssetDelegate processInstructions) {
        processInputFuncs[typeof(O)] = processInstructions;
        loadInputFuncs[typeof(O)] = (content, isGlobal) => {
            if (isGlobal) {
                return LoadGlobal<I>(content);
            } else {
                return Load<I>(content);
            }
        };
    }

    private static bool IsCustomType(Type t) {
        return loadInputFuncs.ContainsKey(t) && processInputFuncs.ContainsKey(t);
    }

    private static T GetCustomContent<T>(string content, bool isGlobal) {
        Type type = typeof(T);
        if (!IsCustomType(type)) {
            throw new Exception($"Cannot load content, asset type \"{typeof(T)}\" was never added to the asset pipeline!");
        }

        Dictionary<string, object> contentDict = isGlobal ? customGlobalContent : customLocalContent;

        content = content.Replace('\\', '/');
        if (!contentDict.TryGetValue(content, out object processedOutput)) {
            // we get the input content by loading from the cached function
            Func<string, bool, object> loadInput = loadInputFuncs[type];
            object inputContent = loadInput.Invoke(content, isGlobal);

            // then we process the input content into our output type
            ProcessCustomAssetDelegate processInput = processInputFuncs[type];
            processedOutput = processInput.Invoke(inputContent);

            // then we cache :]
            contentDict[content] = processedOutput;
        }

        return (T)processedOutput;
    }
}
