using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using QQRobotFramework;
namespace PluginXQ
{
    public partial class Set : Form
    {
        Dictionary<uint, DataGridViewRow> Groups = new Dictionary<uint, DataGridViewRow>();
       public XQ Plugin { get; set; }
        public Set(XQ f)
        {
            Plugin = f;
            InitializeComponent();
            this.Text = Robot.name + "(" + QQRobotFramework.Robot.id + ")";
            Thread thread = new Thread(new ThreadStart(GroupList));
            thread.Start();
        }
        private void GroupList()
        {
            Thread.Sleep(100);
            Dictionary<uint, string> tags = Plugin.tag();
            GroupInfo[] data = Cluster.Get(true);

            this.BeginInvoke(new EventHandler(delegate {
                foreach (GroupInfo group in data)
                {
                    int index = dataGridView1.Rows.Add();
                    dataGridView1.Rows[index].Cells[0].Value = group.group_id;
                    dataGridView1.Rows[index].Cells[1].Value = group.group_name;
                    if (tags.ContainsKey(group.group_id))
                    {
                        dataGridView1.Rows[index].Cells[2].Value = tags[group.group_id];
                    }
                    else
                    {
                        dataGridView1.Rows[index].Cells[2].Value = "";
                    }
                    Groups.Add(group.group_id, dataGridView1.Rows[index]);
                }
            }));
        }

        internal void SetTagData(uint gid, string txt)
        {
            Groups[gid].Cells[2].Value = txt;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                string tag = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                string gid = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                string name = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                SetTag set = new SetTag(this, name, gid, tag);
                set.ShowDialog();
            }
        }
    }

   
}
