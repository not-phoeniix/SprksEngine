using Microsoft.Xna.Framework;

namespace Embyr;

internal static class Palette {
    public static readonly Color Col0 = new(5, 7, 13);          // #05070d
    public static readonly Color Col1 = new(24, 26, 38);        // #181a26
    public static readonly Color Col2 = new(53, 58, 77);        // #353a4d
    public static readonly Color Col3 = new(167, 174, 204);     // #a7aecc
    public static readonly Color Col4 = new(225, 229, 242);     // #e1e5f2
    public static readonly Color Col5 = new(94, 8, 31);         // #5e081f
    public static readonly Color Col6 = new(143, 19, 52);       // #8f1334
    public static readonly Color Col7 = new(242, 27, 84);       // #f21b54
    public static readonly Color Col8 = new(110, 51, 2);        // #6e3302
    public static readonly Color Col9 = new(183, 88, 11);       // #b7580b
    public static readonly Color Col10 = new(238, 142, 64);     // #ee8e40
    public static readonly Color Col11 = new(8, 49, 107);       // #08316b
    public static readonly Color Col12 = new(19, 78, 161);      // #134ea1
    public static readonly Color Col13 = new(47, 119, 219);     // #2f77db
    public static readonly Color Col14 = new(8, 110, 45);       // #086e2d
    public static readonly Color Col15 = new(25, 171, 79);      // #19ab4f
    public static readonly Color Col16 = new(63, 237, 127);     // #3fed7f

    public static readonly Vector4[] AsArray = new Vector4[] {
        Col0.ToVector4(),
        Col1.ToVector4(),
        Col2.ToVector4(),
        Col3.ToVector4(),
        Col4.ToVector4(),
        Col5.ToVector4(),
        Col6.ToVector4(),
        Col7.ToVector4(),
        Col8.ToVector4(),
        Col9.ToVector4(),
        Col10.ToVector4(),
        Col11.ToVector4(),
        Col12.ToVector4(),
        Col13.ToVector4(),
        Col14.ToVector4(),
        Col15.ToVector4(),
        Col16.ToVector4()
    };
}
