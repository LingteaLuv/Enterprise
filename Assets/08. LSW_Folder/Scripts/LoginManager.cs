using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LoginManager : Singleton<LoginManager>
{
    public string Nickname { get; private set; }
    public LoginTypeEnum LoginType { get; private set; }

    public Action OnNicknameChanged;
    
    public void Start()
    {
        AuthManager.Instance.LoginCompleted += SetNickname;
    }
    
    public enum LoginTypeEnum
    {
        Anonymous,
        Google,
        GooglePlay
    }

    public void SetNickname()
    {
        DatabaseManager.Instance.LoadNickname((nickname) =>
        {
            Nickname = nickname;
            OnNicknameChanged?.Invoke();
        });
    }
    
    public void SetLoginType(string type)
    {
        switch (type)
        {
            case "anonymous" :
                LoginType = LoginTypeEnum.Anonymous;
                break;
            case "google.com" :
                LoginType = LoginTypeEnum.Google;
                break;
            case "playergames.google.com" :
                LoginType = LoginTypeEnum.GooglePlay;
                break;
            default :
                break;
        } 
    }
}
