using System;
using Embyr;
using Embyr.Scenes;
using Embyr.Tiles;
using Embyr.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LightingTest2D;

public enum TileType {
    Tile,
}

public class MainScene(string name) : Scene2D(name) {
    private static readonly int[,] types = {
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1 },
        { 1, 0, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 1 },
        { 1, 0, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 1 },
        { 1, 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
    };

    private Light2D light;

    public override void LoadContent() {
        AmbientColor = Color.Black;
        base.Gravity = 0;

        Texture2D tileset = ContentHelper.I.Load<Texture2D>("tileset");
        Texture2D tileNormals = ContentHelper.I.Load<Texture2D>("normals");

        TileMap<TileType> map = new("map", Vector2.Zero, 1000, this);
        for (int x = 0; x < types.GetLength(1); x++) {
            for (int y = 0; y < types.GetLength(0); y++) {
                if (types[y, x] == 1) {
                    Tile t = new(TileType.Tile, tileset, tileNormals, this);
                    Point pos = new(
                        x - types.GetLength(1) / 2,
                        y - types.GetLength(0) / 2
                    );

                    map.AddTile(t, pos);
                }
            }
        }
        AddActor(map);

        light = new Light2D() {
            Color = Color.White,
            Intensity = 1.0f,
            Radius = 50
        };
        AddLight(light);

        base.LoadContent();
    }

    public override void Update(float dt) {
        light.Transform.GlobalPosition = Input.MouseWorldPos;

        base.Update(dt);
    }
}
