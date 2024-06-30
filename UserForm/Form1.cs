using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UserForm
{
    public partial class Form1 : Form
    {
        TcpClient client;
        NetworkStream stream;
        CancellationTokenSource cts;

        public Form1()
        {
            InitializeComponent();
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient("127.0.0.1", 8888);
                stream = client.GetStream();
                MessageBox.Show("Connected to server!");
                cts = new CancellationTokenSource();
                await Task.Run(() => ReceiveData(cts.Token));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to server: {ex.Message}");
            }
        }

        private void btnRock_Click(object sender, EventArgs e)
        {
            
            SendChoice("Rock");
        }

        private void btnPaper_Click(object sender, EventArgs e)
        {
            
            SendChoice("Paper");
        }

        private void btnScissors_Click(object sender, EventArgs e)
        {
            
            SendChoice("Scissors");
        }

        private void SendChoice(string choice)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(choice);
            stream.Write(buffer, 0, buffer.Length);
        }

        private async Task ReceiveData(CancellationToken token)
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested)
                    {
                        Invoke(new Action(() => MessageBox.Show($"Error receiving data: {ex.Message}")));
                    }
                    break;
                }

                if (bytesRead == 0) break;

                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Invoke(new Action(() =>
                {
                    txtStatus.Text = message + Environment.NewLine;
                }));
            }
        }
        private void ClearStatus()
        {
            txtStatus.Clear();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            cts?.Cancel();
            stream?.Close();
            client?.Close();
        }
    }
}
