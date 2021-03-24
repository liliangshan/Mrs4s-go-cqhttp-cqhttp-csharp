using CefSharp;
using CefSharp.Handler;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PluginBingIce
{
    public partial class GetTk : Form
    {
        ChromiumWebBrowser wb { get; set; }
        ice Plugin { get; set; }
        public GetTk(ice f,string url)
        {
            Plugin = f;
            SetCookie.Plugin = f;
            SetCookie.tkPlugin = this;
            InitializeComponent();
            this.Icon = QQRobotFramework.Static.icon;
            panel1.Controls.Clear();

            CefSettings settings = new CefSettings();
            settings.Locale = "zh-CN";
            settings.CefCommandLineArgs.Add("disable-gpu", "1");
            try
            {
                Cef.Initialize(settings);
            }
            catch
            {

            }
            if (File.Exists(QQRobotFramework.Robot.path + @"PluginBingIce\cookie.txt"))
            {
                string cookie = File.ReadAllText(QQRobotFramework.Robot.path + @"PluginBingIce\cookie.txt");
                SetCookie.cookies = QQRobotFramework.JsonHelper.DeserializeObject<CookieData[]>(cookie).ToList<CookieData>();
            }


            wb = new ChromiumWebBrowser(url);

            BrowserSettings browserSettings = new BrowserSettings();
            browserSettings.FileAccessFromFileUrls = CefState.Enabled;
            browserSettings.UniversalAccessFromFileUrls = CefState.Enabled;
            wb.BrowserSettings = browserSettings;

            wb.RequestHandler = new WinFormsRequestHandler();
            panel1.Controls.Add(wb);

            wb.Dock = DockStyle.Fill;
        }

        private void GetTk_Load(object sender, EventArgs e)
        {
            
        }
    }
    
    public class SetCookie
    {
        public static ice Plugin { get; set; }
        public static GetTk tkPlugin { get; set; }
        public static List<CookieData> cookies = new List<CookieData>();
        public static void set(string url)
        {
            var cookieManager = CefSharp.Cef.GetGlobalCookieManager();
            foreach (CookieData c in cookies)
            {
                DateTime dValue = DateTime.MaxValue;
                if (c.Expires != "")
                {
                    dValue = DateTime.Parse(c.Expires);
                }
                DateTime? d = new Nullable<DateTime>(dValue);

                cookieManager.SetCookie(url, new CefSharp.Cookie()
                {
                    //   Domain = "m.weibo.cn",
                    Name = c.Name,
                    Value = c.Value,
                    Expires = d
                });

            }

        }
    }
    public class WinFormsRequestHandler : RequestHandler
    {
        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {

            SetCookie.set(request.Url);
            return new WinFormResourceRequestHandler();
        }

    }
    public class TestJsonFilter : IResponseFilter
    {
        public List<byte> DataAll = new List<byte>();

        public FilterStatus Filter(System.IO.Stream dataIn, out long dataInRead, System.IO.Stream dataOut, out long dataOutWritten)
        {
            //  try
            //  {
            if (dataIn == null || dataIn.Length == 0)
            {
                dataInRead = 0;
                dataOutWritten = 0;

                return FilterStatus.Done;
            }

            dataInRead = dataIn.Length;

            dataOutWritten = Math.Min(dataInRead, dataOut.Length);
            if (dataIn.Length > dataOut.Length)
            {
                var data = new byte[dataOut.Length];
                dataIn.Seek(0, SeekOrigin.Begin);
                dataIn.Read(data, 0, data.Length);
                dataOut.Write(data, 0, data.Length);

                dataInRead = dataOut.Length;
                dataOutWritten = dataOut.Length;
                return FilterStatus.NeedMoreData;
            }
            else
            {
                dataIn.CopyTo(dataOut);
            }

            dataIn.Seek(0, SeekOrigin.Begin);
            byte[] bs = new byte[dataIn.Length];
            dataIn.Read(bs, 0, bs.Length);
            DataAll.AddRange(bs);

            dataInRead = dataIn.Length;
            dataOutWritten = dataIn.Length;

            return FilterStatus.NeedMoreData;
          
        }

        public bool InitFilter()
        {
            return true;
        }

        public void Dispose()
        {

        }
    }
    public class FilterManager
    {
        private static Dictionary<string, IResponseFilter> dataList = new Dictionary<string, IResponseFilter>();

        public static IResponseFilter CreateFilter(string guid)
        {
            lock (dataList)
            {
                var filter = new TestJsonFilter();
                dataList.Add(guid, filter);

                return filter;
            }
        }

        public static IResponseFilter GetFileter(string guid)
        {
            lock (dataList)
            {
                if (dataList.ContainsKey(guid))
                {
                    return dataList[guid];
                }
                else
                {
                    return null;
                }

            }
        }
    }
    class CookieVisitor : ICookieVisitor

    {
        public CookieVisitor()
        {
            SetCookie.cookies.Clear();
        }
        public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)

        {
            object nullObj = cookie.Expires;
            Console.WriteLine("[" + count + "][" + total + "]" + cookie.Name + ":" + cookie.Value + ":" + cookie.Domain + ":" + (nullObj != null ? ((DateTime)nullObj).ToString("yyyy-MM-dd HH:mm:ss") : ""));

            CookieData data = new CookieData()
            {
                Name = cookie.Name,
                Value = cookie.Value,
                Domain = cookie.Domain,
                Expires = nullObj != null ? ((DateTime)nullObj).ToString("yyyy-MM-dd HH:mm:ss") : ""
            };

            SetCookie.cookies.Add(data);
            if (count == total - 1)
            {
                string cookieList = QQRobotFramework.JsonHelper.SerializeObject(SetCookie.cookies.ToArray());
                File.WriteAllText(QQRobotFramework.Robot.path + @"PluginBingIce\cookie.txt", cookieList);
                Console.WriteLine(cookie);
            }
           

            return true;

        }

        public void Dispose()

        {
        }

    }
    public class WinFormResourceRequestHandler : ResourceRequestHandler
    {
        protected override IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            if (request.Url.IndexOf(".png") == -1 && request.Url.IndexOf(".jpg") == -1 && request.Url.IndexOf(".css") == -1 && request.Url.IndexOf(".gif") == -1 && request.Url.IndexOf(".ico") == -1)
            {
                var filter = FilterManager.CreateFilter(request.Identifier.ToString());
                return filter;
            }
            return null;
        }

        protected override void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            Console.WriteLine(request.Url);
            if (request.Url.IndexOf("/u/5175429989") > -1)
            {
                var cookieManager = chromiumWebBrowser.GetCookieManager();

                cookieManager.VisitAllCookies(new CookieVisitor());



            }

            if (request.Url.IndexOf("/login?") > -1)
            {

                var f = FilterManager.GetFileter(request.Identifier.ToString());
                if (f == null)
                {

                    return;
                }
                var filter = f as TestJsonFilter;

                ASCIIEncoding encoding = new ASCIIEncoding();
                //这里截获返回的数据
                string data = encoding.GetString(filter.DataAll.ToArray());

                string st = new Regex(@"""st"":""(?<ticket>[\s\S]*?)""", RegexOptions.IgnoreCase).Match(data).Groups["ticket"].Value;

                SetCookie.tkPlugin.BeginInvoke(new EventHandler(delegate {
                    SetCookie.Plugin.SetTk();
                    SetCookie.tkPlugin.Close();
                }));
            }




        }
    }
}
