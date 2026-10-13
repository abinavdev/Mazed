using UnityEngine;

namespace Mazed.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private const string PREF_KEY_MUSIC_MUTED = "Mazed_MusicMuted";
        private const string PREF_KEY_SFX_MUTED = "Mazed_SfxMuted";
        private const string PREF_KEY_MUSIC_VOL = "Mazed_MusicVol";
        private const string PREF_KEY_SFX_VOL = "Mazed_SfxVol";

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Background Music Tracks")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip suspenseMusic;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip keyCollectSFX;
        [SerializeField] private AudioClip doorUnlockSFX;
        [SerializeField] private AudioClip correctQuizSFX;
        [SerializeField] private AudioClip incorrectQuizSFX;
        [SerializeField] private AudioClip buttonClickSFX;
        [SerializeField] private AudioClip victorySFX;
        [SerializeField] private AudioClip gameOverSFX;

        [Header("Volume Settings")]
        [Range(0f, 1f)] public float musicVolume = 0.6f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        public bool isMusicMuted { get; private set; } = false;
        public bool isSfxMuted { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Load saved settings
            isMusicMuted = PlayerPrefs.GetInt(PREF_KEY_MUSIC_MUTED, 0) == 1;
            isSfxMuted = PlayerPrefs.GetInt(PREF_KEY_SFX_MUTED, 0) == 1;
            musicVolume = PlayerPrefs.GetFloat(PREF_KEY_MUSIC_VOL, 0.6f);
            sfxVolume = PlayerPrefs.GetFloat(PREF_KEY_SFX_VOL, 0.8f);

            // Ensure AudioSources exist
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            UpdateAudioStates();
        }

        private void Start()
        {
            PlayBGM(backgroundMusic);
        }

        public void UpdateAudioStates()
        {
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
                musicSource.mute = isMusicMuted;
            }
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
                sfxSource.mute = isSfxMuted;
            }
        }

        public void ToggleMusicMute()
        {
            isMusicMuted = !isMusicMuted;
            if (musicSource != null) musicSource.mute = isMusicMuted;
            PlayerPrefs.SetInt(PREF_KEY_MUSIC_MUTED, isMusicMuted ? 1 : 0);
            PlayerPrefs.Save();
            Debug.Log($"[AudioManager] Music mute toggled: {isMusicMuted}");
        }

        public void ToggleSfxMute()
        {
            isSfxMuted = !isSfxMuted;
            if (sfxSource != null) sfxSource.mute = isSfxMuted;
            PlayerPrefs.SetInt(PREF_KEY_SFX_MUTED, isSfxMuted ? 1 : 0);
            PlayerPrefs.Save();
            Debug.Log($"[AudioManager] SFX mute toggled: {isSfxMuted}");
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null) musicSource.volume = musicVolume;
            PlayerPrefs.SetFloat(PREF_KEY_MUSIC_VOL, musicVolume);
            PlayerPrefs.Save();
        }

        public void SetSfxVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null) sfxSource.volume = sfxVolume;
            PlayerPrefs.SetFloat(PREF_KEY_SFX_VOL, sfxVolume);
            PlayerPrefs.Save();
        }

        public void PlayBGM(AudioClip clip)
        {
            if (clip == null || musicSource == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;

            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.mute = isMusicMuted;
            musicSource.Play();
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (isSfxMuted) return;
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
            }
        }

        public void PlayKeyCollectSFX() => PlaySFX(keyCollectSFX);
        public void PlayDoorUnlockSFX() => PlaySFX(doorUnlockSFX);
        public void PlayCorrectQuizSFX() => PlaySFX(correctQuizSFX);
        public void PlayIncorrectQuizSFX() => PlaySFX(incorrectQuizSFX);
        public void PlayButtonClickSFX() => PlaySFX(buttonClickSFX, 0.7f);
        public void PlayVictorySFX() => PlaySFX(victorySFX);
        public void PlayGameOverSFX() => PlaySFX(gameOverSFX);
        public void PlayHintSFX() => PlaySFX(keyCollectSFX != null ? keyCollectSFX : correctQuizSFX);
    }
}
