using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Login
{
    public partial class Page2 : Form
    {
        Page1 page1;
        string user = "";
        public Page2(Page1 page1,string s="")
        {
            user = s;
            this.page1 = page1;
            InitializeComponent();
        }

        private void flowLayoutPanel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            page1.Show();
            this.Hide();
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            var page = new Page3(page1,user);
            page.Show();
            this.Hide();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            page1.pictureBox2_Click(page1, EventArgs.Empty);
            Application.Exit();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp; *.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(@"D:\chat-app\profile.txt", openFileDialog.FileName);
                pictureBox8.Image = new Bitmap(openFileDialog.FileName);
            }
        }

        private void Page2_Load(object sender, EventArgs e)
        {
            label9.Text = user;
            if (System.IO.File.Exists(@"D:\chat-app\profile.txt"))
            {
                string imgPath = System.IO.File.ReadAllText(@"D:\chat-app\profile.txt");
                if (System.IO.File.Exists(imgPath))
                {
                    pictureBox8.Image = new Bitmap(imgPath);
                    pictureBox8.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("                       Credits\nTalha Ali: Team lead and developer (Talha@gmail.com)\nMoix Tanvir: UI/UX designer (Moiz@gmail.com)\nIrtaza: Architecture and UI/UX (Irtaza@gmail.com)\nMuneeb: Tester(Muneeb@gmail.com)","Help");
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            MessageBox.Show("https://docs.google.com/document/d/1-1gfIGtUBSnXooRJgbAwXSrycuQV9MG_/edit","Invite");
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
