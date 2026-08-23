using UnityEngine;

namespace SocketLearningAR
{
    /// <summary>
    /// Static data definitions for socket components and learning steps.
    /// </summary>
    public static class SocketData
    {
        public enum SocketComponent
        {
            Earth,
            Live,
            Neutral
        }

        [System.Serializable]
        public class ComponentInfo
        {
            public SocketComponent Component;
            public string DisplayName;
            public string ShortDescription;
            public string DetailedExplanation;
            public Color LabelColor;
            /// <summary>
            /// Normalized position offset relative to the tracked image center.
            /// X: left(-) / right(+), Y: up(+) / down(-)
            /// These will be multiplied by the tracked image physical size.
            /// </summary>
            public Vector2 NormalizedOffset;
        }

        [System.Serializable]
        public class LearningStep
        {
            public string Title;
            public string Instruction;
            public SocketComponent? HighlightComponent; // null = no highlight
        }

        /// <summary>
        /// Default component definitions for a UK Type G socket.
        /// </summary>
        public static readonly ComponentInfo[] Components = new ComponentInfo[]
        {
            new ComponentInfo
            {
                Component = SocketComponent.Earth,
                DisplayName = "Earth",
                ShortDescription = "Safety ground connection",
                DetailedExplanation = "The Earth pin is the longest pin at the top of the socket. " +
                    "It connects to the ground wire (green/yellow) and provides a safety path for " +
                    "electrical current to flow to the ground if there is a fault. This protects " +
                    "you from electric shock. The Earth pin is longer so it connects first and " +
                    "disconnects last, and also opens the safety shutters on the Live and Neutral holes.",
                LabelColor = new Color(0.2f, 0.8f, 0.2f, 1f), // Green
                NormalizedOffset = new Vector2(0f, 0.22f) // Top center
            },
            new ComponentInfo
            {
                Component = SocketComponent.Live,
                DisplayName = "Live",
                ShortDescription = "Carries current from supply",
                DetailedExplanation = "The Live pin is the bottom-left horizontal pin (when facing the socket). " +
                    "It connects to the brown wire and carries the electrical current from the " +
                    "power supply to your appliance. This is the most dangerous pin as it carries " +
                    "the full mains voltage (230V in the UK). The Live hole has a safety shutter " +
                    "that only opens when the Earth pin is inserted first.",
                LabelColor = new Color(0.9f, 0.3f, 0.2f, 1f), // Red
                NormalizedOffset = new Vector2(-0.23f, -0.2f) // Bottom left
            },
            new ComponentInfo
            {
                Component = SocketComponent.Neutral,
                DisplayName = "Neutral",
                ShortDescription = "Returns current to supply",
                DetailedExplanation = "The Neutral pin is the bottom-right horizontal pin (when facing the socket). " +
                    "It connects to the blue wire and provides the return path for electrical current " +
                    "back to the power supply. While it normally carries the same current as the Live wire, " +
                    "it is at or near ground potential, making it less dangerous. However, you should " +
                    "never assume the Neutral wire is safe to touch.",
                LabelColor = new Color(0.3f, 0.5f, 0.9f, 1f), // Blue
                NormalizedOffset = new Vector2(0.23f, -0.2f) // Bottom right
            }
        };

        /// <summary>
        /// Learning steps that guide the user through the experience.
        /// </summary>
        public static readonly LearningStep[] Steps = new LearningStep[]
        {
            new LearningStep
            {
                Title = "Welcome!",
                Instruction = "Point your camera at an electrical socket (or the reference image) to begin learning about its parts.",
                HighlightComponent = null
            },
            new LearningStep
            {
                Title = "Step 1: Earth Pin",
                Instruction = "Tap the GREEN label to learn about the Earth pin — the safety connection.",
                HighlightComponent = SocketComponent.Earth
            },
            new LearningStep
            {
                Title = "Step 2: Live Pin",
                Instruction = "Tap the RED label to learn about the Live pin — the power source.",
                HighlightComponent = SocketComponent.Live
            },
            new LearningStep
            {
                Title = "Step 3: Neutral Pin",
                Instruction = "Tap the BLUE label to learn about the Neutral pin — the return path.",
                HighlightComponent = SocketComponent.Neutral
            },
            new LearningStep
            {
                Title = "Complete!",
                Instruction = "Great job! You've learned about all three parts of a UK electrical socket.",
                HighlightComponent = null
            }
        };

        public static ComponentInfo GetComponentInfo(SocketComponent component)
        {
            foreach (var info in Components)
            {
                if (info.Component == component)
                    return info;
            }
            return null;
        }
    }
}
