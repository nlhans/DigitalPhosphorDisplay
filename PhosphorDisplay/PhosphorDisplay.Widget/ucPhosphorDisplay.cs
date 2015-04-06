﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhosphorDisplay.Widget
{
    public partial class ucPhosphorDisplay : UserControl
    {
        public delegate void DoubleClickEvent(double time, double scalarCh1);
        public event DoubleClickEvent DoubleClicked;
        #region Waveform draw settings
        public int[][] ChannelColors = new int[][]
        {
            new int[] {Color.Yellow.R, Color.Yellow.G, Color.Yellow.B},
            new int[] {Color.DeepSkyBlue.R, Color.DeepSkyBlue.G, Color.DeepSkyBlue.B},
            new int[] {Color.SpringGreen.R, Color.SpringGreen.G, Color.SpringGreen.B},
            new int[] {Color.Magenta.R, Color.Magenta.G, Color.Magenta.B},
        };
        public int Channels = 2;
        public bool DotsOnly = false;
        #endregion
        #region Grid & scale Settings
        public int HorizontalDivisions = 5;
        public int VerticalDivisions = 5;
        public float HorizontalScale = 1;
        public float HorizontalOffset = 0.0f;
        public float[] VerticalScale = new float[] { 0.2f, 1.0f, 1.0f };
        public float[] VerticalOffset = new float[] { 0, 0, 0f };
        private Pen gridPen = new Pen(Color.DimGray, 1.0f);
        private Pen gridCenterPen = new Pen(Color.LightGray, 1.0f);
        #endregion
        #region Information for draw background worker
        private int GridWidth;
        private int GridHeight;
        private int TotalWidth;
        private int TotalHeight;
        #endregion
        private ConcurrentQueue<Waveform> waveforms = new ConcurrentQueue<Waveform>();
        private Mutex waveformsMutex = new Mutex();
        public ucPhosphorDisplay()
        {
            InitializeComponent();

            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            this.SizeChanged += ucPhosphorDisplay_SizeChanged;
            this.BackColor = Color.Black;
        }
        #region Information for drawing the grid
        private int pxPerHorizontalDivision;
        private int pxPerVerticalDivision;
        private int offsetHorizontalDivision;
        private int offsetVerticalDivision;

        public float DisplayContrast { get; set; }
        public float DisplayBrightness { get; set; }

        #endregion
        private void ucPhosphorDisplay_SizeChanged(object sender, EventArgs e)
        {

            pxPerHorizontalDivision = Width / 2 / HorizontalDivisions;
            offsetHorizontalDivision = Width / 2 - (pxPerHorizontalDivision * HorizontalDivisions);

            pxPerVerticalDivision = Height / 2 / VerticalDivisions;
            offsetVerticalDivision = Height / 2 - (pxPerVerticalDivision * VerticalDivisions);



            GridWidth = Width - offsetHorizontalDivision * 2;
            GridHeight = Height - offsetVerticalDivision * 2;
            TotalWidth = Width;
            TotalHeight = Height;

            this.Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var graph = new Bitmap(TotalWidth, TotalHeight, PixelFormat.Format32bppArgb);
            var g = Graphics.FromImage(graph);

            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.CompositingMode = CompositingMode.SourceCopy;
            
            // Draw grid
            for (int div = -HorizontalDivisions; div <= HorizontalDivisions; div++)
            {
                var x = offsetHorizontalDivision + pxPerHorizontalDivision * (HorizontalDivisions + div);
                var y = offsetVerticalDivision + pxPerVerticalDivision * VerticalDivisions * 2;
                g.DrawLine((div == 0) ? gridCenterPen : gridPen, x, offsetVerticalDivision, x, y);
            }
            for (int div = -VerticalDivisions; div <= VerticalDivisions; div++)
            {
                var x = offsetHorizontalDivision + pxPerHorizontalDivision * HorizontalDivisions * 2;
                var y = offsetVerticalDivision + pxPerVerticalDivision * (VerticalDivisions + div);
                g.DrawLine((div == 0) ? gridCenterPen : gridPen, offsetHorizontalDivision, y, x, y);
            }

            List<Waveform> myWaveforms = new List<Waveform>();
            lock (waveformsMutex)
            {
                Waveform f = default(Waveform);
                while (waveforms.TryDequeue(out f))
                    myWaveforms.Add(f);
            }

            var compressionRatio = 1;

            try
            {

                // Display all waveforms
                // To do that, we map all waveforms to an displayTrig array

                // This array contains the X,Y intensity data per channel
                // intensity[channel][x position][y position] = number of hits
                // The number of hits will later determine which pencil is used for drawing.
                // Which in term makes the specific pixel brighter or dimmer.
                int[][][] intensity = new int[Channels][][];
                for (var ch = 0; ch < Channels; ch++)
                {
                    var arr = new int[TotalWidth][];
                    for (var x = 0; x < TotalWidth; x++)
                        arr[x] = new int[TotalHeight];
                    intensity[ch] = arr;
                }

                //foreach (var wave in myWaveforms)
                Parallel.ForEach(myWaveforms, (wave) =>
                                              {
                                                  // Process may interpolate the waveform if not enough samples are available.
                                                  // The draw engine is only capable of doing dots mode.
                                                  if (DotsOnly)
                                                      wave.Process(GridWidth);

                                                  // The compression ratio is applicable when there are more samples to displayTrig than displayTrig width is available.
                                                  // This will mean that it's possible more than 1 sample is displayed on the same X position.
                                                  // This can increase the number of hits on that specific pixel, and therefor an compression ratio is used.
                                                  // It's a compromise between detail & accuracy. Higher resolution = better accuracy, at all times.
                                                  // But also slower to draw.
                                                  compressionRatio = Math.Max(wave.Samples/GridWidth, compressionRatio);
                                                  for (var ch = 0; ch < Channels; ch++)
                                                  {
                                                      var lastX = -1;
                                                      var lastY = -1;
                                                      var sameX = 0;

                                                      int yCenter =
                                                          (int)
                                                              ((VerticalDivisions - VerticalOffset[ch])*
                                                               pxPerVerticalDivision);
                                                      for (int s = 0; s < wave.Samples; s++)
                                                      {
                                                          var waveTime = wave.Horizontal[s] - wave.TriggerTime;

                                                          // Calculate X position on screen. Check in bounds.
                                                          var x =
                                                              (int)
                                                                  Math.Round(((waveTime - HorizontalOffset)/
                                                                              HorizontalScale + HorizontalDivisions)*
                                                                             pxPerHorizontalDivision);

                                                          if (x < 0)
                                                              continue;
                                                          if (x == GridWidth)
                                                              x = GridWidth - 1;
                                                          if (x > GridWidth)
                                                              break;

                                                          // Calculate Y position on screen. Check in bounds.
                                                          var y =
                                                              (int)
                                                                  ((wave.Data[ch][s]/-VerticalScale[ch])*
                                                                   pxPerVerticalDivision) + yCenter;

                                                          if (y < 0)
                                                              y = 0;
                                                          if (y >= GridHeight)
                                                              y = GridHeight - 1;

                                                          // Make a hit for this pixel.
                                                          if (lastX >= 0 && !DotsOnly)
                                                          {
                                                              var dx = x - lastX;
                                                              var dy = y - lastY;
                                                              if (Math.Abs(dx) > Math.Abs(dy))
                                                              {
                                                                  if (dx > 0)
                                                                  {
                                                                      if (dx > x) dx = x;

                                                                      for (var xInterpolated = 0;
                                                                          xInterpolated < dx;
                                                                          xInterpolated++)
                                                                      {
                                                                          var yInterpolated = y - dy*xInterpolated/dx;
                                                                          try
                                                                          {
                                                                              intensity[ch][x - xInterpolated][
                                                                                  yInterpolated]++;
                                                                          }
                                                                          catch
                                                                          {
                                                                          }
                                                                      }
                                                                  }
                                                                  else
                                                                  {
                                                                      if (-dx > x) dx = -x;

                                                                      for (var xInterpolated = 0;
                                                                          xInterpolated < -dx;
                                                                          xInterpolated++)
                                                                      {
                                                                          var yInterpolated = y + dy*xInterpolated/dx;
                                                                          try
                                                                          {
                                                                              intensity[ch][x - xInterpolated][
                                                                                  yInterpolated]++;
                                                                          }
                                                                          catch
                                                                          {
                                                                          }
                                                                      }
                                                                  }
                                                              }
                                                              else
                                                              {
                                                                  if (dy > 0)
                                                                  {
                                                                      for (var yInterpolated = 0;
                                                                          yInterpolated < dy;
                                                                          yInterpolated++)
                                                                      {
                                                                          var xInterpolated = lastX +
                                                                                              dx*yInterpolated/dy;
                                                                          try
                                                                          {
                                                                              intensity[ch][xInterpolated][
                                                                                  lastY + yInterpolated]++;
                                                                          }
                                                                          catch
                                                                          {
                                                                          }
                                                                      }
                                                                  }
                                                                  else
                                                                  {
                                                                      for (var yInterpolated = 0;
                                                                          yInterpolated < -dy;
                                                                          yInterpolated++)
                                                                      {
                                                                          var xInterpolated = lastX +
                                                                                              dx*yInterpolated/-dy;

                                                                          try
                                                                          {
                                                                              var y_ = Math.Min(GridHeight - 1,
                                                                                  lastY - yInterpolated);

                                                                              intensity[ch][xInterpolated][y_]++;
                                                                          }
                                                                          catch
                                                                          {
                                                                          }
                                                                      }
                                                                  }

                                                              }

                                                              if (lastX != x)
                                                                  sameX = 2;
                                                              else
                                                                  sameX++;
                                                              //compressionRatio = Math.Max(sameX, compressionRatio);
                                                          }
                                                          else
                                                              intensity[ch][x][y]++;

                                                          lastX = x;
                                                          lastY = y;
                                                      }
                                                  }
                                              });

                //compressionRatio = 1;

                // Lock bits on map
                // This is used for a much much faster setpixel performance. The bitmap is accesisable via a raw byte arrya, containing bit information.
                // The format of this array depends on the pixelformat, which is 32-bit R-G-B-A.
                // Alpha seems to be broken, so we modulate the alpha via the pencil.
                var dat = graph.LockBits(e.ClipRectangle, ImageLockMode.ReadWrite, graph.PixelFormat);
                var ptr = dat.Scan0;
                var bytesPerPixel = 4;
                byte[] bitmapBuffer = new byte[TotalWidth * TotalHeight * bytesPerPixel];
                Marshal.Copy(ptr, bitmapBuffer, 0, bitmapBuffer.Length);

                // From here, we must edit our image in the bitmapBuffer, intead via the bitmap normal API routines.

                var k = 0;

                // We take the hit map
                foreach (var channel in intensity)
                {
                    var chColor = ChannelColors[k++];
                    var noOfPens = myWaveforms.Count * compressionRatio + 1;
                    var penPallette = new byte[noOfPens][];

                    // Generate a set of color.
                    for (int i = 0; i < noOfPens; i++)
                    {
                        // The accurateness and contrast of the intensity graded displayTrig can be changed here.
                        // With the SQRT less-freuqent signals are "amplified" and more frequent signals are compressed.
                        // The offset will also determine how visible less frequent options are seen.
                        var perc = 0.0f;

                        if (lowContrast)
                            perc = (float)Math.Pow(i * 1.0f / compressionRatio, 0.5) * DisplayContrast + DisplayBrightness;
                        else
                            perc = (float)Math.Pow(i * 1.0f / noOfPens / compressionRatio, 0.5) * DisplayContrast + DisplayBrightness;

                        // Fix perc if <0% or >100% or "ERR"
                        if (perc >= 100)
                            perc = 100;
                        if (perc <= 0)
                            perc = 0;
                        if (float.IsNaN(perc) || float.IsInfinity(perc))
                            perc = 100;

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
                    for (var x = 0; x < GridWidth; x++)
                    {
                        for (var y = 0; y < GridHeight; y++)
                        {
                            var chVal = (int)(channel[x][y]);

                            if (chVal > 0)
                            {
                                // We only modify when there was a hit here.
                                // i contains the index inside the bitmapBuffer we must edit.
                                var i = (y + offsetVerticalDivision) * TotalWidth + x + offsetHorizontalDivision;
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
            renderTime += (float)sw.ElapsedMilliseconds;

            var dt = DateTime.Now.Subtract(lastWfmsMeasurement);
            if (dt.TotalMilliseconds > 500 && waveformsCount > 10 && myWaveforms.Any())
            {
                var wfms = waveformsCount / (dt.TotalMilliseconds / 1000.0f);
                lastMeasurement = wfms.ToString("0000.0 wfms") + " [" + renderTime + "/" + dt.TotalMilliseconds.ToString("000") + "ms] [" + Math.Round(myWaveforms[0].Samples * wfms) + "sps]";
                lastMeasurementColor = renderTime >= dt.TotalMilliseconds - 10 ? Brushes.Red : 
                    renderTime * 1.2 >= dt.TotalMilliseconds ? Brushes.Orange : Brushes.White;
                waveformsCount = 0;
                lastWfmsMeasurement = DateTime.Now;
                renderTime = 0;
            }
            e.Graphics.DrawString(lastMeasurement, new Font("Verdana", 7), lastMeasurementColor, 0, 0);

            if (speed < 0)
                speed = sw.ElapsedMilliseconds;
            else
                speed = speed * 0.6f + sw.ElapsedMilliseconds * 0.4f;
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
        public bool lowContrast;
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

        private void ucPhosphorDisplay_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // We trigger on ch1
            var xDivValue = 1.0*(e.X - HorizontalDivisions*pxPerHorizontalDivision)/pxPerHorizontalDivision;
            var yDivValue = -1.0 * (e.Y - VerticalDivisions * pxPerVerticalDivision) / pxPerVerticalDivision;
            var scalarValueCh1 = yDivValue*this.VerticalScale[0];
            var scalarValueTime = xDivValue*this.HorizontalScale;

            if (DoubleClicked != null)
                DoubleClicked(scalarValueTime, scalarValueCh1);
        }
    }
}