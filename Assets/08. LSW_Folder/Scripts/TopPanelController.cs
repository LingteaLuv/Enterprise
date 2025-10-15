using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Numerics;

public class TopPanelController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nickname;
    [SerializeField] private Image _profileImage;
    [SerializeField] private PlayerInfoPanel _infoPanel;
    [SerializeField] private TextMeshProUGUI _teamBounty; 

    private void Awake()
    {
        _infoPanel.OnImageChanged += OnProfileImageUpdated;
    }
    
    private void OnEnable()
    {
        // 이벤트 구독
        StatEvents.OnTeamBattlePowerChanged += OnTeamBountyChanged;
        StatEvents.OnCharacterBattlePowerChanged += OnCharacterBountyChanged;
    }
    
    private void OnDisable()
    {
        // 이벤트 구독 해제
        StatEvents.OnTeamBattlePowerChanged -= OnTeamBountyChanged;
        StatEvents.OnCharacterBattlePowerChanged -= OnCharacterBountyChanged;
    }
    
    private void Start()
    {
        if (PlayerDataManager.Instance != null)
        {
            UpdateBountyDisplay(PlayerDataManager.Instance.teamBattlePower);
        }
        
        LoginManager.Instance.OnNicknameChanged += OnNicknameUpdated;
        OnNicknameUpdated();
        OnProfileImageUpdated(_infoPanel.GetCurImage());
    }
    
    private void OnNicknameUpdated()
    {
        _nickname.text = LoginManager.Instance.Nickname;
    }

    private void OnProfileImageUpdated(Sprite sprite)
    {
        _profileImage.sprite = sprite;
    }
    
    /// <summary>
    /// 팀 전체 현상금이 변경되었을 때 호출
    /// </summary>
    private void OnTeamBountyChanged(BigInteger oldPower, BigInteger newPower)
    {
        UpdateBountyDisplay(newPower);
    }

    /// <summary>
    /// 개별 캐릭터의 현상금이 변경되었을 때 호출
    /// </summary>
    private void OnCharacterBountyChanged(PlayerCharacterData character, BigInteger oldPower, BigInteger newPower)
    {
        // 개별 캐릭터 변경 시 특별한 UI 효과를 추가하고 싶다면 여기에 구현
        // 예: 현상금 상승 축하 애니메이션 등
        Debug.Log($"[현상금 변경] {character.characterdata.characterName}: {oldPower} → {newPower}");
    }

    /// <summary>
    /// 현상금 UI를 업데이트
    /// </summary>
    private void UpdateBountyDisplay(BigInteger bounty)
    {
        if (_teamBounty != null)
        {
            _teamBounty.text = $"{bounty}";
        }
    }
}
