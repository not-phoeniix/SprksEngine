using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Embyr.Tools;
using System;
using Embyr.UI;
using Embyr.Shaders;

namespace Embyr.Scenes;

/// <summary>
/// A scene in the game, contains data for lights, entities, menus, and other game logic
/// </summary>
public abstract class Scene : IResolution, IDebugDrawable {
    private readonly Queue<IActor> actorsToRemove = new();
    private readonly Queue<IActor> actorsToAdd = new();
    private readonly Quadtree<IActor> actors;
    private readonly Stack<Menu> menuStack = new();
    private bool menuStackChanged;
    private float volumetricScalar;

    // lighting fields!!
    private const int MaxLightsPerPass = 8;
    private Effect fxLightRender;
    private readonly List<Light> globalLights;
    private readonly Quadtree<Light> localLights;
    private readonly Vector3[] lightPositions = new Vector3[MaxLightsPerPass];
    private readonly Vector3[] lightColors = new Vector3[MaxLightsPerPass];
    private readonly float[] lightIntensities = new float[MaxLightsPerPass];
    private readonly Vector4[] lightSizeParams = new Vector4[MaxLightsPerPass];
    private readonly float[] lightRotations = new float[MaxLightsPerPass];

    /// <summary>
    /// Gets the camera for this scene
    /// </summary>
    public Camera Camera { get; private set; }

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
    /// Gets/sets a volumetric scalar value in this scene, used as lighting fog, clamped between 0 and 1
    /// </summary>
    public float VolumetricScalar {
        get => volumetricScalar;
        set => volumetricScalar = Math.Clamp(value, 0, 1);
    }

    /// <summary>
    /// Gets/sets the ambient color of this scene for lighting shaders
    /// </summary>
    public Color AmbientColor { get; protected set; }

    /// <summary>
    /// Gets the opaque summed color of all global lights in this scene, used to tint background parallax
    /// </summary>
    public Color GlobalLightTint { get; private set; }

    /// <summary>
    /// Gets the gravity strength for this scene
    /// </summary>
    public float Gravity { get; protected set; }

    /// <summary>
    /// Creates a new unloaded scene
    /// </summary>
    /// <param name="name">Name of scene</param>
    public Scene(string name) {
        if (string.IsNullOrWhiteSpace(name)) {
            throw new Exception("Cannot create a scene with a null / empty / whitespace name!");
        }

        AmbientColor = Color.Black;
        actors = new Quadtree<IActor>(new Point(-10_000), new Point(10_000));
        localLights = new Quadtree<Light>(new Point(-10_000), new Point(10_000));
        globalLights = new List<Light>();
        Name = name;
        Gravity = 670;
    }

    /// <summary>
    /// Marks scene as "loaded", should be called last in child override methods
    /// </summary>
    public virtual void LoadContent() {
        Camera = new Camera(EngineSettings.GameCanvasResolution + new Point(Game.CanvasExpandSize));
        fxLightRender = ShaderManager.I.LoadShader("light_render", ShaderManager.ShaderProfile.OpenGL);
        Paused = false;
    }

    /// <summary>
    /// Unloads internal scene data, clearing actors and resetting
    /// content helper, should be called last in child override methods
    /// </summary>
    public virtual void Unload() {
        actors.Clear();
        localLights.Clear();
        globalLights.Clear();
        menuStack.Clear();
        Camera = null;
        fxLightRender = null;
        ContentHelper.I.LocalReset();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="dt"></param>
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
            foreach (IActor actor in actors.GetData(Camera.Position, EngineSettings.SimulationDistance, true)) {
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
    /// Custom actor update logic for physics updating, runs on every actor in the scene
    /// </summary>
    /// <param name="actor">Actor to update</param>
    /// <param name="fdt">Time passed since last fixed update</param>
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
            foreach (IActor actor in actors.GetData(Camera.Position, EngineSettings.SimulationDistance, false)) {
                if (actor is IAgent agent) {
                    agent.Physics.ApplyForce(agent.UpdateBehavior(dt));
                }

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
        foreach (IActor actor in GetActorsInViewport(Camera.ViewBounds)) {
            if (ShouldBeRendered(actor)) {
                actor.Draw(sb);
            }
        }
    }

    /// <summary>
    /// Draws all debug information for this scene
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public virtual void DebugDraw(SpriteBatch sb) {
        foreach (IActor actor in GetActorsInViewport(Camera.ViewBounds)) {
            if (actor is IDebugDrawable debug) {
                debug.DebugDraw(sb);
            }
        }

        actors.DebugDraw(sb);
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
    /// Gets the current parallax background set to draw this frame
    /// </summary>
    /// <returns></returns>
    public abstract ParallaxBackground GetCurrentParallax();

    /// <summary>
    /// Draws scene as a greyscale depth map
    /// </summary>
    /// <param name="fxSolidColor">Solid color effect, useful for drawing colors for depths</param>
    /// <param name="depthBuffer">Depth buffer to draw heightmap to</param>
    /// <param name="sb">SpriteBatch to draw with</param>
    public virtual void DrawDepthmap(Effect fxSolidColor, RenderTarget2D depthBuffer, SpriteBatch sb) { }

    /// <summary>
    /// Draws all lights in the scene to a render target
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    /// <param name="lightBuffer">Light render buffer to draw to</param>
    /// <param name="tileDistanceField">Distance field for tile layer in rendered world</param>
    /// <param name="skyDistanceField">Distance field for sky layer in rendered world</param>
    public void DrawLightsDeferred(SpriteBatch sb, RenderTarget2D lightBuffer, RenderTarget2D tileDistanceField, RenderTarget2D skyDistanceField) {
        sb.GraphicsDevice.SetRenderTarget(lightBuffer);
        sb.GraphicsDevice.Clear(Color.Black);

        Vector3 globalSum = Vector3.Zero;
        int i = 0;

        void SaveLightInArr(Light light) {
            // set array values
            Vector2 lightScreenPos = Vector2.Transform(light.Transform.GlobalPosition, Camera.FlooredMatrix);
            lightScreenPos /= new Vector2(lightBuffer.Width, lightBuffer.Height);
            lightPositions[i] = new Vector3(
                lightScreenPos,
                light.IsGlobal ? 1 : 0
            );
            lightColors[i] = light.Color.ToVector3();
            lightIntensities[i] = light.Intensity;
            lightRotations[i] = light.Rotation;
            lightSizeParams[i] = new Vector4(
                light.Radius,
                light.AngularWidth,
                light.LinearFalloff,
                light.AngularFalloff
            );

            i++;
        }

        void Draw() {
            // ~~~ PASS DATA ~~~

            // "i" at this point should equal the total light
            //   count since it was incremented before we got here
            fxLightRender.Parameters["NumLights"].SetValue(i);
            fxLightRender.Parameters["ScreenRes"].SetValue(new Vector2(lightBuffer.Width, lightBuffer.Height));
            fxLightRender.Parameters["Positions"].SetValue(lightPositions);
            fxLightRender.Parameters["Colors"].SetValue(lightColors);
            fxLightRender.Parameters["Intensities"].SetValue(lightIntensities);
            fxLightRender.Parameters["Rotations"].SetValue(lightRotations);
            fxLightRender.Parameters["SizeParams"].SetValue(lightSizeParams);
            fxLightRender.Parameters["SkyDistanceField"].SetValue(skyDistanceField);

            // ~~~ DRAW TO BUFFER ~~~
            sb.Begin(samplerState: SamplerState.PointClamp, effect: fxLightRender);
            // lights draw on top of distance field, easier
            //   than passing in a texture via parameters
            sb.Draw(tileDistanceField, new Rectangle(0, 0, lightBuffer.Width, lightBuffer.Height), Color.White);
            sb.End();
        }

        foreach (Light light in globalLights) {
            if (light.Enabled) {
                SaveLightInArr(light);
                globalSum += light.Color.ToVector3() * light.Intensity;
            }

            // if max lights has been reached (or end of lights
            //   list has been reached), draw to the deferred buffer!
            if (i > 0 && i % MaxLightsPerPass == 0) {
                Draw();
                i = 0;
            }
        }

        foreach (Light light in localLights.GetData(Utils.ExpandRect(Camera.ViewBounds, 100), true)) {
            // grab reference to light and save values in temporary arrays!
            // bool inView = Utils.ExpandRect(Camera.ViewBounds, (int)light.Radius * 2).Contains(light.Position);
            if (light.Enabled) {
                SaveLightInArr(light);
            }

            // if max lights has been reached (or end of lights
            //   list has been reached), draw to the deferred buffer!
            if (i > 0 && i % MaxLightsPerPass == 0) {
                Draw();
                i = 0;
            }
        }

        // draw one final time to make sure everything is
        //   rendered only if i has not yet been reset
        if (i != 0) {
            Draw();
        }

        GlobalLightTint = new Color(globalSum);
    }

    /// <summary>
    /// Changes the resolution of this scene
    /// </summary>
    /// <param name="width">New resolution width in pixels</param>
    /// <param name="height">New resolution height in pixels</param>
    /// <param name="canvasExpandSize">Number of pixels to expand bounds for scroll smoothing</param>
    public virtual void ChangeResolution(int width, int height, int canvasExpandSize) {
        Camera = new Camera(width + canvasExpandSize, height + canvasExpandSize) {
            Position = Camera.Position
        };
    }

    /// <summary>
    /// Adds an actor to this scene immediately (MAY CRASH WITH UPDATE LOOP)
    /// </summary>
    /// <param name="actor">Actor to add</param>
    protected void AddActor(IActor actor) {
        if (actor != null) {
            if (actor.Scene != this) {
                throw new Exception("Cannot add actor that has already been added to a prior scene!");
            }

            actors.Insert(actor);
            actor.InvokeOnAdded(this);
        }
    }

    /// <summary>
    /// Removes an actor to this scene immediately (MAY CRASH WITH UPDATE LOOP)
    /// </summary>
    /// <param name="actor">Actor to remove</param>
    protected bool RemoveActor(IActor actor) {
        if (actors.Remove(actor)) {
            actor?.InvokeOnRemoved(this);
            return true;
        }

        return false;
    }

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
    public void AddLight(Light light) {
        if (light.IsGlobal) {
            globalLights.Add(light);
        } else {
            localLights.Insert(light);
        }
    }

    /// <summary>
    /// Removes a light from this scene
    /// </summary>
    /// <param name="light">Light to remove</param>
    /// <returns>True if successfully removed, false if not</returns>
    public bool RemoveLight(Light light) {
        if (light.IsGlobal) {
            return globalLights.Remove(light);
        } else {
            return localLights.Remove(light);
        }
    }

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
    public T FindActor<T>() where T : class, IActor {
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
    public IEnumerable<IActor> GetActors() {
        return actors.GetData(false);
    }

    /// <summary>
    /// Gets enumerable of all actors within a given radius in this scene to iterate across
    /// </summary>
    /// <returns>Enumerable of actors within a given radius, to be iterated across</returns>
    public IEnumerable<IActor> GetActorsInRadius(Vector2 center, float radius) {
        return actors.GetData(center, radius, false);
    }

    /// <summary>
    /// Gets enumerable of all actors within a given viewport in this scene to iterate across
    /// </summary>
    /// <returns>Enumerable of actors within a given viewport, to be iterated across</returns>
    public IEnumerable<IActor> GetActorsInViewport(Rectangle viewport) {
        return actors.GetData(viewport, false);
    }

    /// <summary>
    /// Calculates whether or not an actor should be rendered to the screen
    /// </summary>
    /// <param name="actor">Actor to check if should be rendered</param>
    /// <returns>True if actor should be rendered, false if otherwise</returns>
    public bool ShouldBeRendered(IActor actor) {
        return actor.Bounds.Intersects(Camera.ViewBounds);
    }
}
