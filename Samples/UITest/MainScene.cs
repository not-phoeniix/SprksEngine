using System.Diagnostics;
using Embyr;
using Embyr.Scenes;
using Embyr.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UITest;

public class MainScene : Scene2D {
    private Font font;
    private ElementStyle textStyle;
    private Texture2D dogImage;

    public override void LoadContent() {
        font = Assets.Load<Font>("futuristic");
        textStyle = new ElementStyle() {
            Font = font,
            BackgroundColor = Color.DarkSlateGray,
            Color = Color.White,
            Padding = new ElementPadding(2),
        };
        dogImage = Assets.Load<Texture2D>("dogwho_is___also____small");

        base.LoadContent();
    }

    public override void BuildUI() {
        // using C# scopes { } is a nice way to
        //   show hierarchy with the elements :]

        Gooey.BeginElement(new ElementProperties() {
            Direction = AlignDirection.TopToBottom,
            Style = ElementStyle.EmptyTransparent(),
            XSizing = ElementSizing.Grow(),
            YSizing = ElementSizing.Grow()
        });
        {
            BuildTopBar();

            Gooey.BeginElement(new ElementProperties() {
                Style = ElementStyle.EmptyTransparent(),
                XSizing = ElementSizing.Grow(),
                YSizing = ElementSizing.Grow(),
                Direction = AlignDirection.LeftToRight
            });
            {
                BuildSideBar();

                BuildMiddleSection();

                BuildSideBar();
            }
            Gooey.End();

            BuildBottomBar();
        }
        Gooey.End();
    }

    //* !!! Element section build methods !!!
    //*   these are easy-to-reuse smaller chunks of UI code
    //*   that make the BuildUI method much less cluttered

    private void BuildTopBar() {
        Gooey.BeginElement(new ElementProperties() {
            Style = new ElementStyle() {
                BackgroundColor = Color.DarkSlateGray,
                Padding = new ElementPadding(4),
                Gap = 4,
            },
            XSizing = ElementSizing.Grow(),
            YSizing = ElementSizing.Fit(),
            Direction = AlignDirection.LeftToRight
        });
        {
            for (int i = 0; i < 10; i++) {
                Color gradientColor = new(
                    i / 10.0f * 0.5f + 0.5f,
                    1.0f,
                    1.0f
                );

                Gooey.Button(
                    new ElementProperties() {
                        Style = new ElementStyle() {
                            BackgroundColor = gradientColor,
                            Color = Color.Black,
                            Font = font
                        },
                        XSizing = ElementSizing.Grow(),
                        YSizing = ElementSizing.Grow()
                    },
                    ":D",
                    static () => Debug.WriteLine("waow....")
                );
            }

            Gooey.BeginElement(new ElementProperties() {
                Direction = AlignDirection.TopToBottom,
                Style = new ElementStyle() {
                    BackgroundColor = Color.Gray,
                    Gap = 1
                },
            });
            {
                for (int y = 0; y < 3; y++) {
                    Gooey.BeginElement(new ElementProperties() {
                        Direction = AlignDirection.LeftToRight,
                        Style = new ElementStyle() {
                            BackgroundColor = Color.Transparent,
                            Gap = 1
                        },
                    });
                    for (int x = 0; x < 3; x++) {
                        int x2 = x;
                        int y2 = y;

                        Gooey.Clickable(
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
                    Gooey.End();
                }
            }
            Gooey.End();
        }
        Gooey.End();
    }

    private void BuildSideBar() {
        Gooey.BeginElement(new ElementProperties() {
            YSizing = ElementSizing.Grow(),
            Style = textStyle,
            Direction = AlignDirection.TopToBottom
        });

        for (int i = 0; i < 8; i++) {
            Gooey.Image(
                new ElementProperties() {
                    Style = ElementStyle.EmptyTransparent(),
                    YSizing = ElementSizing.Grow()
                },
                new ImageProperties() {
                    Texture = dogImage,
                    Color = Color.LightPink,
                    ManualSize = new Point(60, 30)
                }
            );
        }

        Gooey.End();
    }

    private void BuildMiddleSection() {
        Gooey.BeginElement(new ElementProperties() {
            Style = new ElementStyle(ElementStyle.EmptyTransparent()) {
                Gap = 5
            },
            XSizing = ElementSizing.Grow(),
            YSizing = ElementSizing.Grow(),
            Direction = AlignDirection.TopToBottom,
        });
        {
            Gooey.Element(new ElementProperties() {
                Style = ElementStyle.EmptyTransparent(),
                XSizing = ElementSizing.Grow(),
                YSizing = ElementSizing.Grow(),
            });

            Gooey.Text(
                new ElementProperties() {
                    Style = new ElementStyle() {
                        BackgroundColor = Color.Transparent,
                        Color = Color.White,
                        Font = font
                    },
                    XSizing = ElementSizing.Grow(),
                },
                "this layout sucks lol"
            );
            Gooey.Text(
                new ElementProperties() {
                    Style = new ElementStyle() {
                        BackgroundColor = Color.Transparent,
                        Color = new Color(0.7f, 0.7f, 0.7f),
                        Font = font
                    },
                    XSizing = ElementSizing.Grow(),
                },
                "at least you have an idea of how\nthis layout system works tho :]"
            );

            Gooey.Element(new ElementProperties() {
                Style = ElementStyle.EmptyTransparent(),
                XSizing = ElementSizing.Grow(),
                YSizing = ElementSizing.Grow(),
            });
        }
        Gooey.End();
    }

    private void BuildBottomBar() {
        Gooey.Text(
            new ElementProperties() {
                XSizing = ElementSizing.Grow(),
                Style = new ElementStyle(textStyle) {
                    Padding = new ElementPadding(5),
                }
            },
            "obtrusive gui, etc"
        );
    }
}
