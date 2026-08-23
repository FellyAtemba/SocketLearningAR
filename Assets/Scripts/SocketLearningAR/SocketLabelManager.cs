using UnityEngine;
using System.Collections.Generic;

namespace SocketLearningAR
{
    /// <summary>
    /// Creates and positions world-space labels for each socket component.
    /// Attach to the root of the socket overlay prefab.
    /// </summary>
    public class SocketLabelManager : MonoBehaviour
    {
        [Header("Label Settings")]
        [Tooltip("Prefab for individual socket component labels.")]
        [SerializeField] private GameObject labelPrefab;
        [Tooltip("Height offset above the tracked image surface.")]
        [SerializeField] private float labelHeight = 0.02f;

        private readonly Dictionary<SocketData.SocketComponent, SocketLabel> _labels =
            new Dictionary<SocketData.SocketComponent, SocketLabel>();
        private Vector2 _imageSize = new Vector2(0.08f, 0.08f); // default ~8cm

        /// <summary>
        /// Called by ARImageTracker when the tracked image size is known.
        /// </summary>
        public void UpdateImageSize(Vector2 size)
        {
            _imageSize = size;
            RepositionLabels();
        }

        private void Start()
        {
            CreateLabels();
        }

        private void CreateLabels()
        {
            foreach (var componentInfo in SocketData.Components)
            {
                if (labelPrefab == null)
                {
                    Debug.LogError("[SocketLabelManager] labelPrefab is not assigned!");
                    return;
                }

                var labelObj = Instantiate(labelPrefab, transform);
                var label = labelObj.GetComponent<SocketLabel>();
                if (label != null)
                {
                    label.Initialize(componentInfo);
                    _labels[componentInfo.Component] = label;
                }
                else
                {
                    Debug.LogError($"[SocketLabelManager] Label prefab is missing SocketLabel component!");
                }
            }

            RepositionLabels();
        }

        private void RepositionLabels()
        {
            foreach (var kvp in _labels)
            {
                var info = SocketData.GetComponentInfo(kvp.Key);
                if (info != null)
                {
                    // Convert normalized offset to world-space offset
                    // AR tracked images have X = right, Y = up (normal), Z = forward (into image)
                    // The image lies on the XZ plane, so offset X maps to local X, offset Y maps to local Z
                    float worldX = info.NormalizedOffset.x * _imageSize.x;
                    float worldZ = -info.NormalizedOffset.y * _imageSize.y; // negative because Z is forward/into image
                    kvp.Value.transform.localPosition = new Vector3(worldX, labelHeight, worldZ);
                }
            }
        }

        /// <summary>
        /// Highlights a specific component label (pulses/scales it).
        /// </summary>
        public void HighlightComponent(SocketData.SocketComponent? component)
        {
            foreach (var kvp in _labels)
            {
                kvp.Value.SetHighlighted(component.HasValue && kvp.Key == component.Value);
            }
        }

        /// <summary>
        /// Shows or hides all labels.
        /// </summary>
        public void SetLabelsVisible(bool visible)
        {
            foreach (var kvp in _labels)
            {
                kvp.Value.gameObject.SetActive(visible);
            }
        }

        /// <summary>
        /// Gets the SocketLabel for a given component.
        /// </summary>
        public SocketLabel GetLabel(SocketData.SocketComponent component)
        {
            _labels.TryGetValue(component, out var label);
            return label;
        }
    }
}
