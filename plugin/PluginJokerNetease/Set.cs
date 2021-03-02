using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QQRobotFramework;
namespace PluginJokerNetease
{
    public partial class Set : Form
    {
        JokerNetease Plugin { get; set; }
        public Set(JokerNetease f)
        {
            Plugin = f;
            InitializeComponent();
            this.Icon = Static.icon;
            List<string> uri = Plugin.JokerUrl();
            richTextBox1.Text = string.Join("\n", uri.ToArray());
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> v = new List<string>();
            string[] data = richTextBox1.Text.Replace("\r", "").Replace("\t","").Replace("\n","|").Split('|');
            foreach (string item in data)
            {
                v.Add("('" + item + "')");
            }
            Plugin.JokerUrlSave(v);
            this.Close();
        }
    }
}
