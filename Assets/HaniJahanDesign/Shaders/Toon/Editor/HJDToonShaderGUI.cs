using UnityEditor;
using UnityEngine;

namespace HaniJahanDesign.Shaders
{
    public sealed class HJDToonShaderGUI : ShaderGUI
    {
        private readonly GUIContent[] mappingSourceNames =
        {
            new GUIContent("Main Light"),
            new GUIContent("World Up"),
            new GUIContent("Object Up"),
            new GUIContent("World Height"),
            new GUIContent("Object Height"),
            new GUIContent("View Angle"),
            new GUIContent("Vertex Alpha"),
            new GUIContent("Mask Texture")
        };

        private bool showAdvancedShading;
        private bool showAdvancedLighting;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            DrawSurface(materialEditor, properties);
            DrawToonShading(materialEditor, properties);
            DrawLighting(materialEditor, properties);
            DrawFeature(materialEditor, properties, "Rim Light", "_EnableRim", "_RimColor", "_RimPower", "_RimIntensity");
            DrawFeature(materialEditor, properties, "Outline", "_EnableOutline", "_OutlineColor", "_OutlineWidth", "_OutlineDepthOffset");
            materialEditor.RenderQueueField();
            materialEditor.EnableInstancingField();
        }

        private static void DrawSurface(MaterialEditor editor, MaterialProperty[] properties)
        {
            BeginSection("Surface", "The texture and tint applied before toon shading.");
            MaterialProperty baseMap = FindProperty("_MainTex", properties);
            editor.TexturePropertySingleLine(new GUIContent("Base Map", "Optional texture multiplied by the tint and toon colors."), baseMap, FindProperty("_BaseColor", properties));
            editor.TextureScaleOffsetProperty(baseMap);
            EndSection();
        }

        private void DrawToonShading(MaterialEditor editor, MaterialProperty[] properties)
        {
            BeginSection("Toon Shading", "Choose how dark and light areas are created.");
            MaterialProperty enableShading = FindProperty("_EnableBaseShading", properties);
            editor.ShaderProperty(enableShading, new GUIContent("Enabled"));

            if (IsEnabled(enableShading))
            {
                MaterialProperty mappingSource = FindProperty("_MappingSource", properties);
                MaterialProperty colorStyle = FindProperty("_ColorSource", properties);
                DrawMappingSource(editor, mappingSource);
                editor.ShaderProperty(colorStyle, new GUIContent("Color Mode", "Choose a ramp texture, a smooth two-color blend, a hard two-color split, or multiple color steps."));

                int mapping = mappingSource.hasMixedValue ? -1 : Mathf.RoundToInt(mappingSource.floatValue);
                int style = colorStyle.hasMixedValue ? -1 : Mathf.RoundToInt(colorStyle.floatValue);

                if (style == 0 || style == -1)
                {
                    editor.TexturePropertySingleLine(new GUIContent("Color Ramp", "The left edge is dark; the right edge is light."), FindProperty("_ColorRamp", properties));
                }

                if (style >= 1 || style == -1)
                {
                    editor.ShaderProperty(FindProperty("_Color1", properties), new GUIContent("Shadow Color"));
                    editor.ShaderProperty(FindProperty("_Color2", properties), new GUIContent("Lit Color"));
                }

                if (style == 2 || style == -1)
                {
                    EditorGUILayout.HelpBox("Hard Two Color uses one shadow color and one lit color with an adjustable dividing point. Softness controls whether that division is crisp or slightly blended.", MessageType.Info);
                    editor.ShaderProperty(FindProperty("_CelThreshold", properties), new GUIContent("Shadow Size"));
                    editor.ShaderProperty(FindProperty("_CelFeather", properties), new GUIContent("Shadow Softness", "Use zero for a hard edge; increase slightly to reduce aliasing."));
                }

                if (style == 3 || style == -1)
                {
                    EditorGUILayout.HelpBox("Color Steps creates several distinct shade bands like layered paint. Shade Steps sets the number of colors; Step Softness controls how crisp their boundaries are.", MessageType.Info);
                    editor.ShaderProperty(FindProperty("_CelStepCount", properties), new GUIContent("Shade Steps", "Number of evenly spaced shades, including the shadow and lit colors."));
                    editor.ShaderProperty(FindProperty("_CelStepSoftness", properties), new GUIContent("Step Softness", "Zero makes crisp dividing lines; higher values blend across each boundary."));
                }

                if (mapping == 7 || mapping == -1)
                {
                    MaterialProperty mask = FindProperty("_MaskTex", properties);
                    editor.TexturePropertySingleLine(new GUIContent("Mapping Mask", "Uses the red channel."), mask);
                    editor.TextureScaleOffsetProperty(mask);
                }

                if (mapping == 3 || mapping == 4 || mapping == -1)
                {
                    editor.ShaderProperty(FindProperty("_GradientScale", properties), new GUIContent("Height Scale", "Controls how quickly the height gradient changes."));
                }

                showAdvancedShading = EditorGUILayout.Foldout(showAdvancedShading, "Advanced", true);
                if (showAdvancedShading)
                {
                    editor.ShaderProperty(FindProperty("_GradientOffset", properties), new GUIContent("Shading Offset"));
                    editor.ShaderProperty(FindProperty("_BlendSharpness", properties), new GUIContent("Shading Contrast"));
                }
            }
            EndSection();
        }

        private void DrawMappingSource(MaterialEditor editor, MaterialProperty property)
        {
            EditorGUI.showMixedValue = property.hasMixedValue;
            EditorGUI.BeginChangeCheck();

            int currentValue = Mathf.Clamp(Mathf.RoundToInt(property.floatValue), 0, mappingSourceNames.Length - 1);
            int newValue = EditorGUILayout.Popup(
                new GUIContent("Shading Source", "Chooses what drives the dark-to-light value."),
                currentValue,
                mappingSourceNames);

            if (EditorGUI.EndChangeCheck())
            {
                editor.RegisterPropertyChangeUndo("Shading Source");
                property.floatValue = newValue;
            }

            EditorGUI.showMixedValue = false;
        }

        private void DrawLighting(MaterialEditor editor, MaterialProperty[] properties)
        {
            BeginSection("Scene Lighting", "Make the material respond to the Built-in Render Pipeline's main and ambient lights.");
            MaterialProperty enableLighting = FindProperty("_EnableLighting", properties);
            editor.ShaderProperty(enableLighting, new GUIContent("Enabled"));

            if (IsEnabled(enableLighting))
            {
                editor.ShaderProperty(FindProperty("_AmbientStrength", properties), new GUIContent("Ambient Strength"));

                MaterialProperty mappingSource = FindProperty("_MappingSource", properties);
                if (mappingSource.hasMixedValue || mappingSource.floatValue < 0.5f)
                {
                    editor.ShaderProperty(FindProperty("_LightWrap", properties), new GUIContent("Wrap", "Pushes light around the sides of the model."));
                    editor.ShaderProperty(FindProperty("_Smoothness", properties), new GUIContent("Shadow Falloff"));
                    editor.ShaderProperty(FindProperty("_ShadowMultiplier", properties), new GUIContent("Light Strength"));
                    showAdvancedLighting = EditorGUILayout.Foldout(showAdvancedLighting, "Advanced", true);
                    if (showAdvancedLighting)
                    {
                        editor.ShaderProperty(FindProperty("_LightDirection", properties), new GUIContent("Direction Override", "Leave X, Y, and Z at zero to use Unity's main light."));
                    }
                }
            }

            EditorGUILayout.HelpBox("Lightweight mode: receives light direction and ambient light, but not realtime cast shadows.", MessageType.Info);
            EndSection();
        }

        private static void DrawFeature(MaterialEditor editor, MaterialProperty[] properties, string heading, string toggleName, params string[] propertyNames)
        {
            BeginSection(heading, null);
            MaterialProperty toggle = FindProperty(toggleName, properties);
            editor.ShaderProperty(toggle, new GUIContent("Enabled"));

            if (IsEnabled(toggle))
            {
                foreach (string propertyName in propertyNames)
                {
                    MaterialProperty property = FindProperty(propertyName, properties);
                    editor.ShaderProperty(property, property.displayName);
                }
            }

            EndSection();
        }

        private static bool IsEnabled(MaterialProperty toggle)
        {
            return toggle.hasMixedValue || toggle.floatValue >= 0.5f;
        }

        private static void BeginSection(string title, string description)
        {
            EditorGUILayout.Space(3f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            if (!string.IsNullOrEmpty(description))
            {
                EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.Space(2f);
            }
        }

        private static void EndSection()
        {
            EditorGUILayout.EndVertical();
        }
    }
}
