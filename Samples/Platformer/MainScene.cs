using Embyr;
using Embyr.Scenes;
using Embyr.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite.Tilemaps;

namespace Platformer;

public class MainScene(string name) : Scene2D(name) {
    private static readonly int[,] layout = {
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 1, 1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
    };

    private TileMap<TileType> tilemap;
    private Player player;

    public override void LoadContent() {
        Gravity = 400;

        Texture2D tileTexture = ContentHelper.I.Load<Texture2D>("tileset");
        tilemap = new TileMap<TileType>("tile map", Vector2.Zero, 1000, this);
        for (int y = 0; y < layout.GetLength(0); y++) {
            for (int x = 0; x < layout.GetLength(1); x++) {
                if (layout[y, x] == 1) {
                    Point pos = new(
                        x - layout.GetLength(1) / 2,
                        y - layout.GetLength(0) / 2
                    );

                    Tile tile = new(TileType.Platform, tileTexture, this);
                    tilemap.AddTile(tile, pos);
                } else if (layout[y, x] == 2) {
                    AddLight(new Light2D() {
                        Transform = new Transform2D() {
                            Parent = tilemap.Transform,
                            Position = new Vector2(x * Tile.PixelSize, y * Tile.PixelSize)
                        },
                        Color = Color.Red,
                        Radius = 40
                    });
                }
            }
        }

        AddActor(tilemap);

        AddLight(new Light2D() {
            IsGlobal = true,
            Color = new Color(0.8f, 0.6f, 0.8f),
            Intensity = 0.5f
        });

        player = new Player(new Vector2(0, 0), this);
        AddActor(player);

        base.LoadContent();
    }

    public override void Update(float dt) {
        // Camera.SmoothFollow(player, 5, dt);

        if (player.Transform.GlobalPosition.Y > 100) {
            player.Transform.GlobalPosition = Vector2.Zero;
            player.Physics.Velocity = Vector2.Zero;
        }

        base.Update(dt);
    }

    public override void DrawDepthmap(SpriteBatch sb) {
        // TODO: make depthmap drawing automatic!
        //   implement a z index for every transform ?
        //   that could be nice !

        DrawDepthLayer(0.25f, tilemap.Draw, sb);
    }
}
