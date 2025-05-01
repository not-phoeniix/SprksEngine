using System.Diagnostics;
using System.Reflection;
using Embyr.Scenes;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Shaders;

internal class ShaderManager : Singleton<ShaderManager> {
    private readonly Dictionary<string, Effect> shaderCache = new();

    public enum ShaderProfile {
        OpenGL,
        DirectX
    }

    /// <summary>
    /// Loads a shader effect from this manager
    /// </summary>
    /// <param name="shaderName"></param>
    /// <param name="profile"></param>
    /// <returns></returns>
    public Effect LoadShader(string shaderName, ShaderProfile profile) {
        string profileSuffix = profile switch {
            ShaderProfile.OpenGL => "gl",
            ShaderProfile.DirectX => "dx",
            _ => throw new Exception($"Shader profile {profile} not recognized when loading shader!")
        };

        string resourceName = $"Embyr.Shaders.PrecompiledBinaries.{shaderName}_{profileSuffix}.xnb";

        if (!shaderCache.TryGetValue(resourceName, out Effect? shader)) {
            shader = CreateEmbeddedResourceShader(resourceName);
            shaderCache[resourceName] = shader;
        }

        return shader;
    }

    /// <summary>
    /// Clears and unloads/disposes all shader
    /// </summary>
    public void Unload() {
        foreach (Effect e in shaderCache.Values) {
            e?.Dispose();
        }

        shaderCache.Clear();
    }

    /// <summary>
    /// Creates a new Effect from a precompiled embedded resource in this project
    /// </summary>
    /// <param name="resourceName">Name of compiled shader resource</param>
    /// <returns>A new Effect created from embedded resource</returns>
    /// <exception cref="NullReferenceException">Exception thrown when resource stream returns null</exception>
    private Effect CreateEmbeddedResourceShader(string resourceName) {
        Assembly assembly = this.GetType().Assembly;

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) {
            throw new NullReferenceException($"Embedded resource stream null - cannot find shader resource \"{resourceName}\"");
        }

        Debug.WriteLine($"Creating shader from resource \"{resourceName}\"...");

        using MemoryStream ms = new();
        stream.CopyTo(ms);
        byte[] byteCode = ms.ToArray();

        Effect effect = new(SceneManager.I.GraphicsDevice, byteCode);

        return effect;
    }
}
