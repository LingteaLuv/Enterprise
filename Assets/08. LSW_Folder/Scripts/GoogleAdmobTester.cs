using System;
using GoogleMobileAds.Api;
using UnityEngine;

public class GoogleAdmobTester : Singleton<GoogleAdmobTester>
{
    private string _adID = "ca-app-pub-3940256099942544/5354046379";
    private InterstitialAd _loadedAd;
    private RewardedInterstitialAd _rewardedInterstitialAd;
    
    protected override void Awake()
    {
        base.Awake();
        MobileAds.Initialize(OnInitialized);
    }

    private void OnInitialized(InitializationStatus status)
    {
        if (status == null)
        {
            Debug.LogError("모바일 광고 초기화 실패");
            return;
        }
        LoadAdTest();
    }

    /*public void LoadAd()
    {
        AdRequest adRequest = new AdRequest();
        InterstitialAd.Load(_adID, adRequest, (ad, error) =>
        {
            if (error != null)
            {
                Debug.LogError($"광고 에러 이유 : {error.ToString()}");
                _loadedAd = null;
                return;
            }
            _loadedAd = ad;
            _loadedAd.OnAdFullScreenContentClosed -= LoadAd;
            _loadedAd.OnAdFullScreenContentClosed += LoadAd;
            QuestSignalManager.Instance.ETCAchieve("AdWatch");
        });
    }*/

    public void LoadAdTest()
    {
        AdRequest adRequest = new AdRequest();
        RewardedInterstitialAd.Load(_adID, adRequest, (ad, error) =>
        {
            if (error != null)
            {
                Debug.LogError($"광고 에러 이유 : {error.ToString()}");
                _rewardedInterstitialAd = null;
                return;
            }
            _rewardedInterstitialAd = ad;
            _rewardedInterstitialAd.OnAdFullScreenContentClosed -= LoadAdTest;
            _rewardedInterstitialAd.OnAdFullScreenContentClosed += LoadAdTest;
            QuestSignalManager.Instance.ETCAchieve("AdWatch");
            Debug.LogError("[GoogleAdmobTester] : 광고 로드 완료");
        });
    }
    
    /*public void ShowAd()
    {
        if (_loadedAd != null && _loadedAd.CanShowAd())
        {
            _loadedAd.Show();
        }
    }*/
    
    public void ShowRewardedAd(Action<Reward> callback)
    {
        if (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd())
        {
            // 이 부분에서 끝까지 시청 완료되면 콜백으로 HandleUserEarnedReward을 호출하는듯!
            _rewardedInterstitialAd.Show(callback); 
            Debug.LogError("리워드 광고 표시됨.");
        }
        else
        {
            Debug.LogError("리워드 광고가 로드되지 않았습니다. 먼저 광고를 로드하세요.");
        }
    }
}
