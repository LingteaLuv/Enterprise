using Cysharp.Threading.Tasks;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    public string Nickname { get; private set; }
    public LoginTypeEnum LoginType { get; private set; }

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
                Debug.LogError("대체 무슨 방식으로 로그인을 한 걸까 이 녀석");
                break;
        } 
    }
}
