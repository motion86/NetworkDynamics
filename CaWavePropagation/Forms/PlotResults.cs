using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace NetworkDynamics
{
    public partial class PlotResults : Form
    {
        List<int> data;
        string label;

        public PlotResults(string label, List<int> data)
        {
            InitializeComponent();
            this.data = data;
            this.label = label;
            this.Text = label;
            PlotBins();
            C1.Titles[0].Text = Text;
            C1.MouseDown += (o, e) => { C1.SaveImage(FileIO.GetNewFilePath(Text, "png", "FIGS"), ChartImageFormat.Png); MessageBox.Show("Figure Saved!"); };
        }

        private void PlotBins()
        {
            var plot = C1.Series["S1"];
            plot.Points.Clear();
            int max = 0;
            foreach (int d in data)
            {
                plot.Points.AddY(d);
                if (d > max) max = d;
            }
            C1.ChartAreas[0].AxisY.Maximum = max * 1.05;
            if(MainForm.experimentIndex == 3)
                plot["PixelPointWidth"] = (4).ToString();
            else
                plot["PixelPointWidth"] = (2).ToString();

        }

        public void CloseHist()
        {
            this.Close();
        }
    }
}
