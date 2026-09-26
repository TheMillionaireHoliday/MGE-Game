# Hani Jahan Design Shaders

This folder contains the Hani Jahan Design shader packages and the resources shared by those packages. Each package includes ready-to-use materials, demo scenes, editor tooling where needed, and its own setup documentation.

## Folder layout

```text
Shaders/
├── UnityPackName/
│   ├── Editor/
│   ├── Materials/
│   └── Scenes/
├── SharedShaders/
└── SharedTextures/
```

## Shared resources

Do not remove `SharedShaders` or `SharedTextures` when importing an individual
package. Package materials and scenes can reference assets in these folders.
Shared shader behavior and render-pipeline notes are documented in
[`SharedShaders/README.md`](SharedShaders/README.md).

## Requirements and installation

- Use Unity 2022.3 LTS or newer.
- Built-in materials require the Built-in Render Pipeline.
- URP materials require a project configured for URP.

Import the package, keep its relative folder structure intact, and then open the
demo scene for the render pipeline in use. Refer to the package README for any
additional camera or render-pipeline setup.
