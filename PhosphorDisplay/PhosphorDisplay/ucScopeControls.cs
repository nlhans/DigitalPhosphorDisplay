using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PhosphorDisplay.Acquisition;

namespace PhosphorDisplay
{
    public partial class ucScopeControls : UserControl
    {
        private AcquisitionEngine acq;
        private ucPhosphorDisplay displayTrigger;
        private ucPhosphorDisplay displayLong;

        private float[] timeSteps =
            new float[]
                {
                    0.2f/1000000,
                    0.5f/1000000,
                    1.0f/1000000,
                    2.0f/1000000,
                    5.0f/1000000,
                    10.0f/1000000,
                    20.0f/1000000,
                    50.0f/1000000,
                    100.0f/1000000,
                    200.0f/1000000,
                    500.0f/1000000,
                    1.0f/1000,
                    2.0f/1000,
                    5.0f/1000,
                    10.0f/1000,
                    20.0f/1000,
                    50.0f/1000,
                    100.0f/1000,
                    200.0f/1000,
                    500.0f/1000,
                    1, 2, 5,
                    10, 20, 50,
                    100, 200, 500
                };

        private float[] verticalSteps = new float[]
                                            {
                                                0.1f/1000000,
                                                0.2f/1000000,
                                                0.5f/1000000,
                                                1.0f/1000000,
                                                2.0f/1000000,
                                                5.0f/1000000,
                                                10.0f/1000000,
                                                20.0f/1000000,
                                                50.0f/1000000,
                                                100.0f/1000000,
                                                200.0f/1000000,
                                                500.0f/1000000,
                                                1.0f/1000,
                                                2.0f/1000,
                                                5.0f/1000,
                                                10.0f/1000,
                                                20.0f/1000,
                                                50.0f/1000,
                                                100.0f/1000,
                                                200.0f/1000,
                                                500.0f/1000,
                                            };

        public ucScopeControls(AcquisitionEngine a, ucPhosphorDisplay v1, ucPhosphorDisplay v2)
        {
            acq = a;
            displayTrigger = v1;
            displayLong = v2;

            InitializeComponent();

            tbAmpPerDiv.Minimum = 0;
            tbSecPerDiv.Minimum = 0;
            tbSecPerDivScnd.Minimum = 0;

            tbAmpPerDiv.Maximum = verticalSteps.Length - 1;
            tbSecPerDiv.Maximum = timeSteps.Length - 1;
            tbSecPerDivScnd.Maximum = timeSteps.Length - 1;

            tbAmpPerDiv.ValueChanged += tbAmpPerDiv_ValueChanged;
            tbSecPerDiv.ValueChanged += tbSecPerDiv_ValueChanged;
            tbSecPerDivScnd.ValueChanged += tbSecPerDivScnd_ValueChanged;

            tbAmpPerDiv.Value = tbAmpPerDiv.Maximum - 1;
            tbSecPerDiv.Value = 0;
            tbSecPerDivScnd.Value = tbSecPerDivScnd.Maximum/2;

            tbSecPerDivScnd_ValueChanged(null, new EventArgs());
            tbSecPerDiv_ValueChanged(null, new EventArgs());
            tbAmpPerDiv_ValueChanged(null, new EventArgs());
        }

        private void tbSecPerDivScnd_ValueChanged(object sender, EventArgs e)
        {
            var ind = tbSecPerDivScnd.Value;
            var timestep = timeSteps[ind];

            lbSecPerDivScnd.Text = Units.ToUnit(timestep, "s") + "";

            acq.LongAcquisitionTime = timestep;


            displayLong.horizontalScale = timestep;
        }

        private void tbSecPerDiv_ValueChanged(object sender, EventArgs e)
        {
            var ind = tbSecPerDiv.Value;
            var timestep = timeSteps[ind];

            lbSecPerDiv.Text = Units.ToUnit(timestep, "s") + "/div";

            acq.AcquisitionTime = timestep*displayTrigger.horizontalDivisions*2;

            displayTrigger.horizontalScale = timestep;
        }

        private void tbAmpPerDiv_ValueChanged(object sender, EventArgs e)
        {
            var ind = tbAmpPerDiv.Value;
            var currentstep = verticalSteps[ind];

            lbAmpPerDiv.Text = Units.ToUnit(currentstep, "A") + "/div";

            displayLong.verticalScale = new float[3] {currentstep, 1, 1};
            displayTrigger.verticalScale = new float[3] {currentstep, 1, 1};
        }

    }
}