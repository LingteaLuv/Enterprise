using UnityEngine;
using System.Collections.Generic;

namespace LHI
{
    public class CharacterManager : MonoBehaviour
    {
        // 싱글톤, 캐릭터 정보를 가지는 매니저 클래스
        // 저장 및 불러오기, 캐릭터 진화 및 강화, 캐릭터 생성 및 회수 등의 기능을 포함 (스폰기능은 CharacterPool로 분리)


        public static CharacterManager Instance { get; private set; }

        public List<CharacterInfo> characters = new List<CharacterInfo>();

        public static Dictionary<int , CharacterInfo> charactersDict;


        public void Awake()
        {
            // 싱글톤 패턴 구현
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            // characters 리스트를 Dictionary로 변환
            charactersDict = new Dictionary<int, CharacterInfo>();
            foreach (var character in characters)
            {
                if (!charactersDict.ContainsKey(character.id))
                {
                    charactersDict.Add(character.id, character);
                }
                else
                {
                    Debug.LogWarning($"ID {character.id}가 이미 사전에 있습니다.");
                }
            }

            characters = null; // 메모리 절약을 위해 초기화
        }

        /// <summary>
        /// id로 캐릭터 정보를 가져오는 메소드
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CharacterInfo GetCharacter(int id)
        {
            if (charactersDict.TryGetValue(id, out CharacterInfo characterInfo))
            {
                return characterInfo;
            }
            else
            {
                Debug.LogWarning($"캐릭터 ID: {id} 를 찾을 수 없습니다.");
                return null;
            }
        }

        #region 저장 및 불러오기

        private void Save()
        {
            // 게임 시작 시 캐릭터 데이터를 불러오는 로직
        }

        private void Load()
        {
            // 게임 종료 시 캐릭터 데이터를 저장하는 로직
        }

        #endregion

        #region 캐릭터 진화 및 강화

        private void CharacterEvolve()
        {

        }

        private void CharacterUpgrade()
        {

        }

        #endregion

    }
}