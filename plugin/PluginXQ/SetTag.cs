using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PluginXQ
{
    public partial class SetTag : Form
    {
        Set Plugin { get; set; }
        uint gid { get; set; }
        public SetTag(Set f,string name,string g,string tag)
        {
            InitializeComponent();
            Plugin = f;
            gid = Convert.ToUInt32(g);
            this.Text = name + "(" + g + ")";
            int i = 0;
            int Left = 20;
            int Top = 10;
            int Height = 100;
            List<string> tags = new List<string>(tag.Split('|'));
            foreach (KeyValuePair<string,string>kv in f.Plugin.Type)
            {
                CheckBox box = new CheckBox();
                box.Name = kv.Value;
                box.Text = kv.Key;
                if (tags.IndexOf(kv.Key) > -1)
                {
                    box.Checked = true;
                }
                if( i%2==1)
                {
                    Left = this.Width / 2;
                }
                else
                {
                    Left = 10;
                    Height += 20;
                }
                int n = Convert.ToInt32(Math.Floor((decimal)i / 2));
                
                box.Location = new Point(Left, Top+(n*20));
                this.Controls.Add(box);
                i++;
            }
            this.Height = Height;
            button1.Top = Height - button1.Height-40;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> txtList = new List<string>();
            foreach (KeyValuePair<string, string> kv in Plugin.Plugin.Type)
            {
                Control[] box = this.Controls.Find(kv.Value,false);
                CheckBox b = (CheckBox)box[0];
                if (b.Checked)
                {
                    txtList.Add(kv.Key);
                    Plugin.Plugin.SaveTag(gid, kv.Key);
                }
            }
            
            string txt = string.Join("|", txtList.ToArray());
            //  string txt = richTextBox1.Text.Replace("\n", "|").Replace("\t", "").Replace("\r", "");
              Plugin.Plugin.save(gid,txt);
              Plugin.SetTagData(gid,txt);
            this.Close();
        }
    }
}
