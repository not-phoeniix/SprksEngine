using System;
using Embyr;
using Embyr.Scenes;
using Embyr.UI;
using Embyr.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

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
    private Font font;

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
            Intensity = 6f,
            Radius = 100,
            LinearFalloff = 120,
            OuterAngle = MathHelper.ToRadians(80),
            InnerAngle = MathHelper.ToRadians(0),
        };
        greenLight.Transform.GlobalPosition = new Vector2(70, -70);
        greenLight.Transform.LookAt(Vector2.Zero);
        greenLight.Transform.GlobalZIndex = 4;
        AddLight(greenLight);

        globalLight = new Light2D() {
            IsGlobal = true,
            Color = new Color(1.0f, 0.7f, 1.0f),
            Intensity = 0.2f,
        };
        globalLight.Transform.ZIndex = 20;
        AddLight(globalLight);

        miniLight = new Light2D() {
            Color = new Color(0.5f, 0.9f, 1.0f),
            Intensity = 1.4f,
            Radius = 100,
            LinearFalloff = 40,
            OuterAngle = MathHelper.ToRadians(150),
            InnerAngle = MathHelper.ToRadians(100),
            CastsShadow = true
        };
        miniLight.Transform.GlobalPosition = new Vector2(50, 34);
        miniLight.Transform.LookAt(Vector2.Zero);
        miniLight.Transform.GlobalZIndex = 8;
        AddLight(miniLight);

        Texture2D parallaxTexture = ContentHelper.I.Load<Texture2D>("parallax");
        ParallaxLayer2D parallax = new(parallaxTexture, new Vector2(0.3f), this);
        parallax.Transform.ZIndex = 0;
        parallax.RepeatSize = new Point(4, 4);
        AddActor(parallax);

        font = new Font(
            ContentHelper.I.Load<Texture2D>("futuristic-charmap"),
            5,
            7,
            2,
            1,
            1,
            1
        );

        base.LoadContent();
    }

    public override void Update(float dt) {
        redLight.Transform.GlobalPosition = new Vector2(
            -50,
            MathF.Sin(Performance.TotalTime * 2) * 35
        );

        Vector2 input = Input.GetComposite2D("left", "up", "right", "down");
        Camera.Position += input * 100 * dt;

        globalLight.Transform.GlobalRotation += dt * 0.5f;

        base.Update(dt);
    }

    public override void BuildUI() {
        UIBuilder.BeginElement(new ElementProperties() {
            Style = new ElementStyle() {
                BackgroundColor = Color.Black
            },
            XSizing = ElementSizing.Fixed(200),
            YSizing = ElementSizing.Fit(),
            Gap = 4,
            Direction = AlignDirection.LeftToRight,
            Padding = new ElementPadding(4)
        });
        {
            for (int i = 0; i < 10; i++) {
                Color gradientColor = new(
                    i / 10.0f * 0.5f + 0.5f,
                    1.0f,
                    1.0f
                );

                UIBuilder.BeginClickable(
                    new ElementProperties() {
                        Style = new ElementStyle() {
                            BackgroundColor = gradientColor
                        },
                        // XSizing = ElementSizing.Fixed(10),
                        XSizing = ElementSizing.Grow(),
                        // YSizing = ElementSizing.Fixed(10),
                        YSizing = ElementSizing.Grow()
                    },
                    static () => Debug.WriteLine("waow....")
                );
                UIBuilder.TextElement(ElementProperties.BlankText(font, Color.Black), ":D");
                UIBuilder.End();
            }

            UIBuilder.BeginElement(new ElementProperties() {
                Direction = AlignDirection.TopToBottom,
                Style = new ElementStyle() {
                    BackgroundColor = Color.Gray
                },
                Gap = 1
            });
            {
                for (int y = 0; y < 3; y++) {
                    UIBuilder.BeginElement(new ElementProperties() {
                        Direction = AlignDirection.LeftToRight,
                        Style = new ElementStyle() {
                            BackgroundColor = Color.Transparent
                        },
                        Gap = 1
                    });
                    for (int x = 0; x < 3; x++) {
                        int x2 = x;
                        int y2 = y;

                        UIBuilder.Clickable(
                            new ElementProperties() {
                                Style = new ElementStyle() {
                                    BackgroundColor = Color.Red,
                                    HoverColor = Color.DarkRed
                                },
                                XSizing = ElementSizing.Fixed(10),
                                YSizing = ElementSizing.Fixed(10),
                            },
                            () => Debug.WriteLine($"[{x2}, {y2}]")
                        );
                    }
                    UIBuilder.End();
                }
            }
            UIBuilder.End();
        }
        UIBuilder.End();
    }
}
