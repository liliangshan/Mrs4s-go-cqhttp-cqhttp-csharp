using QQRobotFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace PrivateMsgDoPlugin
{
    class ListenEvent
    {
        internal static Thread ListenThread { get; set; }
        internal static Socket Socket { get; set; }
        internal static PrivateMsgDo Plugin { get; set; }
        internal static int Port = 0;
        internal static string IP = "";
        static  bool canStop = false;
        internal static void ListenSocket()
        {
            canStop = false;
            ListenEvent.CloseSocket();
            
            ListenEvent.ListenThread = new Thread(ListenEvent.ListenThreadRun);
            ListenEvent.ListenThread.IsBackground = true;
            ListenEvent.ListenThread.Start();
        }
        internal static void CloseSocket()
        {
            canStop = true;
            if (ListenEvent.Socket != null)
            {
                if (ListenEvent.Socket.Connected)
                {
                    ListenEvent.Socket.Shutdown(SocketShutdown.Both);

                }
                ListenEvent.Socket.Close();
                ListenEvent.Socket = null;
            }
        }
        internal static void ListenThreadRun()
        {

            try
            {
                int port = ListenEvent.Port;
                if (port < 1)
                {
                    ListenEvent.Plugin.OnLog("端口为空不监听");
                    return;
                }
                
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
                ListenEvent.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ListenEvent.Socket.Bind(endPoint);
                ListenEvent.Socket.Listen(100);
                ListenEvent.Plugin.OnLog("正在监听端口：" + port);
                
                canStop = false;
                while (!canStop)
                {
                    

                    Socket mySocket = ListenEvent.Socket.Accept();
                    if (mySocket.Connected)
                    {
                        
                        try
                        {
                            Byte[] recvBuf = new Byte[2048000];
                            int bytesCount = mySocket.Receive(recvBuf, recvBuf.Length, 0);
                            if (bytesCount < 10)
                            {
                                mySocket.Shutdown(SocketShutdown.Both);
                                mySocket.Close();
                                continue;
                            }
                            string sBuffer = Encoding.UTF8.GetString(recvBuf, 0, bytesCount);
                            if (string.IsNullOrEmpty(sBuffer) || sBuffer.Length < 10)
                            {
                                mySocket.Shutdown(SocketShutdown.Both);
                                mySocket.Close();
                                continue;
                            }
                            string statueCode = "200 OK";

                            // 查找 "HTTP" 的位置
                            int iStartPos = sBuffer.IndexOf("HTTP", 1, StringComparison.CurrentCulture);
                            string sHttpVersion = sBuffer.Substring(iStartPos, 8);
                            if (string.Compare(sBuffer.Substring(0, 3), "GET", StringComparison.CurrentCultureIgnoreCase) == 0)
                            {
                                sBuffer = sBuffer.Substring(5, iStartPos - 6);
                                sBuffer = Static.UrlDecode(sBuffer);
                                
                                if (sBuffer != null)
                                {

                                    string sendOut = ListenEvent.ApiSelect(sBuffer.Trim());
                                    if (sendOut.IndexOf("Not Found Page", StringComparison.CurrentCulture) > -1)
                                    {
                                        sendOut = ListenEvent.ApiSelect(sBuffer.Trim());
                                        
                                    }
                                    byte[] bytesMsg = Encoding.UTF8.GetBytes(sendOut);
                                    ListenEvent.SendHeader(mySocket, sHttpVersion, statueCode, "text/html;charset=UTF-8", "bytes", bytesMsg.Length);
                                    ListenEvent.SocketSendMsg(mySocket, bytesMsg);
                                }

                            }
                            else if ((string.Compare(sBuffer.Substring(0, 4), "POST", StringComparison.CurrentCultureIgnoreCase) == 0))
                            {

                                // string PathBuffer = sBuffer.Substring(4);

                                sBuffer = sBuffer.Substring(sBuffer.IndexOf("\r\n\r\n", StringComparison.CurrentCulture) + 4);
                                //  if(  )
                                string sendOut = ListenEvent.ApiSelect(sBuffer.Trim());

                                if (sendOut.IndexOf("Not Found Page", StringComparison.CurrentCulture) > -1)
                                {
                                    sendOut = ListenEvent.ApiSelect(sBuffer.Trim());
                                    if (sendOut.IndexOf("Not Found Page", StringComparison.CurrentCulture) > -1)
                                    {
                                        statueCode = "404 Not Found";
                                    }
                                }
                                byte[] bytesMsg = Encoding.UTF8.GetBytes(sendOut);
                                ListenEvent.SendHeader(mySocket, sHttpVersion, statueCode, "text/html;charset=UTF-8", "bytes", bytesMsg.Length);
                                ListenEvent.SocketSendMsg(mySocket, bytesMsg);
                            }
                            else
                            {
                                string sendOut = ListenEvent.ApiSelect(sBuffer.Trim());
                                byte[] bytesMsg = Encoding.UTF8.GetBytes(sendOut);
                                ListenEvent.SocketSendMsg(mySocket, bytesMsg);
                            }
                        }
                        catch(Exception e)
                        {
                            
                        }
                    }
                    mySocket.Shutdown(SocketShutdown.Both);
                    mySocket.Close();
                    // Thread.Sleep(2000);
                }
                try
                {
                    ListenEvent.Socket.Close();
                    ListenEvent.Socket = null;
                }
                catch
                {

                }
                canStop = false;

            }
            catch (SocketException e)
            {
                canStop = false;
                
            }
        }
        internal static void SendHeader(Socket mySocket, string sHttpVersion, string sStatusCode, string sMimeHeader, string sAcceptRanges, int iTotBytes)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} {1}\r\n", sHttpVersion, sStatusCode);
            sb.AppendFormat("Server: 03s.cn_{0}\r\n", "");
            sb.AppendFormat("Content-Type: {0}\r\n", sMimeHeader);
            sb.AppendFormat("Accept-Ranges: {0}\r\n", sAcceptRanges);
            sb.AppendFormat("Content-Length: {0}\r\n", iTotBytes);
            sb.AppendFormat("Access-Control-Allow-Origin: {0}\r\n", "*");
            sb.AppendFormat("Access-Control-Allow-Methods: {0}\r\n", "POST");
            sb.AppendLine("Connection: close");
            sb.AppendLine();
            ListenEvent.SocketSendMsg(mySocket, Encoding.ASCII.GetBytes(sb.ToString()));
        }
        internal static void SocketSendMsg(Socket mySocket, byte[] bytesMsg)
        {
            try
            {
                if (mySocket.Connected)
                {
                    if ((mySocket.Send(bytesMsg, bytesMsg.Length, 0)) == -1)
                    {
                        ListenEvent.Plugin.OnLog("Socket Error cannot Send Packet");
                    }
                }
                else
                {
                    ListenEvent.Plugin.OnLog("连接失败....");
                }
            }
            catch (Exception e)
            {
                ListenEvent.Plugin.OnLog("APIDo:" + e.Message);

            }
        }
        /// <summary>
        /// 接口处理
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static Dictionary<string, string> ParseParameter(string buffer)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
        string[] aryData = buffer.TrimStart('?', '&').Split(new[] { "&" }, StringSplitOptions.None);
        string last = string.Empty;
            foreach (string arr in aryData)
            {
                if (last == "Message")
                {
                    dic["Message"] = dic["Message"] + "&" + arr;
                    continue;
                }
                if (arr.IndexOf("=", StringComparison.CurrentCulture) > 1)
                {
                    string[] arrT = arr.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
    last = arrT[0];
                    if (!dic.ContainsKey(last))
                    {
                        dic.Add(last, Static.UrlDecode(arr.Replace(arrT[0] + "=", "")));
                    }
                    else
{
    last = string.Empty;
}
                }
                else
{
    if (!string.IsNullOrEmpty(last))
    {
        dic[last] = dic[last] + "&" + arr;
    }
}
            }
            return dic;
        }
        public static string ApiSelect(string str)
        {
            string api;
            string WebApiStr = str;
            if (str.IndexOf(".do?", StringComparison.CurrentCulture) < 1)
            {
                return @"<!DOCTYPE html><html lang=""zh-CN""><head> <meta charset=""UTF-8"" /> <title>管理中心</title> <meta name=""renderer"" content=""webkit"" /> <meta http-equiv=""X-UA-Compatible"" content=""IE=edge,chrome=1"" /> <meta name=""viewport"" content=""width=device-width, initial-scale=1, maximum-scale=1"" /> <meta name=""apple-mobile-web-app-status-bar-style"" content=""black"" /> <meta name=""apple-mobile-web-app-capable"" content=""yes"" /> <meta name=""format-detection"" content=""telephone=no"" /> <link href=""https://oss-1258026085.cos.ap-shanghai.myqcloud.com/layui-v2.4.5/layui/css/layui.css"" rel=""stylesheet"" /> <style> html, body { width: 100%; height: 100%; } body { display: flex; justify-content: space-between; align-items: stretch; position: relative; } .right { flex: 1 1 auto; overflow-y: auto; } .right > div { width: 100%; height: 100%; padding: 20px; box-sizing: border-box; } i.layui-icon-more { display: none; } .right > div > h2 { margin-bottom: 20px; } .right > div > .layui-card { cursor: pointer; } [lay-filter=""navLeft""] { height: 100%; width: 500px; } .layui-badge, .layui-badge-dot { position: unset !important; top: unset !important; margin-right: 5px; } .layui-layer-content p img{ max-width: 60%; max-height: 200px; } [lay-filter=""AllowSetFilter""]{ height: 100%; display: flex; flex-direction: column; } [lay-filter=""AllowSetFilter""]>.layui-tab-content{ flex: 1 1 auto; } [lay-filter=""AllowSetFilter""]>.layui-tab-content>.layui-tab-item{ height: 100%; } [lay-filter=""AllowSetFilter""]>.layui-tab-content>.layui-tab-item>form{ height: 100%; display: flex; flex-direction: column; } [lay-filter=""AllowSetFilter""]>.layui-tab-content>.layui-tab-item>form>.layui-form-text{ flex: 1 1 auto; display: flex; flex-direction: column; } [lay-filter=""AllowSetFilter""]>.layui-tab-content>.layui-tab-item>form>.layui-form-text>.layui-input-block{ flex: 1 1 auto; } [lay-filter=""AllowSetFilter""]>.layui-tab-content>.layui-tab-item>form>.layui-form-text>.layui-input-block>textarea{ height: 100%; } @media (max-width: 500px) { .left { display: none; } .left.active { display: block; } .right { position: relative; } i.layui-icon-more { display: block; position: absolute; top: 20px; right: 20px; cursor: pointer; font-size: 32px; background-color: #0c0c0c; border-radius: 50%; color: #ffffff; } i.layui-icon-more.active{ background-color: #ffffff; color: #0c0c0c; } .layui-card-body img { width: 100%; } [lay-filter=""navLeft""] { height: 100%; width: 100%; } } </style></head><body><div class=""left""> <ul class=""layui-nav layui-nav-tree"" lay-filter=""navLeft"" style=""overflow-y: auto""> <li class=""layui-nav-item layui-nav-itemed AllowSet""> <a href=""javascript:;""> 管理设置 </a> <dl class=""layui-nav-child""></dl> </li> <li class=""layui-nav-item layui-nav-itemed GroupList""> <a href=""javascript:;""> 群组列表 </a> <dl class=""layui-nav-child""></dl> </li> <li class=""layui-nav-item layui-nav-itemed BadWord""> <a href=""javascript:;""> 黑名单列表 </a> <dl class=""layui-nav-child""></dl> </li> </ul></div><div class=""right""> <div></div></div><i class=""layui-icon layui-icon-more""> </i></body></html><script src=""https://oss-1258026085.cos.ap-shanghai.myqcloud.com/layui-v2.4.5/layui/layui.js""></script><script> let ApiUrl = ""/""; let getQueryStringByName = function (name) { var result = location.search.match( new RegExp(""[\?\&]"" + name + ""=([^\&]+)"", ""i"") ); if (result == null || result.length < 1) { return """"; } return result[1]; }; layui.use([""form"", ""element"", ""layer"",""laypage""], function () { let form = layui.form; let element = layui.element; let $ = layui.jquery; let laypage = layui.laypage; $(""i.layui-icon-more"") .off(""click"") .on(""click"", function () { $("".left,i.layui-icon-more"").toggleClass(""active""); }); layer = layui.layer; let uid = getQueryStringByName(""uid""), error = function (str) { layer.alert(str, { icon: 2 }); }; console.log(uid); if (uid == """") { error(""uid参数缺失""); return false; } var _init = function () { let loadIndex = layer.load(1, { shade: 0.3 }); $.ajax({ dataType: ""json"", type: ""get"", url: ApiUrl + ""GroupList.do"", data: { uid: uid }, async: true, success: function (res) { layer.close(loadIndex); if (res.nFlag) { error(res.strError); return false; } $('[lay-filter=""navLeft""] .layui-nav-child').empty(); $.each(res, function (k, v) { $('[lay-filter=""navLeft""] .GroupList .layui-nav-child').append( '<dd><a href=""javascript:;"" data-id=""' + v.group_id + '"" data-member=""' + v.member_count + '"">' + (v.member_count == 1 ? '<span class=""layui-badge"">管</span>' : """") + v.group_name + ""</a></dd>"" ); if(v.member_count){ $('[lay-filter=""navLeft""] .AllowSet .layui-nav-child,[lay-filter=""navLeft""] .BadWord .layui-nav-child').append( '<dd><a href=""javascript:;"" data-id=""' + v.group_id + '"" data-member=""' + v.member_count + '"">' + (v.member_count == 1 ? '<span class=""layui-badge"">管</span>' : """") + v.group_name + ""</a></dd>""); } }); let showMessage = function (gid, GroupName, member) { if ($(""i.layui-icon-more"").is("":hidden"")) { } else { $("".left,i.layui-icon-more"").removeClass(""active""); } let loadIndex = layer.load(1, { shade: 0.3 }); $.ajax({ dataType: ""json"", type: ""get"", url: ApiUrl + ""MessageCount.do"", data: { uid: uid, group_id: gid }, async: true, success: function (MessageCount) { console.log(MessageCount); if (MessageCount.nFlag) { layer.close(loadIndex); error(MessageCount.strError); return false; } let MessageData=function (page){ $.ajax({ dataType: ""json"", type: ""get"", url: ApiUrl + ""Message.do"", data: { uid: uid, group_id: gid,page:page }, async: true, success: function (Message) { layer.close(loadIndex); if (Message.nFlag) { error(Message.strError); return false; } $("".right>div"") .empty() .html(""<h2>"" + GroupName + ""</h2>""); $.each(Message, function (k, v) { $("".right>div"").append( '<div class=""layui-card"" data-type=""'+v.sub_type+'"" data-id=""' + v.message_id + '"" data-flag=""' + v.flag + '"" data-status=""' + (v.Exit ? 1 : 0) + '""><div class=""layui-card-body""><span class=""layui-badge layui-bg-black"">'+v.raw_message+'</span>' + (v.flag == 1 ? '<span class=""layui-badge"">管</span>' : """") + (v.Exit ? '<span class=""layui-badge layui-bg-blue"">撤</span>' : """") + (v.sub_type!='message' ? '<span class=""layui-badge layui-bg-blue"">'+v.sub_type+'</span>' : """") + v.EnumData + ""</div></div>"" ); }); $("".right>div"").append('<div id=""MessagePage""></div>'); $("".right"").scrollTop(0); laypage.render({ elem: 'MessagePage' ,count: MessageCount.count ,limit:10 ,curr:page ,jump: function(obj, first){ if(!first){ MessageData(obj.curr); } } }); if (member == 1) { $("".right>div>.layui-card"") .off(""click"") .on(""click"", function () { let id = $(this).data(""id""), message = $(this).find("".layui-card-body"").html(), status = $(this).data(""status""), flag = $(this).data(""flag""), type=$(this).data('type'); if (status == ""1"" || flag == ""1""||type!='message') { return false; } let w = $(window).width(); if (w > 500) w = 500; layer.open({ title: ""消息处理"", area: w + ""px"", content: '<div style=""padding: 10px;box-sizing: border-box;""><p>' + message + '</p><div style=""margin: 10px 0;""><a href=""javascript:;"" class=""layui-btn layui-btn-fluid"" data-event=""ReCall"">只撤回消息</a></div><div style=""margin: 10px 0;""><a href=""javascript:;"" class=""layui-btn layui-btn-fluid layui-btn-primary"" data-event=""CommandBan"">撤回加禁言一天</a></div></div>', btn: [""不处理""], yes(a, b) { layer.close(a); }, success(a, b) { a.find(""a[data-event]"") .off(""click"") .on(""click"", function () { let ev = $(this).data(""event""); let load = layer.load(1, { shade: 0.3 }); $.ajax({ dataType: ""json"", type: ""get"", url: ApiUrl + ev + "".do"", data: { uid: uid, message_id: id }, async: true, success: function (DoMessage) { layer.close(load); if (DoMessage.nFlag) { error(DoMessage.strError); return false; } layer.alert(""处理成功"", { icon: 1 }); showMessage(gid, GroupName, member); }, error: function ( request, status, errorThrown ) { layer.close(load); layer.alert(""网络错误"", { icon: 2 }); }, }); }); }, }); }); } else { $("".right>div>.layui-card"").off(""click""); } }, error: function (request, status, errorThrown) { layer.close(loadIndex); layer.alert(""网络错误"", { icon: 2 }); }, }); }; MessageData(1); },error: function (request, status, errorThrown) { layer.close(loadIndex); layer.alert(""网络错误"", { icon: 2 }); }, }); }, showBadWord = function (gid, GroupName, member) { if ($(""i.layui-icon-more"").is("":hidden"")) { } else { $("".left,i.layui-icon-more"").removeClass(""active""); } let loadIndex = layer.load(1, { shade: 0.3 }); $.ajax({ dataType: ""json"", type: ""get"", url: ApiUrl + ""BadWordCount.do"", data: {uid: uid, group_id: gid}, async: true, success: function (MessageCount) { console.log(MessageCount); if (MessageCount.nFlag) { layer.close(loadIndex); error(MessageCount.strError); return false; } let showBadWordData=function (page){ $.ajax({ dataType: ""json"", type: ""get"", url: ApiUrl + ""BadWord.do"", data: { uid: uid, group_id: gid,page:page }, async: true, success: function (Message) { layer.close(loadIndex); if (Message.nFlag) { error(Message.strError); return false; } $("".right>div"") .empty() .html(""<h2>"" + GroupName + ""</h2>""); $.each(Message, function (k, v) { $("".right>div"").append( '<div class=""layui-card"" data-id=""' + v.message_id + '"" data-status=""' + (v.Exit ? 1 : 0) + '""><div class=""layui-card-body""><span class=""layui-badge layui-bg-black"">'+v.raw_message+'</span>' + (v.Exit ? '<span class=""layui-badge layui-bg-blue"">撤</span>' : """") + v.EnumData + ""</div></div>"" ); }); $("".right>div"").append('<div id=""MessagePage""></div>'); $("".right"").scrollTop(0); laypage.render({ elem: 'MessagePage' ,count: MessageCount.count ,limit:10 ,curr:page ,jump: function(obj, first){ if(!first){ showBadWordData(obj.curr); } } }); }, error: function (request, status, errorThrown) { layer.close(loadIndex); layer.alert(""网络错误"", { icon: 2 }); }, }); }; showBadWordData(1); },error: function (request, status, errorThrown) { layer.close(loadIndex); layer.alert(""网络错误"", { icon: 2 }); }, }); },toHump=function (name){ return name.replace(/\-(\w)/g, function(all, letter){ return letter.toUpperCase(); }); },showSet=function (gid, GroupName, member){ if ($(""i.layui-icon-more"").is("":hidden"")) { } else { $("".left,i.layui-icon-more"").removeClass(""active""); } $("".right>div"").empty().append('<h2>' + GroupName + '</h2><div class=""layui-tab"" lay-filter=""AllowSetFilter""><ul class=""layui-tab-title""><li lay-id=""not-allow-text"" >文本黑名单</li><li lay-id=""url-allow"" >网址白名单</li><li lay-id=""qq"" >QQ黑名单</li></ul><div class=""layui-tab-content""><div class=""layui-tab-item"" lay-id=""not-allow-text""></div><div class=""layui-tab-item"" lay-id=""url-allow""></div><div class=""layui-tab-item"" lay-id=""qq""></div></div></div>'); let element = layui.element; element.on('tab(AllowSetFilter)', function(){ let id=this.getAttribute('lay-id'); let loadIndex = layer.load(1, { shade: 0.3 }); $.ajax({ dataType: ""json"", type: ""get"", url: ApiUrl + ""NotAllow.do"", data: {uid: uid, group_id: gid,action:id}, async: true, success: function (AllowInfo) { layer.close(loadIndex); if (AllowInfo.nFlag) { error(AllowInfo.strError); return false; } $('[lay-filter=""AllowSetFilter""]>.layui-tab-content>[lay-id=""'+id+'""]').empty().html('<form class=""layui-form layui-form-pane"" action=""""><div class=""layui-form-item""><label class=""layui-form-label"">开关</label><div class=""layui-input-block""><input type=""checkbox"" name=""switch"" lay-skin=""switch"" lay-text=""开启|关闭""></div></div><div class=""layui-form-item layui-form-text""><label class=""layui-form-label"">列表</label><div class=""layui-input-block""><textarea name=""desc"" placeholder=""请输入内容"" class=""layui-textarea""></textarea></div></div><div class=""layui-form-item""><div class=""layui-input-block""><button class=""layui-btn"" lay-submit lay-filter=""form'+toHump(id)+'"">立即提交</button></div></div></form>'); if(AllowInfo[1]===""true""){ $('[lay-filter=""AllowSetFilter""]>.layui-tab-content>[lay-id=""'+id+'""] input[name=""switch""]').prop(""checked"",true); } if(AllowInfo[2]!=""""){ $('[lay-filter=""AllowSetFilter""]>.layui-tab-content>.layui-tab-item[lay-id=""'+id+'""]>form>.layui-form-text>.layui-input-block>textarea').val( AllowInfo[2].replace(/\|/g,""\n"") ); } layui.form.render(); let _Submit=function (data){ let load = layer.load(1, { shade: 0.3 }); $.ajax({ dataType: ""json"", type: ""get"", url: ApiUrl + ""AllowSet.do"", data: data, async: true, success: function (DoMessage) { layer.close(load); if (DoMessage.nFlag) { error(DoMessage.strError); return false; } layer.alert(""处理成功"", { icon: 1 }); }, error: function ( request, status, errorThrown ) { layer.close(load); layer.alert(""网络错误"", { icon: 2 }); }, }); }; layui.form.on('submit(formnotAllowText)', function(data){ let send={ uid: uid, group_id: gid,action:id,field:data.field.desc.replace(/\n/g,""|"").replace(/\r/g,""""),open:data.field.switch?'true':'false' }; _Submit(send); return false; }); layui.form.on('submit(formqq)', function(data){ let send={ uid: uid, group_id: gid,action:id,field:data.field.desc.replace(/\n/g,""|"").replace(/\r/g,""""),open:data.field.switch?'true':'false' }; _Submit(send); return false; }); layui.form.on('submit(formurlAllow)', function(data){ let send={ uid: uid, group_id: gid,action:id,field:data.field.desc.replace(/\n/g,""|"").replace(/\r/g,""""),open:data.field.switch?'true':'false' }; _Submit(send); return false; }); },error: function (request, status, errorThrown) { layer.close(loadIndex); layer.alert(""网络错误"", { icon: 2 }); }, }); }); $('[lay-filter=""AllowSetFilter""]>.layui-tab-title>li:eq(0)').trigger('click'); }; let dm; let gidHash = window.location.hash ? window.location.hash.substr(1) : """"; if (gidHash != """") { let gidArr = gidHash.split(""&""), gid = gidArr[0], type = gidArr[1]; dm = $( '[lay-filter=""navLeft""] .layui-nav-child>dd>a[data-id=' + gid + ""]"" ); if (type == ""GroupList"") { showMessage(dm.data(""id""), dm.text(), dm.data(""member"")); }else if (type == ""AllowSet"") { showSet(dm.data(""id""), dm.text(), dm.data(""member"")); } else { showBadWord(dm.data(""id""), dm.text(), dm.data(""member"")); } } else { if($('[lay-filter=""navLeft""] .AllowSet .layui-nav-child>dd').size()>0){ dm = $('[lay-filter=""navLeft""] .AllowSet .layui-nav-child>dd:eq(0)>a'); window.location.hash = ""#"" + dm.data(""id"") + ""&AllowSet""; showSet(dm.data(""id""), dm.text(), dm.data(""member"")); }else{ dm = $('[lay-filter=""navLeft""] .GroupList .layui-nav-child>dd:eq(0)>a'); window.location.hash = ""#"" + dm.data(""id"") + ""&GroupList""; showMessage(dm.data(""id""), dm.text(), dm.data(""member"")); } } $('[lay-filter=""navLeft""] .AllowSet .layui-nav-child>dd>a') .off(""click"") .on(""click"", function () { dm = $(this); window.location.hash = ""#"" + dm.data(""id"") + ""&AllowSet""; showSet(dm.data(""id""), dm.text(), dm.data(""member"")); }); $('[lay-filter=""navLeft""] .GroupList .layui-nav-child>dd>a') .off(""click"") .on(""click"", function () { dm = $(this); window.location.hash = ""#"" + dm.data(""id"") + ""&GroupList""; showMessage(dm.data(""id""), dm.text(), dm.data(""member"")); }); $('[lay-filter=""navLeft""] .BadWord .layui-nav-child>dd>a') .off(""click"") .on(""click"", function () { dm = $(this); window.location.hash = ""#"" + dm.data(""id"") + ""&BadWord""; showBadWord(dm.data(""id""), dm.text(), dm.data(""member"")); }); }, error: function (request, status, errorThrown) { layer.close(loadIndex); layer.alert(""网络错误"", { icon: 2 }); }, }); }; _init(); });</script>";
            }
            else
            {
                if (str.StartsWith("/"))
                {
                    str = str.Substring(1);
                }
                api = str.Substring(0, str.IndexOf(".do?", StringComparison.CurrentCulture));
                str = str.Substring(str.IndexOf(".do?", StringComparison.CurrentCulture) + 4);
            }
            
            string outmessage="";
            Dictionary<string, string> param = ListenEvent.ParseParameter(str);

            if (!param.ContainsKey("uid"))
            {
                return Error("uid参数错误001");
            }
            string uid = CryptoUtil.DecryptHex(param["uid"], CryptoUtil.md5("kuaijieyun"), CryptoUtil.md5("Cn"), 256, "CFB") ;
            if (!Regex.IsMatch(uid, @"^([0-9\.]+)$"))
            {
                return Error("uid参数错误");
            }
            string[] uArr = uid.Split('.');
            uint UserId = 0;
            try
            {
                UserId = Convert.ToUInt32(uArr[0]);
            }
            catch
            {
                return Error("uid参数错误");
            }




               
            switch (api)
            {
                case "GroupList":
                    if (Robot.Admin.Contains(UserId.ToString()))
                    {
                        GroupInfo[] clusters = Cluster.Get();
                        List<GroupInfo> GroupInfoList = new List<GroupInfo>();
                        foreach (GroupInfo s in clusters)
                        {
                            if (Cluster.GetAdmin(s.group_id).Contains(Robot.id))
                            {
                                s.member_count = 1;
                            }
                            else
                            {
                                s.member_count = 0;
                            }
                            GroupInfoList.Add(s);
                        }
                        outmessage = JsonHelper.SerializeObject(GroupInfoList);
                    }
                    else
                    {
                        GroupInfo[] clusters = Cluster.Get();
                        List<GroupInfo> GroupInfoList = new List<GroupInfo>();
                        foreach (GroupInfo s in clusters)
                        {
                            if( Cluster.GetAdmin(s.group_id).Contains(UserId))
                            {
                                if (Cluster.GetAdmin(s.group_id).Contains(Robot.id))
                                {
                                    s.member_count = 1;
                                }
                                else
                                {
                                    s.member_count = 0;
                                }
                                GroupInfoList.Add(s);
                            }
                        }
                        outmessage= JsonHelper.SerializeObject(GroupInfoList.ToArray());
                    }
                    break;
                case "MessageCount":
                    if (!param.ContainsKey("group_id"))
                    {
                        outmessage = Error("group_id参数错误");
                    }
                    else
                    {
                        if (!Regex.IsMatch(param["group_id"], @"^([0-9]+)$"))
                        {
                            outmessage = Error("group_id参数错误");
                        }
                        else
                        {
                            if (Robot.Admin.Contains(UserId.ToString()) || Cluster.GetAdmin(Convert.ToUInt32(param["group_id"])).Contains(UserId))
                            {
                                long GetMessageList = SqlConn.GetMessageListCount(Convert.ToUInt32(param["group_id"]));

                                outmessage = "{\"count\":"+ GetMessageList + "}";
                            }
                            else
                            {
                                outmessage = Error("没有权限");
                            }
                        }
                    }
                    break;
                case "Message":
                    if (!param.ContainsKey("group_id"))
                    {
                        outmessage= Error("group_id参数错误");
                    }
                    else
                    {
                        if (!Regex.IsMatch(param["group_id"], @"^([0-9]+)$"))
                        {
                            outmessage= Error("group_id参数错误");
                        }
                        else
                        {
                            if (Robot.Admin.Contains(UserId.ToString()) || Cluster.GetAdmin(Convert.ToUInt32(param["group_id"])).Contains(UserId))
                            {
                                int Page = Convert.ToInt32(param["page"])-1;
                                List<RevMessageEvent> GetMessageList = SqlConn.GetMessageList(Convert.ToUInt32(param["group_id"]),0, Page*10);
                                
                                outmessage = JsonHelper.SerializeObject(GetMessageList);
                            }
                            else
                            {
                                outmessage = Error("没有权限");
                            }
                        }
                    }
                    break;
                case "BadWordCount":
                    if (!param.ContainsKey("group_id"))
                    {
                        outmessage = Error("group_id参数错误");
                    }
                    else
                    {
                        if (!Regex.IsMatch(param["group_id"], @"^([0-9]+)$"))
                        {
                            outmessage = Error("group_id参数错误");
                        }
                        else
                        {
                            if (Robot.Admin.Contains(UserId.ToString()) || Cluster.GetAdmin(Convert.ToUInt32(param["group_id"])).Contains(UserId))
                            {
                                long GetMessageList = SqlConn.GetBadMessageListCount(Convert.ToUInt32(param["group_id"]));

                                outmessage = "{\"count\":" + GetMessageList + "}";
                            }
                            else
                            {
                                outmessage = Error("没有权限");
                            }
                        }
                    }
                    break;
                case "BadWord":
                    if (!param.ContainsKey("group_id"))
                    {
                        outmessage = Error("group_id参数错误");
                    }
                    else
                    {
                        if (!Regex.IsMatch(param["group_id"], @"^([0-9]+)$"))
                        {
                            outmessage = Error("group_id参数错误");
                        }
                        else
                        {
                            if (Robot.Admin.Contains(UserId.ToString()) || Cluster.GetAdmin(Convert.ToUInt32(param["group_id"])).Contains(UserId))
                            {
                                int Page = Convert.ToInt32(param["page"]) - 1;
                                List<RevMessageEvent> GetMessageList = SqlConn.GetBadMessageList(Convert.ToUInt32(param["group_id"]), Page * 10);

                                outmessage = JsonHelper.SerializeObject(GetMessageList);
                            }
                            else
                            {
                                outmessage = Error("没有权限");
                            }
                        }
                    }
                    break;
                case "ReCall":
                    if (!param.ContainsKey("message_id"))
                    {
                        outmessage = Error("message_id参数错误");
                    }
                    else
                    {
                        if (!Regex.IsMatch(param["message_id"], @"^([0-9\-]+)$"))
                        {
                            outmessage = Error("message_id参数错误");
                        }
                        else
                        {

                            List<RevMessageEvent> GetMessageList = SqlConn.GetMessageList(0,long.Parse(param["message_id"]));
                            if (GetMessageList.Count == 0)
                            {
                                outmessage = Error("消息不存在");
                            }
                            else
                            {

                                foreach(RevMessageEvent s in GetMessageList)
                                {
                                    if (Robot.Admin.Contains(UserId.ToString()) || Cluster.GetAdmin(s.group_id).Contains(UserId))
                                    {
                                        Cluster.DelMsg(s.message_id);
                                        outmessage = JsonHelper.SerializeObject(GetMessageList);
                                    }
                                    else
                                    {
                                        outmessage = Error("没有权限");
                                    }
                                    break;
                                }
                            }
                            
                        }
                    }
                    break;
                case "CommandBan":
                    if (!param.ContainsKey("message_id"))
                    {
                        outmessage = Error("message_id参数错误");
                    }
                    else
                    {
                        if (!Regex.IsMatch(param["message_id"], @"^([0-9\-]+)$"))
                        {
                            outmessage = Error("message_id参数错误");
                        }
                        else
                        {

                            List<RevMessageEvent> GetMessageList = SqlConn.GetMessageList(0, long.Parse(param["message_id"]));
                            if (GetMessageList.Count == 0)
                            {
                                outmessage = Error("消息不存在");
                            }
                            else
                            {

                                foreach (RevMessageEvent s in GetMessageList)
                                {
                                    if (Robot.Admin.Contains(UserId.ToString()) || Cluster.GetAdmin(s.group_id).Contains(UserId))
                                    {
                                        Cluster.DelMsg(s.message_id);
                                        int r = Cluster.CommandBan(s.group_id, s.user_id, 1440);
                                        if (r != 0)
                                        {
                                            outmessage = Error("禁言失败");
                                        }
                                        else
                                        {
                                            outmessage = JsonHelper.SerializeObject(GetMessageList);
                                        }
                                        
                                    }
                                    else
                                    {
                                        outmessage = Error("没有权限");
                                    }
                                    break;
                                }
                            }

                        }
                    }
                    break;
                case "AllowSet":
                    {
                        if (!param.ContainsKey("group_id"))
                        {
                            outmessage = Error("group_id参数错误");
                        }
                        else
                        {
                            if (!Regex.IsMatch(param["group_id"], @"^([0-9\-]+)$"))
                            {
                                outmessage = Error("group_id参数错误");
                            }
                            else
                            {
                                
                                if (!param.ContainsKey("action"))
                                {
                                    outmessage = Error("Action参数错误");

                                }
                                else
                                {
                                    string action = param["action"];
                                    if (!param.ContainsKey("open"))
                                    {
                                        outmessage = Error("open参数错误");

                                    }
                                    else
                                    {
                                        string open = param["open"];
                                        if (!param.ContainsKey("field"))
                                        {
                                            outmessage = Error("列表参数错误");

                                        }
                                        else
                                        {
                                            string field = param["field"];
                                            string[] data = new string[] { "false", open, field };
                                            File.WriteAllLines(Robot.path + @"config\" + action + "." + param["group_id"],data);
                                            
                                            outmessage = JsonHelper.SerializeObject(data);
                                        }
                                        
                                    }
                                    
                                }
                                
                            }


                        }
                        break;
                    }
                case "NotAllow":
                    {
                        if (!param.ContainsKey("group_id"))
                        {
                            outmessage = Error("group_id参数错误");
                        }
                        else
                        {
                            if (!Regex.IsMatch(param["group_id"], @"^([0-9\-]+)$"))
                            {
                                outmessage = Error("group_id参数错误");
                            }
                            else
                            {
                                string action = "not-allow-text";
                                if (param.ContainsKey("action"))
                                {
                                    action = param["action"];

                                }
                                string[] data = new string[] { "false", "false", "" };
                                if (File.Exists(Robot.path + @"config\" + action + "." + param["group_id"]))
                                {
                                    data = File.ReadAllLines(Robot.path + @"config\" + action + "." + param["group_id"]);
                                }
                                else
                                {
                                    if (action == "url-allow")
                                    {
                                        data = new string[] { "false", "false", "03s.cn|ximijia.cn|08pr.com|kuaijieyun.cn|lusongsong.com|songsongyun.com|qq.com|weixin.com|qpic.cn|ruanwenpu.com|zui5.com|qqvps.cn|baidu.com|idqqimg.com|gtimg.cn|idcsmart.com|idcsmart.cn|idcsmart.net|html5code.org|bear-studio.net|github.com|java.net|php.net|csdn.net|gitee.io|gitee.com|gitee.net|chinaz.com" };
                                    }
                                }
                                outmessage = JsonHelper.SerializeObject(data);
                            }
                            

                        }
                        break;
                    }
                default:
                    {
                        outmessage = "{\"nFlag\":2, \"strError\":\"Not Found Page2\"}";


                        break;
                    }

            }
            
            return outmessage;
        }


        public static string Error(string error)
        {
            return "{\"nFlag\":2, \"strError\":\"" + error.Replace("\"", "\\\"") + "\"}";
        }
        public static string Success(string success)
        {
            return "{\"nFlag\":1, \"Info\":\"" + success.Replace("\"", "\\\"") + "\"}";
        }
    }
}
