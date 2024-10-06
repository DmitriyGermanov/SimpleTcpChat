using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleTcpChat
{
    internal class Program
    {
        private static readonly ClientManager _clientManager = new();
        static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            IPEndPoint localIP = new(IPAddress.Any, 6577);

            var listenTask = ListenForClientsAsync(localIP, cts.Token);

            Console.WriteLine("Нажмите любую клавишу для завершения...");
            Console.ReadKey();

            cts.Cancel();
            Console.WriteLine("Завершаем сервер...");

            await listenTask;

            Console.WriteLine("Сервер завершил работу.");
        }

        async static Task ListenForClientsAsync(IPEndPoint localIP, CancellationToken token)
        {
            using var listener = new TcpListener(localIP);
            listener.Start();
            Console.WriteLine("Сервер запущен и ожидает подключения клиентов...");

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var socket = await listener.AcceptSocketAsync(token);
                    _ = HandleClientAsync(socket, token);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Операция ожидания подключения отменена.");
            }
            finally
            {
                listener.Stop();
                Console.WriteLine("Лисенер остановлен.");
            }
        }

        async static Task HandleClientAsync(Socket socket, CancellationToken token)
        {
            Console.WriteLine("Клиент подключен.");
            string clientName = String.Empty;
            await SendMessageToClientAsync(socket, "Введите Ник:");
            bool flag = false;
            try
            {
                while (!token.IsCancellationRequested)
                {
                if (flag)
                {
                    await SendMessageToClientAsync(socket, "Введите сообщение:");
                }
                    byte[] buffer = new byte[1024];

                    var receivedBytes = await socket.ReceiveAsync(buffer, SocketFlags.None, token);

                    if (receivedBytes == 0)
                    {
                        Console.WriteLine("Клиент отключился.");
                        if (clientName != String.Empty)
                        {
                            _clientManager.RemoveClient(clientName);
                        }
                        break;
                    }
                    string message = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    if (!flag)
                    {
                        clientName = message;
                        _clientManager.AddNewClient(clientName, socket);
                        flag = true;
                    }
                    else
                    {
                        foreach (var client in _clientManager.ClientDict)
                        {
                            if (!client.Key.Equals(clientName))
                            {
                                await SendMessageToClientAsync(client.Value, $"{clientName}: {message}");
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Операция приема данных отменена.");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Ошибка сокета: {ex.Message}");
            }
            finally
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                Console.WriteLine("Соединение с клиентом закрыто.");
            }
        }
        async static Task SendMessageToClientAsync(Socket socket, String message)
        {
            var buffer = Encoding.UTF8.GetBytes(message + "\n");
            await socket.SendAsync(buffer, SocketFlags.None);
        }
    }
}
