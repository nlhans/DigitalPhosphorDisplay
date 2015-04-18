using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PhosphorDisplay.Filters;

namespace PhosphorDisplay.Data
{
    public class NetStream : IDataSource
    {
        private float LastHighresVoltage=0.0f;
        private bool AdcMcp = false;
        private int Gain = 0;
        private int oversampleRatio = 1;
        
        private DateTime StreamStoppedAt = DateTime.Now;

        private UdpClient _udp;
        private bool _udpListening;

        private Thread netProcesser;

        private List<byte[]> buffer = new List<byte[]>();
        private int bufferWaiting = 0;

        private float[] adcZero = new float[4];


        #region Implementation of IDataSource

        public float SampleRate { get; private set; }
        public double MaximumAmplitude { get; set; }
        public event DataSourceEvent Data;
        public event HighresEvent HighresVoltage;
        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public void Zero(float zeroValue)
        {
            adcZero[(int)Math.Log10(Gain)] = zeroValue;
        }

        public void Connect(object target)
        {
            HighresVoltage += voltage => LastHighresVoltage = voltage;

            netProcesser = new Thread(processData);
            netProcesser.IsBackground = true;
            netProcesser.Priority = ThreadPriority.AboveNormal;
            netProcesser.Start();

            _udp = new UdpClient();

            // Create connection (to target?)
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 3903);
            /*IPAddress ip = IPAddress.Parse("224.5.6.7");

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

            var multOpt = new MulticastOption(ip, ifaceIndex);
            _udp.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, multOpt);*/
            _udp.Client.Bind(ipep);
            _udpListening = true;
            _udp.BeginReceive(udpReadMore, null);
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
            if (configuration is NetStreamConfiguration)
            {
                var cfg = (NetStreamConfiguration) configuration;

                Stop();
                Thread.Sleep(25);

                var settings = new byte[12 + 16];
                settings[0] = 0;
                settings[1] = 0;
                settings[2] = 0;
                settings[3] = 0;

                settings[4] = (byte) (cfg.UseFastAdc ? 0 : 1); // mode 0 
                settings[5] = 0;

                settings[6] = 32;
                settings[7] = 0;//256 depth

                settings[8] = 0;
                settings[9] = 0;
                settings[10] = 0;
                settings[11] = 0;

                settings[12 + 2] = (byte)(Math.Log10(cfg.AfeGain)); // gain
                settings[12 + 4] = (byte)(cfg.AdcSpeed); // acq speed

                AdcMcp = settings[4] == 1;
                Gain = (int)Math.Pow(10, settings[12 + 2]);
                MaximumAmplitude = 2.35*0.591/Gain;
                oversampleRatio = Math.Max(1,cfg.OversampleRatio);

                if (settings[4] == 1)
                    SampleRate = 553321;
                    //SampleRate = 130211*2 / (float)Math.Pow(2, settings[12 + 4]);
                else
                    SampleRate = 1875026;

                SampleRate /= oversampleRatio;

                SendCommand(PaCommand.SET_STREAM_SETTINGS, settings);

                Thread.Sleep(25);

                Start();
            }
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
            HIGHRES_DATA = 0x0031,
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

        private ulong lastPacketId = 0;
        public void processData()
        {
            var notNextId = false;
            byte[] packet;
            while (netProcesser != null && netProcesser.IsAlive)
            {
                while (bufferWaiting == 0) Thread.Sleep(100);
                lock (buffer)
                {
                    if (buffer.Any(x => BitConverter.ToUInt64(x, 16) == lastPacketId + 1))
                    {
                        notNextId = false;
                        packet = buffer.FirstOrDefault(x => BitConverter.ToUInt64(x, 16) == lastPacketId + 1);
                        buffer.Remove(packet);
                    }
                    else
                    {
                        notNextId = true;
                        packet = buffer.OrderBy(x => BitConverter.ToUInt64(x, 16)).FirstOrDefault();
                        buffer.Remove(packet);
                    }

                    bufferWaiting--;
                    if (buffer.Count>1000)
                    {
                        bufferWaiting = 0;
                        buffer.Clear();
                        continue;
                    }
                    if (packet == null) continue;
                }

                var total = packet.Length - 24;
                var payload = new byte[total];
                Array.Copy(packet, 24, payload, 0, payload.Length);

                var header = new byte[24];
                Array.Copy(packet, 0, header, 0, header.Length);

                var pkgType = (PaCommand) header[2];

                if (pkgType == PaCommand.STREAM_DATA)
                {

                    var expectedPacketId = lastPacketId + 1;
                    var packetId = BitConverter.ToUInt64(header, 16);

                    if (expectedPacketId != packetId)
                    {
                        Debug.WriteLine("UDP traffic messed up order.." + ((notNextId) ? "as expected" : " (bug?!)"));
                    }
                    lastPacketId = packetId;

                    // Process all of them
                    ProcessSamples(payload);
                }
                if (pkgType == PaCommand.HIGHRES_DATA)
                {
                    var voltInt = BitConverter.ToUInt32(payload, 0);
                    var voltFlt = (voltInt)*1.2f/0x1FFFFF*2f*23;

                    var voltCalZero = 13.63f/1000.0f; // 13.18mV offset
                    var voltCal2048 = 2.03965f; // 0.9958984375 / 1.0023583984375
                    var voltCal1200 = 1.1925f; // 0.99375 / 1.004775
                    var voltScale = voltCal2048/2.048f;
                    voltFlt -= voltCalZero;
                    voltFlt /= voltScale;
                    //voltFlt = 0;
                    if (voltFlt>-0.1 && voltFlt<12 && HighresVoltage != null)
                        HighresVoltage(voltFlt);
                }
            }

        }

        private List<float> oversamplingSamples = new List<float>(); 

        public void ProcessSamples(byte[] packet)
        {
            float timeInterval = 1.0f/SampleRate;

            int historySize = 8;
            int samplesInPacket = (packet.Length - historySize * 2) / 2;
            int samplesOffset = historySize * 2;

            float[] samples = new float[samplesInPacket];
            Dictionary<int, int> ranges = new Dictionary<int, int>();
            ranges.Add(0, Gain);

            for (int i = 0; i < historySize; i++)
            {
                int d = BitConverter.ToInt16(packet, i * 2);

                if (d == 0) break;

                int r = d >> 12;
                int s = (d & 0xFFF);
                if (s == 0xFFF)
                {
                    ranges[0] =(int) Math.Pow(10, r);
                }
                else
                {
                    ranges.Add(s, r);
                }
            }

            int cooldown = 0;
            for (int k = 0; k < samplesInPacket; k++)
            {
                var l = samplesOffset + k*2;
                var t = packet[l+1];

                ushort sh = BitConverter.ToUInt16(packet, l);

                if (ranges.Keys.Contains(k))
                {
                    Gain = ranges.FirstOrDefault(x => x.Key == k).Value;
                    if (k != 0)
                    {
                        cooldown = (int)Math.Max(5, SampleRate/6666);
                        Debug.WriteLine("Switched to gain range" + Gain + " @ " + k);
                    }
                }
                if (cooldown > 0) cooldown--;

                if(cooldown > 0 && k > 0)
                    samples[k] = samples[k - 1];
                else
                    samples[k] = ConvertCodeToAmp(sh, LastHighresVoltage, Gain, k, AdcMcp, true);
            }

            DataPacket dpk;

            if (oversampleRatio == 1)
            {
                dpk = new DataPacket(samples, DataType.DutCurrent, (float) timeInterval);
            }
            else
            {
                // Add all samples to array
                oversamplingSamples.AddRange(samples);
                List<float> outputSamples = new List<float>();

                while (oversamplingSamples.Count() >= oversampleRatio)
                {
                    var v = oversamplingSamples.Take(oversampleRatio).Sum()/oversampleRatio;
                    outputSamples.Add(v);

                    oversamplingSamples.RemoveRange(0, oversampleRatio);
                }

                dpk = new DataPacket(outputSamples.ToArray(), DataType.DutCurrent, (float)timeInterval);
            }
            if (Data != null)
                Data(dpk);

        }


        public float ConvertCodeToAmp(ushort rawValue, float voltage, float Gain, int k, bool mcpAdc, bool adsAdc)
        {
            float currentValue;

            if (adsAdc)
            {
                currentValue = (rawValue - 0x8000) / 65536.0f * 2.048f * 23;

                currentValue /= 10; // shunt

                currentValue /= Gain; // gain

                currentValue += 11.09f / 1000000;

                var voltageCorrection = voltage / 15220.588235294117647058823529412f;
                voltageCorrection -= voltage*1.43f/1000000.0f;
                currentValue -= voltageCorrection;

                currentValue *= 1000000;

                switch (Gain.ToString())
                {
                    case "1000":
                        currentValue -= 8.921f;
                        currentValue *= 1.004733150620836897795431537004f;
                        break;
                    case "100":
                        currentValue += 6.212f;
                        currentValue -= 0.2f * voltage;
                        currentValue *= 1.0045381673442926137934153855432f;
                        break;
                    case "10":
                        currentValue += 159.604f;
                        currentValue += 0.4f * voltage;
                        currentValue *= 1.0079312257348863006100942872989f;
                        break;
                    case "1":
                        currentValue += 1729.4f;
                        currentValue -= voltage * 2.25f;
                        currentValue *= 1.0021917534421995348722378371204f;
                        break;
                }
                currentValue /= 1000000;
                
                // User zero value:
                currentValue -= this.adcZero[(int) Math.Log10(Gain)];
            }
            else if (mcpAdc)
            {
                currentValue = rawValue / 32768.0f / 1 / 1.5f * 1.2f * 23;
                currentValue /= 10; // shunt

                currentValue /= Gain; // gain

                var voltageCorrection = voltage/15220.588235294117647058823529412f;
                currentValue -= voltageCorrection;

                currentValue *= 1000000;
                if (Gain == 10)
                {
                    currentValue -= 12.88f;

                    var adc = k%4;
                    switch (adc)
                    {
                        case 0:
                            currentValue += 8.57f;
                            break;
                        case 1:
                            currentValue += 19.7f;
                            break;
                        case 2:
                            currentValue -= 61.65f;
                            break;
                        case 3:
                            currentValue += 5.47f;
                            break;
                    }
                }

                if (Gain == 1)
                    currentValue -= 1300;
                if (Gain == 1000)
                {
                    currentValue -= 3.6f;

                    var adc = k%4;
                    switch (adc)
                    {
                        case 0:
                            currentValue -= 0.18f;
                            break;
                        case 1:
                            currentValue += 0.12f;
                            break;
                        case 2:
                            currentValue -= 0.37f;
                            break;
                        case 3:
                            currentValue += 0.34f;
                            break;
                    }
                }
                if (Gain == 100)
                {
                    currentValue -= 2.5f;
                }

                if (Gain == 1000)
                    currentValue /= 1.03f;
                else
                    currentValue /= 1.02f;
                currentValue /= 1000000;
            }
            else
            {
                currentValue = (rawValue - 2048) / 2048.0f;
                currentValue *= 2.5f*23;

                currentValue /= 10; // shunt

                currentValue /= Gain; // gain

                currentValue -= voltage/15220.588235294117647058823529412f;
                currentValue /= 2;

                if (Gain == 1000)
                    currentValue -= 0.000149f;
                if (Gain == 10)
                    currentValue -= 0.0013f;
            }
            return currentValue;
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
