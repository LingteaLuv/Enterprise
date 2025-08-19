using System;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
    [SerializeField] private Button _loginBtn;

    public Action OnTouchLoginBtn;

    private void Start()
    {
        _loginBtn.onClick.AddListener(() => OnTouchLoginBtn?.Invoke());
    }
}
