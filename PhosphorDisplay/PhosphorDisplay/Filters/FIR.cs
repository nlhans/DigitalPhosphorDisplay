using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhosphorDisplay.Filters
{
    public class FIR
    {
        private double[] elements;
        private double[] values;

        public FIR(double[] fils)
        {
            elements = fils;
            values = new double[fils.Length];
        }

        public double Push(double v)
        {
            for (int k = values.Length - 2; k>=0; k--)
            {
                values[k + 1] = values[k];
            }
            values[0] = v;

            return values.Select((t, k) => elements[k]*t).Sum();
        }
    }
}
