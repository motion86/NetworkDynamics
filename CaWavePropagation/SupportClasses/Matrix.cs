using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace NetworkDynamics
{
    public class Matrix
    {
        private static string dllPath = System.IO.Directory.GetCurrentDirectory() + @"\DLLs\liblapack.dll";
        //[DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
       // public static extern 

        private List<double[]> M;
        private double[,] M2d;

        public List<double[]> EigenV { get; private set; }
        public double[,] EigenVectors { get; private set; }

        public double Lambda { get; private set; }
        public double[] LambdaR { get; private set; }
        public double[] LambdaI { get; private set; }

        public int rows { get; private set; }
        public int cols { get; private set; }

        private bool isEigenCalculated;

        public Matrix(int size)
        {
            M = new List<double[]>(size);
            foreach (int i in iterate(size))
            {
                M.Add(new double[size]);
            }
            rows = size;
            cols = size;
            EigenV = null;
            Lambda = 0;
            isEigenCalculated = false;
        }

        public Matrix(int rows_, int cols_)
        {
            M = new List<double[]>(rows_);
            foreach (int i in iterate(rows_))
            {
                M.Add(new double[cols_]);
            }
            rows = rows_;
            cols = cols_;
            EigenV = null;
            Lambda = 0;
            isEigenCalculated = false;
        }

        public Matrix(List<double[]> mat)
        {
            M = mat;
            rows = M.Count;
            cols = M[0].Length;
            EigenV = null;
            Lambda = 0;
            isEigenCalculated = false;
        }

        public void M2M2d()
            // M2M2d - copies the List<double[]> into a double[,]
        {
            int size = M.Count;
            M2d = new double[size, size];

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    M2d[i, j] = M[i][j];
        }

        public bool CalcEigenGeneral()
            // CalcEigenGeneral - uses the ALGLIB to compute the left eigen vectors and eigen values of M.
        {
            double[,] EigenVectors;
            double[,] na; // dummy var for the right eigen vectors.
            double[] LambdaR;
            double[] LambdaI;

            if (M2d == null) // transffer M into M2d.
                M2M2d();

            bool done = alglib.rmatrixevd(M2d, M.Count, 1, out LambdaR, out LambdaI, out na, out EigenVectors);

            this.EigenVectors = EigenVectors;
            this.LambdaR = LambdaR;
            this.LambdaI = LambdaI;

            return done;
        }

        public void calcEigen()
        {
            if (!isEigenCalculated)
            {
                Matrix eigenV;
                Lambda = LinearAlgebra.EigenPowerMethod(this, out eigenV);
                EigenV = eigenV.getList();
                isEigenCalculated = true;
            }
        }

        public bool setEntry(int i, int j, double x)
        {
            if (!checkDim(i, j)) return false;

            M[i][j] = x;
            return true;
        }

        public double getEntry(int i, int j)
        {
            if (!checkDim(i, j)) throw new IndexOutOfRangeException();
            return M[i][j];
        }

        public Matrix matMultiply(Matrix B)
        {
            return LinearAlgebra.MatMultiply(this, B);
        }

        public List<double[]> matMultiply(List<double[]> B)
        {
            return (LinearAlgebra.MatMultiply(this,new Matrix(B))).getList();
        }

        public void scalarMult(double scalar)
        {
            LinearAlgebra.scalarMult(this, scalar);
        }

        public void scalarDiv(double scalar)
        {
            if(scalar == 0) throw new DivideByZeroException();
            scalarMult(1 / scalar);
        }

        public void copy(Matrix B)
        {
            LinearAlgebra.mCopy(B, this);
        }

        public List<double[]> getList() { return M; }

        public void PrintEigenV()
        {
            Console.WriteLine();
            foreach (int i in iterate(rows))
                Console.Write(EigenV[i][0].ToString() + " ");
            Console.WriteLine();
        }

        public void PrintMatrix()
        {
            foreach (int i in iterate(rows)){
                Console.WriteLine();
                foreach (int j in iterate(cols))
                    Console.Write(M[i][j] + " ");
            }
        }

        private bool checkDim(int i, int j)
        {
            if (i > rows || j > cols) return false;
            else return true;
        }

        private static System.Collections.IEnumerable iterate(int range)
        {
            for(int i = 0; i < range; i++) yield return i;
        }
    }
}
