namespace PhosphorDisplay
{
    partial class ucRmsMeter
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
            this.btZero = new System.Windows.Forms.Button();
            this.btClear = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btZero
            // 
            this.btZero.Location = new System.Drawing.Point(129, 22);
            this.btZero.Name = "btZero";
            this.btZero.Size = new System.Drawing.Size(55, 23);
            this.btZero.TabIndex = 0;
            this.btZero.Text = "Zero";
            this.btZero.UseVisualStyleBackColor = true;
            this.btZero.Click += new System.EventHandler(this.btZero_Click);
            // 
            // btClear
            // 
            this.btClear.Location = new System.Drawing.Point(129, 51);
            this.btClear.Name = "btClear";
            this.btClear.Size = new System.Drawing.Size(55, 23);
            this.btClear.TabIndex = 1;
            this.btClear.Text = "Clear";
            this.btClear.UseVisualStyleBackColor = true;
            this.btClear.Click += new System.EventHandler(this.btClear_Click);
            // 
            // ucRmsMeter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btClear);
            this.Controls.Add(this.btZero);
            this.Name = "ucRmsMeter";
            this.Size = new System.Drawing.Size(196, 88);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btZero;
        private System.Windows.Forms.Button btClear;
    }
}
