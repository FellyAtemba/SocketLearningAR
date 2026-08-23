using UnityEngine;
using TMPro;

namespace SocketLearningAR
{
    /// <summary>
    /// Represents a single socket component label in world space.
    /// Handles display, tap detection, and highlight animation.
    /// </summary>
    public class SocketLabel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshPro labelText;
        [SerializeField] private SpriteRenderer backgroundSprite;
        [SerializeField] private SpriteRenderer pinIndicator;

        [Header("Animation")]
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseMinScale = 0.9f;
        [SerializeField] private float pulseMaxScale = 1.1f;

        private SocketData.ComponentInfo _componentInfo;
        private bool _isHighlighted;
        private Vector3 _baseScale;
        private BoxCollider _collider;

        /// <summary>
        /// The component this label represents.
        /// </summary>
        public SocketData.ComponentInfo ComponentInfo => _componentInfo;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _collider = GetComponent<BoxCollider>();
            if (_collider == null)
            {
                _collider = gameObject.AddComponent<BoxCollider>();
                _collider.size = new Vector3(0.04f, 0.02f, 0.001f);
            }
        }

        /// <summary>
        /// Initialize this label with component data.
        /// </summary>
        public void Initialize(SocketData.ComponentInfo info)
        {
            _componentInfo = info;

            if (labelText != null)
            {
                labelText.text = info.DisplayName;
                labelText.color = Color.white;
            }

            if (backgroundSprite != null)
            {
                backgroundSprite.color = info.LabelColor;
            }

            if (pinIndicator != null)
            {
                pinIndicator.color = info.LabelColor;
            }

            gameObject.name = $"Label_{info.DisplayName}";
        }

        /// <summary>
        /// Enable or disable highlight pulsing.
        /// </summary>
        public void SetHighlighted(bool highlighted)
        {
            _isHighlighted = highlighted;
            if (!highlighted)
            {
                transform.localScale = _baseScale;
            }
        }

        private void Update()
        {
            // Billboard: face the camera
            if (Camera.main != null)
            {
                // Look at camera but stay upright
                Vector3 lookDir = Camera.main.transform.position - transform.position;
                lookDir.y = 0; // Keep upright
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(-lookDir, Vector3.up);
                }
            }

            // Pulse animation when highlighted
            if (_isHighlighted)
            {
                float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale,
                    (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
                transform.localScale = _baseScale * scale;
            }
        }
    }
}
