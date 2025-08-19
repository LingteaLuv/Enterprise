using System;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanel : MonoBehaviour
{
    [SerializeField] private Button _startBtn;

    public Action OnTouchStartBtn;

    private void Start()
    {
        _startBtn.onClick.AddListener(() => OnTouchStartBtn?.Invoke());
    }
}
