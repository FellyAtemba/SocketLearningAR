# Socket Learning AR - Identify the Parts of an Electrical Socket

An AR learning prototype built with Unity 6 that teaches users to identify the three key parts of a UK Type G electrical socket (Earth, Live, and Neutral) using augmented reality image tracking.

## 📱 How It Works

1. **Point** your Android phone at a real UK-style electrical socket or the provided reference image
2. **Detect** - AR Foundation's image tracking recognizes the socket and overlays interactive labels
3. **Learn** - Color-coded labels appear on each pin:
   -  **Earth** (green) - Safety ground connection at the top
   -  **Live** (red) - Power supply on the bottom-left
   -  **Neutral** (blue) - Return path on the bottom-right
4. **Tap** each label to read a detailed educational explanation
5. **Progress** through guided learning steps using the Next button
6. **Complete** - Reach the completion screen after learning all three parts

##  How to Run the Project

### Prerequisites
- **Unity 6** (6000.5.8f1 or later)
- **Android Build Support** module installed via Unity Hub
- **Android device** with ARCore support (Android 7.0+) — the device must also appear on Google's [ARCore supported devices list](https://developers.google.com/ar/devices). See **Limitations** below; this project's own test device is not currently certified.

### Opening the Project
1. Open Unity Hub
2. Click **Open** - navigate to the `SocketLearninAR` folder
3. Wait for Unity to import all assets

### First-Time Setup (in Unity Editor)
1. Open the scene: `Assets/socketAR.unity`
2. Run **Socket Learning AR - Build Prefabs** from the menu bar to generate label and overlay prefabs
3. Run **Socket Learning AR - Setup Scene (Full)** to wire up all UI and references
4. Configure the Reference Image Library:
   - Select `Assets/AR_Data/ReferenceImageLibrary` in the Project window
   - In the Inspector, click the image entry and assign `Assets/AR_Data/socket2.jpeg` as the texture
   - Enable **Specify Size** and set the physical width to `0.086` meters (86mm for a standard UK single socket)
5. Save the scene (Ctrl+S)

### Building for Android
1. Go to **File - Build Profiles**
2. Select **Android** and click **Switch Platform** (if not already)
3. Connect your Android device via USB (enable USB debugging)
4. Click **Build and Run**

### Testing in Editor
- Unity's XR Simulation environment allows basic testing without a device
- For full AR testing, build and deploy to an Android device
-  Full on-device AR testing (live image tracking, tap detection against a real camera feed) was not achievable during development due to the device compatibility issue described in Limitations — testing was limited to Editor/XR Simulation and a successful build/install pass

## 🔧 Tools & Packages Used

| Package | Version | Purpose |
|---------|---------|---------|
| Unity | 6000.5.8f1 | Game engine |
| AR Foundation | 6.5.0 | Cross-platform AR framework |
| ARCore XR Plugin | 6.5.0 | Google ARCore provider for Android |
| Universal Render Pipeline | 17.5.0 | Graphics rendering |
| TextMeshPro | (built-in) | High-quality text rendering |
| Input System | 1.20.0 | Input handling |

##  Project Structure

```
Assets/
├── AR_Data/
│   ├── ReferenceImageLibrary.asset    # AR reference image library
│   └── socket2.jpeg                    # Reference image (UK Type G socket)
├── Materials/
│   ├── LabelBackground.mat            # Semi-transparent label background
│   └── LabelAccent.mat                # Color accent bar material
├── Prefabs/
│   ├── SocketLabel.prefab             # Individual label prefab (3D world-space)
│   └── SocketOverlay.prefab           # Root overlay spawned on tracked image
├── Scripts/
│   ├── SocketLearningAR/
│   │   ├── SocketData.cs              # Static data: component info & learning steps
│   │   ├── ARImageTracker.cs          # AR image tracking & overlay spawning
│   │   ├── SocketLabelManager.cs      # Creates & positions labels on the image
│   │   ├── SocketLabel.cs             # Individual label behavior & tap detection
│   │   └── LearningFlowManager.cs     # Learning flow, UI, step progression
│   └── Editor/
│       ├── LabelPrefabBuilder.cs      # Menu tool to generate prefabs
│       └── SceneSetupTool.cs          # Menu tool to wire up the scene
├── socketAR.unity                     # Main AR scene
└── Settings/                          # URP render pipeline settings
```

##  Architecture

```
ARImageTracker (on XR Origin)
    ├── Listens to ARTrackedImageManager events
    ├── Spawns SocketOverlay prefab on detected image
    └── Reports tracking state

SocketOverlay (spawned at runtime)
    └── SocketLabelManager
        ├── Creates 3 SocketLabel instances
        └── Positions labels based on physical image size

SocketLabel (world-space 3D label)
    ├── Billboard-faces camera
    ├── Has BoxCollider for raycasting
    ├── Pulse animation when highlighted
    └── Reports taps to LearningFlowManager

LearningFlowManager (persistent in scene)
    ├── Controls learning step progression
    ├── Manages all UI panels
    ├── Handles touch/tap input → raycasts to labels
    └── Shows component info on tap
```

##  Assumptions & Limitations

1. **Socket type**: The app is designed for UK Type G sockets (3-pin, rectangular). Other socket types are not supported.
2. **Image tracking**: Works best with good lighting and a clear view of the socket. The socket should be roughly 8-9cm wide for accurate tracking.
3. **Single image**: Only one socket is tracked at a time.
4. **No 3D models**: Labels are world-space text with quads, not complex 3D models. This keeps the project lightweight and focused on the learning content.
5. **Android only**: The project targets Android with ARCore. iOS (ARKit) would require adding the ARKit package.
6. **Physical size**: The reference image library assumes a standard UK single socket plate (~86mm wide). Different socket sizes may cause slight label misalignment.
7. **Device compatibility (unresolved)**: My development/test device Redmi 14c and Samsung A07 are not on Google's [ARCore supported devices list](https://developers.google.com/ar/devices). Google Play Services for AR refuses to install on it with a "device not compatible" error, which blocks the app before the AR session can even start. This is a certification issue on Google's side (each model is individually vetted for camera, sensor, and CPU quality), not a defect in the project itself. As a result, the image tracking, tap-to-select, and step-flow logic are implemented and build successfully, but have not been validated end-to-end against a live camera feed on a certified physical device — only in the Editor via XR Simulation. Given the assignment's guidance to document rather than spend excessive time chasing a fix, I left this as a known limitation. The most direct path to closing it out would be testing via the Android Emulator's ARCore-enabled virtual scene, or on a certified device such as a Pixel.

##  Additional Features

- **Guided learning flow**: Step-by-step progression ensures users learn each component in order
- **Interactive labels**: Tap any label at any time to read its detailed explanation
- **Visual highlighting**: The current step's component pulses to draw attention
- **Completion screen**: Summary of all learned components with option to restart
- **Scanning prompt**: Visual feedback when no socket is detected
- **Billboard labels**: Labels always face the camera for readability
- **Editor tools**: Menu-based prefab builder and scene setup tools for easy project configuration

##  Development Notes

- Built with C# following Unity conventions and namespace organization
- All custom code is in the `SocketLearningAR` namespace
- Editor-only code is properly guarded with `#if UNITY_EDITOR`
- The project uses AR Foundation's event-based tracking (not polling) for efficiency
