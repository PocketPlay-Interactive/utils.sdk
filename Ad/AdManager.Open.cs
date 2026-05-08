#if ADMOB
using GoogleMobileAds.Api;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AdManager;

public partial class AdManager
{
#if ADMOB
    [Header("Open Ad Settings")]
    public AdState OpenAdState = AdState.NotAvailable;
    public int _openReloadCount = 0;
    private AppOpenAd appOpenAd;
    private float OpenAdElapsedTime = 0.0f;

    public void CaculaterCounterOpenAd()
    {
        if (RuntimeStorageData.CanShowAds() == false) return;
        OpenAdElapsedTime += Time.deltaTime;
    }

    /// <summary>
    /// Loads the app open ad.
    /// </summary>
    public void LoadAppOpenAd()
    {
        if (RuntimeStorageData.CanShowAds() == false)
            return;
        if (OpenAdState == AdState.Loading)
            return;
        OpenAdState = AdState.Loading;

        // Clean up the old ad before loading a new one.
        if (appOpenAd != null)
        {
            appOpenAd.Destroy();
            appOpenAd = null;
        }

        // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] Loading the app open ad.");

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        AppOpenAd.Load(_adUnitOpenId, adRequest,
            (AppOpenAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    OpenAdState = AdState.NotAvailable;
                    _openReloadCount += 1;
                    // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] app open ad failed to load an ad " + "with error : " + error);
                    return;
                }

                // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] App open ad loaded with response : " + ad.GetResponseInfo());

                appOpenAd = ad;
                ListenToOpenAdEvents();
                OpenAdState = appOpenAd.CanShowAd() == true ? AdState.Ready : AdState.NotAvailable;
                _openReloadCount = 0;
            });
    }

    private void ListenToOpenAdEvents()
    {
        // Raised when the ad is estimated to have earned money.
        appOpenAd.OnAdPaid += (AdValue adValue) =>
        {
            //AppflyerEventSender.Instance.logAdRevenue(adValue);
            // if (IsDebugerMode) Debug.Log(String.Format("App open ad paid {0} {1}.", adValue.Value, adValue.CurrencyCode));

            FirebaseManager.I.TrackingAdsEvent("paid_ads_open", "app_open", (adValue.Value / (double)1000000).ToString());
            AppflyerEventTracking.I.LogAdRevenue(adValue);
        };
        // Raised when an impression is recorded for an ad.
        appOpenAd.OnAdImpressionRecorded += () =>
        {
            // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] App open ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        appOpenAd.OnAdClicked += () =>
        {
            // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] App open ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        appOpenAd.OnAdFullScreenContentOpened += () =>
        {
            // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] App open ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        appOpenAd.OnAdFullScreenContentClosed += () =>
        {
            // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] App open ad full screen content closed.");
            OpenAdState = AdState.NotAvailable;
            onCloseAd();
        };
        // Raised when the ad failed to open full screen content.
        appOpenAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] App open ad failed to open full screen content " + "with error : " + error);
            OpenAdState = AdState.NotAvailable;
            onCloseAd();
        };
    }

    //[Button]
    public void CheckingOpenAd()
    {
        if (RuntimeStorageData.CanShowAds() == false) return;
        if (OpenAdElapsedTime < OpenAdInterval)
        {
            // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] Open Ad interval time not reached yet.");
            return;
        }
        Concurrency.Instance().Enqueue(() =>
        {
            // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] Checking Open Ad");
            ResetOpenAdInterval();
            ShowAppOpenAd();
        });
    }

    public bool IsAdAvailable
    {
        get
        {
            return appOpenAd != null && appOpenAd.CanShowAd() == true;
        }
    }

    /// <summary>
    /// Shows the app open ad.
    /// </summary>
    private void ShowAppOpenAd()
    {
        if (IsAdAvailable)
        {
            // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] Showing app open ad.");
            onShowAd();
            appOpenAd.Show();
        }
        else
        {
            // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] App open ad is not ready yet.");
        }
    }

    public void ResetOpenAdInterval() { OpenAdElapsedTime = 0.0f; }
#endif
}
