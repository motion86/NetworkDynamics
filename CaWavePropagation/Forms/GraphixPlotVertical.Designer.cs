namespace NetworkDynamics
{
    partial class GraphixPlotVertical
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
            this.numRGBb = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numRGBg = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numRGBr = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.cbAutoClose = new System.Windows.Forms.CheckBox();
            this.cbSort = new System.Windows.Forms.CheckBox();
            this.numGain = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.btnPlotStack = new System.Windows.Forms.Button();
            this.numYmax = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.numYmin = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.numScale = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.numLabels = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.cbMap = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numRGBb)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRGBg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRGBr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numYmax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numYmin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLabels)).BeginInit();
            this.SuspendLayout();
            // 
            // numRGBb
            // 
            this.numRGBb.DecimalPlaces = 2;
            this.numRGBb.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.numRGBb.Location = new System.Drawing.Point(41, 64);
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
            this.label4.Location = new System.Drawing.Point(18, 66);
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
            this.numRGBg.Location = new System.Drawing.Point(41, 38);
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
            this.label3.Location = new System.Drawing.Point(17, 40);
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
            this.numRGBr.Location = new System.Drawing.Point(41, 12);
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
            this.label2.Location = new System.Drawing.Point(17, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(18, 13);
            this.label2.TabIndex = 36;
            this.label2.Text = "R:";
            // 
            // cbAutoClose
            // 
            this.cbAutoClose.AutoSize = true;
            this.cbAutoClose.Checked = true;
            this.cbAutoClose.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAutoClose.Location = new System.Drawing.Point(6, 240);
            this.cbAutoClose.Name = "cbAutoClose";
            this.cbAutoClose.Size = new System.Drawing.Size(74, 17);
            this.cbAutoClose.TabIndex = 34;
            this.cbAutoClose.Text = "AutoClose";
            this.cbAutoClose.UseVisualStyleBackColor = true;
            // 
            // cbSort
            // 
            this.cbSort.AutoSize = true;
            this.cbSort.Location = new System.Drawing.Point(6, 217);
            this.cbSort.Name = "cbSort";
            this.cbSort.Size = new System.Drawing.Size(68, 17);
            this.cbSort.TabIndex = 33;
            this.cbSort.Text = "SortData";
            this.cbSort.UseVisualStyleBackColor = true;
            // 
            // numGain
            // 
            this.numGain.DecimalPlaces = 3;
            this.numGain.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numGain.Location = new System.Drawing.Point(41, 90);
            this.numGain.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numGain.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            393216});
            this.numGain.Name = "numGain";
            this.numGain.Size = new System.Drawing.Size(51, 20);
            this.numGain.TabIndex = 30;
            this.numGain.Value = new decimal(new int[] {
            2,
            0,
            0,
            196608});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 92);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 31;
            this.label1.Text = "Gain:";
            // 
            // btnPlotStack
            // 
            this.btnPlotStack.Location = new System.Drawing.Point(6, 291);
            this.btnPlotStack.Name = "btnPlotStack";
            this.btnPlotStack.Size = new System.Drawing.Size(86, 46);
            this.btnPlotStack.TabIndex = 36;
            this.btnPlotStack.Text = "PlotStack";
            this.btnPlotStack.UseVisualStyleBackColor = true;
            this.btnPlotStack.Click += new System.EventHandler(this.btnPlotStack_Click_1);
            // 
            // numYmax
            // 
            this.numYmax.DecimalPlaces = 2;
            this.numYmax.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numYmax.Location = new System.Drawing.Point(41, 116);
            this.numYmax.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numYmax.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numYmax.Name = "numYmax";
            this.numYmax.Size = new System.Drawing.Size(51, 20);
            this.numYmax.TabIndex = 41;
            this.numYmax.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 118);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(36, 13);
            this.label5.TabIndex = 42;
            this.label5.Text = "Ymax:";
            // 
            // numYmin
            // 
            this.numYmin.DecimalPlaces = 2;
            this.numYmin.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numYmin.Location = new System.Drawing.Point(41, 142);
            this.numYmin.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numYmin.Name = "numYmin";
            this.numYmin.Size = new System.Drawing.Size(51, 20);
            this.numYmin.TabIndex = 43;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 144);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(33, 13);
            this.label6.TabIndex = 44;
            this.label6.Text = "Ymin:";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(6, 383);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(86, 46);
            this.btnSave.TabIndex = 45;
            this.btnSave.Text = "SaveFig";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // numScale
            // 
            this.numScale.DecimalPlaces = 2;
            this.numScale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numScale.Location = new System.Drawing.Point(41, 357);
            this.numScale.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numScale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numScale.Name = "numScale";
            this.numScale.Size = new System.Drawing.Size(47, 20);
            this.numScale.TabIndex = 46;
            this.numScale.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 359);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(37, 13);
            this.label7.TabIndex = 47;
            this.label7.Text = "Scale:";
            // 
            // numLabels
            // 
            this.numLabels.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numLabels.Location = new System.Drawing.Point(41, 182);
            this.numLabels.Name = "numLabels";
            this.numLabels.Size = new System.Drawing.Size(51, 20);
            this.numLabels.TabIndex = 48;
            this.numLabels.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 184);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 13);
            this.label8.TabIndex = 49;
            this.label8.Text = "Labels:";
            // 
            // cbMap
            // 
            this.cbMap.AutoSize = true;
            this.cbMap.Location = new System.Drawing.Point(6, 264);
            this.cbMap.Name = "cbMap";
            this.cbMap.Size = new System.Drawing.Size(82, 17);
            this.cbMap.TabIndex = 50;
            this.cbMap.Text = "Map A => S";
            this.cbMap.UseVisualStyleBackColor = true;
            // 
            // GraphixPlotVertical
            // 
            this.AcceptButton = this.btnPlotStack;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1484, 982);
            this.Controls.Add(this.cbMap);
            this.Controls.Add(this.numLabels);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.numScale);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.numYmin);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.numYmax);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cbSort);
            this.Controls.Add(this.cbAutoClose);
            this.Controls.Add(this.numRGBb);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numGain);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnPlotStack);
            this.Controls.Add(this.numRGBg);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numRGBr);
            this.Controls.Add(this.label2);
            this.Name = "GraphixPlotVertical";
            this.Text = "GraphixPlotVertical";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GraphixPlotVertical_MouseDown);
            ((System.ComponentModel.ISupportInitialize)(this.numRGBb)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRGBg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRGBr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numYmax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numYmin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLabels)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPlotStack;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numGain;
        private System.Windows.Forms.CheckBox cbSort;
        private System.Windows.Forms.CheckBox cbAutoClose;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numRGBr;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numRGBg;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numRGBb;
        private System.Windows.Forms.NumericUpDown numYmax;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numYmin;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.NumericUpDown numScale;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown numLabels;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox cbMap;
    }
}