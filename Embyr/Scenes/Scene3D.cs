using Microsoft.Xna.Framework;

namespace Embyr.Scenes;

public abstract class Scene3D : Scene {
    private readonly Octree<IActor3D> actors;
    private readonly Octree<Light3D> localLights;
    private readonly List<Light3D> globalLights;

    public Camera3D Camera { get; private set; }

    public Scene3D(string name) : base(name) {
        actors = new Octree<IActor3D>(Vector3.One * -10_000, Vector3.One * 10_000);
        localLights = new Octree<Light3D>(Vector3.One * -10_000, Vector3.One * 10_000);
        globalLights = new List<Light3D>();
        Camera = new Camera3D(new Vector3(-10, 10, -10), 0.01f, 1000.0f);
    }

    /// <inheritdoc/>
    public override void LoadContent() {
        Camera.Transform.GlobalPosition = new Vector3(-10, 10, -10);
        Camera.LookAt(Vector3.Zero);
        base.LoadContent();
    }

    /// <inheritdoc/>
    public override void Unload() {
        base.Unload();
    }

    #region // Game loop

    /// <inheritdoc/>
    public override void Update(float dt) {
        Point res = EngineSettings.GameCanvasResolution;
        float aspect = (float)res.X / res.Y;
        Camera.Update(aspect);

        base.Update(dt);
    }

    /// <inheritdoc/>
    protected override sealed IEnumerable<IActor> GetUpdatableActors(bool reorganize) {
        foreach (IActor actor in actors.GetData(Camera.Transform.GlobalPosition, EngineSettings.SimulationDistance, reorganize)) {
            yield return actor;
        }
    }

    /// <inheritdoc/>
    public override sealed IEnumerable<IActor> GetDrawableActors() {
        return GetActorsInViewport(Camera.ViewBounds);
    }

    /// <inheritdoc/>
    internal override sealed IEnumerable<Light3D> GetAllLightsToRender() {
        foreach (Light3D light in globalLights) {
            yield return light;
        }

        foreach (Light3D light in localLights.GetData(Camera.Transform.GlobalPosition, Camera.FarPlaneDist, true)) {
            yield return light;
        }
    }

    #endregion

    #region // Actor management

    /// <inheritdoc/>
    protected override sealed void AddActor(IActor actor) {
        if (actor is IActor3D a) {
            if (actor.Scene != this) {
                throw new Exception("Cannot add actor that has already been added to a prior scene!");
            }

            actors.Insert(a);
            actor.InvokeOnAdded(this);
        }
    }

    /// <inheritdoc/>
    protected override sealed bool RemoveActor(IActor actor) {
        if (actor is IActor3D a && actors.Remove(a)) {
            actor?.InvokeOnRemoved(this);
            return true;
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
    public IEnumerable<IActor> GetActorsInRadius(Vector3 center, float radius) {
        return actors.GetData(center, radius, false);
    }

    /// <summary>
    /// Gets enumerable of all actors within a given viewport in this scene to iterate across
    /// </summary>
    /// <returns>Enumerable of actors within a given viewport, to be iterated across</returns>
    public IEnumerable<IActor> GetActorsInViewport(BoundingFrustum viewport) {
        return actors.GetData(viewport, false);
    }

    /// <inheritdoc/>
    public override sealed void AddLight(Light light) {
        if (light is not Light3D l) return;

        if (light.IsGlobal) {
            globalLights.Add(l);
        } else {
            localLights.Insert(l);
        }
    }

    /// <inheritdoc/>
    public override sealed bool RemoveLight(Light light) {
        if (light is not Light3D l) return false;

        if (light.IsGlobal) {
            return globalLights.Remove(l);
        } else {
            return localLights.Remove(l);
        }
    }

    #endregion
}
