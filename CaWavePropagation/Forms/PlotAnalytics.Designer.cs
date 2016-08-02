namespace NetworkDynamics
{
    partial class PlotAnalytics
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.C1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.cmMain = new System.Windows.Forms.ContextMenuStrip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.C1)).BeginInit();
            this.SuspendLayout();
            // 
            // C1
            // 
            this.C1.BorderlineColor = System.Drawing.Color.Black;
            this.C1.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chartArea1.AxisX.IsMarksNextToAxis = false;
            chartArea1.AxisX.LabelAutoFitMaxFontSize = 18;
            chartArea1.AxisX.LabelAutoFitMinFontSize = 10;
            chartArea1.AxisX.MajorGrid.LineWidth = 0;
            chartArea1.AxisX2.LineWidth = 0;
            chartArea1.AxisY.LabelAutoFitMaxFontSize = 18;
            chartArea1.AxisY.LabelAutoFitMinFontSize = 10;
            chartArea1.AxisY.MajorGrid.LineWidth = 0;
            chartArea1.AxisY2.LineWidth = 0;
            chartArea1.BackColor = System.Drawing.Color.White;
            chartArea1.Name = "ChartArea1";
            chartArea1.Position.Auto = false;
            chartArea1.Position.Height = 95F;
            chartArea1.Position.Width = 100F;
            chartArea1.Position.Y = 5F;
            this.C1.ChartAreas.Add(chartArea1);
            this.C1.ContextMenuStrip = this.cmMain;
            legend1.Alignment = System.Drawing.StringAlignment.Center;
            legend1.BackColor = System.Drawing.Color.White;
            legend1.Name = "L1";
            this.C1.Legends.Add(legend1);
            this.C1.Location = new System.Drawing.Point(3, 2);
            this.C1.Name = "C1";
            this.C1.Size = new System.Drawing.Size(1024, 768);
            this.C1.TabIndex = 28;
            this.C1.Text = "chart2";
            title1.BackColor = System.Drawing.Color.White;
            title1.DockedToChartArea = "ChartArea1";
            title1.Font = new System.Drawing.Font("Bookman Old Style", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            title1.IsDockedInsideChartArea = false;
            title1.Name = "title";
            title1.Text = "Title Goes Here";
            this.C1.Titles.Add(title1);
            this.C1.Click += new System.EventHandler(this.C1_Click);
            // 
            // cmMain
            // 
            this.cmMain.Name = "cmMain";
            this.cmMain.Size = new System.Drawing.Size(61, 4);
            // 
            // PlotAnalytics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1029, 771);
            this.Controls.Add(this.C1);
            this.MaximumSize = new System.Drawing.Size(1045, 810);
            this.MinimumSize = new System.Drawing.Size(1045, 810);
            this.Name = "PlotAnalytics";
            this.Text = "PlotAnalytics";
            ((System.ComponentModel.ISupportInitialize)(this.C1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip cmMain;
        public System.Windows.Forms.DataVisualization.Charting.Chart C1;
    }
}