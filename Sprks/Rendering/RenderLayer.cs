using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sprks.Rendering;

/// <summary>
/// A render layer for smooth low-res parallax, handles drawing and auto smooth scrolling
/// </summary>
internal class RenderLayer : IResolution {
    private RenderTarget2D renderTarget;
    private RenderTarget2D effectTarget;
    private readonly GraphicsDevice gd;
    private readonly SurfaceFormat surfaceFormat;

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
    /// Gets the render target of this render layer
    /// </summary>
    public RenderTarget2D RenderTarget => effectTarget;

    /// <summary>
    /// Creates a new RenderLayer
    /// </summary>
    /// <param name="resolution">Resolution of layer</param>
    /// <param name="gd">GraphicsDevice to create render layer with</param>
    /// <param name="surfaceFormat">Surface format of internal render targets</param>
    /// <param name="useMipMaps">Whether or not to generate mip maps for this layer</param>
    public RenderLayer(Point resolution, GraphicsDevice gd, SurfaceFormat surfaceFormat, bool useMipMaps) {
        this.gd = gd;
        this.surfaceFormat = surfaceFormat;
        renderTarget = new RenderTarget2D(
            gd,
            resolution.X,
            resolution.Y,
            useMipMaps,
            surfaceFormat,
            DepthFormat.None
        );
        effectTarget = new RenderTarget2D(
            gd,
            resolution.X,
            resolution.Y,
            useMipMaps,
            surfaceFormat,
            DepthFormat.None
        );
    }

    /// <summary>
    /// Draws to this render layer, automatically configures
    /// and begins/ends the spritebatch draw
    /// </summary>
    /// <param name="drawAction">Void method that passes in a SpriteBatch that contains drawing code</param>
    /// <param name="sb">SpriteBatch to draw with</param>
    /// <param name="transform">Optional transform matrix to pass into SpriteBatch.Begin</param>
    /// <param name="resetTarget">Whether or not to re-set the render target and clear again</param>
    public void DrawTo(Action<SpriteBatch> drawAction, SpriteBatch sb, Matrix? transform = null, bool resetTarget = true) {
        if (ScreenSpaceEffect != null) {
            DrawWithSSE(drawAction, sb, transform, resetTarget);
        } else {
            DrawNoSSE(drawAction, sb, transform, resetTarget);
        }
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

    /// <inheritdoc/>
    public void ChangeResolution(int width, int height) {
        renderTarget?.Dispose();
        renderTarget = new RenderTarget2D(
            gd,
            width,
            height,
            false,
            surfaceFormat,
            DepthFormat.None
        );
        effectTarget?.Dispose();
        effectTarget = new RenderTarget2D(
            gd,
            width,
            height,
            false,
            surfaceFormat,
            DepthFormat.None
        );
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
    private void DrawNoSSE(Action<SpriteBatch> drawActions, SpriteBatch sb, Matrix? transform, bool resetTarget) {
        if (resetTarget) {
            sb.GraphicsDevice.SetRenderTarget(effectTarget);
            sb.GraphicsDevice.Clear(ClearColor);
        }
        sb.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: transform,
            effect: IndividualEffect
        );
        drawActions?.Invoke(sb);
        sb.End();
    }

    // draw with a screenspace effect
    private void DrawWithSSE(Action<SpriteBatch> drawActions, SpriteBatch sb, Matrix? transform, bool resetTarget) {
        // draw all instructions to render target
        sb.GraphicsDevice.SetRenderTarget(renderTarget);
        if (resetTarget) {
            sb.GraphicsDevice.Clear(ClearColor);
        } else {
            sb.GraphicsDevice.Clear(Color.Transparent);
        }
        sb.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: transform,
            effect: IndividualEffect
        );
        drawActions?.Invoke(sb);
        sb.End();

        // draw render target to effect target using effect in draw call
        sb.GraphicsDevice.SetRenderTarget(effectTarget);
        if (resetTarget) {
            sb.GraphicsDevice.Clear(Color.Transparent);
        }
        sb.Begin(samplerState: SamplerState.PointClamp, effect: ScreenSpaceEffect);
        sb.Draw(renderTarget, Vector2.Zero, Color.White);
        sb.End();
    }
}
