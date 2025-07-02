using System;
using Microsoft.Xna.Framework;

namespace Embyr;

/// <summary>
/// A collection in 1D space, can be indexed negatively
/// </summary>
public sealed class NList<T> {
    private T?[] data;
    private int offset;

    /// <summary>
    /// Access data by index, can be negative
    /// </summary>
    public T? this[int i] {
        get { return GetData(i); }
        set { SetData(value, i); }
    }

    /// <summary>
    /// Size/capacity of negative list
    /// </summary>
    public int Size => data.Length;

    /// <summary>
    /// Minimum index values in i (inclusive)
    /// </summary>
    public int Min => -Size / 2;

    /// <summary>
    /// Maximum index values in x/y (exclusive,
    /// these indices are 1 greater than the max indices)
    /// </summary>
    // use mod here to prevent odd sizes from truncating
    public int Max => Size / 2 + Size % 2;

    /// <summary>
    /// Creates a new negative collection array
    /// </summary>
    /// <param name="size">Initial size of NList</param>
    public NList(int size) {
        data = new T[size];
        offset = size / 2;
    }

    /// <summary>
    /// Creates a new negative collection array (1x1 by default)
    /// </summary>
    public NList() : this(1) { }

    /// <summary>
    /// Retrieves an item from the collection via indexed x/y
    /// </summary>
    /// <param name="i">Index (can be negative)</param>
    /// <returns>Reference to item at inputted indices</returns>
    public T? GetData(int i) {
        // throw exception if out of bounds
        if (!InBounds(i)) {
            throw new Exception($"Index ({i}) is out of bounds of negative collection!");
        }

        return data[i + offset];
    }

    /// <summary>
    /// Tries to get data in this NList without throwing an out of bounds exception
    /// </summary>
    /// <param name="i">Index (can be negative)</param>
    /// <param name="data">Output data if it exists</param>
    /// <returns>Whether or not data exists in n list</returns>
    public bool TryGetData(int i, out T? data) {
        if (InBounds(i)) {
            data = this.data[i + offset];
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
    /// <param name="i">Index to set data at (can be negative)</param>
    public void SetData(T? item, int i) {
        // throw exception if out of bounds
        if (!InBounds(i)) {
            throw new Exception($"Index ({i}) is out of bounds of negative collection!");
        }

        data[i + offset] = item;
    }

    /// <summary>
    /// Dynamically adds an item to the collection, expanding if necessary
    /// </summary>
    /// <param name="item">Item to add to collection</param>
    /// <param name="i">Index to add to</param>
    public void Add(T item, int i) {
        // resize/grow array shtuff if x/y not in bounds
        while (!InBounds(i)) {
            DoubleSize();
        }

        SetData(item, i);
    }

    /// <summary>
    /// Clears all data in list, setting to default values
    /// </summary>
    public void Clear() {
        for (int i = 0; i < data.Length; i++) {
            data[i] = default;
        }
    }

    /// <summary>
    /// Calculates whether an index is in bounds of list or not
    /// </summary>
    /// <param name="i">Index in collection</param>
    /// <returns>True if in bounds, false if not</returns>
    public bool InBounds(int i) {
        return i + offset >= 0 && i + offset < data.Length;
    }

    private void DoubleSize() {
        // double x axis in size
        int size = data.Length;
        int newSize = size * 2;

        // create new offset and new array
        int newOffset = newSize / 2;
        T?[] newData = new T[newSize];

        // copy old array contents into "center" of new array
        for (int i = -size / 2; i < Math.Max(size / 2, -size / 2 + 1); i++) {
            newData[i + newOffset] = data[i + offset];
        }

        // update internal data fields
        data = newData;
        offset = newOffset;
    }
}
