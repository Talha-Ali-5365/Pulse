using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using System.IO;
using System.Security.Cryptography;

namespace Login
{
    public partial class Page1 : Form
    {
        public string user = "";
        private bool mouseDown;
        private Point lastLocation;
        private TcpListener server;
        private TcpClient client;
        private NetworkStream stream;
        private List<ClientInfo> clients = new List<ClientInfo>();

        public class ClientInfo
        {
            public TcpClient client;
            public NetworkStream stream;
            public string clientName;
        }

        public Page1(string u="")
        {
            user = u;
            InitializeComponent();
            this.MouseDown += new MouseEventHandler(Form1_MouseDown);
        this.MouseMove += new MouseEventHandler(Form1_MouseMove);
        this.MouseUp += new MouseEventHandler(Form1_MouseUp);
            StartServerOrConnect("127.0.0.1");
        }
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }
        private void StartServerOrConnect(string ipp)
        {
            List<string> ips = new List<string> { "127.0.0.1"};
            foreach (string ip in ips)
            {
                try
                {
                    // Try to connect to the server
                    client = new TcpClient();
                    IAsyncResult ar = client.BeginConnect(ip, 6969, null, null);
                    System.Threading.WaitHandle wh = ar.AsyncWaitHandle;

                    try
                    {
                        if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1), false))
                        {
                            client.Close();
                            throw new TimeoutException();
                        }

                        client.EndConnect(ar);
                    }
                    finally
                    {
                        wh.Close();
                    }

                    stream = client.GetStream();

                    // Send the client name to the server
                    byte[] buffer = Encoding.UTF8.GetBytes(user);
                    stream.Write(buffer, 0, buffer.Length);

                    // Start a new thread to receive messages from the server
                    Thread receiveThread = new Thread(ReceiveMessages);
                    receiveThread.Start();

                    // If connection is successful, exit the function
                    return;
                }
                catch (Exception)
                {
                    // If connection failed, try the next IP
                    continue;
                }
            }

            // If all connections failed, start the server
            StartServer();
        }


        private void StartServer()
        {
            //IPAddress localIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            //if (localIP != null)
            //{
            //    //MessageBox.Show("Local IP: " + localIP.ToString());
            //    server = new TcpListener(localIP, 6969);
            //    //server = new TcpListener(IPAddress.Parse("127.0.0.1"), 6969);
            //}
            //else
            //{
            //    MessageBox.Show("Can not find system IP.");
            //}
            server = new TcpListener(IPAddress.Parse("127.0.0.1"), 6969);
            server.Start();

            Thread serverThread = new Thread(() =>
            {
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string clientName = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    ClientInfo clientInfo = new ClientInfo
                    {
                        client = client,
                        stream = client.GetStream(),
                        clientName = clientName
                    };

                    clients.Add(clientInfo);

                    Thread clientThread = new Thread(() => HandleClient(clientInfo));
                    clientThread.Start();
                }
            });

            serverThread.Start();
        }

        private void HandleClient(ClientInfo clientInfo)
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = clientInfo.stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        break; // Client disconnected
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Broadcast the message to all clients including itself
                    BroadcastMessage($"{clientInfo.clientName}: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Clean up when client disconnects
                clientInfo.stream.Close();
                clientInfo.client.Close();
                clients.Remove(clientInfo);
                BroadcastMessage($"{clientInfo.clientName} has left the chat.");
            }
        }
        private void BroadcastMessage(string message)
        {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            byte[] key = aes.Key;
            byte[] IV = aes.IV;

            byte[] encrypted = EncryptStringToBytes_Aes(message, key, IV);
            byte[] buffer = new byte[encrypted.Length + key.Length + IV.Length];
            key.CopyTo(buffer, 0);
            IV.CopyTo(buffer, key.Length);
            encrypted.CopyTo(buffer, key.Length + IV.Length);

            foreach (var clientInfo in clients)
            {
                clientInfo.stream.Write(buffer, 0, buffer.Length);
            }

            // Update the RichTextBox in the UI thread for the server instance as well
            Invoke((MethodInvoker)delegate
            {
                richTextBox1.AppendText(message + "\n");
                richTextBox1.ScrollToCaret();
            });

            // Write the encrypted message to a text file
            string encryptedMessage = Convert.ToBase64String(buffer);
            using (StreamWriter sw = File.AppendText("messages.txt"))
            {
                sw.WriteLine(encryptedMessage);
            }
        }

        //private void BroadcastMessage(string message)
        //{
        //    AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
        //    byte[] key = aes.Key;
        //    byte[] IV = aes.IV;

        //    byte[] encrypted = EncryptStringToBytes_Aes(message, key, IV);
        //    byte[] buffer = new byte[encrypted.Length + key.Length + IV.Length];
        //    key.CopyTo(buffer, 0);
        //    IV.CopyTo(buffer, key.Length);
        //    encrypted.CopyTo(buffer, key.Length + IV.Length);

        //    foreach (var clientInfo in clients)
        //    {
        //        clientInfo.stream.Write(buffer, 0, buffer.Length);
        //    }

        //    // Update the RichTextBox in the UI thread for the server instance as well
        //    Invoke((MethodInvoker)delegate
        //    {
        //        richTextBox1.AppendText(message + "\n");
        //    });
        //}

        private void ReceiveMessages()
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        
                        int bytesRead;

                        do
                        {
                            bytesRead = stream.Read(buffer, 0, buffer.Length);
                            ms.Write(buffer, 0, bytesRead);
                        } while (stream.DataAvailable);

                        buffer = ms.ToArray();
                    }

                    byte[] key = buffer.Take(32).ToArray();
                    byte[] IV = buffer.Skip(32).Take(16).ToArray();
                    byte[] encrypted = buffer.Skip(48).ToArray();

                    string message = DecryptStringFromBytes_Aes(encrypted, key, IV);
                    Invoke((MethodInvoker)delegate
                    {
                        richTextBox1.AppendText(message + "\n");
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error receiving messages: {ex.Message}");
            }
        }


        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] encrypted;

            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string message = richTextBox2.Text;

            if (!string.IsNullOrEmpty(message)) 
            {
                // Send the message to the server or broadcast it to all clients
                byte[] buffer = Encoding.UTF8.GetBytes(message);

                if (stream != null)
                {
                    stream.Write(buffer, 0, buffer.Length);
                }

                if (server != null) // If this instance is the server
                {
                    //BroadcastMessage($"{user}: {message}");
                    BroadcastMessage($"{user} : {message}");
                }

                richTextBox2.Clear();
            }
        }

        private void Page1_FormClosing(object sender, FormClosingEventArgs e)
        {
           
        }
        private void Page1_Load(object sender, EventArgs e)
        {
            string key = "b14ca5898a4e4133bbce2ea2315a1916"; // Example key, replace with your own

            if (File.Exists("messages.txt"))
            {
                using (StreamReader sr = new StreamReader("messages.txt"))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        byte[] buffer = Convert.FromBase64String(line);
                        byte[] keyBytes = new byte[32];
                        byte[] IV = new byte[16];
                        byte[] encrypted = new byte[buffer.Length - keyBytes.Length - IV.Length];

                        Buffer.BlockCopy(buffer, 0, keyBytes, 0, keyBytes.Length);
                        Buffer.BlockCopy(buffer, keyBytes.Length, IV, 0, IV.Length);
                        Buffer.BlockCopy(buffer, keyBytes.Length + IV.Length, encrypted, 0, encrypted.Length);

                        string message = DecryptStringFromBytes_Aes(encrypted, keyBytes, IV);
                        richTextBox1.AppendText(message + "\n");
                    }
                }
            }
            richTextBox1.ScrollToCaret(); 
            label1.Text = user;
            if (System.IO.File.Exists(@"D:\chat-app\profile.txt"))
            {
                string imgPath = System.IO.File.ReadAllText(@"D:\chat-app\profile.txt");
                if (System.IO.File.Exists(imgPath))
                {
                    pictureBox8.Image = new Bitmap(imgPath);
                    pictureBox8.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }
            if (System.IO.File.Exists(@"D:\chat-app\profile1.txt"))
            {
                string imgPath = System.IO.File.ReadAllText(@"D:\chat-app\profile1.txt");
                if (System.IO.File.Exists(imgPath))
                {
                    pictureBox5.Image = new Bitmap(imgPath);
                    pictureBox5.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }
        }

        private void flowLayoutPanel8_Paint(object sender, PaintEventArgs e)
        {

        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        public void pictureBox2_Click(object sender, EventArgs e)
        {
            server.Stop();
            foreach (ClientInfo clientInfo in clients)
            {
                clientInfo.client.Close();
            }
            Application.Exit();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            var page = new Page3(this,user);
            page.Show();
            this.Hide();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            var page = new Page2(this,user);
            page.Show();
            this.Hide();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            server.Stop();
            foreach (ClientInfo clientInfo in clients)
            {
                clientInfo.client.Close();
            }
            string friendIp = "127.0.0.1";

            StartServerOrConnect(friendIp);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (client != null)
            {
                client.Close();
                client = null;
            }

            string friendIp = "127.0.0.1";

            StartServerOrConnect(friendIp);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            StartServerOrConnect("192.168.18.177");
        }

        private void flowLayoutPanel7_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp; *.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(@"D:\chat-app\profile1.txt", openFileDialog.FileName);
                pictureBox5.Image = new Bitmap(openFileDialog.FileName);
            }
        }
    }
}