using System;
using Microsoft.Xna.Framework;

namespace Embyr;

/// <summary>
/// A collection in 2D space, can be indexed negatively
/// </summary>
public sealed class NList2D<T> {
    private T?[] data;
    private Point offset;

    /// <summary>
    /// Access data by index, can be negative
    /// </summary>
    public T? this[int x, int y] {
        get { return GetData(x, y); }
        set { SetData(value, x, y); }
    }

    /// <summary>
    /// Size/capacity of negative collection
    /// </summary>
    public Point Size { get; private set; }

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
    /// <param name="width">Initial capacity width</param>
    /// <param name="height">Initial capacity height</param>
    public NList2D(int width, int height) {
        if (width <= 0 || height <= 0) {
            throw new Exception("Cannot initialize an NList2D with a negative or 0 capacity!");
        }

        data = new T[width * height];
        offset = new Point(width / 2, height / 2);
        Size = new Point(width, height);
    }

    /// <summary>
    /// Creates a new negative collection array with a capacity of 1x1
    /// </summary>
    public NList2D() : this(1, 1) { }

    /// <summary>
    /// Retrieves an item from the collection via indexed x/y
    /// </summary>
    /// <param name="x">X index (can be negative)</param>
    /// <param name="y">Y index (can be negative)</param>
    /// <returns>Reference to item at inputted indices</returns>
    public T? GetData(int x, int y) {
        // throw exception if out of bounds
        if (!InBounds(x, y)) {
            throw new Exception($"Index ({x}, {y}) is out of bounds of negative collection!");
        }

        return data[(y + offset.Y) * Size.X + x + offset.X];
    }

    /// <summary>
    /// Tries to get data in this NList2D without throwing an out of bounds exception
    /// </summary>
    /// <param name="x">X index (can be negative)</param>
    /// <param name="y">Y index (can be negative)</param>
    /// <param name="data">Output data if it exists</param>
    /// <returns>Whether or not data exists in n list</returns>
    public bool TryGetData(int x, int y, out T? data) {
        if (InBounds(x, y)) {
            data = this.data[(y + offset.Y) * Size.X + x + offset.X];
            return true;
        }

        data = default;
        return false;
    }

    /// <summary>
    /// Puts an item into the collection at the given
    /// index, only within current existing size
    /// </summary>
    /// <param name="item">Item to add to collection</param>
    /// <param name="x">X index (can be negative)</param>
    /// <param name="y">Y index (can be negative)</param>
    public void SetData(T? item, int x, int y) {
        // throw exception if out of bounds
        if (!InBounds(x, y)) {
            throw new Exception($"Index ({x}, {y}) is out of bounds of negative collection!");
        }

        data[(y + offset.Y) * Size.X + x + offset.X] = item;
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
        for (int x = 0; x < Size.X; x++) {
            for (int y = 0; y < Size.Y; y++) {
                data[y * Size.Y + x] = default;
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
            x + offset.X < Size.X;
    }

    private bool YInBounds(int y) {
        return
            y + offset.Y >= 0 &&
            y + offset.Y < Size.Y;
    }

    private void DoubleWidth() {
        int newWidth = Size.X * 2;

        // create new offset and new array
        Point newOffset = new(newWidth / 2, offset.Y);
        T?[] newData = new T[newWidth * Size.Y];

        // copy old array contents into "center" of new array
        for (int x = -Size.X / 2; x < Math.Max(Size.X / 2, -Size.X / 2 + 1); x++) {
            for (int y = -Size.Y / 2; y < Math.Max(Size.Y / 2, -Size.Y / 2 + 1); y++) {
                int oldIndex = (y + offset.Y) * Size.X + x + offset.X;
                int newIndex = (y + newOffset.Y) * newWidth + x + offset.X;
                newData[newIndex] = data[oldIndex];
            }
        }

        // update internal data fields
        data = newData;
        offset = newOffset;
        Size = new Point(newWidth, Size.Y);
    }

    private void DoubleHeight() {
        int newHeight = Size.Y * 2;

        // create new offset and new array
        Point newOffset = new(offset.X, newHeight / 2);
        T?[] newData = new T[Size.X * newHeight];

        // copy old array contents into "center" of new array
        for (int x = -Size.X / 2; x < Math.Max(Size.X / 2, -Size.X / 2 + 1); x++) {
            for (int y = -Size.Y / 2; y < Math.Max(Size.Y / 2, -Size.Y / 2 + 1); y++) {
                int oldIndex = (y + offset.Y) * Size.X + x + offset.X;
                int newIndex = (y + newOffset.Y) * Size.X + x + offset.X;
                newData[newIndex] = data[oldIndex];
            }
        }

        // update internal data fields
        data = newData;
        offset = newOffset;
        Size = new Point(Size.X, newHeight);
    }
}
