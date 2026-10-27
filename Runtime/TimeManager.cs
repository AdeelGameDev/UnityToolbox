using UnityEngine;
using UnityEngine.Serialization;

namespace Adeel.UnityToolbox
{
    /// <summary>
    /// Smooth slow-motion controller. <see cref="DoSlowMo()"/> drops
    /// <c>Time.timeScale</c> and the controller eases it back to 1 over
    /// <see cref="recoveryDuration"/> seconds.
    /// </summary>
    /// <remarks>
    /// <c>Time.fixedDeltaTime</c> is scaled alongside <c>timeScale</c> so physics
    /// behaves consistently while the game is slowed down, and is restored once
    /// the effect ends.
    /// </remarks>
    public class TimeManager : MonoBehaviour
    {
        [FormerlySerializedAs("slowdownFactor")]
        [Range(0.01f, 1f)]
        [Tooltip("Time scale applied while the slow-motion effect is active.")]
        [SerializeField] private float slowMotionScale = 0.05f;

        [FormerlySerializedAs("slowdownLength")]
        [Min(0.01f)]
        [Tooltip("Seconds taken to ease the time scale back to 1.")]
        [SerializeField] private float recoveryDuration = 2f;

        private float _defaultFixedDeltaTime = 0.02f;

        private void Awake()
        {
            // Capture the project's timestep before we change anything.
            _defaultFixedDeltaTime = Time.fixedDeltaTime;
        }

        private void Update()
        {
            if (Time.timeScale >= 1f)
            {
                return;
            }

            // Unscaled delta time: the recovery itself must not be slowed down.
            Time.timeScale = Mathf.MoveTowards(Time.timeScale, 1f, Time.unscaledDeltaTime / recoveryDuration);
            Time.fixedDeltaTime = _defaultFixedDeltaTime * Time.timeScale;
        }

        /// <summary>Slows time to the configured scale and begins easing back.</summary>
        public void DoSlowMo()
        {
            DoSlowMo(slowMotionScale);
        }

        /// <summary>Slows time to an explicit scale and begins easing back.</summary>
        public void DoSlowMo(float scale)
        {
            Time.timeScale = Mathf.Clamp(scale, 0.0001f, 1f);
            Time.fixedDeltaTime = _defaultFixedDeltaTime * Time.timeScale;
        }

        /// <summary>Restores normal time immediately.</summary>
        public void StopSlowMo()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = _defaultFixedDeltaTime;
        }

        private void OnDisable()
        {
            // Never leave the game slowed down after this component goes away.
            if (Time.timeScale < 1f)
            {
                StopSlowMo();
            }
        }
    }
}
