using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Embyr.UI;
using System.Reflection;
using Embyr.Rendering;

namespace Embyr.Scenes;

/// <summary>
/// Manager class that deals with all game scenes, is a singleton
/// </summary>
public class SceneManager : Singleton<SceneManager>, IResolution {
    private Dictionary<string, Scene> scenes;
    private Game game;

    /// <summary>
    /// Gets whether or not the scene manager is currently loading a scene
    /// </summary>
    public bool IsLoading { get; private set; }

    /// <summary>
    /// Reference to current scene in manager
    /// </summary>
    public Scene CurrentScene { get; private set; }

    /// <summary>
    /// Gets an enumerable object to iterate across all currently stored scenes
    /// </summary>
    public IEnumerable<Scene> Scenes {
        get {
            foreach (Scene scene in scenes.Values) {
                yield return scene;
            }
        }
    }

    /// <summary>
    /// Access to game graphics device, used for aseprite processing
    /// </summary>
    internal GraphicsDevice GraphicsDevice => game.GraphicsDevice;

    /// <summary>
    /// Access to the game renderer, used for batch restarting and other info access
    /// </summary>
    internal Renderer Renderer => game.Renderer;

    /// <summary>
    /// Gets the menu bounds for any new menu to be created
    /// </summary>
    public Rectangle MenuBounds => new(
        new Point(Game.CanvasExpandSize / 2),
        EngineSettings.GameCanvasResolution
    );

    /// <summary>
    /// Initializes the scene manager by loading shaders and setting up scenes. Doesn't load scene content.
    /// </summary>
    /// <param name="game">Game reference to use ContentManager from</param>
    /// <param name="initialScene">Initial scene to use in game</param>
    public void Init(Game game, Scene initialScene) {
        this.game = game;

        // on game exit, unload current scene
        game.Exiting += (sender, args) => CurrentScene?.Unload();

        scenes = new Dictionary<string, Scene>() {
            { initialScene.Name, initialScene }
        };

        CurrentScene = initialScene;
        initialScene.LoadContent();

        ChangeResolution(
            EngineSettings.GameCanvasResolution.X + Game.CanvasExpandSize,
            EngineSettings.GameCanvasResolution.Y + Game.CanvasExpandSize
        );
    }

    #region // Game loop

    /// <summary>
    /// Updates the current scene in the manager
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    public void Update(float dt) {
        if (!IsLoading) {
            CurrentScene.Update(dt);
        }
    }

    /// <summary>
    /// Updates physics of the current scene in the scene manager
    /// </summary>
    /// <param name="dt">Time passed since last fixed update</param>
    public void PhysicsUpdate(float dt) {
        if (!IsLoading) {
            CurrentScene.PhysicsUpdate(dt);
        }
    }

    #endregion

    /// <summary>
    /// Unloads current scene and loads the next scene, switching
    /// after loading is complete
    /// </summary>
    /// <param name="sceneName">Scene name to switch to</param>
    /// <param name="onChangeSuccess">Action to execute when scene loading succeeds</param>
    /// <param name="onChangeFail">Action to execute when scene loading fails</param>
    public void ChangeScene<T>(string sceneName, Action? onChangeSuccess = null, Action? onChangeFail = null) where T : Scene {
        // prevent changing scene while another is already loading
        if (IsLoading) {
            onChangeFail?.Invoke();
            return;
        }

        IsLoading = true;

        if (!scenes.TryGetValue(sceneName, out Scene next)) {
            ConstructorInfo ctor = typeof(T).GetConstructor([typeof(string)]);

            next = ctor?.Invoke([sceneName]) as Scene;
            if (next != null) {
                scenes.Add(sceneName, next);
            } else {
                // if scene is null, we know something went wrong...
                onChangeFail?.Invoke();
                return;
            }
        }

        // doesn't switch scene if the next scene is the same as the current one
        if (next == CurrentScene) {
            IsLoading = false;
            onChangeFail?.Invoke();
            return;
        }

        //* save prev, load next, update "Current" after done loading,
        //*   then unload prev after everything is done

        Scene prev = CurrentScene;

        // try/catch so if an exception is thrown (where a world
        //   scene's type is invalid) it unloads and removes that
        //   from the dictionary
        try {
            // queue current events to clear so new events
            //   (set up in next.LoadContent()) aren't cleared
            Input.QueueEventsToClear();

            CurrentScene = next;

            //! this could throw an exception
            next.LoadContent();

            prev?.Unload();

            // actually clear events in queue
            Input.ClearQueuedEvents();

            onChangeSuccess?.Invoke();

        } catch (Exception ex) {
            // if "next" load fails, unload and remove
            next.Unload();
            scenes.Remove(sceneName);
            CurrentScene = prev;

            onChangeFail?.Invoke();
            Debug.WriteLine("SCENE LOAD FAILED! Stack trace:\n" + ex);

        } finally {
            IsLoading = false;

            // discard any leftover events if there are any from a failed load
            Input.DiscardEventQueue();

            // run garbage collector after unloading prior scenes to
            //   free up memory from old unloaded scenes
            GC.Collect();
        }
    }

    /// <inheritdoc/>
    public void ChangeResolution(int width, int height) {
        if (!IsLoading) {
            CurrentScene.ChangeResolution(width, height);
        }
    }

    /// <summary>
    /// Exits the entire game
    /// </summary>
    public void ExitGame() {
        game.Exit();
    }
}
