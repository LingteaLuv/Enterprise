using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JHT;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DataManager : Singleton<DataManager>
{
    public List<ItemSO> AllRelics = new List<ItemSO>();
    public List<CharacterData> AllCharacters = new List<CharacterData>();
    public List<ItemSO> AllWeapons = new List<ItemSO>();

    public Action OnWeaponReady;
    public Action OnCrewReady;

    // 캐릭터 에셋을 담아둘 핸들입니다.
    private AsyncOperationHandle<IList<CharacterData>> _characterLoadHandle;
    public bool IsDataLoaded { get; private set; } = false;

    protected override void Awake()
    {
        base.Awake(); // 싱글톤 유지!
        StartCoroutine(LoadInitialData());
    }

    private IEnumerator LoadInitialData()
    {
        Debug.Log("[DataManager] 모든 캐릭터 데이터 로딩을 시작합니다...");
        _characterLoadHandle = Addressables.LoadAssetsAsync<CharacterData>("Characters", null);
        yield return _characterLoadHandle;

        if (_characterLoadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            // ToList()로 새로운 리스트를 만들어 저장해야 안전해요.
            AllCharacters = _characterLoadHandle.Result.ToList();
            Debug.Log($"[DataManager] {AllCharacters.Count}명의 캐릭터 로딩 성공!");
        }
        else
        {
            Debug.LogError("[DataManager] 캐릭터 데이터 로딩에 실패했습니다!");
        }

        // TODO: 나중에 무기나 유물 데이터도 이런 방식으로 불러올 수 있어요.

        IsDataLoaded = true;
        // 다른 시스템들에게 데이터 준비가 끝났다고 알려줍니다.
        OnWeaponReady?.Invoke();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // 게임이 완전히 종료될 때만 에셋을 메모리에서 해제합니다.
        if (_characterLoadHandle.IsValid())
        {
            Addressables.Release(_characterLoadHandle);
            Debug.Log("[DataManager] 캐릭터 데이터 핸들을 해제했습니다.");
        }
    }
}