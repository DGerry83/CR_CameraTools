using System;
using UnityEngine;

namespace CameraTools.ModIntegration
{
    /// <summary>
    /// Public API for CinematicRecorder integration.
    /// Provides safe, static access to CameraTools functionality for external mods.
    /// </summary>
    public static class CinematicRecorderIntegration
    {
        /// <summary>
        /// Checks if CameraTools is available and initialized.
        /// </summary>
        public static bool IsAvailable => CamTools.fetch != null;

        /// <summary>
        /// Struct containing current camera state for snapshot capture.
        /// </summary>
        public struct CameraState
        {
            public ToolModes Mode;
            public Vector3 ManualPosition;
            public Vector3 LastVesselCoM;
            public GameObject CameraParent;
            public bool HasTarget;
            public Part CamTarget;
            public bool IsPlayingPath;
            public int SelectedPathIndex;
            public int CurrentKeyframeIndex;
            public float PathStartTime;
            public bool CinematicRecorderControl;
            public bool CinematicRecorderDeterministic;
            public bool LockPathingToPlaybackRate;
            public float CurrentFOV;
        }

        /// <summary>
        /// Gets the current camera state.
        /// </summary>
        public static CameraState GetCurrentState()
        {
            if (!IsAvailable) return default;

            var ct = CamTools.fetch;
            return new CameraState
            {
                Mode = ct.toolMode,
                ManualPosition = ct.manualPosition,
                LastVesselCoM = ct.lastVesselCoM,
                CameraParent = ct.cameraParent,
                HasTarget = ct.hasTarget,
                CamTarget = ct.camTarget,
                IsPlayingPath = ct.isPlayingPath,
                SelectedPathIndex = ct.selectedPathIndex,
                CurrentKeyframeIndex = ct.currentKeyframeIndex,
                PathStartTime = ct.pathStartTime,
                CinematicRecorderControl = ct.cinematicRecorderControl,
                CinematicRecorderDeterministic = ct.cinematicRecorderDeterministic,
                LockPathingToPlaybackRate = ct.lockPathingToPlaybackRate,
                CurrentFOV = ct.currentFOV
            };
        }

        /// <summary>
        /// Gets the actual current FOV.
        /// </summary>
        public static float GetActualFOV()
        {
            if (!IsAvailable) return 60f;
            return CamTools.fetch.currentFOV;
        }

        // NEW: State Getters (inserted after GetActualFOV)
        /// <summary>
        /// Gets the current tool mode (Stationary, Dogfight, Pathing).
        /// </summary>
        public static ToolModes GetToolMode() => IsAvailable ? CamTools.fetch.toolMode : ToolModes.StationaryCamera;

        /// <summary>
        /// Checks if the camera tool is currently active.
        /// </summary>
        public static bool IsCameraActive() => IsAvailable && CamTools.fetch.cameraToolActive;

        /// <summary>
        /// Gets the manual FOV target (not the interpolated currentFOV).
        /// </summary>
        public static float GetManualFOV() => IsAvailable ? CamTools.fetch.manualFOV : 60f;

        /// <summary>
        /// Sets the external FOV immediately (bypasses smoothing).
        /// </summary>
        public static void SetExternalFOV(float fov)
        {
            if (!IsAvailable) return;
            CamTools.fetch.SetExternalFOV(fov);
        }

        /// <summary>
        /// Enables/disables cinematic recorder control mode.
        /// </summary>
        public static void SetCinematicRecorderControl(bool enabled, bool deterministicMode)
        {
            if (!IsAvailable) return;
            CamTools.fetch.SetCinematicRecorderControl(enabled, deterministicMode);
        }

        /// <summary>
        /// Sets whether pathing camera advances by playback time (true) or physics time (false).
        /// When true, camera moves at video frame rate independent of physics speed (for Kraken-Time recording).
        /// When false, camera moves at physics simulation rate (deterministic recording).
        /// </summary>
        public static void SetLockPathingToPlaybackRate(bool enabled)
        {
            if (!IsAvailable) return;
            CamTools.fetch.lockPathingToPlaybackRate = enabled;
        }

        // Cinematic Recorder Fix: Proper deactivation method that ensures camera is fully released
        // Use this instead of manually calling RevertCamera to avoid state desync issues
        public static void DeactivateCamera()
        {
            if (!IsAvailable) return;
            CamTools.fetch.DeactivateCamera();
        }

        // Cinematic Recorder API: Explicitly set the deterministic path time (in evaluation time units)
        // Use this to seek to a specific position or resync if drift occurs
        public static void SetDeterministicPathTime(float evaluationTime)
        {
            if (!IsAvailable) return;
            if (!CamTools.fetch.cinematicRecorderDeterministic) return;

            CamTools.fetch.deterministicTimeAccumulator = Mathf.Max(0, evaluationTime);

            if (CamTools.fetch.currentPath != null)
            {
                // Clamp to path duration
                float maxTime = CamTools.fetch.currentPath.times.Count > 0
                    ? CamTools.fetch.currentPath.times[CamTools.fetch.currentPath.times.Count - 1]
                    : 0f;
                CamTools.fetch.deterministicTimeAccumulator = Mathf.Min(CamTools.fetch.deterministicTimeAccumulator, maxTime);
            }
        }

        public static float GetDeterministicPathTime()
        {
            if (!IsAvailable) return 0f;
            return CamTools.fetch.deterministicTimeAccumulator;
        }

        // Cinematic Recorder Fix: Switch between CT camera modes without reverting to stock camera
        // Prevents flicker/1-frame stock camera between transitions
        // Usage: SwitchCamera(ToolModes.Pathing) or SwitchCamera(ToolModes.StationaryCamera)
        public static void SwitchCamera(ToolModes newMode)
        {
            if (!IsAvailable) return;
            CamTools.fetch.SwitchCamera(newMode);
        }

        #region Preset Management

        /// <summary>
        /// Sets the camera tool mode (Stationary, Dogfight, Pathing).
        /// </summary>
        public static void SetToolMode(ToolModes mode)
        {
            if (!IsAvailable) return;
            CamTools.fetch.toolMode = mode;
        }

        // NEW: Stationary Configuration (inserted in Preset Management)
        /// <summary>
        /// Sets positioning mode flags for stationary camera.
        /// </summary>
        public static void SetStationaryFlags(bool presetOffset, bool autoFlyby, bool autoLanding, bool manualOffset)
        {
            if (!IsAvailable) return;
            CamTools.fetch.setPresetOffset = presetOffset;
            CamTools.fetch.autoFlybyPosition = autoFlyby;
            CamTools.fetch.autoLandingPosition = autoLanding;
            CamTools.fetch.manualOffset = manualOffset;
        }

        /// <summary>
        /// Sets manual offset values (Forward/Right/Up).
        /// </summary>
        public static void SetManualOffset(float forward, float right, float up)
        {
            if (!IsAvailable) return;
            CamTools.fetch.manualOffsetForward = forward;
            CamTools.fetch.manualOffsetRight = right;
            CamTools.fetch.manualOffsetUp = up;
        }

        /// <summary>
        /// Sets advanced stationary camera options.
        /// </summary>
        public static void SetStationaryAdvanced(bool saveRot, bool maintainVel, bool useOrb, bool autoZoom)
        {
            if (!IsAvailable) return;
            CamTools.fetch.saveRotation = saveRot;
            CamTools.fetch.maintainInitialVelocity = maintainVel;
            CamTools.fetch.useOrbital = useOrb;
            CamTools.fetch.autoZoomStationary = autoZoom;
        }

        /// <summary>
        /// Gets the initial velocity for stationary camera velocity maintenance.
        /// Used for maintaining camera position relative to vessel motion in orbit.
        /// </summary>
        public static Vector3d GetInitialVelocity()
        {
            if (!IsAvailable) return Vector3d.zero;
            return CamTools.fetch.initialVelocity;
        }

        /// <summary>
        /// Sets the initial velocity for stationary camera velocity maintenance.
        /// Call this before ActivateCamera() when using maintainInitialVelocity mode.
        /// </summary>
        public static void SetInitialVelocity(Vector3d velocity)
        {
            if (!IsAvailable) return;
            CamTools.fetch.initialVelocity = velocity;
        }

        /// <summary>
        /// Gets initial velocity as Vector3 (single precision).
        /// Convenience method for consumers using Unity's Vector3 type.
        /// </summary>
        public static Vector3 GetInitialVelocityAsVector3()
        {
            Vector3d vel = GetInitialVelocity();
            return new Vector3((float)vel.x, (float)vel.y, (float)vel.z);
        }

        // NEW: Dogfight Configuration (inserted in Preset Management)
        /// <summary>
        /// Sets dogfight camera configuration.
        /// </summary>
        public static void SetDogfightConfig(float distance, float offsetX, float offsetY, bool chasePlane)
        {
            if (!IsAvailable) return;
            CamTools.fetch.dogfightDistance = distance;
            CamTools.fetch.dogfightOffsetX = offsetX;
            CamTools.fetch.dogfightOffsetY = offsetY;
            CamTools.fetch.dogfightChasePlaneMode = chasePlane;
        }

        /// <summary>
        /// Sets the dogfight target vessel.
        /// </summary>
        public static void SetDogfightTarget(Vessel target)
        {
            if (!IsAvailable) return;
            CamTools.fetch.dogfightTarget = target;
        }

        // NEW: Targeting (inserted in Preset Management)
        /// <summary>
        /// Sets camera target and targeting mode.
        /// </summary>
        public static void SetTarget(Part target, bool useCoM)
        {
            if (!IsAvailable) return;
            CamTools.fetch.camTarget = target;
            CamTools.fetch.hasTarget = target != null;
            CamTools.fetch.targetCoM = useCoM;
        }

        /// <summary>
        /// Activates the camera with current settings.
        /// </summary>
        public static void ActivateCamera()
        {
            if (!IsAvailable) return;
            CamTools.fetch.CameraActivate();
        }

        /// <summary>
        /// Sets stationary camera position and optional target.
        /// Call before ActivateCamera().
        /// </summary>
        public static void SetStationaryPosition(Vector3 position, Part target = null)
        {
            if (!IsAvailable) return;

            // CRITICAL: Set flag so StartStationaryCamera() uses presetOffset/manual logic
            CamTools.fetch.setPresetOffset = true;
            CamTools.fetch.presetOffset = position;

            // Also set manualPosition for runtime updates
            CamTools.fetch.manualPosition = position;
            CamTools.fetch.camTarget = target;
            CamTools.fetch.hasTarget = (target != null);

            // DISABLE conflicting auto-modes that would overwrite position
            CamTools.fetch.autoFlybyPosition = false;
            CamTools.fetch.autoLandingPosition = false;
            CamTools.fetch.manualOffset = false;
            CamTools.fetch.randomMode = false;
        }

        /// <summary>
        /// Finds path index by name. Returns -1 if not found.
        /// </summary>
        public static int GetPathIndexByName(string pathName)
        {
            if (!IsAvailable) return -1;
            var paths = CamTools.fetch.availablePaths;
            for (int i = 0; i < paths.Count; i++)
            {
                if (paths[i].pathName == pathName)
                    return i;
            }
            return -1;
        }

        // NEW: Path List Access (inserted in Preset Management, near GetPathIndexByName)
        /// <summary>
        /// Gets the number of available paths.
        /// </summary>
        public static int GetPathCount() => IsAvailable ? CamTools.fetch.availablePaths.Count : 0;

        /// <summary>
        /// Gets the name of a path by index.
        /// </summary>
        public static string GetPathName(int index) =>
            (IsAvailable && index >= 0 && index < CamTools.fetch.availablePaths.Count)
            ? CamTools.fetch.availablePaths[index].pathName
            : null;

        public static void ActivateCameraWithSettings(ToolModes mode, Action<CamTools> configureSettings)
        {
            if (!IsAvailable) return;
            var camTools = CamTools.fetch;

            // Disable random mode (it overrides everything)
            camTools.randomMode = false;

            // Apply user configuration before activation
            configureSettings?.Invoke(camTools);

            // Now activate - CameraActivate() will call Start* methods
            // which will read the flags we just set
            camTools.CameraActivate();
        }

        /// <summary>
        /// Sets the starting keyframe index for path playback.
        /// </summary>
        public static void SetPathingStartKeyframe(int keyframeIndex)
        {
            if (!IsAvailable) return;
            CamTools.fetch.currentKeyframeIndex = keyframeIndex;
        }

        #endregion

        #region Pathing API

        /// <summary>
        /// Selects a path by index.
        /// </summary>
        public static void SelectPath(int index)
        {
            if (!IsAvailable) return;
            CamTools.fetch.SelectPath(index);
        }

        /// <summary>
        /// Starts path playback.
        /// </summary>
        public static void StartPathPlayback()
        {
            if (!IsAvailable) return;
            CamTools.fetch.StartPathPlayback();
        }

        /// <summary>
        /// Stops path playback.
        /// </summary>
        public static void StopPathPlayback()
        {
            if (!IsAvailable) return;
            CamTools.fetch.StopPathPlayback();
        }

        /// <summary>
        /// Gets the time scale for a specific path.
        /// </summary>
        public static float GetPathTimeScale(int index)
        {
            if (!IsAvailable) return 1f;
            return CamTools.fetch.GetPathTimeScale(index);
        }

        /// <summary>
        /// Sets the time scale for a specific path.
        /// </summary>
        public static void SetPathTimeScale(int index, float scale)
        {
            if (!IsAvailable) return;
            CamTools.fetch.SetPathTimeScale(index, scale);
        }

        /// <summary>
        /// Checks if a path exists at the given index.
        /// </summary>
        public static bool PathExists(int index)
        {
            if (!IsAvailable) return false;
            return CamTools.fetch.PathExists(index);
        }

        // NEW: Pathing State Management (inserted in Pathing API)
        /// <summary>
        /// Sets the complete pathing state (index, keyframe, playback status).
        /// </summary>
        public static void SetPathState(int pathIndex, int keyframeIndex, bool isPlaying, float startTime)
        {
            if (!IsAvailable) return;
            CamTools.fetch.selectedPathIndex = pathIndex;
            CamTools.fetch.currentKeyframeIndex = keyframeIndex;
            CamTools.fetch.isPlayingPath = isPlaying;
            CamTools.fetch.pathStartTime = startTime;
        }

        /// <summary>
        /// Sets path timing options.
        /// </summary>
        public static void SetPathTiming(bool useRealTime, float smoothing)
        {
            if (!IsAvailable) return;
            CamTools.fetch.useRealTime = useRealTime;
            CamTools.fetch.pathingSecondarySmoothing = smoothing;
        }

        /// <summary>
        /// Gets the current path time (physics time or playback time depending on settings).
        /// </summary>
        public static float GetCurrentPathTime()
        {
            if (!IsAvailable) return 0f;
            return CamTools.fetch.GetPathTime();
        }

        /// <summary>
        /// Gets the total duration of a path (time of last keyframe).
        /// </summary>
        public static float GetPathDuration(int index)
        {
            if (!IsAvailable || !PathExists(index)) return 0f;
            var path = CamTools.fetch.availablePaths[index];
            if (path.times.Count == 0) return 0f;
            return path.times[path.times.Count - 1];
        }

        #endregion

        #region Deterministic Mode

        /// <summary>
        /// Updates the camera for a physics step in deterministic mode.
        /// Provide both physics delta time and playback delta time; the active mode determines which is used.
        /// </summary>
        /// <param name="physicsDeltaTime">Time.fixedDeltaTime or custom physics step</param>
        /// <param name="playbackDeltaTime">Video frame time (1/playbackFPS)</param>
        public static void PhysicsStepUpdate(float physicsDeltaTime, float playbackDeltaTime)
        {
            if (!IsAvailable) return;
            CamTools.fetch.PhysicsStepUpdate(physicsDeltaTime, playbackDeltaTime);
        }

        /// <summary>
        /// Legacy single-delta overload. Assumes physics time.
        /// </summary>
        public static void PhysicsStepUpdate(float physicsDeltaTime)
        {
            PhysicsStepUpdate(physicsDeltaTime, physicsDeltaTime);
        }

        /// <summary>
        /// Gets the current delta time being used (either deterministic or TimeWarp.fixedDeltaTime).
        /// </summary>
        public static float GetDeltaTime()
        {
            if (!IsAvailable) return TimeWarp.fixedDeltaTime;
            return CamTools.fetch.GetDeltaTime();
        }

        #endregion

        #region Events

        public static event Action OnCameraActivated
        {
            add { if (IsAvailable) CamTools.OnCameraActivated += value; }
            remove { if (IsAvailable) CamTools.OnCameraActivated -= value; }
        }

        public static event Action OnCameraDeactivated
        {
            add { if (IsAvailable) CamTools.OnCameraDeactivated += value; }
            remove { if (IsAvailable) CamTools.OnCameraDeactivated -= value; }
        }

        public static event Action OnPathingStarted
        {
            add { if (IsAvailable) CamTools.OnPathingStarted += value; }
            remove { if (IsAvailable) CamTools.OnPathingStarted -= value; }
        }

        public static event Action OnPathingStopped
        {
            add { if (IsAvailable) CamTools.OnPathingStopped += value; }
            remove { if (IsAvailable) CamTools.OnPathingStopped -= value; }
        }

        public static event Action OnCinematicRecorderControlTaken
        {
            add { if (IsAvailable) CamTools.OnCinematicRecorderControlTaken += value; }
            remove { if (IsAvailable) CamTools.OnCinematicRecorderControlTaken -= value; }
        }

        #endregion
    }
}