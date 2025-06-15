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
        AmbientColor = Color.Black;

        Texture2D tileTexture = ContentHelper.I.Load<Texture2D>("tileset");
        tilemap = new TileMap<TileType>("tile map", Vector2.Zero, 1000, this);

        int width = layout.GetLength(1);
        int height = layout.GetLength(0);

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (layout[y, x] == 1) {
                    Point pos = new(
                        x - width / 2,
                        y - height / 2
                    );

                    Tile tile = new(TileType.Platform, tileTexture, this);
                    tilemap.AddTile(tile, pos);
                } else if (layout[y, x] == 2) {
                    AddLight(new Light2D() {
                        Transform = new Transform2D() {
                            Parent = tilemap.Transform,
                            Position = new Vector2(
                                (x - width / 2) * Tile.PixelSize,
                                (y - height / 2) * Tile.PixelSize
                            ),
                            GlobalZIndex = 3
                        },
                        Color = new Color(1.0f, 0.7f, 0.7f),
                        Radius = 20,
                        Intensity = 0.7f,
                        LinearFalloff = 20
                    });
                }
            }
        }

        AddActor(tilemap);

        AddLight(new Light2D() {
            IsGlobal = true,
            Color = new Color(1.0f, 0.8f, 1.0f),
            Intensity = 1.0f,
            Transform = new Transform2D() {
                GlobalZIndex = 50
            }
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
}
