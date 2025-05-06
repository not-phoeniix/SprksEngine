using Microsoft.Xna.Framework;

namespace Embyr.Scenes;

public abstract class Scene3D : Scene {
    private readonly Octree<IActor3D> actors;
    private readonly Octree<Light3D> localLights;
    private readonly List<Light3D> globalLights;

    public Scene3D(string name) : base(name) {
        actors = new Octree<IActor3D>(Vector3.One * -10_000, Vector3.One * 10_000);
        localLights = new Octree<Light3D>(Vector3.One * -10_000, Vector3.One * 10_000);
        globalLights = new List<Light3D>();
    }
}
