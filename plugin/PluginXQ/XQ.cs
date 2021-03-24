using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using QQRobotFramework;


namespace PluginXQ
{
    public class XQ:QQRobotFramework.Plugin
    {
        private IDbConnection conn { get; set; }
        public Dictionary<string, string> Type = new Dictionary<string, string>();
        public List<string> UseType = new List<string>();
        public Dictionary<string, List<uint>> GroupTags = new Dictionary<string, List<uint>>();
        Thread SendVideoThread = null;
        int DownLoadIndex = 0;
        public XQ(){
            this.PluginName = "戏曲接口";
            Type.Add("豫剧", "yuju");
            Type.Add("黄梅戏", "huangmeixi");
            Type.Add("梆子戏", "bangzixi");
            Type.Add("川剧", "chuanju");
            Type.Add("坠子戏", "zhuizixi");
            Type.Add("越剧", "yueju");
            Type.Add("民间小调", "minjianxiaodiao");
            Type.Add("曲剧", "quju");
            Type.Add("云贵山歌", "yunguishange");
            LoadTag();
        }
        public override string Install()
        {
            OnLog("aaaaa");
            Friend.Send(88172719,"test");
           

            

            return "";
        }
        private void connet()
        {
            if (conn == null)
            {
                if (!File.Exists(Robot.path + "group.xq.db"))
                {
                    byte[] bySave = Resource1.data;
                    FileStream fsObj = new FileStream(Robot.path + "group.xq.db", FileMode.CreateNew);
                    fsObj.Write(bySave, 0, bySave.Length);
                    fsObj.Close();
                }
                DbBase db = new DbBase(@"Data Source=" + Robot.path + "group.xq.db");
                conn = db.Conn;
            }
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
        }
        public  Dictionary<uint, string> tag()
        {
            connet();
            Dictionary<uint, string> pairs = new Dictionary<uint, string>();
            IDbCommand comm = conn.CreateCommand();
            comm.CommandText = "SELECT * FROM `grouptag`";
            IDataReader reader = comm.ExecuteReader();
            while (reader.Read())
            {
                pairs.Add(Convert.ToUInt32(reader["gid"]), reader["tag"].ToString());
            }
            reader.Close();

            return pairs;
        }
        public long save(object gid, string tag)
        {
           
            connet();
            IDbCommand comm = conn.CreateCommand();
            comm.CommandText = "REPLACE INTO `grouptag`(`rid`,`gid`,`tag`) VALUES (@rid,@gid,@tag);select last_insert_rowid();";
            DbParameter parameter = new DbParameter(comm);


            comm.Parameters.Add(parameter.Parameter("@rid", 0));
            comm.Parameters.Add(parameter.Parameter("@gid", gid));
            comm.Parameters.Add(parameter.Parameter("@tag", tag));

            object id = comm.ExecuteScalar();

            if (id == null)
            {
                return 0;
            }

            LoadTag();
           

            return Convert.ToInt64(id);
        }
        public override string UnInstall()
        {
            
                return "success";
        }
        public override string Stop()
        {
            if (SendVideoThread != null)
            {
                try
                {
                    SendVideoThread.Abort();
                }
                catch
                {

                }
                

            }
            UseType.Clear();
            OnLog("已停止");
            return "success";
        }
        public void SaveTag(uint gid,string v)
        {
            if (Type.ContainsKey(v))
            {
                if (!GroupTags.ContainsKey(Type[v]))
                {
                    GroupTags.Add(Type[v], new List<uint>());
                    UseType.Add(Type[v]);
                }
                if (GroupTags[Type[v]].IndexOf(gid) == -1)
                {
                    GroupTags[Type[v]].Add(gid);
                }
            }
        }
        private void LoadTag()
        {
            UseType.Clear();
            GroupTags.Clear();
            Dictionary<uint, string> tags = tag();
            foreach (KeyValuePair<uint, string> kv in tags)
            {
                if (kv.Value != "")
                {
                    string[] t = kv.Value.Split('|');
                    foreach (string v in t)
                    {
                        SaveTag(kv.Key, v);
                    }

                }

            }
        }
        public override string Start()
        {

            LoadTag();

            SendVideoThread = new Thread(new ThreadStart(SendVideo));
            SendVideoThread.Start();
            return "success";
        }

        private void SendVideo()
        {
            while (true)
            {
                if (UseType.Count > 0)
                {
                    if (DownLoadIndex >= UseType.Count)
                    {
                        DownLoadIndex = 0;
                    }
                    string typeData = UseType[DownLoadIndex];
                    DownLoadIndex++;
                    DownLoad(typeData);
                }
                
                Thread.Sleep(3600000);
            }
        }

        private void DownLoad(string typeData)
        {
            if (UseType.Count == 0)
            {
                return;
            }
            

            
            OnLog(typeData+":"+ DownLoadIndex);

            string connectionString = "Database=xiqu;Data Source=sh-cdb-oq3ldfii.sql.tencentcdb.com;Port=61697;UserId=xiqu;Password=@lls19830803;Charset=utf8;TreatTinyAsBoolean=false;Allow User Variables=True";
            DbBase db = new DbBase(connectionString,DBType.MySQL);

            IDbConnection Mysql = db.Conn;
            Mysql.Open();

            
            IDbCommand command = Mysql.CreateCommand();
            command.CommandText = "SELECT * FROM `hlwidc_video` WHERE `isall`=2 AND `server`=2 AND `type`='"+ typeData + "' ORDER BY RAND() LIMIT 1";
            IDataReader reader = command.ExecuteReader();
            string filename = "";
            string VideoName = "";
            while (reader.Read())
            {
                VideoName = reader["title"].ToString();
                try
                {
                    object url = reader["video_path"];
                    if (url != null)
                    {
                        string Uri = url.ToString().Substring(1);
                        Uri = Uri.Substring(0, Uri.LastIndexOf("."));
                        string VideoUrl = "https://h5vv6.video.qq.com/getinfo?callback=j&platform=11001&charge=0&otype=json&ehost=https%3A%2F%2Fview.inews.qq.com&sphls=0&sb=1&nocache=0&_rnd=1613111258363&guid=1613111258363&appVer=V2.0Build9496&vids=" + Uri + "&defaultfmt=auto&&_qv_rmt=VEZfwXxMA12593l4u=&_qv_rmt2=48nK/4IM1535169lQ=&sdtfrom=v3110&_=1613111256247&jsonpCallback=j";
                        WebClient web = new WebClient();
                        web.Encoding = Encoding.UTF8;
                        string VideoInfp = web.DownloadString(VideoUrl);
                        VideoInfp = VideoInfp.Substring(2);
                        VideoInfp = VideoInfp.Substring(0, VideoInfp.Length - 1);
                        Console.WriteLine(VideoInfp);
                        VideoInfo v = JsonHelper.DeserializeObject<VideoInfo>(VideoInfp);

                        if (v.msg == null)
                        {
                            
                            OnLog(VideoName + "获取地址成功");
                            vi[] vData = v.vl.vi;
                            foreach (vi vv in vData)
                            {
                                string fvkey = vv.fvkey;
                                int br = vv.br;
                                string ti = vv.ti;
                                string DownUi = vv.ul.ui[0].url;
                                string DownUrl = DownUi + vv.fn + "?vkey=" + fvkey + "&br=" + br + "&platform=2&fmt=auto&level=0&sdtfrom=v3110&guid=1613111258363";
                                Console.WriteLine(DownUrl);
                                DateTime start = DateTime.Now;
                                Uri uri = new Uri(DownUrl);
                                filename = DateTime.Now.ToString("yyyyMMddHHmmss") + Static.RandomNum(10000, 99999) + ".mp4";
                                if (!Directory.Exists(Robot.path + @"\tmp\"))
                                {
                                    Directory.CreateDirectory(Robot.path + @"\tmp\");
                                }
                                filename = Robot.path + @"\tmp\" + filename;

                                //指定url 下载文件
                                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(DownUrl);
                                Stream stream = request.GetResponse().GetResponseStream();
                                //创建写入流
                                FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                                byte[] bytes = new byte[1024 * 1024];
                                int readCount = 0;
                                while (true)
                                {
                                    readCount = stream.Read(bytes, 0, bytes.Length);
                                    if (readCount <= 0)
                                        break;
                                    fs.Write(bytes, 0, readCount);
                                    fs.Flush();
                                }
                                fs.Close();
                                stream.Close();
                                OnLog("下载文件成功,用时：" + (DateTime.Now - start).TotalSeconds + "秒" + filename);
                                break;
                            }

                        }
                        else
                        {
                            OnLog(reader["title"].ToString() + "获取地址失败");
                        }


                    }
                }
                catch
                {
                    filename = "";
                }
                
            }
            reader.Close();

            Mysql.Close();
            if (filename == "")
            {
                DownLoad(typeData);
            }
            else
            {
                ffmpeg mpeg = new ffmpeg();
                mpeg.title = VideoName;
                mpeg.len = 30;
                mpeg.RunText = false;
                string file= mpeg.Start(filename);
                File.Delete(filename);
                OnLog("正在推送[CQ:video,file=file:///" + file + "]");
                foreach (uint gid in GroupTags[typeData])
                {
                   long SendResult= Cluster.Send(gid, "[CQ:video,file=file:///" + file + "]");
                   OnLog( "["+ gid + "]" +(SendResult!=0?"成功":"失败") );
                    
                }
                

            }
        }

      

        public override string ShowForm()
        {
            Set set = new Set(this);
            set.ShowDialog();
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
