namespace NetworkDynamics
{
    partial class GraphixPlots
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.numGain = new System.Windows.Forms.NumericUpDown();
            this.btnPlotStack = new System.Windows.Forms.Button();
            this.cbSort = new System.Windows.Forms.CheckBox();
            this.cbAutoClose = new System.Windows.Forms.CheckBox();
            this.numRGBb = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numRGBg = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numRGBr = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.numGain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRGBb)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRGBg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRGBr)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 31;
            this.label1.Text = "Gain:";
            // 
            // numGain
            // 
            this.numGain.DecimalPlaces = 3;
            this.numGain.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numGain.Location = new System.Drawing.Point(45, 15);
            this.numGain.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numGain.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numGain.Name = "numGain";
            this.numGain.Size = new System.Drawing.Size(51, 20);
            this.numGain.TabIndex = 30;
            this.numGain.Value = new decimal(new int[] {
            2,
            0,
            0,
            196608});
            // 
            // btnPlotStack
            // 
            this.btnPlotStack.Location = new System.Drawing.Point(8, 847);
            this.btnPlotStack.Name = "btnPlotStack";
            this.btnPlotStack.Size = new System.Drawing.Size(80, 46);
            this.btnPlotStack.TabIndex = 32;
            this.btnPlotStack.Text = "PlotStack";
            this.btnPlotStack.UseVisualStyleBackColor = true;
            this.btnPlotStack.Click += new System.EventHandler(this.btnPlotStack_Click);
            // 
            // cbSort
            // 
            this.cbSort.AutoSize = true;
            this.cbSort.Location = new System.Drawing.Point(193, 15);
            this.cbSort.Name = "cbSort";
            this.cbSort.Size = new System.Drawing.Size(68, 17);
            this.cbSort.TabIndex = 33;
            this.cbSort.Text = "SortData";
            this.cbSort.UseVisualStyleBackColor = true;
            // 
            // cbAutoClose
            // 
            this.cbAutoClose.AutoSize = true;
            this.cbAutoClose.Checked = true;
            this.cbAutoClose.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAutoClose.Location = new System.Drawing.Point(113, 15);
            this.cbAutoClose.Name = "cbAutoClose";
            this.cbAutoClose.Size = new System.Drawing.Size(74, 17);
            this.cbAutoClose.TabIndex = 34;
            this.cbAutoClose.Text = "AutoClose";
            this.cbAutoClose.UseVisualStyleBackColor = true;
            // 
            // numRGBb
            // 
            this.numRGBb.DecimalPlaces = 2;
            this.numRGBb.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.numRGBb.Location = new System.Drawing.Point(193, 38);
            this.numRGBb.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRGBb.Name = "numRGBb";
            this.numRGBb.Size = new System.Drawing.Size(51, 20);
            this.numRGBb.TabIndex = 39;
            this.numRGBb.Value = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(179, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 13);
            this.label4.TabIndex = 40;
            this.label4.Text = "B:";
            // 
            // numRGBg
            // 
            this.numRGBg.DecimalPlaces = 2;
            this.numRGBg.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.numRGBg.Location = new System.Drawing.Point(122, 38);
            this.numRGBg.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRGBg.Name = "numRGBg";
            this.numRGBg.Size = new System.Drawing.Size(51, 20);
            this.numRGBg.TabIndex = 37;
            this.numRGBg.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(102, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(18, 13);
            this.label3.TabIndex = 38;
            this.label3.Text = "G:";
            // 
            // numRGBr
            // 
            this.numRGBr.DecimalPlaces = 2;
            this.numRGBr.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.numRGBr.Location = new System.Drawing.Point(45, 38);
            this.numRGBr.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRGBr.Name = "numRGBr";
            this.numRGBr.Size = new System.Drawing.Size(51, 20);
            this.numRGBr.TabIndex = 35;
            this.numRGBr.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(18, 13);
            this.label2.TabIndex = 36;
            this.label2.Text = "R:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numRGBb);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.numRGBg);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.numRGBr);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cbAutoClose);
            this.groupBox1.Controls.Add(this.cbSort);
            this.groupBox1.Controls.Add(this.numGain);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(94, 838);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(272, 64);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            // 
            // GraphixPlots
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1644, 914);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnPlotStack);
            this.Name = "GraphixPlots";
            this.Text = "GraphixPlots";
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.GraphixPlots_MouseClick);
            ((System.ComponentModel.ISupportInitialize)(this.numGain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRGBb)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRGBg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRGBr)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numGain;
        private System.Windows.Forms.Button btnPlotStack;
        private System.Windows.Forms.CheckBox cbSort;
        private System.Windows.Forms.CheckBox cbAutoClose;
        private System.Windows.Forms.NumericUpDown numRGBb;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numRGBg;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numRGBr;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}