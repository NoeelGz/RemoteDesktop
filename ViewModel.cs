using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Remote
{
    public enum PanelType { Menu, Server, ClientMenu, Client}
    class ViewModel : RoxDev.ViewModelBase
    {
        public static ViewModel Instance { get; set; }

        public ViewModel()
        {
            Instance = this;

        }

        PanelType panel = PanelType.Menu;
        public PanelType Panel
        {
            get => panel;
            set
            {
                panel = value;
                OnPropertyChanged(nameof(this.Panel));
            }
        }

        BitmapImage img;
        public BitmapImage Img
        {
            get => img;
            set
            {
                img = value;
                OnPropertyChanged(nameof(this.Img));
            }
        }

        WriteableBitmap img2;
        public WriteableBitmap Img2
        {
            get => img2;
            set
            {
                img2 = value;
                OnPropertyChanged(nameof(this.Img2));
            }
        }


        int fps;
        public int Fps
        {
            get => fps;
            set
            {
                fps = value;
                OnPropertyChanged(nameof(this.Fps));
            }
        }

        int ms;
        public int Ms
        {
            get => ms;
            set
            {
                ms = value;
                OnPropertyChanged(nameof(this.Ms));
            }
        }

        double pointX;
        public double PointX
        {
            get => pointX;
            set
            {
                pointX = value;
                OnPropertyChanged(nameof(this.PointX));
            }
        }

        double pointY;
        public double PointY
        {
            get => pointY;
            set
            {
                pointY = value;
                OnPropertyChanged(nameof(this.PointY));
            }
        }

        int selectedRes;
        public int SelectedRes
        {
            get => selectedRes;
            set
            {
                selectedRes = value;
                OnPropertyChanged(nameof(this.SelectedRes));
                newChanges = true;
            }
        }

        int selectedQuality = 1;
        public int SelectedQuality
        {
            get => selectedQuality;
            set
            {
                selectedQuality = value;
                OnPropertyChanged(nameof(this.SelectedQuality));
                newChanges = true;
            }
        }

        bool isMouseActive;
        public bool IsMouseActive
        {
            get => isMouseActive;
            set
            {
                isMouseActive = value;
                OnPropertyChanged(nameof(this.IsMouseActive));
            }
        }

        BitmapScalingMode render = BitmapScalingMode.Linear;
        public BitmapScalingMode Render
        {
            get => render;
            set
            {
                render = value;
                OnPropertyChanged(nameof(this.Render));
            }
        }

        public BitmapScalingMode[] RenderList => new[] { BitmapScalingMode.Fant, BitmapScalingMode.Linear, BitmapScalingMode.NearestNeighbor };

        bool isConnected;
        public bool IsConnected
        {
            get => isConnected;
            set
            {
                isConnected = value;
                OnPropertyChanged(nameof(this.IsConnected));
            }
        }

        string receivedPackets;
        public string ReceivedPackets
        {
            get => receivedPackets;
            set
            {
                receivedPackets = value;
                OnPropertyChanged(nameof(this.ReceivedPackets));
            }
        }

        string sendedPackets;
        public string SendedPackets
        {
            get => sendedPackets;
            set
            {
                sendedPackets = value;
                OnPropertyChanged(nameof(this.SendedPackets));
            }
        }

        public bool newChanges = false;
    }

    public class Msg
    {
        public string Content { get; set; }
        public bool IsLocal { get; set; }

        public Msg(bool local, string content)
        {
            this.IsLocal = local;
            this.Content = content;
        }
    }
}
