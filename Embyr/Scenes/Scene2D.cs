using Embyr.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Scenes;

/// <summary>
/// An abstract 2D game scene, must be inherited to create new scenes in an Embyr game. Inherits from <c>Scene</c>
/// </summary>
public abstract class Scene2D : Scene {
    private readonly Quadtree<Actor2D> actors;
    private readonly List<Actor2D> actorsNoCulling;
    private readonly List<Actor2D>[] actorsToDraw;
    private readonly Quadtree<Light2D> localLights;
    private readonly List<Light2D> globalLights;

    /// <summary>
    /// Gets the 2D camera for this scene
    /// </summary>
    public Camera2D Camera { get; private set; }

    /// <summary>
    /// Creates a new Scene2D instance
    /// </summary>
    public Scene2D() {
        actors = new Quadtree<Actor2D>(new Point(-10_000), new Point(10_000));
        actorsNoCulling = new List<Actor2D>();
        localLights = new Quadtree<Light2D>(new Point(-10_000), new Point(10_000));
        globalLights = new List<Light2D>();
        Camera = new Camera2D(EngineSettings.GameCanvasResolution + new Point(Game.CanvasExpandSize));

        actorsToDraw = new List<Actor2D>[Transform2D.MaxZIndex + 1];
        for (int i = 0; i < actorsToDraw.Length; i++) {
            actorsToDraw[i] = new List<Actor2D>();
        }
    }

    /// <inheritdoc/>
    public override void LoadContent() {
        base.LoadContent();
    }

    /// <inheritdoc/>
    public override void Unload() {
        actors.Clear();
        actorsNoCulling.Clear();
        localLights.Clear();
        globalLights.Clear();
        base.Unload();
    }

    #region // Game loop

    /// <inheritdoc/>
    protected override sealed IEnumerable<IActor> GetUpdatableActors(bool reorganize) {
        foreach (IActor actor in actors.GetData(Camera.Position, EngineSettings.SimulationDistance, reorganize)) {
            yield return actor;
        }
    }

    /// <inheritdoc/>
    public override sealed IEnumerable<IActor> GetDrawableActors() {
        // get all visible actors, toss them in the correct index
        foreach (Actor2D actor in GetActorsInViewport(Camera.ViewBounds)) {
            if (!actor.PreventCulling) {
                actorsToDraw[actor.Transform.GlobalZIndex].Add(actor);
            }
        }

        // get all actors without culling and queue em to draw
        for (int i = actorsNoCulling.Count - 1; i >= 0; i--) {
            Actor2D a = actorsNoCulling[i];

            if (a.PreventCulling) {
                actorsToDraw[a.Transform.GlobalZIndex].Add(a);
            } else {
                // remove actors that shouldn't be in this list lol,
                //   iterate backwards so we don't have to worry about
                //   shifting i due to the decreased index
                actorsNoCulling.RemoveAt(i);
            }
        }

        // return the sorted actors in the correct order
        foreach (List<Actor2D> list in actorsToDraw) {
            foreach (Actor2D a in list) {
                yield return a;
            }

            // clear at the end of each return so lists are
            //   ready for next frame
            list.Clear();
        }
    }

    /// <inheritdoc/>
    internal override sealed IEnumerable<Light2D> GetAllLightsToRender() {
        foreach (Light2D light in globalLights) {
            yield return light;
        }

        foreach (Light2D light in localLights.GetData(Utils.ExpandRect(Camera.ViewBounds, 100), true)) {
            yield return light;
        }
    }

    #endregion

    #region // Actor management

    /// <inheritdoc/>
    protected override sealed void AddActor(IActor actor) {
        if (actor is Actor2D a) {
            if (actor.Scene != this) {
                throw new Exception("Cannot add actor that has already been added to a different scene!");
            }

            actors.Insert(a);
            if (a.PreventCulling) {
                actorsNoCulling.Add(a);
            }

            actor.InvokeOnAdded(this);
        }
    }

    /// <inheritdoc/>
    protected override sealed bool RemoveActor(IActor actor) {
        if (actor is Actor2D a) {
            if (a.PreventCulling) {
                actorsNoCulling.Remove(a);
            }

            if (actors.Remove(a)) {
                actor?.InvokeOnRemoved(this);
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public override sealed IEnumerable<IActor> GetActors() {
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

    /// <inheritdoc/>
    public override sealed void AddLight(Light light) {
        if (light is not Light2D l) return;

        if (light.IsGlobal) {
            globalLights.Add(l);
        } else {
            localLights.Insert(l);
        }
    }

    /// <inheritdoc/>
    public override sealed bool RemoveLight(Light light) {
        if (light is not Light2D l) return false;

        if (light.IsGlobal) {
            return globalLights.Remove(l);
        } else {
            return localLights.Remove(l);
        }
    }

    #endregion

    /// <inheritdoc/>
    public override void ChangeResolution(int width, int height) {
        Camera = new Camera2D(width, height) {
            Position = Camera.Position,
            Zoom = Camera.Zoom,
            Rotation = Camera.Rotation
        };
    }
}
