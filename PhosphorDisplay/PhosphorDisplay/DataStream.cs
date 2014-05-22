using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public int Gain = 10;

        public DateTime StartTime;

        public double wfms = 1;

        public event EventHandler WaveformDone;

        private Thread _mListener;
        private UdpClient _udp;

        private bool Listening = false;

        public void Start()
        {
            if (_mListener == null)
            {
                Listening = true;

                StartTime = DateTime.Now;

                _mListener = new Thread(_fListener);
                _mListener.Priority = ThreadPriority.Highest;
                _mListener.Start();
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
                Listening = false;
                _mListener = null;
            }
        }

        private void _fListener()
        {


            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 1234);
            IPAddress ip = IPAddress.Parse("224.5.6.7");

            List<byte> bigBf = new List<byte>();

            var _udp = new UdpClient();
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

            List<int> rawSample = new List<int>();
            Waveform f = new Waveform(1, 20480);

            while (Listening)
            {
                try
                {

                    var any = (IPEndPoint)new IPEndPoint(IPAddress.Any, 1234);
                    byte[] bf = new byte[1028];

                    bf = _udp.Receive(ref any);
                    bigBf.AddRange(bf);
                    while (bigBf.Count >= 1028)
                    {
                        for (int i = 0; i < bigBf.Count - 4; i++)
                        {
                            if (bigBf[i] == 0 && bigBf[i + 1] == 0 && bigBf[i + 2] == 0 && bigBf[i + 3] == 0)
                            {
                                bigBf.RemoveRange(0, i + 4);
                                break;
                            }
                        }

                        if (bigBf.Count >= 1024)
                        {
                            var b = bigBf.ToArray();
                            int k = 0;

                            while (k < 512)
                            {
                                var t = b[k * 2];
                                b[k * 2] = b[k * 2 + 1];
                                b[k * 2 + 1] = t;


                                var d = BitConverter.ToInt16(b, k * 2);
                                rawSample.Add(d);


                                k++;
                            }

                            bigBf.RemoveRange(0, 1024);

                            var maxInt = 65535.0;
                            int fftSize = 2048;

                            if (rawSample.Count < fftSize) continue;

                            int acqSamples = 28 + 1;
                            acqSamples = 15;
                            var oversample = 1;
                            var samplesPerSecond = 12288;
                            wfms = samplesPerSecond/acqSamples/oversample;
                            for (int i = 0; i < rawSample.Count; i += oversample)
                            {
                                Sample++;

                                int d = 0;
                                for (int j = i; j < i + oversample; j++)
                                    d += rawSample[j];

                                var volt = 3.3;

                                var currentValue = d * 1.0 / oversample;
                                currentValue = currentValue / 32768.0 / 1 / 1.5 * 1.2 * 23;
                                currentValue /= 10; // shunt

                                currentValue /= Gain; // gain

                                currentValue -= volt / 15220.588235294117647058823529412;

                                currentValue *= 1000000;
                                if (Gain == 10)
                                    currentValue -= 148.3;
                                if (Gain == 1)
                                    currentValue -= 1300;
                                if (Gain == 1000)
                                    currentValue -= 5.7;
                                if (Gain == 100)
                                    currentValue -= 10.64 + 8.8;

                                currentValue /= 1.02;
                                currentValue /= 1000000;

                                f.Store((Sample-1) * 1.0f / (acqSamples-1) * 0.035f, new float[1] { (float)currentValue });

                                if (Sample >= acqSamples)
                                {

                                    f.TriggerTime = 0.035f/2;
                                    Sample = 0;

                                    if (WaveformDone != null)
                                        WaveformDone(f, new EventArgs());

                                    f = new Waveform(1, acqSamples);
                                }
                            }
                            rawSample.RemoveRange(0, fftSize);
                        }

                    }
                }
                catch (Exception ex)
                {
                    //
                }
                //Thread.Sleep(1);
            }

            _udp.Close();
        }
    }
}