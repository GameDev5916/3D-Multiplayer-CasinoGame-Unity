// Copyright (C) 2018 Google, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using UnityEngine;

using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

namespace GoogleMobileAds.Android
{
    public class RewardedAdClient : AndroidJavaProxy, IRewardedAdClient
    {
        private AndroidJavaObject androidRewardedAd;

        public RewardedAdClient() : base(Utils.UnityRewardedAdCallbackClassName)
        {
            AndroidJavaClass playerClass = new AndroidJavaClass(Utils.UnityActivityClassName);
            AndroidJavaObject activity =
                playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            androidRewardedAd = new AndroidJavaObject(Utils.UnityRewardedAdClassName, activity, this);
        }

        #region IRewardedClient implementation

        public event EventHandler<EventArgs> OnAdLoaded;

        public event EventHandler<AdErrorEventArgs> OnAdFailedToLoad;

        public event EventHandler<AdErrorEventArgs> OnAdFailedToShow;

        public event EventHandler<EventArgs> OnAdOpening;

        public event EventHandler<Reward> OnUserEarnedReward;

        public event EventHandler<EventArgs> OnAdClosed;

        public event EventHandler<AdValueEventArgs> OnPaidEvent;

        public void CreateRewardedAd(string adUnitId)
        {
            androidRewardedAd.Call("create", adUnitId);
        }

        public void LoadAd(AdRequest request)
        {
            androidRewardedAd.Call("loadAd", Utils.GetAdRequestJavaObject(request));
        }

        public bool IsLoaded()
        {
            return androidRewardedAd.Call<bool>("isLoaded");
        }

        public void Show()
        {
            androidRewardedAd.Call("show");
        }

        public void SetServerSideVerificationOptions(ServerSideVerificationOptions serverSideVerificationOptions)
        {
            androidRewardedAd.Call("setServerSideVerificationOptions", Utils.GetServerSideVerificationOptionsJavaObject(serverSideVerificationOptions));
        }

        public void DestroyRewardBasedVideoAd()
        {
            androidRewardedAd.Call("destroy");
        }

        // Returns the reward item for the loaded rewarded ad.
        public Reward GetRewardItem()
        {
            AndroidJavaObject rewardItem = this.androidRewardedAd.Call<AndroidJavaObject>("getRewardItem");
            if (rewardItem == null)
            {
                return null;
            }
            string type = rewardItem.Call<string>("getType");
            int amount = rewardItem.Call<int>("getAmount");
            return new Reward()
            {
                Type = type,
                Amount = (double)amount
            };
        }

        // Returns the mediation adapter class name.
        public string MediationAdapterClassName()
        {
            return this.androidRewardedAd.Call<string>("getMediationAdapterClassName");
        }

        // Returns ad request response info
        public IResponseInfoClient GetResponseInfoClient()
        {
            return new ResponseInfoClient(this.androidRewardedAd);
        }

        #endregion

        #region Callbacks from UnityRewardBasedVideoAdListener.

        void onRewardedAdLoaded()
        {
            if (this.OnAdLoaded != null)
            {
                this.OnAdLoaded(this, EventArgs.Empty);
            }
        }

        void onRewardedAdFailedToLoad(string errorReason)
        {
            if (this.OnAdFailedToLoad != null)
            {
                AdErrorEventArgs args = new AdErrorEventArgs()
                {
                    Message = errorReason
                };
                this.OnAdFailedToLoad(this, args);
            }
        }

        void onRewardedAdFailedToShow(string errorReason)
        {
            if (this.OnAdFailedToShow != null)
            {
                AdErrorEventArgs args = new AdErrorEventArgs()
                {
                    Message = errorReason
                };
                this.OnAdFailedToShow(this, args);
            }
        }

        void onRewardedAdOpened()
        {
            if (this.OnAdOpening != null)
            {
                this.OnAdOpening(this, EventArgs.Empty);
            }
        }


        void onRewardedAdClosed()
        {
            if (this.OnAdClosed != null)
            {
                this.OnAdClosed(this, EventArgs.Empty);
            }
        }

        void onUserEarnedReward(string type, float amount)
        {
            if (this.OnUserEarnedReward != null)
            {
                Reward args = new Reward()
                {
                    Type = type,
                    Amount = amount
                };
                this.OnUserEarnedReward(this, args);
            }
        }

        public void onPaidEvent(int precision, long valueInMicros, string currencyCode)
        {
            if (this.OnPaidEvent != null)
            {
                AdValue adValue = new AdValue()
                {
                    Precision = (AdValue.PrecisionType)precision,
                    Value = valueInMicros,
                    CurrencyCode = currencyCode
                };
                AdValueEventArgs args = new AdValueEventArgs()
                {
                    AdValue = adValue
                };

                this.OnPaidEvent(this, args);
            }
        }


        #endregion
    }
}
