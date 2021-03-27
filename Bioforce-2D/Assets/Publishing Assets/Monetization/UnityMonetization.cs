using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

enum AdsPlatform
{
    notSupported,
    iOS,
    Android
}
public class UnityMonetization : MonoBehaviour, IUnityAdsListener
{
    const string ProjectIDiOS = "4059326";
    const string ProjectIDAndroid = "4059326";
    private AdsPlatform AdsPlatform { get; set; }

    [SerializeField] private bool Testing = true;

    //Add the platform when initialisingAdsPlatform _iOS or _Android
    string InterstitialPlacementID { get; set; } = "Interstitial";
    string RewardPlacementID { get; set; } = "Rewarded"; 
    string BannerPlacementID { get; set; } = "Banner";

    public Action AdRewardIsReadyEvent { get; set; }
    public Action AdInterstitialIsReadyEvent { get; set; }
    public Action AdStartedEvent { get; set; }
    public Action AdRewardEvent { get; set; }
    
    private void Awake()
    {
        PrivacyPolicyTermsConditionsMenu.Accepted += OnAcceptedPolicy;
    }

    private void OnAcceptedPolicy()
    {
        InitiliseAdsPlatform();
        //StartCoroutine(ShowBannerAdWhenInitialised());

        Advertisement.AddListener(this);

        AdRewardIsReadyEvent += () => Debug.Log("Reward ad is ready");
        AdInterstitialIsReadyEvent += () => Debug.Log("Interstitial ad is ready");
    }

    private IEnumerator ShowBannerAdWhenInitialised()
    {
        while (!Advertisement.isInitialized)
            yield return new WaitForSeconds(1f);

        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        Advertisement.Banner.Show(BannerPlacementID);
    }

    public void ShowInterstialAd()
    {
        ShowAd(InterstitialPlacementID);       
    }
    public void ShowRewardedAd()
    {
        ShowAd(RewardPlacementID);
    }

    private void ShowAd(string placementID)
    {
        TurnOffBannerAd();

        if (!Advertisement.IsReady(placementID))
        {
            Debug.Log($"Ad: {placementID} is not ready...");
            return;
        }

        Advertisement.Show(placementID);
    }

    private void TurnOffBannerAd()
    {
        if (!Advertisement.Banner.isLoaded)
            return;

        //Advertisement.Banner.Hide();
        Advertisement.Banner.Hide(destroy: true);
    }

    private void InitiliseAdsPlatform()
    {
        RuntimePlatform platform = Application.platform;
        switch (platform)
        {
            case RuntimePlatform.IPhonePlayer:
                AdsPlatform = AdsPlatform.iOS;
                Advertisement.Initialize(ProjectIDiOS, Testing);
                AddPlatformToPlacementIDs("_iOS");
                break;
            case RuntimePlatform.Android:
                AdsPlatform = AdsPlatform.Android;
                Advertisement.Initialize(ProjectIDAndroid, Testing);
                AddPlatformToPlacementIDs("_Android");
                break;
            default:
                AdsPlatform = AdsPlatform.notSupported;
                if (Testing)
                {
                    Advertisement.Initialize(ProjectIDiOS, Testing);
                    AddPlatformToPlacementIDs("_iOS");
                }
                break;
        }
    }

    private void AddPlatformToPlacementIDs(string platform)
    {
        InterstitialPlacementID += platform;
        RewardPlacementID += platform;
        BannerPlacementID += platform;
    }


    #region Unity Ads Interfaces

    public void OnUnityAdsReady(string placementId)
    {
        if (placementId == RewardPlacementID)
            // Optional actions to take when theAd Unit or legacy Placement becomes ready (for example, enable the rewarded ads button)
            AdRewardIsReadyEvent?.Invoke();
        else if (placementId == InterstitialPlacementID)
            AdInterstitialIsReadyEvent?.Invoke();
    }
    public void OnUnityAdsDidError(string message)
    {
        Debug.LogError($"Error in Unity Ad...\n{message}");
    }
    public void OnUnityAdsDidStart(string placementId)
    {
        AdStartedEvent?.Invoke();
    }
    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        switch (showResult)
        {
            case ShowResult.Failed:
                Debug.LogWarning($"Error in showing ad: {placementId}");
                break;
            case ShowResult.Skipped:
                //No reward as the ad was skipped
                break;
            case ShowResult.Finished:
                //Reward player
                if (placementId == RewardPlacementID)
                    AdRewardEvent?.Invoke();
                break;
        }
    }
    
    #endregion Unity Ads Interfaces
}
