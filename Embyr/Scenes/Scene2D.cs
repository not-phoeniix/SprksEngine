using Embyr.Rendering;
using Embyr.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Scenes;

public abstract class Scene2D : Scene {
    private Effect fxSolidColor;
    private readonly Quadtree<IActor2D> actors;
    private readonly Quadtree<Light2D> localLights;
    private readonly List<Light2D> globalLights;

    /// <summary>
    /// Gets the 2D camera for this scene
    /// </summary>
    public Camera2D Camera { get; private set; }

    public Scene2D(string name) : base(name) {
        actors = new Quadtree<IActor2D>(new Point(-10_000), new Point(10_000));
        localLights = new Quadtree<Light2D>(new Point(-10_000), new Point(10_000));
        globalLights = new List<Light2D>();
    }

    public override void LoadContent() {
        fxSolidColor = ShaderManager.I.LoadShader("solid_color");
        Camera = new Camera2D(EngineSettings.GameCanvasResolution + new Point(Game.CanvasExpandSize));
        base.LoadContent();
    }

    public override void Unload() {
        actors.Clear();
        localLights.Clear();
        globalLights.Clear();

        fxSolidColor = null;
        Camera = null;
        base.Unload();
    }

    #region // Game loop

    public override void DebugDraw(SpriteBatch sb) {
        base.DebugDraw(sb);
        actors.DebugDraw(sb);
    }

    protected override sealed IEnumerable<IActor> GetUpdatableActors(bool reorganize) {
        foreach (IActor actor in actors.GetData(Camera.Position, EngineSettings.SimulationDistance, reorganize)) {
            yield return actor;
        }
    }

    protected override sealed IEnumerable<IActor> GetDrawableActors() {
        foreach (IActor actor in GetActorsInViewport(Camera.ViewBounds)) {
            yield return actor;
        }
    }

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
        if (actor is IActor2D a) {
            if (actor.Scene != this) {
                throw new Exception("Cannot add actor that has already been added to a prior scene!");
            }

            actors.Insert(a);
            actor.InvokeOnAdded(this);
        }
    }

    /// <inheritdoc/>
    protected override sealed bool RemoveActor(IActor actor) {
        if (actor is IActor2D a && actors.Remove(a)) {
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

    #region // Drawing

    /// <summary>
    /// Gets the current parallax background set to draw this frame
    /// </summary>
    /// <returns></returns>
    public virtual ParallaxBackground? GetCurrentParallax() { return null; }

    /// <summary>
    /// Draws scene as a greyscale depth map
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public virtual void DrawDepthmap(SpriteBatch sb) { }

    /// <summary>
    /// Draws a layer to the depth map
    /// </summary>
    /// <param name="depth">Depth to draw to, from 0 to 1</param>
    /// <param name="drawInstructions">Action of draw instructions to draw for layer</param>
    /// <param name="sb">SpriteBatch to draw with</param>
    protected void DrawDepthLayer(float depth, Action drawInstructions, SpriteBatch sb) {
        fxSolidColor.Parameters["Color"].SetValue(new Vector4(depth, depth, depth, 1.0f));
        sb.Begin(samplerState: SamplerState.PointClamp, effect: fxSolidColor, transformMatrix: Camera.FlooredMatrix);
        drawInstructions?.Invoke();
        sb.End();
    }

    /// <summary>
    /// Draws a layer to the depth map
    /// </summary>
    /// <param name="depth">Depth to draw to, from 0 to 1</param>
    /// <param name="drawInstruction">Action of draw instruction to draw for layer</param>
    /// <param name="sb">SpriteBatch to draw with</param>
    protected void DrawDepthLayer(float depth, Action<SpriteBatch> drawInstruction, SpriteBatch sb) {
        fxSolidColor.Parameters["Color"].SetValue(new Vector4(depth, depth, depth, 1.0f));
        sb.Begin(samplerState: SamplerState.PointClamp, effect: fxSolidColor, transformMatrix: Camera.FlooredMatrix);
        drawInstruction?.Invoke(sb);
        sb.End();
    }

    #endregion

    public override void ChangeResolution(int width, int height, int canvasExpandSize) {
        Camera = new Camera2D(width + canvasExpandSize, height + canvasExpandSize) {
            Position = Camera.Position
        };
    }
}
