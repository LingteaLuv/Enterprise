using UnityEngine;
using UnityEngine.TextCore.Text;

namespace LHI
{
    /// <summary>
    /// 캐릭터 오브젝트 풀 클래스
    /// </summary>
    public abstract class CharacterObjectPool : MonoBehaviour
    {
        [Header("캐릭터 풀 설정")]
        public int characterpoolSize = 5; // 캐릭터 풀의 크기
        public GameObject characterPrefab; // 캐릭터 프리팹

        // 캐릭터 매니저 참조

        void Awake()
        {
            for (int i = 0; i < characterpoolSize; i++)
            {
                GameObject characterObj = Instantiate(characterPrefab); // 캐릭터 풀에 캐릭터 오브젝트를 생성
                characterObj.transform.SetParent(transform); // 캐릭터 오브젝트를 자식으로 설정
                characterObj.SetActive(false); // 캐릭터 오브젝트를 비활성화
            }
        }

        /// <summary>
        /// 예시로 1번 캐릭터 정보 가져와 생성 (테스트용 메소드)
        /// </summary>
        [ContextMenu("테스트 실행")]
        private void ExampleCharacterCreation()
        {
            CharacterInfo testCharInfo = CharacterManager.charactersDict[1]; // 예시로 ID 1번 캐릭터 정보 가져오기
            CharacterRespawn(testCharInfo); // 아군 캐릭터 생성
        }

        /// <summary>
        /// 데이터를 기반으로 캐릭터를 생성하는 메소드
        /// </summary>
        /// <param name="charInfo">캐릭터 정보</param>
        public abstract void CharacterRespawn(CharacterInfo charInfo);

        /// <summary>
        /// 만들어진 캐릭터를 회수하는 메소드
        /// </summary>
        /// <param name="Character"></param>
        private void CharacterBring(GameObject Character)
        {
            Character.SetActive(false);
            // 캐릭터 풀에서 해당 캐릭터를 비활성화하고, 필요한 경우 데이터를 초기화
        }

        /// <summary>
        /// 비활성화된 캐릭터 오브젝트를 찾아 활성화하는 메소드
        /// </summary>
        /// <param name="charInfo"></param>
        public void FindInactiveObjects(CharacterInfo charInfo)
        {
            // 캐릭터 풀에서 비활성화 중인 오브젝트를 가져와서 캐릭터를 생성
            int characterId = charInfo.id;

            if (CharacterManager.charactersDict.ContainsKey(characterId))
            {
                // 비활성화된 캐릭터 오브젝트를 찾기
                GameObject characterObj = null;
                foreach (Transform child in transform)
                {
                    if (!child.gameObject.activeInHierarchy)
                    {
                        // 그 비활성화 된 오브젝트를 가져오기
                        characterObj = child.gameObject;
                        break; // 반복문 나가기
                    }
                }
                // 비활성화 된 캐릭터 오브젝트가 없으면 오류 로그 출력
                if (characterObj == null)
                {
                    Debug.LogError("활성화되지 않은 캐릭터 오브젝트가 없습니다. 캐릭터 풀의 크기를 늘려주세요.");
                    return;
                }
                // 캐릭터 오브젝트를 활성화
                characterObj.SetActive(true);
            }
            else
            {
                Debug.LogError($"캐릭터 ID: {characterId} 가 캐릭터 사전에 없습니다.");
            }

        }
    }
}
