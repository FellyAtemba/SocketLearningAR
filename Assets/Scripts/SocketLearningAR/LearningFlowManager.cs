using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SocketLearningAR
{
    /// <summary>
    /// Manages the learning flow: step progression, UI updates, and component selection.
    /// Attach to a persistent GameObject in the scene (e.g., a "GameManager" object).
    /// </summary>
    public class LearningFlowManager : MonoBehaviour
    {
        [Header("AR References")]
        [SerializeField] private ARImageTracker imageTracker;

        [Header("UI - Top Panel")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI stepCounterText;

        [Header("UI - Info Panel")]
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private TextMeshProUGUI infoTitleText;
        [SerializeField] private TextMeshProUGUI infoBodyText;
        [SerializeField] private Image infoPanelAccent;

        [Header("UI - Bottom Buttons")]
        [SerializeField] private Button nextButton;
        [SerializeField] private TextMeshProUGUI nextButtonText;

        [Header("UI - Completion Screen")]
        [SerializeField] private GameObject completionPanel;
        [SerializeField] private Button restartButton;

        [Header("UI - Scanning Prompt")]
        [SerializeField] private GameObject scanningPrompt;

        private int _currentStep = 0;
        private SocketLabelManager _labelManager;
        private bool _hasSelectedComponent;

        private void Start()
        {
            // Setup button listeners
            if (nextButton != null)
                nextButton.onClick.AddListener(OnNextButtonClicked);
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            // Initialize
            if (completionPanel != null)
                completionPanel.SetActive(false);
            if (infoPanel != null)
                infoPanel.SetActive(false);

            UpdateUI();
        }

        private void Update()
        {
            // Check for label manager when tracking starts
            if (_labelManager == null && imageTracker != null && imageTracker.SpawnedOverlay != null)
            {
                _labelManager = imageTracker.SpawnedOverlay.GetComponent<SocketLabelManager>();
                if (_labelManager != null)
                {
                    UpdateHighlight();
                }
            }

            // Show/hide scanning prompt
            bool isTracking = imageTracker != null && imageTracker.IsTracking;
            if (scanningPrompt != null)
            {
                scanningPrompt.SetActive(!isTracking && _currentStep < SocketData.Steps.Length - 1);
            }

            // Handle touch/click input for label selection
            HandleInput();
        }

        private void HandleInput()
        {
            // Check for touch or mouse click
            bool inputDetected = false;
            Vector2 inputPosition = Vector2.zero;

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    inputDetected = true;
                    inputPosition = touch.position;
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                inputDetected = true;
                inputPosition = Input.mousePosition;
            }

            if (!inputDetected || Camera.main == null) return;

            // Raycast from screen point
            Ray ray = Camera.main.ScreenPointToRay(inputPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                var label = hit.collider.GetComponent<SocketLabel>();
                if (label != null)
                {
                    OnLabelTapped(label);
                }
            }
        }

        private void OnLabelTapped(SocketLabel label)
        {
            if (label.ComponentInfo == null) return;

            var step = GetCurrentStep();
            if (step == null) return;

            // Show info panel for the tapped component
            ShowComponentInfo(label.ComponentInfo);

            // Check if this is the correct component for the current step
            if (step.HighlightComponent.HasValue &&
                label.ComponentInfo.Component == step.HighlightComponent.Value)
            {
                _hasSelectedComponent = true;
                UpdateNextButtonState();
            }
        }

        private void ShowComponentInfo(SocketData.ComponentInfo info)
        {
            if (infoPanel == null) return;

            infoPanel.SetActive(true);

            if (infoTitleText != null)
                infoTitleText.text = info.DisplayName;
            if (infoBodyText != null)
                infoBodyText.text = info.DetailedExplanation;
            if (infoPanelAccent != null)
                infoPanelAccent.color = info.LabelColor;
        }

        private void OnNextButtonClicked()
        {
            _currentStep++;
            _hasSelectedComponent = false;

            if (infoPanel != null)
                infoPanel.SetActive(false);

            if (_currentStep >= SocketData.Steps.Length - 1)
            {
                ShowCompletionScreen();
            }

            UpdateUI();
        }

        private void OnRestartClicked()
        {
            _currentStep = 0;
            _hasSelectedComponent = false;

            if (completionPanel != null)
                completionPanel.SetActive(false);
            if (infoPanel != null)
                infoPanel.SetActive(false);

            UpdateUI();
        }

        private void ShowCompletionScreen()
        {
            if (completionPanel != null)
                completionPanel.SetActive(true);
        }

        private void UpdateUI()
        {
            var step = GetCurrentStep();
            if (step == null) return;

            if (titleText != null)
                titleText.text = step.Title;
            if (instructionText != null)
                instructionText.text = step.Instruction;
            if (stepCounterText != null)
                stepCounterText.text = $"{_currentStep + 1} / {SocketData.Steps.Length}";

            UpdateHighlight();
            UpdateNextButtonState();
        }

        private void UpdateHighlight()
        {
            var step = GetCurrentStep();
            if (_labelManager != null && step != null)
            {
                _labelManager.HighlightComponent(step.HighlightComponent);
            }
        }

        private void UpdateNextButtonState()
        {
            if (nextButton == null) return;

            var step = GetCurrentStep();
            if (step == null) return;

            bool isLastStep = _currentStep >= SocketData.Steps.Length - 1;

            // On welcome step (0) and completion step, the Next button advances freely
            // On learning steps (1-3), require tapping the correct component first
            if (isLastStep)
            {
                nextButton.gameObject.SetActive(false);
            }
            else if (step.HighlightComponent.HasValue)
            {
                nextButton.gameObject.SetActive(true);
                nextButton.interactable = _hasSelectedComponent;
                if (nextButtonText != null)
                    nextButtonText.text = _hasSelectedComponent ? "Next ▶" : "Tap the label first";
            }
            else
            {
                nextButton.gameObject.SetActive(true);
                nextButton.interactable = true;
                if (nextButtonText != null)
                    nextButtonText.text = _currentStep == 0 ? "Start ▶" : "Next ▶";
            }
        }

        private SocketData.LearningStep GetCurrentStep()
        {
            if (_currentStep >= 0 && _currentStep < SocketData.Steps.Length)
                return SocketData.Steps[_currentStep];
            return null;
        }
    }
}
