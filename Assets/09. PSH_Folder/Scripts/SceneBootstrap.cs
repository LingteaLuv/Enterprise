using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬이 로드될 때마다 필요한 초기화 작업을 수행하는 클래스입니다.
/// 확장성을 위해 다양한 초기화 로직을 이곳에 추가할 수 있습니다.
/// </summary>
public class SceneBootstrap : Singleton<SceneBootstrap>
{
    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 씬이 로드될 때마다 호출되는 메인 함수입니다.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[{nameof(SceneBootstrap)}] '{scene.name}' 씬 로드 완료. 초기화 작업을 시작합니다.");

        // 여기에 씬이 로드될 때마다 실행하고 싶은 다른 코드들을 추가할 수 있습니다.

        InitializeParty();
    }

    /// <summary>
    /// 파티를 초기화하거나 재설정합니다.
    /// </summary>
    private void InitializeParty()
    {
        if (PartyManager.Instance != null)
        {
            PartyManager.Instance.SetupBattleParty();
        }
        else
        {
            Debug.LogWarning($"[{nameof(SceneBootstrap)}] PartyManager.Instance가 없어서 파티를 설정할 수 없습니다.");
        }
    }
}
