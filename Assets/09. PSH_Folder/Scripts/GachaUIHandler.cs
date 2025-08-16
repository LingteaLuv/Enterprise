
using UnityEngine;

public class GachaUIHandler : MonoBehaviour
{
    // 인스펙터에서 GachaManager를 연결해줘야 합니다.
    public GachaManager gachaManager;

    /// <summary>
    /// 이 함수를 UI Button의 OnClick 이벤트에 연결할 것입니다.
    /// </summary>
    public void OnGachaButtonPressed()
    {
        if (gachaManager == null)
        {
            Debug.LogError("GachaManager가 GachaUIHandler에 연결되지 않았습니다!");
            return;
        }

        // GachaManager의 DrawCharacter 함수를 호출하여 캐릭터를 뽑습니다.
        CharacterData drawnCharacter = gachaManager.DrawCharacter();

        // 뽑기에 성공했다면 결과를 처리합니다.
        if (drawnCharacter != null)
        {
            // TODO: 여기에 뽑은 캐릭터를 플레이어 데이터에 추가하거나,
            // 화려한 연출과 함께 결과 UI를 보여주는 로직을 추가하면 됩니다.

            Debug.Log($"UI 버튼 클릭으로 뽑기 성공! 획득: [{drawnCharacter.rarity}] {drawnCharacter.characterName}");

            // 예시: 결과 창을 띄우는 함수 호출
            // ShowGachaResultPopup(drawnCharacter);
        }
        else
        {
            // 뽑기에 실패한 경우 (예: 해당 등급의 캐릭터가 없는 경우 등)
            Debug.LogWarning("캐릭터 뽑기에 실패했습니다.");
        }
    }

    // private void ShowGachaResultPopup(CharacterSO character)
    // {
    //     // 결과 창 UI를 활성화하고, 캐릭터 정보를 표시하는 코드...
    // }
}
