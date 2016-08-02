using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkDynamics
{
    public class VertexSystem
    {
        public List<double[]> systemState { private set; get; }      // {x, y, state, CaConcent} @ time t
        private List<double[]> systemState_tm1;                      // {x, y, state, CaConcent} CRUs @ t-1
        public List<double[]> adjMat { private set; get; }           // NxN matrix storing relationships between nodes.
        public List<List<int>> adjList { private set; get; }         // contains the indecies of the non zero entries of the adjMatrix.

        private bool ready;

        private Random rdn;

        public int sysSize { private set; get; }

        public SystemParameters parameters;

        protected string[] netTypes = { "DF", "ER", "BA", "CH" };                 // Diffusive, Erdős–Rényi, Barabási–Albert
        protected int netType;

        public event EventHandler InitiationReady; // this is raised when the adjMat is ready.

        public VertexSystem(List<double[]> stateAtT, List<double[]> distMatrix)
        {
            systemState = listCopy(stateAtT);
            systemState_tm1 = listCopy(stateAtT);
            adjMat = listCopy(distMatrix);

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
            systemState = listCopy(v.systemState);
            systemState_tm1 = listCopy(v.systemState);
            adjMat = listCopy(v.adjMat);
            adjList = listCopy(v.adjList);

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
            if (!loadMatrix(path)) throw new Exception(); //throw if file name was incompatible.
            parameters.N = adjMat.Count;
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
                systemState.Add(_assembleStateVector(coords, ic, parameters.Eta, i));
                systemState_tm1.Add(_assembleStateVector(coords, ic, parameters.Eta, i));
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

        private bool loadMatrix(string path)
        // load the adjacency matrix from file.
        {
            adjMat = new List<double[]>();
            FileIO f = new FileIO(path, false); // open a file for reading.

            string matType = FileIO.getFileNameFromPath(path);
            matType = matType.Slice(matType.IndexOf("-") + 1, matType.IndexOf("_"));
            bool found = false;
            foreach (int i in range(netTypes.Length))
                if (matType.Equals(netTypes[i]))
                { netType = i; found = true; }
            if (!found) return false;


            while (true)
            {
                List<double> row = f.ReadLineListDouble();
                if (row == null) break; // break if end of file is reached.
                if (row.Count == 0) continue;
                adjMat.Add(new double[row.Count]);
                int i = 0; // set a counter.
                foreach (double d in row)
                {
                    if (d != 0) adjMat[adjMat.Count - 1][i] = 1;
                    else adjMat[adjMat.Count - 1][i] = 0;
                    i++;
                }
            }
            return true;
        }

        private void genAdjMat()
        // generates the network adjMatrix.
        {
            adjMat = new List<double[]>();
            if (parameters.NetType.Equals("BA")) //powerLaw - Barabási–Albert
            {
                ResearchNetwork barbasi = new ResearchNetwork(parameters.N, (int)parameters.K, parameters.nClustes);
                adjMat = barbasi.adjMatrix;
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
                        else if (parameters.NetType.Equals(netTypes[3])) // Chain network.
                        {
                            bool closeLoop = parameters.K > 1; // connect tail to head if K > 1
                            ResearchNetwork.ChainNet(adjMat, closeLoop, parameters.DirectedNetwork);
                        }
                    }
                    //adjMat[i] = dist;
                }
            }
            if (parameters.DirectedNetwork && !parameters.NetType.Equals(netTypes[3])) // randomize connection directions if not a circle net.
                GetDirectedNetwork();
            if (parameters.Weights && !parameters.NetType.Equals(netTypes[0])) // randomize connection directions if not a circle net.
                ApplyRandomWeights();

            if (adjMat != null) ready = true;
        }

        private void ApplyRandomWeights()
        // ApplyRandomWeights - applys random non-negative weights to the edges.
        {
            for (int i = 0; i < parameters.N; i++)
                for (int j = 0; j < parameters.N; j++) // only go through the upper diagonal entries.
                    if (i != j && adjMat[i][j] != 0)
                    {
                        if (adjMat[i][j] < 0) adjMat[i][j] = -0.2;
                        else adjMat[i][j] = rdn.NextDouble();
                        //adjMat[i][j] = 1 - 2d * rdn.NextDouble();
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

        private List<double[]> listCopy(List<double[]> listToCopy)
            // Deep Copies a list of double[]
        {
            List<double[]> copy = new List<double[]>(listToCopy.Count);
            int arrSize = listToCopy[0].Length;
            foreach (double[] arr in listToCopy)
            {
                double[] arr_copy = new double[arrSize];
                for (int i = 0; i < arrSize; i++)
                    arr_copy[i] = arr[i];
                copy.Add(arr_copy);
            }
            return copy;
        }

        private List<List<T>> listCopy<T>(List<List<T>> listToCopy)
        // Deep Copies a list of double[]
        {
            List<List<T>> copy = new List<List<T>>(listToCopy.Count);
            
            foreach (List<T> arr in listToCopy)
            {
                int arrSize = arr.Count;
                List<T> arr_copy = new List<T>(arrSize);
                for (int i = 0; i < arrSize; i++)
                    arr_copy.Add(arr[i]);
                copy.Add(arr_copy);
            }
            return copy;
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
            ResetState(GetInitialCondition(IC), parameters.Eta);
        }

        public void ResetState(int state, double background)
            //ResetState - resets systemState and systemState_tm1, to the default background and activation levels.
        {
            for (int i = 0; i < systemState.Count; i++)
                _ResetState(state, background, i);   
        }

        public void ResetState(List<int> state, double background)
            //ResetState - resets systemState and systemState_tm1, to the default background and activation levels.
        {
            for (int i = 0; i < systemState.Count; i++)
                _ResetState(state[i], background, i);
        }

        private void _ResetState(int state, double background, int i)
        {
            systemState[i][2] = state;
            systemState[i][3] = background;
            systemState_tm1[i][2] = state;
            systemState_tm1[i][3] = background;
        }

        public void updateConcentration(double eta, double alpha, double gamma)
        //public void updateConcentration(double R, double background)
            // updateCaConcentration - updates the concentration of Ca for the system
            // applies R (adjMatrix must have only 0 or 1 entries)
        {
            int size = adjMat.Count;
            for (int i = 0; i < size; i++)
                _updateConcList(eta, alpha, gamma, i, size);
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

        private void _updateConcList(double eta, double alpha, double gamma, int i, int size)
        // this method uses the adjList to update background.
        {
            double spike = 0;
            if (parameters.Weights) //adjMat[i][non_zero_j] - can be omitted if a[ij] is 0 or 1 resulting in ~13% performance gain.
                foreach (int non_zero_j in adjList[i])
                    spike += systemState_tm1[non_zero_j][2] * adjMat[i][non_zero_j]; 
            else
                foreach (int non_zero_j in adjList[i])
                    spike += systemState_tm1[non_zero_j][2];

            systemState[i][3] = eta * Math.Pow((1 + alpha * spike), gamma);
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
            updateSystemState(parameters);
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
                      if (rdn.NextDouble() < systemState[i][3] * inPars.Beta) //!!!!! here Beta is dT!!
                            systemState[i][2] = 1;
                }
                if (systemState_tm1[i][2] == 1)
                {
                    if (rdn.NextDouble() < inPars.Beta)    //probability of transitioning from 1 -> 0
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

        private double[] _assembleStateVector(CoordPoint[] coords, List<int> ic, double cZero, int i)
        {
            return new double[] { coords[i].x, coords[i].y, ic[i], cZero };
        }

        private static System.Collections.IEnumerable range(int range)
        {
            for (int i = 0; i < range; i++) yield return i;
        }
    }

    public class SystemParameters
    {
        public double Eta, Alpha, Gamma, Beta, Connectivity, K;
        public int N, nClustes;
        public CoordPoint bc;
        public string NetType; // powerLaw(BA), uniform(ER), dist(dist)
        public bool DirectedNetwork, Weights;

        public SystemParameters(double Alpha, double Eta, double Gamma, double Beta, double Connectivity,
                                int N, double K, int nClustes, string NetType, CoordPoint bc, bool DirectedNetwork, bool Weights)
        {
            this.Eta = Eta;
            this.Alpha = Alpha;
            this.Gamma = Gamma;
            this.Beta = Beta;
            this.Connectivity = Connectivity;
            this.N = N;
            this.K = K;
            this.nClustes = nClustes;
            this.NetType = NetType;
            this.bc = bc;
            this.DirectedNetwork = DirectedNetwork;
            this.Weights = Weights;
        }
        public SystemParameters(SystemParameters p)
        {
            this.Eta = p.Eta;
            this.Alpha = p.Alpha;
            this.Gamma = p.Gamma;
            this.Beta = p.Beta;
            this.Connectivity = p.Connectivity;
            this.N = p.N;
            this.K = p.K;
            this.nClustes = p.nClustes;
            this.NetType = p.NetType;
            this.bc = p.bc;
            this.DirectedNetwork = p.DirectedNetwork;
            this.Weights = p.Weights;
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
