using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using QQRobotFramework;

namespace PluginBingIce
{
    public class ice:Plugin
    {
        public string cookie = "";
        public string referer = "";
        public string messageUrl = "";
        public string st = "";
        public string aid = "";
        public string ak = "";
        public string QcloudBotId = "";
        List<string> TxtAi = new List<string>();
        List<string> PluginType = new List<string>();
        int isRev = 0;
      
        public ice()
        {
            this.PluginName = "微软小冰";
        }
        public override string Install()
        {
            if (!Directory.Exists(Robot.path + @"PluginBingIce\"))
            {
                Directory.CreateDirectory(Robot.path + @"PluginBingIce\");
            }
            SetDefault();


            return "success";
        }

        internal void SetDefault()
        {
            if (File.Exists(Robot.path + @"PluginBingIce\cookie.plugin"))
            {
                cookie = File.ReadAllText(Robot.path + @"PluginBingIce\cookie.plugin");
            }
            if (File.Exists(Robot.path + @"PluginBingIce\referer.plugin"))
            {
                referer = File.ReadAllText(Robot.path + @"PluginBingIce\referer.plugin");
            }
            if (File.Exists(Robot.path + @"PluginBingIce\messageUrl.plugin"))
            {
                messageUrl = File.ReadAllText(Robot.path + @"PluginBingIce\messageUrl.plugin");
            }
            if (File.Exists(Robot.path + @"PluginBingIce\st.plugin"))
            {
                st = File.ReadAllText(Robot.path + @"PluginBingIce\st.plugin");
            }
            if (File.Exists(Robot.path + @"PluginBingIce\txtai.plugin"))
            {
                TxtAi = File.ReadAllLines(Robot.path + @"PluginBingIce\txtai.plugin").ToList<string>();
            }
            if (File.Exists(Robot.path + @"PluginBingIce\aid.plugin"))
            {
                aid = File.ReadAllText(Robot.path + @"PluginBingIce\aid.plugin");
            }
            if (File.Exists(Robot.path + @"PluginBingIce\ak.plugin"))
            {
                ak = File.ReadAllText(Robot.path + @"PluginBingIce\ak.plugin");
            }
            if (File.Exists(Robot.path + @"PluginBingIce\QcloudBotId.plugin"))
            {
                QcloudBotId = File.ReadAllText(Robot.path + @"PluginBingIce\QcloudBotId.plugin");
            }
            PluginType.Clear();
            if (cookie != "" && referer != "" && messageUrl != "" && st != "")
            {
                PluginType.Add("微软小冰");
            }
            if (aid != "" && ak != "" && QcloudBotId!="")
            {
                PluginType.Add("腾讯小冰");
            }
        }

        public override string Start()
        {
            SetDefault();
            isRev = 0;
            
            
            Event.OnMessage += Event_OnMessage;
            
            return "success";
        }

        

        public string iceSend(string message, RevMessageEvent e=null)
        {
            string rev = "";
            if (message.Trim() != "")
            {

                try
                {
                    isRev = 1;
                    WebClient web = new WebClient();
                    web.Encoding = Encoding.UTF8;
                    web.Headers.Add("User-Agent: Mozilla/5.0 (iPhone; CPU iPhone OS 13_2_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0.3 Mobile/15E148 Safari/604.1 Edg/88.0.4324.182");
                    web.Headers.Add("referer: "+ referer);
                    web.Headers.Add("content-type: application/x-www-form-urlencoded");
                    web.Headers.Add("Cookie: " + cookie);
                    string data = web.UploadString("https://m.weibo.cn/msgDeal/sendMsg?", st+System.Web.HttpUtility.UrlEncode(message));
                    
                    try
                    {
                        SendResult result = JsonHelper.DeserializeObject<SendResult>(data);
                        if (result.ok == 1)
                        {
                            OnLog("提交数据成功："+message);
                            if (e!=null)
                            {
                                Cluster.Send(e.group_id, "提交数据成功：" + message);
                            }
                            int i = 0;
                            while (i < 5)
                            {
                                WebClient client = new WebClient();
                                client.Encoding = Encoding.UTF8;
                                client.Headers.Add("User-Agent: Mozilla/5.0 (iPhone; CPU iPhone OS 13_2_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0.3 Mobile/15E148 Safari/604.1 Edg/88.0.4324.182");
                               // client.Headers.Add("referer: " + referer);
                                
                                client.Headers.Add("Cookie: " + cookie);
                                string r = client.DownloadString(messageUrl);
                               
                                try
                                {
                                    SendResult rv = JsonHelper.DeserializeObject<SendResult>(r);
                                    if (rv.ok == 1 && rv.data != null)
                                    {
                                        
                                        if (rv.data.Count() > 0)
                                        {
                                            
                                            AskRev ask = rv.data[0];
                                          
                                            if (ask.sender_screen_name == "小冰")
                                            {
                                                rev = ask.text;
                                                OnLog("收到小冰回复："+rev);
                                                
                                                rev = Regex.Replace(rev, @"\<.+?\>", "");
                                                if (e != null)
                                                {
                                                    Cluster.Send(e.group_id, "收到小冰回复：" + rev);
                                                }
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        OnLog(r);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    OnLog(ex.Message);
                                }
                                i++;
                                Thread.Sleep(2000);
                            }

                        }
                        else
                        {
                            if (e != null)
                            {
                                Cluster.Send(e.group_id, "提交数据失败：" + data);
                            }
                            OnLog(data);
                        }
                    }
                    catch(Exception ex)
                    {
                        if (e != null)
                        {
                            Cluster.Send(e.group_id, "提交数据失败：" + ex.Message);
                        }
                        OnLog(data+ex.ToString());
                    }
                    finally
                    {
                        
                    }
                }
                catch (Exception ex)
                {
                    if (e != null)
                    {
                        Cluster.Send(e.group_id, "提交数据失败：" + ex.Message);
                    }
                    OnLog(ex.Message);
                }
                finally
                {
                    
                }

            }
            isRev = 0;
            return rev;
        }
        private void Event_OnMessage(string sender, RevMessageEvent e)
        {
            
            if(e.post_type == "message" && e.message_type == "group" && e.group_id > 0 && e.message!=null && (e.message=="开启小冰审核"|| e.message == "关闭小冰审核" || (e.message.Length>5 && e.message.Substring(0,5)== "开启小冰 ") || e.message=="开启小冰"|| e.message =="关闭小冰" || e.message == "小冰"))
            {
                if (Robot.Admin.Contains(e.user_id.ToString()))
                {
                    e.Exit = true;
                    if(e.message == "开启小冰")
                    {
                        File.WriteAllText(Robot.path + @"PluginBingIce\" + e.group_id, "微软小冰");
                        Cluster.Send(e.group_id, e.message + "成功");
                    }
                    else if (e.message == "关闭小冰")
                    {
                        File.Delete(Robot.path + @"PluginBingIce\" + e.group_id);
                        Cluster.Send(e.group_id, e.message + "成功");
                    }
                    else if (e.message == "开启小冰审核")
                    {
                        if( !TxtAi.Contains(e.group_id.ToString()))
                        {
                            TxtAi.Add(e.group_id.ToString());
                            File.WriteAllLines(Robot.path + @"PluginBingIce\txtai.plugin",TxtAi.ToArray());
                        }
                        Cluster.Send(e.group_id, e.message + "成功");
                    }
                    else if (e.message == "关闭小冰审核")
                    {
                        if (TxtAi.Contains(e.group_id.ToString()))
                        {
                            TxtAi.Remove(e.group_id.ToString());
                            File.WriteAllLines(Robot.path + @"PluginBingIce\txtai.plugin", TxtAi.ToArray());
                        }
                        Cluster.Send(e.group_id, e.message + "成功");
                       
                    }
                    else if (e.message.Length > 5 && e.message.Substring(0, 5) == "开启小冰 ")
                    {
                        string bot = e.message.Substring(5);
                        if( !PluginType.Contains(bot))
                        {
                            Cluster.Send(e.group_id, e.message + "失败，"+ bot + "不存在->"+string.Join("|", PluginType.ToArray()));
                            return;
                        }
                        File.WriteAllText(Robot.path + @"PluginBingIce\" + e.group_id, bot);
                        Cluster.Send(e.group_id, e.message + "成功");
                    }
                    else
                    {
                        if(File.Exists(Robot.path + @"PluginBingIce\" + e.group_id))
                        {
                            string usingbot = File.ReadAllText(Robot.path + @"PluginBingIce\" + e.group_id);
                            string data = "[CQ:at,qq=" + e.user_id + "]我在线，当前使用接口：" + usingbot + "\n相关指令：\n关闭请发【关闭小冰】";
                            foreach(string s in PluginType)
                            {
                                data += "\n开启"+s+"请发【开启小冰 "+s+"】";
                            }

                            Cluster.Send(e.group_id, data);
                        }
                        else
                        {
                            Cluster.Send(e.group_id, "[CQ:at,qq=" + e.user_id + "]我没在线，开启请发【开启小冰】");
                        }
                    }
                    
                }
                return;
            }
            

            if(isRev==0 && e.post_type=="message" && e.message_type=="group" && e.group_id>0 && e.message!=null)
            {
                if (!File.Exists(Robot.path + @"PluginBingIce\" + e.group_id))
                {
                    
                    return;
                }
                string usingbot = File.ReadAllText(Robot.path + @"PluginBingIce\" + e.group_id);
                if (!PluginType.Contains(usingbot))
                {
                   
                    return;
                }
                string message = e.message;
                
                
                if ( (message.IndexOf("冰冰") > -1 && message.Substring(0, 2) == "冰冰")||(message.IndexOf(",qq="+Robot.id) > -1) )
                {
                    e.Exit = true;
                    message = Regex.Replace(e.message, @"\[CQ\:(?<CQ>.*?)\,.*?\]", "");
                    message = message.Replace("冰冰", "").Trim();
                    
                    if (message != "")
                    {
                        OnLog(message);
                        
                        
                        RevMessageEvent zj = null;
                       if(message.Length>2&& message.Substring(0,2)=="自检"&& Robot.Admin.Contains(e.user_id.ToString()))
                        {
                            message = message.Replace("自检", "").Trim();
                            zj = e;
                        }
                        string rev;
                        if (usingbot == "微软小冰")
                        {
                            rev = iceSend(message, zj);
                        }
                        else
                        {
                            
                            rev = qcloudSend(message, zj);
                        }
                        if (rev != "")
                        {
                            Cluster.Send(e.group_id, rev);
                        }
                    }
                    
                    
                }
                
            }
        }
        private string Sha256(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            SHA256 shaM = new SHA256Managed();
            byte[] hashBytes = shaM.ComputeHash(data);
            string hex = BitConverter.ToString(hashBytes, 0).Replace("-", string.Empty).ToLower();
            return hex;
        }
        private byte[] Encrypt(byte[] messageBytes, string secret)
        {
            secret = secret ?? "";
            var encoding = new UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secret);
           
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                
                return hashmessage;
            }
        }
        private string qcloudSend(string message, RevMessageEvent e)
        {
            string rev = "";
            try
            {
                string service = "tbp";
                string endpoint = "tbp.tencentcloudapi.com";
                string region = "ap-guangzhou";
                string action = "TextProcess";
                string version = "2019-06-27";
                TextProcessRequest req = new TextProcessRequest
                {
                    BotId = QcloudBotId,
                    BotEnv = "dev",
                    InputText = message,
                    TerminalId = "bing_ice"
                };
                string requestPayload = JsonHelper.SerializeObject(req);
                DateTime date = DateTime.UtcNow;
                Dictionary<string, string> headers = Qcloud.BuildHeaders(aid, ak, service, endpoint, region, action, version, date, requestPayload);
                 WebClient webClient = new WebClient();
                webClient.Encoding = Encoding.UTF8;
                foreach (KeyValuePair<string, string> kv in headers)
                {
                    webClient.Headers.Add(kv.Key + ": " + kv.Value);
                    Console.WriteLine(kv.Key + ": " + kv.Value);
                }
                
                string resp = webClient.UploadString("https://tbp.tencentcloudapi.com/", requestPayload);
                TextProcessResponse response = JsonHelper.DeserializeObject<TextProcessResponse>(resp);
                if (response.Response == null)
                {
                    OnLog("获取数据失败");
                    if (e != null)
                    {
                        Cluster.Send(e.group_id, "获取数据失败NULL");
                    }
                }else if (response.Response.Error != null)
                {
                    OnLog("获取数据失败："+ response.Response.Error.Message);
                    if (e != null)
                    {
                        Cluster.Send(e.group_id, "获取数据失败：" + response.Response.Error.Message);
                    }
                }
                else
                {
                    rev = response.Response.ResponseMessage.GroupList[0].Content;
                    OnLog("收到小冰回复：" + rev);

                }
                


            }
            catch(Exception ex)
            {
                OnLog(ex.ToString());
                if (e != null)
                {
                    Cluster.Send(e.group_id, ex.Message);
                }
            }
            return rev;
        }

        public override string Stop()
        {
            Event.OnMessage -= Event_OnMessage;
            PluginType.Clear();
            return "success";
        }
        public override string UnInstall()
        {
            return "success";
        }
        public override string ShowForm()
        {
            Set set = new Set(this);
            set.ShowDialog();
            return "success";
        }
    }
    public class SendResult
    {
        public int ok { get; set; }
        public string msg { get; set; }
        public AskRev[] data { get; set; }
    }

    public class AskRev
    {
        public string sender_screen_name { get; set; }
        public string text { get; set; }
    }
    public class TextProcessRequest
    {
      
        
        public string BotId { get; set; }
        public string BotEnv { get; set; }
        public string TerminalId { get; set; }
        public string InputText { get; set; }
    }
    public class TextProcessResponse
    {


        public BotTextResponse Response { get; set; }
       
    }

    public class BotTextResponse
    {
        public BotErrorResponse Error { get; set; }
        public BotResponseMessage ResponseMessage { get; set; }
    }

    public class BotResponseMessage
    {
        public BotGroupList[] GroupList { get; set; }
    }

    public class BotGroupList
    {
        public string Content { get; set; }
    }

    public class BotErrorResponse
    {
        public string Message { get; set; }
    }
}
