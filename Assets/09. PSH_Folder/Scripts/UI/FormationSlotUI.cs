using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FormationSlotUI : MonoBehaviour
{
    [Header("슬롯 정보")]
    public CrewRole assignedPosition;

    [System.Serializable]
    public class CharacterDisplaySet
    {
        public RawImage DisplayImage;
        public string CameraName;
        [HideInInspector] public Camera RenderCamera;
        [HideInInspector] public GameObject SpawnedCharacter;
        [HideInInspector] public RectTransform DisplayRect;
    }

    [Header("캐릭터 표시 세트 (2명)")]
    public CharacterDisplaySet[] displaySets = new CharacterDisplaySet[2];
    [SerializeField] private float modelYOffset = -0.5f; // 캐릭터 모델 Y축 위치 보정값

    private bool _isInitialized = false;

    void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_isInitialized) return;

        foreach (var set in displaySets)
        {
            if (set.DisplayImage != null)
            {
                set.DisplayRect = set.DisplayImage.GetComponent<RectTransform>();
            }

            if (!string.IsNullOrEmpty(set.CameraName))
            {
                GameObject camObj = GameObject.Find(set.CameraName);
                if (camObj != null)
                {
                    set.RenderCamera = camObj.GetComponent<Camera>();
                    if (set.RenderCamera == null) Debug.LogError($"'{set.CameraName}' 오브젝트에서 Camera 컴포넌트를 찾을 수 없습니다!", this.gameObject);
                }
                else Debug.LogError($"'{set.CameraName}' 이름의 카메라 오브젝트를 씬에서 찾을 수 없습니다!", this.gameObject);
            }
            else Debug.LogWarning("CameraName이 설정되지 않은 DisplaySet이 있습니다.", this.gameObject);
        }
        _isInitialized = true;
    }

    public void Setup(List<PlayerCharacterData> charactersInSlot)
    {
        // Setup이 Awake보다 먼저 호출될 경우를 대비하여, 초기화가 안되었다면 먼저 실행합니다.
        if (!_isInitialized) Initialize();

        // 1. 모든 세트를 초기화합니다.
        foreach (var set in displaySets)
        {
            if (set.SpawnedCharacter != null) Destroy(set.SpawnedCharacter);
            if (set.DisplayImage != null) set.DisplayImage.gameObject.SetActive(false);
        }

        if (charactersInSlot == null || charactersInSlot.Count == 0) return;

        // 2. 캐릭터 수에 맞춰 프리팹을 생성하고 활성화합니다.
        for (int i = 0; i < charactersInSlot.Count; i++)
        {
            if (i >= displaySets.Length) break;

            var set = displaySets[i];
            var characterToDisplay = charactersInSlot[i];
            var prefabToInstantiate = characterToDisplay.characterdata.characterPrefab; // 직접 프리팹 참조를 사용합니다.

            if (prefabToInstantiate != null && set.DisplayImage != null && set.RenderCamera != null)
            {
                set.DisplayImage.gameObject.SetActive(true);

                Vector3 spawnPos = set.RenderCamera.transform.position + set.RenderCamera.transform.forward * 10f;
                spawnPos.y += modelYOffset; // Y축 위치 보정
                set.SpawnedCharacter = Instantiate(prefabToInstantiate, spawnPos, set.RenderCamera.transform.rotation, set.RenderCamera.transform);
            }
        }

        // 3. UI 위치 조정
        if (charactersInSlot.Count == 1)
        {
            if (displaySets.Length > 0 && displaySets[0].DisplayRect != null) displaySets[0].DisplayRect.anchoredPosition = Vector2.zero;
        }
        else if (charactersInSlot.Count == 2)
        {
            if (displaySets.Length >= 2 && displaySets[0].DisplayRect != null && displaySets[1].DisplayRect != null)
            {
                displaySets[0].DisplayRect.anchoredPosition = new Vector2(-25f, 50f);
                displaySets[1].DisplayRect.anchoredPosition = new Vector2(25f, -50f);
            }
        }
    }
}


