using System;
using UnityEngine;
using UnityEngine.UI;

public class CommandPanel : MonoBehaviour
{
    [SerializeField] private Button _attackBtn;
    [SerializeField] private Button _skillBtn;
    
    public void CreateAnimationPanel(PlayerObj Unit)
    {
        _attackBtn.onClick.AddListener(() =>
        {
            PlayerState state = PlayerState.ATTACK;
            Unit.isAction = true;
            Unit._prefabs._anim.Rebind();
            Unit.SetStateAnimationIndex(state);
            Unit.PlayStateAnimation(state);
        });
        _attackBtn.onClick.AddListener(() =>
        {
            PlayerState state = PlayerState.ATTACK;
            Unit.isAction = true;
            Unit._prefabs._anim.Rebind();
            Unit.SetStateAnimationIndex(state, 1);
            Unit.PlayStateAnimation(state);
        });
    }
}
