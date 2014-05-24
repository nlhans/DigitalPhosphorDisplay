using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace PhosphorDisplay
{
    class DataStream
    {
        // Simple & effective: singleton
        private static DataStream _Instance = new DataStream();
        public static DataStream Instance { get { return _Instance; } set { _Instance = value; } }

        public bool Running { get { return (_mListener != null); } }

        public double MaximumAmplitude { get { return 1300000.0 / Gain; } }
        public int Sample;
        public int Gain = 1000;

        public DateTime StartTime;

        public double wfms = 1;

        public event EventHandler WaveformDone;

        private Thread _mListener;
        private UdpClient _udp;

        private List<byte[]> buffer = new List<byte[]>();
        private int bufferWaiting = 0;

        private ManualResetEvent bufferData = new ManualResetEvent(false);

        private int packets = 0;
        private bool Listening = false;
        public void Start()
        {
            if (_mListener == null)
            {
                Listening = true;

                StartTime = DateTime.Now;

                // make connection

                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 1234);
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
                                                    (int)IPAddress.HostToNetworkOrder(p.Index));
                        break;
                    }
                }
                _udp.Client.Bind(ipep);

                var multOpt = new MulticastOption(ip, ifaceIndex);
                _udp.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, multOpt);

                _udp.BeginReceive(udpReadMore, null);

                // Create thread
                _mListener = new Thread(_fListener);
                _mListener.Priority = ThreadPriority.Highest;
                _mListener.Start();
            }
        }

        private void udpReadMore(IAsyncResult iar)
        {
            try
            {
                if (_udp == null) return;
                var any = (IPEndPoint) new IPEndPoint(IPAddress.Any, 1234);
                var read = _udp.EndReceive(iar, ref any);

                lock (buffer)
                {
                    buffer.Add(read);
                    bufferWaiting++;
                }
                packets++;
                bufferData.Set();

                if (Listening)
                {
                    _udp.BeginReceive(udpReadMore, null);
                }
            }catch
            {
            }
        }

        public void Restart()
        {
            if (_mListener == null)
            {
                Listening = true;

                _mListener = new Thread(_fListener);
                _mListener.Priority = ThreadPriority.Highest;
                _mListener.Start();
            }
        }

        public void Stop()
        {
            if (_mListener != null)
            {
                _udp.Close();
                _udp = null;
                Listening = false;
                _mListener = null;
            }
        }

        private void _fListener()
        {

            int oversample = 1, acqSamples = 512;

            int samplesPerSecond = 0;
            int sampleCounter = 0;
            DateTime lastSamplePerSecondMeasurement = DateTime.Now;

            Waveform f = new Waveform(1, 20480);

            var wasTriggered = false;
            byte[] b = new byte[0];
            while (Listening)
            {
                Thread.Sleep(1);

                while (bufferWaiting > 0)
                {
                    acqSamples = 401;
                    oversample = 1;

                    var dt = DateTime.Now.Subtract(lastSamplePerSecondMeasurement);
                    if (dt.TotalMilliseconds >= 500)
                    {
                        sampleCounter *= oversample;
                        Debug.WriteLine((sampleCounter / (dt.TotalMilliseconds / 1000)) + "sps / " + (packets * 1070 * 8 * 2 / 1000000.0 + "mbit") + " / " + buffer.Count + " residual");
                        packets = 0;
                        lastSamplePerSecondMeasurement = DateTime.Now;
                        samplesPerSecond = sampleCounter;
                        sampleCounter = 0;
                        wfms = samplesPerSecond / acqSamples;
                    }

                    /*var any = (IPEndPoint)new IPEndPoint(IPAddress.Any, 1234);

                    var bf = _udp.Receive(ref any);
                    buffer.AddRange(bf);
                    */
                    lock (buffer)
                    {
                        b = buffer[0];
                        buffer.RemoveAt(0);
                        bufferWaiting--;
                    }
                    if (b == null) continue;

                    int k = 2;
                    var rawSample = new short[512];

                    while (k < 514)
                    {
                        var t = b[k*2];
                        b[k*2] = b[k*2 + 1];
                        b[k*2 + 1] = t;

                        var d = BitConverter.ToInt16(b, k*2);
                        rawSample[k-2]=d;

                        k++;
                    }

                    var peak = 0.0f;
                    for (int i = 0; i < 512; i += oversample)
                    {
                        Sample++;
                        sampleCounter++;

                        int d = rawSample[i];
                        if (oversample>=2)
                        for (int j = i+1; j < i + oversample; j++)
                            d += rawSample[j];

                        var volt = 3.3f;

                        var currentValue = d*1.0f/oversample;
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
                                currentValue -= 5.7f;
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

                        if (currentValue > 0) wasTriggered = true;
                        peak = Math.Max(currentValue,peak);
                        f.Store((Sample - 1)*1.0f/(acqSamples - 1)*0.035f, new float[1] {(float) currentValue});

                        if (Sample >= acqSamples)
                        {

                            f.TriggerTime = 0.035f/2;
                            Sample = 0;
                            if (WaveformDone != null)
                            {
                                WaveformDone(f, new EventArgs());
                                //Debug.WriteLine(peak);
                            }
                            peak = 0.0f;
                            wasTriggered = false;
                            f = new Waveform(1, acqSamples);
                        }
                    }
                }
            }

        }
    }
}