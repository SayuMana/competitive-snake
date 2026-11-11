using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CompetitiveSnake.EditorTools
{
    public static class SnakeSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Main.unity";
        const string DataDir = "Assets/Data";
        const string ConfigPath = "Assets/Data/SnakeConfig.asset";
        const string PrefKey = "CompetitiveSnake.BuilderVersion";
        const int BuildVersion = 7;

        static readonly Color TextColor = new Color(0.95f, 0.96f, 1f);
        static readonly Color HintColor = new Color(0.62f, 0.66f, 0.75f);

        [MenuItem("Tools/Competitive Snake/Build Main Scene")]
        public static void BuildFromMenu()
        {
            if (Build()) EditorPrefs.SetInt(PrefKey, BuildVersion);
        }

        public static void BuildSceneAndWebGL()
        {
            if (Build()) EditorPrefs.SetInt(PrefKey, BuildVersion);
            WebGLBuilder.Build();
        }

        [InitializeOnLoadMethod]
        static void AutoBuild()
        {
            if (EditorPrefs.GetInt(PrefKey, 0) >= BuildVersion) return;

            EditorApplication.delayCall += () =>
            {
                if (Build()) EditorPrefs.SetInt(PrefKey, BuildVersion);
                else Debug.LogWarning("[SnakeBuilder] build failed, will retry after next reload");
            };
        }

        static bool Build()
        {
            try
            {
                Debug.Log("[SnakeBuilder] start");

                EnsureFolder(DataDir);
                SnakeConfig config = LoadOrCreateConfig();

                EditorSceneManager.SaveOpenScenes();
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                BuildCamera(config);
                BoardView view = BuildBoard(config);
                HudView hud = BuildHud(config);
                BuildMatch(config, view, hud);
                BuildEventSystem();

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, ScenePath);
                AddSceneToBuildSettings();
                ApplyWebGlSettings();
                AssetDatabase.SaveAssets();

                Debug.Log("[SnakeBuilder] OK roots=" + scene.rootCount + " scene=" + ScenePath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("[SnakeBuilder] FAILED " + e);
                return false;
            }
        }

        // --- Scene objects ---

        static void BuildCamera(SnakeConfig config)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";

            var camera = go.AddComponent<UnityEngine.Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 18f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Palette.Background;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 50f;

            go.transform.position = new Vector3(0f, 0f, -10f);

            var fitter = go.AddComponent<SnakeCameraFitter>();
            Wire(fitter, "config", config);
        }

        static BoardView BuildBoard(SnakeConfig config)
        {
            var go = new GameObject("Board");
            var view = go.AddComponent<BoardView>();
            Wire(view, "config", config);
            return view;
        }

        static void BuildMatch(SnakeConfig config, BoardView view, HudView hud)
        {
            var go = new GameObject("Match");
            var match = go.AddComponent<MatchController>();
            Wire(match, "config", config);
            Wire(match, "view", view);
            Wire(match, "hud", hud);
        }

        static void BuildEventSystem()
        {
            var go = new GameObject("EventSystem");
            var eventSystem = go.AddComponent<EventSystem>();
            eventSystem.sendNavigationEvents = false;
            go.AddComponent<StandaloneInputModule>();
        }

        // --- UI ---

        static HudView BuildHud(SnakeConfig config)
        {
            var canvasGo = new GameObject("HUD");

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(690f, 900f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            var hud = canvasGo.AddComponent<HudView>();

            Text timeText = CreateText(canvasGo.transform, "Time", "60", 56, TextAnchor.UpperCenter,
                new Vector2(0f, -34f), new Vector2(300f, 90f), TextColor, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

            Text lengthText = CreateWideText(canvasGo.transform, "Lengths", "YOU 4   BOT 4", 26, 1f, -92f, 50f, HintColor, TextAnchor.UpperCenter);

            var padArea = CreatePadArea(canvasGo.transform, config.bottomUiFraction);

            CreateDpadButton(padArea, "PadUp", new Vector2(0f, 96f), Vector2Int.up, "\u25B2");
            CreateDpadButton(padArea, "PadLeft", new Vector2(-96f, 0f), Vector2Int.left, "\u25C0");
            CreateDpadButton(padArea, "PadRight", new Vector2(96f, 0f), Vector2Int.right, "\u25B6");
            CreateDpadButton(padArea, "PadDown", new Vector2(0f, -96f), Vector2Int.down, "\u25BC");

            GameObject readyPanel = CreatePanel(canvasGo.transform, "ReadyPanel", new Color(0f, 0f, 0f, 0.82f));
            CreateWideText(readyPanel.transform, "Title", "COMPETITIVE SNAKE", 62, 0.5f, 130f, 100f, TextColor, TextAnchor.MiddleCenter);
            CreateWideText(readyPanel.transform, "Start", "TAP TO START", 40, 0.5f, 20f, 70f, TextColor, TextAnchor.MiddleCenter);
            CreateWideText(readyPanel.transform, "Hint", "WASD / ARROWS / D-PAD", 26, 0.5f, -60f, 50f, HintColor, TextAnchor.MiddleCenter);

            GameObject resultPanel = CreatePanel(canvasGo.transform, "ResultPanel", new Color(0f, 0f, 0f, 0.84f));
            Text resultText = CreateWideText(resultPanel.transform, "Result", "YOU WIN", 72, 0.5f, 110f, 110f, TextColor, TextAnchor.MiddleCenter);
            Text detailText = CreateWideText(resultPanel.transform, "Detail", "YOU 4   BOT 4", 34, 0.5f, 20f, 60f, TextColor, TextAnchor.MiddleCenter);
            CreateWideText(resultPanel.transform, "Restart", "TAP TO RESTART", 30, 0.5f, -60f, 50f, HintColor, TextAnchor.MiddleCenter);

            resultPanel.SetActive(false);

            Wire(hud, "timeText", timeText);
            Wire(hud, "lengthText", lengthText);
            Wire(hud, "readyPanel", readyPanel);
            Wire(hud, "resultPanel", resultPanel);
            Wire(hud, "resultText", resultText);
            Wire(hud, "resultDetailText", detailText);

            return hud;
        }

        static Vector2 Center => new Vector2(0.5f, 0.5f);

        static void CreateDpadButton(Transform parent, string name, Vector2 position, Vector2Int direction, string glyph)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Center;
            rect.anchorMax = Center;
            rect.pivot = Center;
            rect.sizeDelta = new Vector2(92f, 92f);
            rect.anchoredPosition = position;

            var image = go.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.10f);

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;

            CreateText(go.transform, "Glyph", glyph, 40, TextAnchor.MiddleCenter,
                Vector2.zero, new Vector2(92f, 92f), TextColor, Center, Center);

            var dpad = go.AddComponent<DpadButton>();
            SetVector2Int(dpad, "direction", direction);
        }

        static Text CreateWideText(Transform parent, string name, string content, int size, float anchorY,
            float offsetY, float height, Color color, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<Text>();
            text.font = BuiltinFont();
            text.text = content;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(12, size / 2);
            text.resizeTextMaxSize = size;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, anchorY);
            rect.anchorMax = new Vector2(1f, anchorY);
            rect.pivot = Center;
            rect.sizeDelta = new Vector2(-80f, height);
            rect.anchoredPosition = new Vector2(0f, offsetY);
            return text;
        }

        static RectTransform CreatePadArea(Transform parent, float bottomFraction)
        {
            var go = new GameObject("PadArea", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, Mathf.Clamp01(bottomFraction));
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.color = color;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return go;
        }

        static Text CreateText(Transform parent, string name, string content, int size, TextAnchor alignment,
            Vector2 position, Vector2 boxSize, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<Text>();
            text.font = BuiltinFont();
            text.text = content;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = Center;
            rect.sizeDelta = boxSize;
            rect.anchoredPosition = position;
            return text;
        }

        static Font BuiltinFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null) Debug.LogError("[SnakeBuilder] builtin font not found");
            return font;
        }

        // --- Assets and settings ---

        static SnakeConfig LoadOrCreateConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<SnakeConfig>(ConfigPath);
            if (existing != null) return existing;

            var created = ScriptableObject.CreateInstance<SnakeConfig>();
            AssetDatabase.CreateAsset(created, ConfigPath);
            Debug.Log("[SnakeBuilder] created " + ConfigPath);
            return created;
        }

        static void AddSceneToBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            Debug.Log("[SnakeBuilder] build settings scenes=" + EditorBuildSettings.scenes.Length);
        }

        static void ApplyWebGlSettings()
        {
            PlayerSettings.companyName = "SayuMana";
            PlayerSettings.productName = "Competitive Snake";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.High);
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.WebGL, Il2CppCompilerConfiguration.Release);
            Debug.Log("[SnakeBuilder] webgl settings applied");
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parent)) parent = parent.Replace("\\", "/");
            string name = Path.GetFileName(path);

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        static void Wire(UnityEngine.Object target, string fieldName, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(fieldName);

            if (property == null)
            {
                Debug.LogError("[SnakeBuilder] missing field " + target.GetType().Name + "." + fieldName);
                return;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetVector2Int(UnityEngine.Object target, string fieldName, Vector2Int value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(fieldName);

            if (property == null)
            {
                Debug.LogError("[SnakeBuilder] missing field " + target.GetType().Name + "." + fieldName);
                return;
            }

            property.vector2IntValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
