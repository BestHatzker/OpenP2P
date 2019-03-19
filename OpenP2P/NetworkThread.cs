﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenP2P
{
    class NetworkThread
    {
        public const int MIN_POOL_COUNT = 10000;
        public const int BUFFER_LENGTH = 4000;
        public const int MAX_BUFFER_PACKET_COUNT = 1000;
        public static int MAX_SENDRATE_PERFRAME = 5000;
        public const int RECEIVE_TIMEOUT = 1000;

        public static int MAX_SEND_THREADS = 0;
        public static int MAX_RECV_THREADS = 0;

        //important to sleep more, since they are on infinite loops
        public const int EMPTY_SLEEP_TIME = 10;
        public const int MAXSEND_SLEEP_TIME = 0;
        
        public static NetworkStreamPool STREAMPOOL = new NetworkStreamPool(MIN_POOL_COUNT, BUFFER_LENGTH);

        //public static Queue<NetworkStream> SENDQUEUE = new Queue<NetworkStream>(MIN_POOL_COUNT);
        //public static Queue<NetworkStream> RECVQUEUE = new Queue<NetworkStream>(MIN_POOL_COUNT);
        public static ConcurrentBag<NetworkStream> SENDQUEUE = new ConcurrentBag<NetworkStream>();
        public static ConcurrentBag<NetworkStream> RECVQUEUE = new ConcurrentBag<NetworkStream>();
        public static List<Thread> SENDTHREADS = new List<Thread>();
        public static List<Thread> RECVTHREADS = new List<Thread>();

        public static void StartNetworkThreads(int sendThreads, int recvThreads)
        {
            MAX_SEND_THREADS = sendThreads;
            MAX_RECV_THREADS = recvThreads;

            for (int i = 0; i < sendThreads; i++)
            {
                SENDTHREADS.Add(new Thread(NetworkThread.SendThread));
                SENDTHREADS[i].Start();
            }
            for (int i = 0; i < recvThreads; i++)
            {
                RECVTHREADS.Add(new Thread(NetworkThread.RecvThread));
                RECVTHREADS[i].Start();
            }
        }

        public static void SendThread()
        {
            NetworkStream stream = null;
            int queueCount = 0;

            while (true)
            {
                //lock (SENDQUEUE)
                {
                    stream = null;
                    queueCount = SENDQUEUE.Count;
                    if (queueCount > 0)
                        SENDQUEUE.TryTake(out stream);
                        //stream = SENDQUEUE.Dequeue();
                }

                //sleep if empty, to avoid 100% cpu
                if (queueCount == 0 || stream == null)
                {
                    Thread.Sleep(EMPTY_SLEEP_TIME);
                    continue;
                }

                //avoid filling up the OS socket buffer or network card's RING buffer
                //important on cloud VPS/dedicated servers with lower buffer settings
                //google compute engine (cheapest one) gave packet loss at >75 sends/frame
                if (queueCount % MAX_SENDRATE_PERFRAME == 0)
                    Thread.Sleep(MAXSEND_SLEEP_TIME);

                stream.socket.SendInternal(stream);
            }
        }

        public static NetworkStream recvStream = null;
        public static void RecvThread()
        {
            NetworkStream stream = null;
            int queueCount = 0;

            while (true)
            {
                //lock (RECVQUEUE)
                {
                    stream = null;
                    queueCount = RECVQUEUE.Count;
                    if (queueCount > 0)
                        RECVQUEUE.TryTake(out stream);
                        //stream = RECVQUEUE.Dequeue();
                }
                
                //sleep if empty, to avoid 100% cpu
                if (queueCount == 0)
                {
                    Thread.Sleep(EMPTY_SLEEP_TIME);
                    continue;
                }
                
                stream.socket.ExecuteListen(stream);
            }
        }
    }
}
