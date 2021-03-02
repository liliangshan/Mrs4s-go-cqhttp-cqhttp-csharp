using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using QQRobotFramework;
using System.Data;
using System.Threading;

namespace PluginZjmf
{
    public class zjmf:Plugin
    {
        IDbConnection Conn = null;
        Dictionary<uint, DateTime> Users = new Dictionary<uint, DateTime>();
        public zjmf()
        {
            this.PluginName = "智简魔方问答";
        }
        public override string Install()
        {
            if (!Directory.Exists(Robot.path + @"PluginZjmf\"))
            {
                Directory.CreateDirectory(Robot.path + @"PluginZjmf\");
            }
            if (!File.Exists(Robot.path + @"PluginZjmf\zjmf.db"))
            {
                byte[] bySave = Resource1.zjmf;
                FileStream fsObj = new FileStream(Robot.path + @"PluginZjmf\zjmf.db", FileMode.CreateNew);
                fsObj.Write(bySave, 0, bySave.Length);
                fsObj.Close();
            }
            return base.Install();
        }
        public override string UnInstall()
        {
            return base.UnInstall();
        }
        public override string ShowForm()
        {
            return base.ShowForm();
        }
        private void DbConn()
        {
            if (Conn == null)
            {
                DbBase db = new DbBase(@"Data Source=" + Robot.path + @"PluginZjmf\zjmf.db");
                Conn = db.Conn;
            }
            if (Conn.State != ConnectionState.Open)
            {
                Conn.Open();
            }
        }
        public override string Start()
        {
            DbConn();
            Event.OnMessage += Event_OnMessage;
            return base.Start();

        }

        private void Event_OnMessage(string sender, RevMessageEvent e)
        {
            if(e.post_type=="message" && e.message_type=="group" && (e.group_id == 867571060 || e.group_id == 1040064691|| e.group_id == 1101678530) && e.message!=null)
            {
                e.Exit = true;
                
                if (e.sender != null && (e.sender.role != "member"|| Robot.Admin.Contains(e.user_id.ToString())))
                {
                    if( e.message.Length>3 && e.message.Substring(0,3)=="del")
                    {
                        string data = e.message.Substring(3).Trim();
                        List<long> idx = SqlConn.GetMessageByKey(e.group_id, data);
                        int i = 0;
                        foreach (long id in idx)
                        {
                           
                           Cluster.DelMsg(id);
                            i++;
                            if (i > 8)
                            {
                                i = 0;
                                Thread.Sleep(1000);
                            }
                        }
                        Cluster.Send(e.group_id, "撤回"+ idx.Count + "条记录成功");
                    }
                    return;
                }
                if (e.message.IndexOf("CQ:redbag") > -1)
                {
                    Cluster.DelMsg(e.message_id);
                    return;
                }
                if (Users.ContainsKey(e.user_id))
                {
                    OnLog((DateTime.Now - Users[e.user_id]).TotalMinutes.ToString());
                    if( (DateTime.Now - Users[e.user_id]).TotalMinutes<20)
                    {
                        return;
                    }
                }
                string message = e.message;
                Match match = new Regex(@"\[CQ:image,file=(?<IMG>[a-zA-Z0-9\-\.\{\}]+),url=(.+?)\]", RegexOptions.IgnoreCase).Match(e.message);
                while (match.Success)
                {
                    string file = match.Groups["IMG"].ToString();
                    message +=Cluster.Ocr(file);
                    match = match.NextMatch();
                }
                message = Regex.Replace(message, @"\[.+?\]", "");
                if (message.IndexOf("thinklexceptionErrorException") > -1)
                {
                    Cluster.Send(e.group_id, "[CQ:at,qq=" + e.user_id + "][CQ:image,file=file:///" + Robot.path + @"\PluginZjmf\1.png][CQ:image,file=file:///" + Robot.path + @"\PluginZjmf\2.png]");
                    return;
                }
                if (message.ToLower().IndexOf("ioncube") > -1)
                {
                    Cluster.Send(e.group_id, "[CQ:at,qq=" + e.user_id + "][CQ:image,file=file:///" + Robot.path + @"\PluginZjmf\3.png]");
                    return;
                }
                int LenSet = 1;
                if(message.IndexOf("魔方")>-1)
                {
                    LenSet = 0;
                }
                message = message.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("托管", "").Replace("租用", "").Replace("代理", "").Replace("挂机宝", "").Replace("玩意", "").Replace("智简", "").Replace("魔方", "");
                if (message != "" && message.Length>5)
                {
                    OnLog(message);
                    string Token = "{\"Date\":\"\",\"Token\":\"\"}";
                    string access_token = "";
                    if (File.Exists(Robot.path + @"PluginZjmf\token"))
                    {
                        Token = File.ReadAllText(Robot.path + @"PluginZjmf\token");
                    }
                    TokenJson json = JsonHelper.DeserializeObject<TokenJson>(Token);
                    if (json.Date != "" && json.Token != "")
                    {
                        DateTime time = DateTime.Parse(json.Date);
                        if( (DateTime.Now-time).TotalSeconds<0)
                        {
                            access_token = json.Token;
                        }
                    }
                    if (access_token == "")
                    {
                        access_token = GetAccessToken();
                    }
                    if (access_token != "")
                    {
                        WebClient web = new WebClient();
                        web.Encoding = Encoding.UTF8;
                        BaiduSendText text = new BaiduSendText();
                        text.text = message;
                        string body = JsonHelper.SerializeObject(text);
                        web.Headers.Add("Content-Type:application/json");
                        string data = web.UploadString("https://aip.baidubce.com/rpc/2.0/nlp/v1/lexer?charset=UTF-8&access_token=" + access_token, body);
                        BaiduText t = JsonHelper.DeserializeObject<BaiduText>(data);
                        List<string> item = new List<string>();
                        List<string> Tag = new List<string>();
                        if (t.items != null)
                        {
                            foreach (BaiduTextItems items in t.items)
                            {
                                if (items.pos == "n"|| items.pos == "nz")
                                {
                                    if (!Tag.Contains(items.item) && items.item.Length>1)
                                    {
                                        item.Add( "content LIKE '%"+ items.item + "%'" );
                                        Tag.Add(items.item);
                                    }
                                }
                            }
                            if (item.Count > LenSet)
                            {
                                
                                IDbCommand command = Conn.CreateCommand();
                                command.CommandText = "SELECT * FROM zjmf WHERE "+string.Join(" AND ",item.ToArray())+" LIMIT 3";
                                IDataReader reader = command.ExecuteReader();
                                List<string> msg = new List<string>();
                                msg.Add(string.Join(",", Tag.ToArray()) + " 相关问题：");
                                while (reader.Read())
                                {
                                    msg.Add( "["+ reader["type2"] + "]"+ reader["title"]+":"+ reader["url"]);
                                }
                                reader.Close();
                                if (msg.Count > 1)
                                {
                                    Users.Add(e.user_id, DateTime.Now);
                                    Cluster.Send(e.group_id, string.Join("\n", msg.ToArray()));
                                    
                                }
                            }
                            
                            
                        }
                    }


                }
                
            }
        }

        private string GetAccessToken()
        {
            WebClient web = new WebClient();
            web.Encoding = Encoding.UTF8;
            string data = web.DownloadString("https://aip.baidubce.com/oauth/2.0/token?grant_type=client_credentials&client_id=jNxCRGAGScaZkSAaBL6LZbwX&client_secret=HkMqFc9ONxb1QXyvsjaZwuQ0WF0ZnKGm");
            BaiduToken token = JsonHelper.DeserializeObject<BaiduToken>(data);
            DateTime time = DateTime.Now.AddSeconds(token.expires_in-86400*3);
            TokenJson tokenJson = new TokenJson();
            tokenJson.Date = time.ToString("yyyy-MM-dd HH:mm:ss");
            tokenJson.Token = token.access_token;
            File.WriteAllText(Robot.path + @"PluginZjmf\token",JsonHelper.SerializeObject(tokenJson));
            return token.access_token;
        }

        public override string Stop()
        {
            try
            {
                if (Conn != null) Conn.Close();
                Conn = null;
            }
            catch
            {

            }
            Event.OnMessage -= Event_OnMessage;
            return base.Stop();
        }
    }
    public class TokenJson
    {
        public string Date { get; set; }
        public string Token { get; set; }
    }
    public class BaiduToken
    {
        public int expires_in { get; set; }
        public string access_token { get; set; }
    }
    public class BaiduSendText
    {
        public string text { get; set; }
    }
    public class BaiduText
    {
        public BaiduText()
        {
            error_msg = "";
        }
        public string error_msg { get; set; }
        public BaiduTextItems[] items { get; set; }
    }

    public class BaiduTextItems
    {
        public BaiduTextItems()
        {
            pos = "";
        }
        public string pos { get; set; }
        public string item { get; set; }

    }
}
