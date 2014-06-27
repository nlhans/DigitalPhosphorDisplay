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
            this.tbADC = new System.Windows.Forms.TabPage();
            this.tabs.SuspendLayout();
            this.tbScope.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbSecPerDivScnd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbSecPerDiv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbAmpPerDiv)).BeginInit();
            this.SuspendLayout();
            // 
            // tabs
            // 
            this.tabs.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabs.Controls.Add(this.tbScope);
            this.tabs.Controls.Add(this.tbADC);
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
            // tbADC
            // 
            this.tbADC.BackColor = System.Drawing.Color.Black;
            this.tbADC.ForeColor = System.Drawing.Color.White;
            this.tbADC.Location = new System.Drawing.Point(4, 25);
            this.tbADC.Name = "tbADC";
            this.tbADC.Padding = new System.Windows.Forms.Padding(3);
            this.tbADC.Size = new System.Drawing.Size(184, 371);
            this.tbADC.TabIndex = 1;
            this.tbADC.Text = "A/D";
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
        private System.Windows.Forms.TabPage tbADC;

    }
}
