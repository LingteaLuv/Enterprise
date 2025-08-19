using UnityEngine;

namespace LHI
{
    public class BountyAssessmentCalculation : MonoBehaviour
    {
        // 역할에 따라 전투력 산정 방식이 다름
        // CombatPower를 줄여서 CP로 명명
        // 처음 AllCPCalculate 메서드로 모든 전투력을 계산하고, 이후에는 각 스탯별로 전투력을 계산하는 메서드들을 호출하여 사용
        // 생각해보니 그냥 옵저버 패턴을 사용해서 캐릭터 데이터가 변경될 때마다 전투력을 자동으로 갱신하는 것이 더 좋을 것 같음
        // 옵저버 패턴으로 구현을 실패하여 나중에 수정할 예정
        // 옵저버 가 아니라 그냥 파라미터에 겟을 정리하여 가져갈때 바로 연산하도록 고려


        public float combatPower = 0;

        private float combatPowerAttack = 0;
        public float CombatPowerAttack
        {
            get { return combatPowerAttack; }
            set { combatPowerAttack = value; }
        }

        public float combatPowerHealth = 0;
        public float combatPowerDefense = 0;
        public float combatPowerCritical = 0;
        public float combatPowerSpeed = 0;

        /// <summary>
        /// 초기화 후에 캐릭터의 모든 전투력을 계산하는 메소드, 최초에 전투력 산정을 위해서 사용
        /// </summary>
        /// <param name="characterData"></param>
        public void AllCPCalculate(CharacterData characterData)
        {
            combatPower = 0f;
            combatPowerAttack = 0f;
            combatPowerHealth = 0f;
            combatPowerDefense = 0f;
            combatPowerCritical = 0f;
            combatPowerSpeed = 0f;

            combatPowerAttack = CPAttack(characterData);
            combatPowerHealth = CPHealth(characterData);
            combatPowerDefense = CPDefense(characterData);
            combatPowerCritical = CPCritical(characterData);
            combatPowerSpeed = CPSpeed(characterData);
            combatPower = combatPowerAttack + combatPowerHealth + combatPowerDefense + combatPowerCritical + combatPowerSpeed;
        }

        #region

        public float GetCombatPower()
        {
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