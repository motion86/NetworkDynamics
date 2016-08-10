using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using NetworkDynamics.Properties;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;

namespace NetworkDynamics
{
    public partial class MainForm : Form
    {
        //VARIABLE DECLARATION vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

        private VertexSystem system;             // contains main system components.
        private SystemParameters parameters;    // container for all system parameters.

        private Matrix adjMatEigen;
        private List<int[]> timeEvolutionSeq;   //double[] - (N)x(1) column vector representing the CRUs (open/closed) state at time t

        private List<VertexData> vertexCinfo = new List<VertexData>();

        private Random rdn = new Random();

        private NumericUpDown myControl;

        private delegate void DELEGATE();

        private int PT_ACTIVE_SIZE = 6, PT_INACTIVE_SIZE = 2, time = 0, chart3ptCount = 0;

        private bool playGoStop = true, symRunStop = false, netGen = false, symRunning = false;

        Thread T5;

        private List<string> expFiles; // stores the file paths to experiment files loaded in ExploreResults tab.
        private List<string> archivedItems; // stores the file paths to experiment files loaded in ExploreResults tab.

        private string[] netTypes = { "DF", "ER", "BA", "CH" };                 // Diffusive, Erdős–Rényi, Barabási–Albert
        private string[] experimentCode = { "X1", "X2", "X3", "X4", "X5", "X6" }; // X1 - ; X2 - ; 
                                                                                  // X3 - Examines the power of the top 10 sites vs the remaining.
                                                                                  // X4 - Produces a density of active sites over a long run for a range of the given parameter.
                                                                                  // X5 - Same as X4 but mixes 2 initial conditions (0 plus the one provided by the user) to provide a filler picture. 
        public static int experimentIndex;
        private int netType;

        private System.Timers.Timer timer;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        public MainForm()
        {
            InitializeComponent();

            for (int i = 1; i < gbSelectParameters.Controls.Count; i++)
            {
                try
                {
                    RadioButton rdb = (RadioButton)gbSelectParameters.Controls[i];
                    rdb.CheckedChanged += new System.EventHandler(gbSelectParameters_CheckedChanged);
                }
                catch { }
            }
            tbarParamSet.ValueChanged += new System.EventHandler(tbarParamSet_ValueChanged);

            tsInfoLabel.Visible = false;
            tsStatusBar.Visible = false;

            enableControls(false);

            this.MouseWheel += When_MouseWheel;
        }



        private void gbSelectParameters_CheckedChanged(object sender, EventArgs e)
        {


            if (sender == rb_e) myControl = numEta; // myButton = "Eta";
            else if (sender == rb_beta) myControl = numDt; // myButton = "BETA";
            else if (sender == rb_gamma) myControl = numGama; // myButton = "GAMMA";
            else if (sender == rb_net_con) myControl = numNet; // myButton = "NETCON";
            else if (sender == rb_num_cru) myControl = numVertecies; // myButton = "NUMCRU";
            else if (sender == rb_a) myControl = numAlpha; // myButton = "Alpha";
            else myControl = numK; //myButton = "K";

            tbarParamSet.Value = (int)((myControl.Value - myControl.Minimum) * tbarParamSet.Maximum / (myControl.Maximum - myControl.Minimum));

            numStartParam.Minimum = myControl.Minimum;
            numStartParam.Maximum = myControl.Maximum;
            numStartParam.Increment = myControl.Increment;
            numStartParam.Value = myControl.Value;

            numEndParam.Minimum = myControl.Minimum;
            numEndParam.Maximum = myControl.Maximum;
            numEndParam.Increment = myControl.Increment;
            numStartParam.Value = myControl.Value;

            numIncrement.Value = myControl.Increment;
            numIncrement.Maximum = myControl.Maximum;
            numEndParam.Increment = numEndParam.Value / 50;
        }

        private void tbarParamSet_ValueChanged(object sender, EventArgs e)
        {
            decimal increment = (myControl.Maximum - myControl.Minimum) / tbarParamSet.Maximum;
            myControl.Value = (tbarParamSet.Value) * increment + myControl.Minimum;
            tbParamSet.Text = (myControl.Value).ToString();
            try
            {
                numStartParam.Value = myControl.Value;
                numIncrement.Increment = increment;
                numEndParam.Value = myControl.Value + numNumIntervals.Value * increment;
            }
            catch { }

        }

        private void button1_Click(object sender, EventArgs e)
        //button1_Click: generates the random placement of CRU and places them in the plotting area
        {
            //if (cbSeed.Checked) rdn = new Random((int)numSeed.Value);
            //else rdn = new Random();
            if (rbNet2.Checked)
            {
                numK.Value = numNet.Value;
                double n = (double)numVertecies.Value;
                if ((Math.Log(n) / n) < ((double)numNet.Value / 100d)) // determine if ER network is connected or not
                    tbTime.Text = "Connected!";
                else
                    tbTime.Text = "Disconnect!";
            }
            if (rbNet3.Checked) numK.Value = (int)numNet.Value;
            if (cbNew.Checked) VertexData.ClearRank();
            genNet(null);

        }

        private void btnRun_Click(object sender, EventArgs e)
        //Performs the main simulation sewuence
        {
            runSym();
        }

        public SystemParameters packSystemParameters()
        // packSystemParameters - returns a new SystemParameters object populated with current param values.
        {
            List<string[]> specPars = null;
            if (tbSpecialPars.Text != (string)tbSpecialPars.Tag) // get the special pars.
                specPars = FileIO.ExtractTokens($"{tbSpecialPars.Text}.", '_', ':');

            return new SystemParameters((double)numAlpha.Value,
                                                (double)numEta.Value,
                                                (double)numGama.Value,
                                                (double)numBeta.Value,
                                                (double)numEtaNeg.Value,
                                                (double)numDt.Value,
                                                (double)numNet.Value,
                                                (int)numVertecies.Value,
                                                (double)numK.Value,
                                                (int)numClusters.Value,
                                                netTypes[netType],
                                                new CoordPoint((double)numX.Value, (double)numY.Value),
                                                cbDirected.Checked,
                                                cbRandWeights.Checked,
                                                specPars);
        }

        public void UnpackParameters(SystemParameters pars)
        // UnpackParameters - updates the GUI values with the ones passed in.
        {
            numAlpha.Value = (decimal)pars.Alpha;
            numEta.Value = (decimal)pars.Eta;
            numGama.Value = (decimal)pars.Gamma;
            numBeta.Value = (decimal)pars.Beta;
            numEtaNeg.Value = (decimal)pars.EtaNeg;
            numDt.Value = (decimal)pars.dt;
            numNet.Value = (decimal)pars.K;
            numVertecies.Value = (decimal)pars.N;
            numK.Value = (decimal)pars.K;
            numClusters.Value = (decimal)pars.nClustes;
            netTypes[netType] = pars.NetType;
            numX.Value = (decimal)pars.bc.x;
            numY.Value = (decimal)pars.bc.y;
            cbDirected.Checked = pars.DirectedNetwork;
            cbRandWeights.Checked = pars.Weights;
        }

        public void genNet(string path)
        // genNet - initializes the system.
        {
            //numVertecies.Value = distMatrix.Count;

            setNetType(); // set the network type from radio buttons on GUI
            parameters = packSystemParameters();

            if (parameters.nClustes > 1) // corect network size for clustered system.
            {
                parameters.N = (parameters.N / parameters.nClustes) * parameters.nClustes;
                numVertecies.Value = parameters.N;
            }

            netGen = false;
            if (cbNew.Checked)
            {
                vertexCinfo.Clear();
                VertexData.Clear();
            }
            bool loadMat = false;
            if (path != null)
            {
                try
                {
                    //T2 = new Thread(() => loadMatrix(path)); T2.Start();
                    //T2.Join();
                    system = new VertexSystem(parameters, (double)numOnProb.Value, cbCircle.Checked, path); //throw if file name was incompatible.
                    UnpackParameters(parameters); // update GUI values.
                    if (parameters.N > 0)
                    {
                        tsInfoLabel.Text = "Matrix was successfully loaded!";
                        tsInfoLabel.Visible = true;
                    }

                    loadMat = true;
                    initiateVertices(loadMat);
                    //setUpNetSeries();
                    netGen = true;
                    enableControls(netGen);
                }
                catch { MessageBox.Show("Failed To Load Matrix!"); }
            }
            else if (!loadMat)
            {
                initiateVertices(loadMat);
                //setUpNetSeries();
                netGen = true;
                //enableControls(netGen);
            }
        }

        private async void initiateVertices(bool loadMat)
        //button1_Click: generates the random placement of CRU and places them in the plotting area
        {
            enableControls(false);
            time = 0;
            chart3ptCount = 0;
            bool newSys = cbNew.Checked;


            if (!newSys)
            {
                bool temp_weights = system.parameters.Weights; // this parameter can be modified internaly -> remember it's current state before passing in new struct.
                system.parameters = parameters;
                system.parameters.Weights = temp_weights;
                system.ResetState((double)numOnProb.Value);
            }

            if (newSys && !loadMat)
            {
                // show flashing msg while adjMat is being generated.
                timer = new System.Timers.Timer(1000);

                bool tick = true;
                string msg1 = "AdjMat Generator Running!";
                string msg2 = "!!!!!!!!!!!!!!!!!!!!!!!!!";

                tsMatGenerator.Enabled = true; tsMatGenerator.Visible = true;

                timer.Elapsed += (o, e) =>
                {
                    if (tick) tsMatGenerator.Text = msg1;
                    else tsMatGenerator.Text = msg2;
                    tick = !tick;
                    timer.Start();
                };

                timer.AutoReset = false;
                timer.Start();

                // initiate system
                system = new VertexSystem(parameters, (double)numOnProb.Value, cbCircle.Checked,
                                                            async (o, e) =>  //this code will be executed when the adjMat is ready.
                                                            {
                                                                await SetUpVizualizationComponents(newSys);
                                                            });
            }
            else if (newSys && loadMat)
                await SetUpVizualizationComponents(newSys);
            if(!newSys)
                await SetUpVizualizationComponents(newSys);
        }

        private async Task SetUpVizualizationComponents(bool newSys)
            // SetUpVizualizationComponents - 
        {
            if (newSys)
            {
                for (int i = 0; i < parameters.N; i++) vertexCinfo.Add(new VertexData());
                setUpNetSeries(); // add the edjes to the graphing area.
                await Task.Run(() => getNetProperties());
                await Task.Run(() => markHighOutbound((uint)numTopNodes.Value));

                if (cbCalcDistMat.Checked)
                    await Task.Run(() => setUpDistMatrixPlot()); // prepare adjMat density plot.

                adjMatEigen = new Matrix(system.adjMat);
                enableControls(true);
                tsMatGenerator.Enabled = false; tsMatGenerator.Visible = false;
            }

            chartSetup(); // initializes chart params.

            timeEvolutionSeq = new List<int[]>((int)numDtSteps.Value);

            plotVertices(newSys); // plots the vertices on chart1

            chart1.Update();

            chart1.Series["S3"].Enabled = false;

            chart1.Update();
            if (!newSys) enableControls(true);
        }        

        private void enableControls(bool e)
        // Enable or disable controls which require an Adj Matrix to be loaded or created first.
        {
            tabExp1.Enabled = e;
            tabEigen.Enabled = e;
            tabAdjMat.Enabled = e;
            btnRun.Enabled = e;
        }

        private void recordStep(bool record)
        // recordStep - records the current step for later playback
        {
            if (record)
            {
                timeEvolutionSeq.Add(new int[parameters.N]);
                Parallel.For(0, parameters.N, i => { timeEvolutionSeq[time + 1][i] = (int)system.systemState[i][2]; });
            }
        }

        private async void runSym()
        // runSym - Main simulation sequence.
        {
            symRunning = true;
            int runUntil = (int)numDtSteps.Value, run = 0, stopCount = 0;
            bool record = cbRecord.Checked;
            symRunStop = true;

            while (stopCount < 5)
            {
                if (!symRunStop) stopCount++;

                await Task.Run(new Action(system.AdvanceSystem));

                plotOpenVsTime(time);

                systemUpdate(time);

                recordStep(record);

                run++;
                if (run > runUntil) { outputToFile(time, cbPDF.Checked); break; }
                time++;

                //Application.DoEvents();
            }



            trPlay.Maximum = time;
            symRunning = false;
        }


        private int getExpIndex(string path)
        // finds and returns the index of the experiment code from the file path.
        {
            int index = -1;
            string name = FileIO.getFileNameFromPath(path);
            int x = name.IndexOf("-") + 1;
            name = name.Slice(x, x + 2);
            foreach (int i in range(experimentCode.Length))
                if (experimentCode[i].Equals(name))
                    return i;
            return index;
        }

        private void updateStatusStrip(object sender, ExperimentEventArgs e)
        {
            tsStatusBar.Value = e.progress;
            tsInfoLabel.Text = e.status;
        }

        private void updateStatsStrip(int interval, int run, DateTime startTime)
        {
            float job = (float)(numNumIntervals.Value * numRuns.Value);
            int progress = (int)(((interval * (float)numRuns.Value + (run + 1f)) / job) * 100f);
            TimeSpan eTime = DateTime.Now.Subtract(startTime);
            if (progress > 100) progress = 100;
            tsStatusBar.Value = progress;
            tsInfoLabel.Text =
                " Progress: " + progress + "%" +
                " :: Step: " + (interval + 1) + "/" + (int)numNumIntervals.Value +
                " :: Current Run: " + (run + 1) + "/" + ((int)numRuns.Value).ToString("D") +
                " :: Elapsed time: " + eTime.ToString(@"hh\:mm\:ss");
        }


        private double powerLawRandomGen(double lowRange, double topRange, double exponent, Random uGen)
        // powerLawRandomGen - returns a PowerLaw randomly distributed variable.
        // @param double - lowRange of distribution.
        // @param double - topRange of distribution.
        // @param double - the power of the distribution.
        // @param Random - instance of the Random class.
        {
            //Random uGen = new Random();
            return Math.Pow(((Math.Pow(topRange, exponent + 1) - Math.Pow(lowRange, exponent + 1)) * uGen.NextDouble() + Math.Pow(lowRange, exponent + 1)), 1 / (exponent + 1));
        }


        // SIMULATION FUNCTIONS vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv


        private void plotVertices(bool newSys)
        // plotVertices - manin network plot operations
        {
            if (newSys) chart2.Series["S1"].Points.Clear();

            //chart1.Series["S3"].Points.Clear();
            //chart1.Series["S1"].Enabled = false;
            //chart1.Series["S3"].Enabled = true;

            // zero the time sequence structure.
            timeEvolutionSeq.Clear();
            timeEvolutionSeq.Add(new int[parameters.N]);

            for (int i = 0; i < parameters.N; i++)
            {

                //if (newSys) vertexCinfo.Add(new VertexData());

                timeEvolutionSeq[0][i] = 0;

                if (newSys)
                {
                    chart1.Series["S1"].Points.AddXY(system.systemState[i][0], system.systemState[i][1]);
                    chart1.Series["S1"].Points[i].MarkerBorderWidth = vertexCinfo[i].borderSize;
                }

                if (system.systemState[i][2] == 1) // if Active
                {
                    chart1.Series["S1"].Points[i].Color = vertexCinfo[i].onColor;
                    chart1.Series["S1"].Points[i].MarkerSize = vertexCinfo[i].onSize;
                }
                else
                {
                    chart1.Series["S1"].Points[i].Color = vertexCinfo[i].offColor;
                    chart1.Series["S1"].Points[i].MarkerSize = vertexCinfo[i].offSize;
                }

            }
        }


        private void getPowerLawMatrix(List<double[]> adjMatrix, int size, int maxConn, double exp)
        // getPowerLawMatrix - get power law adjacency matrix, conections have power law distribution.
        // @param List<double[]> - Adjacency matrix.
        // @param int - size of the system.
        // @param int - max number of connections.
        // @param double - exponent of powerlaw dist.
        {
            if (size >= maxConn)
            {
                adjMatrix.Clear();
                List<int> connects = new List<int>(size);

                for (int i = 1; i < size + 1; i++)
                {
                    adjMatrix.Add(new double[size]);
                    connects.Add((int)(maxConn / Math.Pow((i), exp)));
                    //connects.Add((int)Math.Pow((maxConn / i), (1/(exp - 1))));
                }

                //foreach (int e in connects) Console.WriteLine(e);
                //connects = connects.OrderBy(item => rdn.Next()).ToList(); //shuffle elements
                foreach (int e in connects) Console.Write(e.ToString() + " "); Console.Write("\n");

                for (int i = 0; i < size; i++)
                {
                    int[] used = new int[connects[i]];
                    double used_conn = 0;

                    for (int j = 0; j < i; j++)
                    {
                        if (i == j) continue;
                        if (j < i) used_conn += adjMatrix[i][j]; //count how many connections have already been asigned.
                    }
                    int j_max = (int)((i + 1) + (connects[i] - used_conn));
                    if (j_max > size)
                    {
                        Console.Write((j_max - size).ToString() + " - entries were omitted from " + i.ToString() + " row. \n");
                        j_max = size;
                    }
                    for (int j = i + 1; j < j_max; j++)
                    {
                        if (connects[i] - used_conn > 0)
                        {
                            //int index = rdn.Next(i+2, size+1); //offset since 0 is the defult null value and will always be detected as repeat
                            //while (i == index - 2 || used.Contains(index)) index = rdn.Next(i+2, size+1);

                            adjMatrix[i][j] = 1;
                            adjMatrix[j][i] = 1;
                            //used[j] = index;
                        }
                    }

                }
                //adjMatrix[i][i] = 0.0;

                //foreach (double[] r in adjMatrix) { foreach (double e in r) { Console.Write(e.ToString("F2") + " "); } Console.Write("\n"); }

            }
        }


        private void getNetProperties()
        // getNetProperties - populates the Net Properties list {Vertex #, #inbound, #outbound} and sorts it by the number of outbound connections
        {
            Parallel.For(0, parameters.N, i => _getNetProperties(i));
        }

        private void _getNetProperties(int i)
        {
            for (int j = 0; j < parameters.N; j++)
            {
                if (system.adjMat[i][j] != 0)
                {
                    vertexCinfo[j].inBound++;
                    vertexCinfo[i].outBound++;
                }
            }
        }

        private async void doEigen()
        {
            //adjMatEigen.calcEigen();
            //tbLambda.Text = adjMatEigen.Lambda.ToString("F4");
            /*rtbVectComp.Text = "";
            foreach (int i in iterate(adjMatEigen.rows))
                rtbVectComp.Text += adjMatEigen.EigenV[i][0].ToString("F4") + "\n";
                */
            await Task.Run(() => adjMatEigen.CalcEigenGeneral());

            rtbEigenVals.Text = "Real\tImg\n";
            for (int i = 0; i < adjMatEigen.LambdaR.Length; i++)
                rtbEigenVals.Text += $"{adjMatEigen.LambdaR[i]:F3}\t{adjMatEigen.LambdaI[i]:F3}i \n";   
        }


        private static System.Collections.IEnumerable iterate(int range)
        {
            for (int i = 0; i < range; i++) yield return i;
        }
        /*
                //private void getConnectionType()
                //{
                //    foreach (PointF eg in edges)
                //    {
                //        if (distMatrix[(int)eg.X][(int)eg.Y] == distMatrix[(int)eg.Y][(int)eg.X]) { vertexCinfo[(int)eg.X].}
                //        int p0 = EDG.Points.AddXY(cruMatrix[(int)eg.X][0], cruMatrix[(int)eg.X][1]);
                //        int p1 = EDG.Points.AddXY(cruMatrix[(int)eg.Y][0], cruMatrix[(int)eg.Y][1]);
                //    }
                //}
         */
        // GUI FUNCTIONALITY vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

        private void findExpFiles(bool load)
        // findExpFiles - loads a list of the exp files and populates the list in the ExploreResults tab.
        {
            lbDataFiles.Items.Clear(); comboOby.Items.Clear(); comboTby.Items.Clear();

            if (load) expFiles = FileIO.GetFiles(@"\Data\", "csv", false);
            archivedItems = FileIO.GetDirectories(@"\Data\");

            loadArchiveSubMenu();


            string parNames = FileIO.GetTagsVals(expFiles[0], '_', '#', 0);
            //parNames = parNames.Slice(0, 5) + parNames.Slice(9, parNames.Length);
            lbDataFiles.Items.Add(parNames);

            foreach (var token in FileIO.ExtractTokens(FileIO.getFileNameFromPath(expFiles[0]), '_', '#'))
            {
                comboOby.Items.Add(token[0]);
                comboTby.Items.Add(token[0]);
            }

            // fill the list box.
            foreach (string p in expFiles)
            {
                //System.IO.File.Move(p, p.Insert(p.Length - 4, "_Gamma#2.2_N#500")); // rename file
                string name = FileIO.GetTagsVals(p, '_', '#', 0);
                int i = name.IndexOf("-");
                string netType = name.Slice(0, i);
                string expName = name.Slice(i + 1, i + 3);
                //lbDataFiles.Items.Add(netType + ":" + expName + ": " + FileIO.GetTagsVals(p, '_', '#', 1));
                lbDataFiles.Items.Add(FileIO.GetTagsVals(p, '_', '#', 1));

            }

            setUpLabelsDialog();
        }

        private void loadArchiveSubMenu()
        // constructs sub menu with archived experiment files.
        {
            cmArchiveItems.DropDownItems.Clear();
            int i = 0;
            foreach (string item in archivedItems)
            {
                cmArchiveItems.DropDownItems.Add(FileIO.getFileNameFromPath(item), null, archivesEventHandler_onClick).Tag = (i++).ToString();
                cmArchiveItems.DropDownItems[i - 1].AutoToolTip = true;
                cmArchiveItems.DropDownItems[i - 1].ToolTipText = readWriteCfg(item + @"\token.cfg", null, true);

            }
        }

        private void setUpLabelsDialog()
        // Enables all appropriate label checboxes in ChartLabels.
        {
            var tokens = FileIO.ExtractTokens(FileIO.getFileNameFromPath(expFiles[0]), '_', '#');   // get the param names and values.
            int i = 0;
            foreach (string[] t in tokens)
                if (i++ != 0)
                {
                    ChartLabels.EnableControl("cb" + t[0], true, 1);
                    ChartLabels.EnableControl("cbt" + t[0], true, 2);
                }

        }

        private void archivesEventHandler_onClick(object sender, EventArgs e)
        {
            bool go = true;
            if (expFiles.Count > 0) // check if there are any files in the current working dir.
            {
                var result = MessageBox.Show("Do you want to Archive(YES) or Delete(NO) current data files?", "Archive or Delete?", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes) archiveFiles(true);
                else if (result == DialogResult.No) deleteFiles(true);
                else go = false;
            }
            if (go)
            {
                int selectedItem = Int32.Parse((string)((ToolStripDropDownItem)sender).Tag); // get the index of the archive folder we're about to unpack.
                string archName = FileIO.getFileNameFromPath(archivedItems[selectedItem]); // get arch folder name.

                var files = FileIO.GetFiles($"\\Data\\{archName}\\", "csv", true); // get all files in archive.

                string root = System.IO.Directory.GetCurrentDirectory() + @"\Data\";

                foreach (string f in files) // unpack archive.
                    System.IO.File.Move(f, root + FileIO.getFileNameFromPath(f));

                //// Save the Archive Name in case you wish to re-archive the data later.

                string archiveConfig = root + "archive.cfg";

                readWriteCfg(archiveConfig, archName, false);

                System.IO.Directory.Delete(archivedItems[selectedItem], true); // delete empty archive directory.

                MessageBox.Show($"Done Unpacking Files From: {archName}");

                lbFilesUpdate(); // update listbox
            }
        }

        private string readWriteCfg(string path, string data, bool read)
        // readWriteCfg - reads or writes to cfg file. cfg hidden after write.
        {
            if (read)
            {
                if (System.IO.File.Exists(path))
                {
                    FileIO.HideUnhideFile(path, false);
                    var file = new FileIO(path, false);
                    data = file.ReadLine()[0];
                    file.CloseFile();
                    FileIO.HideUnhideFile(path, true);
                }
            }
            else
            {
                if (System.IO.File.Exists(path))
                    FileIO.HideUnhideFile(path, false);

                var file = new FileIO(path, true);

                file.WriteLine(data);

                file.CloseFile();

                FileIO.HideUnhideFile(path, true); // hide config file.
            }
            return data;
        }

        private void deleteFiles(bool deleteAll)
        // deleteFiles - deletes data files true - to delete all, false - to delete only selected.
        {
            string archiveConfig = System.IO.Directory.GetCurrentDirectory() + @"\Data\archive.cfg";
            if (System.IO.File.Exists(archiveConfig))
                System.IO.File.Delete(archiveConfig);

            try
            {
                if (deleteAll)
                    foreach (string s in expFiles)
                        System.IO.File.Delete(s);
                else
                    foreach (int i in lbDataFiles.SelectedIndices)
                        System.IO.File.Delete(expFiles[i - 1]);

                lbDataFiles.Items.Clear();
                lbFilesUpdate();
            }
            catch
            {
                MessageBox.Show("Something Went Wrong!");
            }
        }

        private void archiveFiles(bool archiveAll)
        // archiveFiles - archives experimental files by moving them to a new directory and freeing the current Data dir.
        // also saves a preview token.
        {
            try
            {
                string root = System.IO.Directory.GetCurrentDirectory() + @"\Data\";
                string archiveConfig = root + "archive.cfg";
                string archiveName = "";

                // check if config file contains an archive name (from previous extractions).
                if (System.IO.File.Exists(archiveConfig))
                {
                    archiveName = readWriteCfg(archiveConfig, null, true);

                    if (MessageBox.Show($"Do you want to use previous archive name: {archiveName}",
                        "Use Original Archive Name?", MessageBoxButtons.YesNo) == DialogResult.No)
                        archiveName = "ARCH_" + DateTime.Now.ToString("MM-dd-yyyy#HH-mm-ss");

                    System.IO.File.Delete(archiveConfig);
                }
                else
                    archiveName = "ARCH_" + DateTime.Now.ToString("MM-dd-yyyy#HH-mm-ss");

                string archDir = root + archiveName + "\\";

                if (!System.IO.Directory.Exists(archDir))
                    System.IO.Directory.CreateDirectory(archDir);

                readWriteCfg(archDir + "token.cfg", FileIO.getFileNameFromPath(expFiles?[0]), false); // Save the name of the first file as a preview token.
                if (archiveAll)
                    foreach (string s in expFiles)
                        System.IO.File.Move(s, archDir + FileIO.getFileNameFromPath(s));
                else
                    foreach (int i in lbDataFiles.SelectedIndices)
                        System.IO.File.Move(expFiles[i - 1], archDir + FileIO.getFileNameFromPath(expFiles[i - 1]));


                lbDataFiles.Items.Clear();
                lbFilesUpdate();
            }
            catch
            {
                MessageBox.Show("Something Went Wrong!");
            }
        }

        // PLOTTING FUNCTIONS vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv


        private void chartSetup()
        // chartSetup: initializez the chart parameters
        // @double - x axis size
        // @double - y axis size
        {
            chart2.Series["S1"].Points.Clear();
            chart2.Series["S2"].Points.Clear();

            events = 0;

            if (cbNew.Checked)
            {
                chart1.Series["S1"].Points.Clear();
                chart1.Series["S2"].Points.Clear();
                chart1.Series["S3"].Points.Clear();
                chart1.Series["S2"].Enabled = true;

                if (cbCircle.Checked)
                {
                    chart1.ChartAreas[0].AxisX.Maximum = 1;
                    chart1.ChartAreas[0].AxisY.Maximum = 1;
                    chart1.ChartAreas[0].AxisX.Minimum = -1;
                    chart1.ChartAreas[0].AxisY.Minimum = -1;
                    chart1.Series["S2"].Points.AddXY(-1, -1); chart1.Series["S2"].Points.AddXY(-1, 1);
                    chart1.Series["S2"].Points.AddXY(1, -1); chart1.Series["S2"].Points.AddXY(1, 1);
                }
                else
                {
                    chart1.ChartAreas[0].AxisX.Maximum = parameters.bc.x;
                    chart1.ChartAreas[0].AxisY.Maximum = parameters.bc.y;
                    chart1.ChartAreas[0].AxisX.Minimum = 0;
                    chart1.ChartAreas[0].AxisY.Minimum = 0;
                    chart1.Series["S2"].Points.AddXY(0, 0); chart1.Series["S2"].Points.AddXY(0, parameters.bc.y);
                    chart1.Series["S2"].Points.AddXY(parameters.bc.x, 0); chart1.Series["S2"].Points.AddXY(parameters.bc.x, parameters.bc.y);
                }
            }
            chart1.Series["S2"].Enabled = false;

        }

        private void systemUpdate(int time_)
        // systemUpdate - updates the system plot (data point colors)
        {
            int maxI = (int)numVertecies.Value;
            //double x, y;
            //int seqLen = (int)(Convert.ToDouble(tbTime.Text)/(double)numDtStepSize.Value);
            for (int i = 0; i < maxI; i++)
            {

                //x = cruMatrix[i][0];    //chart1.Series["S1"].Points[i].XValue;
                //y = cruMatrix[i][1];    //chart1.Series["S1"].Points[i].YValues[0];
                if (timeEvolutionSeq[time_][i] == 1)
                {
                    chart1.Series["S1"].Points[i].Color = vertexCinfo[i].onColor;
                    chart1.Series["S1"].Points[i].MarkerSize = vertexCinfo[i].onSize;
                }
                else
                {
                    chart1.Series["S1"].Points[i].Color = vertexCinfo[i].offColor;
                    chart1.Series["S1"].Points[i].MarkerSize = vertexCinfo[i].offSize;
                }

            }
            if (cbNetConn.Checked) { drawConnections(time_); }
            chart1.Update();

        }

        private void markHighOutbound(uint order)
        // markHighOutbound - tag the top 10 vertecies in the network with a different color
        // @param uint - top n vertices with highest outbound conections
        {
            Series mainPlot = chart1.Series["S1"];
            Func<VertexData, Color, Color, ushort, ushort, bool> colorNsize = (point, cON, cOFF, sON, sOFF) =>
           {
               try
               {
                   point.offColor = cOFF; point.onColor = cON; point.offSize = sOFF; point.onSize = sON; mainPlot.Points[point.vertexNum].MarkerSize = sOFF;
                   mainPlot.Points[point.vertexNum].Color = cOFF; return true;
               }
               catch { return false; }
           };

            List<VertexData> temp = vertexCinfo.OrderByDescending(arr => arr.outBound).ToList();

            if (cbCircle.Checked) reOrderPosition(temp, order, (double)((double)numX.Value / (0.2 * (double)order)), (double)numX.Value);

            uint numVert = (uint)numVertecies.Value;
            if (numVert > order - 1) numVert = order;

            for (int i = 0; i < numVert; i++)
            {
                if (i == 0) { colorNsize(temp[i], Color.DarkRed, Color.DarkBlue, 15, 9); }
                else colorNsize(temp[i], Color.Green, Color.Blue, 10, 8);

                VertexData.addRank(temp[i].vertexNum); //populate the rank property of the VertexData class.

            } //foreach (var r in VertexData.rank) { Console.WriteLine(r); }

        }

        private void reOrderPosition(List<VertexData> orderList, uint whatOrder, double r1, double r2)
        // reOrderPosition - reorders the position of the vericies, in two concentric circles, 
        //                   it puts the vertex with the highest number of connections in the middle
        //                   folowed with the order-1 higghest connected vertices in the inner circle
        //                   the remeining vertecies remein on the outer rim.
        // @param List<VertexData> - ordered list containing the indecies of the verticies in decending order.
        // @param uint - the lowest rank we want to have in the core.
        // @param double - radius of the core.
        // @param double - radius of outer rim.
        {
            double radStep1 = 2 * Math.PI / (double)(whatOrder - 1), radStep2 = 2 * Math.PI / (double)(numVertecies.Value - whatOrder);
            Series mainPlot = chart1.Series["S1"];
            try
            {
                List<int> usednums = new List<int>();
                for (int i = 0; i < whatOrder; i++)
                {
                    if (i == 0) { system.systemState[orderList[i].vertexNum][0] = 0; system.systemState[orderList[i].vertexNum][1] = 0; usednums.Add(orderList[i].vertexNum); }
                    else
                    {
                        system.systemState[orderList[i].vertexNum][0] = r1 * Math.Cos((i - 1) * radStep1);
                        system.systemState[orderList[i].vertexNum][1] = r1 * Math.Sin((i - 1) * radStep1);
                        usednums.Add(orderList[i].vertexNum);
                    }
                }
                int offset = 0;
                for (int i = 0; i < numVertecies.Value; i++)
                {
                    bool used = false;

                    for (int j = 0; j < whatOrder; j++) { if (usednums[j] == i) used = true; }
                    if (used) { offset++; continue; }
                    else
                    {
                        system.systemState[i][0] = r2 * Math.Cos((i - offset) * radStep2);
                        system.systemState[i][1] = r2 * Math.Sin((i - offset) * radStep2);
                    }
                }

                for (int i = 0; i < numVertecies.Value; i++)
                {
                    mainPlot.Points[i].XValue = system.systemState[i][0];
                    mainPlot.Points[i].YValues[0] = system.systemState[i][1];
                }

            }
            catch { MessageBox.Show("There was a problem reordering the points"); }

        }

        private int events = 0; //keeps track of major verticies being open.
        private void plotOpenVsTime(int currentTime)
        // plotOpenVsTime - updates the (# of active cites vs. time) plot.
        // @param currentTime - the current simulation time.
        {

            int channelsOpen = 0;
            int num = (int)numVertecies.Value;
            int rankOrder = VertexData.rank.Count();
            bool masterNode = false;
            bool highImpactNode = false;

            Parallel.For(0, num, i => _openVsTimeCounter(ref channelsOpen, rankOrder, ref masterNode, ref highImpactNode, i));

            //int currentTime = chart2.Series["S1"].Points.Count();

            chart2.Series["S1"].Points.AddXY(currentTime + 1, channelsOpen);

            // major event marks
            if (highImpactNode || masterNode) { chart2.Series["S2"].Points.AddXY(currentTime + 1, channelsOpen); events++; }
            if (masterNode && highImpactNode) chart2.Series["S2"].Points[events - 1].Color = Color.Purple;
            else if (masterNode) chart2.Series["S2"].Points[events - 1].Color = Color.Red;
            else if (highImpactNode) chart2.Series["S2"].Points[events - 1].Color = Color.Green;

            tbTime.Text = (currentTime * (double)numDtStepSize.Value).ToString();

            tbTime.Update();

            if (channelsOpen > num * numStopCond.Value / 100)
            {
                if (!cbTimePlot.Checked) cbTimePlot.Checked = true;
                if (symRunStop)
                {
                    //chart2.SaveImage(@"C:\Users\Myers\Documents\Shif-Projects\Plots\R" + numR.Value.ToString() + 
                    //    "_VERT" + numCrus.Value.ToString() +
                    //    "_CONN" + numNet.Value.ToString() +
                    //    "_TIME" + time.ToString() + ".emf", ChartImageFormat.Emf);
                    outputToFile(currentTime, cbPDF.Checked);
                    //chart2.Scale(-4);
                }

                symRunStop = false; //stop simulation if number of open channels reaches a certain percantage
            }

        }

        private void _openVsTimeCounter(ref int channelsOpen, int rankOrder, ref bool masterNode, ref bool highImpactNode, int i)
        {
            if (system.systemState[i][2] == 1)
            {
                for (int j = 0; j < rankOrder; j++)
                {
                    if (VertexData.rank[j] == i && j == 0) masterNode = true; //Console.WriteLine(currentTime);}

                    if (VertexData.rank[j] == i && j != 0) highImpactNode = true;// Console.WriteLine(currentTime);}

                }
                channelsOpen++;
            }
        }

        private void outputToFile(int myTime, bool outPDF)
        // outputToFile - outputs a pdf of the countVsTime plot
        // @param int - the current simulation time
        {
            List<string[]> paths = new List<string[]>();
            //paths.Add(new string[] {"C:\\Users\\Myers\\Documents\\Shif-Projects\\Plots\\temp.txt", "S1"});
            //paths.Add(new string[] {"C:\\Users\\Myers\\Documents\\Shif-Projects\\Plots\\temp1.txt", "S2"});

            string path = System.IO.Directory.GetCurrentDirectory() + @"\Plots\";

            if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);


            paths.Add(new string[] { path + "temp.txt", "S1" });
            paths.Add(new string[] { path + "temp1.txt", "S2" });

            System.IO.FileInfo fileInfo = null;
            foreach (string[] p in paths)
            {
                try
                {
                    fileInfo = new System.IO.FileInfo(p[0]);
                    fileInfo.Attributes &= ~System.IO.FileAttributes.Hidden;
                    fileInfo.Attributes &= ~System.IO.FileAttributes.ReadOnly;
                }
                catch { }


                using (var file = new System.IO.StreamWriter(p[0]))
                {
                    fileInfo = new System.IO.FileInfo(p[0]);
                    fileInfo.Attributes &= ~System.IO.FileAttributes.ReadOnly;

                    file.WriteLine(@"ALPHA" + numAlpha.Value.ToString() +
                            "_VERT" + numVertecies.Value.ToString() +
                            "_CONN" + numNet.Value.ToString() +
                            "_TIME" + myTime.ToString());
                    try
                    {
                        file.WriteLine(path + (myControl.Name).Substring(3) + "(" + numStartParam.Value.ToString().Substring(0, 6) +
                                "~" + numEndParam.Value.ToString().Substring(0, 6) +
                                ")__VERT" + numVertecies.Value.ToString() +
                                "_CONN" + numNet.Value.ToString() + @"\");
                    }
                    catch { file.WriteLine("error"); }
                    foreach (var pt in chart2.Series[p[1]].Points)
                    {
                        file.WriteLine(pt.XValue.ToString() + "\t" + pt.YValues[0].ToString() + "\t" + pt.Color.Name);
                    }
                }
                try { fileInfo.Attributes |= System.IO.FileAttributes.Hidden; } catch { }
            }
            if (outPDF)
            {
                System.IO.File.Copy(System.IO.Directory.GetCurrentDirectory() + "\\Resources\\temp.py", path + "temp.py", true);
                runScript(path, "temp.py");
            }

        }

        private void runScript(string location, string script)
        // runScript - executes a python script
        // @param string - full path to script file: drive:\path\to\folder\
        // @param string - script file: myscript.py
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            //p.StartInfo.Arguments = @"/c D:\\pdf2xml";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            p.StandardInput.WriteLine("cd " + location);
            p.StandardInput.WriteLine(script);
            //p.WaitForExit();
        }

        private Color setColor(double value, double maxInRange)
        {

            return Color.FromArgb((int)((value * 255) / maxInRange), 0, (int)((value * 255) / maxInRange));

        }

        private void setUpDistMatrixPlot()
        {
            int cruCount = (int)numVertecies.Value;
            int pointNum = 0;
            double space = 0;
            space = (1.0 - (2.0 / cruCount)) / cruCount;
            double[] myArr = new double[cruCount];

            Parallel.For(0, cruCount, i => _distMatGetRowMaxes(myArr, i));

            double maxInRange = myArr.Max();
            for (int i = 0; i < cruCount; i++)
            {
                for (int j = 0; j < cruCount; j++) //this populates the distance matrix
                {
                    pointNum = (cruCount * i + j);

                    //chart1.Series["S3"].Points.AddXY(((j + 1) * space), ((i + 1) * space));
                    chart1.Series["S3"].Points.AddXY(space * j + space, 1 - (space * i + space));
                    chart1.Series["S3"].Points[pointNum].MarkerBorderWidth = 0;
                    chart1.Series["S3"].Points[pointNum].MarkerSize = 8;
                    chart1.Series["S3"].Points[pointNum].MarkerStyle = MarkerStyle.Square;
                    chart1.Series["S3"].Points[pointNum].Color = setColor(system.adjMat[i][j], maxInRange);
                    //chart1.Update();

                }
                //chart1.Update();
            }
        }

        private void _distMatGetRowMaxes(double[] myArr, int i)
        {
            myArr[i] = system.adjMat[i].Max();
        }

        private void playBack(int playTime, int runUntil)
        // playBack - plays back a sequence of the simulation frames
        // @param int - playback begin time
        // @param int - playback stop time
        {
            var frameTime = new DateTime(); var frameTime_ = new DateTime();

            bool fwd = true;
            if (runUntil < playTime) fwd = false;

            playTime = trPlay.Value;

            playGoStop = true;

            lbPlayTime.Text = (playTime * numDtStepSize.Value).ToString();

            while (true) //trPlay.Maximum + 1)
            {
                frameTime = DateTime.UtcNow;
                if ((frameTime - frameTime_).TotalMilliseconds > (double)(1000 / numFps.Value))
                {
                    systemUpdate(playTime);
                    trPlay.Value = playTime;
                    trPlay.Update();

                    lbPlayTime.Update();


                    frameTime_ = frameTime;
                    if (fwd)
                    {
                        playTime++;
                        if (playTime > runUntil) break;
                    }
                    else
                    {
                        playTime--;
                        if (playTime < runUntil) break;
                    }
                }
                Application.DoEvents();
                if (!playGoStop) break;
            }
        }

        private void chart1_MouseDown(object sender, MouseEventArgs e)
        {

            if (cbInfect.Checked)
            {
                ChartArea chratAreas = chart1.ChartAreas[0];
                var xVal = chratAreas.AxisX.PixelPositionToValue(e.X);
                var yVal = chratAreas.AxisX.PixelPositionToValue(e.Y);
                yVal = 1 - yVal;

                if (cbMark.Checked)
                {
                    chart1.Series["S3"].Enabled = true;
                    //chart1.Series["S3"].Points.Clear();
                    chart1.Series["S3"].Points.AddXY(xVal, yVal);
                    chart1.Series["S3"].Points[chart3ptCount].MarkerSize = 10;
                    chart1.Series["S3"].Points[chart3ptCount].Color = Color.Blue;
                    chart3ptCount++;
                }

                FindPts(xVal, yVal);
            }
        }

        private void FindPts(double inX, double inY) //, int len, double rad)
                                                     // FindPts - finds the index of all pts in a given radius and activates them
                                                     // @param double - X coordinate of cursor
                                                     // @param double - Y coordinate of cursor
        {
            int len = (int)numVertecies.Value;
            double rad = (double)numInfect.Value;
            List<int> outList = new List<int>();

            for (int i = 0; i < len; i++)
            {
                if (system.systemState[i][0] > (inX - rad) && system.systemState[i][0] < (inX + rad) && system.systemState[i][1] > (inY - rad) && system.systemState[i][1] < (inY + rad))
                {
                    //outList.Add(i);
                    system.systemState[i][2] = 1;
                    timeEvolutionSeq[time][i] = 1;
                }
            }
            systemUpdate(time);
            //return outList;
        }

        private List<PointF> edges = new List<PointF>();


        private void setUpNetSeries()
        // setUpNetSeries - sets up the series to represent the connections between vertices
        {
            Series EDG = chart1.Series["edges"];
            EDG.ChartType = SeriesChartType.Line;
            EDG.Color = Color.Green;
            int vertexCount = parameters.N;
            EDG.Points.Clear();
            edges.Clear();

            for (int i = 0; i < parameters.N; i++)
            {
                //int connectionCount = 0;
                for (int q = 0; q < parameters.N; q++)
                {
                    if (system.adjMat[i][q] != 0) edges.Add(new Point(i, q));
                }
            }

        }

        private void drawConnections(int time_)
        {
            Series EDG = chart1.Series["edges"];
            EDG.Points.Clear();
            foreach (PointF eg in edges)
            {
                int p0 = EDG.Points.AddXY(system.systemState[(int)eg.X][0], system.systemState[(int)eg.X][1]);
                int p1 = EDG.Points.AddXY(system.systemState[(int)eg.Y][0], system.systemState[(int)eg.Y][1]);

                EDG.Points[p0].Color = Color.Transparent;
                if (timeEvolutionSeq[time_][(int)eg.Y] == 1 && system.adjMat[(int)eg.X][(int)eg.Y] != 0) EDG.Points[p1].Color = Color.DarkRed;
                else EDG.Points[p1].Color = Color.LightGray;


            }
        }

        private void plotAdjMatrix()
        {
            int size = (int)numVertecies.Value;
            var pSeries = chart3.Series["S1"];
            pSeries.Points.Clear();
            int numConnections = 0;
            double avgConnections = 0;
            int min = Int32.MaxValue, max = 0;

            for (int i = 0; i < size; i++)
            {
                numConnections = countConnections(rbRow.Checked, i, size);
                pSeries.Points.Add(numConnections);
                avgConnections += numConnections;
                if (numConnections < min) min = numConnections;
                if (numConnections > max) max = numConnections;
            }
            avgConnections /= size;
            chart3.Titles[0].Text = $"AVG Connectivity: {avgConnections: 0.0} :: Min: {min} :: Max: {max}";
        }

        private int countConnections(bool row, int index, int size)
        {
            int count = 0;
            if (parameters?.N > 0)
                for (int j = 0; j < size; j++)
                {
                    if (row)
                    {
                        if (system?.adjMat[index][j] != 0)
                            count++;
                    }
                    else
                    {
                        if (system?.adjMat[j][index] != 0)
                            count++;
                    }
                }
            return count;
        }

        private void saveEigenToFile()
        {
            string path = FileIO.setupPathForFileWrite("Data", "eigen_vector.vect");
            FileIO.saveMatrixToFile(path, new Matrix(adjMatEigen.EigenV));
        }

        private void saveAdjMatToFile()
        // saveAdjMatToFile - saves the adjMatrix to a .mat file and tags it with today's date, time and size.
        {
            string matName = "MATRIX-" + netTypes[netType] + "_N#" + numVertecies.Value.ToString("F0") + "_Date#" + DateTime.Now.ToString("MM-dd-yyyy#HH-mm-ss") + ".mat";
            string path = FileIO.setupPathForFileWrite("Data", matName);
            FileIO.saveMatrixToFile(path, adjMatEigen);
        }


        private static System.Collections.IEnumerable range(int range)
        // Iterator function to simplify forloops.
        {
            for (int i = 0; i < range; i++) yield return i;
        }


        public void uncheckLoadMatrix()
        {
            cbLoadMatrix.Checked = false;
        }
        public void cbNewState(bool state)
        {
            cbNew.Checked = state;
        }

        private void lbDataFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Console.WriteLine(lbDataFiles.SelectedItem.ToString());
        }

        private List<int> getSelectedItems()
        // getSelectedItems - returns a list of indecies of the selected items in the list.
        {
            List<int> indecies = new List<int>();
            foreach (int i in lbDataFiles.SelectedIndices)
            { if (i == 0) { continue; } indecies.Add(i - 1); }
            return indecies;
        }

        private void exp3ExpNumbers()
        // exp3ExpNumbers - visualizes the results from Experiment 3.
        {
            List<double> r_axis = new List<double>();
            var steadyState = new List<double>();
            string title = "";
            foreach (int i in getSelectedItems())
            {
                steadyState.Add(processExpFile(expFiles[i]));
                r_axis.Add(getParValue(expFiles[i], "A"));
                title = expFiles[i];
            }

            title = "K:" + +getParValue(title, "K") + "  A:" + r_axis[0] + "-" + r_axis[r_axis.Count - 1] + "  Gamma:" + getParValue(title, "Ga");

            if (cbNewPlot.Checked)
                pa = new PlotAnalytics(r_axis, steadyState, null, title);
            else
                pa.AddSeries(r_axis, steadyState, null);
        }

        private double getParValue(string path, string parName)
        // getParValue - Returns the specified parameters value from the file path provided.
        {
            var tokens = FileIO.ExtractTokens(FileIO.getFileNameFromPath(path), '_', '#');
            int i = tokens.FindIndex(t => (t[0].Equals(parName))); // get the param names and values.
            return Double.Parse(tokens[i][1]);
        }

        private double processExpFile(string path)
        // processExpFile - reads a column vector data and returns the average of the components.
        {
            FileIO data = new FileIO(path, false);
            bool eof;
            double result = 0;
            int i = 0;
            while (true)
            {
                double temp = data.ReadLineDouble(out eof);
                if (eof) break;
                result += temp;
                i++;
            }
            data.CloseFile();
            return result / i;
        }

        private void orderListBox(int orderBy, int thenBy)
        // orderListBox - based on the spacified token indecies.
        {
            var orderVector = new List<double[]>();
            foreach (int i in range(expFiles.Count))
            {
                var tokens = FileIO.ExtractTokens(FileIO.getFileNameFromPath(expFiles[i]), '_', '#');   // get the param names and values.
                orderVector.Add(new double[] { i, Double.Parse(tokens[orderBy][1]), Double.Parse(tokens[thenBy][1]) });
            }

            var sorted = orderVector.OrderBy(x => x[1]).ThenBy(x => x[2]).ToList();

            expFiles = reorderPositionFromVector<string>(expFiles, sorted);

            findExpFiles(false);
        }

        private List<List<int>> binExpData(List<int> selected, out List<string> labels)
        {
            string s = "";
            return binExpData(selected, out labels, ref s, false);
        }

        private List<List<int>> binExpData(List<int> selected, out List<string> labels, ref string title, bool customLabel)
        {
            Console.WriteLine("");

            //foreach (Series s in chart1.Series) s.Enabled = false;
            List<List<int>> data = new List<List<int>>();
            //string[] netTypes = { "Diff", "ER", "BA" };
            //string[] parameterNames = { "N", "R", "S", "G", "B", "T" };
            labels = new List<string>();

            int scaleDown = 1;

            //if (experimentIndex == 3) scaleDown = 2;

            var orderVector = new List<double[]>();
            //int j = 0;

            List<string[]> tokens = new List<string[]>();

            foreach (int i in selected)
            {
                //Console.Write(i + " > ");
                tokens = FileIO.ExtractTokens(FileIO.getFileNameFromPath(expFiles[i]), '_', '#');   // get the param names and values.

                int vertNum = (int)Double.Parse(tokens[1][1]);       // get the numver of vertices in system.
                data.Add(processExpFile(expFiles[i], vertNum / scaleDown));             // bin data.
                if (!customLabel) labels.Add("A:" + tokens[4][1] + " T:" + tokens[7][1]);
                else
                {
                    if (ChartLabels.labels == null)
                        ChartLabels.ShowForm(System.Windows.Forms.Cursor.Position);
                    else
                    {
                        string label = "";
                        foreach (int t in ChartLabels.labels)
                            label += tokens[t][0] + ":" + tokens[t][1] + " ";

                        labels.Add(label);
                    }
                }
                //j++;
            }
            if (ChartLabels.labels != null)
            {
                foreach (int par in ChartLabels.title)
                {
                    if (ChartLabels.labels.Contains(par)) continue;
                    if (par == 0) title += tokens[par][0].Slice(0, 2) + "---"; // get the net name ER, BA, etc.. 
                    else title += tokens[par][0] + ":" + tokens[par][1] + " ";
                }
                if (ChartLabels.title.Contains(ChartLabels.labels[0]))
                    title += labels[0].TrimEnd(' ') + "-" + labels[labels.Count - 1].Substring(2).TrimEnd(' ');
            }

            return data;
        }

        private List<T> reorderPositionFromVector<T>(List<T> list, List<double[]> order)
        // reorderPositionFromVector<T>() – this method orders T list based on provided order list.
        {
            List<T> newList = new List<T>();
            foreach (double[] d in order)
            {
                newList.Add(list[(int)d[0]]);
            }
            return newList;
        }

        private List<int> processExpFile(string path, int vertNum)
        // processExpFile - reads the file from the specified path and bins the data before returning it.
        {
            FileIO data = new FileIO(path, false);
            Dictionary<int, int> bins = null;
            List<double> allLines = new List<double>();

            while (true)
            {
                var temp = data.ReadLineListDouble();
                if (temp == null) break;
                allLines.AddRange(temp);
            }
            //data.CloseFile();
            bins = DataTools.binDataOne(allLines, vertNum);
            //foreach (int i in bins.Values.ToList()) Console.WriteLine(" :: " + i + " :: ");
            return bins.Values.ToList();
        }


        private void btnExpDens_Click(object sender, EventArgs e)
        {
            try { experimentIndex = getExpIndex(expFiles[lbDataFiles.SelectedIndex]); } catch { }
            List<string> labels;
            //PlotResults pr = new PlotResults(binExpData(getSelectedItems(), out labels), labels);

            string title = "";

            GraphixPlotVertical gp = new GraphixPlotVertical(binExpData(getSelectedItems(), out labels, ref title, true), labels, title);
        }

        private PlotResults Hist;
        private void btnExpBins_Click(object sender, EventArgs e)
        // event handdler for the display Histogram button. Opens a new histogram window.
        {
            plotHistOne();
        }

        private void plotHistOne()
        // plotHistOne - plots a histogram of the selected data file.
        {
            List<string> labels;
            var data = binExpData(getSelectedItems(), out labels);
            if (data.Count == 1)
            {
                if (cbAutoClose.Checked && Hist != null) Hist.Close();

                List<int> data_ = data[0];
                if (cbSort.Checked)
                {
                    data_ = new List<int>(data[0]);
                    data_.Sort((a, b) => b.CompareTo(a));
                }

                Hist = new PlotResults(labels[0], data_);
                Hist.DesktopLocation = new Point((this.DesktopLocation.X + this.Width / 2), (this.Height - Hist.Height) / 2);
                Hist.Show();
            }
            else MessageBox.Show("You Can Only Display One At a Time!");
        }

        PlotAnalytics pa;
        private void btnExplore_Click(object sender, EventArgs e)
        {
            experimentIndex = getExpIndex(expFiles[lbDataFiles.SelectedIndex]);
            switch (experimentIndex)
            {
                case 1:
                    exp2ExpNumbers();
                    break;
                case 2:
                    exp3ExpNumbers();
                    break;
                case 3:
                    //exp3bExpNumbers();
                    exp5ExpNumbers();
                    break;
                case 4:             //Plot Time Traces of exp 5 results.
                    exp5ExpNumbers();
                    break;
                case 5:             //Plot Leading eigen vector against the Node's firing probability.
                    exp6ExpNumbers();
                    break;
            }
        }

        private async void exp6ExpNumbers()
        {
            cbNewPlot.Checked = true;
            double max;
            bool done = exp5ExpNumbers(out max, GetPlotFilePath()[0]); // plot data

            if (done) // plot Eigen Vector
            {
                List<double> xdata, ydata;
                GetPlotValuesFromDataFile(FileIO.GetFiles("/DATA/", "vect", false)[0], out xdata, out ydata);

                await Task.Run(() =>
                {
                    max = ydata.Max();
                    double scaleFactor = 1 / max; // scale with respect to the data.
                    for (int i = 0; i < ydata.Count; i++)
                        ydata[i] *= scaleFactor;
                });
                pa.C1.ChartAreas[0].AxisY.Maximum = 1;
                pa.AddSeries(xdata, ydata, "EigenVector");
            }
        }

        private bool exp5ExpNumbers()
        {
            List<string> paths = GetPlotFilePath();
            if (paths == null)
                return false;

            if (paths.Count > 1)
            {
                SteadyStateFinalValueExpNumbers(paths);
                return true;
            }
                
            double y;
            return exp5ExpNumbers(out y, paths[0]);
        }

        private void SteadyStateFinalValueExpNumbers(List<string> paths)
        {
            List<double> xdata = new List<double>(), ydata = new List<double>();
            double mapToS = 1;
            if (cbAvgTail.Checked)
            {
                var tokens = FileIO.ExtractTokens(FileIO.getFileNameFromPath(paths[0]), '_', '#');
                double N = Double.Parse(tokens[1][1]);
                double K = Double.Parse(tokens[2][1]);
                mapToS = N * K / 100d;
            }

            foreach (string p in paths)
            {
                List<double> ydata_; // temp vals.
                GetPlotValuesFromDataFile(p, out ydata_);
                ydata.Add(ydata_[ydata_.Count - 1]); // get the last value.
                var tokens = FileIO.ExtractTokens(FileIO.getFileNameFromPath(p), '_', '#');   // get the param names and values.
                xdata.Add(Double.Parse(tokens[ChartLabels.labels[0]][1])*mapToS); // get value of var
            }
            double y;
            exp5ExpNumbers(out y, paths[0], xdata, ydata);

            pa.C1.Legends[0].Enabled = false; // hide the legend
            pa.C1.Series[0].ChartType = SeriesChartType.Point;
            pa.C1.Series[0].MarkerStyle = MarkerStyle.Circle;
        }
        private bool exp5ExpNumbers(out double maxY, string path, List<double> xdata, List<double> ydata)
        {
            maxY = 0;

            if (path == null) return false;
            if(xdata == null && ydata == null)
                GetPlotValuesFromDataFile(path, out xdata, out ydata);

            maxY = ydata.Max();

            var tokens = FileIO.ExtractTokens(FileIO.getFileNameFromPath(path), '_', '#');   // get the param names and values.
            string seriesName = tokens[ChartLabels.labels[0]][0] + ":" + tokens[ChartLabels.labels[0]][1];

            if (cbNewPlot.Checked)
            {
                string title = "";
                if (ChartLabels.labels != null)
                {
                    foreach (int par in ChartLabels.title)
                    {
                        if (ChartLabels.labels.Contains(par)) continue;
                        if (par == 0) title += tokens[par][0].Slice(0, 2) + "---"; // get the net name ER, BA, etc.. 
                        else title += tokens[par][0] + ":" + tokens[par][1] + " ";
                    }
                }

                pa = new PlotAnalytics(xdata, ydata, seriesName, title);
                return true;
            }
            else
            {
                pa.AddSeries(xdata, ydata, seriesName);
                return true;
            }
        }
        private bool exp5ExpNumbers(out double maxY, string path) // returns y-max
        {
            return exp5ExpNumbers(out maxY, path, null, null);
        }

        private static void GetPlotValuesFromDataFile(string path, out List<double> ydata)
        // y-values read from file
        {
            ydata = new List<double>();
            using (var file = new FileIO(path, false))
            {
                double i = 0;
                bool eof = false;
                while (true)
                {
                    double y = file.ReadLineDouble(out eof);
                    if (eof) break;
                    ydata.Add(y);
                }
            }
        }
        private static void GetPlotValuesFromDataFile(string path, out List<double> xdata, out List<double> ydata)
            // y-values read from file, x-values 0-N
        {
            xdata = new List<double>();
            ydata = new List<double>();
            using (var file = new FileIO(path, false))
            {
                double i = 0;
                bool eof = false;
                while (true)
                {
                    double y = file.ReadLineDouble(out eof);
                    if (eof) break;
                    ydata.Add(y);
                    xdata.Add(i++);
                }
            }
        }

        private List<string> GetPlotFilePath()
            // return the selected file path. Performs a check for label sattings.
        {
            var filePaths = new List<string>();
            if (ChartLabels.labels == null)
            {
                ChartLabels.ShowForm(System.Windows.Forms.Cursor.Position);
                return null;
            }
            try
            {
                foreach(int p in getSelectedItems())
                    filePaths.Add(expFiles[p]);
                return filePaths;
            } catch { return null; }
        }



        private void btnEigenViz_Click(object sender, EventArgs e)
        {
            var pa = new PlotAnalytics(adjMatEigen.LambdaR.ToList(), adjMatEigen.LambdaI.ToList(), null, "Eigen Values Plot");
        }

        private void numEigVector_ValueChanged(object sender, EventArgs e)
        {
            int vectNum = (int)numEigVector.Value;
            if(vectNum > parameters.N - 1)
            {
                numEigVector.Value = 0;
                vectNum = 0;
            }
            rtbEigVector.Text = $"{vectNum}th Vector\n";
            tbLambda.Text = $"{adjMatEigen.LambdaR[vectNum]:F3}\t{adjMatEigen.LambdaI[vectNum]:F3}i \n";
            for (int i = 0; i < adjMatEigen.LambdaR.Length; i++)
                rtbEigVector.Text += $" {adjMatEigen.EigenVectors[i, vectNum]:F3}\n";
        }

        private void exp3bExpNumbers()
        {
            plotHistOne();
        }



        private void exp2ExpNumbers()
        // shows a plot of the integral of the head vs the integral of N-head.
        {
            List<string> labels;
            string title = "";
            var data = binExpData(getSelectedItems(), out labels, ref title, true);
            var ratios = new List<double>();
            foreach (var set in data)
            {
                set.Sort((a, b) => b.CompareTo(a));
                double head = integrateInt(set, 0, (int)numHead.Value);
                double tail = integrateInt(set, (int)numHead.Value, set.Count);
                if (cbAvgTail.Checked) tail /= (set.Count - (int)numHead.Value) / (int)numHead.Value;
                ratios.Add(head / tail);
            }
            List<double> xdata = new List<double>();

            foreach (string s in labels)
            {
                int a = s.IndexOf("Ga") + 3; // Find The Gamma param in the string.
                xdata.Add(Double.Parse(s.Slice(a, a + 5)));
            }
            if (cbNewPlot.Checked)
                pa = new PlotAnalytics(xdata, ratios, null, labels[0]);
            else
                pa.AddSeries(xdata, ratios, null);
        }

        private int integrateInt(List<int> fx, int a, int b)
        {
            int result = 0;
            foreach (int i in range(b - a))
                result += fx[a + i];
            return result;
        }

        

        private async void btnRunExp3_Click(object sender, EventArgs e)
        {
            if (cbLS.Checked)
            {
                numStartParam.Value /= (decimal)(system.parameters.N * system.parameters.K / 100d);
                numEndParam.Value /= (decimal)(system.parameters.N * system.parameters.K / 100d);
                cbLS.Checked = false;
            }
            if (myControl == null) MessageBox.Show("You Must Select A Parameter!");
            else
            {
                int expNum = comboExpType.SelectedIndex;

                myControl.Value = numStartParam.Value;
                numStopCond.Value = numStopExpCond.Value;
                numDtSteps.Value = numMaxIter.Value;
                experimentIndex = expNum; // sets the experiment code for file naming.
                //MessageBox.Show("exp Num: " + expNum + " Name: " + comboExpType.SelectedItem);

                tsStatusBar.Enabled = true; tsStatusBar.Visible = true;
                tsInfoLabel.Enabled = true; tsInfoLabel.Visible = true;

                Experiment.UpdateStatusStrip += updateStatusStrip;
                Experiment.ExperimentComplete += ExperimentComplete_Handler;
                system.parameters = packSystemParameters();
                Experiment.PerformExperiment(system,
                                            (double)numStartParam.Value,
                                            (double)numIncrement.Value,
                                            (string)myControl.Tag,
                                            (int)numRuns.Value,
                                            (int)numNumIntervals.Value,
                                            (double)numIC.Value,
                                            (double)numStopExpCond.Value,
                                            (int)numMaxIter.Value,
                                            (int)numThreads.Value,
                                            expNum);

                if(expNum == 5) // if Exp5 - ActiveNode pDensity, calculate and record leading EigenVector.
                {
                    await Task.Run(() => adjMatEigen.calcEigen());
                    saveEigenToFile();
                }

            }
        }

        private void ExperimentComplete_Handler(object sender, ExperimentCompleteEventArgs e)
        {
            Experiment.UpdateStatusStrip -= updateStatusStrip;
            Experiment.ExperimentComplete -= ExperimentComplete_Handler;
        }

        private void setNetType()
        // determine net type from radio buttons on GUI
        {
            if (rbNet1.Checked) netType = 0;
            if (rbNet2.Checked) netType = 1;
            if (rbNet3.Checked) netType = 2;
            if (rbChain.Checked) netType = 3;
        }

        



    }


       

    



}


// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

//private delegate void SetPropertyThreadSafeDelegate<TResult>(Control @this, Expression<Func<TResult>> property, TResult value);

//public static void SetPropertyThreadSafe<TResult>(
//    this Control @this,
//    Expression<Func<TResult>> property,
//    TResult value)
//{
//    var propertyInfo = (property.Body as MemberExpression).Member
//        as PropertyInfo;

//    if (propertyInfo == null ||
//        !@this.GetType().IsSubclassOf(propertyInfo.ReflectedType) ||
//        @this.GetType().GetProperty(
//            propertyInfo.Name,
//            propertyInfo.PropertyType) == null)
//    {
//        throw new ArgumentException("The lambda expression 'property' must reference a valid property on this Control.");
//    }

//    if (@this.InvokeRequired)
//    {
//        @this.Invoke(new SetPropertyThreadSafeDelegate<TResult>
//        (SetPropertyThreadSafe),
//        new object[] { @this, property, value });
//    }
//    else
//    {
//        @this.GetType().InvokeMember(
//            propertyInfo.Name,
//            BindingFlags.SetProperty,
//            null,
//            @this,
//            new object[] { value });
//    }
//}