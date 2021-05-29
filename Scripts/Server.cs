using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Input;

namespace Remote
{
    class Server: SocketBase
    {
        ScreenCapturer capturer;

        Socket client;

        public Server()
        {
            capturer = new ScreenCapturer();

            Start();
        }

        void Start()
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Any, 5533));
                socket.Listen(0);
                socket.BeginAccept(new AsyncCallback(AcceptCallback), null);

                Debug.WriteLine("Server started");
            }
            catch (Exception)
            {
                throw;
            }
        }

        void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                client = socket.EndAccept(ar);
                Debug.WriteLine("Connected");

                new Thread(Broadcast) { IsBackground = true }.Start();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Broadcast()
        {
            long tick = 0;
            int fps = 0;

            while (true)
            {
                if(CaptureFrame()) fps ++;

                long tick1 = Tools.GetCurrentTime();
                if (tick1 - tick > 1000)
                {
                    ViewModel.Instance.Fps = fps;
                    fps = 0;
                    tick = tick1;
                }
            }
        }

        CursorPoint[] sizes = new CursorPoint[] { new CursorPoint(1280, 720), new CursorPoint(854, 480), new CursorPoint(640, 360), new CursorPoint(426, 240) };
        int[] quality = new int[] { 100, 75, 40, 15 };

        bool CaptureFrame()
        {
            byte[] buffer = capturer.CaptureScreen(640,360);

            if (buffer != null)
            {
                SendRaw(buffer, PacketType.Frame, client);
                return true;
            }
            return false;
        }
    }
}
