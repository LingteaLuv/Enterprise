using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SoundManager : Singleton<SoundManager>
{
    private Dictionary<string, AudioClip> _bgmClips = new();
    private Dictionary<string, AudioInfo> _bgmList = new();

    private float _masterBgmVolume;
    private float _masterSfxVolume;
    private string _bgmURL;

    private string _curBgmKey;
    
    public class AudioInfo
    {
        public string Name;
        public string AddressKey;
        public float Volume;
    }

    [Header("Audio Source Reference")]
    [SerializeField] public AudioSource _bgmAudioSource;
    [SerializeField] public AudioSource _sfxAudioSource;

    
    private void Start()
    {
        StartCoroutine(LoadDataCsv(_bgmURL, (parsedCsv) => StartCoroutine(LoadAudioInfo(parsedCsv))));
        
        BgmVolume(SettingManager.Instance.BGM.Value);
        SfxVolume(SettingManager.Instance.SFX.Value);

        // 이벤트 구독
        SettingManager.Instance.BGM.OnChanged += BgmVolume;
        SettingManager.Instance.SFX.OnChanged += SfxVolume;
    }

    // 게임 종료 시 
    protected override void OnDestroy()
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

    public IEnumerator LoadAudioInfo(string[][] parsedCsv)
    {
        for (int i = 0; i < parsedCsv.Length; i++)
        {
            var row = parsedCsv[i];

            AudioInfo info = new AudioInfo
            {
                Name = row[0],
                AddressKey = row[1],
                Volume = float.Parse(row[2])
            };
            
            AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(info.AddressKey);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _bgmClips[info.AddressKey] = handle.Result;
                _bgmList[info.AddressKey] = info;
                Debug.Log($"로드 성공 {info.Name}");
            }
            else
            {
                Debug.Log($"로드 실패 {info.AddressKey}");
            }
        }
    }
    
    private IEnumerator LoadDataCsv(string url, Action<string[][]> onParsed, int startLine = 1)
    {
        using UnityWebRequest www = UnityWebRequest.Get(url);

        yield return www.SendWebRequest();

        if (!string.IsNullOrEmpty(www.error))
            yield break;

        string raw = www.downloadHandler.text.Trim();
        string[] lines = raw.Split('\n');
        List<string[]> parsed = new();

        for (int i = startLine - 1; i < lines.Length; i++)
        {
            string[] row = lines[i].Trim().Split(',');
            parsed.Add(row);
        }

        onParsed?.Invoke(parsed.ToArray());
    }
    
    // BGM 플레이
    public void PlayBGM(string key)
    {
        if (_curBgmKey == key) return;
        StopBGM();
        
        if (_bgmClips.TryGetValue(key, out AudioClip clip) && _bgmList.TryGetValue(key, out AudioInfo info))
        {
            _bgmAudioSource.clip = clip;
            _bgmAudioSource.volume = _masterBgmVolume * info.Volume;
            _bgmAudioSource.loop = true;
            _bgmAudioSource.Play();
            _curBgmKey = key;
        }
    }

    // BGM 정지
    public void StopBGM()
    {
        _bgmAudioSource.Stop();
        _curBgmKey = null;
    }
    
    // BGM 볼륨
    private void BgmVolume(float volume)
    {
        _masterBgmVolume = volume;
        SetVolume(_masterBgmVolume);
    }

    // SFX 볼륨
    private void SfxVolume(float volume)
    {
        _masterSfxVolume = volume;
        SetVolume(_masterSfxVolume);
    }

    private void SetVolume(float volume)
    {
        if (_bgmAudioSource.isPlaying && _bgmAudioSource.clip != null && _curBgmKey != null)
        {
            var data = _bgmList[_curBgmKey];
            if (data != null)
            {
                _bgmAudioSource.volume = volume * data.Volume;
            }
        }
    }
}

