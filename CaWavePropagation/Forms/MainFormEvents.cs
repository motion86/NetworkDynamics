using NetworkDynamics.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkDynamics
{
    public partial class MainForm
    {


        // GUI events vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
        // GUI events vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv


        private void trPlay_ValueChanged(object sender, EventArgs e)
        {
            lbPlayTime.Text = (trPlay.Value * numDtStepSize.Value).ToString();
        }
        private void trPlay_MouseUp(object sender, MouseEventArgs e)
        {
            systemUpdate(trPlay.Value);
        }

        private void cbShowMatrix_CheckedChanged(object sender, EventArgs e)
        {
            if (cbShowMatrix.Checked)
            {
                chart1.Series["S1"].Enabled = false;
                chart1.Series["S2"].Enabled = false;
                chart1.Series["S3"].Enabled = true;
            }
            else
            {
                chart1.Series["S1"].Enabled = true;
                chart1.Series["S2"].Enabled = true;
                chart1.Series["S3"].Enabled = false;
            }
        }

        private void comboTby_SelectedIndexChanged(object sender, EventArgs e)
        {
            int a = comboOby.SelectedIndex;
            int b = comboTby.SelectedIndex;
            if (a < 0 || b < 0) MessageBox.Show("Please Select Both Labels!");
            else orderListBox(a, b);
        }

        private void cmDeleteAll_Click(object sender, EventArgs e)
        {
            deleteFiles(true);
        }

        private void deleteSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteFiles(false);
        }

        private void cmArchiveAll_Click(object sender, EventArgs e)
        {
            archiveFiles(true);
        }

        private void cmArchiveSelected_Click(object sender, EventArgs e)
        {
            archiveFiles(false);
        }

        private void When_MouseWheel(object sender, MouseEventArgs e)
        // Scrolls the Archives submenu when mouse wheel is spun.
        {
            if (cmArchiveItems.Selected)
            {
                if (e.Delta > 0) SendKeys.SendWait("{UP}");
                else SendKeys.SendWait("{DOWN}");
            }
        }


        private void cbNetConn_CheckedChanged(object sender, EventArgs e)
        {
            if (cbNetConn.Checked)
            {
                drawConnections(time);
                chart1.Update();
            }
            else chart1.Series["edges"].Points.Clear();
        }

        private void cbTimePlot_CheckedChanged(object sender, EventArgs e)
        {
            if (cbTimePlot.Checked)
            {
                chart2.Series["S1"].Enabled = true;
            }
            else
            {
                chart2.Series["S1"].Enabled = false;
            }
        }

        private void numStartParam_ValueChanged(object sender, EventArgs e)
        {
            myControl.Value = numStartParam.Value;

            expGUIcalculations();
        }


        private void numStopExpCond_ValueChanged(object sender, EventArgs e)
        {
            numStopCond.Value = numStopExpCond.Value;
        }

        private void numMaxIter_ValueChanged(object sender, EventArgs e)
        {
            numDtSteps.Value = numMaxIter.Value;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch ((sender as TabControl).SelectedTab.Name)
            {
                case "tabExp":
                    numDtSteps.Value = numMaxIter.Value;
                    numStopCond.Value = numStopExpCond.Value;
                    break;
                case "tabAdjMat":
                    if (tabAdjMat.Enabled)
                    {
                        numRowColIndex.Maximum = numVertecies.Value;
                        updateTbCount();
                    }
                    break;
                case "tabExploreResults":
                    lbFilesUpdate();
                    break;
            }

        }

        private void lbFilesUpdate()
        {
            try
            {
                findExpFiles(true);
            }
            catch { lbDataFiles.Items.Add("NO ITEMS FOUND!"); expFiles.Clear(); }
        }


        private void numEndParam_ValueChanged(object sender, EventArgs e)
        {
            expGUIcalculations();
        }

        private void expGUIcalculations()
        {
            try
            {
                decimal increment;
                increment = (numEndParam.Value - numStartParam.Value) / (numNumIntervals.Value - 1);
                numIncrement.Increment = increment / 10;
                numIncrement.Value = increment;
                //numEndParam.Value = myControl.Value + numNumRuns.Value * increment;
            }
            catch
            {
                MessageBox.Show("Ops!");
            }
        }

        private void numNumRuns_ValueChanged(object sender, EventArgs e)
        {
            expGUIcalculations();
        }

        private void numIncrement_ValueChanged(object sender, EventArgs e)
        {
            //try { numEndParam.Value = numStartParam.Value + numNumRuns.Value * numIncrement.Value; }catch{}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            plotAdjMatrix();
        }

        private void numRowColIndex_ValueChanged(object sender, EventArgs e)
        {
            updateTbCount();
        }

        private void updateTbCount()
        {
            tbCount.Text = (countConnections(rbRow.Checked, (int)numRowColIndex.Value, (int)numVertecies.Value)).ToString("F1");
        }

        private void rbRow_CheckedChanged(object sender, EventArgs e)
        {
            if (rbRow.Checked) updateTbCount();
        }

        private void rbCol_CheckedChanged(object sender, EventArgs e)
        {
            if (rbCol.Checked) updateTbCount();
        }

        private void btnTimePlot_Click(object sender, EventArgs e)
        {

            System.Windows.Forms.DataVisualization.Charting.Series[] tpSeries = { chart2.Series["S1"], chart2.Series["S2"] };

            Dictionary<string, NumericUpDown> netParams = new Dictionary<string, NumericUpDown>(){
                                                                                                    {"N", numVertecies},
                                                                                                    {"K", numK},
                                                                                                    {"A", numAlpha},
                                                                                                    {"E", numEta},
                                                                                                    {"Ga", numGama},
                                                                                                    {"dT", numDt},
                                                                                                    };

            TimeTracePlot tpForm = new TimeTracePlot(tpSeries, netParams, netTypes[netType]); // N, s, r, gamma, beta, g, c_0 
            tpForm.Show();

        }

        private void btnCalcEigen_Click(object sender, EventArgs e)
        {
            doEigen();
        }

        private void btnShowMat_Click(object sender, EventArgs e)
        {
            T5 = new Thread(() => new MatrixForm(new Matrix(system.adjMat)).ShowDialog()); T5.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            system.SaveAdjMatToFile();
        }

        private void btnSaveEigenCsv_Click(object sender, EventArgs e)
        {
            saveEigenToFile();
        }


        private void cbLoadMatrix_CheckedChanged(object sender, EventArgs e)
        {
            if (cbLoadMatrix.Checked == true)
            {
                var lf = new LoadFile(this, "mat");
                lf.Show();
            }
        }

        // GUI events vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

        private void pbPlay_MouseLeave(object sender, EventArgs e)
        {
            pbPlay.Image = Resources.play;
        }

        private void pbPlay_MouseEnter(object sender, EventArgs e)
        {
            pbPlay.Image = Resources.play_h;
        }

        private void pbPlay_MouseUp(object sender, MouseEventArgs e)
        {
            pbPlay.Image = Resources.play_h;
        }

        private void pbPlay_MouseDown(object sender, MouseEventArgs e)
        {
            pbPlay.Image = Resources.play_d;
            playBack(trPlay.Value, trPlay.Maximum);
            //T3 = new Thread(playBack); T3.Start();
        }

        private void pbPause_MouseEnter(object sender, EventArgs e)
        {
            pbPause.Image = Resources.pause_h;
        }

        private void pbPause_MouseLeave(object sender, EventArgs e)
        {
            pbPause.Image = Resources.pause;
        }

        private void pbPause_MouseDown(object sender, MouseEventArgs e)
        {
            pbPause.Image = Resources.pause_d;
            playGoStop = false;
        }

        private void pbPause_MouseUp(object sender, MouseEventArgs e)
        {
            pbPause.Image = Resources.pause_h;
        }


        private void pbNext_MouseEnter(object sender, EventArgs e)
        {
            pbNext.Image = Resources.next_h;
        }

        private void pbNext_MouseLeave(object sender, EventArgs e)
        {
            pbNext.Image = Resources.next;
        }

        private void pbNext_MouseDown(object sender, MouseEventArgs e)
        {
            pbNext.Image = Resources.next_d;
            int trValue = trPlay.Value;
            if (trValue < trPlay.Maximum)
                playBack(trValue, trValue + 1);
        }

        private void btnLbFilesSelectAll_Click(object sender, EventArgs e)
        // Selects all items in lbDataFiles.
        {
            for (int i = 0; i < lbDataFiles.Items.Count; i++)
                if (i != 0) lbDataFiles.SetSelected(i, true);
        }

        private void cmChartLabels_Click(object sender, EventArgs e)
        {
            ChartLabels.ShowForm(System.Windows.Forms.Cursor.Position);
        }

        private void rbNet3_CheckedChanged(object sender, EventArgs e)
        {
            if (rbNet3.Checked) numNet.Increment = 1;
            else numNet.Increment = 0.1m;
        }

        private void pbNext_MouseUp(object sender, MouseEventArgs e)
        {
            pbNext.Image = Resources.next_h;
        }


        private void pbPrev_MouseEnter(object sender, EventArgs e)
        {
            pbPrev.Image = Resources.previous_h;
        }

        private void pbPrev_MouseLeave(object sender, EventArgs e)
        {
            pbPrev.Image = Resources.previous;
        }

        private void pbPrev_MouseDown(object sender, MouseEventArgs e)
        {
            pbPrev.Image = Resources.previous_d;
            int trValue = trPlay.Value;
            if (trValue > 0)
                playBack(trValue, trValue - 1);
        }

        private void pbPrev_MouseUp(object sender, MouseEventArgs e)
        {
            pbPrev.Image = Resources.previous_h;
        }

        private void btnStopRun_Click(object sender, EventArgs e)
        {
            symRunStop = false;
        }


        /// ////////////////////////////////////////////////////////////////////////////////////

    }
}
