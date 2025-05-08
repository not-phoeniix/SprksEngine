using Embyr.Scenes;
using Embyr.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

public class RendererForward3D : Renderer3D {
    public RendererForward3D(RendererSettings settings, GraphicsDevice gd, Menu? loadingMenu)
    : base(settings, gd, loadingMenu) {
    }

    public override void RenderScene(Scene inputScene) {
        // don't render non-3D scenes!
        if (inputScene is not Scene3D scene) return;

        GraphicsDevice.SetRenderTarget(MainLayer.RenderTarget);
        GraphicsDevice.Clear(Color.Transparent);

        foreach (IActor3D actor in scene.GetActorsInViewport(scene.Camera.ViewBounds)) {
            actor.Draw(scene.Camera);
        }

        // render all post processing effects !!
        RenderTarget2D prevTarget = MainLayer.RenderTarget;
        for (int i = 0; i < PostProcessingEffects.Count; i++) {
            // just don't do any post processing if it's disabled!
            if (!Settings.EnablePostProcessing) break;

            // grab reference to iteration effect, skip if disabled
            PostProcessingEffect fx = PostProcessingEffects[i];
            if (!fx.Enabled) continue;

            fx.InputRenderTarget = prevTarget;
            fx.Draw(SpriteBatch);

            prevTarget = fx.FinalRenderTarget;
        }

        // draw final post process layer BACK to world layer
        //   (if any post processes were used in the first place)
        if (prevTarget != MainLayer.RenderTarget) {
            MainLayer.ScreenSpaceEffect = null;
            MainLayer.DrawTo(
                () => SpriteBatch.Draw(prevTarget, Vector2.Zero, Color.White),
                SpriteBatch
            );
        }
    }
}
