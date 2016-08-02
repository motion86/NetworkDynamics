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
    public partial class GraphixPlotVertical : Form
    {
        protected List<List<int>> data;
        protected List<string> labels;

        protected float stripHeight, stripSpacing;
        protected int drawAreaHight, drawAreaWidth, drawAreaX, drawAreaY;
        private int borderXoffset, borderYoffset, borederBottomOffset, borderStroke;
        protected int labelY;
        protected static int labelNum;

        protected bool labelsAdded, remapped;

        protected PlotResults Hist;

        protected Graphics graphics;

        private static int axisFontSize = 14;
        private static int titleFontSize = 18;

        Font drawFont = new Font("Bookman Old Style", axisFontSize, FontStyle.Regular);
        SolidBrush drawBrush = new SolidBrush(Color.Black);

        public GraphixPlotVertical(List<List<int>> data, List<string> labels, string title)
        {
            if (data == null || labels == null || labels.Count == 0 || data.Count == 0) this.Close();
            else
            {
                InitializeComponent();

                this.data = data;
                this.labels = labels;

                stripHeight = 30;
                stripSpacing = 0;

                drawAreaHight = 850;
                drawAreaWidth = 1350;
                drawAreaX = 110;
                drawAreaY = 20;

                borderXoffset = 5; // border width
                borderYoffset = 5;
                borederBottomOffset = 100;
                borderStroke = 2;

                labelY = drawAreaHight + drawAreaY + 3;
                labelNum = 0;
                labelsAdded = false;

                numGain.Value = (decimal)(1d / data[0].Count());

                Text = title;

                if (cbMap.Checked)
                    remapVar(labels, title);

                Show();

                graphics = CreateGraphics();
                PlotLines(graphics);

                numYmax.Focus();
            }
        }

        private void remapVar(List<string> labels, string title)
        {
            double p = getPar(title, "K")/100d;
            double N = getPar(title, "N");

            for (int i = 0; i < labels.Count; i++)
            {
                double value = Double.Parse(labels[i].Slice(labels[i].IndexOf(":") + 1, labels[i].Length)); // get the value of Lambda.

                labels[i] = $"S:{value*N*p:F3}"; // reformat the label.
            }
            remapped = true;
        }

        private double getPar(string title, string par)
        {
            double myPar = -1;
            int iPar = title.IndexOf(par);
            if (iPar > 0)
            {
                string temp = title.Slice(iPar + 2, title.IndexOf(" ", iPar));
                myPar = Double.Parse(temp);
            }
            else return -1;

            return myPar;
        }
        

        private void GraphixPlotVertical_MouseDown(object sender, MouseEventArgs e)
        {
            int strip = (int)((e.X - drawAreaX) / (stripHeight + stripSpacing));

            if (strip < data.Count && strip >= 0)
            {
                if (cbAutoClose.Checked && Hist != null) Hist.Close();

                List<int> data_ = data[strip];
                if (cbSort.Checked)
                {
                    data_ = new List<int>(data[strip]);
                    data_.Sort((a, b) => b.CompareTo(a));
                }

                Hist = new PlotResults(Text + " " + labels[strip], data_);
                Hist.Location = new Point((this.Width - Hist.Width) / 2, (this.Height - Hist.Height) / 2);
                Hist.Show();
            }
            Console.WriteLine("Strip: " + strip + " eX: " + e.X);
        }

        private void btnPlotStack_Click_1(object sender, EventArgs e)
        {
            //PlotLines(graphics, (float)numRGBr.Value, (float)numRGBg.Value, (float)numRGBb.Value);
            if (cbMap.Checked && !remapped)
                remapVar(labels, Text);
            PlotLines(graphics);
        }
        protected void drawBackground(Graphics gfx) { drawBackground(gfx, 0, 0); }
        protected void drawBackground(Graphics gfx, int extendX, int extendY)
        // drawBackground – sets up the background for the visualization.
        {
            //Rectangle rectangle1 = new Rectangle(drawAreaX, drawAreaY, drawAreaWidth, drawAreaHight);
            //Rectangle rectangle2 = new Rectangle(drawAreaX, drawAreaY, drawAreaWidth, drawAreaHight);
            SolidBrush brush1 = new SolidBrush(Color.FromArgb(255, 255, 255));
            Pen pen = new Pen(Color.Black);
            pen.Width = borderStroke;

            // draw border.
            gfx.FillRectangle(brush1,   drawAreaX - borderXoffset, 
                                        drawAreaY - borderYoffset, 
                                        drawAreaWidth + 2 * borderXoffset + extendX, 
                                        drawAreaHight + borederBottomOffset + extendY);

            gfx.DrawRectangle(pen,  drawAreaX - borderXoffset, 
                                    drawAreaY - borderYoffset, 
                                    drawAreaWidth + 2 * borderXoffset + extendX, 
                                    drawAreaHight + borederBottomOffset + extendY);

            brush1.Color = Color.FromArgb(10, 10, 10);
            gfx.FillRectangle(brush1, drawAreaX, drawAreaY, drawAreaWidth, drawAreaHight);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SavePlot();
        }
        protected void PlotLines(Graphics gfx) { PlotLines(gfx, 0, 0); }
        protected void PlotLines(Graphics gfx, int extendX, int extendY)
        // PlotLines – performs all the drawing operations needed to visualize the density vectors.
        { // widths and heights are mixed up

            drawBackground(gfx, extendX, extendY);

            float lineWidth = 2f;

            //if (data.Count > drawAreaWidth / stripHeight)
            stripHeight = (float)drawAreaWidth / (data.Count) - stripSpacing;

            SolidBrush brush = new SolidBrush(Color.Black);

            int maxLabels = (int)numLabels.Value;

            int skipLabel = (labels.Count + maxLabels - 1)/ maxLabels;

            foreach (int i in LinearAlgebra.range(data.Count))
            {
                int maxLines = (int)(data[i].Count * (float)numYmax.Value);
                int minLines = (int)(data[i].Count * (float)numYmin.Value);

                lineWidth = (float)drawAreaHight / (maxLines - minLines);
                float x = drawAreaX + (i * (stripHeight + stripSpacing));

                for (int j = minLines; j < maxLines; j++)
                {
                    setBrush(ref brush, data[i][j]);

                    float y = lineWidth + drawAreaY + lineWidth * (j - minLines);

                    gfx.FillRectangle(brush, x, (drawAreaHight + 2 * drawAreaY) - y, stripHeight, lineWidth);
                }

                //if (!labelsAdded) VLabel(labels[i], x , labelY);
                if(i%skipLabel == 0) // skip excessive labels
                    VLabel(gfx, labels[i], x, labelY, true);
            }
            //drawBrush.Dispose();
            //drawFont.Dispose();
            //this.Update();
            labelsAdded = true;
        }

        private void setBrush(ref SolidBrush brush, int dataPt)
        {
            int dynamicRange = 1024;
            int color = (int)(dynamicRange * (dataPt * (float)numGain.Value));
            if (color > dynamicRange) color = dynamicRange;
            brush.Color = ColorTools.IntToGradient(color, dynamicRange);
            //brush.Color = Color.FromArgb((int)(R * color), (int)(G * color), (int)(B * color));
        }

        private void VLabel(Graphics gfx, string drawString, float x, float y, bool vertical)
        {
            gfx.TextContrast = 0;
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (vertical)
            {
                StringFormat drawFormat = new StringFormat(StringFormatFlags.DirectionVertical);
                gfx.DrawString(drawString, drawFont, drawBrush, x, y, drawFormat);
            }
            else
                gfx.DrawString(drawString, drawFont, drawBrush, x, y);
        }

        private void SavePlot()
        {
            int temp_x = drawAreaX;
            drawAreaX = 20;
            int x_offset = 20, y_offset = 30;
            changeOffset(x_offset, y_offset);

            int extendY = 20; //adds extra height to accomodate labels

            int sizeX = (int)(drawAreaWidth + drawAreaX + borderXoffset);
            int sizeY = (int)(drawAreaY + drawAreaHight + borederBottomOffset - borderYoffset + extendY); 

            Bitmap bmap = new Bitmap(sizeX, sizeY, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Graphics gfx = Graphics.FromImage(bmap);
            float scale = (float)numScale.Value;

            SolidBrush brush1 = new SolidBrush(Color.FromArgb(0, 100, 0));
            gfx.FillRectangle(brush1, 0, 0, sizeX, sizeY);

            PlotLines(gfx, 0, extendY);

            //int titleFsize = 12;

            drawFont = new Font("Bookman Old Style", titleFontSize, FontStyle.Regular);

            int titleX = (int)(((drawAreaWidth + 2f * borderXoffset + drawAreaX) / 2f - titleFontSize / 2 * Text.Length)/0.75f);
            VLabel(gfx, Text, titleX, y_offset/2, false);

            drawFont = new Font("Bookman Old Style", axisFontSize, FontStyle.Regular);

            Bitmap newBmap = new Bitmap(bmap, new Size((int)(bmap.Width * scale), (int)(bmap.Height * scale)));
            newBmap.Save(FileIO.GetNewFilePath(Text,"png","FIGS"), System.Drawing.Imaging.ImageFormat.Png);

            changeOffset(-x_offset, -y_offset);
            drawAreaX = temp_x;
            
            gfx.Dispose();
            bmap.Dispose();
            newBmap.Dispose();

            MessageBox.Show("Figure Saved!");
        }

        private void changeOffset(int x, int y)
            // changeOffset - changes the border dimentions.
        {
            drawAreaX += x;
            drawAreaY += y;

            borderXoffset = drawAreaX; // border width
            borderYoffset = drawAreaY;
            borederBottomOffset = 100 + y;
            borderStroke = 2;

            labelY = drawAreaHight + drawAreaY + 3;
        }
    }
}
