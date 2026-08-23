using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SocketLearningAR
{
    /// <summary>
    /// Listens for AR tracked image events and manages the socket label overlay.
    /// Attach to the XR Origin GameObject (same as ARTrackedImageManager).
    /// </summary>
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class ARImageTracker : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Prefab instantiated when the socket image is detected.")]
        [SerializeField] private GameObject socketOverlayPrefab;

        private ARTrackedImageManager _imageManager;
        private GameObject _spawnedOverlay;

        /// <summary>
        /// True when the tracked image is currently visible and being tracked.
        /// </summary>
        public bool IsTracking { get; private set; }

        /// <summary>
        /// Reference to the spawned overlay so other scripts can access it.
        /// </summary>
        public GameObject SpawnedOverlay => _spawnedOverlay;

        private void Awake()
        {
            _imageManager = GetComponent<ARTrackedImageManager>();
        }

        private void OnEnable()
        {
            _imageManager.trackablesChanged.AddListener(OnTrackablesChanged);
        }

        private void OnDisable()
        {
            _imageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
        }

        private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
        {
            // Handle newly detected images
            foreach (var trackedImage in args.added)
            {
                HandleTrackedImage(trackedImage);
            }

            // Handle updated images (tracking state changes)
            foreach (var trackedImage in args.updated)
            {
                HandleTrackedImage(trackedImage);
            }

            // Handle removed images
            foreach (var kvp in args.removed)
            {
                HandleRemovedImage(kvp.Value);
            }
        }

        private void HandleTrackedImage(ARTrackedImage trackedImage)
        {
            if (trackedImage.trackingState == TrackingState.Tracking ||
                trackedImage.trackingState == TrackingState.Limited)
            {
                if (_spawnedOverlay == null)
                {
                    _spawnedOverlay = Instantiate(socketOverlayPrefab, trackedImage.transform);
                    Debug.Log($"[ARImageTracker] Spawned overlay for image: {trackedImage.referenceImage.name}");
                }
                else
                {
                    // Ensure it's parented correctly if it got detached
                    if (_spawnedOverlay.transform.parent != trackedImage.transform)
                    {
                        _spawnedOverlay.transform.SetParent(trackedImage.transform, false);
                    }
                }

                // Scale overlay to match physical image size
                var imageSize = trackedImage.size; // physical size in meters
                _spawnedOverlay.transform.localPosition = Vector3.zero;
                _spawnedOverlay.transform.localRotation = Quaternion.identity;

                // Pass size to the overlay for label positioning
                var labelManager = _spawnedOverlay.GetComponent<SocketLabelManager>();
                if (labelManager != null)
                {
                    labelManager.UpdateImageSize(imageSize);
                }

                _spawnedOverlay.SetActive(true);
                IsTracking = true;
            }
            else
            {
                // Image lost tracking
                if (_spawnedOverlay != null)
                {
                    _spawnedOverlay.SetActive(false);
                }
                IsTracking = false;
            }
        }

        private void HandleRemovedImage(ARTrackedImage trackedImage)
        {
            if (_spawnedOverlay != null)
            {
                _spawnedOverlay.SetActive(false);
            }
            IsTracking = false;
        }
    }
}
