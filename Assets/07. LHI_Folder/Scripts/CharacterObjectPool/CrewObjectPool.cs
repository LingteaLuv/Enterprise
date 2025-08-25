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

            // 캐릭터 정보를 CrewData에 설정
            CrewData.characterData = CharacterManager.charactersDict[ID].characterData; // 캐릭터 데이터 설정

            // 진형에 따라 캐릭터 위치 설정 pos와 posNum에 따라 위치를 다르게 설정

            // 캐릭터 오브젝트 활성화
            characterObj.SetActive(true);
        }
    }
}
