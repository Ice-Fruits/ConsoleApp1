// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;


using System;
using MySqlX.XDevAPI.Common;
using System.Text;
using ConsoleApp1;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        enum Command 
        { 
            Login = 10001, //登录
            Register, 
            Kickout, //踢出
            Listener = 1000000, 
        };

        private static Socket server = null;
        private static byte[] result = new byte[1024];
        private static Dictionary<string, Socket> sockets = new Dictionary<string, Socket>();
        static void Main(string[] args)
        {
            Console.WriteLine("服务器运行中");
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 8899);
            server.Bind(ipEndPoint);
            server.Listen(10);

            //server.BeginAccept(AcceptCallBack, server);

            Console.WriteLine("启动监听{0}成功", server.LocalEndPoint.ToString());
            //通过Clientsoket发送数据
            Thread myThread = new Thread(ListenClientConnect);
            myThread.Start();
            Console.ReadLine();

            while (true)
            {
                string info = Console.ReadLine();
                if (info.Equals("exit")) break;
            }
        }

//         static void AcceptCallBack(IAsyncResult ar)
//         {
//             Socket server = ar.AsyncState as Socket;
// 
//             Socket client = server.EndAccept(ar);
//             Console.WriteLine("进来1用户");
//         }

        static void ListenClientConnect()
        {
            while (true)
            {
                Socket clientSocket = server.Accept();
                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start(clientSocket);
            }
        }

        private static void ReceiveMessage(object clientSocket)
        {
            Socket myClientSocket = (Socket)clientSocket;
            while (true)
            {
                try
                {
                    //通过clientSocket接收数据
                    int receiveNumber = myClientSocket.Receive(result);
                    string strContent = Encoding.UTF8.GetString(result, 0, receiveNumber);
                    Console.WriteLine("接收客户端{0}消息{1}", myClientSocket.RemoteEndPoint.ToString(), strContent);
                    //开始解析
                    var array = strContent.Split(",");
                    if (array.Length > 0)
                    {
                        string str = array[0]+",";//回传值加上消息头
                        switch ((Command)int.Parse(array[0]))
                        {
                            case Command.Login:
                                {
                                    Console.WriteLine("开始登陆");
                                    string qudaoid = array[1];
                                    str += Class1.Login(qudaoid);
                                    //返回给客户端
                                    myClientSocket.Send(Encoding.UTF8.GetBytes(str));
                                }
                                break;
                            case Command.Register:
                                {
                                    Console.WriteLine("开始注册");
                                    string qudaoid = array[1];
                                    string name = array[2];
                                    str += Class1.Register(qudaoid, name);
                                    //返回给客户端
                                    myClientSocket.Send(Encoding.UTF8.GetBytes(str));
                                }
                                break;
                            case Command.Listener:
                                {
                                    Console.WriteLine("开始保存socket");
                                    string userid = array[1];
                                    //返回给客户端
                                    myClientSocket.Send(Encoding.UTF8.GetBytes(str));
                                    if (sockets.ContainsKey(userid))
                                    {
                                        str = (int)Command.Kickout + ",";
                                        sockets[userid].Send(Encoding.UTF8.GetBytes(str));
                                        sockets[userid].Shutdown(SocketShutdown.Both);
                                        sockets[userid].Close();
                                        sockets[userid] = myClientSocket;
                                    }
                                    else
                                    {
                                        sockets.Add(userid, myClientSocket);
                                    }
                                }
                                break;
                        }
                    }

                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    myClientSocket.Shutdown(SocketShutdown.Both);
                    myClientSocket.Close();
                    break;
                }
            }
        }
    }
}







