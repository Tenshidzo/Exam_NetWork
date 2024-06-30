using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Exam_NetWork
{
    internal class Program
    {
        static List<TcpClient> clients = new List<TcpClient>();
        static Dictionary<TcpClient, string> clientChoices = new Dictionary<TcpClient, string>();
        static int round = 0;
        static Dictionary<TcpClient, int> scores = new Dictionary<TcpClient, int>();

        static async Task Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 8888);
            server.Start();
            Console.WriteLine("Server started...");

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                clients.Add(client);
                scores[client] = 0;
                Console.WriteLine("Client connected...");

                _ = HandleClientAsync(client);

                if (clients.Count == 2)
                {
                    if(round == 0) { 
                    NotifyClients("Game started. Make your choice: Rock, Paper, or Scissors.");
                    }
                }
            }
        }

        static async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            while (true)
            {
                int bytesRead;
                try
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading from client: {ex.Message}");
                    break;
                }

                if (bytesRead == 0) break;

                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                clientChoices[client] = message;
                Console.WriteLine($"Received choice from client: {message}");

                if (clientChoices.Count == 2)
                {
                    round++;
                    EvaluateRound();
                    clientChoices.Clear();

                    if (round == 5)
                    {
                        NotifyClients("Game over.");
                        SendScores();
                        break;
                    }
                    else
                    {
                        SendScores();
                        NotifyClients(" Next round.");
                    }
                }
            }

            client.Close();
            clients.Remove(client);
        }

        static void EvaluateRound()
        {
            var client1 = clients[0];
            var client2 = clients[1];
            var choice1 = clientChoices[client1];
            var choice2 = clientChoices[client2];

            string result;

            if (choice1 == choice2)
            {
                result = "Draw";
            }
            else if ((choice1 == "Rock" && choice2 == "Scissors") ||
                     (choice1 == "Paper" && choice2 == "Rock") ||
                     (choice1 == "Scissors" && choice2 == "Paper"))
            {
                result = "Client1 wins";
                scores[client1]++;
            }
            else
            {
                result = "Client2 wins";
                scores[client2]++;
            }

            NotifyClients(result);
        }

        static void NotifyClients(string message)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);

            foreach (var client in clients)
            {
                NetworkStream stream = client.GetStream();
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        static void SendScores()
        {
            string scoreMessage = $"Scores: Client 1 = {scores[clients[0]]}, Client 2 = {scores[clients[1]]} ";
            NotifyClients(scoreMessage);
            Console.WriteLine(scoreMessage);
        }
    }
}

