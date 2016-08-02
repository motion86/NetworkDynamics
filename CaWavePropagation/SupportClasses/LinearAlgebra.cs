using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkDynamics
{
    class LinearAlgebra
    {
        public static Matrix MatMultiply(Matrix m1, Matrix m2)                  // Stupid matrix multiplication
        {
            if (m1.cols != m2.rows) throw new IndexOutOfRangeException("Wrong dimensions of matrix!");
            Matrix result = new Matrix(m1.rows, m2.cols);
            for (int i = 0; i < result.rows; i++)
                for (int j = 0; j < result.cols; j++)
                    for (int k = 0; k < m1.cols; k++)
                        result.setEntry(i,j, result.getEntry(i,j) + m1.getEntry(i,k) * m2.getEntry(k, j));
            return result;
        }

        public static double EigenPowerMethod(Matrix M, out Matrix V)
        {
            double error = 1e-6;
            V = new Matrix(M.rows, 1);
            Matrix V1 = new Matrix(M.rows, 1);
            double lambda = 0;
            double checkSum = 1;
            foreach (int i in range(V.rows)) V1.setEntry(i, 0, 1);
            int j = 0;
            while (checkSum > error && j++ < 30000) // max itterations set here to prevent inf loop for non-converging scenarios
            {
                V = M.matMultiply(V1);
                lambda = VectorMagnitude(V);
                V.scalarDiv(lambda);
                checkSum = Math.Abs(vCheckSum(V1) - vCheckSum(V));
                //V1.PrintEigenV(); V.PrintEigenV();
                V1.copy(V);
            }
            return lambda;
        }

        private static double vCheckSum(Matrix V)
        {
            double temp = 0;
            foreach (int i in range(V.rows)) temp += V.getEntry(i, 0);
            return temp;
        }

        public static void scalarMult(Matrix M, double scalar)
        {
            foreach (int i in range(M.rows))
                foreach (int j in range(M.cols)) 
                    M.setEntry(i, j, M.getEntry(i, j) * scalar);
        }

        public static void mCopy(Matrix A, Matrix B)
        {
            foreach (int i in range(A.rows))
                foreach (int j in range(A.cols))
                    B.setEntry(i, j, A.getEntry(i, j));
        }

        public static double VectorMagnitude(Matrix V)
        {
            double temp = 0;
            foreach (int i in range(V.rows)) temp += Math.Pow(V.getEntry(i, 0), 2);
            return Math.Sqrt(temp);
        }

        public static void insertMat<T>(List<T[]> M1, List<T[]> M2, int index)
            // insert M2 int M1
        {
            int offset = M2.Count * index;
            foreach (int i in range(M2.Count))
                foreach(int j in range(M2.Count))
                {
                    M1[i + offset][j + offset] = M2[i][j];
                }
        }

        public static System.Collections.IEnumerable range(int range)
        {
            for (int i = 0; i < range; i++) yield return i;
        }
        private static System.Collections.IEnumerable range(int range, int offset)
        {
            for (int i = offset; i < range + offset; i++) yield return i;
        }
    }
}
