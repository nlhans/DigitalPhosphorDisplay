using System;
using System.Collections.Concurrent;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MS.Internal.Xml.XPath;

namespace PhosphorDisplay
{
    public partial class ucPhosphorDisplay : UserControl
    {
        private int horizontalDivisions = 7;
        private int verticalDivisions = 7;

        public int[][] channelColors = new int[][]
                                           {
                                               new int[] {Color.Yellow.R, Color.Yellow.G, Color.Yellow.B},
                                               new int[]
                                                   {Color.DeepSkyBlue.R, Color.DeepSkyBlue.G, Color.DeepSkyBlue.B},
                                               new int[]
                                                   {Color.SpringGreen.R, Color.SpringGreen.G, Color.SpringGreen.B},
                                               new int[]
                                                   {Color.Magenta.R, Color.Magenta.G, Color.Magenta.B},
                                           };

        public int channels = 2;

        private float horizontalScale = 0.0025f;
        private float[] verticalScale = new float[] {2.0f/1000000, 1.00f, 0.5f};
        private float[] verticalOffset = new float[] {0, 0, 0f};

        private ConcurrentQueue<Waveform> waveforms = new ConcurrentQueue<Waveform>();
        private Mutex waveformsMutex = new Mutex();

        private Pen gridPen = new Pen(Color.DimGray, 1.0f);
        private Pen gridCenterPen = new Pen(Color.LightGray, 1.0f);

        public ucPhosphorDisplay()
        {
            InitializeComponent();

            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            this.SizeChanged += ucPhosphorDisplay_SizeChanged;
            this.BackColor = Color.Black;
        }

        private void ucPhosphorDisplay_SizeChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (waveforms.Count == 0) return;
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

            //g.FillRectangle(Brushes.Black, e.ClipRectangle);

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

            if (overhead < 0)
                overhead = sw.ElapsedMilliseconds;
            else overhead = overhead/2.0f + sw.ElapsedMilliseconds/2.0f;

            List<Waveform> myWaveforms = new List<Waveform>();
            lock (waveformsMutex)
            {
//                myWaveforms = new List<Waveform>(waveforms);

                Waveform f = default(Waveform);
                while(waveforms.TryDequeue(out f))
                    myWaveforms.Add(f);


                //if (myWaveforms.Count >= 500)
                //    myWaveforms.RemoveRange(0, myWaveforms.Count - 501);
            }

            var compressionRatio = 1;

            try
            {

                // Display all waveforms
                // To do that, we map all waveforms to an display array

                // This array contains the X,Y intensity data per channel
                // intensity[channel][x position][y position] = number of hits
                // The number of hits will later determine which pencil is used for drawing.
                // Which in term makes the specific pixel brighter or dimmer.
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
                    // Process may interpolate the waveform if not enough samples are available.
                    // The draw engine is only capable of doing dots mode.
                    wave.Process(w);

                    // The compression ratio is applicable when there are more samples to display than display width is available.
                    // This will mean that it's possible more than 1 sample is displayed on the same X position.
                    // This can increase the number of hits on that specific pixel, and therefor an compression ratio is used.
                    // It's a compromise between detail & accuracy. Higher resolution = better accuracy, at all times.
                    // But also slower to draw.
                    compressionRatio = Math.Max(wave.Samples/w, compressionRatio);

                    for (var ch = 0; ch < channels; ch++)
                    {

                        int yCenter = (int) ((verticalDivisions - verticalOffset[ch])*pxPerVerticalDivision);
                        for (int s = 0; s < wave.Samples; s++)
                        {
                            var waveTime = wave.Horizontal[s] - wave.TriggerTime;

                            // Calculate X position on screen. Check in bounds.
                            var x =
                                (int)
                                Math.Round((waveTime/horizontalScale + horizontalDivisions)*
                                           pxPerHorizontalDivision);

                            if (x < 0) continue;
                            if (x >= graphWidth) break;

                            // Calculate Y position on screen. Check in bounds.
                            var y = (int) ((wave.Data[ch][s]/-verticalScale[ch])*pxPerVerticalDivision) + yCenter;

                            if (y < 0) y = 0;
                            if (y >= graphHeight) y = graphHeight;

                            // Make a hit for this pixel.
                            intensity[ch][x][y]++;
                        }
                    }
                }

                // Lock bits on map
                // This is used for a much much faster setpixel performance. The bitmap is accesisable via a raw byte arrya, containing bit information.
                // The format of this array depends on the pixelformat, which is 32-bit R-G-B-A.
                // Alpha seems to be broken, so we modulate the alpha via the pencil.
                var dat = graph.LockBits(e.ClipRectangle, ImageLockMode.ReadWrite, graph.PixelFormat);
                var ptr = dat.Scan0;
                var bytesPerPixel = 4;
                byte[] bitmapBuffer = new byte[w*h*bytesPerPixel];
                Marshal.Copy(ptr, bitmapBuffer, 0, bitmapBuffer.Length);

                // From here, we must edit our image in the bitmapBuffer, intead via the bitmap normal API routines.

                var k = 0;

                // We take the hit map
                foreach (var channel in intensity)
                {
                    var chColor = channelColors[k++];
                    var noOfPens = myWaveforms.Count*compressionRatio + 1;
                    var penPallette = new byte[noOfPens][];

                    // Generate a set of color.
                    for (int i = 0; i < noOfPens; i++)
                    {
                        // The accurateness and contrast of the intensity graded display can be changed here.
                        // With the SQRT less-freuqent signals are "amplified" and more frequent signals are compressed.
                        // The offset will also determine how visible less frequent options are seen.
                        var perc = (float) Math.Pow(i*1.0f/noOfPens, 0.5)*2*125.0f + 1f;

                        // Fix perc if <0% or >100% or "ERR"
                        if (perc >= 100) perc = 100;
                        if (perc <= 0) perc = 0;
                        if (float.IsNaN(perc) || float.IsInfinity(perc)) perc = 100;

                        // Colors are saved raw too, because this is faster to access.
                        var c = new byte[4]
                                    {
                                        (byte) (chColor[2]*perc/100),
                                        (byte) (chColor[1]*perc/100),
                                        (byte) (chColor[0]*perc/100),
                                        255
                                    };

                        penPallette[i] = c;
                    }

                    // Walk through the entire "image"
                    for (var x = 0; x < w; x++)
                    {
                        for (var y = 0; y < h; y++)
                        {
                            var chVal = (int) (channel[x][y]);

                            if (chVal > 0)
                            {
                                // We only modify when there was a hit here.
                                // i contains the index inside the bitmapBuffer we must edit.
                                var i = (y + offsetVerticalDivision)*w + x + offsetHorizontalDivision;
                                i *= bytesPerPixel;

                                // We pick a color
                                if (chVal >= noOfPens)
                                    chVal = noOfPens - 1;
                                var c = penPallette[chVal];

                                // And copy it.
                                bitmapBuffer[i] = c[0]; // blue
                                bitmapBuffer[i + 1] = c[1]; // green
                                bitmapBuffer[i + 2] = c[2]; // red
                                bitmapBuffer[i + 3] = c[3]; // alpha

                            }
                        }
                    }

                }
                Marshal.Copy(bitmapBuffer, 0, ptr, bitmapBuffer.Length);
                graph.UnlockBits(dat);

                e.Graphics.DrawImage(graph, 0, 0);
            }
            catch
            {
            }

            sw.Stop();
            waveformsCount += myWaveforms.Count;
            renderTime += (float) sw.ElapsedMilliseconds;

            var dt = DateTime.Now.Subtract(lastWfmsMeasurement);
            if(dt.TotalMilliseconds > 500 && waveformsCount > 10)
            {
                var wfms = waveformsCount/(dt.TotalMilliseconds/1000.0f);
                lastMeasurement = wfms.ToString("0000.0 wfms") + " [" + renderTime + "/"+dt.TotalMilliseconds.ToString("000")+"ms] ["+Math.Round(myWaveforms[0].Samples*wfms)+"sps]";
                lastMeasurementColor = renderTime >= dt.TotalMilliseconds - 10 ? Brushes.Red : 
                    renderTime*1.2 >= dt.TotalMilliseconds ? Brushes.Orange : Brushes.White;
                waveformsCount = 0;
                lastWfmsMeasurement = DateTime.Now;
                renderTime = 0;
            }
            e.Graphics.DrawString(lastMeasurement, new Font("Verdana", 7), lastMeasurementColor, 0, this.Height - 10);

            if (speed < 0)
                speed = sw.ElapsedMilliseconds;
            else
                speed = speed*0.6f + sw.ElapsedMilliseconds*0.4f;
            measurements++;
        }

        private Brush lastMeasurementColor = Brushes.White;
        private string lastMeasurement = "N/A";
        private DateTime lastWfmsMeasurement = DateTime.Now;
        private int waveformsCount = 0;
        private float renderTime = 0;

        public float speed;
        public float overhead;
        public int measurements;

        public void AddRange(IEnumerable<Waveform> wf)
        {
            try
            {
                lock (waveformsMutex)
                {
                    foreach (var w in wf)
                        waveforms.Enqueue(w);
                }
            }
            catch
            {
            }
        }

        public void Add(Waveform waveform)
        {
            waveforms.Enqueue(waveform);
        }
    }
}