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

    private Light2D mouseLight;
    private Light2D redLight;
    private Light2D greenLight;

    public override void LoadContent() {
        AmbientColor = Color.Black;
        base.Gravity = 0;
        // EngineSettings.ShowDebugDrawing = true;

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

        mouseLight = new Light2D() {
            Color = Color.White,
            Intensity = 1.0f,
            Radius = 50
        };
        AddLight(mouseLight);

        redLight = new Light2D() {
            Color = new Color(1.0f, 0.5f, 0.5f),
            Intensity = 0.7f,
            Radius = 40,
            LinearFalloff = 30
        };
        AddLight(redLight);

        greenLight = new Light2D() {
            Color = new Color(0.6f, 1.0f, 0.6f),
            Intensity = 1.5f,
            Radius = 60,
            LinearFalloff = 50
        };
        AddLight(greenLight);

        base.LoadContent();
    }

    public override void Update(float dt) {
        mouseLight.Transform.GlobalPosition = Input.MouseWorldPos;

        redLight.Transform.GlobalPosition = new Vector2(
            -50,
            MathF.Sin(Performance.TotalTime * 2) * 40
        );

        greenLight.Transform.GlobalPosition = new Vector2(50, 50) + new Vector2(
            MathF.Cos(Performance.TotalTime) * 50,
            MathF.Sin(Performance.TotalTime) * 50
        );

        base.Update(dt);
    }
}
