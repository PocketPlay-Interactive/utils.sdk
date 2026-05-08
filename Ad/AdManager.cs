using UnityEngine;
using DG.Tweening;
using System.Collections;
using System;
using UnityEngine.Events;



#if ADMOB
using GoogleMobileAds.Api;
#endif

[DefaultExecutionOrder(-5)]
public partial class AdManager : SingletonGlobal<AdManager>
{
#if ADMOB
    [Header("AdMob Settings")]
#if ADMOB_TEST
    // AdMob Unity test IDs (from official docs)
    public string _adUnitId = "ca-app-pub-3940256099942544~3347511713";
    public string _adUnitBannerId = "ca-app-pub-3940256099942544/6300978111";
    public string _adUnitInterId = "ca-app-pub-3940256099942544/1033173712";
    public string _adUnitInterOpenId = "ca-app-pub-3940256099942544/3419835294";
    public string _adUnitOpenId = "ca-app-pub-3940256099942544/9257395921";
    public string _adUnitRewardId = "ca-app-pub-3940256099942544/5224354917";
#else
    public string _adUnitId = "ca-app-pub-8190506959251235~4605609708";
    public string _adUnitBannerId = "ca-app-pub-8190506959251235/6759524977";
    public string _adUnitNativeBannerId = "ca-app-pub-8190506959251235/7413376494";
    public string _adUnitInterId = "ca-app-pub-8190506959251235/9578734783";
    public string _adUnitNativeInterId = "ca-app-pub-8190506959251235/9861228823";
    public string _adUnitInterOpenId = "ca-app-pub-8190506959251235/2755689020";
    public string _adUnitOpenId = "ca-app-pub-8190506959251235/3340629497";
    public string _adUnitRewardId = "ca-app-pub-8190506959251235/8535641078";
    public string _adUnitCollapseId = "ca-app-pub-8190506959251235/3948752870";
#endif

    public bool IsDebugerMode = false;
    private bool IsBlockedAdByEvent = false;
    public void SetBlockedAdByEvent(bool isBlocked)
    {
        Debug.Log($"[AdManager] SetBlockedAdByEvent: {isBlocked}");
        IsBlockedAdByEvent = isBlocked;
        if (isBlocked)
        {
            HideBanner();
            HideCollapseAd();
        }
        else ShowBanner();
    }

    public bool IsPreloadBanner = true;
    public float _adUnitBannerAutoRefreshTime = 30.0f;

    public bool IsPreloadInterstitial = true;
    public float InterstitialAdInterval = 45.0f;

    public bool IsPreloadOpen = true;
    public static float OpenAdInterval = 15.0f;

    public bool IsPreloadReward = true;
    public enum AdState
    {
        Loading, Ready, NotAvailable
    }

    public enum AdBannerSize
    {
        Banner,
        FullWidth
    }

    public enum AdShowState
    {
        None,
        Pending
    }


    public void OnShowAd() => onShowAd();
    public void OnCloseAd() => onCloseAd();
    private void onShowAd()
    {
        Application.targetFrameRate = 5;
        Concurrency.Instance().Enqueue(() =>
        {
            Time.timeScale = 0.0f;
            DOTween.PauseAll();
        });
    }

    private void onCloseAd()
    {
        Application.targetFrameRate = 60;
        Concurrency.Instance().Enqueue(() =>
        {
            Time.timeScale = 1;
            DOTween.PlayAll();
        });
    }

    public IEnumerator InitializeAsync()
    {
        yield return null;
        MobileAds.SetiOSAppPauseOnBackground(true);
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
    
        yield return new WaitForEndOfFrame();
        MobileAds.Initialize(initStatus =>
        {
            Manager.I.IsAdvertisementReady = true;
            Debug.Log("AdMob initialization completed.");
            FirebaseManager.I.TrackingEvent("ad_mob_initialized");
            if (RuntimeStorageData.CanShowAds() == true)
            {
                FirebaseManager.I.TrackingEvent("ad_mob_show_interstitial_open");
                Concurrency.Instance().Enqueue(() => this.interstitialAdmobOpenSupport.PromiseShowInterstitialOpenAd());
            }
            else Manager.I?.FinalizeAdSequence();
        }); 
        Debug.Log("Waiting for AdMob initialization...");    
    }

    public void InitializedAllOfAds()
    {
        LoadRewardedAd();
        LoadInterstitialAd();
        LoadAppOpenAd();
        LoadBannerAd();
        LoadCollapseAd();

        _hasLoadedAds = true;
    }

    private bool _hasLoadedAds = false;
    private float AfterAdReload = 10.0f;

    // Gộp reload timer thành một biến duy nhất
    private float TimerAdReload = 0f;

    private void Update()
    {
        if (this.interstitialAdmobOpenSupport._hasShownInterstitialOpen == true) this.interstitialAdmobOpenSupport.ShowInterstitialOpenAd();
        if (_hasLoadedAds == false) return;
        TimerAdReload += Time.deltaTime;
        if (TimerAdReload > AfterAdReload)
        {
            TimerAdReload = 0;
            if (IsPreloadInterstitial && InterstitalAdState == AdState.NotAvailable && RuntimeStorageData.CanShowAds() == true)
                LoadInterstitialAd();

            if (IsPreloadReward && RewardAdState == AdState.NotAvailable)
                LoadRewardedAd();

            if (IsPreloadOpen && OpenAdState == AdState.NotAvailable && RuntimeStorageData.CanShowAds() == true)
                LoadAppOpenAd();
        }

#if !USE_ADMOB_CUSTOM_PLUGIN
        if (IsPreloadBanner && BannerAdState == AdState.NotAvailable && RuntimeStorageData.CanShowAds() == true)
            LoadBannerAd();
#endif

        if (RuntimeStorageData.CanShowAds() == false) return;

        CaculaterCounterInterAd();
        CaculaterCounterOpenAd();
        CaculaterCounterBannerAd();
        CaculaterCounterCollapseAd();
    }

    void OnEnable() => GameEvent.OnIAPurchase += HandleIAPurchase;
    void OnDisable() => GameEvent.OnIAPurchase -= HandleIAPurchase;

    private void HandleIAPurchase(string productId)
    {
        Debug.Log("Handling IAP purchase for product ID: " + productId);
        HideBanner();
        HideCollapseAd();

        Debug.Log(RuntimeStorageData.CanShowAds() == false ? "Ads are now blocked due to IAP purchase." : "Ads are still available after IAP purchase.");
    }
#else
    public bool IsNativeAd = false;
    public float OpenAdSpaceTimeCounter = 0.0f;

    private void Start()
    {
        DOVirtual.DelayedCall(5.0f, () => Concurrency.Instance().Enqueue(() => Manager.I.FinalizeAdSequence()));
    }

    public void ShowInterstitialFreeAd(Action action_1, Action action_2)
    {
        Concurrency.Instance().Enqueue(action_1);
        Concurrency.Instance().Enqueue(action_2);
    }

    public void ShowInterstitialOutSpaceTime(Action action)
    {
        Concurrency.Instance().Enqueue(action);
    }

    public void ShowRewardedAd(UnityAction CALLBACK) { CALLBACK?.Invoke(); }

    public void ShowInterstitialAdWithSpaceTime(UnityAction CALLBACK) { CALLBACK?.Invoke(); }

    public void ShowInterstitialAd(UnityAction CALLBACK) { CALLBACK?.Invoke(); }

    public void CheckingOpenAd() { }

    public void ShowBannerAd() { }

    public void ShowRewardedHintAd(UnityAction CALLBACK) { CALLBACK?.Invoke(); }

    public void ShowInterstitialForcedAd(UnityAction CALLBACK) { CALLBACK?.Invoke(); }

    public void ShowRewardedForceAd(UnityAction CALLBACK) { CALLBACK?.Invoke(); }

    public void HideBannerAd() { }

    public void CanShowInterstitialOpen() { }

    public void ShowBanner() { }
    public void HideBanner() { }
    public void HideNativeFullAd() { }
    public void ShowNativeFullAd() { }
    public void ShowRewardedAd(UnityAction CALLBACK_EVENT, string placement_id) { CALLBACK_EVENT?.Invoke(); }
#endif
}
