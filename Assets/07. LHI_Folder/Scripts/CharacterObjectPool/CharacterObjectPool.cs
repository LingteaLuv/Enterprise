using UnityEngine;

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

        void Awake()    
        {
            // 캐릭터 풀 오브젝트 생성
            for (int i = 0; i < characterpoolSize; i++)
            {
                GameObject characterObj = Instantiate(characterPrefab); // 캐릭터 풀에 캐릭터 오브젝트를 생성
                characterObj.transform.SetParent(transform); // 캐릭터 오브젝트를 자식으로 설정
                characterObj.name = $"{characterPrefab.name}_Pool_{i}"; // 캐릭터 오브젝트 이름 설정
                characterObj.SetActive(false); // 캐릭터 오브젝트를 비활성화
            }
        }

        // 생성 메소드 (추상 메소드로 선언, 상속받은 클래스에서 구현)
        public abstract void CharacterRespawn(int ID, CharacterPosition pos, int num);

        /// <summary>
        /// 예시로 1번 캐릭터 정보 가져와 생성 (테스트용 메소드)
        /// 1번 캐릭터 아군을 전방 포지션 형태에 1번으로 생성
        /// </summary>
        [ContextMenu("테스트 실행")]
        private void ExampleCharacterCreation()
        {
            CharacterInfo testCharInfo = CharacterManager.charactersDict[1]; // 예시로 ID 1번 캐릭터 정보 가져오기
            CharacterRespawn(1, CharacterPosition.Forward, 0); // 1아이디 아군 캐릭터 생성
        }

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
        /// 넘버에 맞는 캐릭터 오브젝트를 찾는 메소드
        /// </summary>
        /// <param name="posNum"></param>
        public GameObject FindObjects(int posNum)
        {
            // posNum에 해당하는 인덱스의 오브젝트를 가져옴
            GameObject characterObj = transform.GetChild(posNum).gameObject;

            // 비활성화 된 캐릭터 오브젝트가 없으면 오류 로그 출력
            if (characterObj == null)
            {
                Debug.LogError("오브젝트를 찾을 수 없습니다.");
                return null;
            }
            return characterObj;
        }

        /// <summary>
        /// ID에 맞는 캐릭터가 Dictionary에 있는지 확인하는 메소드
        /// </summary>
        /// <param name="ID"></param>
        public bool CharacterIDCheck(int ID)
        {
            if (CharacterManager.charactersDict.ContainsKey(ID))
            {
                return true;
            }
            else
            {
                Debug.LogError($"캐릭터 ID: {ID} 가 캐릭터 사전에 없습니다.");
                return false;
            }
        }
    }
}
