using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Client.App
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var textForSend = string.Empty;
            while (true)
            {
              textForSend=  Console.ReadLine();
                if (textForSend.ToLower().Contains("exitfatal"))
                {
                   break;
                }
               await ConnectAndSend(textForSend);
            }
        }
        static async Task ConnectAndSend(string message)
        {

            try
            {
                int port = 7070;
                TcpClient client = new TcpClient("127.0.0.1", port);

                Byte[] data = System.Text.Encoding.UTF8.GetBytes(message);

                
                NetworkStream stream = client.GetStream();
              await  stream.WriteAsync(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                data = new Byte[1024];
                String responseData = String.Empty;

                Int32 bytes =await stream.ReadAsync(data, 0, data.Length);
                responseData = System.Text.Encoding.UTF8.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);

                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

           
        }
    }
}
