using GoogleMobileAds.Api;
using UnityEngine;

public class GoogleAdmobTester : MonoBehaviour
{
    private string _adID = "ca-app-pub-3940256099942544/1033173712";
    private InterstitialAd _loadedAd;
    private void Awake()
    {
        MobileAds.Initialize(OnInitialized);
    }

    private void OnInitialized(InitializationStatus status)
    {
        if (status == null)
        {
            Debug.Log("모바일 광고 초기화 실패");
            return;
        }
        Debug.Log("광고 초기화 성공");
        LoadAd();
    }

    public void LoadAd()
    {
        AdRequest adRequest = new AdRequest();
        InterstitialAd.Load(_adID, adRequest, (ad, error) =>
        {
            if (error != null)
            {
                Debug.Log($"광고 에러 이유 : {error.ToString()}");
                _loadedAd = null;
                return;
            }
            Debug.Log("로딩 성공");
            _loadedAd = ad;
            _loadedAd.OnAdFullScreenContentClosed -= LoadAd;
            _loadedAd.OnAdFullScreenContentClosed += LoadAd;
        });
    }

    public void ShowAd()
    {
        if (_loadedAd != null && _loadedAd.CanShowAd())
        {
            _loadedAd.Show();
        }
    }
}
