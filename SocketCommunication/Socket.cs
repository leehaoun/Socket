using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ReceiveClassTest
{
    class Socket
    {
        static byte[] receivebuffer;
        public class AsyncObject
        {
            public byte[] Buffer;
            public System.Net.Sockets.Socket WorkingSocket;
            public readonly int BufferSize;

            public AsyncObject(int buffersize)
            {
                BufferSize = buffersize;
                Buffer = new byte[(long)BufferSize];
            }
            
            public void ClearBuffer()
            {
                Array.Clear(Buffer, 0, BufferSize);
            }
        }

        public class MemServer
        {
            System.Net.Sockets.Socket mainsock;
            bool check = true;
            public void Start()
            {
                mainsock = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 5000);
                mainsock.Bind(serverEP);
                mainsock.Listen(10);//10개의 client까지 대기가능, 넘어서면 퇴짜놓음
                mainsock.BeginAccept(AcceptCallback, null);
            }
            public void Close()
            {
                if (mainsock != null)
                {
                    mainsock.Dispose();
                    mainsock.Close();
                }
            }
            void AcceptCallback(IAsyncResult ar)
            {
                try
                {
                    Console.WriteLine("Connected");

                    System.Net.Sockets.Socket client = mainsock.EndAccept(ar);
                    AsyncObject obj = new AsyncObject(256);
                    obj.WorkingSocket = client;

                    mainsock.BeginAccept(AcceptCallback, null);

                    client.BeginReceive(obj.Buffer, 0, obj.BufferSize, 0, DataReceived, obj);
                }
                catch (Exception e)
                {

                }
            }
            void DataReceived(IAsyncResult result)
            {
                BinaryFormatter bf = new BinaryFormatter();
                AsyncObject obj = (AsyncObject)result.AsyncState;
                MemoryStream ms = new MemoryStream(obj.Buffer);
                Defect defect = (Defect)bf.Deserialize(ms);

                Console.WriteLine("Received Index:" + defect.m_nDefectIndex + "Received Code" + defect.m_nDefectCode + "Received InspectID" + defect.m_strInspectionID);
                obj.WorkingSocket.BeginReceive(obj.Buffer, 0, obj.BufferSize, 0, DataReceived, obj);
            }
        }

        public class MemClient
        {
            System.Net.Sockets.Socket mainsock;
            public MemClient()
            {
                mainsock = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            public void Start()
            {
                mainsock.BeginConnect("10.0.0.10", 5000, new AsyncCallback(ConnectedCallback), mainsock);
            }
            public void Send(byte[] buffer)
            {
                mainsock.Send(buffer);
            }
            public bool Connected()
            {
                return mainsock.Connected;
            }
            void ConnectedCallback(IAsyncResult ar)
            {
                System.Net.Sockets.Socket client = (System.Net.Sockets.Socket)ar.AsyncState;
                if(client.Connected == false)
                {
                    mainsock.Close();
                    mainsock = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    mainsock.BeginConnect("10.0.0.10", 5000, new AsyncCallback(ConnectedCallback), mainsock);
                }
                else
                {
                    Console.WriteLine("Connected");
                    client.EndConnect(ar);
                }
            }
        }

        static void Main(string[] args)
        {

            MemServer server = new MemServer();
            MemClient client = new MemClient();
            ConsoleKeyInfo KeyInfo;
            Console.WriteLine("Mode를 입력해주세요 1:서버 2:클라이언트");
            String mode = Console.ReadLine();
            
            if (mode == "1")
            {
                server.Start();
                Console.WriteLine("Open Server!! Wait for Connect....");
                while (true)
                {
                    KeyInfo = Console.ReadKey();
                    if (KeyInfo.Key == ConsoleKey.Escape)
                    {
                        break;
                    }
                }
                server.Close();
                Console.WriteLine("Close Server...");
            }
            else if(mode == "2")
            {
                Defect defect = new Defect(1, "front", 2);
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();

                bf.Serialize(ms, defect);
                byte[] buffer = ms.GetBuffer();

                client.Start();

                while(true)
                {
                    KeyInfo = Console.ReadKey();
                    if (KeyInfo.Key == ConsoleKey.Enter)
                    {
                        if (client.Connected())
                        {
                            client.Send(buffer);
                            Console.WriteLine("Data send finished.");
                        }
                    }
                    if (KeyInfo.Key == ConsoleKey.Escape)
                    {
                        break;
                    }
                }
                Console.WriteLine("Open Client!! Wait for Connect....");
            }
            else 
            {
                return;
            }
            
        }



    }
}
