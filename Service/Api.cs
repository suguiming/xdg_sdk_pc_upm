using System;
using System.Collections.Generic;
using TapTap.Bootstrap;
using TapTap.Common;
using TapTap.Login;
using UnityEngine;

namespace SDK.PC{
    public class Api{
        private readonly static string BASE_URL = "http://test-xdsdk-intnl-6.xd.com"; //测试
        // private readonly static string BASE_URL = " https://xdsdk-intnl-6.xd.com"; //正式

        //获取配置
        private readonly static string INIT_SDK = BASE_URL + "/api/init/v1/config";

        //IP信息
        private readonly static string IP_INFO = "https://ip.xindong.com/myloc2";

        // login
        private readonly static string XDG_USER_PROFILE = BASE_URL + @"/api/account/v1/info";

        //游客
        private readonly static string XDG_COMMON_LOGIN = BASE_URL + @"/api/login/v1/union";

        // 与leanClound同步
        private readonly static string XDG_LOGIN_SYN = BASE_URL + @"/api/login/v1/syn";

        // 获取用户绑定信息
        private readonly static string XDG_CHECK_BIND_STATU = BASE_URL + @"/api/account/v1/bind/list";

        // 三方绑定接口
        private readonly static string XDG_BIND_INTERFACE = BASE_URL + @"/api/account/v1/bind";

        // 三方解绑接口
        private readonly static string XDG_UNBIND_INTERFACE = BASE_URL + @"/api/account/v1/unbind";

        private readonly static string TDSG_GLOBAL_SDK_DOMAIN = @"https://xdg-1c20f-intl.xd.com";


        public static void InitSDK(string sdkClientId,
            Action<bool> callback){
            DataStorage.SaveString(DataStorage.ClientId, sdkClientId);
            Net.GetRequest(INIT_SDK, null, (data) => {
                var model = XDGSDK.GetModel<InitConfigModel>(data);
                
                //临时用一下
                if (model.data.configs.tapSdkConfig == null){
                    XDGSDK.Log("接口没有tap配置，用临时配置");
                    model.data.configs.tapSdkConfig = new InitConfigModel.TapSdkConfig();
                    model.data.configs.tapSdkConfig.clientId = "jfqhF3x9mat70ez52i";
                    model.data.configs.tapSdkConfig.clientToken = "C91pZh9OJNt7oDPx3lku4H01HelnYjSBS1jaZJed";
                    model.data.configs.tapSdkConfig.enableTapDB = true;
                    model.data.configs.tapSdkConfig.tapDBChannel = "tapdb_channel_1";
                    model.data.configs.tapSdkConfig.serverUrl = "https://jfqhf3x9.cloud.tds1.tapapis.cn";
                }

                InitConfigModel.SaveToLocal(model);

                var tapCfg = model.data.configs.tapSdkConfig;
                TapLogin.Init(tapCfg.clientId, false, false);
                var config = new TapConfig.Builder()
                    .ClientID(tapCfg.clientId) // 必须，开发者中心对应 Client ID
                    .ClientToken(tapCfg.clientToken) // 必须，开发者中心对应 Client Token
                    .ServerURL(tapCfg.serverUrl) // 开发者中心 > 你的游戏 > 游戏服务 > 云服务 > 数据存储 > 服务设置 > 自定义域名 绑定域名
                    .RegionType(RegionType.IO) // 非必须，默认 CN 表示国内
                    .TapDBConfig(tapCfg.enableTapDB, tapCfg.tapDBChannel, "1.0")
                    .ConfigBuilder();
                TapBootstrap.Init(config);
                
                callback(true);
                XDGSDK.Tmp_IsInited = true;
                XDGSDK.Tmp_IsInitSDK_ing = false;
            }, (code, msg) => {
                XDGSDK.Log("初始化失败 code: " + code + " msg: " + msg);
                callback(false);
                XDGSDK.Tmp_IsInitSDK_ing = false;
            });
        }
        
        public static void LoginTyType(LoginType loginType, Action<bool, XDGUserModel> callback){
            Dictionary<string, object> param = new Dictionary<string, object>{
                {"type", (int) loginType},
                {"token", SystemInfo.deviceUniqueIdentifier}
            };
            Net.PostRequest(XDG_COMMON_LOGIN, param, (data) => {
                var model = XDGSDK.GetModel<TokenModel>(data);
                TokenModel.SaveToLocal(model);
                GetUserInfo((userSuccess, userMd) => {
                    if (userSuccess){
                        SyncTdsUser(tdsSuccess => {
                            if (tdsSuccess){
                                CheckPrivacyAlert(isPass => {
                                    if (isPass){
                                        callback(true, userMd);
                                    } 
                                });
                            } else{
                                callback(false, null);
                            }
                        });
                    } else{
                        callback(false, null);
                    }
                });
            }, (code, msg) => {
                XDGSDK.Log("登录失败 code: " + code + " msg: " + msg);
                callback(false, null);
            });
        }

        private static void SyncTdsUser(Action<bool> callback){
            Net.PostRequest(XDG_LOGIN_SYN, null, (data) => {
                var md = XDGSDK.GetModel<SyncTokenModel>(data);
                XDGSDK.Log("sync token: " + md.data.sessionToken);
                TDSUser.BecomeWithSessionToken(md.data.sessionToken);
                callback(true);
            }, (code, msg) => {
                XDGUserModel.ClearUserData();
                callback(false);
                XDGSDK.Log("SyncTdsUser 失败 code: " + code + " msg: " + msg);
            });
            
        }

        public static void GetUserInfo(Action<bool, XDGUserModel> callback){
            Net.GetRequest(XDG_USER_PROFILE, null, (data) => {
                var model = XDGSDK.GetModel<XDGUserModel>(data);
                XDGUserModel.SaveToLocal(model);
                callback(true, model);
            }, (code, msg) => {
                XDGSDK.Log("获取用户信息失败 code: " + code + " msg: " + msg);
                callback(false, null);
            });
        }

        public static void GetIpInfo(Action<bool, IpInfoModel> callback){
            RequestIpInfo(true, callback);
        }

        private static void RequestIpInfo(bool repeat, Action<bool, IpInfoModel> callback){
            Net.GetRequest(IP_INFO, null, (data) => {
                var model = XDGSDK.GetModel<IpInfoModel>(data);
                IpInfoModel.SaveToLocal(model);
                callback(true, model);
            }, (code, msg) => {
                if (repeat){
                    RequestIpInfo(false, callback);
                } else{
                    var oldMd = IpInfoModel.GetLocalModel();
                    if (oldMd != null){
                        callback(true, oldMd);
                    } else{
                        XDGSDK.Log("获取 ip info 失败 code: " + code + " msg: " + msg);
                        callback(false, null);
                    }
                }
            });
        }

        private static void CheckPrivacyAlert(Action<bool> callback){
            if (InitConfigModel.CanShowPrivacyAlert()){
                UIManager.ShowUI<PrivacyAlert>(null, (code, objc) => {
                    if (code == UIManager.RESULT_SUCCESS){
                        callback(true);
                    } 
                });
            } else{
                callback(true);
            }
        }
    }
}