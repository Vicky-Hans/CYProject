using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using DH.Launch;
using DH.NativeCore;
using DH.NativeCore.Platform;
using DH.Proto;
using DHFramework;
using UnityEngine;

namespace DH.Game
{
    public class PayResult
    {
        public int Status;
        public string ErrorMsg;
        public List<Resource> Rewards;
    }

    // public const string ConfRecharge0 = "ConfRecharge0"; //Purchase succeeds
    // public const string ConfRecharge1 = "ConfRecharge1"; //初始化失败，等待初始化成功后进行支付
    // public const string ConfRecharge2 = "ConfRecharge2"; //有未完成的支付商品，需要等待支付完成后，再进行支付
    // public const string ConfRecharge3 = "ConfRecharge3"; //用户取消支付
    // public const string ConfRecharge4 = "ConfRecharge4"; //其它商品正在支付
    // public const string ConfRecharge5 = "ConfRecharge5"; //预下单失败
    // public const string ConfRecharge6 = "ConfRecharge6"; //无效的商品id
    // public const string ConfRecharge7 = "ConfRecharge7"; //验证订单id失败
    // public const string ConfRecharge8 = "ConfRecharge8"; //无效的订单信息
    // public const string ConfRecharge9 = "ConfRecharge9"; //QQ支付失败
    // public const string ConfRecharge10 = "ConfRecharge10"; //支付初始化失败
    // public const string ConfRecharge11 = "ConfRecharge11"; //预下单失败，错误码：{0}
    // public const string ConfRecharge12 = "ConfRecharge12"; //领取奖励失败，错误码：{0}
    // public const string ConfRecharge13 = "ConfRecharge13"; //补单奖励领取失败，错误码：{0}

    public class PayController : Singleton<PayController>, IPayListener
    {
        private Action<PayResult> payResultCallback;
        private IPayController controller;
        private IPayExtension payExtension;
        private bool isInitialized;
        private string pendingProductId = string.Empty;  // recharge PayId
        private int pendingCfgId;       // package id
        private readonly PayResult payResult = new PayResult();

        public void Init()
        {
            isInitialized = false;
            pendingProductId = string.Empty;
            pendingCfgId = 0;

            ConfigurationBuilder builder = new ConfigurationBuilder
            {
                PayPlatform = GetPayPlatform()
            };

            DHLog.Debug($"[PayController] 平台 ：{builder.PayPlatform.ToString()}");

            ConfigurationBuilder.Metadata metadata = new ConfigurationBuilder.Metadata()
            {
                Title = string.Empty,
                Description = string.Empty,
            };

            var cfgs = ConfigCenter.RechargeCfgColl.DataItems;
            foreach (var cfg in cfgs)
            {
                builder.AddProduct(cfg.PayId, ProductType.Consumable, cfg.PayId, metadata, cfg.PriceCn);
            }

            //注册 支付 回调事件（IPayListener接口）
            UPay.InitializeStore(this, builder);
        }

        /// <summary>
        /// 客户端进行支付流程
        /// </summary>
        /// <param name="productId">项目内部定义的礼包Id，Package表里的Id</param>
        /// <param name="index">自选礼包索引, 不是自选的时候默认值-1，是的时候从0开始</param>
        /// <param name="callback">支付成功的回调函数</param>
        public void Pay(int productId, int index = -1, Action<PayResult> callback = null)
        {
            if (!isInitialized)
            {
                ToastManager.Show(ConfigCenter.GetGlobalString(GlobalLanguageId.ConfRecharge1));
                return;
            }

            if (!string.IsNullOrEmpty(pendingProductId))
            {
                ToastManager.Show(ConfigCenter.GetGlobalString(GlobalLanguageId.ConfRecharge2));
                return;
            }
            var pCfg = ConfigCenter.PackageCfgColl.GetDataById(productId);
            var cfg = ConfigCenter.RechargeCfgColl.GetDataById(pCfg.RechargeId);
            pendingProductId = cfg.PayId;
            pendingCfgId = productId;
            this.payResultCallback = callback;
            // 向GameServer进行下单操作
            SendPrePay(productId, index).Forget();
        }
        
        /// <summary>
        /// 获取本地化的价格信息，只在Apple/Google平台有效
        /// </summary>
        /// <param name="productId">项目的商品id</param>
        /// <returns></returns>
        public string GetLocalizePrice(string productId)
        {
            var cfgList = ConfigCenter.RechargeCfgColl.GetDataByPayId(productId);

            if (cfgList == null || cfgList.Count == 0)
            {
                DHLog.Error($"没有找到produceId为{productId}对应的配置");
                return "";
            }

            var cfg = cfgList[0];
            string priceStr = cfg.PriceStr;
            var targetPriceStr = payExtension == null ? priceStr : payExtension.GetLocalizedPriceString(cfg.PayId);

            if (payExtension != null)
            {
                DHLog.Debug($"GetLocalizePrice{productId} is {targetPriceStr}");
            }

            return targetPriceStr;
        }

        /// <summary>
        /// 获取本地化的价格信息，只在Apple/Google平台有效
        /// </summary>
        /// <param name="productId">项目的商品id</param>
        /// <returns></returns>
        public double GetLocalizePriceDecimal(string productId)
        {
            var cfgList = ConfigCenter.RechargeCfgColl.GetDataByPayId(productId);

            if (cfgList == null || cfgList.Count == 0)
            {
                DHLog.Error($"没有找到produceId为{productId}对应的配置");
                return 0;
            }

            var cfg = cfgList[0];
            var targetPrice = payExtension == null ? cfg.Price : (double)payExtension.GetLocalizedPrice(cfg.PayId);

            if (payExtension != null)
            {
                DHLog.Debug($"GetLocalizePrice{productId} is {targetPrice}");
            }

            return targetPrice;
        }

        /// <summary>
        /// 获取本地化的价格信息类型，只在Apple/Google平台有效
        /// </summary>
        /// <param name="productId">项目的商品id</param>
        /// <returns></returns>
        public string GetPriceCurrencyCode(string productId)
        {
            var cfgList = ConfigCenter.RechargeCfgColl.GetDataByPayId(productId);

            if (cfgList == null || cfgList.Count == 0)
            {
                DHLog.Error($"没有找到produceId为{productId}对应的配置");
                return "";
            }

            var cfg = cfgList[0];
            var currencyCode = payExtension == null ? "" : payExtension.GetPriceCurrencyCode(cfg.PayId);

            if (payExtension != null)
            {
                DHLog.Debug($"GetPriceCurrencyCode{productId} is {currencyCode}");
            }

            return currencyCode;
        }

        /// <summary>
        /// 预下单请求，由客户端发给GameServer
        /// </summary>
        /// <param name="productId"></param>
        private async UniTask SendPrePay(int productId, int index = -1)
        {
            var reqPrePay = new ReqPrePay
            {
                App = CreateAppInfo(),
                PackageId = productId,
                Index = index,
            };
            var result = await GameNetworkManager.Instance.SendAsync<RspPrePay>(reqPrePay);
            HandleRspPrePay(result.rsp);
        }
        /// <summary>
        /// 支付领奖
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="productId"></param>
        private async UniTask SendRewardPay(string orderId, string productId)
        {
            var reqRewardPay = new ReqRewardPay
            {
                App = CreateAppInfo(),
                OrderId = orderId,
                StoreId = productId,
                CurrencyInfo = CreateCurrencyInfo(productId)
            };

            var result = await GameNetworkManager.Instance.SendAsync<RspRewardPay>(reqRewardPay);
            HandleRspRewardPay(result.rsp);
        }

        /// <summary>
        /// 补单奖励
        /// </summary>
        /// <param  name="orderId"></param>
        /// <param name="productId"></param>
        private async UniTask SendLoginRewardPay(string orderId, string productId)
        {
            var reqLoginRewardPay = new ReqLoginRewardPay
            {
                App = CreateAppInfo(),
                OrderId = orderId,
                StoreId = productId,
                CurrencyInfo = CreateCurrencyInfo(productId)
            };

            var result = await GameNetworkManager.Instance.SendAsync<RspLoginRewardPay>(reqLoginRewardPay);
            HandleRspLoginRewardPay(result.rsp);
        }

        private appInfo CreateAppInfo()
        {
            var appInfo = new appInfo
            {
                DeviceId = DeviceUtility.GetShuMeiDroidHangID(),
                Package = DeviceUtility.GetBundleId(),
                Plat = UPay.GetPayPlatformName(),
                PayType = "inapp"
            };

            return appInfo;
        }

        private currencyInfo CreateCurrencyInfo(string productId)
        {
            var currencyInfo = new currencyInfo
            {
                Price = (UInt64)GetLocalizePriceDecimal(productId),
                Type = GetPriceCurrencyCode(productId)
            };

            return currencyInfo;
        }

        private void HandleRspPrePay(RspPrePay data)
        {
            if (data.Status == 0)
            {
                if (string.IsNullOrEmpty(data.OrderId))
                {
                    var tipStr = ConfigCenter.GetGlobalString(GlobalLanguageId.ConfRecharge5);
                    if (!ReturnPayResult((int)PayFailReason.PreOrderFail, tipStr, null))
                    {
                        OpenMessageBox(tipStr);
                    }
                    pendingProductId = string.Empty;
                    pendingCfgId = 0;
                    //GameEvent.PayContainer.TriggerEvent(PayEventType.EnableClick); //TODO
                    return;
                }

                DHLog.Debug($"RspPrePay Success");
                controller.StartPay(data.OrderId, pendingProductId);
            }
            else
            {
                //GameEvent.PayContainer.TriggerEvent(PayEventType.EnableClick); //TODO
                pendingProductId = string.Empty;
                pendingCfgId = 0;

                var tipStr = DHUtility.Format(ConfigCenter.GetGlobalString(GlobalLanguageId.ConfRecharge11), data.Status);
                if (!ReturnPayResult((int)PayFailReason.PreOrderFail, tipStr, null))
                {
                    OpenMessageBox(tipStr);
                }
            }
        }

        private void HandleRspRewardPay(RspRewardPay data)
        {
            if (data.Status == 0)
            {
                if (pendingCfgId > 0)
                {
                    DHLog.Debug($"RspRewardPay Success");
                    if (!ReturnPayResult(0, "", data.Rewards))
                    {
                        ToastManager.Show(ConfigCenter.GetGlobalString(GlobalLanguageId.ConfRecharge0));
                    }
                }
                else
                {
                    DHLog.Error($"RspRewardPay Status = 0, But Parse ProductId Fail, {pendingProductId}, {pendingCfgId}");
                }
            }
            else
            {
                var tipStr = DHUtility.Format(ConfigCenter.GetGlobalString(GlobalLanguageId.ConfRecharge12), data.Status);

                if (!ReturnPayResult(data.Status, tipStr, null))
                {
                    OpenMessageBox(tipStr);
                }
            }

            pendingProductId = string.Empty;
            pendingCfgId = 0;
        }

        private void HandleRspLoginRewardPay(RspLoginRewardPay data)
        {
            if (data.Status != 0)
            {
                var tipStr = DHUtility.Format(ConfigCenter.GetGlobalString(GlobalLanguageId.ConfRecharge13), data.Status);

                OpenMessageBox(tipStr);
            }
            else
            {
                ToastManager.Show("补单奖励领取成功，后端会走邮件接口，发放到邮件");
            }

            pendingProductId = string.Empty;
            pendingCfgId = 0;
        }

        private PayPlatform GetPayPlatform()
        {
            if (Application.isMobilePlatform) //移动平台
            {
                var isReleaseMode = StartupEntry.Instance.StartupConfig.EnableRelease;

                if (Application.platform == RuntimePlatform.Android)
                {
                    if (isReleaseMode)
                    {
#if DOMESTIC
                        //国内渠道
                        return PayPlatform.Domestic;
#else
                        return PayPlatform.GoogleStore;
#endif
                    }

                    return PayPlatform.Fake;
                }

                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    if (GameConst.IsIosAuditState || isReleaseMode)
                    {
                        return PayPlatform.AppleStore;
                    }

                    return PayPlatform.Fake;
                }

                DHLog.Error("不支持的移动平台");
                return PayPlatform.Fake;
            }

            return PayPlatform.Fake;
        }

        private void OpenMessageBox(string msgContent)
        {
            var messageBox = CommonMessageBoxViewModel.CreateConfirmButtonOnly(msgContent, null);
            UIManager.Instance.OpenDialog<CommonMessageBox>(messageBox, true).Forget();
        }

        private bool ReturnPayResult(int status, string msg, ICollection<Resource> rewards)
        {
            if (this.payResultCallback == null)
            {
                return false;
            }

            payResult.Status = status;
            payResult.ErrorMsg = msg;

            if (payResult.Rewards == null)
            {
                payResult.Rewards = new List<Resource>();
            }
            else
            {
                payResult.Rewards.Clear();
            }

            if (rewards != null)
            {
                payResult.Rewards.AddRange(rewards);
                // var pCfg = ConfigCenter.PackageCfgColl.GetDataById(pendingCfgId);
                // var cfg = ConfigCenter.RechargeCfgColl.GetDataById(pCfg.RechargeId);
                // if (cfg != null)
                // {
                //     var rewardInfo = new Resource();
                //     rewardInfo.Id = (int)GameConst.ItemIdCode.RebatePoint;
                //     rewardInfo.Type = (int)RewardType.Item;
                //     rewardInfo.Count = cfg.Points;
                //     payResult.Rewards.Add(rewardInfo);
                //     //DataCenter.totalRechargeData.Count += cfg.Points;
                // }
            }

            if (payResult.Status == 0) //检查首充  维护充值金额
            {
                var pCfg = ConfigCenter.PackageCfgColl.GetDataById(pendingCfgId);
                ReportedManager.Instance.RechargeCallBack(pendingCfgId);//数据埋点
                var rechargeCfg = ConfigCenter.RechargeCfgColl.GetDataById(pCfg.RechargeId);
                if (rechargeCfg != null) DataCenter.charcaterData.AddTotalRecharge(rechargeCfg.Price);
            }
            this.payResultCallback?.Invoke(payResult);
            this.payResultCallback = null;
            return true;
        }

        #region IPayListener

        /// <summary>
        /// 初始化失败后的回调
        /// </summary>
        /// <param name="reason">失败的原因</param>
        public void OnInitializeFailed(InitializationFailReason reason)
        {
            DHLog.Error($"IPayListener OnInitializeFailed reason = {reason}");

            isInitialized = false;
        }

        /// <summary>
        /// 初始化成功后的回调
        /// </summary>
        /// <param name="ctrl">初始化参数指定的平台Controller，用于拉起支付</param>
        /// <param name="extension">初始化参数指定的平台extension，用于获取平台的特定信息（例如：Google play的本地化价格信息）</param>
        public void OnInitialized(IPayController ctrl, IPayExtension extension)
        {
            DHLog.Debug($"IPayListener OnInitialized");

            this.controller = ctrl;
            if (extension is IGooglePayExtension payExt)
            {
                this.payExtension = payExt;
            }

            isInitialized = true;
        }

        /// <summary>
        /// 支付失败回调
        /// </summary>
        /// <param name="reason">失败原因</param>
        /// <param name="errorMsg">第三方返回的错误信息</param>
        /// <param name="productId">购买失败的商品Id，有可能是null</param>
        public void OnPurchaseFailed(PayFailReason reason, string errorMsg, string productId)
        {
            string errMsg = $"IPayListener OnPurchaseFailed reason = {reason}";
            DHLog.Error(errMsg);

     
            switch (reason)
            {
                case PayFailReason.OtherProductPaying:
                    errMsg = ConfigCenter.GetGlobalString(GlobalLanguageId.ConfRecharge4);
                    break;
                case PayFailReason.UserCancel:
                    errMsg = ConfigCenter.GetGlobalString(GlobalLanguageId.ConfRecharge3);
                    break;
                case PayFailReason.PreOrderFail:
                    errMsg = ConfigCenter.GetGlobalString(GlobalLanguageId.ConfRecharge5);
                    break;
                case PayFailReason.InvalidProductId:
                    errMsg = ConfigCenter.GetGlobalString(GlobalLanguageId.ConfRecharge6);
                    break;
                case PayFailReason.NativePayFailed:
                    errMsg = errorMsg;
                    break;
                case PayFailReason.VerifyOrderIdFailed:
                    errMsg = ConfigCenter.GetGlobalString(GlobalLanguageId.ConfRecharge7);
                    break;
                case PayFailReason.InvalidOrderInfo:
                    errMsg = ConfigCenter.GetGlobalString(GlobalLanguageId.ConfRecharge8);
                    break;
                case PayFailReason.SubsChanged:
                    errMsg = errorMsg;
                    break;
                case PayFailReason.QQPayFail:
                    errMsg = ConfigCenter.GetGlobalString(GlobalLanguageId.ConfRecharge9);
                    break;
                case PayFailReason.InitializeFailed:
                    errMsg = ConfigCenter.GetGlobalString(GlobalLanguageId.ConfRecharge10);
                    break;
            }

            if (!ReturnPayResult((int)reason, errMsg, null))
            {
                OpenMessageBox(errMsg);
            }
            
            pendingProductId = string.Empty;
            pendingCfgId = 0;
            //GameEvent.PayContainer.TriggerEvent(PayEventType.EnableClick); //TODO
        }

        /// <summary>
        ///  支付成功的回调，项目接受后，开始处理奖励下发，此处保证订单已被消耗掉，可以添加付费上报事件
        /// </summary>
        /// <param name="eventArgs">购买成功的商品参数，包含productId， OrderId</param>
        public void ProcessPurchase(PayEventArgs eventArgs)
        {
            DHLog.Debug($"IPayListener ProcessPurchase: OrderId = {eventArgs.OrderId}, ProductId = {eventArgs.ProductId}");

            if (eventArgs.ProductId == pendingProductId)
            {
                SendRewardPay(eventArgs.OrderId, eventArgs.ProductId).Forget();
            }
            else
            {
                SendLoginRewardPay(eventArgs.OrderId, eventArgs.ProductId).Forget();
            }

            //GameEvent.PayContainer.TriggerEvent(PayEventType.EnableClick); //TODO
        }

        #endregion
    }
}