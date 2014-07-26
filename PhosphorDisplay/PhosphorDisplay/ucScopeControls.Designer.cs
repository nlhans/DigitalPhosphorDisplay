namespace PhosphorDisplay
{
    partial class ucScopeControls
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabs = new System.Windows.Forms.TabControl();
            this.tbScope = new System.Windows.Forms.TabPage();
            this.lbSecPerDivScnd = new System.Windows.Forms.Label();
            this.tbSecPerDivScnd = new System.Windows.Forms.TrackBar();
            this.cbTrigger = new System.Windows.Forms.ComboBox();
            this.lbSecPerDiv = new System.Windows.Forms.Label();
            this.lbAmpPerDiv = new System.Windows.Forms.Label();
            this.tbSecPerDiv = new System.Windows.Forms.TrackBar();
            this.tbAmpPerDiv = new System.Windows.Forms.TrackBar();
            this.tabADC = new System.Windows.Forms.TabPage();
            this.cbAdcOversample = new System.Windows.Forms.ComboBox();
            this.cbAdcSpeed = new System.Windows.Forms.ComboBox();
            this.lbSuggestedGain = new System.Windows.Forms.Label();
            this.cbGain = new System.Windows.Forms.ComboBox();
            this.cbAdcType = new System.Windows.Forms.ComboBox();
            this.btSendCfg = new System.Windows.Forms.Button();
            this.tabs.SuspendLayout();
            this.tbScope.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbSecPerDivScnd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbSecPerDiv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbAmpPerDiv)).BeginInit();
            this.tabADC.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabs
            // 
            this.tabs.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabs.Controls.Add(this.tbScope);
            this.tabs.Controls.Add(this.tabADC);
            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.Location = new System.Drawing.Point(0, 0);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(192, 400);
            this.tabs.TabIndex = 0;
            // 
            // tbScope
            // 
            this.tbScope.BackColor = System.Drawing.Color.Black;
            this.tbScope.Controls.Add(this.lbSecPerDivScnd);
            this.tbScope.Controls.Add(this.tbSecPerDivScnd);
            this.tbScope.Controls.Add(this.cbTrigger);
            this.tbScope.Controls.Add(this.lbSecPerDiv);
            this.tbScope.Controls.Add(this.lbAmpPerDiv);
            this.tbScope.Controls.Add(this.tbSecPerDiv);
            this.tbScope.Controls.Add(this.tbAmpPerDiv);
            this.tbScope.ForeColor = System.Drawing.Color.White;
            this.tbScope.Location = new System.Drawing.Point(4, 25);
            this.tbScope.Name = "tbScope";
            this.tbScope.Padding = new System.Windows.Forms.Padding(3);
            this.tbScope.Size = new System.Drawing.Size(184, 371);
            this.tbScope.TabIndex = 0;
            this.tbScope.Text = "Scope";
            // 
            // lbSecPerDivScnd
            // 
            this.lbSecPerDivScnd.AutoSize = true;
            this.lbSecPerDivScnd.ForeColor = System.Drawing.Color.Transparent;
            this.lbSecPerDivScnd.Location = new System.Drawing.Point(74, 140);
            this.lbSecPerDivScnd.Name = "lbSecPerDivScnd";
            this.lbSecPerDivScnd.Size = new System.Drawing.Size(35, 13);
            this.lbSecPerDivScnd.TabIndex = 13;
            this.lbSecPerDivScnd.Text = "label1";
            // 
            // tbSecPerDivScnd
            // 
            this.tbSecPerDivScnd.Location = new System.Drawing.Point(6, 108);
            this.tbSecPerDivScnd.Name = "tbSecPerDivScnd";
            this.tbSecPerDivScnd.Size = new System.Drawing.Size(165, 45);
            this.tbSecPerDivScnd.TabIndex = 12;
            // 
            // cbTrigger
            // 
            this.cbTrigger.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTrigger.FormattingEnabled = true;
            this.cbTrigger.Location = new System.Drawing.Point(6, 159);
            this.cbTrigger.Name = "cbTrigger";
            this.cbTrigger.Size = new System.Drawing.Size(165, 21);
            this.cbTrigger.TabIndex = 11;
            // 
            // lbSecPerDiv
            // 
            this.lbSecPerDiv.AutoSize = true;
            this.lbSecPerDiv.ForeColor = System.Drawing.Color.Transparent;
            this.lbSecPerDiv.Location = new System.Drawing.Point(74, 89);
            this.lbSecPerDiv.Name = "lbSecPerDiv";
            this.lbSecPerDiv.Size = new System.Drawing.Size(35, 13);
            this.lbSecPerDiv.TabIndex = 10;
            this.lbSecPerDiv.Text = "label1";
            // 
            // lbAmpPerDiv
            // 
            this.lbAmpPerDiv.AutoSize = true;
            this.lbAmpPerDiv.ForeColor = System.Drawing.Color.Transparent;
            this.lbAmpPerDiv.Location = new System.Drawing.Point(74, 38);
            this.lbAmpPerDiv.Name = "lbAmpPerDiv";
            this.lbAmpPerDiv.Size = new System.Drawing.Size(35, 13);
            this.lbAmpPerDiv.TabIndex = 9;
            this.lbAmpPerDiv.Text = "label1";
            // 
            // tbSecPerDiv
            // 
            this.tbSecPerDiv.Location = new System.Drawing.Point(6, 57);
            this.tbSecPerDiv.Name = "tbSecPerDiv";
            this.tbSecPerDiv.Size = new System.Drawing.Size(165, 45);
            this.tbSecPerDiv.TabIndex = 8;
            // 
            // tbAmpPerDiv
            // 
            this.tbAmpPerDiv.LargeChange = 1;
            this.tbAmpPerDiv.Location = new System.Drawing.Point(6, 6);
            this.tbAmpPerDiv.Name = "tbAmpPerDiv";
            this.tbAmpPerDiv.Size = new System.Drawing.Size(165, 45);
            this.tbAmpPerDiv.TabIndex = 7;
            // 
            // tabADC
            // 
            this.tabADC.BackColor = System.Drawing.Color.Black;
            this.tabADC.Controls.Add(this.cbAdcOversample);
            this.tabADC.Controls.Add(this.cbAdcSpeed);
            this.tabADC.Controls.Add(this.lbSuggestedGain);
            this.tabADC.Controls.Add(this.cbGain);
            this.tabADC.Controls.Add(this.cbAdcType);
            this.tabADC.Controls.Add(this.btSendCfg);
            this.tabADC.ForeColor = System.Drawing.Color.White;
            this.tabADC.Location = new System.Drawing.Point(4, 25);
            this.tabADC.Name = "tabADC";
            this.tabADC.Padding = new System.Windows.Forms.Padding(3);
            this.tabADC.Size = new System.Drawing.Size(184, 371);
            this.tabADC.TabIndex = 1;
            this.tabADC.Text = "A/D";
            // 
            // cbAdcOversample
            // 
            this.cbAdcOversample.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAdcOversample.FormattingEnabled = true;
            this.cbAdcOversample.Items.AddRange(new object[] {
            "1x",
            "2x",
            "4x",
            "8x",
            "16x",
            "32x",
            "64x",
            "128x",
            "256x",
            "512x"});
            this.cbAdcOversample.Location = new System.Drawing.Point(6, 100);
            this.cbAdcOversample.Name = "cbAdcOversample";
            this.cbAdcOversample.Size = new System.Drawing.Size(121, 21);
            this.cbAdcOversample.TabIndex = 20;
            // 
            // cbAdcSpeed
            // 
            this.cbAdcSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAdcSpeed.FormattingEnabled = true;
            this.cbAdcSpeed.Items.AddRange(new object[] {
            "1/1",
            "1/2",
            "1/4",
            "1/8",
            "1/16",
            "1/32",
            "1/64"});
            this.cbAdcSpeed.Location = new System.Drawing.Point(6, 73);
            this.cbAdcSpeed.Name = "cbAdcSpeed";
            this.cbAdcSpeed.Size = new System.Drawing.Size(121, 21);
            this.cbAdcSpeed.TabIndex = 19;
            // 
            // lbSuggestedGain
            // 
            this.lbSuggestedGain.AutoSize = true;
            this.lbSuggestedGain.Location = new System.Drawing.Point(6, 57);
            this.lbSuggestedGain.Name = "lbSuggestedGain";
            this.lbSuggestedGain.Size = new System.Drawing.Size(35, 13);
            this.lbSuggestedGain.TabIndex = 18;
            this.lbSuggestedGain.Text = "label1";
            // 
            // cbGain
            // 
            this.cbGain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGain.FormattingEnabled = true;
            this.cbGain.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.cbGain.Items.AddRange(new object[] {
            "1",
            "10",
            "100",
            "1000"});
            this.cbGain.Location = new System.Drawing.Point(6, 33);
            this.cbGain.Name = "cbGain";
            this.cbGain.Size = new System.Drawing.Size(121, 21);
            this.cbGain.TabIndex = 17;
            // 
            // cbAdcType
            // 
            this.cbAdcType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAdcType.FormattingEnabled = true;
            this.cbAdcType.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.cbAdcType.Items.AddRange(new object[] {
            "STM32F4",
            "MCP3911"});
            this.cbAdcType.Location = new System.Drawing.Point(6, 6);
            this.cbAdcType.Name = "cbAdcType";
            this.cbAdcType.Size = new System.Drawing.Size(121, 21);
            this.cbAdcType.TabIndex = 16;
            // 
            // btSendCfg
            // 
            this.btSendCfg.ForeColor = System.Drawing.Color.Black;
            this.btSendCfg.Location = new System.Drawing.Point(52, 127);
            this.btSendCfg.Name = "btSendCfg";
            this.btSendCfg.Size = new System.Drawing.Size(75, 23);
            this.btSendCfg.TabIndex = 15;
            this.btSendCfg.Text = "Send Cfg";
            this.btSendCfg.UseVisualStyleBackColor = true;
            this.btSendCfg.Click += new System.EventHandler(this.btSendCfg_Click);
            // 
            // ucScopeControls
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.tabs);
            this.Name = "ucScopeControls";
            this.Size = new System.Drawing.Size(192, 400);
            this.tabs.ResumeLayout(false);
            this.tbScope.ResumeLayout(false);
            this.tbScope.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbSecPerDivScnd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbSecPerDiv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbAmpPerDiv)).EndInit();
            this.tabADC.ResumeLayout(false);
            this.tabADC.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tbScope;
        private System.Windows.Forms.Label lbSecPerDivScnd;
        private System.Windows.Forms.TrackBar tbSecPerDivScnd;
        private System.Windows.Forms.ComboBox cbTrigger;
        private System.Windows.Forms.Label lbSecPerDiv;
        private System.Windows.Forms.Label lbAmpPerDiv;
        private System.Windows.Forms.TrackBar tbSecPerDiv;
        private System.Windows.Forms.TrackBar tbAmpPerDiv;
        private System.Windows.Forms.TabPage tabADC;
        private System.Windows.Forms.ComboBox cbAdcSpeed;
        private System.Windows.Forms.Label lbSuggestedGain;
        private System.Windows.Forms.ComboBox cbGain;
        private System.Windows.Forms.ComboBox cbAdcType;
        private System.Windows.Forms.Button btSendCfg;
        private System.Windows.Forms.ComboBox cbAdcOversample;

    }
}
