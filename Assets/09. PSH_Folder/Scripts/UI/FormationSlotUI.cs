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
        public string CameraName; // 카메라 이름을 입력할 필드
        [HideInInspector] public Camera RenderCamera; // 스크립트 내부에서 사용할 카메라 참조
        [HideInInspector] public GameObject SpawnedCharacter;
        [HideInInspector] public RectTransform DisplayRect;
    }

    [Header("캐릭터 표시 세트 (2명)")]
    public CharacterDisplaySet[] displaySets = new CharacterDisplaySet[2];

    void Awake()
    {
        foreach (var set in displaySets)
        {
            // RawImage의 RectTransform을 미리 찾아둡니다.
            if (set.DisplayImage != null)
            {
                set.DisplayRect = set.DisplayImage.GetComponent<RectTransform>();
            }

            // 카메라 이름으로 오브젝트를 찾아 자동으로 연결합니다.
            if (!string.IsNullOrEmpty(set.CameraName))
            {
                GameObject camObj = GameObject.Find(set.CameraName);
                if (camObj != null)
                {
                    set.RenderCamera = camObj.GetComponent<Camera>();
                    if (set.RenderCamera == null)
                    {
                        Debug.LogError($"'{set.CameraName}' 오브젝트에서 Camera 컴포넌트를 찾을 수 없습니다!", this.gameObject);
                    }
                }
                else
                {
                    Debug.LogError($"'{set.CameraName}' 이름의 카메라 오브젝트를 씬에서 찾을 수 없습니다!", this.gameObject);
                }
            }
            else
            {
                Debug.LogWarning("CameraName이 설정되지 않은 DisplaySet이 있습니다.", this.gameObject);
            }
        }
    }

    public void Setup(List<PlayerCharacterData> charactersInSlot)
    {
        // 1. 모든 세트를 초기화합니다. (이전 캐릭터 삭제, 이미지 숨김)
        foreach (var set in displaySets)
        {
            if (set.SpawnedCharacter != null)
            {
                Destroy(set.SpawnedCharacter);
            }
            if (set.DisplayImage != null)
            {
                set.DisplayImage.gameObject.SetActive(false);
            }
        }

        if (charactersInSlot == null || charactersInSlot.Count == 0) return;

        // 2. 캐릭터 수에 맞춰 프리팹을 생성하고 활성화합니다.
        for (int i = 0; i < charactersInSlot.Count; i++)
        {
            if (i >= displaySets.Length) break; // 2명까지만 처리

            var set = displaySets[i];
            var characterToDisplay = charactersInSlot[i];
            var prefabToInstantiate = characterToDisplay.characterdata.characterPrefab;

            if (prefabToInstantiate != null && set.DisplayImage != null && set.RenderCamera != null)
            {
                set.DisplayImage.gameObject.SetActive(true);

                // 카메라 위치를 기준으로 스폰 위치를 계산합니다. (카메라 앞 10유닛)
                Vector3 spawnPos = set.RenderCamera.transform.position + set.RenderCamera.transform.forward * 10f;

                // 계산된 위치에 캐릭터를 생성하고, 정리를 위해 카메라의 자식으로 만듭니다.
                set.SpawnedCharacter = Instantiate(prefabToInstantiate, spawnPos, set.RenderCamera.transform.rotation, set.RenderCamera.transform);
            }
        }

        // 3. 캐릭터 수에 따라 위치를 조정합니다.
        if (charactersInSlot.Count == 1)
        {
            // 1명일 때: 첫 번째 세트를 중앙에 배치
            if (displaySets.Length > 0 && displaySets[0].DisplayRect != null)
            {
                displaySets[0].DisplayRect.anchoredPosition = Vector2.zero;
            }
        }
        else if (charactersInSlot.Count == 2)
        {
            // 2명일 때: 두 세트를 나란히 배치
            if (displaySets.Length >= 2 && displaySets[0].DisplayRect != null && displaySets[1].DisplayRect != null)
            {
                displaySets[0].DisplayRect.anchoredPosition = new Vector2(-25f, 50f);
                displaySets[1].DisplayRect.anchoredPosition = new Vector2(25f, -50f);
            }
        }
    }
}

