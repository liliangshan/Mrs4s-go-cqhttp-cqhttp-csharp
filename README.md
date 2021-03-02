# Mrs4s-go-cqhttp-cqhttp-csharp
# Bot信息：
        /**Bot号**/
        public static uint id { get; set; }
        /**Bot密码**/
        public static string password { get; set; }
        /**Bot昵称**/
        public static string name { get; set; }
        /**API端口**/
        public static int port { get; set; }
        /**WebSocket端口**/
        public static int ws { get; set; }
        /**Bot进程**/
        public static Process process { get; set; }
        /**Bot状态**/
        public static string status { get; set; }
        /**API监听端口**/
        public static string ApiPort { get; set; }
        /**API监听密钥**/
        public static string ApiKey { get; set; }
        /**API通信地址**/
        public static string Api { get; set; }
        /**Bot目录**/
        public static string path { get; set; }
        /**到期时间**/
        public static string EndTime { get; set; }
        /**主人列表**/
        public static List<string> Admin { get; set; }
        /**WebUI端口**/
        public static int WebUi { get; set; }


# 事件：
        Event.OnMessage(string sender, RevMessageEvent e)  接收信息
        RevMessageEvent
        public class RevMessageEvent
        {
        public uint target_id;
        public string honor_type { get; set; }
        public object comment { get; set; }
        public GroupFileInfo file { get; set; }
        public object flag { get; set; }
        public string request_type { get; set; }
        public object operator_id { get; set; }
        public object duration { get; set; }

        public string notice_type { get; set; }

        public RevMessageEvent() {
            Exit = false;
        }
        public long time { get; set; }
        public long self_id { get; set; }
        public string meta_event_type { get; set; }
        public string post_type { get; set; }
        public string message_type { get; set; }
        public string sub_type { get; set; }
        public long message_id { get; set; }
        public uint group_id { get; set; }
        public uint user_id { get; set; }

        public string message { get; set; }

        public string raw_message { get; set; }
        public anonymous anonymous { get; set; }

        public GroupUserInfo sender { get; set; }
        public bool Exit { get; set; }
    }
    public class anonymous
    {
        public anonymous() { }
        public long id { get; set; }
        public string name { get; set; }
        public string flag { get; set; }

    }
    public class GroupUserInfo
    {

        public uint group_id { get; set; }
        public uint user_id { get; set; }
        public string nickname { get; set; }
        public string card { get; set; }
        public string sex { get; set; }
        public int age { get; set; }
        public string area { get; set; }
        public int join_time { get; set; }
        public int last_sent_time { get; set; }
        public string level { get; set; }
        public string role { get; set; }
        public bool unfriendly { get; set; }
        public string title { get; set; }
        public long title_expire_time { get; set; }
        public bool card_changeable { get; set; }

    }
    
# 支持API：
    群组：Cluster
    发消息 long  Cluster.Send(uint 群组ID,string 内容)
    群列表 GroupInfo[] Cluster.GroupInfo[] Get(bool 是否强制刷新)
    群信息 GroupInfo Cluster.ClusterInfo(uint 群组ID)
    群管理员列表 List<uint> Cluster.GetAdmin(uint 群组ID, bool 是否强制刷新)
    群成员列表 GroupUserInfo[] Cluster.GetGroupUser(uint 群组ID)
    OCR图片识字 string Cluster.Ocr(string 图片ID)
    二维码识别  string Cluster.QrCode(string 图片路径)
    消息撤回 void Cluster.DelMsg(long 消息ID)
    禁言/解除禁言 Cluster.CommandBan(uint 群组ID, uint 成员ID, int 时间分钟，0为解除禁言)
    踢出成员 int Cluster.ClusterKickMember(uint 群组ID, uint 成员ID)
    邀请我入群处理 int Cluster.ClusterInviteMe(string 邀请flag标志, string 结果：0为同意，其他拒绝)
    成员加群处理 int Cluster.ClusterRequestJoin(string 邀请flag标志, string 结果：0为同意，其他拒绝)
    
# 好友：Friend
    好友消息 Friend.Send(uint 好友ID, string 内容);
    好友列表 Friend.FriendInfo[] Get(bool reload = 是否强制刷新)
    好友信息 Friend.FriendInfo FriendById(uint 好友ID)
    好友申请处理 int Friend.Add(string 邀请flag标志, string 结果：0为同意，其他拒绝)
    
# 数据库：DbBase
    连接信息 IDbConnection Conn
    创建连接 DbBase(string Sqlite字符串)
    创建连接 DbBase(string 字符串, DBType 数据库类型枚举)
    数据库类型枚举
    public enum DBType
    {
        SQLite=0,
        MySQL=1,
        SQLServer=2
    }
    DbParameter 创建 DbParameter(IDbCommand cmd)
    DbParameter生成 IDbDataParameter Parameter(string 名称, object 值)
    
    
# 公共：Static
    应用路径 string Static.Path
    应用图标 System.Drawing.Icon Static.icon
    提交数据 string Static.Post(string 网址, string 数据)
    时间缀转时间  string Static.GetDateTime(long 时间缀)
    随机数字 int Static.RandomNum(int 起始数字, int 结束数字)
    地址解码 string Static.UrlDecode(string 字符串)
    地址转码 string Static.UrlEncode(string 字符串)
    当前时间缀 long Static.TimeStamp()
    
# 视频处理：ffmpeg
        /**剪辑时长**/
        public int len { get;set;}
        /**水印文字**/
        public string title { get; set; }
        /**水印图片**/
        public byte[] images { get; set; }
        /**水印文字是否滚动**/
        public bool RunText { get; set; }
        
      视频处理(返回处理后的路径)  string Start(string 视频路径)

# JSON处理：JsonHelper
        JSON字符串转JSON对象 T JsonHelper.JsonHelper<T>(string json字符串)
        JSON转JSON字符串 string JsonHelper.SerializeObject(object JSON对象)     
