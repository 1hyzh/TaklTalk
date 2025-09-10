using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;

class Program
{
    const int Port = 12345;
    static string passcode = "";
    static ConcurrentDictionary<string, TcpClient> connectedClients = new();

    static void Main()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[*] TaklTalk v0.5\n");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Magenta;

        Console.Write("Do you want to host or join? (host/join): ");
        Console.ResetColor();
        string mode = Console.ReadLine()?.Trim().ToLower();

        if (mode == "host")
        {
            HostServerAsync();
        }
        else if (mode == "join")
        {
            JoinServerAsync();
        }
        else
        {
            Console.WriteLine("Invalid option.");
        }
    }

    // ============================= HOST =============================
    static async Task HostServerAsync()
    {
        Console.Write("Choose your username: ");
        string hostUsername = Console.ReadLine()?.Trim();

        Console.Write("Set server passcode: ");
        passcode = Console.ReadLine()?.Trim();

        TcpListener listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();
        Console.WriteLine($"Server started on port {Port}. Waiting for clients...");

        _ = Task.Run(() => AcceptClientsAsync(listener));

        // Connect host as a client to their own server
        var hostClient = new TcpClient("127.0.0.1", Port);
        using NetworkStream stream = hostClient.GetStream();
        using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
        using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

        writer.WriteLine($"{hostUsername}|{passcode}");
        string response = reader.ReadLine();
        if (response != "AUTH_OK")
        {
            Console.WriteLine("Host failed to authenticate to own server.");
            return;
        }

        CancellationTokenSource cts = new();

        _ = Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                string? line = await reader.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(line))
                    Console.WriteLine(line);
            }
        });

        // Host sends messages
        while (true)
        {
            string? input = Console.ReadLine();
            if (input?.Trim().ToLower() == "/exit")
            {
                await writer.WriteLineAsync("/exit");
                break;
            }

            await writer.WriteLineAsync(input);
        }

        cts.Cancel();
        hostClient.Close();
        listener.Stop();
        Console.WriteLine("Server shut down.");
    }

    static async Task AcceptClientsAsync(TcpListener listener)
    {
        while (true)
        {
            TcpClient client;
            try
            {
                client = await listener.AcceptTcpClientAsync();
            }
            catch
            {
                break; // Listener stopped
            }

            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    static async Task HandleClientAsync(TcpClient client)
    {
        using NetworkStream stream = client.GetStream();
        using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
        using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

        // First line: username|passcode
        string? authLine = await reader.ReadLineAsync();
        if (string.IsNullOrEmpty(authLine) || !authLine.Contains('|'))
        {
            await writer.WriteLineAsync("AUTH_FAILED");
            client.Close();
            return;
        }

        var parts = authLine.Split('|');
        string username = parts[0];
        string clientPass = parts[1];

        if (clientPass != passcode || connectedClients.ContainsKey(username))
        {
            await writer.WriteLineAsync("AUTH_FAILED");
            client.Close();
            return;
        }

        await writer.WriteLineAsync("AUTH_OK");
        connectedClients[username] = client;

        Broadcast($"{username} has joined the chat.", "Server");

        try
        {
            while (true)
            {
                string? message = await reader.ReadLineAsync();
                if (message == null || message.Trim().ToLower() == "/exit")
                    break;

                Broadcast(message, username);
            }
        }
        catch { }

        connectedClients.TryRemove(username, out _);
        client.Close();
        Broadcast($"{username} has left the chat.", "Server");
    }

    static void Broadcast(string message, string sender)
    {
        foreach (var kv in connectedClients)
        {
            TcpClient client = kv.Value;
            try
            {
                using var writer = new StreamWriter(client.GetStream(), Encoding.UTF8, leaveOpen: true) { AutoFlush = true };
                string prefix = kv.Key == sender ? "You.> " : $"[{sender}]> ";
                writer.WriteLine($"{prefix}{message}");
            }
            catch { }
        }
    }

    // ============================= CLIENT =============================
    static async Task JoinServerAsync()
    {
        Console.Write("Enter host IP (e.g. 127.0.0.1): ");
        string ip = Console.ReadLine()?.Trim();

        Console.Write("Enter passcode: ");
        string pass = Console.ReadLine()?.Trim();

        Console.Write("Enter your username: ");
        string username = Console.ReadLine()?.Trim();

        var client = new TcpClient();
        try
        {
            client.Connect(IPAddress.Parse(ip), Port);
        }
        catch
        {
            Console.WriteLine("Failed to connect.");
            return;
        }

        using NetworkStream stream = client.GetStream();
        using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
        using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

        writer.WriteLine($"{username}|{pass}");

        string response = reader.ReadLine();
        if (response != "AUTH_OK")
        {
            Console.WriteLine("Authentication failed.");
            return;
        }

        Console.WriteLine("Connected! Type messages below. Type /exit to leave.");

        CancellationTokenSource cts = new();

        _ = Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                string? line = await reader.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(line))
                    Console.WriteLine(line);
            }
        });

        while (true)
        {
            string? input = Console.ReadLine();
            if (input?.Trim().ToLower() == "/exit")
            {
                await writer.WriteLineAsync("/exit");
                break;
            }

            await writer.WriteLineAsync(input);
        }

        cts.Cancel();
        client.Close();
    }
}
