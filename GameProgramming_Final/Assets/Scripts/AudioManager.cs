using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    
    [System.Serializable]
    public struct SceneMusic
    {
        public string sceneName;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    [Header("Scene Music Map")]
    public SceneMusic[] sceneMusics;
    public AudioClip fallbackMusic;
    [Range(0f, 1f)] public float defaultMusicVolume = 0.5f;
    [Header("Global Volumes")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Header("Game Event SFX")]
    public AudioClip levelCompleteClip;
    public AudioClip gameOverClip;
    [Header("UI SFX")]
    public AudioClip buttonClickClip;
    public AudioClip purchaseClip;

    float currentMusicBaseVolume = 0.5f;
    float currentLoopingSfxBaseVolume = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void PlayMusic(AudioClip clip, float volume = 0.5f)
    {
        if (clip == null || musicSource == null) return;
        currentMusicBaseVolume = Mathf.Clamp01(volume);
        musicSource.loop = true;
        musicSource.clip = clip;
        ApplyMusicVolume();
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume) * sfxVolume);
    }

    /// <summary>
    /// 공유 SFX 소스로 재생하지만 loop를 끄고 clip을 설정해둔 뒤 Stop으로 중간 정지 가능.
    /// 다른 루프 SFX가 이미 돌고 있으면 방해하지 않고 OneShot으로 재생한다.
    /// </summary>
    public void PlayStoppableSFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;

        // 루프 SFX가 점유 중이면 덮어쓰지 말고 OneShot만 재생
        if (sfxSource.loop && sfxSource.isPlaying && sfxSource.clip != null && sfxSource.clip != clip)
        {
            PlaySFX(clip, volume);
            return;
        }

        sfxSource.loop = false;
        sfxSource.clip = clip;
        sfxSource.volume = Mathf.Clamp01(volume) * sfxVolume;
        sfxSource.Play();
    }

    public void StopStoppableSFX(AudioClip clip = null)
    {
        if (sfxSource == null) return;
        // 루프 상태면 건드리지 않는다
        if (sfxSource.loop) return;
        if (clip != null && sfxSource.clip != clip) return;
        sfxSource.Stop();
        sfxSource.clip = null;
    }

    public void PlayLoopingSFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        if (sfxSource == null) return;

        if (sfxSource.isPlaying && sfxSource.clip == clip)
        {
            currentLoopingSfxBaseVolume = Mathf.Clamp01(volume);
            ApplyLoopingSfxVolume(currentLoopingSfxBaseVolume);
            return;
        }

        sfxSource.loop = true;
        sfxSource.clip = clip;
        currentLoopingSfxBaseVolume = Mathf.Clamp01(volume);
        ApplyLoopingSfxVolume(currentLoopingSfxBaseVolume);
        sfxSource.Play();
    }

    public void StopLoopingSFX(AudioClip clip = null)
    {
        if (sfxSource == null) return;
        if (clip != null && sfxSource.clip != null && sfxSource.clip != clip) return;
        sfxSource.Stop();
        sfxSource.loop = false;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    public void PlayMusicForScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        AudioClip targetClip = null;
        float volume = defaultMusicVolume;

        foreach (var sm in sceneMusics)
        {
            if (string.IsNullOrEmpty(sm.sceneName)) continue;
            if (sceneName.Contains(sm.sceneName) || sm.sceneName.Contains(sceneName))
            {
                targetClip = sm.clip;
                volume = sm.volume > 0f ? sm.volume : defaultMusicVolume;
                break;
            }
        }

        if (targetClip != null)
        {
            PlayMusic(targetClip, volume);
        }
        else if (fallbackMusic != null)
        {
            PlayMusic(fallbackMusic, defaultMusicVolume);
        }
        else
        {
            StopMusic();
        }
    }

    public void SetMusicVolume(float normalized)
    {
        musicVolume = Mathf.Clamp01(normalized);
        ApplyMusicVolume();
    }

    public void SetSFXVolume(float normalized)
    {
        sfxVolume = Mathf.Clamp01(normalized);
        ApplyLoopingSfxVolume(currentLoopingSfxBaseVolume);
    }

    // --- UI helpers ---
    public void PlayButtonClick(float volume = 1f) => PlaySFX(buttonClickClip, volume);
    public void PlayPurchase(float volume = 1f) => PlaySFX(purchaseClip, volume);
    public void PlayLevelCompleteSFX(float volume = 1f) => PlaySFX(levelCompleteClip, volume);
    public void PlayGameOverSFX(float volume = 1f) => PlaySFX(gameOverClip, volume);

    void ApplyMusicVolume()
    {
        if (musicSource == null) return;
        musicSource.volume = Mathf.Clamp01(currentMusicBaseVolume * musicVolume);
    }

    void ApplyLoopingSfxVolume(float baseVolume)
    {
        if (sfxSource == null) return;
        sfxSource.volume = Mathf.Clamp01(Mathf.Clamp01(baseVolume) * sfxVolume);
    }
}
