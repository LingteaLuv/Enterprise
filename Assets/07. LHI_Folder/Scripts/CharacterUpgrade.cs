using UnityEngine;

namespace LHI
{
    public class CharacterUpgrade : MonoBehaviour
    {
        // 캐릭터 CharacterInfo 에서 강화 및 진화 레벨을 올리는 메소드

        // 강화 및 진화에 따흔 스텟 증가 적용 순서는 (강화 후 진화 적용)
        // 

        /// <summary>
        /// ID로 캐릭터 강화 레벨을 올리는 메소드 (소모 자원 체크는 추후 구현)
        /// </summary>
        /// <param name="ID"></param>
        public void CharacterStrengthen(int ID)
        {
            CharacterManager.charactersDict[ID].strengthenLevel += 1;
            // 스탯 업데이트
            CharacterStatUpdate(ID);
        }

        /// <summary>
        /// ID로 캐릭터 진화 레벨을 올리는 메소드 (소모 자원 체크는 추후 구현)
        /// </summary>
        /// <param name="ID"></param>
        public void CharacterEvolve(int ID)
        {
            CharacterManager.charactersDict[ID].evolveLevel += 1;
            // 스탯 업데이트
            CharacterStatUpdate(ID);
        }


        public void CharacterStatUpdate(int ID)
        {
            // 아이디 에서 캐릭터 정보를 가져옴 진화, 강화, 직책
            CharacterInfo characterInfo = CharacterManager.charactersDict[ID];
            CharacterInfo basecharactersInfo = CharacterManager.basecharactersDict[ID];

            int strLv = characterInfo.strengthenLevel;
            int evoLv = characterInfo.evolveLevel;
            CrewRole role = characterInfo.characterData.role;

            switch (role)
            {
                case CrewRole.Captain: // 선장
                    characterInfo.characterData.attack = (int)((basecharactersInfo.characterData.attack + (5 * strLv)) * (0.1 * evoLv + 1));
                    // 예시: 강화 레벨당 5 증가, 진화 레벨당 10% 증가
                    // 위 내용은 예시이며, 실제 게임 밸런스에 맞게 조정 필요
                    // 또한 선장의 경우는 캐릭터 마다 개별 증가치가 달라 따로 작성필요

                    break;
                case CrewRole.Boatswain: // 갑판장

                    break;
                case CrewRole.Sailor: // 선원

                    break;
                case CrewRole.Cook: // 요리사

                    break;
                default:
                    Debug.LogError("알 수 없는 직책입니다.");
                    break;
            }
        }
    }
}
