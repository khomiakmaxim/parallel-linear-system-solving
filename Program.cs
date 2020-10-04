using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//N 1024
//Additional threads: 1023, time: 00:00:03.1886711
//No additional threads, time: 00:00:05.7406855

//N 16
//Additional threads: 15, time: 00:00:00.1137965
//No additional threads, time: 00:00:00.0005096

//N 524
//Additional threads: 523, time: 00:00:00.5207423
//No additional threads, time: 00:00:00.6901752
namespace SystemOfLinearEquations
{
    class Program
    {
        public static double[][] MakeMatrix(int n)
        {
            Random rnd = new Random();
            double[][] M = new double[n][];

            for (int i = 0; i < n; i++)
            {
                M[i] = new double[n + 1];
                for (int j = 0; j < n + 1; j++)
                    M[i][j] = rnd.Next(-20, 20);
            }
            return M;
        }

        public static string Print2(double[][] M)
        {
            string str = "";
            int rowCount = M.GetUpperBound(0) + 1;
            for (int row = 0; row < rowCount; row++)
            {
                for (int i = 0; i <= rowCount; i++)
                {
                    str += M[row][i];
                    str += "\t";
                }
                str += Environment.NewLine;
            }
            return str;
        }


        static void Main(string[] args)
        {
            Console.Write("N:");
            int n = int.Parse(Console.ReadLine());
            double[][] M, M1;
            M = M1 = MakeMatrix(n);

            var startTime = DateTime.Now;
            GaussParallel(M);
            //Console.WriteLine(Print2(M));
            var duration = DateTime.Now - startTime;
            Console.WriteLine($"Additional threads: {M.GetLength(0)-1}, time: {duration}");

            startTime = DateTime.Now;
            Gauss(M1);
            //Console.WriteLine(Print2(M1));
            duration = DateTime.Now - startTime;
            Console.WriteLine($"No additional threads, time: {duration}");
        }

        public static bool Gauss(double[][] M)
        {
            int rowCount = (M.GetLength(0));

            for (int sourceRow = 0; sourceRow + 1 < rowCount; sourceRow++)
            {
                for (int destRow = sourceRow + 1; destRow < rowCount; destRow++)
                {
                    double df = M[sourceRow][sourceRow];
                    double sf = M[destRow][sourceRow];

                    for (int j = 0; j < rowCount + 1; j++)
                    {
                        M[destRow][j] = (M[destRow][j] * df - M[sourceRow][j] * sf) / df;
                    }
                }
            }

            //підставляємо назад
            for (int row = rowCount - 1; row >= 0; row--)
            {
                double f = M[row][row];
                if (f == 0) return false;

                for (int i = 0; i < rowCount + 1; i++) M[row][i] /= f;
                for (int destRow = 0; destRow < row; destRow++)
                {
                    M[destRow][rowCount] -= M[destRow][row] * M[row][rowCount];
                    M[destRow][row] = 0;
                }
            }
            return true;
        }

        public static bool GaussParallel(double[][] M)
        {
            int rowCount = (M.GetLength(0));
            int temp = rowCount - 1;

            //elimination
            //кожен рядок виконуєтсья в окремому потоці
            Parallel.For(0, temp, sourceRow =>            
            {

                for (int destRow = sourceRow + 1; destRow < rowCount; destRow++)
                {
                    double df = M[sourceRow][sourceRow];
                    double sf = M[destRow][sourceRow];

                    for (int j = 0; j < rowCount + 1; j++)
                    {
                        M[destRow][j] = (M[destRow][j] * df - M[sourceRow][j] * sf) / df;
                    }
                }
            });

            //підставляємо назад
            for (int row = rowCount - 1; row >= 0; row--)
            {
                double f = M[row][row];
                if (f == 0) return false;

                for (int i = 0; i < rowCount + 1; i++) M[row][i] /= f;
                for (int destRow = 0; destRow < row; destRow++)
                {
                    M[destRow][rowCount] -= M[destRow][row] * M[row][rowCount];
                    M[destRow][row] = 0;
                }
            }
            return true;
        }
    }       
}
