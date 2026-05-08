#if APPFLYER
using AppsFlyerSDK;
#endif
#if ADMOB
using GoogleMobileAds.Api;
#endif
using System.Collections.Generic;

public class AppflyerEventTracking : SingletonGlobal<AppflyerEventTracking>
{
#if APPFLYER
    private void Start()
    {
        AppsFlyer.startSDK();
    }

    public void LogAdRevenue(AdValue adValue)
    {
        Dictionary<string, string> additionalParams = new Dictionary<string, string>();
        double value = adValue.Value / (double)1000000;
        var afData = new AFAdRevenueData("Admob", MediationNetwork.GoogleAdMob, adValue.CurrencyCode, value);
        AppsFlyer.logAdRevenue(afData, additionalParams);
    }
#endif
}