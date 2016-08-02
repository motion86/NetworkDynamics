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
    public partial class PlotAnalytics : Form
    {

        private int seriesNum;
        bool spline;
        bool points;
        /*
        private Color[] chartColors = {
            Color.DarkOrange, Color.DarkRed, Color.DarkSeaGreen, 
            Color.Azure, Color.MintCream, Color.LightPink, Color.LightSkyBlue, Color.LightGray};
        */
        private Color[] chartColors = {
            Color.Black, Color.DarkRed, Color.DarkOrange,
            Color.DarkBlue, Color.DarkSlateGray, Color.DarkMagenta, Color.Brown, Color.CornflowerBlue};

        public PlotAnalytics(List<double> xdata, List<double> ydata, string name, string title)
        {
            InitializeComponent();
            if (title != null)
            {
                this.Text = title;
                C1.Titles[0].Text = title;
            }

            AddSeries(xdata, ydata, name);
            //if (name != null) C1.Series[0].Name = name;
            //if (xdata == null) C1.Series[0].Points.DataBindY(ydata);
            //else C1.Series[0].Points.DataBindXY(xdata, ydata);
            this.Show();
        }
        public void AddSeries(List<double> xdata, List<double> ydata, string name)
        {
            if (seriesNum < chartColors.Length && ydata != null)
            {
                C1.Series.Add(createSeries(chartColors[seriesNum], 1 + seriesNum));
                if (xdata == null) C1.Series[seriesNum].Points.DataBindY(ydata);
                else C1.Series[seriesNum].Points.DataBindXY(xdata, ydata);
                if (name != null) C1.Series[seriesNum].Name = name;
                seriesNum++;
            }
            this.BringToFront();
            cmMain.Items.Add(getSubMenu());
        }

        private ToolStripMenuItem getSubMenu()
        {
            ToolStripMenuItem sub = new ToolStripMenuItem();
            sub.Name = C1.Series[seriesNum - 1].Name;
            sub.Text = C1.Series[seriesNum - 1].Name;

            sub.DropDownItems.Add("Line Points", null, new EventHandler(subHandler));
            sub.DropDownItems.Add("Spline", null, new EventHandler(subHandler));
            sub.DropDownItems.Add("Points", null, new EventHandler(subHandler));
            foreach (ToolStripDropDownItem smi in sub.DropDownItems)
                smi.Tag = sub.Text;
            return sub;
        }

        private Series createSeries(Color color, int sNumber)
        {
            Series newSeries = new Series();

            newSeries.Name = "S" + sNumber;
            newSeries.ChartType = SeriesChartType.Point;
            newSeries.MarkerStyle = MarkerStyle.Circle;
            newSeries.MarkerSize = 4;
            newSeries.Color = color;

            return newSeries;
        }

        private void subHandler(object sender, EventArgs e)
        {
            var options = (ToolStripDropDownItem)sender;

            switch (options.Text)
            {
                case "Line Points":
                    C1.Series[(string)options.Tag].ChartType = SeriesChartType.FastLine;
                    C1.Series[(string)options.Tag].MarkerStyle = MarkerStyle.Circle;
                    break;
                case "Spline":
                    C1.Series[(string)options.Tag].ChartType = SeriesChartType.Spline;
                    C1.Series[(string)options.Tag].MarkerStyle = MarkerStyle.None;
                    break;
                case "Points":
                    C1.Series[(string)options.Tag].ChartType = SeriesChartType.Point;
                    C1.Series[(string)options.Tag].MarkerStyle = MarkerStyle.Circle;
                    C1.Series[(string)options.Tag].MarkerSize = 4;
                    break;
            }

            //MessageBox.Show("Chart Type: " + options.Text + " Tag: " + options.Tag);
        }

        private void C1_Click(object sender, EventArgs e)
        {
            C1.SaveImage(FileIO.GetNewFilePath(Text, "png", "FIGS"), ChartImageFormat.Png);
            /*
            string dir = System.IO.Directory.GetCurrentDirectory() + "\\FIGS\\" + DateTime.Today.ToString("D") + "\\";
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            string name = Text;

            name = name.Replace(':', '#').Replace(' ', '_');
        
            C1.SaveImage(dir + name + "__" + DateTime.Now.ToString("HH-mm-ss") + 
                        ".png", ChartImageFormat.Png);
            */
            MessageBox.Show("Figure Saved!");
        }

    }
}
