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
    public partial class TimeTracePlot : Form
    {
        private Dictionary<string, NumericUpDown> netParams;
        private int fileNumber;
        private string dirPath = System.IO.Directory.GetCurrentDirectory() + @"\FIGS\"; // @"C:\Users\Myers\Documents\Shif-Projects\Plots\c#_plots\";
        private string[] paramKeys = {"N","K","A","E","Ga","dT"};
        private double chartXend, chartYend;
        private string netType;

        public TimeTracePlot(Series[] chartSeries, Dictionary<string, NumericUpDown> netParams, string netType)
        {
            InitializeComponent();

            dirPath += DateTime.Today.ToString("D") + "\\";
            tbPath.Text = dirPath;
            chart1.Series.Clear();
            chart1.Series.Add(chartSeries[0]);
            chart1.Series.Add(chartSeries[1]);

            this.netParams = netParams; // N, s, r, gamma, beta, g, c_0
            this.netType = netType;

            // file number counter.
            fileNumber = getLastFileNumber(dirPath) + 1;
            //getLastFileNumber(dirPath);
            // chart autoresize parameters.

            chart1.Titles["title"].Text = GetTitle(true);

            chartXend = this.Size.Width - chart1.Location.X - chart1.Size.Width;
            chartYend = this.Size.Height - chart1.Location.Y - chart1.Size.Height;

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Create new folder
            dirPath = tbPath.Text;
            
            try { System.IO.Directory.CreateDirectory(dirPath); } catch { }

            string imageName = fileNumber.ToString() + $"-{GetTitle(false)}";
            
            this.chart1.SaveImage(dirPath + imageName + ".png", ChartImageFormat.Png);

            fileNumber++;
        }

        private string GetTitle(bool title)
        {
            string imageName = $"{netType}-";
            string[] dl = { "#", "_" };
            if(title) dl = new string[] {" ", ":"};

            foreach (string s in paramKeys)
                imageName += dl[0] + s + dl[1] + netParams[s].Value.ToString($"F{netParams[s].DecimalPlaces}");
            return imageName;
        }

        private void TimePlotAnalysis_SizeChanged(object sender, EventArgs e)
        {
            Point newSize = new Point((int)(this.Size.Width - chart1.Location.X - chartXend), (int)(this.Size.Height - chart1.Location.Y - chartYend));
            chart1.Size = new Size(newSize);
        }

        private void TimePlotAnalysis_FormClosing(object sender, FormClosingEventArgs e)
        {
            chart1.Series.Clear();
        }

        private int getLastFileNumber(string path)
        {
            string[] fileNames;
            int number = -1;
            try
            {
                fileNames = System.IO.Directory.GetFiles(path);
                int index = 0;
                foreach (string s in fileNames)
                {
                    fileNames[index] = System.IO.Path.GetFileName(s);
                    index++;
                }
                
                foreach (string s in fileNames){
                    string sNumber = "";
                    foreach (char c in s)
                    {
                        if (c != '#') sNumber += c;
                        else
                        {
                            int n = int.Parse(sNumber);
                            if (n > number) number = n;
                            break;
                        }
                        
                    }
                }
                
            }
            catch { }
            Console.WriteLine(number);
            return number;
        }
    }
}
