using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkDynamics
{
    class NetStats
    {
        private List<double[]> data;
        private double thresholdLow;
        private double thresholdHigh;
        
        public NetStats()
        {
            data = new List<double[]>();
        }

        public NetStats(List<double[]> inData)
        {
            data = inData;
        }

        public void addDataPoint(double x, double y)
        {
            data.Add(new double[2] {x,y});
        }

        public void setThresholds(double low, double high)
        {
            thresholdHigh = high;
            thresholdLow = low;
        }

        public double getAvgActivTime()
            // getAvgActiveTime() - returns the average activation time for the data set.
        {
            bool lowCrossed = false;
            bool newPeek = false;
            if (data[0][1] > thresholdLow) lowCrossed = true;

            double timer = data[0][0];
            List<double> times = new List<double>();

            foreach (double[] xy in data)
            {
                if (!lowCrossed && xy[1] > thresholdLow) 
                { 
                    lowCrossed = true;
                    newPeek = false; 
                    timer = xy[0];
                }
                else if (lowCrossed && xy[1] < thresholdLow) lowCrossed = false;

                if (xy[1] > thresholdHigh && !newPeek)
                {
                    newPeek = true;
                    times.Add(xy[0] - timer);
                }
            }

            return average(times);
        }

        private double average(List<double> inData)
        {
            if (inData.Count != 0) return inData.Sum() / inData.Count;
            else throw new DivideByZeroException("Empty Data Set");
        }

    }
}
