using Microsoft.Xna.Framework;
using MonoGame.Aseprite;

namespace Embyr.Scenes;

/// <summary>
/// A collection of ParallaxLayer's, combined to make one full background
/// </summary>
public class ParallaxBackground {
    private readonly Dictionary<GameLayer, ParallaxLayer> layers;

    /// <summary>
    /// Gets/sets the offset of all layers in this background
    /// </summary>
    public Vector2 Offset { get; set; }

    /// <summary>
    /// Creates a new ParallaxBackground
    /// </summary>
    /// <param name="file">File to grab frames from for layers</param>
    public ParallaxBackground(AsepriteFile file) {
        // initialize layers
        layers = new Dictionary<GameLayer, ParallaxLayer>();
        AddLayer(file, 1, GameLayer.ParallaxBg, true, true);
        AddLayer(file, 2, GameLayer.ParallaxFar, true, false);
        AddLayer(file, 3, GameLayer.ParallaxMid, true, false);
        AddLayer(file, 4, GameLayer.ParallaxNear, true, false);

        // initialize offset to be same as the layers
        Offset = new Vector2(0, -file.CanvasHeight / 2);
    }

    /// <summary>
    /// Creates a new empty ParallaxBackground
    /// </summary>
    public ParallaxBackground() {
        layers = new Dictionary<GameLayer, ParallaxLayer>();
    }

    /// <summary>
    /// Adds a layer to this parallax background
    /// </summary>
    /// <param name="file">File to add layer from</param>
    /// <param name="frameIndex">Frame index in file to make layer from</param>
    /// <param name="gameLayer">Game layer on which to render the layer</param>
    /// <param name="hRepeat">Whether or not background repeats horizontally</param>
    /// <param name="vRepeat">Whether or not background repeats vertically</param>
    public void AddLayer(AsepriteFile file, int frameIndex, GameLayer gameLayer, bool hRepeat = true, bool vRepeat = false) {
        //! NOTE: the speed of the frame correlates to
        //!   the speed of the layer (in a weird and fucked up way)
        //!
        //!   100ms -> 0.1f, 900ms -> 0.9f, etc

        float speed = file.Frames[frameIndex].DurationInMilliseconds / 1000f;
        layers[gameLayer] = new ParallaxLayer(
            file,
            frameIndex,
            speed,
            new Vector2(0, -file.CanvasHeight / 2),
            hRepeat,
            vRepeat
        );
    }

    /// <summary>
    /// Adds a layer to this parallax background
    /// </summary>
    /// <param name="layer">Pre-created layer to add to background</param>
    /// <param name="gameLayer">Game layer on which to render the layer</param>
    public void AddLayer(ParallaxLayer layer, GameLayer gameLayer) {
        layers[gameLayer] = layer;
    }

    /// <summary>
    /// Gets a parallax layer from this background
    /// </summary>
    /// <param name="gameLayer">GameLayer to retrieve from, only works for parallax varieties</param>
    /// <returns>Reference to stored ParallaxLayer from background</returns>
    public ParallaxLayer GetLayer(GameLayer gameLayer) {
        if (layers.TryGetValue(gameLayer, out ParallaxLayer layer)) {
            return layer;
        }

        return null;
    }
}
