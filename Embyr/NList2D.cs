using System;
using Microsoft.Xna.Framework;

namespace Embyr;

/// <summary>
/// A collection in 2D space, can be indexed negatively
/// </summary>
public sealed class NList2D<T> {
    private T[,] data;
    private Point offset;

    /// <summary>
    /// Access data by index, can be negative
    /// </summary>
    public T this[int x, int y] {
        get { return GetData(x, y); }
        set { SetData(value, x, y); }
    }

    /// <summary>
    /// Size/capacity of negative collection
    /// </summary>
    public Point Size {
        get {
            return new Point(
                data.GetLength(0),
                data.GetLength(1)
            );
        }
    }

    /// <summary>
    /// Minimum index values in x/y (inclusive)
    /// </summary>
    public Point Min => new(-Size.X / 2, -Size.Y / 2);

    /// <summary>
    /// Maximum index values in x/y (exclusive,
    /// these indices are 1 greater than the max indices)
    /// </summary>
    // use mod here to prevent odd sizes from truncating
    public Point Max => new(Size.X / 2 + Size.X % 2, Size.Y / 2 + Size.Y % 2);

    /// <summary>
    /// Creates a new negative collection array
    /// </summary>
    /// <param name="width">Width of collection array</param>
    /// <param name="height">Height of collection array</param>
    public NList2D(int width, int height) {
        data = new T[width, height];
        offset = new Point(width / 2, height / 2);
    }

    /// <summary>
    /// Creates a new negative collection array (1x1 by default)
    /// </summary>
    public NList2D() : this(1, 1) { }

    /// <summary>
    /// Retrieves an item from the collection via indexed x/y
    /// </summary>
    /// <param name="x">X index (can be negative)</param>
    /// <param name="y">Y index (can be negative)</param>
    /// <returns>Reference to item at inputted indices</returns>
    public T GetData(int x, int y) {
        // throw exception if out of bounds
        if (!InBounds(x, y)) {
            throw new Exception($"Index ({x}, {y}) is out of bounds of negative collection!");
        }

        return data[x + offset.X, y + offset.Y];
    }

    /// <summary>
    /// Puts an item into the collection at the given
    /// index, only within current existing size
    /// </summary>
    /// <param name="item">Item to add to collection</param>
    /// <param name="x">X index (can be negative)</param>
    /// <param name="y">Y index (can be negative)</param>
    public void SetData(T item, int x, int y) {
        // throw exception if out of bounds
        if (!InBounds(x, y)) {
            throw new Exception($"Index ({x}, {y}) is out of bounds of negative collection!");
        }

        data[x + offset.X, y + offset.Y] = item;
    }

    /// <summary>
    /// Dynamically adds an item to the collection, expanding if necessary
    /// </summary>
    /// <param name="item">Item to add to collection</param>
    /// <param name="x">X index to add to</param>
    /// <param name="y">Y index to add to</param>
    public void Add(T item, int x, int y) {
        // resize/grow array shtuff if x/y not in bounds
        while (!InBounds(x, y)) {
            if (!XInBounds(x)) DoubleWidth();
            if (!YInBounds(y)) DoubleHeight();
        }

        SetData(item, x, y);
    }

    /// <summary>
    /// Clears all data in collection
    /// </summary>
    public void Clear() {
        for (int x = 0; x < data.GetLength(0); x++) {
            for (int y = 0; y < data.GetLength(1); y++) {
                data[x, y] = default;
            }
        }
    }

    /// <summary>
    /// Calculates whether an x/y position is in bounds of array or not
    /// </summary>
    /// <param name="x">X index in collection</param>
    /// <param name="y">Y index in collection</param>
    /// <returns>True if in bounds, false if not</returns>
    public bool InBounds(int x, int y) {
        return XInBounds(x) && YInBounds(y);
    }

    private bool XInBounds(int x) {
        return
            x + offset.X >= 0 &&
            x + offset.X < data.GetLength(0);
    }

    private bool YInBounds(int y) {
        return
            y + offset.Y >= 0 &&
            y + offset.Y < data.GetLength(1);
    }

    private void DoubleWidth() {
        // double x axis in size
        int width = data.GetLength(0);
        int height = data.GetLength(1);
        int newWidth = width * 2;

        // create new offset and new array
        Point newOffset = new(newWidth / 2, offset.Y);
        T[,] newData = new T[newWidth, height];

        // copy old array contents into "center" of new array
        for (int x = -width / 2; x < width / 2; x++) {
            for (int y = -height / 2; y < height / 2; y++) {
                newData[x + newOffset.X, y + newOffset.Y] = data[x + offset.X, y + offset.Y];
            }
        }

        // update internal data fields
        data = newData;
        offset = newOffset;
    }

    private void DoubleHeight() {
        // double y axis in size
        int width = data.GetLength(0);
        int height = data.GetLength(1);
        int newHeight = height * 2;

        // create new offset and new array
        Point newOffset = new(offset.X, newHeight / 2);
        T[,] newData = new T[width, newHeight];

        // copy old array contents into "center" of new array
        for (int x = -width / 2; x < width / 2; x++) {
            for (int y = -height / 2; y < height / 2; y++) {
                newData[x + newOffset.X, y + newOffset.Y] = data[x + offset.X, y + offset.Y];
            }
        }

        // update internal data fields
        data = newData;
        offset = newOffset;
    }
}
