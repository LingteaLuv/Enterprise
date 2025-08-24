using UnityEngine;

namespace LHI
{
    /// <summary>
    /// 캐릭터 오브젝트 풀 클래스
    /// </summary>
    public class CharacterObjectPool : MonoBehaviour
    {
        [Header("캐릭터 풀 설정")]
        public int characterpoolSize = 10; // 캐릭터 풀의 크기
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

        void Start()
        {
            // 캐릭터 생성 테스트\
            CharacterInfo testCharInfo = CharacterManager.charactersDict[1]; // 예시로 ID 1번 캐릭터 정보 가져오기
            CharacterRespawn(testCharInfo, true); // 아군 캐릭터 생성
        }

        /// <summary>
        /// 데이터를 기반으로 캐릭터를 생성하는 메소드
        /// </summary>
        /// <param name="charInfo">캐릭터 정보</param>
        /// <param name="isPlayer">아군인지 여부를 체크하는 변수</param>
        private void CharacterRespawn(CharacterInfo charInfo, bool isPlayer)
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
                // 아군 적군에 따라 다른 컴포넌트를 추가하거나 설정
                if (isPlayer)
                {
                    gameObject.tag = "Crew"; // 태그 설정
                    // 아군 캐릭터 설정
                    // gameObject.AddComponent<>
                }
                else
                {
                    // 적군 캐릭터 설정
                }
                

                Character character = characterObj.GetComponent<Character>();

                if (character == null)
                {
                    Debug.LogError("캐릭터 오브젝트에 Character 컴포넌트가 없습니다.");
                    return;
                }
                // character.characterData = charInfo.characterData; // 캐릭터 데이터 설정
                // character.evolveLevel = charInfo.evolveLevel; // 진화 레벨 설정
                // character.upgradeLevel = charInfo.upgradeLevel; // 업그레이드 레벨 설정
                // character.isOwned = charInfo.isOwned; // 소유 여부 설정

            }
            else
            {
                Debug.LogError($"캐릭터 ID: {characterId} 가 캐릭터 사전에 없습니다.");
            }
        }

        /// <summary>
        /// 만들어진 캐릭터를 회수하는 메소드
        /// </summary>
        /// <param name="charInfo"></param>
        private void CharacterBring(CharacterInfo charInfo)
        {
            // 캐릭터 풀에서 해당 캐릭터를 비활성화하고, 필요한 경우 데이터를 초기화
        }
    }
}
