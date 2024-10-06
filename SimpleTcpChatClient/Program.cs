using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        IPEndPoint iPEndPoint = new(IPAddress.Parse("127.0.0.1"), 6577);

        using TcpClient client = new();

        CancellationTokenSource ctsForDots = new();
        CancellationTokenSource cts = new();

        Console.Write("Выполняется попытка подкючения к серверу!");
        Task connectTask = ConnectAsync(client, iPEndPoint, cts.Token);
        Task printingTask = PrintDotsAsync(ctsForDots.Token);

        try
        {
            await connectTask;
            ctsForDots.Cancel();
            await printingTask;
            
            Task recieve = ReceiveMessagesFromServerAsync(client, cts.Token);
               while (!cts.Token.IsCancellationRequested)
                {
                    string message = Console.ReadLine() ?? "";
                if (message.Equals("Выход"))
                {
                    await cts.CancelAsync();
                    recieve.Wait();
                }
                    await SendMessageToServerAsync(client, message, cts.Token);
                }
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка подключения: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }

    private static async Task ReceiveMessagesFromServerAsync(TcpClient tcpClient, CancellationToken cts)
    {
        using var stream = tcpClient.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        while (!cts.IsCancellationRequested)
        {
            string? message = await reader.ReadLineAsync(cts);

            if (message == null)
            {
                Console.WriteLine("Соединение разорвано.");
                break;
            }

            Console.WriteLine(message);
        }
    }

    static async Task ConnectAsync(TcpClient client, IPEndPoint ep, CancellationToken cancellationToken)
    {
        await client.ConnectAsync(ep);
        Console.WriteLine("\nПодключение успешно выполнено!");

    }

    static async Task PrintDotsAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(2000);
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000);
            Console.Write(".");
        }
    }

    async static Task SendMessageToServerAsync(TcpClient client, String message, CancellationToken cts)
    {
        var buffer = Encoding.UTF8.GetBytes(message + "\n");
        await client.Client.SendAsync(buffer, SocketFlags.None, cts);
    }

}