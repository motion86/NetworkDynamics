using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkDynamics
{
    public partial class GraphixPlots : Form
    {
        protected List<List<int>> data;
        protected List<string> labels;

        protected int stripHeight, stripSpacing;
        protected int drawAreaHight, drawAreaWidth, drawAreaX, drawAreaY;
        protected int labelX;
        protected static int labelNum;

        protected bool labelsAdded;

        protected PlotResults Hist;

        protected Graphics graphics;

        public GraphixPlots()
        {
            InitializeComponent();
        }

        public GraphixPlots(List<List<int>> data, List<string> labels)
        {
            if (data == null || labels == null || labels.Count == 0 || data.Count == 0) this.Close();
            else
            {
                InitializeComponent();

                this.data = data;
                this.labels = labels;

                stripHeight = 30;
                stripSpacing = 1;

                drawAreaHight = 800;
                drawAreaWidth = 1510;
                drawAreaX = 100;
                drawAreaY = 30;

                labelX = 10;
                labelNum = 0;
                labelsAdded = false;

                numGain.Value = (decimal)(1d / data[0].Count());
                this.Show();

                graphics = this.CreateGraphics();
                PlotLines((float)numRGBr.Value , (float)numRGBg.Value, (float)numRGBb.Value);
            }
        }


        protected void drawBackground()
            // drawBackground – sets up the background for the visualization.
        {
            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(drawAreaX, drawAreaY, drawAreaWidth, drawAreaHight);
            SolidBrush brush1 = new SolidBrush(Color.FromArgb(10, 10, 10));
            graphics.FillRectangle(brush1, drawAreaX, drawAreaY, drawAreaWidth, drawAreaHight);
        }

        protected virtual void PlotLines(float R, float G, float B)
            // PlotLines – performs all the drawing operations needed to visualize the density vectors.
        {

            drawBackground();

            int lineWidth = 2;

            if (data.Count > drawAreaHight / stripHeight) stripHeight = drawAreaHight / (data.Count + 2);

            foreach (int i in LinearAlgebra.range(data.Count))
            {
                lineWidth = drawAreaWidth / data[i].Count;
                int y = drawAreaHight + drawAreaY - stripHeight - (i * (stripHeight + stripSpacing));

                foreach (int j in LinearAlgebra.range(data[i].Count))
                {
                    int color = (int)(255 * ((double)data[i][j] * (double)numGain.Value));
                    if (color > 255) color = 255;
                    SolidBrush brush = new SolidBrush(Color.FromArgb((int)(R * color), (int)(G * color), (int)(B * color)));

                    //Console.WriteLine("i: " + i + "j: " + j);
                    int x = 2 + drawAreaX + lineWidth * j;

                    graphics.FillRectangle(brush, x, y, lineWidth, stripHeight);
                }

                if(!labelsAdded) addLabel(y, labels[i]);
            }
            this.Update();
            labelsAdded = true;
        }

        private void addLabel(int y, string text)
        {
            Label lb = new Label();
            lb.Text = text;
            lb.AutoSize = true;
            lb.Location = new System.Drawing.Point(labelX, y + 4);
            lb.Name = "lb" + labelNum;
            lb.Size = new System.Drawing.Size(32, 13);
            labelNum++;
            this.Controls.Add(lb);
        }
        int k = 0;
        private void btnPlotStack_Click(object sender, EventArgs e)
        {
            PlotLines((float)numRGBr.Value, (float)numRGBg.Value, (float)numRGBb.Value);
        }

        protected void GraphixPlots_MouseClick(object sender, MouseEventArgs e)
        {
            int strip = (drawAreaY + drawAreaHight - e.Y) / (stripHeight + 1);
            
            if (strip < data.Count && strip >= 0)
            {
                if (cbAutoClose.Checked && Hist != null) Hist.Close();

                List<int> data_ = data[strip];
                if (cbSort.Checked)
                {
                    data_ = new List<int>(data[strip]);
                    data_.Sort((a, b) => b.CompareTo(a));
                }

                Hist = new PlotResults(labels[strip], data_);
                Hist.Location = new Point((this.Width - Hist.Width) / 2, (this.Height - Hist.Height) / 2);
                Hist.Show(); 
            }
            Console.WriteLine("Strip: " + strip + " eY: " + e.Y);
        }

        
    }

}
