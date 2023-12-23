using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Login;

namespace Login
{
    public partial class Page3 : Form
    {
        Page1 page1;
        string user = "";
        public Page3(Page1 page1,string s="")
        {
            InitializeComponent();
            user = s;
            this.page1 = page1;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            var page = new Page2(page1,user);
            page.Show();
            this.Hide();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            page1.Show();
            this.Hide();

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            page1.pictureBox2_Click(page1, EventArgs.Empty);
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "Image files (.jpeg;.jpg;.png;.gif;.bmp)|*.jpeg;*.jpg;*.png;*.gif;*.bmp";

            openFileDialog1.ShowDialog();

            txtFilePath.Text = openFileDialog1.FileName;
            string path= txtFilePath.Text;
            pictureBox6.Image = new Bitmap(path);
            pictureBox6.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void Page3_Load(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(@"D:\chat-app\profile.txt"))
            {
                string imgPath = System.IO.File.ReadAllText(@"D:\chat-app\profile.txt");
                if (System.IO.File.Exists(imgPath))
                {
                    pictureBox6.Image = new Bitmap(imgPath);
                    pictureBox6.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(txtFilePath.Text != "")
            {
                string FileName = Path.GetFileName(txtFilePath.Text);

                string dir = Application.StartupPath.ToString() + "\\aaa\\";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if(Directory.Exists(dir))
                {
                    File.Copy(txtFilePath.Text, dir + "\\" + FileName, true);
                }


            }
        }

        private void flowLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private int currentIndex = 0;
        private string[] files;

        private void button3_Click(object sender, EventArgs e)
        {
           
        }

        

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            files = Directory.GetFiles(Application.StartupPath + "\\aaa\\") ;

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 2000; 
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (currentIndex < files.Length)
            {
                pictureBox5.Image = Image.FromStream(new MemoryStream(File.ReadAllBytes(files[currentIndex])));
                currentIndex++;
            }
            else
            {
                ((System.Windows.Forms.Timer)sender).Stop();
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {

        }
    }
}
