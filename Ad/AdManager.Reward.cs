#if ADMOB
using GoogleMobileAds.Api;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public partial class AdManager
{
#if ADMOB
    [Header("Rewarded Ad Settings")]
    public AdState RewardAdState = AdState.NotAvailable;
    public int _rewardReloadCount = 0;
    private RewardedAd _rewardedAd;

    /// <summary>
    /// Loads the rewarded ad.
    /// </summary>
    public void LoadRewardedAd()
    {
        if (RewardAdState == AdState.Loading)
            return;
        RewardAdState = AdState.Loading;

        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        // if (AdConfig.IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        RewardedAd.Load(_adUnitRewardId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            // if error is not null, the load request failed.
            if (error != null || ad == null)
            {
                RewardAdState = AdState.NotAvailable;
                _rewardReloadCount += 1;
                // if (AdConfig.IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] Rewarded ad failed to load an ad " + "with error : " + error);
                return;
            }

            // if (AdConfig.IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] Rewarded ad loaded with response : " + ad.GetResponseInfo());

            _rewardedAd = ad;
            ListenToRewardAdEvents();
            RewardAdState = AdState.Ready;
            _rewardReloadCount = 0;
        });
    }

    public string reward_placement_id = "";

    public void ShowRewardedAd(UnityAction CALLBACK_EVENT, string placement_id)
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            this.reward_placement_id = placement_id;
            ResetOpenAdInterval();
            onShowAd();
            _rewardedAd.Show((Reward reward) =>
            {
                onCloseAd();
                Concurrency.Instance().Enqueue(() => CALLBACK_EVENT?.Invoke());
            });
        }
        // else NotificationManager.Instance.ShowNotification("No ads available. Check your network connection.", NotificationManager.AppearStyle.SlideIn_FromTop);
    }

    private void ListenToRewardAdEvents()
    {
        // Raised when the ad is estimated to have earned money.
        _rewardedAd.OnAdPaid += (AdValue adValue) =>
        {
            //AppflyerEventSender.Instance.logAdRevenue(adValue);
            // if (AdConfig.IsDebugerMode) Debug.Log(String.Format("Rewarded ad paid {0} {1}.", adValue.Value, adValue.CurrencyCode));

            FirebaseManager.I.TrackingAdsEvent("paid_ads_reward", $"reward_{this.reward_placement_id}", (adValue.Value / (double)1000000).ToString());
            AppflyerEventTracking.I.LogAdRevenue(adValue);
        };
        // Raised when an impression is recorded for an ad.
        _rewardedAd.OnAdImpressionRecorded += () =>
        {
            // if (AdConfig.IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        _rewardedAd.OnAdClicked += () =>
        {
            // if (AdConfig.IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        _rewardedAd.OnAdFullScreenContentOpened += () =>
        {
            // if (AdConfig.IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        _rewardedAd.OnAdFullScreenContentClosed += () =>
        {
            // if (AdConfig.IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] Rewarded ad full screen content closed.");
            if (RewardAdState != AdState.Loading) RewardAdState = AdState.NotAvailable;
            onCloseAd();

        };
        // Raised when the ad failed to open full screen content.
        _rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            // if (AdConfig.IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] Rewarded ad failed to open full screen content " + "with error : " + error);
            if (RewardAdState != AdState.Loading) RewardAdState = AdState.NotAvailable;
            onCloseAd();
        };
    }
#endif
}
