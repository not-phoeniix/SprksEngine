using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Embyr.Tools;
using Embyr.UI;
using Embyr.Rendering;

namespace Embyr.Scenes;

/// <summary>
/// A scene in the game, contains data for lights, entities, menus, and other game logic
/// </summary>
public abstract class Scene : IResolution, IDebugDrawable2D {
    private readonly Queue<IActor> actorsToRemove = new();
    private readonly Queue<IActor> actorsToAdd = new();
    private readonly Stack<Menu> menuStack = new();
    private bool menuStackChanged;

    /// <summary>
    /// Gets/sets whether or not scene is paused,
    //  prevents updating with agents but not menus
    /// </summary>
    protected bool Paused { get; set; }

    /// <summary>
    /// Gets the name of this scene
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the gravity strength for this scene
    /// </summary>
    public float Gravity { get; protected set; }

    /// <summary>
    /// Gets/sets the ambient color of this scene for lighting shaders
    /// </summary>
    public Color AmbientColor { get; protected set; }

    /// <summary>
    /// Creates a new unloaded scene
    /// </summary>
    /// <param name="name">Name of scene</param>
    public Scene(string name) {
        if (string.IsNullOrWhiteSpace(name)) {
            throw new Exception("Cannot create a scene with a null / empty / whitespace name!");
        }

        AmbientColor = Color.Black;
        Name = name;
        Gravity = 670;
    }

    /// <summary>
    /// Marks scene as "loaded", should be called last in child override methods
    /// </summary>
    public virtual void LoadContent() {
        Paused = false;
    }

    /// <summary>
    /// Unloads internal scene data, clearing actors and resetting
    /// content helper, should be called last in child override methods
    /// </summary>
    public virtual void Unload() {
        menuStack.Clear();
        ContentHelper.I.LocalReset();
    }

    /// <summary>
    /// Custom actor update logic, runs on every actor in the scene
    /// </summary>
    /// <param name="actor">Actor to update</param>
    /// <param name="fdt">Time passed since last frame</param>
    protected virtual void CustomActorUpdate(IActor actor, float dt) { }

    /// <summary>
    /// Updates the logic for this scene, updating all internal actors
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    public virtual void Update(float dt) {
        // only update current menu if no changes to the menu
        //   stack have occured, prevents double back/pause
        //   actions from triggering
        if (!menuStackChanged) {
            MenuStackPeek()?.Update(dt);
        }

        // deal with actor and scene updating only when not paused
        if (!Paused) {
            // update all actors, reorganize them in the update loop!!
            foreach (IActor actor in GetUpdatableActors(true)) {
                actor.Update(dt);
                CustomActorUpdate(actor, dt);
            }
        }

        // flush the add/remove queues after all actor updating,
        //   happens regardless of pause state
        FlushAddQueue();
        FlushRemovalQueue();

        menuStackChanged = false;
    }

    /// <summary>
    /// Gets an enumerable to iterate across of all actors to update in this scene
    /// </summary>
    /// <param name="reorganize">Whether or not to reorganize containers when updating</param>
    /// <returns>Enumerable of updateable actors</returns>
    protected abstract IEnumerable<IActor> GetUpdatableActors(bool reorganize);

    /// <summary>
    /// Gets an enumerable to iterate across of all actors to draw in this scene
    /// </summary>
    /// <returns>Enumerable of drawable actors</returns>
    protected abstract IEnumerable<IActor> GetDrawableActors();

    /// <summary>
    /// Gets an enumerable to iterate across of all lights to render in a scene
    /// </summary>
    /// <returns>Enumerable of visible lights</returns>
    internal abstract IEnumerable<Light> GetAllLightsToRender();

    /// <summary>
    /// Custom actor update logic for physics updating, runs on every actor in the scene
    /// </summary>
    /// <param name="actor">Actor to update</param>
    /// <param name="fdt">Time passed since last physics update</param>
    protected virtual void CustomActorPhysicsUpdate(IActor actor, float fdt) { }

    /// <summary>
    /// Updates the physics of this scene
    /// </summary>
    /// <param name="dt">Time passed since last fixed update</param>
    public virtual void PhysicsUpdate(float dt) {
        if (!menuStackChanged) {
            MenuStackPeek()?.PhysicsUpdate(dt);
        }

        if (!Paused) {
            foreach (IActor actor in GetUpdatableActors(false)) {
                actor.PhysicsUpdate(dt);
                CustomActorPhysicsUpdate(actor, dt);
            }
        }
    }

    /// <summary>
    /// Draws this scene to the canvas
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public virtual void Draw(SpriteBatch sb) {
        foreach (IActor actor in GetDrawableActors()) {
            if (actor is IDrawable2D drawable) {
                drawable.Draw(sb);
            }
        }
    }

    /// <summary>
    /// Draws all debug information for this scene
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public virtual void DebugDraw(SpriteBatch sb) {
        foreach (IActor actor in GetDrawableActors()) {
            if (actor is IDebugDrawable2D debug) {
                debug.DebugDraw(sb);
            }
        }
    }

    /// <summary>
    /// Draws all overlays/menus to the screen
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public virtual void DrawOverlays(SpriteBatch sb) {
        MenuStackPeek()?.Draw(sb);
    }

    /// <summary>
    /// Draws all debug information for overlays/menus to the screen
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public virtual void DebugDrawOverlays(SpriteBatch sb) {
        MenuStackPeek()?.DebugDraw(sb);
    }

    /// <summary>
    /// Changes the resolution of this scene
    /// </summary>
    /// <param name="width">New resolution width in pixels</param>
    /// <param name="height">New resolution height in pixels</param>
    /// <param name="canvasExpandSize">Number of pixels to expand bounds for scroll smoothing</param>
    public virtual void ChangeResolution(int width, int height, int canvasExpandSize) { }

    /// <summary>
    /// Adds an actor to this scene immediately (MAY CRASH WITH UPDATE LOOP)
    /// </summary>
    /// <param name="actor">Actor to add</param>
    protected abstract void AddActor(IActor actor);

    /// <summary>
    /// Removes an actor from this scene immediately (MAY CRASH WITH UPDATE LOOP)
    /// </summary>
    /// <param name="actor">Actor to remove</param>
    protected abstract bool RemoveActor(IActor actor);

    /// <summary>
    /// Queues an actor to be added at the end of update cycle
    /// </summary>
    /// <param name="actor">Actor to add</param>
    public void QueueAddActor(IActor actor) {
        actorsToAdd.Enqueue(actor);
    }

    /// <summary>
    /// Queues an actor to be removed at end of update cycle
    /// </summary>
    /// <param name="actor">Actor to remove</param>
    public void QueueRemoveActor(IActor actor) {
        actorsToRemove.Enqueue(actor);
    }

    /// <summary>
    /// Flushes and adds all actors within the "ToAdd" actor queue
    /// </summary>
    protected void FlushAddQueue() {
        while (actorsToAdd.Count > 0) {
            IActor actor = actorsToAdd.Dequeue();
            if (actor != null) {
                AddActor(actor);
            }
        }
    }

    /// <summary>
    /// Flushes and removes all actors within the "ToRemove" actor queue
    /// </summary>
    protected void FlushRemovalQueue() {
        while (actorsToRemove.Count > 0) {
            IActor actor = actorsToRemove.Dequeue();
            if (actor != null) {
                RemoveActor(actor);
            }
        }
    }

    /// <summary>
    /// Adds a light to the scene
    /// </summary>
    /// <param name="light">Light to add</param>
    public abstract void AddLight(Light light);

    /// <summary>
    /// Removes a light from this scene
    /// </summary>
    /// <param name="light">Light to remove</param>
    /// <returns>True if successfully removed, false if not</returns>
    public abstract bool RemoveLight(Light light);

    /// <summary>
    /// Pushes a menu onto the menu stack
    /// </summary>
    /// <param name="menu">Menu to push</param>
    public void MenuStackPush(Menu menu) {
        if (menu == null) return;
        menuStackChanged = true;
        menuStack.Push(menu);
    }

    /// <summary>
    /// Pops a menu from the menu stack
    /// </summary>
    /// <returns>Reference to popped menu, null if empty</returns>
    public Menu? MenuStackPop() {
        menuStackChanged = true;
        return menuStack.Count > 0 ? menuStack.Pop() : null;
    }

    /// <summary>
    /// Peeks at the top menu on the menu stack
    /// </summary>
    /// <returns>Reference to current top menu on the stack</returns>
    public Menu? MenuStackPeek() {
        return menuStack.Count > 0 ? menuStack.Peek() : null;
    }

    /// <summary>
    /// Finds the first actor in this scene of a desired type - warning, this may be slow!
    /// </summary>
    /// <typeparam name="T">Type of actor to find</typeparam>
    /// <returns>Reference to actor, null if not found</returns>
    public T? FindActor<T>() where T : class, IActor {
        foreach (IActor actor in GetActors()) {
            if (actor is T found) {
                return found;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the first actor in this scene of a desired type and name (exact match) - warning, this may be slow!
    /// </summary>
    /// <typeparam name="T">Type of actor to find</typeparam>
    /// <param name="name">Name of actor to search, finds exact matches</param>
    /// <param name="caseSensitive">Whether or not to search using case sensitive matches</param>
    /// <returns>Reference to actor, null if not found</returns>
    public T FindActor<T>(string name, bool caseSensitive = true) where T : class, IActor {
        foreach (IActor actor in GetActors()) {
            bool nameMatch = actor.Name.Equals(
                name,
                caseSensitive ?
                    StringComparison.CurrentCulture :
                    StringComparison.CurrentCultureIgnoreCase
            );

            if (actor is T found && nameMatch) {
                return found;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets enumerable of all actors in this scene to iterate across
    /// </summary>
    /// <returns>Enumerable of actors, to be iterated across</returns>
    public abstract IEnumerable<IActor> GetActors();
}
