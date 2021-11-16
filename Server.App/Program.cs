using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Server.App
{
    class Program
    {
        public static string RootDir() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @".\"));
        static async Task Listen()
        {
            TcpListener server = null;

            try
            {
                int port = 7070;
                IPAddress local = IPAddress.Parse("127.0.0.1");
                server = new TcpListener(local, port);
                server.Start();
              
                
                Byte[] bytes = new Byte[1024];
                String data = null;
                var prefix = @$"..\..\..\Files\";
                while (true)
                {

                    TcpClient client = await server.AcceptTcpClientAsync();
                    Console.WriteLine("Ready for receive message!");

                    data = null;

                    NetworkStream stream = client.GetStream();

                    int i;
                    var st=new StringBuilder();
                    while ((i = await stream.ReadAsync(bytes, 0, bytes.Length)) != 0)
                    {
                        data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
                        byte[] msg = System.Text.Encoding.UTF8.GetBytes(data);
                        await stream.WriteAsync(msg, 0, msg.Length);
                       
                       
                    }
                   var time= DateTime.UtcNow;
                   var path = time.ToString("yyyy-MM-dd-tt-HH-mm");
                    if (time.Second > 30)
                    {
                        path += "-30.txt";
                    }
                    else
                    {
                        path += "-00.txt";
                    }
                    var mainPath= Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,prefix+path));
                    var lines = data.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                  
                    await File.AppendAllLinesAsync(mainPath, lines);

                    client.Close();
                }

            }
            catch (Exception e)
            {
               

            }
        }
        public static async Task CreateFileInfo()
        {
            var memory = new ConcurrentDictionary<string, string>();
            var prefix = @$"..\..\..\Files";
            var wordRegex = new Regex(@"\w*");
            while (true)
            {
                
                
               var files= Directory.GetFiles(prefix);
                
                foreach(var file in files)
                {
                    if (file.Contains("info"))
                    {
                        continue;
                    }
                    if (!memory.ContainsKey(file))
                    {
                        var builder = new StringBuilder();
                        var lines= await  File.ReadAllLinesAsync(file);
                        ConcurrentDictionary<string, int> allWordsInText = new ConcurrentDictionary<string, int>();
                        builder.AppendLine($"Total Lines:{lines.Length}");
                        foreach(var line in lines)
                        {
                             var matchCollection = wordRegex.Matches(line);
                             var words=  matchCollection.Cast<Match>().Where(d => d.Success).Where(x => !string.IsNullOrWhiteSpace(x.Value)).GroupBy(x => x.Value).ToDictionary(x => x.Key, e => e.ToList().Count);
                            foreach(var w in words)
                            {
                                if (allWordsInText.ContainsKey(w.Key))
                                {
                                    allWordsInText[w.Key] += words[w.Key];
                                }
                                else
                                {
                                    allWordsInText.TryAdd(w.Key, words[w.Key]);
                                }
                            }

                        }
                        var max = -1;
                        var maxword = string.Empty;
                        foreach(var word in allWordsInText)
                        {
                            if (word.Value > max)
                            {
                                max = word.Value;
                                maxword = word.Key;
                            }
                        }

                        builder.AppendLine($"Max Word:{maxword} with number of:{max}");
                        
                        memory.TryAdd(file, file);
                        builder.AppendLine("Max word in number lines:");
                        var regexMaxWord = new Regex($"\\b({maxword})\\b");
                        for (var i= 0; i < lines.Length; i++)
                        {
                            if (regexMaxWord.Match(lines[i]).Success)
                            {
                                builder.Append($"{i + 1},");
                            }
                        }
                        await File.WriteAllTextAsync(file + "-info.txt", builder.ToString());
                    }
                    
                }
                await Task.Delay(30000);
            }
            
        }
        static async Task Main(string[] args)
        {
            await Task.WhenAll(Listen(), CreateFileInfo());
        }

    }
}
