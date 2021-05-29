using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Threading;
using SharpDX.DXGI;
using SharpDX;
using SharpDX.Direct3D11;

namespace Remote
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new ViewModel();

            clientBtn.Click += (s,e) => Client();
            serverBtn.Click += (s, e) => Server();
            iptarg.KeyDown += Iptarg_KeyDown;


            //new Thread(Test).Start();         
        }

        void Test()
        {
            var capturer = new ScreenCapturer();

            var last = Tools.GetCurrentTime();
            int frames = 0;
            long ms = 0;
            int interval = 0;

            while (true)
            {
                var tick1 = Tools.GetCurrentTime();
                byte[] buffer = capturer.CaptureScreen(75);
                if (buffer != null) frames++;        
                var tick2 = Tools.GetCurrentTime();
                if (tick2 - last > 1000)
                {
                    ViewModel.Instance.Fps = frames;
                    ViewModel.Instance.Ms = (ms == 0 || interval == 0) ? 0 : (int)(ms / interval);

                    frames = 0;
                    ms = 0;
                    interval = 0;
                    last = tick2;
                }
                else
                {
                    ms += tick2 - tick1;
                    interval++;
                }
            }


        }

        void Client()
        {
            ViewModel.Instance.Panel = PanelType.ClientMenu;
        }

        void Server()
        {
            ViewModel.Instance.Panel = PanelType.Server;

            Server s = new Server();
        }

        private void Iptarg_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var input = (System.Windows.Controls.TextBox)sender;

            if(e.Key == Key.Enter)
            {
                ViewModel.Instance.Panel = PanelType.Client;
                Debug.WriteLine(input.Text);
                Client client = new Client(input.Text);
            }
        }
    }
}
