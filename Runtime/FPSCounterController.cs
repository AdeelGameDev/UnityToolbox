using UnityEngine;
using UnityEngine.UI;

namespace Adeel.UnityToolbox
{
    /// <summary>
    /// Draws a small FPS readout in a <see cref="Text"/> label and optionally
    /// pins the project frame rate.
    /// </summary>
    /// <remarks>
    /// The label is refreshed a few times per second rather than every frame,
    /// which keeps the per-frame string allocation (and the garbage it creates)
    /// negligible.
    /// </remarks>
    public class FPSCounterController : MonoBehaviour
    {
        [Tooltip("Show the live FPS value. When disabled the label is cleared.")]
        public bool showFPSCounter = true;

        [Tooltip("Apply VSync-off and the target frame rate on Awake.")]
        public bool setFrameRate = true;

        [Tooltip("Frame rate to target when 'Set Frame Rate' is enabled.")]
        public int targetFrameRate = 60;

        [Tooltip("Label that receives the FPS text.")]
        public Text fpsCounterText;

        [Tooltip("How often the label is refreshed, in seconds.")]
        [Min(0.05f)]
        public float refreshInterval = 0.25f;

        private float _accumulatedTime;
        private int _accumulatedFrames;
        private int _lastFps;

        private void Awake()
        {
            if (!setFrameRate)
            {
                return;
            }

            QualitySettings.vSyncCount = 0;

            if (targetFrameRate > 0)
            {
                Application.targetFrameRate = targetFrameRate;
            }
        }

        private void Update()
        {
            if (!showFPSCounter)
            {
                if (fpsCounterText != null && fpsCounterText.text.Length > 0)
                {
                    fpsCounterText.text = string.Empty;
                }

                return;
            }

            _accumulatedTime += Time.unscaledDeltaTime;
            _accumulatedFrames++;

            if (_accumulatedTime < refreshInterval)
            {
                return;
            }

            // Round rather than truncate so a steady 60 reads as 60, not 59.
            _lastFps = Mathf.RoundToInt(_accumulatedFrames / _accumulatedTime);
            _accumulatedTime = 0f;
            _accumulatedFrames = 0;

            if (fpsCounterText != null)
            {
                fpsCounterText.text = $"FPS: {_lastFps}";
            }
        }

        /// <summary>The most recently measured frame rate.</summary>
        public int CurrentFps => _lastFps;
    }
}
