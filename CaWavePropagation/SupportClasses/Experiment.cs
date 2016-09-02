using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkDynamics
{
    public class Experiment : VertexSystem
    {
        //private VertexState vertState;
        //public bool success;
        // Experiment Results Storage Devices;
        
        public ExperimentResults Results;
        public ExperimentParameters ExpPars;

        public static int experimentIndex;
        private string[] experimentCode = { "X1", "X2", "X3", "X4", "X5", "X6", "X7" }; // X1 - ; X2 - ; 
                                                                            // X3 - Examines the power of the top 10 sites vs the remaining.
                                                                            // X4 - Produces a density of active sites over a long run for a range of the given parameter.
                                                                            // X5 - Same as X4 but mixes 2 initial conditions (0 plus the one provided by the user) to provide a filler picture.
                                                                            // X7 - Calculates n/N for systems of different connectivity. n - number of components of the leading eigen vector's components exceeding a threshold of 10% above AVG component size. 

        private static List<Experiment> jobs;
        //private static double varMin, varStep;
        //private static double stopCondition, initialCondition;
        //private static int maxItr;

        public static event EventHandler<ExperimentEventArgs> UpdateStatusStrip; //this event provides status updates.
        public static event EventHandler<ExperimentCompleteEventArgs> ExperimentComplete; //this event is fired when the experiment is complete.

        public Experiment(VertexSystem vState) : base(vState)
        {
            Results = new ExperimentResults(1000);
            Results.Success = false;
            //vertState = vState;
        }

        public Experiment(VertexSystem vState, int expID) : this(vState)
        {
            experimentIndex = expID;
        }

        public Experiment(VertexSystem vState, int expID,
                            double initialCondition, double threshold, double maxItr, 
                            string varName, double varMin, double varStep) : this(vState, expID)
        {
            ExpPars = new ExperimentParameters(this.parameters, initialCondition, threshold, maxItr, varName, varMin, varStep);
        }

        public static async void PerformExperiment( VertexSystem vs, 
                                                    double varMin, double varStep, string varName,
                                                    int numRuns, int numIntervals,
                                                    double initialCondition, double stopCondition,
                                                    int maxIterations, int numJobs, int expID )
        // varNames: R, Gamma, Beta, G, cZero
        {
            DateTime startTime = DateTime.Now;
            getExperiments(vs, numJobs, expID, initialCondition, stopCondition, maxIterations, varName, varMin, varStep);

            if (numRuns > 1) //numRuns has to be >= to numJobs
            {
                for (int i = 0; i < numIntervals; i++) //step through the variable parameter.
                {
                    FileIO file = jobs[0].SetFile(i); //get file name all jobs are just copies of eachother so it doesn't metter which one the method is called on.
                    if (numRuns > numJobs)
                    {
                        for (int j = 0; j < numRuns; j += numJobs) // this loop runs the jobs on parallel threads. All same work.
                        {
                            await ParallelJobExecutor(numJobs, expID); // releases the main thread until done.
                            WriteResultsToFile(file, expID);
                            SendStatusUpdate(numIntervals, numRuns, i, j + numJobs, startTime);
                        }
                    }
                    else
                    {
                        await ParallelJobExecutor(numRuns, expID);
                        WriteResultsToFile(file, expID);
                        SendStatusUpdate(numIntervals, numRuns, i, numRuns, startTime);
                    }
                    IncrementVarForAllJobs(i);
                    file.CloseFile();
                    
                }
            }
            else
            {
                int k = 0;
                FileIO[] files = new FileIO[numJobs];
                for (int i = 0; i < numIntervals; i++) //step through the variable parameter.
                {
                    jobs[k].ExpPars.IncrementVar(i);
                    files[k] = jobs[k].SetFile(i);
                    if (++k >= numJobs)
                    {
                        await ParallelJobExecutor(files, expID);
                        SendStatusUpdate(numIntervals, numRuns, i, 0, startTime); // update GUI status bar.
                        k = 0;
                    }
                }
            }
        }

        private static void SendStatusUpdate(int numIntervals, int numRuns, int currInterval, int currRun, DateTime startTime)
            // Delivers status information to the calling object.
        {
            if (UpdateStatusStrip != null)
            {
                float job = numIntervals * numRuns;

                int progress = (int)(((currInterval * (float)numRuns + (currRun + 1f)) / job) * 100f);

                TimeSpan eTime = DateTime.Now.Subtract(startTime);

                if (progress > 100) progress = 100;

                string msg =
                    " Progress: " + progress + "%" +
                    " :: Step: " + (currInterval + 1) + "/" + numIntervals +
                    " :: Current Run: " + (currRun + 1) + "/" + (numRuns).ToString("D") +
                    " :: Elapsed time: " + eTime.ToString(@"hh\:mm\:ss");

                UpdateStatusStrip(null, new ExperimentEventArgs(progress, msg));
            }
        }

        private static async Task ParallelJobExecutor(FileIO[] files, int expID)
        {
            await Task.Run(() =>
            {
                //Parallel.ForEach<Experiment>(jobs, job => job.Simulation(expID, job));
                Parallel.For(0, jobs.Count, i => jobs[i].Simulation(expID, jobs[i]));

                int j = 0;
                foreach (FileIO file in files)
                {
                    if (jobs[j].Results.Success)
                    {
                        ExperimentCallbacks.CallBacks[expID](jobs[j], file);
                        file.CloseFile();
                        jobs[j++].Results.Reset(); //Reset the results and clear for reuse.
                    }
                    else { file.CloseFile(); jobs[j++].Results.Reset(); }
                }
            });
        }

        private static void IncrementVarForAllJobs(int step)
            // use when doing multiple parallel runs with same params.
        {
            foreach(Experiment job in jobs)
                job.ExpPars.IncrementVar(step);
        }

        private static async Task ParallelJobExecutor(int numJobs, int simNum)
        {
            await Task.Run(() => Parallel.For(0, numJobs, i => jobs[i].Simulation(simNum, jobs[i])));
        }

        private static void WriteResultsToFile(FileIO file, int expID)
        {
            //int k = 0;
            foreach (Experiment job in jobs) {
                if (job.Results.Success)
                    ExperimentCallbacks.CallBacks[expID](job, file);
                job.Results.Reset(); // Reset the result for next itterations
            }
        }

        private static void getExperiments(VertexSystem v, int numJobs, int expID)
        // getExperiments - loads the network instances into experiments.
        {
            jobs = new List<Experiment>(numJobs);
            for (int i = 0; i < numJobs; i++)
                jobs.Add(new Experiment(v, expID));
        }

        private static void getExperiments(VertexSystem v, int numJobs, int expID,
                                            double initialCondition, double threshold, double maxItr,
                                            string varName, double varMin, double varStep)
        // getExperiments - loads the network instances into experiments.
        {
            jobs = new List<Experiment>(numJobs);
            for (int i = 0; i < numJobs; i++)
                jobs.Add(new Experiment(v, expID, initialCondition, threshold, maxItr, varName, varMin, varStep));
        }

        private void Simulation(int simNum, Experiment job)
            // Signature conversion to legacy method competability.
        {
            Simulation(job.ExpPars.pars, simNum, job.ExpPars.InitialCodition);
        }

        public bool Simulation(double[] pars, int simNum, double initialCondition)
            // Legacy
            // Simulation - runs the specified simulation.
        {
            bool success = false;
            switch(simNum)
            {
                case 1:
                    success = runSim1(pars);
                    break;
                case 2:
                    success = runSim2(pars, initialCondition);
                    break;
                case 3:
                    success = runSim3(pars, initialCondition);
                    break;
                case 4:
                    success = runSim4(pars, initialCondition);
                    break;
                case 5:
                    success = runSim5();
                    break;
                case 6:
                    success = runSim6();
                    break;

            }
            return success;
        }

        

        private bool runSim1(double[] pars)
        // Simulation routine for experiment 2.
        //pars: 0-g, 1-Gamma, 2-Beta, 3-R, 4-Background, 5-Threshold, 6-MaxItr.
        // 0-Eta, 1-Gamma, 2-dt, 3-Alpha, 4-Threshold, 5-MaxItr, 6-EtaNeg, 7-Beta.
        {
            bool symRunStop = true;
            int runs = 0, time = 0;
            
            double threshold = (double)(sysSize) * pars[4] / 100.0;

            ResetState(0, pars[0], pars[6]);

            while (symRunStop)
            {
                if (runs > pars[5]) { Results.Success = false; break; } //return false; }

                updateConcentration(pars[0], pars[3], pars[1]);
                updateSystemState(pars);

                UpdateTm1();
                time++;
                runs++;
                if (NumActiveSites() > threshold)
                {
                    symRunStop = false; //stop simulation if number of open channels reaches a certain percantage
                    Results.Success = true;
                    break;
                }
            }
            if (!Results.Success) Results.Success = false;
            //Results.Succes = Results.Succes;
            return Results.Success;
        }
    
        private bool runSim2(double[] pars, double state)
        // Simulation routine for experiment 3 (PhaseTransitions I).
        //pars: 0-g, 1-Gamma, 2-Beta, 3-R, 4-Background, 5-Threshold, 6-MaxItr.
        // 0-Eta, 1-Gamma, 2-dt, 3-Alpha, 4-Threshold, 5-MaxItr, 6-EtaNeg, 7-Beta.
        {
            int numSamples = 150;
            int blockSize = 30;
            double[] samples = new double[numSamples];
            double old_avg = -10, avg = 0;

            bool symRunStop = true;
            int runs = 0;

            double threshold = pars[4] / 100.0;

            if (state == 0)         ResetState(0, pars[0], pars[6]);
            else if (state == 1)    ResetState(1, pars[0], pars[6]);
            else                    ResetState(GetInitialCondition(state), pars[0], pars[6]);

            while (symRunStop)
            {
                if (runs > pars[5]) { Results.Success = false; break; }

                AdvanceSystem(pars);
                runs++;

                samples[(runs++) % numSamples] = NumActiveSites(); //running avg bracket;

                if (runs > numSamples)
                {
                    avg = samples.Average(); //runAvg(samples);
                    if (runs % blockSize == 0)
                    {
                        if (Math.Abs(old_avg - avg) / avg < threshold)
                        {
                            symRunStop = false;
                            Results.Success = true;
                            Results.MeanActivity = avg;
                            break;
                        }
                        old_avg = avg;
                    }
                }
            }
            if (!Results.Success) Results.Success = false;
            //success = success;
            return Results.Success;
        }

        private bool runSim3(double[] pars, double initialCondition)
        //  Simulation routine for experiment 4 (PhaseTransitions II).
        //  pars: 0-g, 1-Gamma, 2-Beta, 3-R, 4-Background, 5-Threshold, 6-MaxItr.
        // 0-Eta, 1-Gamma, 2-dt, 3-Alpha, 4-Threshold, 5-MaxItr, 6-EtaNeg, 7-Beta.
        {
            bool symRunStop = true;
            int runs = 0;
            int sampleEvery = 5;

            if (initialCondition == 0) ResetState(0, pars[0], pars[6]);
            else if (initialCondition == 1) ResetState(1, pars[0], pars[6]);
            else ResetState(GetInitialCondition(initialCondition), pars[0], pars[6]);

            while (symRunStop)
            {
                if (runs > pars[5]) { 
                    if(Results.NumActiveSites.Count > 0)
                        Results.Success = true; 
                    symRunStop = false; 
                    break; 
                }

                AdvanceSystem(pars);
                runs++;

                if(runs % sampleEvery == 0) //record evry sampling interval.
                    Results.NumActiveSites.Add(NumActiveSites());
            }

            //Results.Succes = success;
            return Results.Success;
        }

        private bool runSim4(double[] pars, double initialCondition)
        //  Simulation routine for experiment 4 (PhaseTransitions II).
        //  Exp 4 produces a density of active sites over a long run for a range of params
        //  This method is an extension to experiment 4 running the system with the initial
        //  condition provided as well as a IC = 0;
        //  pars: 0-g, 1-Gamma, 2-Beta, 3-R, 4-Background, 5-Threshold, 6-MaxItr.
        // 0-Eta, 1-Gamma, 2-Beta, 3-Alpha, 4-Threshold, 5-MaxItr.
        {
            int segments = 5; // number of different IC used throughout this run
            bool success = false;
            double temp = pars[5]; // store for recovery
            pars[5] /= segments;   // split the exp duration it 2
            double dIC = initialCondition / (segments - 1);

            while (segments > 0)
            {
                //Console.WriteLine($"Segment: {segments}, IC: {initialCondition}");
                success = runSim3(pars, 0);
                if (success)
                {
                    initialCondition -= dIC; 
                    segments--;
                }
            }

            pars[5] = temp; // recover original value.
            return success;
        }

        private bool runSim5()
        //  Simulation routine for experiment 4 (PhaseTransitions II).
        //  pars: 0-g, 1-Gamma, 2-Beta, 3-R, 4-Background, 5-Threshold, 6-MaxItr.
        // 0-Eta, 1-Gamma, 2-dt, 3-Alpha, 4-Threshold, 5-MaxItr, 6-EtaNeg, 7-Beta.
        {
            bool symRunStop = true;
            int runs = 0;
            int sampleEvery = 5;

            double warmUp = ExpPars.N / 2;

            if (ExpPars.InitialCodition == 0) ResetState(0, ExpPars.Eta, ExpPars.EtaNeg);
            else if (ExpPars.InitialCodition == 1) ResetState(1, ExpPars.Eta, ExpPars.EtaNeg);
            else ResetState(GetInitialCondition(ExpPars.InitialCodition), ExpPars.Eta, ExpPars.EtaNeg);

            // This Vector will store the active sites density.
            Results.DensityVector = new List<double>(ExpPars.N);
            for (int i = 0; i < ExpPars.N; i++)
                Results.DensityVector.Add(0d);

            while (symRunStop)
            {
                if (runs > ExpPars.MaxItr)
                {
                    Results.Success = true;
                    symRunStop = false;
                    double sum = Results.DensityVector.Sum();
                    for (int i = 0; i < ExpPars.N; i++)
                        Results.DensityVector[i] /= sum;
                    break;
                }

                AdvanceSystem(ExpPars);
                runs++;

                if (runs > warmUp && runs % sampleEvery == 0) //record evry sampling interval after the initial warmup.
                    getActiveNodes(Results.DensityVector);
            }

            //Results.Succes = success;
            return Results.Success;
        }

        private bool runSim6()
            // calculate n/N where n - the number of leading eigen vector components exceeding 10% of the average component size.
        {
            bool symRunStop = true;
            int runs = 0;
            parameters.Connectivity = ExpPars.K;
            parameters.K = ExpPars.K;
            adjMat = null;
            Matrix M;
            int maxLambdaIndex = 0;
            if (parameters.NetType == "BA")
            {
                adjMat = ResearchNetwork.barbasiAlbert(parameters.N, (int)parameters.K);
                M = new Matrix(adjMat);
                M.calcEigen();
            }
            else
            {
                genAdjMat(); // gen new network (this updates only the adjMat)
                M = new Matrix(adjMat);
                // get eigen vectors.
                M.CalcEigenGeneral();
                // find largest eigen vector.
                for (int i = 0; i < M.LambdaR.Length; i++)
                    if (M.LambdaR[i] > M.LambdaR[maxLambdaIndex])
                        maxLambdaIndex = i;
            }
            


            
            // get the avg componet size.
            double norm = 0;
            double max = 0;
            List<double> v = new List<double>(parameters.N);
            for (int i = 0; i < parameters.N; i++)
            {
                double e;
                if (parameters.NetType == "BA")
                    e = Math.Abs(M.EigenV[i][0]);
                else
                    e = Math.Abs(M.EigenVectors[i, maxLambdaIndex]);

                norm += Math.Pow(e, 2);
                if (e > max)
                    max = e;
                v.Add(e);
            }

            norm = Math.Sqrt(norm);

            // get the number of components exceeding the threshold.
            int numberAboveThreshold = 0;
            double threshold = ExpPars.StopCodition * max / 100;
            for (int i = 0; i < parameters.N; i++)
                if (v[i] > threshold)
                    numberAboveThreshold++;

            M = null;
            // record result.
            Results.MeanActivity = numberAboveThreshold; // use this container to store the output.
            Results.Success = true;
            return true;
            //throw new NotImplementedException();
        }

        private double runAvg(double[] samples)
        {
            double avg = 0;
            foreach (double s in samples)
                avg += s;
            return avg / samples.Length;
        }

        public string genExpFileName(int run)
        // genExpFileName - generates a csv file name with the proper signature.
        // pars: 0-g, 1-Gamma, 2-Beta, 3-R, 4-Background, 5-Threshold, 6-MaxItr
        //       0-Eta, 1-Gamma, 2-Beta, 3-Alpha, 4-Threshold, 5-MaxItr.
        {
            string k_decimapPlaces = "F0";
            if (ExpPars.NetType == "ER") k_decimapPlaces = "F2";

            string fileName =
                    ExpPars.NetType + 
                    "-" + experimentCode[experimentIndex] +
                    "#" + run +
                    "_N#" + ExpPars.N.ToString("F0") +
                    "_K#" + ExpPars.K.ToString(k_decimapPlaces) +
                    "_E#" + ExpPars.Eta.ToString("F6") +
                    "_A#" + ExpPars.Alpha.ToString("F3") +
                    "_Ga#" + ExpPars.Gamma.ToString("F3") +
                    "_dT#" + ExpPars.Beta.ToString("F2") +
                    "_T#" + ExpPars.StopCodition.ToString("F1") +
                    "_I#" + ExpPars.InitialCodition.ToString("F1") +
                    ".csv";
            return fileName;
        }

        public FileIO SetFile(int i)
        // SetFile - creats a new file to record experimental results.
        {
            string fileName = genExpFileName(i);
            return new FileIO(FileIO.setupPathForFileWrite("data", fileName), true);
        }
    }

    public class ExperimentCallbacks
    {
        public static Action<Experiment, FileIO>[] CallBacks = {null, null, MeanActivityToFile, NumActiveSitesToFile, NumActiveSitesToFile, DensityVectorToFile, MeanActivityToFile};

        private static void MeanActivityToFile(Experiment exp, FileIO file)
        // callbackExp3 - records the results of the finished jobs to file.
        {
            file.WriteLine(exp.Results.MeanActivity.ToString()); 
            //Console.WriteLine(vs.meanActivity);
        }

        private static void NumActiveSitesToFile(Experiment exp, FileIO file)
        // callbackExp3b - records the results of the finished jobs to file.
        {
            foreach (int i in exp.Results.NumActiveSites) // writes the vector of active sites to file.
                file.WriteLine(i.ToString());
        }

        private static void DensityVectorToFile(Experiment exp, FileIO file)
        // callbackExp3b - records the results of the finished jobs to file.
        {
            foreach (double i in exp.Results.DensityVector) // writes the vector of active sites to file.
                file.WriteLine(i.ToString());
        }
    }

    public class ExperimentResults : EventArgs
    {
        public bool Success;
        public double Activity;
        public double MeanActivity;
        public List<int> NumActiveSites;
        public List<double> DensityVector;

        public ExperimentResults(int runs)
        {
            Success = false;
            Activity = 0;
            NumActiveSites = new List<int>(runs);
        }

        public void Reset()
        {
            Success = false;
            Activity = 0;
            MeanActivity = 0;
            NumActiveSites.Clear();
        }
    }

    public class ExperimentParameters : SystemParameters
    {
        public double[] pars { get; private set; } //  pars: 0-g, 1-Gamma, 2-Beta, 3-R, 4-Background, 5-Threshold, 6-MaxItr
                                                   // 0-Eta, 1-Gamma, 2-dt, 3-Alpha, 4-Threshold, 5-MaxItr, 6-EtaNeg, 7-Beta.
        private int varParameter;
        int numSteps;
        private double varMin, varStep;
        public double InitialCodition { get; private set; }
        public double StopCodition { get; private set; }
        public double MaxItr { get; private set; }
        public ExperimentParameters(SystemParameters p, double initialCondition, double threshold, double maxItr, 
                             string varName, double varMin, double varStep) : base(p)
        {
            pars = new double[] { p.Eta, p.Gamma, p.dt, p.Alpha, threshold, maxItr, p.EtaNeg, p.Beta, p.K };

            InitialCodition = initialCondition;
            StopCodition = threshold;
            MaxItr = maxItr;

            int? temp = GetParameterIndex(varName);
            if (temp != null)
                varParameter = (int)temp;
            else throw new ArgumentException();

            pars[varParameter] = varMin;
            SyncMemberVars();
            this.varMin = varMin;
            this.varStep = varStep;
            numSteps = 0;
        }
        public int? GetParameterIndex(string name)
            // GetParameterIndex - returns the index of a system prameter given its name.
            // varNames: Eta, Gamma, dt, Alpha
        {
            switch (name)
            {
                case "Eta":
                    return 0;
                case "Gamma":
                    return 1;
                case "dt":
                    return 2;
                case "Alpha":
                    return 3;
                case "Beta":
                    return 7;
                case "EtaNeg":
                    return 6;
                case "K":
                    return 8;
                default:
                    return null;
            }
        }
        public void SyncMemberVars()
            //Syncs the pars array with the member vars;
        {
            Alpha = pars[3]; Gamma = pars[1]; dt = pars[2]; Eta = pars[0]; Beta = pars[7]; EtaNeg = pars[6]; K = pars[8];
        }
        public double IncrementVar()
            // IncrementVar - increments the variable paramenter.
        {
            pars[varParameter] += varStep;
            SyncMemberVars();
            return pars[varParameter];
        }
        public double IncrementVar(int step)
        // IncrementVar - increments the variable paramenter a given number of steps.
        {
            pars[varParameter] = varMin + step * varStep;
            SyncMemberVars();
            return pars[varParameter];
        }

        public void ResetVar()
            // ResetVar - resets the variable parameter to its min value.
        {
            pars[varParameter] = varMin;
            numSteps = 0;
        }
    }

    public class ExperimentCompleteEventArgs : EventArgs
    {
        public bool Done, Success;
        public ExperimentCompleteEventArgs(bool Done, bool Success)
        {
            this.Done = Done;
            this.Success = Success;
        }
    }

    public class ExperimentEventArgs : EventArgs
    {
        public int progress;
        public string status;
        public ExperimentEventArgs(int progress, string status)
        {
            this.progress = progress;
            this.status = status;
        }
    }
}
