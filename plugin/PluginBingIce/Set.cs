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
        
            checkBox1.Checked = Plugin.PluginType.Contains("微软小冰");
            textBox5.Text = Plugin.aid;
            textBox6.Text = Plugin.ak;
            textBox7.Text = Plugin.QcloudBotId;
        }

       

        private void button1_Click(object sender, EventArgs e)
        {

            
           
            Plugin.aid = textBox5.Text;
            Plugin.ak = textBox6.Text;
            Plugin.QcloudBotId = textBox7.Text;
            File.WriteAllText(Robot.path + @"PluginBingIce\ice.plugin", checkBox1.Checked?"yes":"no");
            
            File.WriteAllText(Robot.path + @"PluginBingIce\aid.plugin", Plugin.aid);
            File.WriteAllText(Robot.path + @"PluginBingIce\ak.plugin", Plugin.ak);
            File.WriteAllText(Robot.path + @"PluginBingIce\QcloudBotId.plugin", Plugin.QcloudBotId);
            Plugin.SetDefault();
            MessageBox.Show("保存在功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }
    }
}
