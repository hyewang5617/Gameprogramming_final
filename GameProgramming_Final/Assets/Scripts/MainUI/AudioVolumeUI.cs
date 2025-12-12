// Assets/Scripts/MainUI/AudioVolumeUI.cs (새로 추가)
using UnityEngine;
using UnityEngine.UI;

public class AudioVolumeUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Image musicFill;   // 슬라이더 Fill 이미지 연결
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Image sfxFill;     // 슬라이더 Fill 이미지 연결

    private AudioManager audioManager;

    void Start()
    {
        audioManager = AudioManager.Instance ?? FindObjectOfType<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogWarning("[AudioVolumeUI] AudioManager를 찾지 못했습니다.");
            enabled = false;
            return;
        }

        // 초기값 세팅
        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.value = audioManager.musicVolume;
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
            UpdateFill(musicFill, musicSlider.value);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.value = audioManager.sfxVolume;
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            UpdateFill(sfxFill, sfxSlider.value);
        }
    }

    void OnDestroy()
    {
        if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
    }

    private void OnMusicChanged(float v)
    {
        audioManager.SetMusicVolume(v);
        UpdateFill(musicFill, v);
    }

    private void OnSfxChanged(float v)
    {
        audioManager.SetSFXVolume(v);
        UpdateFill(sfxFill, v);
    }

    private void UpdateFill(Image fill, float value)
    {
        if (fill == null) return;
        // 슬라이더 Fill 이미지의 fillAmount를 업데이트
        fill.fillAmount = Mathf.Clamp01(value);
    }
}
