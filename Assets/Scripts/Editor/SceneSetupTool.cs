#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.ARFoundation;

namespace SocketLearningAR.Editor
{
    /// <summary>
    /// Automatically sets up the AR scene with all required UI and component references.
    /// Run via menu: Socket Learning AR > Setup Scene
    /// </summary>
    public static class SceneSetupTool
    {
        [MenuItem("Socket Learning AR/Setup Scene (Full)")]
        public static void SetupScene()
        {
            // 1. Find existing AR objects
            var xrOrigin = Object.FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin == null)
            {
                EditorUtility.DisplayDialog("Error", "No XR Origin found in scene. Please open Socket_AR_Main scene first.", "OK");
                return;
            }

            var imageManager = Object.FindAnyObjectByType<ARTrackedImageManager>();
            if (imageManager == null)
            {
                EditorUtility.DisplayDialog("Error", "No ARTrackedImageManager found. Please check your AR setup.", "OK");
                return;
            }

            // 2. Wire up the image library
            var imageLibrary = AssetDatabase.LoadAssetAtPath<UnityEngine.XR.ARSubsystems.XRReferenceImageLibrary>(
                "Assets/AR_Data/ReferenceImageLibrary.asset");
            if (imageLibrary != null)
            {
                var imSo = new SerializedObject(imageManager);
                imSo.FindProperty("m_SerializedLibrary").objectReferenceValue = imageLibrary;
                imSo.ApplyModifiedProperties();
                Debug.Log("[SceneSetup] Wired up ReferenceImageLibrary to ARTrackedImageManager");
            }

            // 3. Add ARImageTracker to XR Origin
            var tracker = xrOrigin.GetComponent<ARImageTracker>();
            if (tracker == null)
            {
                tracker = xrOrigin.gameObject.AddComponent<ARImageTracker>();
            }

            // Wire up the overlay prefab
            var overlayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SocketOverlay.prefab");
            if (overlayPrefab != null)
            {
                var trackerSo = new SerializedObject(tracker);
                trackerSo.FindProperty("socketOverlayPrefab").objectReferenceValue = overlayPrefab;
                trackerSo.ApplyModifiedProperties();
            }

            // 4. Create UI Canvas
            var existingCanvas = Object.FindAnyObjectByType<Canvas>();
            GameObject canvasObj;
            if (existingCanvas != null)
            {
                canvasObj = existingCanvas.gameObject;
            }
            else
            {
                canvasObj = new GameObject("UICanvas");
                var canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 10;
                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
                canvasObj.GetComponent<CanvasScaler>().matchWidthOrHeight = 0.5f;
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Ensure EventSystem exists
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // 5. Create UI hierarchy
            // -- Top Panel --
            var topPanel = CreatePanel(canvasObj.transform, "TopPanel",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -10), new Vector2(0, -200),
                new Color(0.1f, 0.1f, 0.15f, 0.9f));

            var titleTMP = CreateTMPText(topPanel.transform, "TitleText",
                "Welcome!", 36, TextAlignmentOptions.Center, Color.white,
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -10), new Vector2(-20, -60));

            var instructionTMP = CreateTMPText(topPanel.transform, "InstructionText",
                "Point camera at socket", 22, TextAlignmentOptions.Center, new Color(0.8f, 0.8f, 0.85f),
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(20, -65), new Vector2(-20, -10));

            var stepCounterTMP = CreateTMPText(topPanel.transform, "StepCounter",
                "1 / 5", 18, TextAlignmentOptions.TopRight, new Color(0.6f, 0.6f, 0.65f),
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-80, -10), new Vector2(-20, -40));

            // -- Info Panel (hidden by default) --
            var infoPanel = CreatePanel(canvasObj.transform, "InfoPanel",
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(20, 160), new Vector2(-20, 420),
                new Color(0.15f, 0.15f, 0.2f, 0.95f));

            // Accent bar on left side of info panel
            var infoAccent = CreateImage(infoPanel.transform, "InfoAccent",
                Color.white,
                new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(6, 0));

            var infoTitleTMP = CreateTMPText(infoPanel.transform, "InfoTitle",
                "Component Name", 28, TextAlignmentOptions.TopLeft, Color.white,
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -10), new Vector2(-20, -50));

            var infoBodyTMP = CreateTMPText(infoPanel.transform, "InfoBody",
                "Description...", 20, TextAlignmentOptions.TopLeft, new Color(0.85f, 0.85f, 0.9f),
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(20, 10), new Vector2(-20, -55));

            // -- Scanning Prompt --
            var scanPrompt = CreatePanel(canvasObj.transform, "ScanningPrompt",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-200, -60), new Vector2(200, 60),
                new Color(0.1f, 0.1f, 0.15f, 0.85f));

            CreateTMPText(scanPrompt.transform, "ScanText",
                "📱 Scan a socket or reference image", 24, TextAlignmentOptions.Center, Color.white,
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(10, 10), new Vector2(-10, -10));

            // -- Next Button --
            var nextBtnObj = CreateButton(canvasObj.transform, "NextButton",
                "Start ▶", new Color(0.2f, 0.6f, 0.9f, 1f),
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-140, 80), new Vector2(140, 140));

            // -- Completion Panel (hidden by default) --
            var completionPanel = CreatePanel(canvasObj.transform, "CompletionPanel",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                new Color(0.05f, 0.1f, 0.15f, 0.95f));

            CreateTMPText(completionPanel.transform, "CompletionEmoji",
                "🎉", 72, TextAlignmentOptions.Center, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-100, 40), new Vector2(100, 140));

            CreateTMPText(completionPanel.transform, "CompletionTitle",
                "Well Done!", 42, TextAlignmentOptions.Center, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-200, -20), new Vector2(200, 40));

            CreateTMPText(completionPanel.transform, "CompletionBody",
                "You've successfully learned about all three parts of a UK Type G electrical socket:\n\n" +
                "🟢 Earth — Safety ground\n🔴 Live — Power supply\n🔵 Neutral — Return path\n\n" +
                "Stay curious and keep learning!",
                22, TextAlignmentOptions.Center, new Color(0.8f, 0.8f, 0.85f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-250, -200), new Vector2(250, -30));

            var restartBtnObj = CreateButton(completionPanel.transform, "RestartButton",
                "🔄 Try Again", new Color(0.2f, 0.7f, 0.5f, 1f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-120, -280), new Vector2(120, -220));

            // 6. Create GameManager and wire up LearningFlowManager
            var gameManagerObj = GameObject.Find("GameManager");
            if (gameManagerObj == null)
            {
                gameManagerObj = new GameObject("GameManager");
            }

            var flowManager = gameManagerObj.GetComponent<LearningFlowManager>();
            if (flowManager == null)
            {
                flowManager = gameManagerObj.AddComponent<LearningFlowManager>();
            }

            // Wire up all references
            var fmSo = new SerializedObject(flowManager);
            fmSo.FindProperty("imageTracker").objectReferenceValue = tracker;
            fmSo.FindProperty("titleText").objectReferenceValue = titleTMP;
            fmSo.FindProperty("instructionText").objectReferenceValue = instructionTMP;
            fmSo.FindProperty("stepCounterText").objectReferenceValue = stepCounterTMP;
            fmSo.FindProperty("infoPanel").objectReferenceValue = infoPanel;
            fmSo.FindProperty("infoTitleText").objectReferenceValue = infoTitleTMP;
            fmSo.FindProperty("infoBodyText").objectReferenceValue = infoBodyTMP;
            fmSo.FindProperty("infoPanelAccent").objectReferenceValue = infoAccent.GetComponent<Image>();
            fmSo.FindProperty("nextButton").objectReferenceValue = nextBtnObj.GetComponent<Button>();
            fmSo.FindProperty("nextButtonText").objectReferenceValue = nextBtnObj.GetComponentInChildren<TextMeshProUGUI>();
            fmSo.FindProperty("completionPanel").objectReferenceValue = completionPanel;
            fmSo.FindProperty("restartButton").objectReferenceValue = restartBtnObj.GetComponent<Button>();
            fmSo.FindProperty("scanningPrompt").objectReferenceValue = scanPrompt;
            fmSo.ApplyModifiedProperties();

            // 7. Update build settings
            var currentScene = EditorSceneManager.GetActiveScene();
            var buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(currentScene.path, true)
            };
            EditorBuildSettings.scenes = buildScenes;

            // Mark scene dirty
            EditorSceneManager.MarkSceneDirty(currentScene);

            Debug.Log("[SceneSetup] Scene setup complete!");
            EditorUtility.DisplayDialog("Scene Setup Complete",
                "The Socket AR Learning scene has been set up!\n\n" +
                "Please:\n" +
                "1. Run 'Socket Learning AR > Build Prefabs' first if you haven't.\n" +
                "2. Save the scene (Ctrl+S).\n" +
                "3. Configure the Reference Image Library with your reference image in the Inspector.\n" +
                "4. Test in Play mode or build for Android.",
                "OK");
        }

        private static GameObject CreatePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            Color bgColor)
        {
            var panelObj = new GameObject(name);
            panelObj.transform.SetParent(parent, false);

            var rect = panelObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var image = panelObj.AddComponent<Image>();
            image.color = bgColor;

            return panelObj;
        }

        private static TextMeshProUGUI CreateTMPText(Transform parent, string name,
            string text, float fontSize, TextAlignmentOptions alignment, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            var rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Ellipsis;

            return tmp;
        }

        private static GameObject CreateImage(Transform parent, string name,
            Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var imgObj = new GameObject(name);
            imgObj.transform.SetParent(parent, false);

            var rect = imgObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var image = imgObj.AddComponent<Image>();
            image.color = color;

            return imgObj;
        }

        private static GameObject CreateButton(Transform parent, string name,
            string text, Color bgColor,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var image = btnObj.AddComponent<Image>();
            image.color = bgColor;

            var button = btnObj.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = bgColor * 1.1f;
            colors.pressedColor = bgColor * 0.8f;
            colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            button.colors = colors;

            // Button text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 26;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return btnObj;
        }
    }
}
#endif
