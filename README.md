# Embyr Engine
Embyr Engine is a small & lightweight free and open source 2D game engine! It aims to make the creation of games in the [MonoGame Framework](https://monogame.net/) much simpler!

## Features
- Physics engine
- Asset management
- Immediate-mode UI
- Actor component system
- Customizable rendering pipelines
  - 2D Deferred rendering
  - Forward 3D rendering (EARLY TESTING)
- Input and action binding systems
- Awesome 2D and 3D lighting
- 2D tile map creation

## Installation
Clone this repository into a MonoGame project:
```
git clone https://github.com/not-phoeniix/EmbyrEngine.git
```
Add a project reference to the engine in your `.csproj` file
```xml
<ItemGroup>
  <ProjectReference Include="EmbyrEngine/Embyr/Embyr.csproj" />
</ItemGroup>
```

## License
Embyr Engine is licensed under the **MIT License**. Please see the [LICENSE](LICENSE) document for more details.
