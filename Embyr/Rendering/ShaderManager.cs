using System.Diagnostics;
using System.Reflection;
using Embyr.Scenes;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

internal class ShaderManager : Singleton<ShaderManager> {
    public enum ShaderProfile {
        OpenGL,
        DirectX
    }

    private readonly Dictionary<string, Effect> shaderCache = new();
    private GraphicsDevice gd;
    private ShaderProfile profile;

    /// <summary>
    /// Gets/sets the current base effect applied to actors when drawn
    /// </summary>
    public Effect? CurrentActorEffect { get; set; }

    /// <summary>
    /// Initializes the shader manager
    /// </summary>
    /// <param name="gd">Graphics device to create shaders with</param>
    /// <param name="profile">Graphics profile of program</param>
    public void Init(GraphicsDevice gd, ShaderProfile profile) {
        this.gd = gd;
        this.profile = profile;
    }

    /// <summary>
    /// Loads a shader effect from this manager
    /// </summary>
    /// <param name="shaderName">Name of shader file to load, can include paths</param>
    /// <returns>Reference to newly loaded shader</returns>
    public Effect LoadShader(string shaderName) {
        string profileSuffix = profile switch {
            ShaderProfile.OpenGL => "gl",
            ShaderProfile.DirectX => "dx",
            _ => throw new Exception($"Shader profile {profile} not recognized when loading shader!")
        };

        // allow paths to work by replacing slash separators with periods
        shaderName = shaderName.Replace("/", ".");
        shaderName = shaderName.Replace("\\", ".");

        string namespaceName = typeof(ShaderManager).Namespace!;
        string resourceName = $"{namespaceName}.Shaders.{shaderName}_{profileSuffix}.xnb";

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

        Effect effect = new(gd, byteCode);

        return effect;
    }

    // private Model CreateEmbeddedResourceModel(string resourceName) {
    //     Assembly assembly = this.GetType().Assembly;

    //     using Stream? stream = assembly.GetManifestResourceStream(resourceName);
    //     if (stream == null) {
    //         throw new NullReferenceException($"Embedded resource stream null - cannot find model resource \"{resourceName}\"");
    //     }

    //     Debug.WriteLine($"Creating model from resource \"{resourceName}\"...");

    //     Model m = new(
    //         SceneManager.I.GraphicsDevice,

    //     )
    // }
}
