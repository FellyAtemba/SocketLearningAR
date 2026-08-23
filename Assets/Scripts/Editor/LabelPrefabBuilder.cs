#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;

namespace SocketLearningAR.Editor
{
    /// <summary>
    /// Editor utility to generate prefabs needed for the Socket Learning AR app.
    /// Run via menu: Socket Learning AR > Build Prefabs
    /// </summary>
    public static class LabelPrefabBuilder
    {
        private const string PrefabFolder = "Assets/Prefabs";

        [MenuItem("Socket Learning AR/Build Prefabs")]
        public static void BuildAllPrefabs()
        {
            // Ensure folders exist
            if (!AssetDatabase.IsValidFolder(PrefabFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            BuildLabelPrefab();
            BuildSocketOverlayPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[LabelPrefabBuilder] All prefabs built successfully!");
            EditorUtility.DisplayDialog("Prefabs Built",
                "Label and Socket Overlay prefabs have been created in Assets/Prefabs/.\n\n" +
                "Next steps:\n" +
                "1. Assign SocketOverlay prefab's LabelPrefab field to the SocketLabel prefab.\n" +
                "2. Assign the SocketOverlay prefab to ARImageTracker's socketOverlayPrefab field.\n" +
                "3. Set up the ReferenceImageLibrary with your reference image.",
                "OK");
        }

        private static void BuildLabelPrefab()
        {
            // Create label root
            var labelRoot = new GameObject("SocketLabel");

            // Add SocketLabel component
            var socketLabel = labelRoot.AddComponent<SocketLabel>();

            // Add BoxCollider for tap detection
            var collider = labelRoot.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.04f, 0.015f, 0.001f);

            // Create background quad
            var bgObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bgObj.name = "Background";
            bgObj.transform.SetParent(labelRoot.transform, false);
            bgObj.transform.localScale = new Vector3(0.04f, 0.015f, 1f);
            bgObj.transform.localPosition = Vector3.zero;

            // Remove the default collider from quad
            var bgCollider = bgObj.GetComponent<Collider>();
            if (bgCollider != null) Object.DestroyImmediate(bgCollider);

            // Create and assign material for background
            var bgMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            bgMaterial.color = new Color(0.2f, 0.2f, 0.2f, 0.85f);
            // Enable transparency
            bgMaterial.SetFloat("_Surface", 1); // Transparent
            bgMaterial.SetFloat("_Blend", 0); // Alpha
            bgMaterial.SetOverrideTag("RenderType", "Transparent");
            bgMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            bgMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            bgMaterial.SetInt("_ZWrite", 0);
            bgMaterial.DisableKeyword("_ALPHATEST_ON");
            bgMaterial.EnableKeyword("_ALPHABLEND_ON");
            bgMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            bgMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            AssetDatabase.CreateAsset(bgMaterial, "Assets/Materials/LabelBackground.mat");
            bgObj.GetComponent<Renderer>().material = bgMaterial;

            // Create accent bar (color indicator)
            var accentObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            accentObj.name = "AccentBar";
            accentObj.transform.SetParent(labelRoot.transform, false);
            accentObj.transform.localScale = new Vector3(0.003f, 0.015f, 1f);
            accentObj.transform.localPosition = new Vector3(-0.0185f, 0f, -0.0001f);

            var accentCollider = accentObj.GetComponent<Collider>();
            if (accentCollider != null) Object.DestroyImmediate(accentCollider);

            var accentMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            accentMaterial.color = Color.white;
            AssetDatabase.CreateAsset(accentMaterial, "Assets/Materials/LabelAccent.mat");
            accentObj.GetComponent<Renderer>().material = accentMaterial;

            // Create TextMeshPro label
            var textObj = new GameObject("LabelText");
            textObj.transform.SetParent(labelRoot.transform, false);
            textObj.transform.localPosition = new Vector3(0.002f, 0f, -0.0002f);

            var tmp = textObj.AddComponent<TextMeshPro>();
            tmp.text = "Label";
            tmp.fontSize = 1.5f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableAutoSizing = false;
            tmp.rectTransform.sizeDelta = new Vector2(0.035f, 0.012f);
            tmp.overflowMode = TextOverflowModes.Ellipsis;

            // Wire up serialized fields via SerializedObject
            var so = new SerializedObject(socketLabel);
            so.FindProperty("labelText").objectReferenceValue = tmp;
            so.FindProperty("backgroundSprite").objectReferenceValue = null; // We use a Renderer instead
            so.FindProperty("pinIndicator").objectReferenceValue = null;
            so.ApplyModifiedProperties();

            // Save as prefab
            var prefabPath = $"{PrefabFolder}/SocketLabel.prefab";
            PrefabUtility.SaveAsPrefabAsset(labelRoot, prefabPath);
            Object.DestroyImmediate(labelRoot);

            Debug.Log($"[LabelPrefabBuilder] Created label prefab at {prefabPath}");
        }

        private static void BuildSocketOverlayPrefab()
        {
            // Create overlay root
            var overlayRoot = new GameObject("SocketOverlay");

            // Add SocketLabelManager
            var labelManager = overlayRoot.AddComponent<SocketLabelManager>();

            // Load the label prefab we just created
            var labelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/SocketLabel.prefab");

            // Wire up the label prefab reference
            var so = new SerializedObject(labelManager);
            so.FindProperty("labelPrefab").objectReferenceValue = labelPrefab;
            so.FindProperty("labelHeight").floatValue = 0.005f;
            so.ApplyModifiedProperties();

            // Save as prefab
            var prefabPath = $"{PrefabFolder}/SocketOverlay.prefab";
            PrefabUtility.SaveAsPrefabAsset(overlayRoot, prefabPath);
            Object.DestroyImmediate(overlayRoot);

            Debug.Log($"[LabelPrefabBuilder] Created overlay prefab at {prefabPath}");
        }
    }
}
#endif
