# CameraTools - AI Agent Documentation

## Project Overview

CameraTools is a **Kerbal Space Program (KSP) mod** that provides advanced camera controls for cinematics and BDArmory integration. It allows players to create cinematic camera shots with features like dogfight chase cameras, stationary cameras with velocity tracking, and path-based camera animations.

- **Current Version**: 1.37.1
- **KSP Version Compatibility**: 1.9.0 to 1.12.99
- **Target Framework**: .NET Framework 4.8
- **Language**: C# with preview language version features

## Technology Stack

- **Platform**: Kerbal Space Program (Unity Engine-based)
- **Language**: C# (.NET Framework 4.8)
- **Build System**: MSBuild (Visual Studio / .NET SDK)
- **Dependencies**:
  - KSP Assembly-CSharp.dll (game API)
  - KSPAssets.dll
  - Unity Engine modules (Core, UI, Physics, Audio, etc.)
  - KSPBuildTools NuGet package (v1.1.1)

## Project Structure

```
CameraTools/
├── CameraTools.csproj          # Main project file
├── Properties/
│   └── AssemblyInfo.cs         # Assembly version info (1.37.1.0)
│
├── Core Components:
│   ├── CamTools.cs             # Main controller (1000+ lines, KSPAddon)
│   ├── CameraPath.cs           # Path system for camera animations
│   ├── CameraKeyframe.cs       # Keyframe structure for paths
│   ├── CameraTransformation.cs # Camera state container
│   ├── CTPersistantField.cs    # Settings persistence system
│   └── InputField.cs           # UI float input component
│
├── Animation:
│   ├── Vector3Animation.cs     # Position interpolation
│   ├── RotationAnimation.cs    # Rotation interpolation
│   └── Curve3D.cs              # 3D curve calculations
│
├── Audio:
│   ├── CTAtmosphericAudioController.cs
│   └── CTPartAudioController.cs
│
├── Extensions/:
│   ├── PartExtensions.cs
│   └── VesselExtensions.cs     # Vessel velocity helpers
│
├── Utils/:
│   ├── CCInputUtils.cs         # Input handling
│   ├── CTKrakensbaneUtils.cs   # Floating origin optimizations
│   ├── LayerMasks.cs
│   ├── MathUtils.cs            # Apple Silicon workaround
│   ├── ReflectionUtils.cs
│   ├── SplineUtils.cs          # Hermite spline interpolation
│   ├── StringUtils.cs          # Localization helpers
│   └── Tooltips.cs
│
├── ModIntegration/:
│   ├── BDArmory.cs             # BDArmory competition integration
│   ├── BetterTimeWarp.cs
│   ├── CinematicRecorderIntegration.cs  # Public API for external mods
│   ├── MouseAimFlight.cs
│   └── TimeControl.cs
│
├── LocalDev/                   # Build configuration (user-specific)
│   ├── 7za_dir.txt             # 7-Zip path
│   ├── dist_dir.txt            # Distribution output path
│   ├── ksp_dir.txt             # KSP install path
│   ├── ksp_dir2.txt
│   ├── mono_exe.txt
│   └── pdb2mdb_exe.txt
│
└── Distribution/               # Release packaging
    └── GameData/CameraTools/
        ├── Plugins/            # Compiled DLL output
        ├── Sounds/             # Audio assets (wind, sonic boom)
        ├── Textures/           # icon.png
        ├── Localization/       # en-us.cfg, zh-cn.cfg
        ├── ATM_CameraTools.cfg # Active Texture Manager config
        ├── CameraTools.version # Version file for KSP-AVC
        └── Changelog.txt
```

## Build Configuration

### Build Requirements

1. **LocalDev Configuration Files** (in `CameraTools/LocalDev/`):
   - `ksp_dir.txt`: Path to KSP installation for testing
   - `7za_dir.txt`: Path to 7-Zip for packaging
   - `dist_dir.txt`: Output directory for release ZIP files
   - `pdb2mdb_exe.txt`: Path to pdb2mdb for debug symbols (optional)

2. **KSP References**: The project references KSP DLLs from `../../_LocalDev/KSPRefs/`
   - `Assembly-CSharp.dll`
   - `KSPAssets.dll`
   - Various `UnityEngine.*.dll` modules

### Build Commands

```bash
# Build the project (Debug)
MSBuild CameraTools.sln /p:Configuration=Debug

# Build the project (Release)
MSBuild CameraTools.sln /p:Configuration=Release
```

### Post-Build Process

The `.csproj` includes platform-specific post-build events:

**Windows**:
1. Copies compiled DLL to `Distribution/GameData/CameraTools/Plugins/`
2. Copies PDB files in Debug configuration
3. Creates timestamped ZIP package using 7-Zip
4. Deploys to KSP GameData folder for testing

**Linux/macOS**:
1. Copies assemblies to Distribution folder
2. Packages with 7z
3. Deploys to all KSP folders listed in `ksp_dir.txt`

## Code Organization

### Main Camera Modes (`ToolModes` enum)

1. **StationaryCamera**: Fixed camera position with optional velocity tracking
2. **DogfightCamera**: Chase camera following targets with inertia options
3. **Pathing**: Predefined camera path playback with spline interpolation

### Key Classes

| Class | Purpose |
|-------|---------|
| `CamTools` | Main KSPAddon controller, handles UI and camera state |
| `CameraPath` | Manages path data, serialization, keyframe interpolation |
| `CameraKeyframe` | Data structure for path keyframes |
| `BDArmory` | Integration with BDArmory mod for auto-targeting |
| `CinematicRecorderIntegration` | Public API for external mod control |
| `CTKrakensbaneUtils` | Optimized floating origin handling |

### Interpolation Systems

- **Position**: Linear or CubicSpline (Hermite)
- **Rotation**: Linear, CubicSpline, or Slerp
- Implementation in `SplineUtils.cs` using Hermite polynomial basis functions

## Development Conventions

### Coding Style

- **Naming**: PascalCase for classes/methods, camelCase for fields
- **Private fields**: Prefix with `_` for backing fields
- **Regions**: Code is organized into `#region` blocks (Fields, Input, GUI, etc.)
- **Comments**: XML documentation for public APIs, inline comments for complex logic

### KSP-Specific Patterns

1. **KSPAddon Attribute**:
   ```csharp
   [KSPAddon(KSPAddon.Startup.Flight, false)]
   public class CamTools : MonoBehaviour
   ```

2. **Persistent Fields**: Use `[CTPersistantField]` attribute for settings:
   ```csharp
   [CTPersistantField] public float dogfightDistance = 50f;
   ```

3. **GameEvents**: Subscribe to KSP events in `Start()`, unsubscribe in `OnDestroy()`

4. **Localization**: Use `StringUtils.Localize()` with `#LOC_CameraTools_*` keys

### Settings Persistence

Settings are stored in `GameData/CameraTools/PluginData/settings.cfg`:
- Uses `CTPersistantField.Save()` / `Load()` methods
- Migrates from old location automatically
- Section-based organization per class

## Testing Strategy

### Manual Testing Checklist

1. **Camera Modes**:
   - Stationary camera with velocity maintenance
   - Dogfight camera with targeting
   - Path playback with various interpolation types

2. **Integration**:
   - BDArmory competition auto-enable
   - Time warp behavior
   - Vessel switching

3. **Platform Compatibility**:
   - Windows (primary)
   - macOS (including Apple Silicon workaround)
   - Linux

### Debug Features

- `DEBUG` and `DEBUG2` flags for verbose logging
- Debug UI overlay showing internal state
- Configurable via settings file

```csharp
[CTPersistantField] public static bool DEBUG = false;
public static void DebugLog(string m) => debugMessages.Add(new Tuple<double, string>(Time.time, m));
```

## Mod Integration API

CameraTools provides a public API via `CinematicRecorderIntegration`:

```csharp
// Check availability
bool available = CinematicRecorderIntegration.IsAvailable;

// Get/Set camera state
var state = CinematicRecorderIntegration.GetCurrentState();
CinematicRecorderIntegration.SetToolMode(ToolModes.Pathing);

// Path control
CinematicRecorderIntegration.SelectPath(index);
CinematicRecorderIntegration.StartPathPlayback();

// Events
CinematicRecorderIntegration.OnCameraActivated += MyHandler;
```

### BDArmory Integration

- Auto-enables during competitions
- AI target following
- Incoming missile priority targeting
- Centroid mode for multi-vessel scenes

## Deployment Process

1. **Development**:
   - Build in Debug configuration
   - Auto-deploys to KSP GameData for testing

2. **Release**:
   - Build in Release configuration
   - Creates `CameraTools.{version}_{date}.zip`
   - Includes all Distribution assets

3. **Version Files**:
   - Update `AssemblyInfo.cs` for assembly version
   - Update `CameraTools.version` for KSP-AVC compatibility

## Important Notes

### Floating Origin / Krakensbane

The mod handles KSP's floating origin system carefully:
- `CTKrakensbaneUtils` provides cached access to frame velocity
- Compensates for velocity shifts during time warp
- Handles transitions between surface and orbital velocity (>100km altitude)

### Audio Effects

- Requires spatializer plugin for Doppler effects
- Atmospheric audio (wind, sonic boom) when enabled
- Respects KSP's SoundManager channels

### Known Limitations

- Stationary camera has visible jitter at large distances (half-precision graphics limitation)
- Small drift in certain time warp scenarios
- Editor mode support planned but not implemented

## Localization

Supported languages:
- English (en-us)
- Chinese Simplified (zh-cn)

Add new translations in `Distribution/GameData/CameraTools/Localization/`.

## Resources

- **KSP Forum**: https://forum.kerbalspaceprogram.com/index.php?/topic/201063-camera-tools-continued-v1150/
- **Repository**: https://github.com/BrettRyland/CameraTools
- **Issue Tracking**: Use GitHub issues for bug reports and feature requests
