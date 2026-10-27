using UnityEngine;

namespace Adeel.UnityToolbox
{
    /// <summary>
    /// Applies project-wide frame-rate and power settings once at startup.
    /// Attach to a single bootstrap <see cref="GameObject"/> in your first scene.
    /// </summary>
    /// <remarks>
    /// Disabling VSync is required for <see cref="Application.targetFrameRate"/>
    /// to have any effect on mobile and desktop platforms.
    /// </remarks>
    public class PerformanceManager : MonoBehaviour
    {
        [Tooltip("Turn off VSync so the target frame rate can be enforced.")]
        [SerializeField] private bool disableVSync = true;

        [Tooltip("Frame rate to aim for. -1 leaves the platform default untouched.")]
        [SerializeField] private int targetFrameRate = 60;

        [Tooltip("Stops the screen from dimming or sleeping during play.")]
        [SerializeField] private bool preventScreenSleep = true;

        private void Awake()
        {
            Apply();
        }

        /// <summary>Applies every configured setting immediately.</summary>
        public void Apply()
        {
            if (disableVSync)
            {
                QualitySettings.vSyncCount = 0;
            }

            if (targetFrameRate > 0)
            {
                Application.targetFrameRate = targetFrameRate;
            }

            if (preventScreenSleep)
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }
        }

        /// <summary>Changes the target frame rate at runtime and applies it.</summary>
        public void SetTargetFrameRate(int framesPerSecond)
        {
            targetFrameRate = framesPerSecond;
            Application.targetFrameRate = framesPerSecond;
        }
    }
}
