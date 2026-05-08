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
    [Header("Collapse Ad Settings")]
    public AdmobNativeCollap collapseAdmobSupport;
    public AdmobNativeCollap collapseAdmobPopupSupport;
    public float CollapseAdElapsedTime = 0.0f;
    public float CollapseAdInterval = 30.0f;

    public void CaculaterCounterCollapseAd()
    {
        if (IsBlockedAdByEvent) return;
        if (this.collapseAdmobSupport.CollapseAdShowState == AdShowState.Pending) return;
        if (this.collapseAdmobPopupSupport.CollapseAdShowState == AdShowState.Pending) return;
        CollapseAdElapsedTime += Time.deltaTime;
        if (CollapseAdElapsedTime >= CollapseAdInterval)
        {
            CollapseAdElapsedTime = 0;
            if (this.collapseAdmobSupport.CollapseAdState == AdState.Ready)
                this.collapseAdmobSupport.ShowCollapse();
        }
    }

    public void LoadCollapseAd()
    {
        this.collapseAdmobSupport.LoadCollapseAd();
        this.collapseAdmobPopupSupport.LoadCollapseAd();
    }

    public void ShowCollapseAd()
    {
        if (this.collapseAdmobSupport.CollapseAdShowState == AdShowState.Pending)
            this.collapseAdmobSupport.HideCollapse();
        this.collapseAdmobPopupSupport.ShowCollapse();
    }

    public void HideCollapseAd()
    {
        Debug.Log("[AdManager] Hide Collapse Ad.");
        this.collapseAdmobPopupSupport.HideCollapse();
    }

    public bool IsReadyCollapseAd()
    {
        Debug.Log($"[AdManager] Collapse Ad State: {this.collapseAdmobPopupSupport.CollapseAdState}");
        return this.collapseAdmobPopupSupport.CollapseAdState == AdState.Ready;
    }
#endif
}
