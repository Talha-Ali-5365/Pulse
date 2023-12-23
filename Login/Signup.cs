using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Security.Cryptography;

namespace Login
{
    public partial class Signup : Form
    {
        public Signup()
        {
            InitializeComponent();
        }

        private void Signup_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private static string EncryptString(string text, string keyString)
        {
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        var iv = aesAlg.IV;

                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text;
            string password = textBox2.Text;

            if (username.Length < 1 || username.Length > 15 || !Regex.IsMatch(username, @"^[a-zA-Z0-9]+$"))
            {
                MessageBox.Show("Invalid username. Username must be 1-15 characters long and cannot contain special characters or spaces.");
                return;
            }

            if (password.Length < 6 || password.Length > 20 || !Regex.IsMatch(password, @"^(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{6,20}$"))
            {
                MessageBox.Show("Invalid password. Password must be 6-20 characters long, contain at least one uppercase letter and one number, and cannot contain special characters or spaces.");
                return;
            }
            if (!File.Exists("users.enc"))
            {
                File.Create("users.enc").Close();
            }
            if (File.ReadLines("users.enc").Any(line => line.Split('|')[0] == username))
            {
                MessageBox.Show("Username already exists. Please choose a different username.");
                return;
            }

            string key = "b14ca5898a4e4133bbce2ea2315a1916";
            string encryptedPassword = EncryptString(password, key);
            using (StreamWriter sw = File.AppendText("users.enc"))
            {
                sw.WriteLine(username + "|" + encryptedPassword);
            }

            MessageBox.Show("Sign up successful!");
            var page = new Form1();
            page.Show();
            this.Hide();
        }
        //private void button1_Click(object sender, EventArgs e)
        //{
        //    string username = textBox1.Text;
        //    string password = textBox2.Text;

        //    // Validate username
        //    if (username.Length < 1 || username.Length > 15 || !Regex.IsMatch(username, @"^[a-zA-Z0-9]+$"))
        //    {
        //        MessageBox.Show("Invalid username. Username must be 1-15 characters long and cannot contain special characters or spaces.");
        //        return;
        //    }

        //    // Validate password,"Error"
        //    if (password.Length < 6 || password.Length > 20 || !Regex.IsMatch(password, @"^(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{6,20}$"))
        //    {
        //        MessageBox.Show("Invalid password. Password must be 6-20 characters long, contain at least one uppercase letter and one number, and cannot contain special characters or spaces.", "Error");
        //        return;
        //    }

        //    // Check if username already exists
        //    if (File.ReadLines("users.txt").Any(line => line.Split('|')[0] == username))
        //    {
        //        MessageBox.Show("Username already exists. Please choose a different username.");
        //        return;
        //    }

        //    // Write username and password to file
        //    using (StreamWriter sw = File.AppendText("users.txt"))
        //    {
        //        sw.WriteLine(username + "|" + password);
        //    }

        //    MessageBox.Show("Sign up successful!");
        //    var page = new Form1();
        //    page.Show();
        //    this.Hide();
        //}

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
