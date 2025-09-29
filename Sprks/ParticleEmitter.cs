using Sprks.Scenes;
using Microsoft.Xna.Framework;

namespace Sprks;

/// <summary>
/// An actor that emits gpu-simulated particles
/// </summary>
public class ParticleEmitter2D : Actor2D {
    //! https://www.reddit.com/r/gameenginedevs/comments/pdanwj/i_implemented_a_gpu_particle_system_this_week_it/
    //! https://wickedengine.net/2017/11/gpu-based-particle-simulation/
    //! https://www.gamedeveloper.com/programming/building-a-million-particle-system

    private float counter;

    /// <summary>
    /// Gets/sets whether or not this emitter emits particles
    /// </summary>
    public bool Emitting { get; set; }

    /// <summary>
    /// Gets/sets the number of seconds between each emission
    /// </summary>
    public float EmitSpeed { get; set; }

    /// <summary>
    /// Gets number of particles to emit this frame
    /// </summary>
    internal int NumParticlesToEmit { get; private set; }

    /// <summary>
    /// Creates a new ParticleEmitter2D
    /// </summary>
    /// <param name="position">Position of particle emitter in scene</param>
    /// <param name="scene">Scene to spawn emitter in</param>
    public ParticleEmitter2D(Vector2 position, Scene2D scene)
    : base(position, scene) {
        Emitting = true;
        EmitSpeed = 0.1f;
    }

    /// <inheritdoc/>
    public override void Update(float deltaTime) {
        counter += deltaTime;

        NumParticlesToEmit = 0;

        while (counter > EmitSpeed) {
            counter -= EmitSpeed;
            NumParticlesToEmit++;
        }

        base.Update(deltaTime);
    }
}
