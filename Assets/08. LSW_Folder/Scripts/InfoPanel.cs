using System;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] private Button _infoBtn;

    public Action OnTouchInfoBtn;

    private void Start()
    {
        _infoBtn.onClick.AddListener(() => OnTouchInfoBtn?.Invoke());
    }
}
