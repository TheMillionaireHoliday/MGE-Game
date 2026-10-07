# Changelog

## 1.0.1

- Fixed URP toon objects missing from the camera depth texture by adding a dedicated depth-only pass, restoring depth-based intersection effects such as stylized water foam.

## 1.0.0

- Initial Unity Asset Store release.
- Added matching handwritten shaders for the Built-in Render Pipeline and Universal Render Pipeline (URP).
- Added eight shading sources, four color modes, optional scene lighting, rim light, and inverted-hull outlines.
- Added four ready-to-use toon material presets and one demo scene for each supported render pipeline.
- Added a shared HLSL implementation and a custom material inspector for both shaders.
- Added setup instructions, control reference, compatibility notes, troubleshooting guidance, and a package welcome window.
- Tested with Unity 2022.3 LTS and Unity 6.0 LTS; HDRP is not included.
