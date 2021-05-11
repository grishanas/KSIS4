using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

namespace WindowsService1
{
    class Proxy1
    {

        private int port;
        private string host;
        private Socket TCPSocket;
        private int MaxListen = 10;
        public const int BUFFER_LENGTH = 4048;
        public string[] BlackList;

        public ConcurrentQueue<string> message = new ConcurrentQueue<string>();

        public Proxy1(string host, int port,string[] BLackList)
        {
            this.host = host;
            this.port = port;
            this.BlackList = BLackList;
            TCPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TCPSocket.Bind(new IPEndPoint(IPAddress.Parse(this.host), port));
            
        }

        public void Listen()
        {
            TCPSocket.Listen(MaxListen);
        }
        
        public Socket Accept()
        {
            return TCPSocket.Accept();
        }
        public void LogTr()
        {
            string mes;
            while (true)
            {
                try
                {
                    if (message.Count != 0)
                        while (message.TryPeek(out mes))
                        {

                            Log(mes);
                            message.TryDequeue(out mes);
                        }
                }
                catch
                {
                    return;
                }
                Thread.Sleep(10);
            }
        }



        public void RecvData(Socket client)
        {
            NetworkStream browserSream = new NetworkStream(client);
            var buf = new byte[BUFFER_LENGTH];
            int inputLength = 0;
            while (true)
            {
                if (!browserSream.CanRead)
                    return;
                try
                {
                    inputLength = browserSream.Read(buf, 0, buf.Length);
                    //message.Enqueue($"Recive data");
                }
                catch (IOException)
                {
                    return;
                }
                HTTPserv(buf, browserSream, client, inputLength);
            }
        }

        public bool IsInBlackList(string host)
        {
           /* message.Enqueue(host);
            if (BlackList == null)
                return false;
            foreach (var i in BlackList)
            {
                if (host.Contains(i) && (i.Length > 0))
                    return true;
            }*/
            return false;
        }

        public byte[] ConvertFullUrlToReal(byte[] input)
        {
            string buffer = Encoding.ASCII.GetString(input);
            Regex regex = new Regex(@"http:\/\/[a-z0-9а-яё\:\.]*");
            MatchCollection matches = regex.Matches(buffer);
            if (matches.Count != 0)
            {
                string host = matches[0].Value;


                buffer = buffer.Replace(host, "");
                input = Encoding.ASCII.GetBytes(buffer);
            }
            return input;
        }

        public const int ERROR_BUFFER = 1024;

        private byte[] getErrorPage()
        {
            string head = $"HTTP/1.1 403 Forbidden\r\nContent-Type: text/html\r\nContent-Length: {ERROR_BUFFER}\r\n\r\n";
            byte[] headBuffer = Encoding.UTF8.GetBytes(head);
            byte[] result = new byte[ERROR_BUFFER + head.Length];
            byte[] page = Encoding.UTF8.GetBytes("<h1>Forbidden</h1><br> You don't have permission");
            Buffer.BlockCopy(headBuffer, 0, result, 0, head.Length);
            Buffer.BlockCopy(page, 0, result, head.Length, page.Length);
            return result;
        }

        public void Log(string message)
        {
            using (StreamWriter sw = new StreamWriter("F://1.txt", true, System.Text.Encoding.Default))
            {
                sw.WriteLine(message);
            }
        }


        public void HTTPserv(byte[] buf, NetworkStream browser, Socket client, int Length)
        {
            try
            {
                //message.Enqueue("HTTP start work");
                string[] temp = Encoding.ASCII.GetString(buf).Trim().Split(new char[] { '\r', '\n' });

                string req = temp.FirstOrDefault(x => x.Contains("Host"));
               // if (req == null)
               //     return;
                if(IsInBlackList(req))
                {
                    req = req.Substring(req.IndexOf(":") + 2);
                    message.Enqueue(req);
                    //Console.WriteLine($"\n{req}:This site in black list");
                    message.Enqueue($"\n{req}:This site in black list");
                    var ErrorePage = getErrorPage();
                    browser.Write(ErrorePage, 0, ErrorePage.Length);



                    return;

                }
                req = req.Substring(req.IndexOf(":") + 2);
                string[] port = req.Trim().Split(new char[] { ':' });

                Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPHostEntry iPHostEntry = Dns.GetHostEntry(port[0]);
                if (port.Length == 2)
                {

                    var ip1 = new IPEndPoint(iPHostEntry.AddressList[0], int.Parse(port[1]));

                    server.Connect(ip1);
                }
                else
                {
                    var ip1 = new IPEndPoint(iPHostEntry.AddressList[0], 80);
                    server.Connect(ip1);
                }
                NetworkStream servStream = new NetworkStream(server);

                
                servStream.Write(ConvertFullUrlToReal(buf), 0, Length);
                var ServerAnsver = new byte[BUFFER_LENGTH];

               
                servStream.Read(ServerAnsver, 0, ServerAnsver.Length);
                
                browser.Write(ServerAnsver, 0, ServerAnsver.Length);

                string[] head = Encoding.UTF8.GetString(ServerAnsver).Split(new char[] { '\r', '\n' });

                string ResponseCode = head[0].Substring(head[0].IndexOf(" ") + 1);
                //Console.WriteLine($"\n{req} {ResponseCode}");
                message.Enqueue($"\n{req} {ResponseCode}");
                servStream.CopyTo(browser);

            }
            catch //(Exception e)
            {
                // Console.WriteLine(e);
                return;
            }
            finally
            {
                client.Dispose();
            }

        }

    }
}
