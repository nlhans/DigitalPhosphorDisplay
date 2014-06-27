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
                g.DrawString(Units.ToUnit(rmsCurrent, "A"), fontBig, Brushes.Turquoise, 8, 25);

                var watts = sixDigitVoltage * rmsCurrent;
                g.DrawString(Units.ToUnit(watts, "W"), fontBig, Brushes.Turquoise, 8, 65);
            }else
            {
                g.DrawString("Mean:", fontSmall, Brushes.Turquoise, 5, 5);
                g.DrawString(Units.ToUnit(meanCurrent, "A"), fontBig, Brushes.Turquoise, 8, 25);

                var ohms = sixDigitVoltage / meanCurrent;
                g.DrawString(Units.ToUnit(ohms, "R"), fontBig, Brushes.Turquoise, 8, 65);
            }
            g.DrawString(Units.ToUnit(sixDigitVoltage, "V"), fontBig, Brushes.Turquoise, 8, 45);

            g.DrawString("Min:", fontSmall, Brushes.Turquoise, 5, 90);
            g.DrawString(Units.ToUnit(minCurrent, "A"), fontBig, Brushes.Turquoise, 8, 105);
            g.DrawString("Max:", fontSmall, Brushes.Turquoise, 5, 130);
            g.DrawString(Units.ToUnit(maxCurrent, "A"), fontBig, Brushes.Turquoise, 8, 145);

        }
    }

    public static class Units
    {
        public static string ToUnit(float v, string appendix)
        {
            if (Math.Abs(v) < 1.0 / 1000)
            {
                return ((v >= 0) ? " " : "") + (v * 1000000).ToString("000.000 ") + "µ" + appendix;
            }
            else if (Math.Abs(v) < 1.0)
            {
                return ((v >= 0) ? " " : "") + (v * 1000).ToString("000.00000") + "m" + appendix;
            }
            else if (Math.Abs(v) < 1000)
            {
                return ((v >= 0) ? " " : "") + v.ToString("000.00000") + appendix;
            }
            else if (Math.Abs(v) < 1000000)
            {
                return ((v >= 0) ? " " : "") + (v / 1000).ToString("000.000") + "k" + appendix;
            }
            else
            {
                return ((v >= 0) ? " " : "") + (v / 1000000).ToString("000.000") + "M" + appendix;
            }
        }
    }
}
