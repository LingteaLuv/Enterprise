using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

public class DatabaseManager : Singleton<DatabaseManager>
{
    public async Task<bool> SaveNicknameAsync()
    {
        FirebaseUser currentUser = FirebaseManager.Auth.CurrentUser;
        string uid = currentUser.UserId;
        string userNickname = FirebaseManager.Auth.CurrentUser.DisplayName;

        Dictionary<string, object> dictionary = new Dictionary<string, object>();

        // 익명계정 RankData 저장 x
        if (currentUser.IsAnonymous)
        {
            //dictionary[$"RankData/{uid}"] = new Dictionary<string, object>();
            dictionary[$"UserData/{uid}/Nickname"] = userNickname;
        }
        else
        {
            dictionary[$"UserData/{uid}/Nickname"] = userNickname;
            dictionary[$"RankData/{uid}/Nickname"] = userNickname;
        }
       
        var task = FirebaseManager.DataReference.UpdateChildrenAsync(dictionary);
        await task; 

        if (task.IsCompletedSuccessfully)
        {
            Debug.Log("UserData / RankData 에 닉네임 저장 성공");
            return true;
        }
        else
        {
            Debug.LogError("닉네임 저장 실패");
            return false;
        }
    }
}
