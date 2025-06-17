using System;
using Embyr;
using Embyr.Scenes;
using Embyr.Tiles;
using Embyr.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

    private Light2D redLight;
    private Light2D greenLight;
    private Light2D globalLight;
    private Light2D miniLight;

    public override void LoadContent() {
        AmbientColor = Color.Black;
        Gravity = 0;

        Texture2D tileset = ContentHelper.I.Load<Texture2D>("tileset");
        Texture2D tileNormals = ContentHelper.I.Load<Texture2D>("normals");

        TileMap<TileType> map = new(Vector2.Zero, 1000, this);
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
        map.Transform.GlobalZIndex = 5;
        AddActor(map);

        redLight = new Light2D() {
            Color = new Color(1.0f, 0.5f, 0.5f),
            Intensity = 0.7f,
            Radius = 40,
            LinearFalloff = 30,
            CastsShadow = true
        };
        redLight.Transform.GlobalZIndex = 20;
        AddLight(redLight);

        greenLight = new Light2D() {
            Color = new Color(0.6f, 1.0f, 0.7f),
            Intensity = 10f,
            Radius = 100,
            LinearFalloff = 120,
            AngularWidth = MathHelper.ToRadians(80),
            AngularFalloff = MathHelper.ToRadians(45),
        };
        greenLight.Transform.GlobalPosition = new Vector2(70, -70);
        greenLight.Transform.LookAt(Vector2.Zero);
        greenLight.Transform.GlobalZIndex = 4;
        AddLight(greenLight);

        globalLight = new Light2D() {
            IsGlobal = true,
            Color = new Color(1.0f, 0.7f, 1.0f),
            Intensity = 0.8f,
        };
        globalLight.Transform.ZIndex = 20;
        AddLight(globalLight);

        miniLight = new Light2D() {
            Color = new Color(0.5f, 0.9f, 1.0f),
            Intensity = 1.4f,
            Radius = 100,
            LinearFalloff = 40,
            AngularWidth = MathHelper.ToRadians(150),
            AngularFalloff = MathHelper.ToRadians(100),
            CastsShadow = true
        };
        miniLight.Transform.GlobalPosition = new Vector2(50, 34);
        miniLight.Transform.LookAt(Vector2.Zero);
        miniLight.Transform.GlobalZIndex = 8;
        AddLight(miniLight);

        base.LoadContent();
    }

    public override void Update(float dt) {
        redLight.Transform.GlobalPosition = new Vector2(
            -50,
            MathF.Sin(Performance.TotalTime * 2) * 35
        );

        globalLight.Transform.GlobalRotation += dt * 0.5f;

        base.Update(dt);
    }
}
