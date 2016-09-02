using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkDynamics
{
    public class VertexSystem
    {
        public List<double[]> systemState { private set; get; }      // {x, y, state, pON, pOFF, Inactive} @ time t
        private List<double[]> systemState_tm1;                      // {x, y, state, CaConcent, BackRate, Inactive} CRUs @ t-1

        public List<double[]> adjMat { protected set; get; }           // NxN matrix storing relationships between nodes.
        public double[,] adjMatScaled { private set; get; }          // NxN matrix storing relationships between nodes with adjusted weights.

        public List<List<int>> adjList { private set; get; }         // contains the indecies of the non zero entries of the adjMatrix.
        public List<List<int>> adjListInhibitory { private set; get; }         // contains the indecies of the non zero entries of the adjMatrix which are inhibitory.


        private bool ready;

        private Random rdn;

        public int sysSize { private set; get; }

        public SystemParameters parameters;

        protected string[] netTypes = { "DF", "ER", "BA", "CH" };                 // Diffusive, Erdős–Rényi, Barabási–Albert
        protected int netType;

        public event EventHandler InitiationReady; // this is raised when the adjMat is ready.

        public VertexSystem(List<double[]> stateAtT, List<double[]> distMatrix)
        {
            systemState = DataTools.listCopy(stateAtT);
            systemState_tm1 = DataTools.listCopy(stateAtT);
            adjMat = DataTools.listCopy(distMatrix);

            this.rdn = new Random();
            //this.background = background;
            
            sysSize = adjMat.Count;

            ApplyR(1); // reset non-zero entries to 1.

            //success = false;
            //Results = new ExperimentResults(1000);

            ready = true;
        }

        public VertexSystem(VertexSystem v)
        {
            systemState = DataTools.listCopy(v.systemState);
            systemState_tm1 = DataTools.listCopy(v.systemState);
            adjMat = DataTools.listCopy(v.adjMat);
            adjList = DataTools.listCopy(v.adjList);

            parameters = new SystemParameters(v.parameters);

            this.rdn = new Random();
            //this.background = background;

            sysSize = adjMat.Count;

            //ApplyR(1); // reset non-zero entries to 1.

            

            ready = true;
        }

        public VertexSystem(SystemParameters pars, double initialCondition, bool circle, string path)
            // Loads AdjMat from file.
        {
            parameters = pars;
            if (!loadMatrix(path).Result) throw new Exception(); //throw if file name was incompatible.
            //parameters.N = adjMat.Count;
            sysSize = parameters.N;
            ready = true;
            this.rdn = new Random();
            _constructor(initialCondition, circle);
        }

        public VertexSystem(SystemParameters pars, double initialCondition, bool circle, EventHandler handler)
        // creates a new network from scratch.
        {
            this.parameters = pars;

            for (int i = 0; i < netTypes.Length; i++)
                if (netTypes[i].Equals(parameters.NetType))
                    netType = i;

            sysSize = parameters.N;
            ready = false;
            this.rdn = new Random();
            InitiationReady += handler;
            genAdjMatAsync();
            _constructor(initialCondition, circle);
            //while (!ready)
                //System.Threading.Thread.Sleep(10);

        }

        private void _constructor(double initialCondition, bool circle)
        {
            var ic = GetInitialCondition(initialCondition);
            var coords = genCoords(parameters.bc, circle); // generate vertex coordinates.

            systemState = new List<double[]>(sysSize);
            systemState_tm1 = new List<double[]>(sysSize);
            for (int i = 0; i < sysSize; i++)
            {
                systemState.Add(_assembleStateVector(coords, ic, parameters.Eta, parameters.EtaNeg, 0, i));
                systemState_tm1.Add(_assembleStateVector(coords, ic, parameters.Eta, parameters.EtaNeg, 0, i));
            }
        }


        private async void genAdjMatAsync()
            // generates the network adjMatrix.
            // sets the ready property to true - after matrix is generated.
        {
            await Task.Run(new Action(genAdjMat));
            await Task.Run(() => GetAdjList());
            //Console.WriteLine("DONE!");
            InitiationReady?.Invoke(this, null); // call event handler when AdjMat is ready.
            ready = true;
        }

        private void GetAdjList()
            // Fills in the AdjList from the adjMat
        {
            // initialize the adjList
            adjList = new List<List<int>>(parameters.N);
            for (int i = 0; i < parameters.N; i++)
                adjList.Add(new List<int>(20));
            
            // populate with non-zero j indecies
            Parallel.For(0, parameters.N, (i) =>
            {
                for (int j = 0; j < parameters.N; j++)
                    if (adjMat[i][j] != 0 && i != j)
                        adjList[i].Add(j);
            });
        }

        private async Task<bool> loadMatrix(string path)
        // load the adjacency matrix from file.
        {
            adjMat = new List<double[]>();
            FileIO f = new FileIO(path, false); // open a file for reading.
            
            if (!LoadMatrixUpdateParameters(path)) return false; // Update parameters object with extracted parameters.

            while (true)
            {
                List<double> row = f.ReadLineListDouble();
                if (row == null) break; // break if end of file is reached.
                if (row.Count == 0) continue;
                adjMat.Add(new double[row.Count]);
                int i = 0; // set a counter.
                foreach (double d in row)
                {
                    adjMat[adjMat.Count - 1][i] = d;
                    //if (d != 0) adjMat[adjMat.Count - 1][i] = 1;
                    //else adjMat[adjMat.Count - 1][i] = 0;
                    i++;
                }
            }
            GetAdjList();
            return true;
        }

        public void SaveAdjMatToFile()
        // saveAdjMatToFile - saves the adjMatrix to a .mat file and tags it with today's date, time and size.
        {
            int weights = parameters.Weights ? 1 : 0;
            string matName =    "MATRIX-" + netTypes[netType] +
                                "_N#" + parameters.N.ToString("F0") +
                                "_K#" + parameters.K.ToString("F3") +
                                "_W#" + weights +
                                "_Date#" + DateTime.Now.ToString("MM-dd-yyyy#HH-mm-ss") + 
                                ".mat";

            string path = FileIO.setupPathForFileWrite("Data", matName);
            FileIO.saveMatrixToFile(path, new Matrix(adjMat));
        }

        private bool LoadMatrixUpdateParameters(string path)
            // LoadMatrixUpdateParameters - extract the Matrix build parameters from the import file and updates the parameters object.
        {
            string fileName = FileIO.getFileNameFromPath(path);
            string matType = fileName.Slice(fileName.IndexOf("-") + 1, fileName.IndexOf("_"));

            bool found = false;
            foreach (int i in range(netTypes.Length))
                if (matType.Equals(netTypes[i]))
                { netType = i; found = true; }
            if (!found) return false;

            var tagVals = FileIO.ExtractTokens(fileName, '_', '#');

            switch (netTypes[netType])
            {
                case "DF":
                    throw new NotImplementedException("Importing Euclidean Matrix is not supported at this time!");
                default:
                    {
                        try
                        {
                            parameters.N = (int)Double.Parse(FileIO.GetValue(tagVals, "N"));
                            parameters.K = Double.Parse(FileIO.GetValue(tagVals, "K"));
                            parameters.Weights = Double.Parse(FileIO.GetValue(tagVals, "W")) == 1 ? true : false;
                            return true;
                        }
                        catch { return false; }
                    }
            }

        }

        protected void genAdjMat()
        // generates the network adjMatrix.
        {
            adjMat = new List<double[]>();
            bool useWeightedAlgorithmForDynamics = false;
            if (parameters.NetType.Equals("BA")) //powerLaw - Barabási–Albert
            {
                ResearchNetwork barbasi = new ResearchNetwork(parameters.N, (int)parameters.K, parameters.nClustes);
                adjMat = barbasi.adjMatrix;
            }
            else if (parameters.NetType.Equals(netTypes[3])) // Chain network.
            {
                bool closeLoop = parameters.K > 1; // connect tail to head if K > 1
                useWeightedAlgorithmForDynamics = ResearchNetwork.ChainNet(adjMat, parameters.N, closeLoop, parameters.DirectedNetwork, parameters.SpecialPars);
            }
            else
            {
                double L_SQUARED = Math.Pow((parameters.K * parameters.bc.x) / (2 * Math.Sqrt(parameters.N)), 2.0);
                double conn = parameters.Connectivity/100d;
                for (int i = 0; i < sysSize; i++)
                {
                    double[] dist = new double[sysSize];
                    adjMat.Add(new double[sysSize]);
                }
                for (int i = 0; i < sysSize; i++)
                {
                    //double[] dist = new double[sysSize];
                    //adjMat.Add(new double[sysSize]);

                    for (int j = i; j < sysSize; j++)
                    {
                        if (parameters.NetType.Equals("DF")) // Diffusive
                        {
                            if (i == j) { adjMat[i][j] = 0; continue; }
                            adjMat[i][j] = Math.Pow((systemState[i][0] - systemState[j][0]), 2) +
                                      Math.Pow((systemState[i][1] - systemState[j][1]), 2);
                            //dist[j] = PARAM_R / Math.Exp(dist[j] / L_SQUARED);
                            adjMat[i][j] = 1 / Math.Exp(adjMat[i][j] / L_SQUARED);

                            adjMat[j][i] = adjMat[i][j]; //symertry
                        }
                        else if (parameters.NetType.Equals("ER")) //uniform - Erdős–Rényi 
                        {
                            if (i == j) { adjMat[i][j] = 0; continue; }
                            if (rdn.NextDouble() < conn) adjMat[i][j] = 1;
                            else adjMat[i][j] = 0;
                            adjMat[j][i] = adjMat[i][j]; //symertry
                        }
                        
                    }
                    //adjMat[i] = dist;
                }
            }
            if (parameters.DirectedNetwork && !parameters.NetType.Equals(netTypes[3])) // randomize connection directions if not a circle net.
                GetDirectedNetwork();
            if (parameters.Weights && !parameters.NetType.Equals(netTypes[0])) // randomize connection directions if not a circle net.
                ApplyRandomWeights();

            if (!parameters.Weights && useWeightedAlgorithmForDynamics) // if special weights are used bypassing the ApplyRandomWeights() this statement makes sure the right algo is used for the dynamics
                parameters.Weights = true;

            if (adjMat != null) ready = true;
        }

        private void ApplyRandomWeights()
        // ApplyRandomWeights - applys random non-negative weights to the edges.
        {
            //adjMatScaled = new double[parameters.N, parameters.N];
            double scaleFactorInh = parameters.Beta / parameters.Alpha;

            for (int i = 0; i < parameters.N; i++)
                for (int j = 0; j < parameters.N; j++) // only go through the upper diagonal entries.
                    if (i != j && adjMat[i][j] != 0)
                    {
                        //if (adjMat[i][j] < 0) adjMat[i][j] = -0.2;
                        //else adjMat[i][j] = rdn.NextDouble();

                        // assign random weights in the interval [-1,1]
                        adjMat[i][j] = 1 - 2d * rdn.NextDouble();

                        /*
                        // populate the scaled matrix.
                        if (adjMat[i][j] < 0)
                            adjMatScaled[i, j] = scaleFactorInh * adjMat[i][j];
                        else
                            adjMatScaled[i, j] = adjMat[i][j];
                            */
                    }
        }

        private void RescaleAdjMat()
            // RescaleAdjMat - rescales the Inhibitory entries.
        {
            double scaleFactorInh = parameters.Beta / parameters.Alpha;
            for (int i = 0; i < parameters.N; i++)
                for (int j = 0; j < parameters.N; j++) // only go through the upper diagonal entries.
                    if (i != j && adjMat[i][j] != 0)
                    {
                        // scale Inhibitory entries.
                        if (adjMat[i][j] < 0)
                            adjMatScaled[i, j] = scaleFactorInh * adjMat[i][j];
                        else
                            adjMatScaled[i, j] = adjMat[i][j];
                    }
        }

        private void GetDirectedNetwork()
            // GetDirectedNetwork - turns a undirected network into a directed network.
        {
            double pIn = 1.0 / 3.0;
            double pOut = 2 * pIn;
            double uNum = 0;

            for (int i = 0; i < parameters.N; i++)
                for (int j = i + 1; j < parameters.N; j++) // only go through the upper diagonal entries.
                    if (adjMat[i][j] > 0)
                    {
                        uNum = rdn.NextDouble();
                        if (uNum < pIn) // only keep the in-link
                        {
                            adjMat[i][j] = 0;
                            continue;
                        }
                        if (uNum >= pIn && uNum < pOut) // only keep the out link.
                        {
                            adjMat[j][i] = 0;
                            continue;
                        }
                    }
        }

        

        public List<int> getActiveNodes()
            // getActiveNodes - returns a list of the indecies of the active nodes.
        {
            var active = new List<int>(systemState.Count / 3);
            for (int i = 0; i < systemState.Count; i++)
                if (systemState[i][2] == 1)
                    active.Add(i);
            return active;
        }

        public void getActiveNodes(List<double> active)
        // getActiveNodes - returns a list of the indecies of the active nodes.
        // input list MUST be populated with N elements.
        {
            for (int i = 0; i < systemState.Count; i++)
                if (systemState[i][2] == 1)
                    active[i]++;
        }

        public int NumActiveSites()
            // getNumActiveSites - returns the number of active sites at the time of the call.
        {
            int channelsOpen = 0;
            for (int i = 0; i < systemState.Count; i++)
                if (systemState[i][2] == 1)
                    channelsOpen++;
            return channelsOpen;
        }

        public void UpdateTm1()
            // UpdateTm1 - copies the curent system state to t - 1 
        {
            int arrSize = systemState[0].Length;
            int count = 0;
            foreach (double[] arr in systemState)
            {
                double[] arr_copy = new double[arrSize];
                for (int i = 0; i < arrSize; i++)
                    arr_copy[i] = arr[i];
                systemState_tm1[count] = arr_copy;
                count++;
            }
        }

        public void UpdateTm1Fast()
        // UpdateTm1Fast - copies the curent system state to t - 1 using parallel looping.
        {
            int arrSize = systemState[0].Length;
            Parallel.For(0, sysSize, i => _updateTm1(arrSize, i));
        }

        private void _updateTm1(int arrSize, int count)
        {
            double[] arr_copy = new double[arrSize];
            for (int i = 0; i < arrSize; i++)
                arr_copy[i] = systemState[count][i];
            systemState_tm1[count] = arr_copy;
        }

        public void ResetState(double IC)
        {
            ResetState(GetInitialCondition(IC), parameters.Eta, parameters.EtaNeg);
        }

        public void ResetState(int state, double background, double extRate)
            //ResetState - resets systemState and systemState_tm1, to the default background and activation levels.
        {
            for (int i = 0; i < systemState.Count; i++)
                _ResetState(state, background, extRate, i);   
        }

        public void ResetState(List<int> state, double background, double extRate)
            //ResetState - resets systemState and systemState_tm1, to the default background and activation levels.
        {
            for (int i = 0; i < systemState.Count; i++)
                _ResetState(state[i], background, extRate, i);
        }

        private void _ResetState(int state, double background, double extRate, int i)
        {
            systemState[i][2] = state;
            systemState[i][3] = background;
            systemState[i][4] = extRate;
            systemState_tm1[i][2] = state;
            systemState_tm1[i][3] = background;
            systemState_tm1[i][4] = extRate;
        }

        public void updateConcentration(double eta, double alpha, double gamma)
        //public void updateConcentration(double R, double background)
            // updateCaConcentration - updates the concentration of Ca for the system
            // applies R (adjMatrix must have only 0 or 1 entries)
        {
            int size = adjMat.Count;
            for (int i = 0; i < size; i++)
                _updateConcList(eta, parameters.EtaNeg, alpha, parameters.Beta, gamma, i, size);
                //_updateConcList(R, background, i, size); // now using the adjList!
                //_updateConc(R, background, i, size); // now using the adjMat!
        }

        public void updateConcentrationParallel(double R, double background)
        {
            int size = adjMat.Count;
            Parallel.For(0, size, x => _updateConc(R, background, x, size));
        }

        private void _updateConc(double R, double background, int i, int size)
            // this method uses the adjMatrix to update background.
        {
            double spike = 0;
            for (int j = 0; j < size; j++)
            {
                if (i == j) continue;
                spike += R * adjMat[i][j] * systemState_tm1[j][2];
            }
            systemState[i][3] = background + spike;
        }

        private void _updateConcList(double R, double background, int i, int size)
            // this method uses the adjList to update background.
        {
            double spike = 0;
            foreach (int non_zero_j in adjList[i])
            {
                //if (i == non_zero_j) continue;  // this is checked for when list is populated.
                spike += R * systemState_tm1[non_zero_j][2];
            }
            systemState[i][3] = background + spike;
        }

        private void _updateConcList(double eta, double etaNeg, double alpha, double beta, double gamma, int i, int size)
        // this method uses the adjList to update background.
        {
            double spike = 0;
            double inhibition = 0;

            if (parameters.Weights) //adjMat[i][non_zero_j] - can be omitted if a[ij] is 0 or 1 resulting in ~13% performance gain.
            {
                double Aji = 0;
                foreach (int non_zero_j in adjList[i])  // calculate excitation
                {
                    Aji = adjMat[non_zero_j][i]; // incoming edges.
                    if (Aji > 0) // positive edges are excitatory
                        spike += systemState_tm1[non_zero_j][2] * Aji; 
                    else // negative edges are inhibitory
                        inhibition += systemState_tm1[non_zero_j][2] * Aji;
                }
                spike += inhibition * beta / alpha; // divide by alpha to cancel out downstream mult by alpha. 
                systemState[i][4] = etaNeg * Math.Pow((1 - beta * inhibition), gamma); // update BackRate
            }
            else
                foreach (int non_zero_j in adjList[i])
                    spike += systemState_tm1[non_zero_j][2];

            if (1 + alpha * spike < 0) // if forward rate is negative set it to zero;
                systemState[i][3] = 0;
            else
                systemState[i][3] = eta * Math.Pow(1 + alpha * spike, gamma); // update forward rate.
        }

        public void ApplyR(double R)
            //ApplyR - multiplies the connectivity matrix by R
            // @param double - parameter R
        {
            Parallel.For(0, sysSize, i =>
             {
                 for (int j = 0; j < sysSize; j++)
                 {
                     if (adjMat[i][j] != 0) adjMat[i][j] = 1d;
                     if (R != 1) adjMat[i][j] *= R;
                 }
             });
            /*
            for (int i = 0; i < sysSize; i++)
                for (int j = 0; j < sysSize; j++)
                {
                    if (adjMat[i][j] != 0) adjMat[i][j] = 1d;
                    if(R!=1) adjMat[i][j] *= R;
                }
                */
        }

        public void updateSystemState(double[] pars)
        //pars: 0-g, 1-Gamma, 2-Beta, 3-R, 4-Background, 5-Threshold, 6-MaxItr.
        // 0-Eta, 1-Gamma, 2-Beta, 3-Alpha, 4-Threshold, 5-MaxItr.
        {
            parameters.Eta = pars[0];
            parameters.Alpha = pars[3];
            parameters.Gamma = pars[1];
            parameters.Beta = pars[2];
            updateSystemState();
        }

        public void updateSystemState()
        {
            if(parameters.DynamicModelTwoState)
                updateSystemState(parameters);
            else
                updateSystemStateThreeState(parameters);
        }

        public void updateSystemStateThreeState(SystemParameters inPars)
        {
            double tao1 = 0.25; // Temp hardcoded parameter.
            double tao2 = 0.25; // Temp hardcoded parameter

            for (int i = 0; i < inPars.N; i++)
            {
                if (systemState_tm1[i][2] == 1) // check id site is ON
                {
                    if (rdn.NextDouble() < systemState[i][4] * inPars.dt)    //probability of transitioning from 1 -> 0
                        systemState[i][2] = 0;
                    else if (rdn.NextDouble() < tao1 * inPars.dt) // probability of trans into inactive state.
                    {
                        systemState[i][2] = 0; // turn site OFF
                        systemState[i][5] = 1; // set inactive
                    }
                }
                else if (systemState_tm1[i][5] == 1 && rdn.NextDouble() < tao2 * inPars.dt) // check if site is inactive and run against the probability of activating.
                    systemState[i][5] = 0;
                else if (systemState_tm1[i][5] == 0 && rdn.NextDouble() < systemState[i][3] * inPars.dt) // check if active and run probabiliti of turning ON
                    systemState[i][2] = 1;
            }
        }

        public void updateSystemState(SystemParameters inPars)
            // updateSystemState - updates the state for every unit in the system
        {
            for (int i = 0; i < inPars.N; i++)
            {
                if (systemState_tm1[i][2] == 0)
                {
                    //double probability = parameters.G * Math.Pow(systemState[i][3], parameters.Gamma);
                    
                    //if (rdn.NextDouble() < probability) //runs the probability of transitioning from 0 -> 1
                      if (rdn.NextDouble() < systemState[i][3] * inPars.dt) //!!!!! here Beta is dT!!
                            systemState[i][2] = 1;
                }
                //if (systemState_tm1[i][2] == 1)
                else
                {
                    if (rdn.NextDouble() < systemState[i][4] * inPars.dt)    //probability of transitioning from 1 -> 0
                        systemState[i][2] = 0;
                }
            }
        }
        public void AdvanceSystem(SystemParameters inPars)
        //pars: 0-g, 1-Gamma, 2-Beta, 3-R, 4-Background, 5-Threshold, 6-MaxItr.
        // 0-Eta, 1-Gamma, 2-Beta, 3-Alpha, 4-Threshold, 5-MaxItr.
        {
            updateConcentration(parameters.Eta, parameters.Alpha, parameters.Gamma);
            updateSystemState(inPars);
            UpdateTm1();
        }

        public void AdvanceSystem(double[] pars)
        // 0-Eta, 1-Gamma, 2-Beta, 3-Alpha, 4-Threshold, 5-MaxItr.
        {
            parameters.Alpha = pars[3];
           
            //updateConcentration(parameters.R, parameters.cZero);
            updateConcentration(parameters.Eta, parameters.Alpha, parameters.Gamma);
            updateSystemState(pars);
            UpdateTm1();
        }

        public void AdvanceSystem()
        //pars: 0-g, 1-Gamma, 2-Beta, 3-R, 4-Background, 5-Threshold, 6-MaxItr.
        // 0-Eta, 1-Gamma, 2-Beta, 3-Alpha, 4-Threshold, 5-MaxItr.
        {
            updateConcentration(parameters.Eta, parameters.Alpha, parameters.Gamma);
            updateSystemState();
            UpdateTm1();
        }

        public List<int> GetInitialCondition(double probabilityOfOnState)
            // GetInitialCondition -    returns the initial condition vector 
            //                          based on the provided probability for channels beeing on.
        {
            var ic = new List<int>(sysSize);
            foreach (int i in range(sysSize))
                if (probabilityOfOnState > 0.999)
                    ic.Add(1);
                else if (probabilityOfOnState < 0.001)
                    ic.Add(0);
                else {
                    if (rdn.NextDouble() < probabilityOfOnState)
                        ic.Add(1);
                    else
                        ic.Add(0);
                }
            return ic;
        }

        private CoordPoint[] genCoords(CoordPoint bc, bool circle)
            // NewMethod - generates coordinate points for the vertecies.
            // circle: true - get circularly distributed points. false - uniform distribution.
        {
            double x, y;
            double radStep = 2 * Math.PI / sysSize;
            var temp = new CoordPoint[sysSize];

            for (int i = 0; i < sysSize; i++)
            {
                if (!circle)
                {
                    x = rdn.NextDouble();
                    y = rdn.NextDouble();
                }
                else
                {
                    x = bc.x * Math.Cos(radStep * i);
                    y = bc.y * Math.Sin(radStep * i);
                }
                temp[i] = new CoordPoint(x, y);
            }
            return temp;
        }

        private double[] _assembleStateVector(CoordPoint[] coords, List<int> ic, double cZero, double BackRate, double Inactive, int i)
        {
            return new double[] { coords[i].x, coords[i].y, ic[i], cZero, BackRate, Inactive };
        }

        private static System.Collections.IEnumerable range(int range)
        {
            for (int i = 0; i < range; i++) yield return i;
        }
    }

    public class SystemParameters
    {
        public double Eta, Alpha, Gamma, Beta, Connectivity, K, dt, EtaNeg;
        public int N, nClustes;
        public CoordPoint bc;
        public string NetType; // powerLaw(BA), uniform(ER), dist(dist)
        public bool DirectedNetwork, Weights, DynamicModelTwoState;
        public List<string[]> SpecialPars;

        public SystemParameters(double Alpha, double Eta, double Gamma, double Beta, double EtaNeg, double dt, double Connectivity,
                                int N, double K, int nClustes, string NetType, CoordPoint bc, 
                                bool DirectedNetwork, bool Weights, List<string[]> SpecialPars,
                                bool DynamicModelTwoState)
        {
            this.Eta = Eta;
            this.Alpha = Alpha;
            this.Gamma = Gamma;
            this.Beta = Beta;
            this.EtaNeg = EtaNeg; 
            this.dt = dt;
            this.Connectivity = Connectivity;
            this.N = N;
            this.K = K;
            this.nClustes = nClustes;
            this.NetType = NetType;
            this.bc = new CoordPoint(bc.x, bc.x);
            this.DirectedNetwork = DirectedNetwork;
            this.Weights = Weights;
            this.SpecialPars = DataTools.listCopy(SpecialPars);
            this.DynamicModelTwoState = DynamicModelTwoState;
            
        }
        public SystemParameters(SystemParameters p)
        {
            this.Eta = p.Eta;
            this.Alpha = p.Alpha;
            this.Gamma = p.Gamma;
            this.Beta = p.Beta;
            this.EtaNeg = p.EtaNeg;
            this.dt = p.dt;
            this.Connectivity = p.Connectivity;
            this.N = p.N;
            this.K = p.K;
            this.nClustes = p.nClustes;
            this.NetType = p.NetType;
            this.bc = new CoordPoint(p.bc.x, p.bc.x);
            this.DirectedNetwork = p.DirectedNetwork;
            this.Weights = p.Weights;
            this.SpecialPars = DataTools.listCopy(p.SpecialPars);
            this.DynamicModelTwoState = p.DynamicModelTwoState;
        }

        public double? GetParameter(string name)
        {
            switch (name)
            {
                case "Alpha":
                    return Alpha;
                case "Gamma":
                    return Gamma;
                case "Beta":
                    return Beta;
                case "Eta":
                    return Eta;
                default:
                    return null;
            }
        }

    } 

    public struct CoordPoint
    { 
        public double x, y;
        public CoordPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        
    }

    
}
