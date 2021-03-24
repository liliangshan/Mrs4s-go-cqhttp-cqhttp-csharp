using QQRobotFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace QQPlugiPoke
{
    public class poke:Plugin
    {
        public uint[] Config = new uint[] { 0, 0,60000 };
        //Thread thread;
        Dictionary<uint, Dictionary<uint, Thread>> thread =new Dictionary<uint, Dictionary<uint, Thread>>();
        bool Run = true;
        public poke()
        {
            this.PluginName = "定时戳一戳";
            if (File.Exists(Robot.path + @"poke.config"))
            {
                string[] v = File.ReadAllLines(Robot.path + @"poke.config");
                Config[0] = Convert.ToUInt32(v[0]);
                Config[1] = Convert.ToUInt32(v[1]);
            }
        }
        public override string ShowForm()
        {
            Set set = new Set(this);
            set.ShowDialog();
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
        public override string Start()
        {
            if( Config[0]==0 || Config[1] == 0)
            {
                OnLog("群号群友号不存在");
                return "群号群友号不存在";

            }
            Event.OnMessage += Event_OnMessage;
            // thread = new Thread(new ThreadStart(pk));
           // thread.Start();
            return "success";
        }

        private void Event_OnMessage(string sender, RevMessageEvent e)
        {
            if (e.post_type == "message" && e.message_type == "group" && e.group_id > 0 && e.message != null && ( (e.message.Length>4 && e.message.Substring(0,2) == "戳戳") || e.message == "停止戳戳" || e.message == "戳戳"))
            {
                if (Robot.Admin.Contains(e.user_id.ToString()))
                {
                    string message = e.message;
                    if (message.Length > 4 && message.Substring(0, 2) == "戳戳" && message.IndexOf("CQ")>-1 && message.IndexOf("时间") > -1)
                    {
                        message = "开启戳戳";
                        string uid = new Regex(@"\[CQ\:at,qq=(?<UID>\d+)\]",RegexOptions.IgnoreCase).Match(e.message).Groups["UID"].Value;
                        uint id = 0;
                        
                        try {
                            id= Convert.ToUInt32(uid);
                        } catch
                        {

                        }
                        if (id == 0)
                        {
                            Cluster.Send(e.group_id, "戳戳对象不存在");
                            return;
                        }
                        string time = new Regex(@"时间(?<TIME>\d+)秒", RegexOptions.IgnoreCase).Match(e.message).Groups["TIME"].Value;
                        int t = 0;
                        try
                        {
                            t = Convert.ToInt32(time);
                        }
                        catch
                        {

                        }
                        if (t == 0)
                        {
                            Cluster.Send(e.group_id, "戳戳格式不正确，请输入【戳戳+CQAT对象+时间x秒】");
                            return;
                        }
                        
                        if (!thread.ContainsKey(e.group_id))
                        {
                            thread.Add(e.group_id, new Dictionary<uint, Thread>());
                        }
                        if (thread[e.group_id].ContainsKey(id))
                        {
                            Cluster.Send(e.group_id, "戳戳对象已存在");
                            return;
                        }
                        Thread th = new Thread(new ParameterizedThreadStart(pk));
                        th.Start(t);
                        thread[e.group_id].Add(id, th);
                        Cluster.Send(e.group_id, "已开启对[CQ:at,qq=" + id + "]的戳戳");
                    }
                    else if (message == "停止戳戳")
                    {
                        if (!thread.ContainsKey(e.group_id))
                        {
                            Cluster.Send(e.group_id, "当前群没有开启戳戳");
                            return;
                        }
                        try
                        {
                            List<uint> v = new List<uint>();
                            foreach (KeyValuePair<uint, Thread> kv in thread[e.group_id])
                            {
                                try
                                {
                                    kv.Value.Abort();
                                }
                                catch
                                {

                                }
                                finally
                                {
                                    v.Add(kv.Key);
                                }
                                
                            }
                            foreach (uint kv in v)
                            {
                                thread[e.group_id].Remove(kv);
                            }
                            Cluster.Send(e.group_id, "已停止本群全部戳戳");
                            return;
                        }
                        catch (Exception ex)
                        {
                            Cluster.Send(e.group_id, "停止戳戳失败");
                            return;
                        }
                        finally
                        {
                            if (thread[e.group_id].Count == 0)
                            {
                                thread.Remove(e.group_id);
                            }
                        }
                        
                    }
                    else if (message == "戳戳")
                    {
                        if (!thread.ContainsKey(e.group_id))
                        {
                            Cluster.Send(e.group_id, "当前群没有开启戳戳");
                            return;
                        }
                        List<string> v = new List<string>();
                        foreach (KeyValuePair<uint, Thread> kv in thread[e.group_id])
                        {
                            v.Add("[CQ:at,qq="+kv.Key+"]");
                        }
                        Cluster.Send(e.group_id, "当前群正在戳戳"+string.Join("",v.ToArray()));
                        return;
                    }
                }
            }
        }

        private void pk(object obj)
        {
            int t = Convert.ToInt32(obj);
            while (Run)
            {
                if (Config[0] == 0 || Config[1] == 0)
                {
                    break;

                }
                Cluster.Send(Config[0],"[CQ:poke,qq="+ Config[1] + "]");
                Thread.Sleep( t*1000 );
            }
        }

        public override string Stop()
        {
            Run = false;
            try
            {
                foreach(KeyValuePair<uint,Dictionary<uint,Thread>>kv in thread)
                {
                    foreach(KeyValuePair<uint,Thread>kvv in kv.Value)
                    {
                        try
                        {
                            kvv.Value.Abort();
                        }
                        catch
                        {

                        }
                    }
                    thread[kv.Key].Clear();
                }
                thread.Clear();
                //thread.Abort();
            }
            finally
            {

            }
            Event.OnMessage -= Event_OnMessage;
            return "success";
        }
    }
}
