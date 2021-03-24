using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using QQRobotFramework;
namespace PluginJokerNetease
{
    public class JokerNetease:Plugin
    {
        IDbConnection conn = null;
        Thread t1 = null;
        Thread t2 = null;
        List<uint> GroupData = new List<uint>();
        public JokerNetease()
        {
            this.PluginName = "163搞笑视频";
        }
        public override string Install()
        {
            if (!Directory.Exists(Robot.path + @"JokerNetease\"))
            {
                Directory.CreateDirectory(Robot.path + @"JokerNetease\");
            }
           
            DbBase db = new DbBase(@"Data Source=" + Robot.path + @"JokerNetease\db.db", DBType.SQLite);
            conn = db.Conn;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            IDbCommand command = conn.CreateCommand();
            command.CommandText = "SELECT * FROM sqlite_master where type='table' and name='JokerList'";
            IDataReader reader = command.ExecuteReader();
            if (!reader.Read())
            {
                reader.Close();
                command.CommandText = "CREATE TABLE JokerList (joker TEXT(30) NOT NULL,PRIMARY KEY (\"joker\"),CONSTRAINT \"joker\" UNIQUE (\"joker\"));";
                command.ExecuteNonQuery();
            }
            else
            {
                reader.Close();
            }
           
            command.CommandText = "SELECT * FROM sqlite_master where type='table' and name='JokerUrl'";
            reader = command.ExecuteReader();
            if (!reader.Read())
            {
                reader.Close();
                command.CommandText = "CREATE TABLE JokerUrl (uri TEXT(1000) NOT NULL,PRIMARY KEY (\"uri\"),CONSTRAINT \"uri\" UNIQUE (\"uri\"));";
                command.ExecuteNonQuery();
            }
            else
            {
                reader.Close();
            }
            command.CommandText = "SELECT * FROM sqlite_master where type='table' and name='GroupJoker'";
            reader = command.ExecuteReader();
            if (!reader.Read())
            {
                reader.Close();
                command.CommandText = "CREATE TABLE GroupJoker (gid INTEGER(20) NOT NULL,t INTEGER(50) NOT NULL DEFAULT 0,PRIMARY KEY (\"gid\"),CONSTRAINT \"gid\" UNIQUE (\"gid\"));";
                command.ExecuteNonQuery();
            }
            else
            {
                reader.Close();
            }
            command.CommandText = "SELECT * FROM sqlite_master where type='table' and name='GroupData'";
            reader = command.ExecuteReader();
            if (!reader.Read())
            {
                reader.Close();
                command.CommandText = "CREATE TABLE GroupData (gid INTEGER(20) NOT NULL,PRIMARY KEY (\"gid\"),CONSTRAINT \"gid\" UNIQUE (\"gid\"));";
                command.ExecuteNonQuery();
            }
            else
            {
                reader.Close();
            }

            return "success";
        }

        internal void JokerUrlSave(List<string> v)
        {
            
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            IDbCommand command = conn.CreateCommand();
            command.CommandText = "DELETE FROM JokerUrl";
            command.ExecuteNonQuery();
            command.CommandText = "REPLACE INTO JokerUrl(uri) VALUES " + string.Join(",", v.ToArray());
            command.ExecuteNonQuery();
            t2 = new Thread(new ThreadStart(DownUri));
            t2.Start();
        }
        public override string ShowForm()
        {
            Set set = new Set(this);
            set.ShowDialog();
            return "success";
        }
        public override string Start()
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            GroupData.Clear();
            IDbCommand command = conn.CreateCommand();
            command.CommandText = "SELECT * FROM GroupData";
            IDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                GroupData.Add(Convert.ToUInt32(reader["gid"]));
            }
            reader.Close();
            Event.OnMessage += Event_OnFriend;
            t1 = new Thread(new ThreadStart(DownLoad));
            t1.Start();
            

            return "successV2";
        }
        private int AddGroup(uint gid)
        {
            IDbCommand command = conn.CreateCommand();
            command.CommandText = "REPLACE INTO GroupData (gid) VALUES ("+ gid + ")";
            int index = command.ExecuteNonQuery();
            if(index>0 && !GroupData.Contains(gid))
            {
                GroupData.Add(gid);
            }
            return index;
        }
        private int DeleteGroup(uint gid)
        {
            IDbCommand command = conn.CreateCommand();
            command.CommandText = "DELETE FROM GroupData WHERE gid=" + gid + "";
            int index = command.ExecuteNonQuery();
            if (index > 0 && GroupData.Contains(gid))
            {
                GroupData.Remove(gid);
            }
            return index;
        }
        public List<string> JokerUrl()
        {
            List<string> uri = new List<string>();
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            IDbCommand command = conn.CreateCommand();
            command.CommandText = "SELECT * FROM JokerUrl";
            IDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                uri.Add(reader["uri"].ToString());
            }
            reader.Close();
            return uri;
        }
        private void GetVideo(object obj)
        {
            RevMessageEvent e = (RevMessageEvent)obj;
            string fname = "";
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                IDbCommand command = conn.CreateCommand();
                if (e.message!=null && (e.message.IndexOf("毕加猪") > -1 || e.message.IndexOf("bjz") > -1))
                {
                    command.CommandText = "SELECT * FROM JokerList WHERE joker LIKE 'QVideo-%' ORDER BY RANDOM() limit 1";
                }
                else
                {
                    command.CommandText = "SELECT * FROM JokerList  ORDER BY RANDOM() limit 1";
                }

                string id = "";
                IDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    id = reader["joker"].ToString();
                }
                reader.Close();
                string DownUrl = "";
                if (id != "")
                {
                    OnLog("获取ID：" + id);
                    if (!File.Exists(Robot.path + @"JokerNetease\" + id + ".mp4"))
                    {
                        WebClient web = new WebClient();
                        web.Encoding = Encoding.UTF8;
                        
                        if (id.Length > 7 && id.Substring(0, 7) == "QVideo-")
                        {
                            string vid = id.Replace("QVideo-", "");
                            string VideoInfp = web.DownloadString("https://h5wx.video.qq.com/getinfo?callback=j&platform=11001&charge=0&otype=json&ehost=https%3A%2F%2Fview.inews.qq.com&sphls=0&sb=1&nocache=0&_rnd=1613111258363&guid=1613111258363&appVer=V2.0Build9496&vids=" + vid + "&defaultfmt=auto&&_qv_rmt=VEZfwXxMA12593l4u=&_qv_rmt2=48nK/4IM1535169lQ=&sdtfrom=v3110&_=1613111256247&jsonpCallback=j");
                            VideoInfp = VideoInfp.Substring(2);
                            VideoInfp = VideoInfp.Substring(0, VideoInfp.Length - 1);
                            VideoInfo v = JsonHelper.DeserializeObject<VideoInfo>(VideoInfp);
                            if (v.msg == null)
                            {

                               
                                vi[] vData = v.vl.vi;
                                foreach (vi vv in vData)
                                {
                                    string fvkey = vv.fvkey;
                                    int br = vv.br;
                                    string ti = vv.ti;
                                    string DownUi = vv.ul.ui[0].url;
                                    DownUrl = DownUi + vv.fn + "?vkey=" + fvkey + "&br=" + br + "&platform=2&fmt=auto&level=0&sdtfrom=v3110&guid=1613111258363";

                                }

                            }
                            else
                            {
                                OnLog("获取地址失败："+ VideoInfp);
                                return;
                            }

                        }
                        else
                        {
                            web.Headers.Add("referer: https://c.m.163.com/news/v/" + id + ".html?spss=newsapp");
                            string data = web.DownloadString("https://gw.m.163.com/nc-gateway/api/v1/video/detail/" + id);
                            DownUrl = new Regex(@"""mp4Hd_url""\:""(?<URI>.+?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Match(data).Groups["URI"].ToString();
                            if (DownUrl == "")
                            {
                                DownUrl = new Regex(@"""mp4_url""\:""(?<URI>.+?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline).Match(data).Groups["URI"].ToString();
                            }
                        }






                        if (DownUrl != "")
                        {
                            if (e.message_type == "private")
                            {
                                Friend.Send(e.user_id, "获取地址成功，正在下载视频");
                            }
                            OnLog("获取地址成功：" + DownUrl);
                            DownLoad down = new DownLoad(DownUrl, Robot.path + @"JokerNetease\" + id + ".mp4");
                            if (down.errorCode == 0)
                            {
                                if (e.message_type == "private")
                                {
                                    Friend.Send(e.user_id, "下载文件成功,用时：" + down.Time + "秒，等待发送");
                                }
                                OnLog("下载文件成功,用时：" + down.Time + "秒");
                                fname = Robot.path + @"JokerNetease\" + id + ".mp4";
                            }
                            else
                            {
                                if (e.message_type == "private")
                                {
                                    Friend.Send(e.user_id, "下载文件失败：" + down.errorMessage);
                                }
                                OnLog("下载文件失败：" + down.errorMessage);
                            }

                        }
                        else
                        {
                            OnLog("获取地址失败：" + DownUrl);
                        }


                    }
                    else
                    {
                        if (e.message_type == "private")
                        {
                            Friend.Send(e.user_id, "获取文件成功，等待发送");
                        }
                        OnLog("获取文件成功，等待发送");
                        fname = Robot.path + @"JokerNetease\" + id + ".mp4";
                    }
                }
                else
                {
                    OnLog("获取ID："+ command.CommandText);
                }
            }
            catch(Exception ex)
            {
                OnLog(ex.Message);
            }


            string Video = fname;
            if (Video != "")
            {
                if (e.message_type == "private")
                {
                    Friend.Send(e.user_id, "[CQ:video,file=file:///" + Video + "]");
                }
                if (e.message_type == "group" && e.group_id > 0)
                {
                    Cluster.Send(e.group_id, "[CQ:video,file=file:///" + Video + "]");
                }

            }
            else
            {
                OnLog("获取视频失败");
            }
        }
        private void Event_OnFriend(string sender, RevMessageEvent e)
        {
            
            try
            {
                if (e.post_type== "message")
                {
                    if(e.message != null && e.group_id > 0)
                    {
                        if ( (e.message == "开启" + this.PluginName || e.message == "关闭" + this.PluginName) &&  (Robot.Admin.Contains(e.user_id.ToString())||(e.sender!=null&&e.sender.role!=null&&e.sender.role== "owner")) )
                        {
                            OnLog(e.message);
                            e.Exit = true;
                            int index = 0;
                            if (e.message == "开启" + this.PluginName)
                            {
                                index = AddGroup(e.group_id);
                            }
                            if (e.message == "关闭" + this.PluginName)
                            {
                                index = DeleteGroup(e.group_id);
                            }
                            if (index == 0)
                            {
                                Cluster.Send(e.group_id, e.message + "失败");
                            }
                            else
                            {
                                Cluster.Send(e.group_id, e.message + "成功");
                            }
                            return;
                        }
                        if (!GroupData.Contains(e.group_id))
                        {
                            return;
                        }
                    }

                    if (e.message != null && ((e.message.IndexOf("车") > -1 && e.message.IndexOf("来") > -1)|| (e.message.IndexOf("搞") > -1 && e.message.IndexOf("笑") > -1) || e.message.IndexOf("毕加猪") > -1 || e.message.IndexOf("bjz") > -1 || e.message.IndexOf("setu") > -1) )
                    {
                        e.Exit = true;
                        if (e.message_type == "group" && e.group_id > 0)
                        {
                            if (conn.State != ConnectionState.Open)
                            {
                                conn.Open();
                            }
                            IDbCommand command = conn.CreateCommand();
                            command.CommandText = "SELECT * FROM GroupJoker WHERE gid="+ e.group_id;
                          
                            string id = "";
                            IDataReader reader = command.ExecuteReader();
                            long LastTime = 0;
                            if (reader.Read())
                            {
                                LastTime=Convert.ToInt64(reader["t"]);
                            }
                            reader.Close();
                            long NowTime = Static.TimeStamp();
                            if(NowTime- LastTime < 30)
                            {
                                OnLog("时间不足30秒，不发送");
                                return;
                            }
                            command = conn.CreateCommand();
                            command.CommandText = "REPLACE INTO GroupJoker (gid,t) VALUES (@gid,@t)";
                            DbParameter dbParameter = new DbParameter(command);
                            command.Parameters.Add(dbParameter.Parameter("@gid", e.group_id));
                            command.Parameters.Add(dbParameter.Parameter("@t", NowTime));
                            command.ExecuteNonQuery();
                        }
                        OnLog("正在获取视频");
                        Thread thread = new Thread(new ParameterizedThreadStart(GetVideo));
                        thread.Start(e);
                        /*string Video = GetVideo(e);
                        if (Video != "")
                        {
                            if(e.message_type == "private")
                            {
                                Friend.Send(e.user_id, "[CQ:video,file=file:///" + Video + "]");
                            }
                            if (e.message_type == "group" && e.group_id>0)
                            {
                                Cluster.Send(e.group_id, "[CQ:video,file=file:///" + Video + "]");
                            }

                        }
                        else
                        {
                            OnLog("获取视频失败");
                        }*/

                    }
                    
                     
                }
            }
            catch(Exception ex)
            {
                OnLog(ex.Message);
            }
            
        }

        public void DownUri()
        {
            List<string> uriList = JokerUrl();
            foreach (string uri in uriList)
            {
                try
                {

                    WebClient web = new WebClient();
                    web.Encoding = Encoding.UTF8;
                    string data = web.DownloadString(uri);

                    Match m = new Regex(@"<a href=""\/\/c\.m\.163\.com\/news\/v\/(?<ID>[a-zA-Z0-9]+)\.html\?from=subscribe&spss=newsapp"">$", RegexOptions.IgnoreCase | RegexOptions.Multiline).Match(data);
                    List<string> v = new List<string>();
                    while (m.Success)
                    {
                        v.Add("('" + m.Groups["ID"].ToString() + "')");

                        m = m.NextMatch();
                    }
                    IDbCommand command = conn.CreateCommand();

                    command.CommandText = "REPLACE INTO JokerList(joker) VALUES " + string.Join(",", v.ToArray());
                    command.ExecuteNonQuery();
                    OnLog("地址解析成功：" + uri + "");
                }
                catch(Exception e)
                {
                    OnLog("地址解析错误："+uri+"|原因："+e.Message);
                }
                Thread.Sleep(2000);
            }
            
        }

        private void DownLoad()
        {
            while (true)
            {
                try
                {
                    DownUri();
                }
                catch
                {

                }
                Thread.Sleep(86400000);
            }
            
        }

        public override string Stop()
        {
            if (conn != null) conn.Close();
            Event.OnMessage -= Event_OnFriend;
            if (t1 != null)
            {
                try
                {
                    t1.Abort();
                }
                catch
                {

                }
            }
            if (t2 != null)
            {
                try
                {
                    t2.Abort();
                }
                catch
                {

                }
            }


            return "success";
        }
        public override string UnInstall()
        {
            return "success";
        }
        
    }
    public class VideoInfo
    {
        public object msg { get; set; }
        public vl vl { get; set; }
    }

    public class vl
    {
        public vi[] vi { get; set; }
    }

    public class vi
    {
        public string fvkey { get; set; }
        public string ti { get; set; }
        public ul ul { get; set; }
        public int br { get; set; }
        public string fn { get; set; }
    }

    public class ul
    {
        public ui[] ui { get; set; }
    }

    public class ui
    {
        public string url { get; set; }

    }
}
