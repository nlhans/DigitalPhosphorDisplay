using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MS.Internal.Xml.XPath;

namespace PhosphorDisplay
{
    public partial class ucPhosphorDisplay : UserControl
    {
        private int horizontalDivisions = 7;
        private int verticalDivisions = 5;

        public int[][] channelColors = new int[][]
                                           {
                                               new int[] {Color.Yellow.R, Color.Yellow.G, Color.Yellow.B},
                                               new int[]
                                                   {Color.DeepSkyBlue.R, Color.DeepSkyBlue.G, Color.DeepSkyBlue.B},
                                               new int[]
                                                   {Color.SpringGreen .R, Color.SpringGreen.G, Color.SpringGreen.B},
                                               new int[]
                                                   {Color.Magenta .R, Color.Magenta.G, Color.Magenta.B},
                                           };

        public int channels = 2;

        private float horizontalScale = 0.0025f;
        private float[] verticalScale = new float[] { 0.5f, 1.0f, 1.0f };
        private float[] verticalOffset = new float[] { 2, -2, 0 };

        private List<Waveform> waveforms = new List<Waveform>(); 

        private Pen gridPen = new Pen(Color.DimGray, 1.0f);
        private Pen gridCenterPen = new Pen(Color.LightGray, 1.0f);

        public ucPhosphorDisplay()
        {
            InitializeComponent();

            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint,true);

            this.SizeChanged += ucPhosphorDisplay_SizeChanged;
        }

        void ucPhosphorDisplay_SizeChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            var w = e.ClipRectangle.Width;
            var h = e.ClipRectangle.Height;
            if (w < 1) w = 1;

            var graph = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            var g = Graphics.FromImage(graph);

            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.CompositingMode = CompositingMode.SourceCopy;

            g.FillRectangle(Brushes.Black, e.ClipRectangle);

            var pxPerHorizontalDivision = w/2/horizontalDivisions;
            var offsetHorizontalDivision = w/2 - (pxPerHorizontalDivision*horizontalDivisions);

            var pxPerVerticalDivision = h/2/verticalDivisions;
            var offsetVerticalDivision = h/2 - (pxPerVerticalDivision*verticalDivisions);
            // Draw grid
            for (int div = -horizontalDivisions; div <= horizontalDivisions; div++)
            {
                var x = offsetHorizontalDivision + pxPerHorizontalDivision*(horizontalDivisions + div);
                var y = offsetVerticalDivision + pxPerVerticalDivision*verticalDivisions*2;
                g.DrawLine((div == 0) ? gridCenterPen : gridPen, x, offsetVerticalDivision, x, y);
            }
            for (int div = -verticalDivisions; div <= verticalDivisions; div++)
            {
                var x = offsetHorizontalDivision + pxPerHorizontalDivision*horizontalDivisions*2;
                var y = offsetVerticalDivision + pxPerVerticalDivision*(verticalDivisions + div);
                g.DrawLine((div == 0) ? gridCenterPen : gridPen, offsetHorizontalDivision, y, x, y);
            }

            var graphWidth = w - offsetHorizontalDivision*2;
            var graphHeight = h - offsetVerticalDivision*2;

            if (waveforms.Count == 0) return;
            if (overhead < 0)
                overhead = sw.ElapsedMilliseconds;
            else overhead = overhead/2.0f + sw.ElapsedMilliseconds/2.0f;

            var myWaveforms = new List<Waveform>(waveforms);
            waveforms.Clear();
            var compressionRatio = 1;

            // Display all waveforms
            // To do that, we map all waveforms to an display array
            int[][][] intensity = new int[channels][][];
            for (var ch = 0; ch < channels; ch++)
            {
                var arr = new int[w][];
                for (var x = 0; x < w; x++)
                    arr[x] = new int[h];
                intensity[ch] = arr;
            }
            foreach (var wave in myWaveforms)
            {
                wave.Process(w);
                compressionRatio = Math.Max(wave.Samples/w, compressionRatio);
                var i = 0;
                for (int s = 0; s < wave.Samples; s++)
                {
                    var waveTime = wave.Horizontal[s] - wave.TriggerTime;
                    var x =
                        (int)
                        ((waveTime/horizontalScale + horizontalDivisions)*
                         pxPerHorizontalDivision);
                    if (x < 0) continue;
                    if (x >= graphWidth) break;

                    for (var ch = 0; ch < channels; ch++)
                    {
                        var y = (int)((wave.Data[ch][s]/-verticalScale[ch] +
                                  verticalDivisions-verticalOffset[ch])*pxPerVerticalDivision);

                        if (y < 0) continue;
                        if (y >= graphHeight) break;
                        intensity[ch][x][y]++;
                    }

                    i++;
                }
            }
            // Lock bits on map
            var dat = graph.LockBits(e.ClipRectangle, ImageLockMode.ReadWrite, graph.PixelFormat);
            var ptr = dat.Scan0;
            var bytesPerPixel = 4;
            byte[] bitmapBuffer = new byte[w*h*bytesPerPixel];
            Marshal.Copy(ptr, bitmapBuffer, 0, bitmapBuffer.Length);

            var k = 0;
            foreach (var channel in intensity)
            {
                var chColor = channelColors[k++];
                var noOfPens = myWaveforms.Count*compressionRatio+1;
                var penPallette = new byte[noOfPens][];

                for (int i = 0; i < noOfPens; i++)
                {
                    var perc = (float)Math.Pow(i*1.0f/noOfPens,0.45) * 95.0f+ 5f;
                    
                    if (perc >= 100) perc = 100;
                    if (perc <= 0) perc = 0;
                    if (float.IsNaN(perc) || float.IsInfinity(perc)) perc = 100;
                    var c = new byte[3]
                                {
                                    (byte) (chColor[0]*perc/100),
                                    (byte) (chColor[1]*perc/100),
                                    (byte) (chColor[2]*perc/100)
                                };

                    penPallette[i] = c;
                }
                for (var x = 0; x < w; x++)
                {
                    for (var y = 0; y < h; y++)
                    {
                        var chVal =(int)(channel[x][y] );
                        if (chVal > 0)
                        {
                            var i = (y + offsetVerticalDivision)*w + x + offsetHorizontalDivision;
                            i *= bytesPerPixel;
                            //chVal /= compressionRatio;
                            if (chVal >= noOfPens)
                                chVal = noOfPens-1;
                            var c = penPallette[chVal];

                            bitmapBuffer[i] = c[2]; // blue
                            bitmapBuffer[i + 1] = c[1]; // green
                            bitmapBuffer[i + 2] = c[0]; // red

                        }
                    }
                }

            }
            Marshal.Copy(bitmapBuffer, 0, ptr, bitmapBuffer.Length);
            graph.UnlockBits(dat);

            e.Graphics.DrawImage(graph, 0, 0);
            sw.Stop();
            if (speed < 0)
                speed = sw.ElapsedMilliseconds;
            else 
                speed = speed*0.6f + sw.ElapsedMilliseconds*0.4f;
            measurements++;
        }

        public float speed;
        public float overhead;
        public int measurements;

        public void Add(Waveform waveform)
        {
            waveforms.Add(waveform);
        }
    }
}
