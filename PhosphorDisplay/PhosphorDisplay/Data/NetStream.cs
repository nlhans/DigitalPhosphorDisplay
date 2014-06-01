using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhosphorDisplay.Data
{
    public class NetStream : IDataSource
    {
        private DateTime StreamStoppedAt = DateTime.Now;

        private UdpClient _udp;
        private bool _udpListening;

        private Thread netProcesser;

        private List<byte[]> buffer = new List<byte[]>();
        private int bufferWaiting = 0;

        #region Implementation of IDataSource

        public event DataSourceEvent Data;
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public void Connect(object target)
        {
            netProcesser = new Thread(processData);
            netProcesser.IsBackground = true;
            netProcesser.Priority = ThreadPriority.AboveNormal;
            netProcesser.Start();

            // Create connection (to target?)
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 3903);
            IPAddress ip = IPAddress.Parse("224.5.6.7");

            _udp = new UdpClient();
            var ifaceIndex = 0;

            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                if (adapter.Name == "Local Area Connection")
                {

                    IPv4InterfaceProperties p = adapter.GetIPProperties().GetIPv4Properties();
                    ifaceIndex = p.Index;
                    // now we have adapter index as p.Index, let put it to socket option
                    _udp.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface,
                                                (int) IPAddress.HostToNetworkOrder(p.Index));
                    break;
                }
            }
            _udp.Client.Bind(ipep);

            var multOpt = new MulticastOption(ip, ifaceIndex);
            _udp.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, multOpt);
            _udpListening = true;
            _udp.BeginReceive(udpReadMore, null);

            Configure(null);
        }

        public void Disconnect()
        {
            netProcesser.Abort();
            bufferWaiting++;

            _udpListening = false;
            _udp.Close();
        }

        public void Configure(object configuration)
        {
            var settings = new byte[12 + 16];
            settings[0] = 0;
            settings[1] = 0;
            settings[2] = 0;
            settings[3] = 0;

            settings[4] = 1; // mode 0 
            settings[5] = 0;

            settings[6] = 32;
            settings[7] = 0;//256 depth

            settings[8] = 0;
            settings[9] = 0;
            settings[10] = 0;
            settings[11] = 0;

            settings[12 + 2] = 1; // gain
            settings[12 + 4] = 0; // acq speed

            SendCommand(PaCommand.SET_STREAM_SETTINGS, settings);
        }

        public void Start()
        {
            // Give the power analyzer 50ms to recover
            if (DateTime.Now.Subtract(StreamStoppedAt).TotalMilliseconds<50)
                Thread.Sleep((int)DateTime.Now.Subtract(StreamStoppedAt).TotalMilliseconds);

            SendCommand(PaCommand.SET_STREAM_START, new byte[0]);
        }

        public void Stop()
        {
            SendCommand(PaCommand.SET_STREAM_STOP, new byte[0]);

            StreamStoppedAt = DateTime.Now;

        }

        #endregion

        public enum PaCommand : ushort
        {
            GET_CAPABILITIES = 0x0000,
            GET_CALIBATION = 0x0010,
            SET_CALIBRATION = 0x8010,
            GET_CALIBRATOIN_PAGES = 0x0011,
            GET_STREAM_SETTINGS = 0x0020,
            SET_STREAM_SETTINGS = 0x8020,
            SET_STREAM_START = 0x8021,
            SET_STREAM_STOP = 0x8022,
            SET_STREAM_RESTART = 0x8023,
            STREAM_DATA = 0x0030,
        }
        
        private void udpReadMore(IAsyncResult iar)
        {
            try
            {
                if (_udp == null) return;
                var any = (IPEndPoint)new IPEndPoint(IPAddress.Any, 3903);
                var read = _udp.EndReceive(iar, ref any);


                lock (buffer)
                {
                    buffer.Add(read);
                    bufferWaiting++;
                }

                if (_udpListening)
                {
                    _udp.BeginReceive(udpReadMore, null);
                }
            }
            catch
            {
            }
        }

        public void processData()
        {
            byte[] packet;
            while(netProcesser!= null && netProcesser.IsAlive)
            {
                while (bufferWaiting == 0) Thread.Sleep(1);
                lock (buffer)
                {
                    packet = buffer[0];

                    buffer.RemoveAt(0);
                    bufferWaiting--;
                    if (packet == null) continue;
                }

                var total = packet.Length - 24;
                var payload = new byte[total];
                Array.Copy(packet, 24, payload, 0, payload.Length);

                // Process all of them
                ProcessSamples(payload);
            }

        }

        public void ProcessSamples(byte[] packet)
        {
            float timeInterval = 1.0f/1000000;
            float[] samples = new float[packet.Length/2];

            for (int k = 0; k < packet.Length / 2; k++)
            {
                float currentValue = BitConverter.ToInt16(packet, k*2)/1.0f;
                float Gain = 10;
                float volt = 3.3f;

                if (true)
                {
                    currentValue = currentValue/32768.0f/1/1.5f*1.2f*23;
                    currentValue /= 10; // shunt

                    currentValue /= Gain; // gain

                    currentValue -= volt/15220.588235294117647058823529412f;

                    currentValue *= 1000000;
                    if (Gain == 10)
                        currentValue -= 148.3f;
                    if (Gain == 1)
                        currentValue -= 1300;
                    if (Gain == 1000)
                        currentValue -= 4.8f;
                    if (Gain == 100)
                        currentValue -= 10.64f + 8.8f;

                    currentValue /= 1.02f;
                    currentValue /= 1000000;
                }
                else
                {
                    currentValue = (currentValue - 2048)/2048.0f;
                    currentValue *= 2.5f*23;

                    currentValue /= 10; // shunt

                    currentValue /= Gain; // gain

                    currentValue -= volt/15220.588235294117647058823529412f;
                    currentValue /= 2;

                    if (Gain == 1000)
                        currentValue -= 0.000149f;
                    if (Gain == 10)
                        currentValue -= 0.0013f;
                }

                samples[k] = currentValue;
            }

            var dpk = new DataPacket(samples, DataType.DutCurrent, (float)timeInterval);

            if (Data != null)
                Data(dpk);

        }

        public void SendCommand(PaCommand cmd, byte[] payload)
        {
            var ep = new IPEndPoint(IPAddress.Parse("192.168.1.198"), 3903);
            var u = new UdpClient();
            var ddd = new byte[18 + payload.Length];
            ddd[2] = (byte)(18 + payload.Length);

            ddd[4] = (byte)((ushort)cmd & 0xFF);
            ddd[5] = (byte)((ushort)cmd >> 8);
            if (payload.Length > 0)
                Array.Copy(payload, 0, ddd, 18, payload.Length);

            u.Send(ddd, ddd.Length, ep);

        }
    }
}
