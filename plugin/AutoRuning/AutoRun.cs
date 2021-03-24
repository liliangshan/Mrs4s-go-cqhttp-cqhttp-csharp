using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using QQRobotFramework;
namespace AutoRuning
{
    public class AutoRun:Plugin
    {
        Dictionary<uint, Thread> thread = new Dictionary<uint, Thread>();
        public AutoRun()
        {
            this.PluginName = "自动化插件";
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
            Event.OnMessage += Event_OnMessage;
            return "success";
        }
        
        private void Event_OnMessage(string sender, RevMessageEvent e)
        {
            
            
            if(e.post_type == "message" && e.message_type == "group" && e.message != null && e.group_id > 0 && e.message.Length>4 && e.message.Substring(0,4)==".自动化" )
            {
                e.Exit = true;
                if (Robot.Admin.Contains(e.user_id.ToString()))
                {
                    string message = e.message.Substring(4).Trim();
                    message = System.Web.HttpUtility.HtmlDecode(message);
                    OnLog(message);
                    if (message == "停止")
                    {
                        if (thread.ContainsKey(e.group_id))
                        {
                            thread.Remove(e.group_id);
                        }
                        Cluster.Send(e.group_id, "停止成功");
                        return;
                    }
                    if (thread.ContainsKey(e.group_id))
                    {
                        Cluster.Send(e.group_id,"当前有任务在执行，请先停止");
                        return;
                    }
                    
                    string time = new Regex(@"时间(?<TIME>\d+)秒").Match(message).Groups["TIME"].Value;
                    int t = 0;
                    if (time != null && time != "")
                    {
                        message = message.Replace("时间" + time + "秒","").Trim();
                        try
                        {
                            t = Convert.ToInt32(time);
                        }
                        catch
                        {

                        }
                    }
                    
                    
                    if (message != "")
                    {
                        Thread threads = new Thread(new ParameterizedThreadStart(AutoSend));
                        thread.Add(e.group_id, threads);
                        Cluster.Send(e.group_id, "启动自动化成功，" + (t>0?(t+ "秒执行一次") : "执行一次"));
                        threads.Start(new object[] { e, message, t });
                        
                    }
                    
                }
            }
        }

        private void AutoSend(object obj)
        {
            object[] o = (object[])obj;
            RevMessageEvent e = (RevMessageEvent)o[0];
            string message = o[1].ToString();
            int time = Convert.ToInt32(o[2]);
         
            if (time > 0)
            {
                while (thread.ContainsKey(e.group_id))
                {
                    Cluster.Send(e.group_id, message);
                    Thread.Sleep(time * 1000);

                }
            }
            else
            {
                if (message == "发送消息")
                {
                    message = "这是测试【" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "】";
                    Cluster.Send(e.group_id, "执行API：http://127.0.0.1:"+Robot.port+ "/send_group_msg?group_id="+e.group_id+ "&message="+Static.UrlEncode(message));
                    Thread.Sleep(100);

                }
                
                Cluster.Send(e.group_id, message);
            }
            
            try
            {
                if (thread.ContainsKey(e.group_id))
                {
                    thread.Remove(e.group_id);
                }
              //  Cluster.Send(e.group_id, "自动化已停止");
                OnLog("自动化已停止");
               
            }
            catch (ThreadAbortException ex)
            {

            }
            catch(Exception ex)
            {

            }
            finally
            {
                
            }
            
        }

        public override string Stop()
        {
            Event.OnMessage -= Event_OnMessage;
            foreach(KeyValuePair<uint,Thread>kv in thread)
            {
                try
                {
                    kv.Value.Abort();
                }
                catch
                {

                }
            }
            thread.Clear();
            return "success";
        }
    }
}
