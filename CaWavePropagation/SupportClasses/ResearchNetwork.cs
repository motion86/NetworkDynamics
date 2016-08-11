using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkDynamics
{
    class ResearchNetwork
        // ResearchNetwork - this class provides the different types of network you might be interested to study.
        //                   Network types supported: Random, simple pLaw, Barabási–Albert pLaw
        // PublicVars:
        //              @List<double[]> adjMatrix - the adjacency matrix for the network
        //
        // Public Methods:
        //              @
    {
        public List<double[]> adjMatrix;      //(#verticies)x(#verticies) adjacency matrix for the system
        public List<Vertex> vNet { get; private set; }

        private int size;
        private int connectivity;
        private int nLinks;
        private static Random rdn;

        public ResearchNetwork(int inSize)
            // Empty Network Constructor.
        {
            size = inSize;
            //createMatrix(inSize);
            spawnVertices(inSize);
            rdn = new Random();

        }

        public ResearchNetwork(int inSize, int inNlinks, int clusters)
            // Barbasi Albert Constructor.
        {
            size = inSize;
            //connectivity = inConn;
            nLinks = inNlinks;
            rdn = new Random();

            //createMatrix(inSize);
            spawnVertices(inSize);
            BarbasiAlbert(inNlinks, clusters);

        }

        public void addEdges(List<double[]> adjMat)
            // Adds all the edges to the vNet based on an adjMat and sets the weigths to 1
        {
            foreach (int i in range(size))
                for (int j = i+1; j < size; j++)
                {
                    if (adjMat[i][j] > 0)
                    {
                        vNet[i].addEdge(vNet[j]);
                        vNet[j].addEdge(vNet[i]);
                    }
                }
        }

        public void BarbasiAlbert(int links)
        {
            adjMatrix = barbasiAlbert(this.size, links);
        }

        public void BarbasiAlbert(int links, int clusters)
        {
            if (clusters <= 1) BarbasiAlbert(links);
            else
            {
                int clusterSize = size / clusters;
                if(clusterSize * clusters != size) 
                    size = clusterSize * clusters;

                adjMatrix = createMatrix(size);
                foreach (int i in range(clusters))
                {
                    var subNet = barbasiAlbert(clusterSize, links);
                    LinearAlgebra.insertMat<double>(adjMatrix, subNet, i);
                }
                // connect the clusters together.
                foreach(int i in range(clusters, 1))
                    foreach (int j in range(clusters, 1))
                    {
                        if (i == j) continue;
                        foreach (int k in range(links))
                            connectClustersRand(i, j, clusterSize);
                    }
            }
        }

        private void connectClustersRand(int ClustA, int ClustB, int cSize)
            // connectCulsterRand - makes random connection between cluster A and cluster B.
            // cluster intex is in range (1 to n) where n is the number of clusters in the sys.
        {
            while (true)
            {
                int i = rdn.Next(cSize * (ClustA - 1), cSize * ClustA);
                int j = rdn.Next(cSize * (ClustB - 1), cSize * ClustB);

                if (adjMatrix[i][j] == 0)
                {
                    adjMatrix[i][j] = 1; adjMatrix[j][i] = 1;
                    break;
                }
            }
        }

        private List<double[]> barbasiAlbert(int size, int links)
            // barbasiAlbert - generates a Barbasi Albert network.
        {
            List<double[]> adjMat = createMatrix(size);

            double[] probabilities = new double[size];

            for (int i = 0; i < size; i++) 
            {
                if (i <= links) //the first few nodes are automatically connected.
                {
                    for (int j = 0; j < links+1; j++)
                    {
                        if (i == j) continue;
                        adjMat[i][j] = 1; adjMat[j][i] = 1;

                    }
                }
                else
                {
                    int links_ = links;

                    getProbabilities(ref probabilities, adjMat, i); // update probabilities

                    while (links_ > 0) // run untill all connections are established.
                    {
                        for (int j = 0; j < i; j++) // runs only up to the current size of the system.
                        {
                            if (i == j) continue;
                            if (adjMat[i][j] == 0 && probabilities[j] > rdn.NextDouble())
                            {
                                adjMat[i][j] = 1; adjMat[j][i] = 1;
                                links_--;
                            }
                        }
                    }
                }
            }
            return adjMat;
        }

        public static bool ChainNet(List<double[]> adjMat, int size, bool closeLoop, bool oneWay, List<string[]> specialPars)
            // ChainNet - chains up all nodes in the network one after the other.
        {
            //Get the values from the special parameter's array
            string temp_fwd = FileIO.GetValue(specialPars, "fwd");
            string temp_bkw = FileIO.GetValue(specialPars, "bkw");
            bool rand = FileIO.GetValue(specialPars, "rand") != null ? true : false;
            // if null set to 1
            double fwd = temp_fwd != null ? Double.Parse(temp_fwd) : 1d;
            double bkw = temp_bkw != null ? Double.Parse(temp_bkw) : 1d;

            if (rand && rdn == null)
                rdn = new Random();

            for (int i = 0; i < size; i++)
            {
                double[] dist = new double[size];
                adjMat.Add(new double[size]);
            }
            for (int i = 0; i < size; i++)
            {
                if (i < size - 1)
                {
                    adjMat[i][i + 1] = GetRandomWeight(fwd, rand);
                    if (!oneWay)
                        adjMat[i + 1][i] = GetRandomWeight(bkw, rand);

                }
                else if (closeLoop) // connect last node to first.
                {
                    adjMat[i][0] = GetRandomWeight(fwd, rand);
                    if (!oneWay)
                        adjMat[0][i] = GetRandomWeight(bkw, rand);
                }
            }
            
            if (temp_fwd != null || temp_bkw != null) 
                return true; // true to use Wighted Network Dynamics.
            else
                return false;
        }

        private static double GetRandomWeight(double weight, bool rand)
            // rand = false => returns weight
            // rand = true, weight = -1 => returns neg. random num. [-1,0]
            //            , weight = 0 => returns random in interval [-1,1]
            //            , weight = 1 => returns pos random [0,1]
        {
            if (rand)
            {
                switch ((int)weight)
                {
                    case -1:
                        return -rdn.NextDouble();
                    case 0:
                        return 1 - 2d * rdn.NextDouble();
                    case 1:
                        return rdn.NextDouble();
                    default:
                        return rdn.NextDouble();
                }
                
            }
            else
                return weight;

        }

        private int getSumOfDeg(List<double[]> adjMat, int upto)
            // getSumDeg - O(N^2) - returns the sum of the degrees of all nodes in the system.
            // @return int - the sum of the degrees on all nodes.
        {
            int sum = 0;

            for (int i = 0; i < upto; i++) sum += (int)adjMat[i].Sum();

            return sum;
        }

        private void getProbabilities(ref double[] probabilities, List<double[]> adjMat, int upto)
            // getProbabilities - updates the connection probabilities.
            // @param double[] - reference to the connection probability array.
            // @param int - current net size.
        {
            int denomSum = getSumOfDeg(adjMat, upto);
            for (int i = 0; i < upto; i++)
            {
                probabilities[i] = adjMat[i].Sum() / denomSum;
            }
        }

        private void spawnVertices(int inSize)
        {
            vNet = new List<Vertex>(inSize);
            foreach (int i in range(inSize)) vNet.Add(new Vertex());
        }

        private List<double[]> createMatrix(int inSize)
            // createMatrix - generate an empty adj matrix.
        {
            var adjMat = new List<double[]>(inSize);
            for (int i = 1; i < inSize + 1; i++)
            {
                adjMat.Add(new double[inSize]);
            }
            return adjMat;
        }

        private static System.Collections.IEnumerable range(int range)
        {
            for (int i = 0; i < range; i++) yield return i;
        }
        private static System.Collections.IEnumerable range(int range, int offset)
        {
            for (int i = offset; i < range + offset; i++) yield return i;
        }

    }

}
