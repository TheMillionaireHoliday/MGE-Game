using UnityEditor;
using UnityEngine;

namespace HaniJahanDesign.ToonShaderPack
{
    public class MyAssetWelcome : EditorWindow
    {
        private const string HeaderImagePath = "Assets/HaniJahanDesign/Shaders/Toon/HJD_ReferenceImage.png";
        private const string DocumentationUrl = "https://hanijahan.com/products/unity/toon-shader/#documentation";
        private const string ChangelogUrl = "https://hanijahan.com/products/unity/toon-shader/#what-s-new-changelog";
        private const string SupportUrl = "https://discord.gg/xpcfCyaycx";
        private const string ReviewUrl = "https://assetstore.unity.com/packages/3d/392094#reviews";
        private const string WindowTitle = "Toon Shader Pack";
        private const string Version = "1.0.1";

        private Texture2D headerTexture;
        private Vector2 scrollPosition;

        [MenuItem("Window/My Assets/Hani Jahan Design/Toon Shader Pack/Show Welcome")]
        public static void ShowWindow()
        {
            var window = GetWindow<MyAssetWelcome>(WindowTitle);
            window.titleContent = new GUIContent(WindowTitle);
            window.Show();
        }

        private void OnEnable()
        {
            headerTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(HeaderImagePath);
        }

        private void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            DrawHeaderImage();
            GUILayout.Space(20);

            DrawIntroSection();
            DrawSupportSection();
            DrawReviewSection();
            DrawResourcesSection();
            DrawDiscordSection();

            GUILayout.FlexibleSpace();
            GUILayout.Label("Made by Hani Jahan Design", EditorStyles.centeredGreyMiniLabel);

            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawHeaderImage()
        {
            if (headerTexture == null)
            {
                return;
            }

            float aspect = (float)headerTexture.width / headerTexture.height;
            float width = Mathf.Max(0f, position.width - 40f);
            float height = width / aspect;
            Rect rect = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(rect, headerTexture, ScaleMode.ScaleAndCrop);
        }

        private static void DrawIntroSection()
        {
            GUILayout.Label("Welcome!", EditorStyles.boldLabel);
            GUILayout.Label(
                "Thanks for choosing Toon Shader Pack! This pack includes Built-in and URP toon shader variants, ready-made materials, demo scenes, and helper notes to get your toon rendering set up quickly.",
                EditorStyles.wordWrappedLabel
            );
            GUILayout.Space(5);
            GUILayout.Label($"Version: {Version}", EditorStyles.label);
            GUILayout.Space(15);
        }

        private static void DrawSupportSection()
        {
            GUILayout.Label(
                "Need help or want to suggest a feature? We're here for you!",
                EditorStyles.wordWrappedLabel
            );
            DrawLinkButton("Get Support", SupportUrl);
            GUILayout.Space(15);
        }

        private static void DrawReviewSection()
        {
            GUILayout.Label(
                "Enjoying this asset? A review helps us keep improving and means a lot to us. Thank you!",
                EditorStyles.wordWrappedLabel
            );
            DrawLinkButton("Leave a Review", ReviewUrl);
            GUILayout.Space(15);
        }

        private static void DrawResourcesSection()
        {
            GUILayout.Label(
                "Quick links to help you get started:",
                EditorStyles.wordWrappedLabel
            );
            DrawLinkButton("What's New", ChangelogUrl);
            DrawLinkButton("Documentation", DocumentationUrl);
            GUILayout.Space(15);
        }

        private static void DrawDiscordSection()
        {
            GUILayout.Label(
                "Join our Discord! Show off what you're making, and connect with other creators",
                EditorStyles.wordWrappedLabel
            );
            DrawLinkButton("Join Discord", SupportUrl);
        }

        private static void DrawLinkButton(string label, string url)
        {
            if (GUILayout.Button(label))
            {
                Application.OpenURL(url);
            }
        }
    }
}
