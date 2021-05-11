using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
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
    public partial class KSIS_4 : ServiceBase
    {
        public KSIS_4()
        {
            InitializeComponent();
            
        }

        protected override void OnStart(string[] args)
        {
            Thread Start = new Thread(() => Man());
            Start.Start();
        }

        public static void Man()
        {
            string str;
            using (StreamReader fs = new StreamReader("F://config.txt"))
            {
                str = fs.ReadToEnd();
            }
            string[] BlackList = str.Trim().Split(new char[] { '\r', '\n' });
            Proxy1 proxy = new Proxy1("127.0.0.1", 777, BlackList);
            proxy.message.Enqueue("proxy -start");
            proxy.Listen();
            //proxy.message.Enqueue("Listen -start");
            Thread log = new Thread(proxy.LogTr);
            log.Start();
            //proxy.message.Enqueue("Log -start");
            while (true)
            {

                Socket Sock = proxy.Accept();
                //proxy.message.Enqueue("Accept new users");
                Thread thread = new Thread(() => proxy.RecvData(Sock));
                thread.Start();
                //proxy.message.Enqueue("Close new users");
            }
        }

        protected override void OnStop()
        {

        }
    }
}
