using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhosphorDisplay
{
    public partial class ucRmsMeter : UserControl
    {
        public bool displayRms = true;

        public float rmsCurrent = 0.0f;
        public float meanCurrent = 0.0f;
        public float sixDigitVoltage = 0.0f;
        public float minCurrent = 0.0f;
        public float maxCurrent = 0.0f;

        public ucRmsMeter()
        {
            InitializeComponent();

            DoubleClick += ucRmsMeter_DoubleClick;
        }

        void ucRmsMeter_DoubleClick(object sender, EventArgs e)
        {
            displayRms = !displayRms;
            this.Invalidate();
        }

        private string ToUnit(float v, string appendix)
        {
            if (Math.Abs(v) < 1.0/1000)
            {
                return ((v>=0)?" ":"") + (v * 1000000).ToString("000.00  ") + "µ" + appendix;
            }
            else if (Math.Abs(v) < 1.0)
            {
                return ((v >= 0) ? " " : "") + (v * 1000).ToString("000.0000") + "m" + appendix;
            }
            else
            {
                return ((v >= 0) ? " " : "") + v.ToString("000.0000") +  appendix;
            }
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.FillRectangle(Brushes.Black, e.ClipRectangle);

            var fontBig = new Font("Digital-7 Mono", 16.0f);
            var fontSmall = new Font("Digital-7 Mono", 14.0f);

            if (displayRms)
            {
                g.DrawString("RMS:", fontSmall, Brushes.Turquoise, 5, 5);
                g.DrawString(ToUnit(rmsCurrent, "A"), fontBig, Brushes.Turquoise, 8, 25);
            }else
            {
                g.DrawString("Mean:", fontSmall, Brushes.Turquoise, 5, 5);
                g.DrawString(ToUnit(meanCurrent, "A"), fontBig, Brushes.Turquoise, 8, 25);
            }
            g.DrawString(ToUnit(sixDigitVoltage, "V"), fontBig, Brushes.Turquoise, 8, 45);

            g.DrawString("Min:", fontSmall, Brushes.Turquoise, 5, 70);
            g.DrawString(ToUnit(minCurrent, "A"), fontBig, Brushes.Turquoise, 8, 85);
            g.DrawString("Max:", fontSmall, Brushes.Turquoise, 5, 110);
            g.DrawString(ToUnit(maxCurrent, "A"), fontBig, Brushes.Turquoise, 8, 125);

        }
    }
}
