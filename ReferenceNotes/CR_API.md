# CameraTools CinematicRecorder Integration API

**Version:** 1.37.1  
**Namespace:** `CameraTools.ModIntegration`

This document describes the public API surface provided by `CinematicRecorderIntegration` for external mod integration. This API allows other KSP mods to control CameraTools programmatically for cinematic recording and automated camera operations.

---

## Table of Contents

1. [Getting Started](#getting-started)
2. [Availability Check](#availability-check)
3. [Camera State](#camera-state)
4. [Tool Modes](#tool-modes)
5. [State Getters](#state-getters)
6. [FOV Control](#fov-control)
7. [Camera Activation](#camera-activation)
8. [Stationary Camera](#stationary-camera)
9. [Dogfight Camera](#dogfight-camera)
10. [Pathing Camera](#pathing-camera)
11. [Deterministic Mode](#deterministic-mode)
12. [Events](#events)

---

## Getting Started

The `CinematicRecorderIntegration` class is a static class that provides safe access to CameraTools functionality. All methods are null-safe and will gracefully return default values if CameraTools is not available.

```csharp
using CameraTools.ModIntegration;

// Check if CameraTools is available before using API
if (CinematicRecorderIntegration.IsAvailable)
{
    // Use the API...
}
```

---

## Availability Check

### `IsAvailable`
```csharp
public static bool IsAvailable { get; }
```
Returns `true` if CameraTools is installed and initialized. Always check this before using any other API methods.

---

## Camera State

### `CameraState` Struct
```csharp
public struct CameraState
{
    public ToolModes Mode;                    // Current camera mode
    public Vector3 ManualPosition;            // Manual position override
    public Vector3 LastVesselCoM;            // Last vessel center of mass
    public GameObject CameraParent;          // Camera parent GameObject
    public bool HasTarget;                   // Whether camera has a target
    public Part CamTarget;                   // Target part (if any)
    public bool IsPlayingPath;               // Path playback status
    public int SelectedPathIndex;            // Currently selected path
    public int CurrentKeyframeIndex;         // Current keyframe in path
    public float PathStartTime;              // When path playback started
    public bool CinematicRecorderControl;    // CR control mode active
    public bool CinematicRecorderDeterministic; // Deterministic mode active
    public bool LockPathingToPlaybackRate;   // Lock to video frame rate
    public float CurrentFOV;                 // Current field of view
}
```

### `GetCurrentState()`
```csharp
public static CameraState GetCurrentState()
```
Retrieves a complete snapshot of the current camera state.

---

## Tool Modes

### `ToolModes` Enum
```csharp
public enum ToolModes
{
    StationaryCamera,    // Fixed position camera
    DogfightCamera,      // Chase camera following targets
    Pathing              // Predefined camera path playback
}
```

### Mode Management

#### `GetToolMode()`
```csharp
public static ToolModes GetToolMode()
```
Returns the current tool mode. Defaults to `StationaryCamera` if unavailable.

#### `SetToolMode(ToolModes mode)`
```csharp
public static void SetToolMode(ToolModes mode)
```
Sets the camera tool mode.

#### `IsCameraActive()`
```csharp
public static bool IsCameraActive()
```
Returns `true` if the camera tool is currently active (controlling the camera).

#### `SwitchCamera(ToolModes newMode)`
```csharp
public static void SwitchCamera(ToolModes newMode)
```
Switches between camera modes without reverting to stock camera first. **Use this instead of `SetToolMode` when switching modes** to prevent flickering and 1-frame stock camera flashes between transitions.

---

## State Getters

### `GetManualFOV()`
```csharp
public static float GetManualFOV()
```
Returns the manual FOV target value (the desired FOV, not the interpolated current value). Defaults to `60f`.

---

## FOV Control

### `GetActualFOV()`
```csharp
public static float GetActualFOV()
```
Returns the actual current interpolated FOV. Defaults to `60f` if unavailable.

### `SetExternalFOV(float fov)`
```csharp
public static void SetExternalFOV(float fov)
```
Sets the FOV immediately, bypassing smoothing interpolation. Use for precise frame-accurate FOV control.

---

## Camera Activation

### `ActivateCamera()`
```csharp
public static void ActivateCamera()
```
Activates CameraTools with current settings. The camera will take control from stock KSP camera.

### `DeactivateCamera()`
```csharp
public static void DeactivateCamera()
```
Properly deactivates the camera and releases control back to stock KSP camera. **Use this instead of manual `RevertCamera` calls** to avoid state desynchronization issues.

### `ActivateCameraWithSettings(ToolModes mode, Action<CamTools> configureSettings)`
```csharp
public static void ActivateCameraWithSettings(ToolModes mode, Action<CamTools> configureSettings)
```
Activates the camera with custom configuration. The `configureSettings` callback allows you to configure CameraTools internals before activation.

**Example:**
```csharp
CinematicRecorderIntegration.ActivateCameraWithSettings(
    ToolModes.StationaryCamera,
    ct => {
        ct.manualPosition = new Vector3(100, 50, 100);
        ct.manualFOV = 45f;
    }
);
```

---

## Stationary Camera

### Positioning

#### `SetStationaryPosition(Vector3 position, Part target = null)`
```csharp
public static void SetStationaryPosition(Vector3 position, Part target = null)
```
Sets the stationary camera position and optional target. **Call this before `ActivateCamera()`**.

- Automatically disables conflicting auto-modes (`autoFlybyPosition`, `autoLandingPosition`, `manualOffset`, `randomMode`)
- Sets `setPresetOffset` flag so `StartStationaryCamera()` uses the preset position

### Configuration

#### `SetStationaryFlags(bool presetOffset, bool autoFlyby, bool autoLanding, bool manualOffset)`
```csharp
public static void SetStationaryFlags(bool presetOffset, bool autoFlyby, bool autoLanding, bool manualOffset)
```
Sets positioning mode flags:
- `presetOffset`: Use preset position offset
- `autoFlyby`: Auto-position for flyby shots
- `autoLanding`: Auto-position for landing shots
- `manualOffset`: Use manual offset values

#### `SetManualOffset(float forward, float right, float up)`
```csharp
public static void SetManualOffset(float forward, float right, float up)
```
Sets manual offset values relative to the vessel (Forward/Right/Up axes).

#### `SetStationaryAdvanced(bool saveRot, bool maintainVel, bool useOrb, bool autoZoom)`
```csharp
public static void SetStationaryAdvanced(bool saveRot, bool maintainVel, bool useOrb, bool autoZoom)
```
Sets advanced stationary camera options:
- `saveRot`: Save rotation state between activations
- `maintainVel`: Maintain initial velocity (camera moves with vessel's initial velocity)
- `useOrb`: Use orbital velocity instead of surface velocity
- `autoZoom`: Enable auto-zoom for stationary camera

### Targeting

#### `SetTarget(Part target, bool useCoM)`
```csharp
public static void SetTarget(Part target, bool useCoM)
```
Sets the camera target and targeting mode:
- `target`: The part to target (null for no target)
- `useCoM`: If true, camera points at vessel CoM; if false, points at target part

---

## Dogfight Camera

### `SetDogfightConfig(float distance, float offsetX, float offsetY, bool chasePlane)`
```csharp
public static void SetDogfightConfig(float distance, float offsetX, float offsetY, bool chasePlane)
```
Configures the dogfight camera:
- `distance`: Camera distance from target
- `offsetX`: Horizontal offset from target
- `offsetY`: Vertical offset from target
- `chasePlane`: Enable chase plane mode (camera follows behind target)

### `SetDogfightTarget(Vessel target)`
```csharp
public static void SetDogfightTarget(Vessel target)
```
Sets the target vessel for dogfight camera.

---

## Pathing Camera

### Path Selection

#### `SelectPath(int index)`
```csharp
public static void SelectPath(int index)
```
Selects a path by index from the available paths list.

#### `GetPathIndexByName(string pathName)`
```csharp
public static int GetPathIndexByName(string pathName)
```
Finds a path index by its name. Returns `-1` if not found.

#### `GetPathCount()`
```csharp
public static int GetPathCount()
```
Returns the number of available paths.

#### `GetPathName(int index)`
```csharp
public static string GetPathName(int index)
```
Returns the name of a path by index. Returns `null` if index is invalid.

#### `PathExists(int index)`
```csharp
public static bool PathExists(int index)
```
Returns `true` if a valid path exists at the given index.

### Path Control

#### `StartPathPlayback()`
```csharp
public static void StartPathPlayback()
```
Starts playback of the currently selected path.

#### `StopPathPlayback()`
```csharp
public static void StopPathPlayback()
```
Stops path playback.

#### `SetPathingStartKeyframe(int keyframeIndex)`
```csharp
public static void SetPathingStartKeyframe(int keyframeIndex)
```
Sets the starting keyframe index for path playback.

#### `SetPathState(int pathIndex, int keyframeIndex, bool isPlaying, float startTime)`
```csharp
public static void SetPathState(int pathIndex, int keyframeIndex, bool isPlaying, float startTime)
```
Sets the complete pathing state in one call.

### Path Timing

#### `GetPathTimeScale(int index)` / `SetPathTimeScale(int index, float scale)`
```csharp
public static float GetPathTimeScale(int index)
public static void SetPathTimeScale(int index, float scale)
```
Gets or sets the time scale multiplier for a specific path.

#### `SetPathTiming(bool useRealTime, float smoothing)`
```csharp
public static void SetPathTiming(bool useRealTime, float smoothing)
```
Sets path timing options:
- `useRealTime`: Use real time instead of game time
- `smoothing`: Secondary smoothing factor

#### `GetCurrentPathTime()`
```csharp
public static float GetCurrentPathTime()
```
Returns the current path time (physics time or playback time depending on settings).

#### `GetPathDuration(int index)`
```csharp
public static float GetPathDuration(int index)
```
Returns the total duration of a path (time of the last keyframe).

---

## Deterministic Mode

Deterministic mode allows frame-accurate camera control for cinematic recording, ensuring that camera movements are reproducible across multiple recording sessions.

### Control Mode

#### `SetCinematicRecorderControl(bool enabled, bool deterministicMode)`
```csharp
public static void SetCinematicRecorderControl(bool enabled, bool deterministicMode)
```
Enables/disables cinematic recorder control mode:
- `enabled`: Enable external control
- `deterministicMode`: Use deterministic (frame-accurate) mode

#### `SetLockPathingToPlaybackRate(bool enabled)`
```csharp
public static void SetLockPathingToPlaybackRate(bool enabled)
```
Controls how pathing camera advances:
- `true`: Camera moves at video frame rate independent of physics speed (for Kraken-Time recording)
- `false`: Camera moves at physics simulation rate (deterministic recording)

### Time Control

#### `SetDeterministicPathTime(float evaluationTime)`
```csharp
public static void SetDeterministicPathTime(float evaluationTime)
```
Explicitly sets the deterministic path time for seeking to a specific position. Automatically clamps to valid path duration. Only works when `cinematicRecorderDeterministic` is enabled.

#### `GetDeterministicPathTime()`
```csharp
public static float GetDeterministicPathTime()
```
Returns the current deterministic path time accumulator.

### Physics Update

#### `PhysicsStepUpdate(float physicsDeltaTime, float playbackDeltaTime)`
```csharp
public static void PhysicsStepUpdate(float physicsDeltaTime, float playbackDeltaTime)
```
Updates the camera for a physics step in deterministic mode. Provide both physics and playback delta times; the active mode determines which is used.

**Parameters:**
- `physicsDeltaTime`: `Time.fixedDeltaTime` or custom physics step
- `playbackDeltaTime`: Video frame time (1/playbackFPS)

#### `PhysicsStepUpdate(float physicsDeltaTime)`
```csharp
public static void PhysicsStepUpdate(float physicsDeltaTime)
```
Legacy single-delta overload. Assumes physics time.

#### `GetDeltaTime()`
```csharp
public static float GetDeltaTime()
```
Returns the current delta time being used (either deterministic or `TimeWarp.fixedDeltaTime`).

---

## Events

The API provides events for reacting to camera state changes:

### `OnCameraActivated`
```csharp
public static event Action OnCameraActivated
```
Raised when CameraTools takes control of the camera.

### `OnCameraDeactivated`
```csharp
public static event Action OnCameraDeactivated
```
Raised when CameraTools releases control of the camera.

### `OnPathingStarted`
```csharp
public static event Action OnPathingStarted
```
Raised when path playback starts.

### `OnPathingStopped`
```csharp
public static event Action OnPathingStopped
```
Raised when path playback stops.

### `OnCinematicRecorderControlTaken`
```csharp
public static event Action OnCinematicRecorderControlTaken
```
Raised when cinematic recorder control mode is activated.

**Example:**
```csharp
CinematicRecorderIntegration.OnCameraActivated += () =>
{
    Debug.Log("CameraTools has taken control!");
};
```

---

## Usage Examples

### Basic Stationary Camera
```csharp
if (CinematicRecorderIntegration.IsAvailable)
{
    // Position camera 100m above vessel
    Vector3 position = FlightGlobals.ActiveVessel.transform.position + Vector3.up * 100;
    CinematicRecorderIntegration.SetStationaryPosition(position);
    CinematicRecorderIntegration.SetToolMode(ToolModes.StationaryCamera);
    CinematicRecorderIntegration.ActivateCamera();
}
```

### Path Playback
```csharp
if (CinematicRecorderIntegration.IsAvailable)
{
    // Find and select path by name
    int pathIndex = CinematicRecorderIntegration.GetPathIndexByName("MyCinematicPath");
    if (pathIndex >= 0)
    {
        CinematicRecorderIntegration.SelectPath(pathIndex);
        CinematicRecorderIntegration.SetToolMode(ToolModes.Pathing);
        CinematicRecorderIntegration.ActivateCamera();
        CinematicRecorderIntegration.StartPathPlayback();
    }
}
```

### Deterministic Recording
```csharp
if (CinematicRecorderIntegration.IsAvailable)
{
    // Enable deterministic mode
    CinematicRecorderIntegration.SetCinematicRecorderControl(true, true);
    CinematicRecorderIntegration.SetLockPathingToPlaybackRate(false);
    
    // Setup and activate
    CinematicRecorderIntegration.SelectPath(0);
    CinematicRecorderIntegration.SetToolMode(ToolModes.Pathing);
    CinematicRecorderIntegration.ActivateCamera();
    
    // In FixedUpdate():
    // CinematicRecorderIntegration.PhysicsStepUpdate(Time.fixedDeltaTime, 1f/60f);
}
```

### Mode Switching Without Flicker
```csharp
// Instead of this (causes flicker):
// CinematicRecorderIntegration.SetToolMode(ToolModes.StationaryCamera);

// Do this:
CinematicRecorderIntegration.SwitchCamera(ToolModes.StationaryCamera);
```

---

## Best Practices

1. **Always check `IsAvailable`** before using any API methods
2. **Use `SwitchCamera()`** instead of `SetToolMode()` when switching between modes to avoid flicker
3. **Use `DeactivateCamera()`** instead of manual revert calls to avoid state desync
4. **Call `SetStationaryPosition()` before `ActivateCamera()`** for stationary camera setup
5. **Use deterministic mode** for reproducible cinematic recordings
6. **Subscribe to events** to react to camera state changes rather than polling
