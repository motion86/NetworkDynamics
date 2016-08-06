using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkDynamics
{
    public class DataTools
    {
        public static List<double[]> listCopy(List<double[]> listToCopy)
        // Deep Copies a list of double[]
        {
            if (listToCopy == null) return null;

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

        public static List<string[]> listCopy(List<string[]> listToCopy)
        // Deep Copies a list of stirng[]
        {
            if (listToCopy == null) return null;

            List<string[]> copy = new List<string[]>(listToCopy.Count);
            foreach (string[] arr in listToCopy)
            {
                int arrSize = arr.Length;
                string[] arr_copy = new string[arrSize];
                for (int i = 0; i < arrSize; i++)
                    arr_copy[i] = String.Copy(arr[i]);
                copy.Add(arr_copy);
            }
            return copy;
        }

        public static List<List<T>> listCopy<T>(List<List<T>> listToCopy)
        // Deep Copies a list of double[]
        {
            if (listToCopy == null) return null;

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

        public static Dictionary<int, int> binDataOne(List<double> data, Dictionary<int, int> dataBins)
        // @binDataOne - function takes a data list and returns the bins as dictationary.
        // @param data - the data list.
        {

            foreach (double e in data)
            {
                if (dataBins.ContainsKey((int)e)) dataBins[(int)e]++;
                else dataBins.Add((int)e, 1);
            }

            return dataBins;
        }

        public static Dictionary<int, int> binDataOne(List<double> data, int numBins)
        {
            Dictionary<int, int> dataBins = new Dictionary<int, int>();
            foreach (int i in LinearAlgebra.range(numBins))
                dataBins.Add(i, 0);
            return binDataOne(data, dataBins);
        }
        public static Dictionary<int, int> binDataOne(List<double> data)
        {
            Dictionary<int, int> dataBins = new Dictionary<int, int>();
            return binDataOne(data, dataBins);
        }

        public static List<double[]> binData(List<double> data, int numBins)
        {
            List<double[]> bins = new List<double[]>(); // {count, x1, x2}
            foreach (int i in range(numBins)) bins.Add(new double[3]);

            double min = data.Min();
            double max = data.Max();
            double binWidth = (max - min) / numBins;

            foreach (double p in data)
            {
                double x1 = min, x2 = x1 + binWidth;
                int i = 0;
                while (x1 < max)
                {
                    if (i > numBins - 1) break;
                    if (p >= x1 && p < x2)
                    {
                        bins[i][0]++;
                        bins[i][1] = x1;
                        bins[i][2] = x2;
                        break;
                    } i++;
                    x1 = x2; x2 = x1 + binWidth;
                }
            }
            foreach (int j in range(numBins)) bins[j][0] /= binWidth;

            return bins;
        }

        private static System.Collections.IEnumerable range(int range)
        {
            for (int i = 0; i < range; i++) yield return i;
        }

        public static double Gauss(double z)
        {
            // input = z-value (-inf to +inf)
            // output = p under Standard Normal curve from -inf to z
            // e.g., if z = 0.0, function returns 0.5000
            // ACM Algorithm #209
            double y; // 209 scratch variable
            double p; // result. called 'z' in 209
            double w; // 209 scratch variable
            if (z == 0.0)
                p = 0.0;
            else
            {
                y = Math.Abs(z) / 2;
                if (y >= 3.0)
                {
                    p = 1.0;
                }
                else if (y < 1.0)
                {
                    w = y * y;
                    p = ((((((((0.000124818987 * w
                      - 0.001075204047) * w + 0.005198775019) * w
                      - 0.019198292004) * w + 0.059054035642) * w
                      - 0.151968751364) * w + 0.319152932694) * w
                      - 0.531923007300) * w + 0.797884560593) * y * 2.0;
                }
                else
                {
                    y = y - 2.0;
                    p = (((((((((((((-0.000045255659 * y
                      + 0.000152529290) * y - 0.000019538132) * y
                      - 0.000676904986) * y + 0.001390604284) * y
                      - 0.000794620820) * y - 0.002034254874) * y
                      + 0.006549791214) * y - 0.010557625006) * y
                      + 0.011630447319) * y - 0.009279453341) * y
                      + 0.005353579108) * y - 0.002141268741) * y
                      + 0.000535310849) * y + 0.999936657524;
                }
            }
            if (z > 0.0)
                return (p + 1.0) / 2;
            else
                return (1.0 - p) / 2;
        }

        public static double Student(double t, double df)
        {
            // for large integer df or double df
            // adapted from ACM algorithm 395
            // returns 2-tail p-value
            double n = df; // to sync with ACM parameter name
            double a, b, y;
            t = t * t;
            y = t / n;
            b = y + 1.0;
            if (y > 1.0E-6) y = Math.Log(b);
            a = n - 0.5;
            b = 48.0 * a * a;
            y = a * y;
            y = (((((-0.4 * y - 3.3) * y - 24.0) * y - 85.5) /
              (0.8 * y * y + 100.0 + b) + y + 3.0) / b + 1.0) *
              Math.Sqrt(y);
            return 2.0 * Gauss(-y); // ACM algorithm 209
        }

        public static double TTest(double[] x, double[] y)
        /* The p-value is the probability that the two means are equal. 
           If the p-value is low, typically less than 0.05 or 0.01, then you
           conclude there is statistcal evidence the two means are different.
           But if the p-value isn't below the 5% or 1% critical value then you
           conclude there's not enough evidence to say the means are different.
        */
        {
            // Welch's t-test for two samples
            double sumX = 0.0; // calculate means
            double sumY = 0.0;

            for (int i = 0; i < x.Length; ++i)
                sumX += x[i];

            for (int i = 0; i < y.Length; ++i)
                sumY += y[i];

            int n1 = x.Length;
            int n2 = y.Length;
            double meanX = sumX / n1;
            double meanY = sumY / n2;

            double sumXminusMeanSquared = 0.0; // calculate (sample) variances
            double sumYminusMeanSquared = 0.0;

            for (int i = 0; i < n1; ++i)
                sumXminusMeanSquared += (x[i] - meanX) * (x[i] - meanX);

            for (int i = 0; i < n2; ++i)
                sumYminusMeanSquared += (y[i] - meanY) * (y[i] - meanY);

            double varX = sumXminusMeanSquared / (n1 - 1);
            double varY = sumYminusMeanSquared / (n2 - 1);

            // t statistic
            double top = (meanX - meanY);
            double bot = Math.Sqrt((varX / n1) + (varY / n2));
            double t = top / bot;


            // df mildly tricky
            // http://www.statsdirect.com/help/default.htm#parametric_methods/unpaired_t.htm

            double num = ((varX / n1) + (varY / n2)) * ((varX / n1) + (varY / n2));
            double denomLeft = ((varX / n1) * (varX / n1)) / (n1 - 1);
            double denomRight = ((varY / n2) * (varY / n2)) / (n2 - 1);
            double denom = denomLeft + denomRight;
            double df = num / denom;

            double p = Student(t, df); // cumulative two-tail density
            /*
            Console.WriteLine("mean of x = " + meanX.ToString("F2"));
            Console.WriteLine("mean of y = " + meanY.ToString("F2"));
            Console.WriteLine("t = " + t.ToString("F4"));
            Console.WriteLine("df = " + df.ToString("F3"));
            Console.WriteLine("p-value = " + p.ToString("F5"));
            */
            return p;
        }

    }
}
