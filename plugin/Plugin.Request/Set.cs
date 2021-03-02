using QQRobotFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace pluginRequest
{
    public partial class Set : Form
    {
        Req req { get; set; }
        public Set(Req f)
        {
            InitializeComponent();
            req = f;
            this.Icon = Static.icon;
            this.Text = "好友申请/好友加群设置";
            if (req.Config[0] == "true") checkBox1.Checked = true;
            if (req.Config[1] == "true") checkBox2.Checked = true;
            if (req.Config[2] == "true") checkBox3.Checked = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> v = new List<string>();
            if (checkBox1.Checked)
            {
                v.Add("true");
            }
            else
            {
                v.Add("false");
            }
            if (checkBox2.Checked)
            {
                v.Add("true");
            }
            else
            {
                v.Add("false");
            }
            if (checkBox3.Checked)
            {
                v.Add("true");
            }
            else
            {
                v.Add("false");
            }
            File.WriteAllLines(Robot.path + "plugin.Request.config", v.ToArray());
            req.Config = v.ToArray();
            if( MessageBox.Show("保存成功","成功",MessageBoxButtons.OK,MessageBoxIcon.Information)==DialogResult.OK)
            {
                this.Close();
            }
        }
    }
}
