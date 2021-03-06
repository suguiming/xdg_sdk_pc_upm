using TapTap.Common;
using UnityEngine;
using UnityEngine.UI;

namespace com.xd.intl.pc{
    public class DeleteAccountAlert : UIElement{
        public GameObject pannelOne;
        public Text titleOne;
        public Text msgOne;
        public Button cancelOne;
        public Button sureOne;

        public GameObject pannelTwo;
        public InputField fieldTwo;
        public Text hintTxt;
        public Text titleTwo;
        public Text msgTwo;
        public Button cancelTwo;
        public Button sureTwo;

        private LoginType loginType = LoginType.Guest;
        private DeleteAlertType alertType = DeleteAlertType.DeleteGuest;
        private LanguageModel langMd;
        private bool inputError = false;

        void Start(){
            langMd = LanguageMg.GetCurrentModel();
            loginType = (LoginType) SafeDictionary.GetValue<int>(extra, "loginType");
            alertType = (DeleteAlertType) SafeDictionary.GetValue<int>(extra, "alertType");
            
            if (alertType == DeleteAlertType.DeleteGuest){ //删除游客
                titleOne.text = langMd.tds_delete_account_title;
                msgOne.text = langMd.tds_delete_content;
                cancelOne.transform.Find("Text").GetComponent<Text>().text = langMd.tds_cancel;
                sureOne.transform.Find("Text").GetComponent<Text>().text = langMd.tds_delete_account_sure;

                //two
                titleTwo.text = langMd.tds_delete_account;
                msgTwo.text = langMd.tds_delete_confirm_content;
                cancelTwo.transform.Find("Text").GetComponent<Text>().text = langMd.tds_cancel;
                sureTwo.transform.Find("Text").GetComponent<Text>().text = langMd.tds_delete_account;
                
            } else if (alertType == DeleteAlertType.Unbindthird){ //解绑第三方
                titleOne.text = langMd.tds_unbind_account_title;
                msgOne.text = langMd.tds_unbind_content.Replace("%s", new LoginTypeModel(loginType).typeName);
                cancelOne.transform.Find("Text").GetComponent<Text>().text = langMd.tds_cancel;
                sureOne.transform.Find("Text").GetComponent<Text>().text = langMd.tds_unbind_account;

                //two
                titleTwo.text = langMd.tds_unbind_account;
                msgTwo.text = langMd.tds_unbind_confirm_Content;
                cancelTwo.transform.Find("Text").GetComponent<Text>().text = langMd.tds_cancel;
                sureTwo.transform.Find("Text").GetComponent<Text>().text = langMd.tds_unbind_account_button;
                
            } else{ //删除第三方
                titleOne.text = langMd.tds_delete_account_title;
                msgOne.text = langMd.tds_unbind_delete_content.Replace("%s", new LoginTypeModel(loginType).typeName);
                cancelOne.transform.Find("Text").GetComponent<Text>().text = langMd.tds_cancel;
                sureOne.transform.Find("Text").GetComponent<Text>().text = langMd.tds_delete_account_sure;

                //two
                titleTwo.text = langMd.tds_delete_account;
                msgTwo.text = langMd.tds_delete_confirm_content;
                cancelTwo.transform.Find("Text").GetComponent<Text>().text = langMd.tds_cancel;
                sureTwo.transform.Find("Text").GetComponent<Text>().text = langMd.tds_delete_account;
                
            }

            fieldTwo.onValueChanged.AddListener((param) => { OnInputFieldChange(param); });
            Invoke("updatePosition", 0.1f);
        }

        private void updatePosition(){
            var x = cancelOne.transform.GetComponent<RectTransform>().sizeDelta.x / 2;
            sureOne.transform.localPosition = new Vector3(-x, -60f, 0);
            
            var x2 = 10 + sureTwo.transform.GetComponent<RectTransform>().sizeDelta.x / 2;
            cancelTwo.transform.localPosition = new Vector3(-x2, -60f, 0);
        }

        private void OnInputFieldChange(string txt){
            if (inputError){
                hintTxt.text = "";
                fieldTwo.GetComponent<Image>().sprite = Resources.Load("Images/border_gray", typeof(Sprite)) as Sprite;
            }

            inputError = false;
        }

        public void cancelOneTap(){
            UIManager.Dismiss();
        }

        public void sureOneTap(){
            pannelOne.SetActive(false);
            pannelTwo.SetActive(true);
        }

        public void cancelTwoTap(){
            UIManager.Dismiss();
        }

        public void sureTwoTap(){
            var str = fieldTwo.text;
            if (alertType == DeleteAlertType.DeleteGuest || alertType == DeleteAlertType.DeleteThird){ //删除游客或第三方
                if (!"Delete".Equals(str)){
                    hintTxt.text = langMd.tds_input_error;
                    fieldTwo.GetComponent<Image>().sprite =
                        Resources.Load("Images/border_red", typeof(Sprite)) as Sprite;
                    inputError = true;
                } else{
                    OnCallback(UIManager.RESULT_SUCCESS, "确认删除或解绑");
                    UIManager.Dismiss();
                }
            } else{
                if (!"Confirm".Equals(str)){
                    inputError = true;
                    fieldTwo.GetComponent<Image>().sprite =
                        Resources.Load("Images/border_red", typeof(Sprite)) as Sprite;
                    hintTxt.text = langMd.tds_input_error;
                } else{
                    OnCallback(UIManager.RESULT_SUCCESS, "确认删除或解绑");
                    UIManager.Dismiss();
                }
            }
        }
    }

    public enum DeleteAlertType : int{ 
        DeleteGuest = 0,   //删除游客
        DeleteThird = 1,   //删除第三方
        Unbindthird = 2,   //解绑第三方
    }

}