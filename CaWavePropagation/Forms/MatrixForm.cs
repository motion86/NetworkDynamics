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
    public partial class MatrixForm : Form
    {
        private double chartXend, chartYend;
        public MatrixForm(Matrix adjMat)
        {
            InitializeComponent();
            dispMat(adjMat);
            chartXend = this.Size.Width - rtbMat.Location.X - rtbMat.Size.Width;
            chartYend = this.Size.Height - rtbMat.Location.Y - rtbMat.Size.Height;
        }

        private void dispMat(Matrix adjMat)
        {
            Color c;
            foreach (int i in iterate(adjMat.rows))
            {
                rtbMat.AppendText("\n");
                foreach (int j in iterate(adjMat.cols))
                {
                    if (adjMat.getEntry(i, j) != 0d) c = Color.DarkBlue;
                    else c = Color.LightGray;
                    rtbMat.AppendText((adjMat.getEntry(i, j).ToString("F2") + " "), c);
                }
            }
        }
        private static System.Collections.IEnumerable iterate(int range)
        {
            for (int i = 0; i < range; i++) yield return i;
        }

        private void MatrixForm_SizeChanged(object sender, EventArgs e)
        {
            Point newSize = new Point((int)(this.Size.Width - rtbMat.Location.X - chartXend), (int)(this.Size.Height - rtbMat.Location.Y - chartYend));
            rtbMat.Size = new Size(newSize);
        }
    }
}
