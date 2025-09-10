using System.Diagnostics;
using System.Reflection;
using Embyr.Rendering;

namespace Embyr.Scenes;

/// <summary>
/// Manager class that deals with all game scenes, is a singleton
/// </summary>
public static class SceneManager {
    private static Game game;
    private static Action? queuedSceneChangeProc;

    /// <summary>
    /// Gets whether or not the scene manager is currently loading a scene
    /// </summary>
    public static bool IsLoading { get; private set; }

    /// <summary>
    /// Reference to current scene in manager
    /// </summary>
    public static Scene CurrentScene { get; private set; }

    /// <summary>
    /// Access to the game renderer, used for batch restarting and other info access
    /// </summary>
    internal static Renderer Renderer => game.Renderer;

    /// <summary>
    /// Initializes the scene manager by loading shaders and setting up scenes. Doesn't load scene content.
    /// </summary>
    /// <param name="game">Game reference to use ContentManager from</param>
    /// <param name="initialSceneType">Initial scene to use in game</param>
    public static void Init(Game game, Type initialSceneType) {
        SceneManager.game = game;
        ChangeScene(initialSceneType);
    }

    #region // Game loop

    /// <summary>
    /// Updates the current scene in the manager
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    internal static void Update(float dt) {
        if (!IsLoading) {
            CurrentScene.Update(dt);
        }
    }

    /// <summary>
    /// Updates physics of the current scene in the scene manager
    /// </summary>
    /// <param name="dt">Time passed since last fixed update</param>
    internal static void PhysicsUpdate(float dt) {
        if (!IsLoading) {
            CurrentScene.PhysicsUpdate(dt);
        }
    }

    /// <summary>
    /// Changes scene if one was queued to change
    /// </summary>
    internal static void ChangeQueuedScene() {
        if (queuedSceneChangeProc != null) {
            queuedSceneChangeProc.Invoke();
            queuedSceneChangeProc = null;
        }
    }

    #endregion

    /// <summary>
    /// Queues a scene change to occur at the end of the update loop, unloading previous and loading scene
    /// </summary>
    /// <typeparam name="T">Type of scene to change to, must inherit from <see cref="Scene"/></typeparam>
    /// <param name="onChangeSuccess">Action to execute when scene loading succeeds, passes newly loaded scene in as parameter</param>
    /// <param name="onChangeFail">Action to execute when scene loading fails</param>
    public static void QueueChangeScene<T>(Action<T>? onChangeSuccess = null, Action? onChangeFail = null) where T : Scene {
        queuedSceneChangeProc = () => ChangeScene<T>(onChangeSuccess, onChangeFail);
    }

    /// <summary>
    /// Change currently active scene, unloading and destroying the previous scene
    /// </summary>
    /// <param name="onChangeSuccess">Action to execute when scene loading succeeds, passes newly loaded scene in as parameter</param>
    /// <param name="onChangeFail">Action to execute when scene loading fails</param>
    /// <typeparam name="T">Type of scene to change to, must inherit from scene and have an empty constructor</typeparam>
    public static void ChangeScene<T>(Action<T>? onChangeSuccess = null, Action? onChangeFail = null) where T : Scene {
        Action<Scene>? onSuccessNonGeneric = null;
        if (onChangeSuccess != null) {
            onSuccessNonGeneric = (scene) => {
                onChangeSuccess.Invoke((scene as T)!);
            };
        }

        ChangeScene(typeof(T), onSuccessNonGeneric, onChangeFail);
    }

    /// <summary>
    /// Change currently active scene, unloading and destroying the previous scene
    /// </summary>
    /// <param name="sceneType">Type of scene to change to, must inherit from scene and have an empty constructor</param>
    /// <param name="onChangeSuccess">Action to execute when scene loading succeeds, passes newly loaded scene in as parameter</param>
    /// <param name="onChangeFail">Action to execute when scene loading fails</param>
    public static void ChangeScene(Type sceneType, Action<Scene>? onChangeSuccess = null, Action? onChangeFail = null) {
        // prevent changing scene while another is already loading
        if (IsLoading) {
            onChangeFail?.Invoke();
            return;
        }

        IsLoading = true;

        ConstructorInfo? ctor = sceneType.GetConstructor([]);
        if (ctor == null) {
            throw new Exception("Scene does not contain a valid constructor! Scene cannot be changed!");
        }

        Scene? next = ctor?.Invoke([]) as Scene;
        if (next == null) {
            // if scene is null, we know something went wrong...
            IsLoading = false;
            onChangeFail?.Invoke();
            return;
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

            onChangeSuccess?.Invoke(next);

        } catch (Exception ex) {
            // if "next" load fails, unload and remove
            next.Unload();
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

    /// <summary>
    /// Exits the entire game
    /// </summary>
    public static void ExitGame() {
        game.Exit();
    }
}
