using UnityEngine;

namespace LHI
{
    public class Bounty : MonoBehaviour
    {
        // 리스트 캐릭터 정보(편성된)를 넣으면 점수로 반환 받는 방식
        // 메서드로 활용
        // 초기 전투력 산정은 AllCPCalculate 메서드를 사용
        // 편성된 캐릭터의 스탯 변경시 재계산

        public float combatPower = 0;

        #region

        /// <summary>
        /// 편성 된 모든 캐릭터의 전투력을 계산하는 메소드
        /// </summary>
        /// <param name="characterData"></param>
        public float AllCPCalculate(CharacterData[] characterData)
        {
            // 초기화
            float oldCombatPower = combatPower;
            combatPower = 0;

            foreach (var character in characterData)
            {
                // 캐릭터가 null인 경우는 제외
                if (character == null)
                {
                    continue;
                }

                combatPower += AllCPCalculate(character);
            }

            float index = combatPower - oldCombatPower;

            // ui 나 신호 추가
            if (index < 0)
            {
                Debug.Log($"{oldCombatPower}에서 {combatPower} 으로 변경, 전투력이 감소했습니다. 감소량: {Mathf.Abs(index)}");
            }
            else if (index > 0)
            {
                Debug.Log($"{oldCombatPower}에서 {combatPower} 으로 변경, 전투력이 증가했습니다. 증가량: {index}");
            }
            else
            {
                Debug.Log($"{oldCombatPower}에서 {combatPower} 으로 변경, 전투력이 변동이 없습니다.");
            }

            return combatPower;
        }

        /// <summary>
        /// 한 캐릭터의 전투력을 계산하는 메소드
        /// </summary>
        /// <param name="characterData"></param>
        /// <returns></returns>
        public float AllCPCalculate(CharacterData characterData)
        {
            float CP =
                CPAttack(characterData) +
                CPHealth(characterData) +
                CPDefense(characterData) +
                CPCritical(characterData) +
                CPSpeed(characterData);
            combatPower += CP;
            return combatPower;
        }
        #endregion

        #region 스탯별 각 전투력 산정 메소드들
        /// <summary>
        /// 초기화 후 전투력 산정 공격력 부분을 계산하는 메소드
        /// </summary>
        /// <param name="characterData"></param>
        /// <returns></returns>
        public float CPAttack(CharacterData characterData)
        {
            switch (characterData.role)
            {
                case Role.Captain:
                    return characterData.attack * 1.5f;
                case Role.Boatswain:
                    return characterData.attack * 0.2f;
                case Role.Sailor:
                    return characterData.attack * 1.2f;
                case Role.Cook:
                    return characterData.attack;
                default:
                    Debug.Log("캐릭터의 역할이 없습니다. 0을 반환합니다. (전투력 산출 공격력 부분)");
                    return 0f;
            }
        }

        /// <summary>
        /// 초기화 후 전투력 산정 체력 부분을 계산하는 메소드
        /// </summary>
        /// <param name="characterData"></param>
        /// <returns></returns>
        public float CPHealth(CharacterData characterData)
        {
            switch (characterData.role)
            {
                case Role.Captain:
                    return characterData.health * 0.3f;
                case Role.Boatswain:
                    return characterData.health * 0.4f;
                case Role.Sailor:
                    return characterData.health * 0.3f;
                case Role.Cook:
                    return characterData.health * 0.4f;
                default:
                    Debug.Log("캐릭터의 역할이 없습니다. 0을 반환합니다. (전투력 산출 체력 부분)");
                    return 0f;
            }
        }

        /// <summary>
        /// 초기화 후 전투력 산정 방어력 부분을 계산하는 메소드
        /// </summary>
        /// <param name="characterData"></param>
        /// <returns></returns>
        public float CPDefense(CharacterData characterData)
        {
            switch (characterData.role)
            {
                case Role.Captain:
                    return characterData.defense * 0.3f;
                case Role.Boatswain:
                    return characterData.defense * 0.4f;
                case Role.Sailor:
                    return characterData.defense * 0.2f;
                case Role.Cook:
                    return characterData.defense * 0.3f;
                default:
                    Debug.Log("캐릭터의 역할이 없습니다. 0을 반환합니다. (전투력 산출 방어력 부분)");
                    return 0f;
            }
        }

        /// <summary>
        /// 초기화 후 전투력 산정 치명타(치명타 확률 및 치명타 데미지) 부분을 계산하는 메소드
        /// </summary>
        /// <param name="characterData"></param>
        /// <returns></returns>
        public float CPCritical(CharacterData characterData)
        {
            switch (characterData.role)
            {
                case Role.Captain:
                    return (characterData.criticalChance * characterData.criticalDamage / 200) * 0.8f;
                case Role.Boatswain:
                    return (characterData.criticalChance * characterData.criticalDamage / 200) * 0.1f;
                case Role.Sailor:
                    return (characterData.criticalChance * characterData.criticalDamage / 200) * 0.8f;
                case Role.Cook:
                    return (characterData.criticalChance * characterData.criticalDamage / 200) * 0.3f;
                default:
                    Debug.Log("캐릭터의 역할이 없습니다. 0을 반환합니다. (전투력 산출 치명타 확률 부분)");
                    return 0f;
            }

        }

        /// <summary>
        /// 초기화 후 전투력 산정 속도 부분을 계산하는 메소드
        /// </summary>
        /// <param name="characterData"></param>
        /// <returns></returns>
        public float CPSpeed(CharacterData characterData)
        {
            switch (characterData.role)
            {
                case Role.Captain:
                    return characterData.speed * 1000f;
                case Role.Boatswain:
                    return characterData.speed * 1000f;
                case Role.Sailor:
                    return characterData.speed * 1000f;
                case Role.Cook:
                    return characterData.speed * 1000f;
                default:
                    Debug.Log("캐릭터의 역할이 없습니다. 0을 반환합니다. (전투력 산출 속도 부분)");
                    return 0f;
            }
        }
        #endregion
    }
}