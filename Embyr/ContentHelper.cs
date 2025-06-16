using System;
using System.Collections.Generic;
using Embyr.UI;
using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Content.Processors;
using MonoGame.Aseprite.Sprites;
using System.Reflection;
using Embyr.Rendering;

namespace Embyr;

/// <summary>
/// Helper class that deals with loading assets/content
/// </summary>
public class ContentHelper : Singleton<ContentHelper> {
    private Microsoft.Xna.Framework.Game game;
    private Game.RenderPipeline pipeline;
    private ContentManager localContent;
    private readonly Dictionary<string, AFont> globalAFontCache = new();

    private readonly Dictionary<string, Sprite> spriteCache = new();
    private readonly Dictionary<string, SpriteSheet> spriteSheetCache = new();
    private readonly Dictionary<string, AFont> aFontCache = new();
    private readonly Dictionary<string, Texture2D> tilesetCache = new();
    private readonly Dictionary<string, GameMesh> gameMeshCache = new();

    /// <summary>
    /// Gets the root directory of the game's content managers
    /// </summary>
    public string ContentRootDir => game.Content.RootDirectory;

    /// <summary>
    /// Initializes the content helper singleton
    /// </summary>
    /// <param name="game">Game to initialize content from</param>
    public void Init(Microsoft.Xna.Framework.Game game, Game.RenderPipeline pipeline) {
        this.game = game;
        this.pipeline = pipeline;
        LocalReset();
    }

    /// <summary>
    /// Resets and unloads all local content of the current scene, creating a new local content manager
    /// </summary>
    public void LocalReset() {
        spriteCache.Clear();
        spriteSheetCache.Clear();
        aFontCache.Clear();
        tilesetCache.Clear();
        gameMeshCache.Clear();

        localContent?.Unload();
        localContent?.Dispose();
        localContent = new ContentManager(game.Content.ServiceProvider, game.Content.RootDirectory);
    }

    #region // content loading itself

    /// <summary>
    /// Loads content either from custom format or from internal
    /// local content manager. Will be unloaded at every scene change.
    /// </summary>
    /// <typeparam name="T">Type of content to load</typeparam>
    /// <param name="contentName">Content string path</param>
    /// <returns>Reference to cached content</returns>
    public T Load<T>(string contentName) {
        if (localContent == null) {
            return default;
        }

        Type type = typeof(T);

        if (type == typeof(Sprite)) {
            return (T)GetContentSprite(contentName);
        } else if (type == typeof(SpriteSheet)) {
            return (T)GetContentSpriteSheet(contentName);
        } else if (type == typeof(AFont)) {
            return (T)GetContentAFont(contentName);
        } else if (type == typeof(Texture2D)) {
            return (T)GetContentTileset(contentName);
        } else if (type == typeof(GameMesh)) {
            return (T)GetContentGameMesh(contentName);
        }

        return localContent.Load<T>(contentName);

        throw new NotSupportedException($"Type {typeof(T)} not supported!");
    }

    /// <summary>
    /// Unloads and removes all instances of an asset from internal
    /// cache and content manager from LOCAL CACHE/MANAGER.
    /// </summary>
    /// <param name="contentName">Content string path to unload</param>
    public void Unload(string contentName) {
        spriteCache.Remove(contentName);
        spriteSheetCache.Remove(contentName);
        aFontCache.Remove(contentName);
        tilesetCache.Remove(contentName);

        localContent.UnloadAsset(contentName);
    }

    /// <summary>
    /// Loads a content asset globally from global content cache, is never unloaded
    /// </summary>
    /// <typeparam name="T">Object type of content to load</typeparam>
    /// <param name="contentName">Content asset name/path</param>
    /// <returns>A reference to the loaded content</returns>
    public T LoadGlobal<T>(string contentName) {
        if (typeof(T) == typeof(AFont)) {
            if (!globalAFontCache.TryGetValue(contentName, out AFont font)) {
                AsepriteFile file = game.Content.Load<AsepriteFile>(contentName);
                font = new(file, 1, game.GraphicsDevice);
                globalAFontCache[contentName] = font;
            }

            return (T)(object)font;
        }

        return game.Content.Load<T>(contentName);
    }

    /// <summary>
    /// Gets a Sprite from internal scene cache
    /// </summary>
    /// <param name="contentName">Content string path to the Sprite's aseprite file</param>
    /// <returns>Reference to cached Sprite</returns>
    private object GetContentSprite(string contentName) {
        if (!spriteCache.TryGetValue(contentName, out Sprite sprite)) {
            AsepriteFile file = localContent.Load<AsepriteFile>(contentName);
            sprite = SpriteProcessor.Process(game.GraphicsDevice, file, 0);
            spriteCache[contentName] = sprite;
        }

        return sprite;
    }

    /// <summary>
    /// Gets a SpriteSheet from internal scene cache
    /// </summary>
    /// <param name="contentName">Content string path to the SpriteSheet's aseprite file</param>
    /// <returns>Reference to cached SpriteSheet</returns>
    private object GetContentSpriteSheet(string contentName) {
        if (!spriteSheetCache.TryGetValue(contentName, out SpriteSheet sheet)) {
            AsepriteFile file = localContent.Load<AsepriteFile>(contentName);
            sheet = SpriteSheetProcessor.Process(game.GraphicsDevice, file);
            spriteSheetCache[contentName] = sheet;
        }

        return sheet;
    }

    /// <summary>
    /// Gets an AFont from internal scene cache
    /// </summary>
    /// <param name="contentName">Content string path to the AFont aseprite file</param>
    /// <returns>Reference to cached AFont</returns>
    private object GetContentAFont(string contentName) {
        if (!aFontCache.TryGetValue(contentName, out AFont font)) {
            AsepriteFile file = localContent.Load<AsepriteFile>(contentName);
            font = new(file, 1, game.GraphicsDevice);
            aFontCache[contentName] = font;
        }

        return font;
    }

    /// <summary>
    /// Gets a Tile tileset from internal scene cache
    /// </summary>
    /// <param name="contentName">Content string path to the tileset aseprite file</param>
    /// <returns>Reference to cached Texture2D tileset</returns>
    private object GetContentTileset(string contentName) {
        // TODO: figure out a better way to store and do this I BEG
        if (!tilesetCache.TryGetValue(contentName, out Texture2D tileset)) {
            try {
                tileset = localContent.Load<Texture2D>(contentName);
            } catch {
                try {
                    AsepriteFile file = localContent.Load<AsepriteFile>(contentName);
                    Sprite sprite = SpriteProcessor.Process(game.GraphicsDevice, file, 0);
                    tileset = sprite.TextureRegion.Texture;
                } catch {
                    throw;
                }
            }

            tilesetCache[contentName] = tileset;
        }

        return tileset;
    }

    /// <summary>
    /// Gets a GameMesh from internal scene cache
    /// </summary>
    /// <param name="contentName">Content string path to mesh</param>
    /// <returns>Reference to cached game mesh</returns>
    private object GetContentGameMesh(string contentName) {
        if (!gameMeshCache.TryGetValue(contentName, out GameMesh mesh)) {
            Model model = localContent.Load<Model>(contentName);
            mesh = new GameMesh(model, game.GraphicsDevice);
            gameMeshCache[contentName] = mesh;
        }

        return mesh;
    }

    #endregion
}
