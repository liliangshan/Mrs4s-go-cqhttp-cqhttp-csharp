using QQRobotFramework;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;

namespace PluginUsage
{
    public class Usage : Plugin
    {
      public Usage()
        {
            this.PluginName = "机器负载";
        }
        public override string Install()
        {
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
        public override string Start()
        {
            Event.OnMessage += Event_OnMessage;
            return "启动成功";
        }
        public override string Stop()
        {
            Event.OnMessage -= Event_OnMessage;
            return base.Stop();
        }
        private void Event_OnMessage(string sender, RevMessageEvent e)
        {
            OnLog(e.post_type+"|"+e.Exit.ToString()+"|"+e.message);
            if(e.post_type=="message" && e.message!=null && e.message.Length>2 && e.message.Substring(0,2) == "冰冰")
            {
                if (Robot.Admin.Contains(e.user_id.ToString()))
                {
                    string message = e.message.Substring(2).Trim();
                    switch (message)
                    {
                        case "资源负载":
                            e.Exit = true;
                            if (e.group_id > 0)
                            {
                                Cluster.Send(e.group_id, "[CQ:at,qq="+e.user_id+"]正在统计，请稍候");
                            }
                            else
                            {
                                Friend.Send(e.user_id, "[CQ:at,qq=" + e.user_id + "]正在统计，请稍候");
                            }
                            UsageGet(e);
                            break;
                    }
                }
            }
        }

        private void UsageGet(RevMessageEvent e)
        {
            string Cpu = "";
            List<string> SendData = new List<string>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select PercentProcessorTime from Win32_PerfFormattedData_PerfOS_Processor WHERE Name=\"_Total\"");
            var cpuItem = searcher.Get().Cast<ManagementObject>().Select(item => new { PercentProcessorTime = item["PercentProcessorTime"] }).First();
            if (cpuItem != null && cpuItem.PercentProcessorTime != null)
            {
                Cpu = cpuItem.PercentProcessorTime.ToString();
            }
            if (Cpu != "") SendData.Add("CPU使用率：" + Cpu + "%");
            string MemoryInfo = "";
            int MbDiv = 1024 * 1024;
            long availablebytes = 0;
            long PhysicalMemory = 0;
            var managementClassOs = new ManagementClass("Win32_OperatingSystem");
            foreach (var managementBaseObject in managementClassOs.GetInstances())
            {
                if (managementBaseObject["FreePhysicalMemory"] != null)
                {
                    availablebytes = 1024*long.Parse(managementBaseObject["FreePhysicalMemory"].ToString()) / MbDiv;
                    break;
                }
            }

            var managementClass = new ManagementClass("Win32_ComputerSystem");
            var managementObjectCollection = managementClass.GetInstances();
            foreach (var managementBaseObject in managementObjectCollection)
            {
                if (managementBaseObject["TotalPhysicalMemory"] != null)
                {
                    PhysicalMemory = long.Parse(managementBaseObject["TotalPhysicalMemory"].ToString()) / MbDiv;
                }

            }
            if (PhysicalMemory > 0)
            {
                MemoryInfo += "总内存" + PhysicalMemory + "M ";
            }
            if (availablebytes > 0)
            {
                MemoryInfo += "空闲内存" + availablebytes + "M ";
            }
            if(availablebytes>0&& PhysicalMemory > 0)
            {
                MemoryInfo += "内存使用率" + decimal.Round((PhysicalMemory- availablebytes) *100/ PhysicalMemory) + "%";
            }
            if (MemoryInfo != "") SendData.Add(MemoryInfo);
            int KbDiv = 1024;

            try
            {
                decimal BotMemoryInfo = Robot.process.WorkingSet64;
                if (BotMemoryInfo < 1024*1024)
                {
                    SendData.Add("Bot使用内存：" + decimal.Round((decimal)BotMemoryInfo / KbDiv,2).ToString() + "K");
                }
                else
                {
                    SendData.Add("Bot使用内存：" + decimal.Round((decimal)BotMemoryInfo / MbDiv,2).ToString() + "M");
                }
                
            }
            catch
            {

            }
            try
            {
                decimal UIMemoryInfo = Process.GetCurrentProcess().WorkingSet64;
                if (UIMemoryInfo < 1024 * 1024)
                {
                    SendData.Add("框架使用内存：" + decimal.Round((decimal)UIMemoryInfo / KbDiv,2).ToString() + "K");
                }
                else
                {
                    SendData.Add("框架使用内存：" + decimal.Round((decimal)UIMemoryInfo / MbDiv,2).ToString() + "M");
                }
                
            }
            catch
            {

            }

            DriveInfo[] allDirves = DriveInfo.GetDrives();
         
            int GbDiv = 1024 * 1024 * 1024;
            foreach (DriveInfo item in allDirves)
            {

                if (item.IsReady)
                {
                    SendData.Add(item.Name + "=>总空间：" + decimal.Round(item.TotalSize / GbDiv) + "G 可用空间：" + decimal.Round(item.AvailableFreeSpace / GbDiv) + "G 剩余百分比：" + decimal.Round(item.AvailableFreeSpace * 100 / item.TotalSize) + "%");

                }


            }

            
            if (e.group_id > 0)
            {
                Cluster.Send(e.group_id, string.Join("\n", SendData.ToArray()));
            }
            else
            {
                Friend.Send(e.user_id, string.Join("\n", SendData.ToArray()));
            }
        }

        
    }
}
