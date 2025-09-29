# Sprks Engine
**Sprks Engine** is a small & lightweight free and open source 2D game engine! It aims to make the creation of games in the [MonoGame Framework](https://monogame.net/) much simpler!

## Important Notice
***!!! THIS ENGINE IS STILL EARLY IN DEVELOPMENT !!!***

This is my passion project and I do not recommend anyone use this as their engine due to many bugs, missing features, and rapid changes. If you'd like to try it out, you can, but please be warned. This project is rapidly changing and very unstable, and I'm a very busy college student. I work on this project in my free time :]

## Features
- Actor component system
- Physics system (2D only)
- Asset management
- Immediate-mode UI
- Awesome 2D and 3D lighting
- Customizable rendering pipelines
  - Deferred 2D rendering
  - Forward 3D rendering (EARLY TESTING)
- Input and action binding systems
- 2D tile map creation

## Installation
Clone this repository into a MonoGame project:
```
git clone https://github.com/not-phoeniix/SprksEngine.git
```
Add a project reference to the engine in your `.csproj` file
```xml
<ItemGroup>
  <ProjectReference Include="SprksEngine/Sprks/Sprks.csproj" />
</ItemGroup>
```

## Getting Started
Take a look at the [Samples](Samples) to see how projects are set up. Dotnet project templates are planned but have yet to be made. Documentation exists soley in the C# XML comments.

## License
Sprks Engine is licensed under the **MIT License**. Please see the [LICENSE](LICENSE) document for more details.
