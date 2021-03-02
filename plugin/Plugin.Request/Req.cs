using System.Collections.Generic;
using System.IO;
using QQRobotFramework;
namespace pluginRequest
{
    public class Req: Plugin
    {
        public string[] Config = new string[] {"false", "false", "false" };
        
        public Req()
        {
            PluginName = "好友申请/加群处理";
          
            if (File.Exists(Robot.path + "plugin.Request.config"))
            {
                Config = File.ReadAllLines(Robot.path + "plugin.Request.config");
            }
        }
        public override string Start()
        {
         
            Event.OnMessage += new Event.RevMessage(Event_OnMessage);
            return "success";
        }

        

        public override string Stop()
        {
            Event.OnMessage -= new Event.RevMessage(Event_OnMessage);
          
            return "success";
        }
        public override string ShowForm()
        {
            Set s = new Set(this);
            s.ShowDialog();
            return "success";
        }
        public override string Install()
        {
            
            return "success";
        }
        public override string UnInstall()
        {
            return "success";
        }
        private void Event_OnMessage(string sender, RevMessageEvent e)
        {
           
            
            if (Config[0] == "true")
            {
                if(e.post_type== "request"&&e.request_type== "friend")
                {
                    e.Exit = true;
                    Friend.Add(e.flag.ToString(), "0");
                }
            }
            if (Config[1] == "true")
            {
                if (e.post_type == "request" && e.request_type == "group"&&e.sub_type== "invite")
                {
                    e.Exit = true;
                    Cluster.ClusterInviteMe(e.flag.ToString(), "0");
                }
            }
            if (Config[2] == "true")
            {
                if (e.post_type == "request" && e.request_type == "group" && e.sub_type == "add")
                {
                    e.Exit = true;
                    List<uint> Adm = new List<uint>();
                    if (e.group_id > 0)
                    {
                        Adm = Cluster.GetAdmin(e.group_id);
                    }
                    if ( Adm.Contains(Robot.id))
                    {
                        Cluster.ClusterRequestJoin(e.flag.ToString(), "0");
                    }
                    else
                    {
                        OnLog("没有权限同意群"+Cluster.ClusterInfo(e.group_id).group_name+"的加群请求");
                    }
                    
                }
            }
        }
    }
}
