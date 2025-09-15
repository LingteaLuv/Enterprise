using UnityEngine;

// 각종 알림(레드닷) 조건들을 중앙에서 관리하는 싱글톤 클래스
public class NotificationManager : Singleton<NotificationManager>
{
    /// <summary>
    /// 특정 캐릭터의 성급(별)을 업그레이드할 수 있는지 확인합니다.
    /// </summary>
    /// <param name="character">확인할 캐릭터 데이터</param>
    /// <returns>업그레이드 가능하면 true, 아니면 false</returns>
    public bool CanUpgradeStar(PlayerCharacterData character)
    {
        if (character == null) return false;

        // 1. 최대 성급인지 확인 (5성이라고 가정)
        if (character.stars.Value >= 5)
        {
            return false;
        }

        // 2. 다음 성급으로 업그레이드하는 데 필요한 비용(영혼 조각 수)을 가져옵니다.
        if (PlayerDataManager.Instance.TryGetUpgradeCost(character.stars.Value, out int requiredCost))
        {
            // 3. 현재 보유한 영혼 조각 수를 가져옵니다.
            PlayerDataManager.Instance.characterSoulFragments.TryGetValue(character.characterdata.characterID, out int currentFragments);

            // 4. 보유량이 필요량보다 많거나 같으면 true를 반환합니다.
            return currentFragments >= requiredCost;
        }

        // 업그레이드 비용 정보가 없으면 불가능으로 처리
        return false;
    }

    // 추후 다른 알림 조건들을 여기에 추가할 수 있습니다.
    /// <summary>
    /// 특정 캐릭터를 레벨업할 수 있는지 확인합니다.
    /// </summary>
    public bool CanLevelUp(PlayerCharacterData character)
    {
        // 코드 바꾸면서 이상해짐 나중에 수정
        return false;
        if (character == null) return false;

        // 레벨업 비용 계산
        //double costDouble = (double)PlayerDataManager.Instance.baseLevelUpCost * System.Math.Pow(PlayerDataManager.Instance.levelUpCostIncreaseRatio, character.characterLevel - 1);
        //System.Numerics.BigInteger requiredCost = (System.Numerics.BigInteger)costDouble;

        // 현재 보유 재화와 비교
       // return CurrencyManager.Instance.GetCurrency(CurrencyType.EnhancementStone) >= requiredCost;
    }

    /// <summary>
    /// 승급 또는 레벨업이 가능한 캐릭터가 한 명이라도 있는지 확인합니다. (상위 레드닷용)
    /// </summary>
    public bool ShouldShowOverallCharacterRedDot()
    {
        // 모든 보유 캐릭터를 순회합니다.
        foreach (var character in PlayerDataManager.Instance.ownedCharacters.Values)
        {
            // 승급이 가능한지 확인
            if (CanUpgradeStar(character))
            {
                return true; // 가능한 캐릭터를 한 명이라도 찾으면 즉시 true 반환
            }

            // 레벨업이 가능한지 확인
            if (CanLevelUp(character))
            {
                return true; // 가능한 캐릭터를 한 명이라도 찾으면 즉시 true 반환
            }
        }

        // 모든 캐릭터를 확인했는데 아무도 해당되지 않으면 false 반환
        return false;
    }
}

