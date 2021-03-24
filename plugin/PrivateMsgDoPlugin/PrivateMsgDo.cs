using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using QQRobotFramework;
namespace PrivateMsgDoPlugin
{
    public class PrivateMsgDo:Plugin
    {
        
        public PrivateMsgDo()
        {
            this.PluginName = "私聊操作";
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
           
            if (e.post_type=="message" && e.message_type== "private" && e.message!=null && e.message.IndexOf(" ")>-1)
            {
                if (e.message == "do reload")
                {
                    if (ListenEvent.Socket != null)
                    {
                        Friend.Send(e.user_id, "管理地址生成中，请稍候，如果生成的地址无法使用请重新发送do reload");
                        ListenEvent.ListenSocket();

                        Thread.Sleep(1000);
                        Friend.Send(e.user_id, "http://" + ListenEvent.IP + ":" + ListenEvent.Port + "/?uid=" + CryptoUtil.EncryptHex(e.user_id.ToString() + "." + Static.TimeStamp(), CryptoUtil.md5("kuaijieyun"), CryptoUtil.md5("Cn"), 256, "CFB"));

                        return;
                    }
                    e.message = "do web";
                }
                if(e.message.Length>5 && e.message.Substring(0,6)=="do web")
                {
                    string rip = "";

                    if (ListenEvent.Socket != null)
                    {
                        rip += ListenEvent.IP+":" + ListenEvent.Port;
                        Friend.Send(e.user_id, "http://" + rip + "/?uid=" + CryptoUtil.EncryptHex(e.user_id.ToString() + "." + Static.TimeStamp(), CryptoUtil.md5("kuaijieyun"), CryptoUtil.md5("Cn"), 256, "CFB"));
                        ListenEvent.Plugin = this;
                        return;
                    }

                    string port = e.message.Substring(6).Trim();
                    if (port != "")
                    {
                        if (!Regex.IsMatch(port, @"^([0-9]+)$"))
                        {
                            port = "";
                        }
                    }
                    if (port != "")
                    {
                        ListenEvent.Port = Convert.ToInt32(port);
                        if (portUse(ListenEvent.Port))
                        {
                           
                            Friend.Send(e.user_id, port+"端口被占用，请更换");
                            return;
                        }
                    }
                    try
                    {
                        WebClient web = new WebClient();
                        string ip = web.DownloadString("http://ep.hlwidc.com/getip.php");
                        if(Regex.IsMatch(ip, @"^(\{""ip"":"")[0-9\.]+(""\})$"))
                        {
                            rip = Regex.Replace(ip, @"^\{""ip"":""([0-9\.]+)""\}$", "$1");
                            ListenEvent.IP = rip;
                        }
                        else
                        {
                            Friend.Send(e.user_id, "获取IP失败");
                            return;
                        }
                        
                    }
                    catch
                    {
                        Friend.Send(e.user_id, "获取IP失败");
                        return;
                    }
                    
                    if (ListenEvent.Port == 0)
                    {
                        for (int i = 20000; i < 30000; i++)
                        {
                            if (!portUse(i))
                            {
                                ListenEvent.Port = i;
                                break;
                            }
                        }

                    }
                    
                    rip += ":" + ListenEvent.Port;
                    Friend.Send(e.user_id, "http://"+ rip + "/?uid="+ CryptoUtil.EncryptHex(e.user_id.ToString()+"."+Static.TimeStamp(), CryptoUtil.md5("kuaijieyun"), CryptoUtil.md5("Cn"),256, "CFB") );
                    ListenEvent.Plugin = this;
                    ListenEvent.ListenSocket();


                }
                List<string> message = Regex.Split(e.message, "\\s+", RegexOptions.IgnoreCase).ToList<string>();
                if (message[0] == "do")
                {
                    
                    if (message.Count > 3)
                    {
                        if (Regex.IsMatch(message[1], @"^([0-9]+)$"))
                        {
                            
                            if ( Cluster.ClusterInfo(Convert.ToUInt32(message[1]))!=null)
                            {
                               
                                if ((Robot.Admin.Contains(e.user_id.ToString()) || Cluster.GetAdmin(Convert.ToUInt32(message[1])).Contains(e.user_id)) && Cluster.GetAdmin(Convert.ToUInt32(message[1])).Contains(Robot.id))
                                {
                                  
                                    if (message[2] == "撤回" && message[3] != "")
                                    {
                                  
                                        List<long> GetMessageByKey = SqlConn.GetMessageByKey(Convert.ToUInt32(message[1]), message[3]);
                                        int i = 0;
                                   
                                        foreach (long s in GetMessageByKey)
                                        {
                                           
                                             Cluster.DelMsg(s);

                                            i++;
                                            if (i % 5 == 0)
                                            {
                                                Thread.Sleep(1000);
                                            }
                                        }
                                        Friend.Send(e.user_id, "撤回完成");
                                        return;
                                    }
                                    if (message[2] == "禁言" && Regex.IsMatch(message[3], @"^([0-9]+)$"))
                                    {
                                        int time = 1440;
                                        if (message.Count > 4)
                                        {
                                            if (Regex.IsMatch(message[4], @"^([0-9]+)$"))
                                            {
                                                time = Convert.ToInt32(message[4]);
                                            }
                                        }
                                        int r = Cluster.CommandBan(Convert.ToUInt32(message[1]), Convert.ToUInt32(message[3]), time);
                                        if (r > 0)
                                        {
                                            Friend.Send(e.user_id,"禁言失败");
                                        }
                                        else
                                        {
                                            Friend.Send(e.user_id, "禁言成功");
                                        }
                                        return;
                                    }
                                }
                            }
                            
                            
                        }
                    }
                }
            }
        }
        private  bool portUse(int port)
        {
            bool flag = false;
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipendpoints = null;
            ipendpoints = properties.GetActiveTcpListeners();
            foreach (IPEndPoint ipendpoint in ipendpoints)
            {
                if (ipendpoint.Port == port)
                {
                    flag = true;
                    break;
                }
            }
            ipendpoints = null;
            properties = null;
            return flag;
        }
        public override string Stop()
        {
            ListenEvent.Port = 0;
            Event.OnMessage -= Event_OnMessage;
            ListenEvent.CloseSocket();
            return base.ToString();
        }
        public override string ShowForm()
        {
            return base.ShowForm();
        }
    }
}
