using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    // 인게임 BGM
    public enum Bgms
    {

    }

    // 인게임 SFX
    public enum Sfxs
    {

    }

    [Header("BGM, SFX Sound Files")]
    [SerializeField] AudioClip[] _bgmFiles;
    [SerializeField] AudioClip[] _sfxFiles;

    [Header("Audio Source Reference")]
    [SerializeField] public AudioSource _bgmAudioSource;
    [SerializeField] public AudioSource _sfxAudioSource;

    private void Start()
    {
        BgmVolume(SettingManager.Instance.BGM.Value);
        SfxVolume(SettingManager.Instance.SFX.Value);

        // 이벤트 구독
        SettingManager.Instance.BGM.OnChanged += BgmVolume;
        SettingManager.Instance.SFX.OnChanged += SfxVolume;
    }

    // 게임 종료 시 
    protected override void OnDestroy()
    {
        // 플레이 모드 확인하여 이벤트 구독 해제
        if (Application.isPlaying)
        {
            if(SettingManager.Instance != null)
            {
                if(SettingManager.Instance.BGM != null)
                {
                    SettingManager.Instance.BGM.OnChanged -= BgmVolume;
                }

                if(SettingManager.Instance.SFX != null)
                {
                    SettingManager.Instance.SFX.OnChanged -= SfxVolume;
                }
            }
        }
    }

    // BGM 플레이
    public void PlayBGM(Bgms bgm)
    {
        _bgmAudioSource.clip = _bgmFiles[(int)bgm];
        _bgmAudioSource.loop = true;
        _bgmAudioSource.Play();
    }

    // BGM 정지
    public void StopBGM()
    {
        _bgmAudioSource.Stop();
    }
    
    // SFX 플레이
    public void PlaySFX(Sfxs sfx)
    {
        _sfxAudioSource.PlayOneShot(_sfxFiles[(int)sfx]);
    }
    
    // SFX 정지
    public void StopSFX()
    {
        _sfxAudioSource.Stop();
    }
    
    // BGM 볼륨
    private void BgmVolume(float volume)
    {
        _bgmAudioSource.volume = volume;
    }

    // SFX 볼륨
    private void SfxVolume(float volume)
    {
        _sfxAudioSource.volume = volume;
    }
}

