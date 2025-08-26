using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

// 전역적으로 사용되는 메서드를 모아두는 Utility class
public static class Utility
{
    public static string DictToJson1(Dictionary<string, object> dict)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("{");

        bool first = true;
        foreach (var keyValuePair in dict)
        {
            if (!first) builder.Append(",");
            first = false;

            builder.Append($"\"{keyValuePair.Key}\":");

            if (keyValuePair.Value is string)
                builder.Append($"\"{keyValuePair.Value}\"");
            else if (keyValuePair.Value is Dictionary<string, object>)
                builder.Append(DictToJson1(keyValuePair.Value as Dictionary<string, object>));
            else
                builder.Append(keyValuePair.Value);
        }

        builder.Append("}");
        return builder.ToString();
    }
    
    public static Dictionary<string, object> JsonToDict(string json)
    {
        object obj = Google.MiniJSON.Json.Deserialize(json);
        Dictionary<string, object> dictionary = obj as Dictionary<string, object>;
        if(dictionary != null ) return dictionary;
        return null;
    }

    public static string DictToJson(Dictionary<string, object> dictionary)
    {
        return Google.MiniJSON.Json.Serialize(dictionary);
    }
    
    #region Legacy

    public static async Task SetGuestNickname(FirebaseUser currentUser)
    {
        UserProfile profile = new UserProfile();
        profile.DisplayName = $"게스트{Random.Range(1000, 10000)}";

        await currentUser.UpdateUserProfileAsync(profile);
        await currentUser.ReloadAsync();
        
        Debug.Log("닉네임 설정 성공");
        Debug.Log($"변경된 유저 닉네임 : {currentUser.DisplayName}");
    }
    
    public static async Task SetGoogleNickname(FirebaseUser currentUser ,string googleDisplayName)
    {
        UserProfile profile = new UserProfile();
        profile.DisplayName = googleDisplayName;
        
        Debug.Log($"SetGoogleNickname : googleDisplayName = {googleDisplayName}" );

        await currentUser.UpdateUserProfileAsync(profile);
        await currentUser.ReloadAsync();

        Debug.Log("닉네임 설정 성공");
        Debug.Log($"변경된 유저 닉네임 : {currentUser.DisplayName}");
    }

    #endregion
}
