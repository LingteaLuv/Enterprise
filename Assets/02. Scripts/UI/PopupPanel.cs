using System;
using _05._CSJ_Folder.Scripts.Quest;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupPanel : UIBase
{
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Button _leftButton;
    [SerializeField] private TMP_Text _leftButtonText;
    [SerializeField] private Button _rightButton;
    [SerializeField] private TMP_Text _rightButtonText;

    public void Awake()
    {
        TutorialTargets.Register("leftButton", _leftButton.transform as RectTransform);
        TutorialTargets.Register("rightButton", _rightButton.transform as RectTransform);
    }

    public void SetShow(string message,
        string leftText, Action onLeftClick,
        string rightText, Action onRightClick)
    {
        gameObject.SetActive(true);
        _messageText.text = message;

        _leftButton.gameObject.SetActive(true);
        _leftButtonText.text = leftText;
        _leftButton.onClick.RemoveAllListeners();
        _leftButton.onClick.AddListener(() => {
            SetHide();
            onLeftClick?.Invoke();
        });

        _rightButton.gameObject.SetActive(true);
        _rightButtonText.text = rightText;
        _rightButton.onClick.RemoveAllListeners();
        _rightButton.onClick.AddListener(() => {
            onRightClick?.Invoke();
            SetHide();
        });
    }

    public void SetShow(string message,
        string leftText, Action onLeftClick)
    {
        gameObject.SetActive(true);
        _messageText.text = message;

        _leftButton.gameObject.SetActive(true);
        _leftButtonText.text = leftText;
        _leftButton.onClick.RemoveAllListeners();
        _leftButton.onClick.AddListener(() => {
            SetHide();
            onLeftClick?.Invoke();
        });

        _rightButton.gameObject.SetActive(false);
        _rightButton.onClick.RemoveAllListeners();
    }

    public override void SetHide()
    {
        base.SetHide();
        _messageText.text = "";
        _leftButton.onClick.RemoveAllListeners();
        _rightButton.onClick.RemoveAllListeners();
    }
}
