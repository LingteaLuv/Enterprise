using UnityEditor.U2D.Animation;
using UnityEngine;

namespace LHI
{
    public class CrewObjectPool : CharacterObjectPool
    {
        /// <summary>
        /// 데이터를 기반으로 캐릭터를 생성하는 메소드
        /// </summary>
        /// <param name="ID">선원 아아디</param>
        /// <param name="pos">진형 스타일</param>
        /// <param name="posNum">자리 번호</param>
        public override void CharacterRespawn(int ID, CharacterPosition pos, int posNum)
        {
            // 캐릭터 ID 유효성 검사
            if (CharacterIDCheck(ID) == false)
                return;

            // 포지션에 맞는 캐릭터 오브젝트 가져오기
            GameObject characterObj = FindObjects(posNum);

            // 오브젝트의 컴포넌트 가져오기
            Crew CrewData = characterObj.GetComponentInChildren<Crew>();

            // 캐릭터 오브젝트에 Crew 컴포넌트가 있는지 확인
            if (CrewData == null)
            {
                Debug.LogError("캐릭터 오브젝트에 Crew 컴포넌트가 없습니다.");
                return;
            }





            // 캐릭터 진화 강화 레벨 적용 charInfo 에 적용 (이 경우 스크립터블 오브젝트의 값이 변경되지 않도록 주의)

            // 적용한 수치를 CrewData에 설정
            //CrewData.characterData.characterName = charInfo.characterData.characterName;
            //CrewData.characterData.role = charInfo.characterData.role;
            //CrewData.characterData.affiliation = charInfo.characterData.affiliation;
            //CrewData.characterData.characterSprite = charInfo.characterData.characterSprite;

            //CrewData.characterData.attack = charInfo.characterData.attack;
            //CrewData.characterData.health = charInfo.characterData.health;
            //CrewData.characterData.defense = charInfo.characterData.defense;
            //CrewData.characterData.criticalChance = charInfo.characterData.criticalChance;
            //CrewData.characterData.criticalDamage = charInfo.characterData.criticalDamage;
            //CrewData.characterData.speed = charInfo.characterData.speed;

            CrewData.characterData = CharacterManager.charactersDict[ID].characterData; // 캐릭터 데이터 설정

            // 진형에 따라 캐릭터 위치 설정 pos와 posNum에 따라 위치를 다르게 설정

            // 캐릭터 오브젝트 활성화
            characterObj.SetActive(true);
        }
    }
}
