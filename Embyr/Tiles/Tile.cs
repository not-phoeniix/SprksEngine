using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Embyr.Scenes;
using Embyr.Physics;
using Embyr.Rendering;

namespace Embyr.Tiles;

/// <summary>
/// Immovable tile struct, collides with other actors and can be instantiated within a <c>TileMap</c>
/// </summary>
public abstract class Tile<T> : Actor2D where T : Enum {
    #region // Static members

    /// <summary>
    /// Enum that converts the direction of a relative neighbor to an
    /// integer index in a binary value. Represents relative directions,
    /// directions, where to point from where to point from "current tile"
    /// </summary>
    public enum NeighborDirection {
        Top = 7,
        TopRight = 6,
        Right = 5,
        BottomRight = 4,
        Bottom = 3,
        BottomLeft = 2,
        Left = 1,
        TopLeft = 0
    }

    /// <summary>
    /// Size (width/height) of a tile in pixels
    /// </summary>
    public static readonly int PixelSize = 8;

    private static Rectangle GetBlobSource(byte bitmask) {
        // hi :D this is easily the worst part of code in this project
        //   hardcoding literally every one of the 256 possible cases
        //   of a value for a byte variable
        //
        // there's probably a way better way to do this lol

        // layout guide:
        //   1's represent tile existing, 0 represent air
        //   T TR R BR B BL L TL

        // this switch is organized so the return rects iterate from
        //   left to right top to bottom in the actual 7x7 tileset sheet

        // another note:
        //   the bottom-most case right above all return statements
        //   is always the "least connecting case",,, aka the "most
        //   common case" i guess? all other cases above that final
        //   case represent other possible combinations but the
        //   bottom-most one is the "least connections" case with the
        //   least 1's in its binary
        switch (bitmask) {
            case 0b01010101:
            case 0b01010100:
            case 0b01010001:
            case 0b01010000:
            case 0b01000101:
            case 0b01000100:
            case 0b01000001:
            case 0b01000000:
            case 0b00010101:
            case 0b00010100:
            case 0b00010001:
            case 0b00010000:
            case 0b00000101:
            case 0b00000100:
            case 0b00000001:
            case 0b00000000:
                return new Rectangle(0 * PixelSize, 0 * PixelSize, PixelSize, PixelSize);

            case 0b01110101:
            case 0b01110100:
            case 0b01110001:
            case 0b01110000:
            case 0b01100101:
            case 0b01100100:
            case 0b01100001:
            case 0b01100000:
            case 0b00110101:
            case 0b00110100:
            case 0b00110001:
            case 0b00110000:
            case 0b00100101:
            case 0b00100100:
            case 0b00100001:
            case 0b00100000:
                return new Rectangle(1 * PixelSize, 0 * PixelSize, PixelSize, PixelSize);

            case 0b01111011:
            case 0b01111010:
            case 0b00111011:
            case 0b00111010:
                return new Rectangle(2 * PixelSize, 0 * PixelSize, PixelSize, PixelSize);

            case 0b01111111:
            case 0b01111110:
            case 0b00111111:
            case 0b00111110:
                return new Rectangle(3 * PixelSize, 0 * PixelSize, PixelSize, PixelSize);

            case 0b01101111:
            case 0b00101111:
            case 0b01101110:
            case 0b00101110:
                return new Rectangle(4 * PixelSize, 0 * PixelSize, PixelSize, PixelSize);

            case 0b01011011:
            case 0b01011010:
            case 0b01001011:
            case 0b00011011:
            case 0b00001011:
            case 0b00011010:
            case 0b01001010:
            case 0b00001010:
                return new Rectangle(5 * PixelSize, 0 * PixelSize, PixelSize, PixelSize);

            case 0b01011101:
            case 0b01011100:
            case 0b01011001:
            case 0b01011000:
            case 0b01001101:
            case 0b01001100:
            case 0b01001001:
            case 0b01001000:
            case 0b00011101:
            case 0b00011100:
            case 0b00011001:
            case 0b00011000:
            case 0b00001101:
            case 0b00001100:
            case 0b00001001:
            case 0b00001000:
                return new Rectangle(0 * PixelSize, 1 * PixelSize, PixelSize, PixelSize);

            case 0b01101101:
            case 0b01101100:
            case 0b01101001:
            case 0b01101000:
            case 0b00101101:
            case 0b00101100:
            case 0b00101001:
            case 0b00101000:
                return new Rectangle(1 * PixelSize, 1 * PixelSize, PixelSize, PixelSize);

            case 0b11101010:
                return new Rectangle(2 * PixelSize, 1 * PixelSize, PixelSize, PixelSize);

            case 0b11111011:
                return new Rectangle(3 * PixelSize, 1 * PixelSize, PixelSize, PixelSize);

            case 0b11011111:
            case 0b11001111:
            case 0b10011111:
            case 0b10001111:
                return new Rectangle(4 * PixelSize, 1 * PixelSize, PixelSize, PixelSize);

            case 0b10101101:
            case 0b10101100:
            case 0b10101001:
            case 0b10101000:
                return new Rectangle(5 * PixelSize, 1 * PixelSize, PixelSize, PixelSize);

            case 0b01010111:
            case 0b01010110:
            case 0b01010011:
            case 0b01010010:
            case 0b01000111:
            case 0b01000110:
            case 0b01000011:
            case 0b01000010:
            case 0b00010111:
            case 0b00010110:
            case 0b00010011:
            case 0b00010010:
            case 0b00000111:
            case 0b00000110:
            case 0b00000011:
            case 0b00000010:
                return new Rectangle(6 * PixelSize, 1 * PixelSize, PixelSize, PixelSize);

            case 0b10111101:
            case 0b10111100:
            case 0b10111001:
            case 0b10111000:
                return new Rectangle(0 * PixelSize, 2 * PixelSize, PixelSize, PixelSize);

            case 0b10101110:
                return new Rectangle(1 * PixelSize, 2 * PixelSize, PixelSize, PixelSize);

            case 0b10101010:
                return new Rectangle(2 * PixelSize, 2 * PixelSize, PixelSize, PixelSize);

            case 0b11110110:
            case 0b11100110:
            case 0b11110010:
            case 0b11100010:
                return new Rectangle(3 * PixelSize, 2 * PixelSize, PixelSize, PixelSize);

            case 0b10111011:
                return new Rectangle(4 * PixelSize, 2 * PixelSize, PixelSize, PixelSize);

            case 0b10111110:
                return new Rectangle(5 * PixelSize, 2 * PixelSize, PixelSize, PixelSize);

            case 0b01011111:
            case 0b01011110:
            case 0b01001111:
            case 0b00011111:
            case 0b00001111:
            case 0b00011110:
            case 0b01001110:
            case 0b00001110:
                return new Rectangle(6 * PixelSize, 2 * PixelSize, PixelSize, PixelSize);

            case 0b11111101:
            case 0b11111100:
            case 0b11111001:
            case 0b11111000:
                return new Rectangle(0 * PixelSize, 3 * PixelSize, PixelSize, PixelSize);

            case 0b10111111:
                return new Rectangle(1 * PixelSize, 3 * PixelSize, PixelSize, PixelSize);

            case 0b11011110:
            case 0b11001110:
            case 0b10011110:
            case 0b10001110:
                return new Rectangle(2 * PixelSize, 3 * PixelSize, PixelSize, PixelSize);

            case 0b01111101:
            case 0b01111100:
            case 0b01111001:
            case 0b01111000:
            case 0b00111101:
            case 0b00111100:
            case 0b00111001:
            case 0b00111000:
                return new Rectangle(3 * PixelSize, 3 * PixelSize, PixelSize, PixelSize);

            case 0b11111110:
                return new Rectangle(4 * PixelSize, 3 * PixelSize, PixelSize, PixelSize);

            case 0b11101111:
                return new Rectangle(5 * PixelSize, 3 * PixelSize, PixelSize, PixelSize);

            case 0b11011011:
            case 0b11001011:
            case 0b10011011:
            case 0b10001011:
                return new Rectangle(6 * PixelSize, 3 * PixelSize, PixelSize, PixelSize);

            case 0b11101101:
            case 0b11101100:
            case 0b11101001:
            case 0b11101000:
                return new Rectangle(0 * PixelSize, 4 * PixelSize, PixelSize, PixelSize);

            case 0b11110111:
            case 0b11110011:
            case 0b11100111:
            case 0b11100011:
                return new Rectangle(1 * PixelSize, 4 * PixelSize, PixelSize, PixelSize);

            case 0b10101011:
                return new Rectangle(2 * PixelSize, 4 * PixelSize, PixelSize, PixelSize);

            case 0b11111010:
                return new Rectangle(3 * PixelSize, 4 * PixelSize, PixelSize, PixelSize);

            case 0b11111111:
                return new Rectangle(4 * PixelSize, 4 * PixelSize, PixelSize, PixelSize);

            case 0b10101111:
                return new Rectangle(5 * PixelSize, 4 * PixelSize, PixelSize, PixelSize);

            case 0b11011010:
            case 0b11001010:
            case 0b10011010:
            case 0b10001010:
                return new Rectangle(6 * PixelSize, 4 * PixelSize, PixelSize, PixelSize);

            case 0b10110101:
            case 0b10110100:
            case 0b10110001:
            case 0b10110000:
            case 0b10100101:
            case 0b10100100:
            case 0b10100001:
            case 0b10100000:
                return new Rectangle(0 * PixelSize, 5 * PixelSize, PixelSize, PixelSize);

            case 0b01101011:
            case 0b01101010:
            case 0b00101011:
            case 0b00101010:
                return new Rectangle(1 * PixelSize, 5 * PixelSize, PixelSize, PixelSize);

            case 0b10111010:
                return new Rectangle(2 * PixelSize, 5 * PixelSize, PixelSize, PixelSize);

            case 0b11101110:
                return new Rectangle(3 * PixelSize, 5 * PixelSize, PixelSize, PixelSize);

            case 0b11101011:
                return new Rectangle(4 * PixelSize, 5 * PixelSize, PixelSize, PixelSize);

            case 0b11010111:
            case 0b11010011:
            case 0b11000111:
            case 0b11000011:
            case 0b10010111:
            case 0b10010011:
            case 0b10000111:
            case 0b10000011:
                return new Rectangle(5 * PixelSize, 5 * PixelSize, PixelSize, PixelSize);

            case 0b11011101:
            case 0b11011100:
            case 0b11011001:
            case 0b11011000:
            case 0b11001101:
            case 0b11001100:
            case 0b11001001:
            case 0b11001000:
            case 0b10011101:
            case 0b10011100:
            case 0b10011001:
            case 0b10011000:
            case 0b10001101:
            case 0b10001100:
            case 0b10001001:
            case 0b10001000:
                return new Rectangle(6 * PixelSize, 5 * PixelSize, PixelSize, PixelSize);

            case 0b11010101:
            case 0b11010100:
            case 0b11010001:
            case 0b11010000:
            case 0b11000101:
            case 0b11000100:
            case 0b11000001:
            case 0b11000000:
            case 0b10010101:
            case 0b10010100:
            case 0b10010001:
            case 0b10010000:
            case 0b10000101:
            case 0b10000100:
            case 0b10000001:
            case 0b10000000:
                return new Rectangle(1 * PixelSize, 6 * PixelSize, PixelSize, PixelSize);

            case 0b11110101:
            case 0b11110100:
            case 0b11110001:
            case 0b11110000:
            case 0b11100101:
            case 0b11100100:
            case 0b11100001:
            case 0b11100000:
                return new Rectangle(2 * PixelSize, 6 * PixelSize, PixelSize, PixelSize);

            case 0b10110111:
            case 0b10110011:
            case 0b10100111:
            case 0b10100011:
                return new Rectangle(3 * PixelSize, 6 * PixelSize, PixelSize, PixelSize);

            case 0b10110110:
            case 0b10110010:
            case 0b10100110:
            case 0b10100010:
                return new Rectangle(4 * PixelSize, 6 * PixelSize, PixelSize, PixelSize);

            case 0b01110111:
            case 0b01110110:
            case 0b01110011:
            case 0b01110010:
            case 0b01100111:
            case 0b01100110:
            case 0b01100011:
            case 0b01100010:
            case 0b00110111:
            case 0b00110110:
            case 0b00110011:
            case 0b00110010:
            case 0b00100111:
            case 0b00100110:
            case 0b00100011:
            case 0b00100010:
                return new Rectangle(5 * PixelSize, 6 * PixelSize, PixelSize, PixelSize);

            case 0b11010110:
            case 0b11010010:
            case 0b11000110:
            case 0b11000010:
            case 0b10010110:
            case 0b10010010:
            case 0b10000110:
            case 0b10000010:
                return new Rectangle(6 * PixelSize, 6 * PixelSize, PixelSize, PixelSize);
        }
    }

    #endregion

    #region // Fields & Properties

    // TODO: make tiles have sprite components rather than manual drawing?

    private readonly bool usesTileset;
    private readonly SpriteComponent2D sprite;

    // 8 Bit integer that holds 8-directional exposed information.
    //   1 means tile exists, 0 means air.
    //   Pattern: T TR R BR B BL L TL
    private byte neighborBitmask;

    /// <summary>
    /// Gets the enumeration type value of this tile
    /// </summary>
    public T Type { get; }

    /// <summary>
    /// Position of this tile in tilespace, where units of 1 are entire tiles
    /// </summary>
    public Point TilespacePosition {
        get {
            Vector2 pos = Transform.GlobalPosition / PixelSize;
            return pos.ToPoint();
        }
    }

    /// <summary>
    /// Gets the collider for for this tile
    /// </summary>
    public BoxCollider2D Collider { get; }

    /// <summary>
    /// Gets/sets whether or not this tile obstructs light
    /// </summary>
    public bool ObstructsLight {
        get => sprite.ObstructsLight;
        set => sprite.ObstructsLight = value;
    }

    #endregion

    /// <summary>
    /// Creates a new Tile object, with given information
    /// </summary>
    public Tile(T type, string name, Texture2D texture, Texture2D? normal, bool usesTileset, Scene2D scene)
        : base(name, Vector2.Zero, scene) {
        this.Transform = new Transform2D();
        this.usesTileset = usesTileset;
        this.Type = type;

        Collider = AddComponent<BoxCollider2D>();
        Collider.Size = new Point(PixelSize);

        sprite = AddComponent<SpriteComponent2D>();
        sprite.Texture = texture;
        sprite.Normal = normal;

        if (usesTileset) {
            sprite.SourceRect = GetBlobSource(0);
        }
    }

    #region Methods

    /// <summary>
    /// Checks 8 surrounding tiles and updates which edges are exposed to disconnected types
    /// </summary>
    /// <param name="disconnectedTypes">List of enum types which shouldn't be connected to this tile</param>
    public void UpdateEdges(TileMap<T> mapContainer, List<T>? disconnectedTypes) {
        bool TileDirCheck(NeighborDirection direction) {
            // world pos for tile in inputted direction
            Point adjPos = TilespacePosition + direction switch {
                NeighborDirection.Top => new Point(0, -1),
                NeighborDirection.TopRight => new Point(1, -1),
                NeighborDirection.Right => new Point(1, 0),
                NeighborDirection.BottomRight => new Point(1, 1),
                NeighborDirection.Bottom => new Point(0, 1),
                NeighborDirection.BottomLeft => new Point(-1, 1),
                NeighborDirection.Left => new Point(-1, 0),
                NeighborDirection.TopLeft => new Point(-1, -1),
            };

            Tile<T> tile = mapContainer[adjPos.X, adjPos.Y];

            // no tile existing means nothing's there, therefore no tile exists (uhhuh)
            if (tile == null) return false;

            // start as connecting but mark not connecting
            //   if any ignored types are detected
            bool connecting = true;
            if (disconnectedTypes != null) {
                foreach (T type in disconnectedTypes) {
                    if (tile.Type.Equals(type)) {
                        connecting = false;
                        break;
                    }
                }
            }

            return connecting;
        }

        foreach (NeighborDirection dir in Enum.GetValues<NeighborDirection>()) {
            bool exists = TileDirCheck(dir);
            NeighborUpdate(dir, exists);
        }

        if (usesTileset) {
            sprite.SourceRect = GetBlobSource(neighborBitmask);
        } else {
            sprite.SourceRect = null;
        }
    }

    /// <summary>
    /// Changes/updates a directional value inside the neighbor bitmask
    /// </summary>
    /// <param name="direction">Direction to change</param>
    /// <param name="exists">New value of whether or not neighbor exists there</param>
    public void NeighborUpdate(NeighborDirection direction, bool exists) {
        neighborBitmask = NeighborUpdate(direction, exists, neighborBitmask);
    }

    private static byte NeighborUpdate(NeighborDirection direction, bool exists, byte bitmask) {
        int index = (int)direction;
        byte modifier = (byte)(1 << index);

        // flip input if setting bit to false
        if (exists == false)
            bitmask = (byte)~bitmask;

        bitmask |= modifier;

        // re-flip again if setting bit to false
        if (exists == false)
            bitmask = (byte)~bitmask;

        return bitmask;
    }

    /// <summary>
    /// Grabs a value whether or not a neighboring tile exists at a given relative direction
    /// </summary>
    /// <param name="direction">What direction to check if a tile exists relative to this tile</param>
    /// <returns>True if there's a tile at that location, false if not</returns>
    public bool NeighborExists(NeighborDirection direction) {
        return NeighborExists(direction, neighborBitmask);
    }

    private static bool NeighborExists(NeighborDirection direction, byte bitmask) {
        int index = (int)direction;

        int modifier = 1 << index;
        int result = bitmask & modifier;
        result >>= index;
        return result == 1;
    }

    #endregion
}
