using Embyr;
using Embyr.Scenes;
using Embyr.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer;

public class MainScene(string name) : Scene2D(name) {
    // private static readonly int[,] layout = {
    //     { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
    //     { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
    //     { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
    //     { 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
    //     { 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
    //     { 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1 },
    //     { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
    //     { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
    //     { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
    //     { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 1 },
    //     { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 1 },
    //     { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
    //     { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
    //     { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
    // };

    private static readonly int[,] layout = {
        { 1, 0, 1, 1 },
        { 1, 1, 0, 1 },
        { 1, 0, 1, 1 },
    };

    private TileMap<TileType> tilemap;
    private Player player;

    public override void LoadContent() {
        EngineSettings.ShowDebugDrawing = true;
        Gravity = 0;

        Texture2D tileTexture = ContentHelper.I.Load<Texture2D>("tileset");
        tilemap = new(Vector2.Zero, 1000, "TileMap", this);
        for (int x = 0; x < layout.GetLength(1); x++) {
            for (int y = 0; y < layout.GetLength(0); y++) {
                if (layout[y, x] == 1) {
                    Point pos = new(
                        x - layout.GetLength(1) / 2,
                        y - layout.GetLength(0) / 2
                    );

                    Tile tile = new(TileType.Platform, tileTexture, this);
                    tilemap.AddTile(tile, pos);
                }
            }
        }

        AddActor(tilemap);

        AddLight(new Light2D() {
            IsGlobal = true,
            Color = new Color(0.8f, 0.6f, 0.8f),
            Intensity = 0.5f
        });

        player = new Player(new Vector2(-40, 0), this);
        AddActor(player);

        base.LoadContent();
    }

    public override void Update(float dt) {
        Camera.SmoothFollow(player, 5, dt);

        base.Update(dt);
    }

    public override void DrawDepthmap(SpriteBatch sb) {
        // TODO: make depthmap drawing automatic!
        //   implement a z index for every transform ?
        //   that could be nice !

        DrawDepthLayer(0.25f, tilemap.Draw, sb);
    }
}
