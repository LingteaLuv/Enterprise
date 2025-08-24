using UnityEngine;

namespace LHI
{
    public class CrewObjectPool : CharacterObjectPool
    {
        /// <summary>
        /// 데이터를 기반으로 캐릭터를 생성하는 메소드
        /// </summary>
        /// <param name="charInfo">캐릭터 정보</param>
        public override void CharacterRespawn(CharacterInfo charInfo)
        {
            // 캐릭터 풀에서 비활성화 중인 오브젝트를 찾아 활성화
            FindInactiveObjects(charInfo);
            // 프리팹에서 캐릭터 컴포넌트 가져오기
            Debug.Log("CrewObjectPool - CharacterRespawn called");

            Crew CrewData = this.transform.Find("CrewData").GetComponent<Crew>();
            // 캐릭터 오브젝트에 Crew 컴포넌트가 있는지 확인
            if (CrewData == null)
            {
                Debug.LogError("캐릭터 오브젝트에 Crew 컴포넌트가 없습니다.");
                return;
            }

            // 캐릭터 진화 강화 래벌 적용
            // 수치 가져옴

            Debug.Log($"Character Info ID: {charInfo.id}, Name: {charInfo.characterData.characterName}");
            // 캐릭터 데이터 설정 (강화 및 진화 수치 반영 필요)
            CrewData.characterData.characterName = charInfo.characterData.characterName;
            CrewData.characterData.role = charInfo.characterData.role;
            CrewData.characterData.affiliation = charInfo.characterData.affiliation;
            CrewData.characterData.characterSprite = charInfo.characterData.characterSprite;

            CrewData.characterData.attack = charInfo.characterData.attack;
            CrewData.characterData.health = charInfo.characterData.health;
            CrewData.characterData.defense = charInfo.characterData.defense;
            CrewData.characterData.criticalChance = charInfo.characterData.criticalChance;
            CrewData.characterData.criticalDamage = charInfo.characterData.criticalDamage;
            CrewData.characterData.speed = charInfo.characterData.speed;


        }
    }
}
