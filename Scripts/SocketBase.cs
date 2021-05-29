using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Remote
{
    public enum PacketType
    {
        Frame, CursorPosition, Button, Key, Options
    }

    abstract class SocketBase
    {
        protected Socket socket;

        public static int packetSize = 1024;

        public Queue<Packet> receivedPackets = new Queue<Packet>();
        public Packet lastPacket;

        public void BeginReceive() => BeginReceive(this.socket);
        public void Send(object obj, PacketType type) => Send(obj, type, this.socket);
        public void SendRaw(byte[] buffer, PacketType type) => SendRaw(buffer, type, this.socket);

        public void BeginReceive(Socket socket)
        {
            var packet = new PacketHandler(socket);
            socket.BeginReceive(packet.receiveBuffer, 0, packet.receiveBuffer.Length, 0, new AsyncCallback(ReceiveCallback), packet);
        }

        public void Send(object obj, PacketType type, Socket socket)
        {
            byte[] buffer = Serializator.Serialize(obj);
            SendRaw(buffer, type, socket);
        }

        public void SendRaw(byte[] buffer, PacketType type, Socket socket)
        {
            try
            {
                byte[] data = new byte[8];

                Array.Copy(BitConverter.GetBytes((int)type), 0, data, 0, 4);
                Array.Copy(BitConverter.GetBytes(buffer.Length), 0, data, 4, 4);

                socket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), socket);

                var bytesLeftToTransmit = buffer.Length;
                byte[] sendBuffer = new byte[packetSize];
                var offset = 0;
                while (bytesLeftToTransmit > 0)
                {
                    var dataToSend = (buffer.Length - offset > packetSize) ? packetSize : buffer.Length - offset;
                    if(dataToSend < packetSize) sendBuffer = new byte[dataToSend];
                    Array.Copy(buffer, offset, sendBuffer, 0, dataToSend);

                    bytesLeftToTransmit -= dataToSend;
                    offset += dataToSend;
                    socket.BeginSend(sendBuffer, 0, sendBuffer.Length, 0, new AsyncCallback(SendCallback), socket);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        void ReceiveCallback(IAsyncResult ar)
        {
            PacketHandler data = (PacketHandler)ar.AsyncState;
            Socket socket = data.socket;
            int received = data.socket.EndReceive(ar);
            if (received == 0) throw new Exception("ERROR");

            if (data.state == PacketState.Receiving)
            {
                int bytesToCopy = Math.Min(received, data.remain);
                data.stream.Write(data.receiveBuffer, 0, bytesToCopy);
                data.remain -= bytesToCopy;

                if (data.remain == 0)
                {
                    //var buffer = data.stream.ToArray();
                    //receivedPackets.Enqueue(new Packet(data));
                    lastPacket = new Packet(data);
                    data.stream.Dispose();

                    //PacketReceived?.Invoke(data.type, data.buffer);
                    //OnPacketReceived(data.type, buffer);

                    data = new PacketHandler(socket);
                }
                else
                {
                    if(data.remain < packetSize)
                    {
                        data.receiveBuffer = new byte[data.remain];
                        Debug.WriteLine(data.remain);
                    }
                }
            }
            else
            {
                data.type = (PacketType)BitConverter.ToInt32(data.receiveBuffer, 0);
                data.size = BitConverter.ToInt32(data.receiveBuffer, 4);
                data.remain = data.size;

                data.stream = new MemoryStream();
                data.receiveBuffer = new byte[packetSize];
                data.state = PacketState.Receiving;
            }
            socket.BeginReceive(data.receiveBuffer, 0, data.receiveBuffer.Length, 0, new AsyncCallback(ReceiveCallback), data);
        }

        void SendCallback(IAsyncResult ar)
        {
            try
            {
                var socket = (Socket)ar.AsyncState;
                socket.EndSend(ar);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public class PacketHandler
    {
        public Socket socket;

        public PacketType type;
        public int size;
        public int remain;

        public MemoryStream stream;
        public byte[] receiveBuffer;

        public PacketState state;

        public PacketHandler(Socket socket)
        {
            this.socket = socket;
            receiveBuffer = new byte[8];
        }
    }

    public class Packet
    {
        public PacketType type;
        public byte[] buffer;

        public Packet(PacketHandler data)
        {
            this.type = data.type;
            this.buffer = data.stream.ToArray();
        }
    }

    public enum PacketState { Initializing, Receiving}
}
