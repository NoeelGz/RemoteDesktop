using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Remote
{
    public enum ButtonCode { Unknown, LButtonDown, LButtonUp, RButtonDown, RButtonUp }

    class Client: SocketBase
    {
        System.Windows.Window mainWindow;
        System.Windows.FrameworkElement viewport;

        ButtonCode mousePressed = ButtonCode.Unknown;
        Key keyPressed = Key.Help;

        public Client(string ip)
        {
            mainWindow = System.Windows.Application.Current.MainWindow;
            viewport = mainWindow.FindName("viewport") as System.Windows.FrameworkElement;

            Start(ip);
        }

        public void Start(string ip)
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), 5533), new AsyncCallback(ConnectCallback), null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                socket.EndConnect(ar);

                ViewModel.Instance.Panel = PanelType.Client;
                ViewModel.Instance.IsConnected = true;

                Console.WriteLine("Connected");
                new Thread(Listen) { IsBackground = true }.Start();
                BeginReceive();
            }
            catch (Exception)
            {
                throw;
            }
        }

        void Listen()
        {
            long tick = 0;
            int fps = 0;

            while (true)
            {
                if(lastPacket != null)
                {
                    //var packet = receivedPackets.Dequeue();
                    var packet = lastPacket;
                    lastPacket = null;

                    if(packet.type == PacketType.Frame)
                    {
                        Draw(packet.buffer);
                        fps++;
                    }
                }

                long tick1 = Tools.GetCurrentTime();
                if(tick1 - tick > 1000)
                {
                    ViewModel.Instance.Fps = fps;
                    fps = 0;
                    tick = tick1;
                }
            }
        }

        void Draw(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                ViewModel.Instance.Img = Tools.GetImage(stream);
            }
        }
    }

    [ProtoBuf.ProtoContract]
    struct CursorPoint
    {
        [ProtoBuf.ProtoMember(1)]
        public double x { get; set; }
        [ProtoBuf.ProtoMember(2)]
        public double y { get; set; }

        public CursorPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
            
        }

        public bool IsZero()
        {
            return (x == 0 && y == 0);
        }

        public static bool operator ==(CursorPoint a, CursorPoint b)
        {
            return (a.x == b.x && a.y == b.y);
        }

        public static bool operator !=(CursorPoint a, CursorPoint b)
        {
            return (a.x != b.x || a.y != b.y);
        }

    }

    [ProtoBuf.ProtoContract]
    class OptionsPacket
    {
        [ProtoBuf.ProtoMember(1)]
        public int quality { get; set; }
        [ProtoBuf.ProtoMember(2)]
        public int resolution { get; set; }
        [ProtoBuf.ProtoMember(3)]
        public bool gDefault { get; set; }
    }
}
