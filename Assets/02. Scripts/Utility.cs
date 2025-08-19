using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

// 전역적으로 사용되는 메서드를 모아두는 Utility class
public static class Utility
{

    
    
    
    
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
