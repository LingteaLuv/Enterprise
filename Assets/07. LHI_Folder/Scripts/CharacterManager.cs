using UnityEngine;
using System.Collections.Generic;

namespace LHI
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance { get; private set; }

        public List<CharacterInfo> characters = new List<CharacterInfo>();

        public Dictionary<int , CharacterInfo> charactersDict;

        public void Awake()
        {
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
        private CharacterInfo GetCharacter(int id)
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

        #region 캐릭터 생성 및 회수

        /// <summary>
        /// 데이터를 기반으로 캐릭터를 생성하는 메소드
        /// </summary>
        /// <param name="charInfo"></param>
        private void CharacterRespawn(CharacterInfo charInfo)
        {

        }


        /// <summary>
        /// 만들어진 캐릭터를 회수하는 메소드
        /// </summary>
        /// <param name="charInfo"></param>
        private void CharacterBring(CharacterInfo charInfo)
        {

        }

        private void CharacterPoolSet(CharacterInfo charInfo)
        {

        }


        private void OnEnable()
        {

        }

        private void OnDisable()
        {
            
        }


    }
}