using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

/// <summary>
/// A render layer for smooth low-res parallax, handles drawing and auto smooth scrolling
/// </summary>
public class RenderLayer : IResolution {
    private RenderTarget2D renderTarget;
    private RenderTarget2D effectTarget;
    private readonly GraphicsDevice gd;

    /// <summary>
    /// Gets/sets the effect applied to every item within the DrawTo instructions for this render layer
    /// </summary>
    public Effect? IndividualEffect { get; set; }

    /// <summary>
    /// Gets/sets the screen-wide shader effect to apply to this render layer
    /// </summary>
    public Effect? ScreenSpaceEffect { get; set; }

    /// <summary>
    /// Gets/sets the color tint to use when drawing layers to the screen
    /// </summary>
    public Color ColorTint { get; set; } = Color.White;

    /// <summary>
    /// Gets/sets the position offset for drawing smooth parallax for this layer
    /// </summary>
    public Vector2 SmoothingOffset { get; set; }

    /// <summary>
    /// Gets/sets the clear color that the layer is cleared every frame with
    /// </summary>
    public Color ClearColor { get; set; } = Color.Transparent;

    /// <summary>
    /// Creates a new RenderLayer
    /// </summary>
    /// <param name="resolution">Resolution of layer</param>
    /// <param name="gd">GraphicsDevice to create render layer with</param>
    public RenderLayer(Point resolution, GraphicsDevice gd) {
        this.gd = gd;
        renderTarget = new RenderTarget2D(gd, resolution.X, resolution.Y);
        effectTarget = new RenderTarget2D(gd, resolution.X, resolution.Y);
    }

    /// <summary>
    /// Draws to this render layer, automatically configures
    /// and begins/ends the spritebatch draw
    /// </summary>
    /// <param name="drawActions">Action to invoke that contains all internal drawing code</param>
    /// <param name="sb">SpriteBatch to draw with</param>
    /// <param name="transform">Optional transform matrix to pass into SpriteBatch.Begin</param>
    public void DrawTo(Action drawActions, SpriteBatch sb, Matrix? transform = null) {
        if (ScreenSpaceEffect != null) {
            DrawWithSSE(drawActions, sb, transform);
        } else {
            DrawNoSSE(drawActions, sb, transform);
        }
    }

    /// <summary>
    /// Draws to this render layer, automatically configures
    /// and begins/ends the spritebatch draw
    /// </summary>
    /// <param name="drawAction">Void method that passes in a SpriteBatch that contains drawing code</param>
    /// <param name="sb">SpriteBatch to draw with</param>
    /// <param name="transform">Optional transform matrix to pass into SpriteBatch.Begin</param>
    public void DrawTo(Action<SpriteBatch> drawAction, SpriteBatch sb, Matrix? transform = null) {
        DrawTo(
            () => drawAction?.Invoke(sb),
            sb,
            transform
        );
    }

    /// <summary>
    /// Draws this RenderLayer to the screen, does NOT handle Begin/end calls with SpriteBatch
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    /// <param name="canvasScaling">Float scalar value of canvas to resize on screen</param>
    /// <param name="screenTarget">Target rectangle to render on screen, automatically location-adjusted</param>
    public void Draw(SpriteBatch sb, Rectangle screenTarget, float canvasScaling) {
        Rectangle target = GetSmoothParallaxOffset(
            SmoothingOffset,
            screenTarget,
            canvasScaling
        );

        sb.Draw(effectTarget, target, ColorTint);
    }

    /// <summary>
    /// Draws this RenderLayer to the screen, does NOT handle Begin/end calls with SpriteBatch
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    /// <param name="position">Position on screen to draw RenderLayer</param>
    public void Draw(SpriteBatch sb, Vector2 position) {
        sb.Draw(effectTarget, position, ColorTint);
    }

    /// <summary>
    /// Changes the resolution of this RenderLayer
    /// </summary>
    /// <param name="width">New width of layer in pixels</param>
    /// <param name="height">New height of layer in pixels</param>
    /// <param name="canvasExpandSize">Number of pixels to expand bounds for scroll smoothing</param>
    public void ChangeResolution(int width, int height, int canvasExpandSize) {
        renderTarget?.Dispose();
        renderTarget = new RenderTarget2D(gd, width + canvasExpandSize, height + canvasExpandSize);
        effectTarget?.Dispose();
        effectTarget = new RenderTarget2D(gd, width + canvasExpandSize, height + canvasExpandSize);
    }

    private static Rectangle GetSmoothParallaxOffset(Vector2 worldPos, Rectangle toModify, float canvasScaling) {
        // value from 0-1 of current whole-pixel offset
        Vector2 worldOffset = worldPos - Vector2.Floor(worldPos);

        if (worldOffset.X > 1 || worldOffset.X < 0) {
            Debug.WriteLine("wow");
        }

        // apply scaling and floor for the whole pixel offset
        Vector2 modifyOffset = Vector2.Floor(worldOffset * canvasScaling);

        // apply whole pixel offset point to desired rectangle
        toModify.Location -= modifyOffset.ToPoint();

        return toModify;
    }

    // draw without a screenspace effect
    private void DrawNoSSE(Action drawActions, SpriteBatch sb, Matrix? transform) {
        sb.GraphicsDevice.SetRenderTarget(effectTarget);
        sb.GraphicsDevice.Clear(ClearColor);
        sb.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: transform,
            effect: IndividualEffect
        );
        drawActions?.Invoke();
        sb.End();
    }

    // draw with a screenspace effect
    private void DrawWithSSE(Action drawActions, SpriteBatch sb, Matrix? transform) {
        // draw all instructions to render target
        sb.GraphicsDevice.SetRenderTarget(renderTarget);
        sb.GraphicsDevice.Clear(ClearColor);
        sb.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: transform,
            effect: IndividualEffect
        );
        drawActions?.Invoke();
        sb.End();

        // draw render target to effect target using effect in draw call
        sb.GraphicsDevice.SetRenderTarget(effectTarget);
        sb.GraphicsDevice.Clear(Color.Transparent);
        sb.Begin(samplerState: SamplerState.PointClamp, effect: ScreenSpaceEffect);
        sb.Draw(renderTarget, Vector2.Zero, Color.White);
        sb.End();
    }
}
