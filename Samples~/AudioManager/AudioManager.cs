using System;
using UnityEngine;

namespace Adeel.UnityToolbox.Samples
{
    /// <summary>
    /// Plays named sounds and music from a single persistent object.
    /// </summary>
    /// <remarks>
    /// Add sound entries in the Inspector, then call
    /// <c>AudioManager.instance.Play("Music")</c>. Every entry gets its own
    /// <see cref="AudioSource"/> so clips can overlap.
    /// </remarks>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;

        [Tooltip("Every sound this manager can play.")]
        public Sound[] sounds;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (Sound sound in sounds)
            {
                if (sound == null || sound.clip == null)
                {
                    Debug.LogWarning("AudioManager: an entry has no AudioClip and was skipped.", this);
                    continue;
                }

                sound.source = gameObject.AddComponent<AudioSource>();
                sound.source.clip = sound.clip;
                sound.source.volume = sound.volume;
                sound.source.pitch = sound.pitch;
                sound.source.loop = sound.loop;
            }
        }

        /// <summary>Plays the sound registered under <paramref name="name"/>.</summary>
        public void Play(string name)
        {
            Sound sound = Array.Find(sounds, entry => entry != null && entry.name == name);

            if (sound == null)
            {
                Debug.LogWarning($"AudioManager: no sound named '{name}'.", this);
                return;
            }

            if (sound.source == null)
            {
                Debug.LogWarning($"AudioManager: '{name}' has no AudioSource (was its clip assigned before Play?).", this);
                return;
            }

            sound.source.Play();
        }

        /// <summary>Stops the sound registered under <paramref name="name"/>.</summary>
        public void Stop(string name)
        {
            Sound sound = Array.Find(sounds, entry => entry != null && entry.name == name);
            sound?.source?.Stop();
        }
    }
}
