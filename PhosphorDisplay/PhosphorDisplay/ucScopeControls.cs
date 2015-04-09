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
using PhosphorDisplay.Data;
using PhosphorDisplay.Widget;

namespace PhosphorDisplay
{
    public partial class ucScopeControls : UserControl
    {
        private AcquisitionEngine acq;
        private ucPhosphorDisplay displayTrigger;
        private ucPhosphorDisplay displayLong;

        public delegate void UpdateRateChange(float period);
        public event UpdateRateChange UpdateRateChanged;

        private float[] updatePeriods = new float[]
                                        {
                                            20,
                                            40,
                                            50,
                                            80,
                                            100,
                                            125,
                                            200,
                                            250,
                                            333,
                                            400,
                                            500,
                                            800,
                                            1000,
                                            1500,
                                            2000,
                                            2500,
                                            3000,
                                            5000,
                                            10000,
                                        };

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

            foreach (var tr in acq.TriggerSources)
                cbTrigger.Items.Add(tr.Name.ToString());

            cbTrigger.SelectedIndex = 1;
            cbTrigger.SelectedIndexChanged += cbTrigger_SelectedIndexChanged;

            tbAmpPerDiv.Minimum = 0;
            tbSecPerDiv.Minimum = 0;
            tbSecPerDivScnd.Minimum = 0;

            tbAmpPerDiv.Maximum = verticalSteps.Length - 1;
            tbSecPerDiv.Maximum = timeSteps.Length - 1;
            tbSecPerDivScnd.Maximum = timeSteps.Length - 1;

            tbAmpPerDiv.ValueChanged += tbAmpPerDiv_ValueChanged;
            tbSecPerDiv.ValueChanged += tbSecPerDiv_ValueChanged;
            tbSecPerDivScnd.ValueChanged += tbSecPerDivScnd_ValueChanged;

            tbAmpPerDiv.Value = tbAmpPerDiv.Maximum;
            tbSecPerDiv.Value = timeSteps.Count(x => x < 2.0f / 1000);
            tbSecPerDivScnd.Value = timeSteps.Count(x => x < 100.0f / 1000);

            tbRefreshPeriod.Minimum = 0;
            tbRefreshPeriod.Maximum = this.updatePeriods.Length - 1;
            tbRefreshPeriod.Value = 0;

            tbSecPerDivScnd_ValueChanged(null, new EventArgs());
            tbSecPerDiv_ValueChanged(null, new EventArgs());
            tbAmpPerDiv_ValueChanged(null, new EventArgs());
            tbContrast_ValueChanged(null, new EventArgs());
            tbMinBrightness_ValueChanged(null, new EventArgs());

            cbAdcSpeed.SelectedIndex = 0;
            cbAdcType.SelectedIndex = 1;
            cbGain.SelectedIndex = 3;
            cbAdcOversample.SelectedIndex = 0;
            tbContrast.Value = 100;
            tbMinBrightness.Value = 0;

        }
        void cbTrigger_SelectedIndexChanged(object sender, EventArgs e)
        {
            var name = cbTrigger.SelectedItem.ToString();

            if (acq.TriggerSources.Any(x => x.Name == name))
            {
                var trigger = acq.TriggerSources.FirstOrDefault(x => x.Name == name);

                acq.SetTrigger(trigger);
            }
        }

        private void tbSecPerDivScnd_ValueChanged(object sender, EventArgs e)
        {
            var ind = tbSecPerDivScnd.Value;
            var timestep = timeSteps[ind];

            lbSecPerDivScnd.Text = Units.ToUnit(timestep, "s") + "";

            acq.LongAcquisitionTime = timestep;
                displayLong.HorizontalScale = timestep;
        }

        private void tbSecPerDiv_ValueChanged(object sender, EventArgs e)
        {
            var ind = tbSecPerDiv.Value;
            var timestep = timeSteps[ind];


            acq.AcquisitionTime = timestep*displayTrigger.HorizontalDivisions*2;

            if (cbFFT.Checked)
            {
                displayTrigger.HorizontalScale = acq.Source.SampleRate/displayTrigger.HorizontalDivisions/4;
                displayTrigger.HorizontalOffset = displayTrigger.HorizontalScale * displayTrigger.HorizontalDivisions;
                lbSecPerDiv.Text = Units.ToUnit(displayTrigger.HorizontalScale, "Hz") + "/div";
            }
            else
            {
                displayTrigger.HorizontalScale = timestep;
                displayTrigger.HorizontalOffset = 0;
                lbSecPerDiv.Text = Units.ToUnit(timestep, "s") + "/div";
            }
        }

        private int lastSugGain = 1;
        private void tbAmpPerDiv_ValueChanged(object sender, EventArgs e)
        {
            var ind = tbAmpPerDiv.Value;
            var currentstep = verticalSteps[ind];


            displayLong.VerticalScale = new float[3] { currentstep, 1, 1 };

            if (cbFFT.Checked)
            {
                displayTrigger.lowContrast = true;
                displayTrigger.VerticalScale = new float[3] {100*currentstep, 1, 1};
                displayTrigger.VerticalOffset = new float[3] { 5, 0, 0 };
                lbAmpPerDiv.Text = Units.ToUnit(displayTrigger.VerticalScale[0], "dB") + "/div";
            }
            else
            {
                displayTrigger.lowContrast = false;
                displayTrigger.VerticalScale = new float[3] { currentstep, 1, 1 };
                displayTrigger.VerticalOffset = new float[3] { 0, 0, 0 };
                lbAmpPerDiv.Text = Units.ToUnit(currentstep, "A") + "/div";
            }
            var scopeMaxAmplitude = displayTrigger.VerticalDivisions * currentstep;
            // Max amplitude per gain
            var R = 0.1;
            var maxAmplG1 = 13.6 * R;
            var maxAmplG10 = 1.36 * R;
            var maxAmplG100 = 0.136 * R;
            var maxAmplG1000 = 0.0136 * R;
            var sugGain = 1;

            if (scopeMaxAmplitude < maxAmplG1000)
                sugGain = 1000;
            else if (scopeMaxAmplitude < maxAmplG100)
                sugGain = 100;
            else if (scopeMaxAmplitude < maxAmplG10)
                sugGain = 10;
            else if (scopeMaxAmplitude < maxAmplG1)
                sugGain = 1;

            if (sugGain != lastSugGain)
                lbAmpPerDiv.ForeColor = Color.Orange;
            else
                lbAmpPerDiv.ForeColor = Color.White;

            lbSuggestedGain.Text = "Suggested gain: " + sugGain;
        }
        private void btSendCfg_Click(object sender, EventArgs e)
        {
            try
            {
                var osrRatioStr = cbAdcOversample.SelectedItem.ToString();
                var osrRatio = int.Parse(osrRatioStr.Substring(0, osrRatioStr.Length - 1));

                lbAmpPerDiv.ForeColor = Color.White;

                NetStreamConfiguration cfg = new NetStreamConfiguration();
                cfg.OversampleRatio = osrRatio;
                cfg.AfeGain = (int) Math.Pow(10, cbGain.SelectedIndex);
                cfg.AdcSpeed = cbAdcSpeed.SelectedIndex;
                cfg.UseFastAdc = cbAdcType.SelectedIndex == 0;

                acq.Source.Configure(cfg);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cbFFT_CheckedChanged(object sender, EventArgs e)
        {
            acq.FFT = cbFFT.Checked;
            tbSecPerDiv_ValueChanged(sender, e);
            tbAmpPerDiv_ValueChanged(sender, e);
        }

        private void tbContrast_ValueChanged(object sender, EventArgs e)
        {
            lbContrast.Text = "Contrast: " + tbContrast.Value + "%";

            displayTrigger.DisplayContrast = tbContrast.Value;
            displayLong.DisplayContrast = tbContrast.Value;
        }

        private void tbMinBrightness_ValueChanged(object sender, EventArgs e)
        {
            lbBrightness.Text = "Min. Brightness: " + tbMinBrightness.Value + "%";

            displayTrigger.DisplayBrightness = tbMinBrightness.Value;
            displayLong.DisplayBrightness = tbMinBrightness.Value;
        }

        private void lbBrightness_DoubleClick(object sender, EventArgs e)
        {
            tbMinBrightness.Value = 0;
        }

        private void lbContrast_DoubleClick(object sender, EventArgs e)
        {
            tbContrast.Value = 100;
        }

        private void tbRefreshPeriod_ValueChanged(object sender, EventArgs e)
        {
            lbRefrehPeriod.Text = string.Format("{0:0}ms ({1:0.0}Hz)", updatePeriods[tbRefreshPeriod.Value], (1000/updatePeriods[tbRefreshPeriod.Value]));
            if (UpdateRateChanged != null)
                UpdateRateChanged(updatePeriods[tbRefreshPeriod.Value]);
        }
    }
}