using UnityEngine;

namespace Adeel.UnityToolbox.Samples
{
    /// <summary>
    /// One named sound effect or music track: the clip plus how it should play.
    /// </summary>
    [System.Serializable]
    public class Sound
    {
        [Tooltip("Name used when calling AudioManager.Play().")]
        public string name;

        public AudioClip clip;

        [Range(0f, 1f)]
        [Tooltip("Playback volume. 1 = unchanged.")]
        public float volume = 1f;

        [Range(0.1f, 3f)]
        [Tooltip("Playback pitch. 1 = unchanged.")]
        public float pitch = 1f;

        [Tooltip("Loop this clip forever (music).")]
        public bool loop;

        [HideInInspector]
        public AudioSource source;
    }
}
