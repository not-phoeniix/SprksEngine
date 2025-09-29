namespace Sprks;

/// <summary>
/// Abstract singleton design parent, allows
/// static thread-safe instance of the child class
/// </summary>
/// <typeparam name="T">Type of singleton, child class type</typeparam>
public abstract class Singleton<T> where T : class, new() {
    protected Singleton() { }

    private static readonly object key = new();
    private static T instance = null;

    /// <summary>
    /// Instance of static singleton
    /// </summary>
    public static T I {
        get {
            // only locks and instantiates if instance is null
            //   (only allowing 1 thread to create instance)
            if (instance == null) {
                lock (key) {
                    instance = new T();
                }
            }

            return instance;
        }
    }
}
