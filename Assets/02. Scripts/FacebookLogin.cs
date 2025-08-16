using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Facebook.Unity;
using Firebase.Auth;
using Firebase.Extensions;

public class FacebookLogin : MonoBehaviour
{
    [SerializeField] private Button _facebookLoginButton;

    private void Awake()
    {
        _facebookLoginButton.interactable = false;
        
        if (!FB.IsInitialized)
        {
            FB.Init(() =>
            {
                FB.ActivateApp();
                Debug.Log("FB Init");
                _facebookLoginButton.interactable = true;
            });
        }
        else 
        {
            FB.ActivateApp();
            _facebookLoginButton.interactable = true;
        }
    }
    
    private void Start()
    {
        _facebookLoginButton.onClick.AddListener(OnFacebookSignInMyID);
    }
    
    
    private void OnFacebookSignInMyID()
    {
        FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email" }, result1 =>
        {
            if (FB.IsLoggedIn)
            {
                var accessToken = AccessToken.CurrentAccessToken;
                Debug.Log("Access Token: " + accessToken.TokenString);
                
                Credential credential = FacebookAuthProvider.GetCredential(accessToken.TokenString);
                
                FirebaseManager.Auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWithOnMainThread(async task =>
                {
                    if (task.IsCanceled)
                    {
                        Debug.LogError("Firebase sign-in canceled.");
                        return;
                    }
                    if (task.IsFaulted)
                    {
                        Debug.LogError("Firebase sign-in error: " + task.Exception);
                        return;
                    }

                    Firebase.Auth.AuthResult result = task.Result;
                    Debug.LogFormat("Firebase user signed in: {0} ({1})",
                        result.User.DisplayName, result.User.UserId);
                    
                    FirebaseUser user = FirebaseManager.Auth.CurrentUser;
                    await user.ReloadAsync();
                });
            }
            else
            {
                Debug.Log("Not logged in");
            }
        });
    }
    
    private void OnFacebookSignInClicked()
    {
        if (AccessToken.CurrentAccessToken == null)
        {
            Debug.LogWarning("Facebook AccessToken is null. Make sure FB.Init() is completed.");
            return;
        }
        
        string accessToken = AccessToken.CurrentAccessToken.TokenString;
        Firebase.Auth.Credential credential = Firebase.Auth.FacebookAuthProvider.GetCredential(accessToken); 
        FirebaseManager.Auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task => 
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled."); 
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception); 
                return;
            } 
            Firebase.Auth.AuthResult result = task.Result; 
            Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId); 
        });
    }

}
