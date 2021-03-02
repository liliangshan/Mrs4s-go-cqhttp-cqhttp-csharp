using System;
using System.IO;
using System.Windows.Forms;
using QQRobotFramework;
namespace PluginBingIce
{
    public partial class Set : Form
    {
        ice Plugin { get; set; }
        public Set(ice f)
        {
            Plugin = f;
            InitializeComponent();
            this.Icon = Static.icon;
            this.Text = "小冰助手";
        }

        private void Set_Load(object sender, EventArgs e)
        {
            textBox1.Text = Plugin.cookie;
            textBox2.Text = Plugin.referer;
            textBox3.Text = Plugin.messageUrl;
            textBox4.Text = Plugin.st==""? "fileId=null&uid=[换成自己的UID]&st=[换成自己的ST]&content=111":Plugin.st;
            textBox5.Text = Plugin.aid;
            textBox6.Text = Plugin.ak;
            textBox7.Text = Plugin.QcloudBotId;
        }

       

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("请输入cookie","提示",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }
            if (textBox2.Text == "")
            {
                MessageBox.Show("请输入Referer", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (textBox3.Text == "")
            {
                MessageBox.Show("请输入消息提取网址", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (textBox4.Text == "")
            {
                MessageBox.Show("请输入消息提交参数", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Plugin.cookie = textBox1.Text;
            Plugin.messageUrl = textBox3.Text;
            Plugin.referer = textBox2.Text;
            Plugin.st = textBox4.Text;
            Plugin.aid = textBox5.Text;
            Plugin.ak = textBox6.Text;
            Plugin.QcloudBotId = textBox7.Text;
            File.WriteAllText(Robot.path + @"PluginBingIce\cookie.plugin", textBox1.Text);
            File.WriteAllText(Robot.path + @"PluginBingIce\referer.plugin", textBox2.Text);
            File.WriteAllText(Robot.path + @"PluginBingIce\messageUrl.plugin", textBox3.Text);
            File.WriteAllText(Robot.path + @"PluginBingIce\st.plugin", textBox4.Text);
            File.WriteAllText(Robot.path + @"PluginBingIce\aid.plugin", Plugin.aid);
            File.WriteAllText(Robot.path + @"PluginBingIce\ak.plugin", Plugin.ak);
            File.WriteAllText(Robot.path + @"PluginBingIce\QcloudBotId.plugin", Plugin.QcloudBotId);
            Plugin.SetDefault();
            

            // Plugin.iceSend("你是谁，你怎么样");
            MessageBox.Show("保存在功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
