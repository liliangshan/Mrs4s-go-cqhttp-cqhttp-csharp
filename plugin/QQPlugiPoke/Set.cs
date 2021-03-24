using System.Drawing;
using System.Windows.Forms;

namespace QQPlugiPoke
{
    public partial class Set : Form
    {
        poke Plugin { get; set; }
        TextBoxEx textBox1 = new TextBoxEx();
        TextBoxEx textBox2 = new TextBoxEx();
        TextBoxEx textBox3 = new TextBoxEx();
        public Set(poke f)
        {
            Plugin = f;
            InitializeComponent();
            this.Icon = QQRobotFramework.Static.icon;
            this.Text = "戳一戳";
            
            
         
            textBox1.Location = new Point(21, 20);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(198, 21);
            textBox1.TabIndex = 0;
            textBox1.PlaceHolderStr = "输入Q群号";
            if (Plugin.Config[0] > 0)
            {
                textBox1.Text = Plugin.Config[0].ToString();
            }
            Controls.Add(textBox1);
            


            textBox2.Location = new Point(21, 46);
            textBox2.Name = "textBox2";
            textBox2.Size = new System.Drawing.Size(198, 21);
            textBox2.TabIndex = 0;
            textBox2.PlaceHolderStr = "输入Q用户号";
            if (Plugin.Config[1] > 0)
            {
                textBox2.Text = Plugin.Config[1].ToString();
            }
            Controls.Add(textBox2);

            textBox3.Location = new Point(21, 71);
            textBox3.Name = "textBox3";
            textBox3.Size = new System.Drawing.Size(198, 21);
            textBox3.TabIndex = 0;
            textBox3.PlaceHolderStr = "间隔时间（秒）";
            if (Plugin.Config[2] > 0)
            {
                textBox3.Text = (Plugin.Config[2]/1000).ToString();
            }
            Controls.Add(textBox3);

            Button button = new Button();
            button.Location = new Point(21, 100);
            button.Name = "button1";
            button.Size = new System.Drawing.Size(198, 25);
            button.TabIndex = 0;
            button.Text = "提交";
            button.Click += Button_Click;
            Controls.Add(button);

        }

        private void Button_Click(object sender, System.EventArgs e)
        {
            
            if (textBox1.Text == "")
            {
                Plugin.Config[0] = 0;
            }
            else
            {
                Plugin.Config[0] = System.Convert.ToUInt32(textBox1.Text);
            }
            if (textBox2.Text == "")
            {
                Plugin.Config[1] = 0;
            }
            else
            {
                Plugin.Config[1] = System.Convert.ToUInt32(textBox2.Text);
            }
            if (textBox3.Text == "")
            {
                Plugin.Config[2] = 10000;
            }
            else
            {
                uint Time = System.Convert.ToUInt32(textBox3.Text);
                if (Time < 10)
                {
                    MessageBox.Show("最少间隔10秒", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Plugin.Config[2] = Time * 1000;
            }
            System.IO.File.WriteAllLines(QQRobotFramework.Robot.path + @"poke.config", new string[] { Plugin.Config[0].ToString(), Plugin.Config[1].ToString(), Plugin.Config[2].ToString() } );
            this.Close();
        }
    }
}
