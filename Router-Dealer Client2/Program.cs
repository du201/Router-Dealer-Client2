using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using NetMQ;
using NetMQ.Sockets;

namespace Router_Dealer_Client2
{
    class Program
    {
        static void Main(string[] args)
        {
            const int delay = 5000; // millis
            var clientSocketPerThread = new ThreadLocal<DealerSocket>();
            using (var poller = new NetMQPoller())
            {
                // Start some threads, each with its own DealerSocket
                // to talk to the server socket. Creates lots of sockets,
                // but no nasty race conditions no shared state, each
                // thread has its own socket, happy days.
                //for (int i = 0; i < 2; i++)
                //{
                Task.Factory.StartNew(state =>
                {
                    DealerSocket client = null;
                    if (!clientSocketPerThread.IsValueCreated)
                    {
                        client = new DealerSocket();
                        client.Options.Identity =
                            Encoding.Unicode.GetBytes(state.ToString());
                        client.Connect("tcp://127.0.0.1:5000");
                        client.ReceiveReady += Client_ReceiveReady;
                        clientSocketPerThread.Value = client;
                        poller.Add(client);
                    }
                    else
                    {
                        client = clientSocketPerThread.Value;
                    }
                    while (true)
                    {
                        var messageToServer = new NetMQMessage();
                        messageToServer.AppendEmptyFrame();
                        messageToServer.Append("message from client 2");
                        Console.WriteLine("======================================");
                        Console.WriteLine(" OUTGOING MESSAGE TO SERVER ");
                        Console.WriteLine("======================================");
                        PrintFrames("Client Sending", messageToServer);
                        client.SendMultipartMessage(messageToServer);
                        Thread.Sleep(delay);
                    }
                }, string.Format("client 2"), TaskCreationOptions.LongRunning);
                //}
                // start the poller
                poller.RunAsync();
                /*
                DealerSocket client = new DealerSocket("tcp://127.0.0.1:5000");
                client.Options.Identity = Encoding.Unicode.GetBytes("1");
                client.Connect("tcp://127.0.0.1:5000");
                //client.ReceiveReady += Client_ReceiveReady;
                var messageToServer = new NetMQMessage();
                messageToServer.AppendEmptyFrame();
                messageToServer.Append("Hello server~");
                client.SendMultipartMessage(messageToServer);
                */
                while (true) ;
            }
        }
        public static void PrintFrames(string operationType, NetMQMessage message)
        {
            for (int i = 0; i < message.FrameCount; i++)
            {
                Console.WriteLine("{0} Socket : Frame[{1}] = {2}", operationType, i,
                    message[i].ConvertToString());
            }
        }

        public static void Client_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            //Console.WriteLine("received back");
            /*
            bool hasmore = false;
            e.Socket.Receive(out hasmore);
            if (hasmore)
            {
                string result = e.Socket.ReceiveFrameString(out hasmore);
                Console.WriteLine("REPLY {0}", result);
            }
            */
        }
    }
}
