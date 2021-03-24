using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using QQRobotFramework;
namespace PassWordPlugin
{
    public class PassWord:Plugin
    {
        public PassWord()
        {
            this.PluginName = "加密解密";
        }
        public override string Install()
        {
            return base.Install();
        }
        public override string UnInstall()
        {
            return base.UnInstall();
        }
        public override string Start()
        {
            Event.OnMessage += Event_OnMessage;
            return base.Start();
        }
        
        private void Event_OnMessage(string sender, RevMessageEvent e)
        {
            if(e.message != null&&e.message!=""&&e.message.IndexOf(" ")>-1)
            {
                List<string> message = Regex.Split(e.message, "\\s+", RegexOptions.IgnoreCase).ToList<string>();
                string action = message[0].ToLower();
                if (action == "aes")
                {
                    if (message.Count != 4 && message.Count != 3)
                    {
                        MessageSend(e,"AES格式不正确，请输入 AES 字符串(加密/解密) 密钥(KEY) 偏移量(IV)");
                        return;
                    }
                    if(message.Count == 3)
                    {
                        message.Add("");
                    }


                    if(message[2].Length!=32 && message[2].Length != 16)
                    {
                        MessageSend(e, "AES密钥(KEY)不正确，请输入16位字符串(AES128)或者32位字符串(AES256)");
                        return;
                    }
                    if (message[3]!="" && message[3].Length != 32 && message[3].Length != 16 && message[3].Length!= message[2].Length)
                    {
                        MessageSend(e, "AES偏移量(IV)不正确，请输入16位字符串(AES128)或者32位字符串(AES256)");
                        return;
                    }
                    int KeySize = message[2].Length == 16 ? 128 : 256;
                    
                    
                    string r = CryptoUtil.Decrypt(message[1], message[2], message[3], KeySize);
                    if (r != "") {
                        if (message[3] == "")
                        {
                            MessageSend(e, "【" + message[1] + "】经AES-ECB" + KeySize + "解密后：\n" + r);
                        }
                        else
                        {
                            MessageSend(e, "【" + message[1] + "】经AES-CBC" + KeySize + "解密后：\n" + r);
                        }
                        
                        return;
                    }
                    r = CryptoUtil.DecryptHex(message[1], message[2], message[3], KeySize);
                    if (r != "")
                    {
                        if (message[3] == "")
                        {
                            MessageSend(e, "【" + message[1] + "】经AES-ECB" + KeySize + "解密后：\n" + r);
                        }
                        else
                        {
                            MessageSend(e, "【" + message[1] + "】经AES-CBC" + KeySize + "解密后：\n" + r);
                        }

                        return;
                    }
                    r = CryptoUtil.Decrypt(message[1], message[2], message[3], KeySize, "CFB");
                    if (r != "")
                    {
                        MessageSend(e, "【" + message[1] + "】经AES-CFB" + KeySize + "解密后：\n" + r);
                        return;
                    }
                    r = CryptoUtil.DecryptHex(message[1], message[2], message[3], KeySize, "CFB");
                    if (r != "")
                    {
                        MessageSend(e, "【" + message[1] + "】经AES-CFB" + KeySize + "解密后：\n" + r);
                        return;
                    }


                    string revMessage = "";
                    string hexStr = "";
                    if (message[3] == "")
                    {
                        r = CryptoUtil.Encrypt(message[1], message[2], message[3], KeySize);
                        hexStr = CryptoUtil.BaseToHex(r);
                        revMessage += "【" + message[1] + "】经AES-ECB" + KeySize + "加密后";
                        revMessage += "\nBase64：" + r;
                        if (hexStr != "")
                        {
                            revMessage += "\nHEX：" + hexStr;
                        }
                        MessageSend(e, revMessage);
                        return;
                    }
                    r = CryptoUtil.Encrypt(message[1], message[2], message[3], KeySize);
                    hexStr = CryptoUtil.BaseToHex(r);
                    revMessage += "【" + message[1] + "】经AES-CBC" + KeySize + "加密后";
                    revMessage += "\nBase64：" + r;
                    if (hexStr != "")
                    {
                        revMessage += "\nHEX：" + hexStr;
                    }
                    r = CryptoUtil.Encrypt(message[1], message[2], message[3], KeySize, "CFB");
                    hexStr = CryptoUtil.BaseToHex(r);
                    revMessage += "\n【" + message[1] + "】经AES-CFB" + KeySize + "加密后";
                    revMessage += "\nBase64：" + r;
                    if (hexStr != "")
                    {
                        revMessage += "\nHEX：" + hexStr;
                    }
                    
                    MessageSend(e, revMessage);
                    return;
                }
                if (action == "md5")
                {
                    if (message.Count != 2)
                    {
                        MessageSend(e, "MD5格式不正确，请输入 MD5 待加密字符串");
                        return;
                    }
                    string r = CryptoUtil.md5(message[1]);
                    string rHex = CryptoUtil.MD5Hex(message[1]);
                    string revMessage = "【" + message[1] + "】经MD5加密后";
                    revMessage += "\n32位：" + r;
                    if (rHex != "")
                    {
                        revMessage += "\n16位：" + rHex;
                    }
                    MessageSend(e, revMessage);
                    return;
                }
                if (action == "base64")
                {
                    if (message.Count != 2)
                    {
                        MessageSend(e, "Base64格式不正确，请输入Base64 待加密/解密字符串");
                        return;
                    }
                    string r = CryptoUtil.Base64De(message[1]);
                    if (r != "")
                    {
                        MessageSend(e, "【" + message[1] + "】经Base64解密后：\n" + r);
                        return;
                    }
                    r = CryptoUtil.Base64(message[1]);
                    MessageSend(e, "【" + message[1] + "】经Base64加密后：\n" + r);
                    return;
                }
            }
            
        }

        private void MessageSend(RevMessageEvent e, string v)
        {
            if (e.message_type !=null)
            {
                if (e.message_type == "private")
                {
                    Friend.Send(e.user_id, v);
                }
                else
                {
                    Cluster.Send(e.group_id,"[CQ:at,qq="+e.user_id+"]"+v);
                }
            }
        }

        public override string Stop()
        {
            Event.OnMessage -= Event_OnMessage;
            return base.Stop();
        }
        public override string ShowForm()
        {
            return base.ShowForm();
        }
    }
}
