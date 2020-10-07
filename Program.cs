using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//Результати роботи програми в залежності від розміру входу і кількості додаткових потоків

//N:100
//Additional threads: 1, time: 00:00:00.1164598
//Additional threads: 4, time: 00:00:00.0274764

//N:100
//Additional threads: 1, time: 00:00:00.1820647
//Additional threads: 16, time: 00:00:00.0414736

//N:1024
//Additional threads: 1, time: 00:00:05.7101077
//Additional threads: 16, time: 00:00:02.8566659

//N:1024
//Additional threads: 1, time: 00:00:06.0186376
//Additional threads: 4, time: 00:00:02.7923653

//N:512
//Additional threads: 1, time: 00:00:00.8684674
//Additional threads: 4, time: 00:00:00.3921659

//N:512
//Additional threads: 1, time: 00:00:00.8446070
//Additional threads: 16, time: 00:00:00.4442046
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

        public static void PrintMatrix(double[][] M)
        {
            for (int i = 0; i < M.Length; ++i)
            {
                for (int j = 0; j < M[0].Length; ++j)
                {
                    Console.Write(M[i][j] + " ");
                }
                Console.Write("\n");
            }
            Console.Write("\n");
        }

        public static void TestParallelism(double[][] M, int numThreads)
        {
            var startTime = DateTime.Now;
            if (GaussianElimination(M, numThreads))
            {
                //Console.WriteLine("result:");
                //PrintMatrix(M);
                var duration = DateTime.Now - startTime;
                Console.WriteLine($"Additional threads: {numThreads}, time: {duration}");
            }
            else
            {
                Console.WriteLine("No solution was found");
            }
            
        }


        static void Main(string[] args)
        {
            Console.Write("N:");
            int n = int.Parse(Console.ReadLine());
            double[][] M;            
            M = MakeMatrix(n);

            //Console.WriteLine("matrix:");
            //PrintMatrix(M);

            TestParallelism(M, 1);
            TestParallelism(M, 16);
        }

        public static bool GaussianElimination(double[][] M, int numThreads)
        {
            int rowCount = (M.GetLength(0));

            for (int sourceRow = 0; sourceRow + 1 < rowCount; ++sourceRow)//останній рядок не опрацьовуємо
            {
                //кількість рядків,що будуть опрацьовуватися додатковими потоками
                int execRows = rowCount - sourceRow - 1;
                int partRowCount = execRows / numThreads;

                //в такому випадку видаліяємо кожному потоку лише один рядок
                partRowCount = (partRowCount < 1) ? 1 : partRowCount;
                var tasks = new List<Task>();
                for (int start_row = sourceRow+1; start_row < rowCount; start_row += partRowCount)
                {
                    var start = start_row;
                    tasks.Add(Task.Factory.StartNew(() => PartialResult(M, start, partRowCount, sourceRow)));
                }
                Task.WaitAll(tasks.ToArray());                
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

        public static void PartialResult(double[][] M, int startRow, int rowCount, int sourceRow)
        {
            //опрацьовуємо лише ті рядки, за які відповідає даний потік
            for (int i = 0; i < rowCount; ++i)
            {
                var destRow = startRow + i;
                if (destRow >= M.Length)
                    return;                
                double df = M[sourceRow][sourceRow];
                double sf = M[destRow][sourceRow];

                for (int j = 0; j < M.Length + 1; j++)
                {
                    M[destRow][j] = (M[destRow][j] * df - M[sourceRow][j] * sf) / df;
                }                
            }
            
        }

        #region Альтернативна реалізація

        //public static bool Gauss(double[][] M)
        //{
        //    int rowCount = (M.GetLength(0));

        //    for (int sourceRow = 0; sourceRow + 1 < rowCount; sourceRow++)
        //    {
        //        for (int destRow = sourceRow + 1; destRow < rowCount; destRow++)
        //        {
        //            double df = M[sourceRow][sourceRow];
        //            double sf = M[destRow][sourceRow];

        //            for (int j = 0; j < rowCount + 1; j++)
        //            {
        //                M[destRow][j] = (M[destRow][j] * df - M[sourceRow][j] * sf) / df;
        //            }
        //        }
        //    }

        //    //підставляємо назад
        //    for (int row = rowCount - 1; row >= 0; row--)
        //    {
        //        double f = M[row][row];
        //        if (f == 0) return false;

        //        for (int i = 0; i < rowCount + 1; i++) M[row][i] /= f;
        //        for (int destRow = 0; destRow < row; destRow++)
        //        {
        //            M[destRow][rowCount] -= M[destRow][row] * M[row][rowCount];
        //            M[destRow][row] = 0;
        //        }
        //    }
        //    return true;
        //}

        //public static bool GaussParallel(double[][] M)
        //{
        //    int rowCount = (M.GetLength(0));
        //    int temp = rowCount - 1;

        //    //elimination
        //    //кожен рядок виконуєтсья в окремому потоці
        //    Parallel.For(0, temp, sourceRow =>
        //    {

        //        for (int destRow = sourceRow + 1; destRow < rowCount; destRow++)
        //        {
        //            double df = M[sourceRow][sourceRow];
        //            double sf = M[destRow][sourceRow];

        //            for (int j = 0; j < rowCount + 1; j++)
        //            {
        //                M[destRow][j] = (M[destRow][j] * df - M[sourceRow][j] * sf) / df;
        //            }
        //        }
        //    });

        //    //підставляємо назад
        //    for (int row = rowCount - 1; row >= 0; row--)
        //    {
        //        double f = M[row][row];
        //        if (f == 0) return false;

        //        for (int i = 0; i < rowCount + 1; i++) M[row][i] /= f;
        //        for (int destRow = 0; destRow < row; destRow++)
        //        {
        //            M[destRow][rowCount] -= M[destRow][row] * M[row][rowCount];
        //            M[destRow][row] = 0;
        //        }
        //    }
        //    return true;
        //} 
        #endregion
    }       
}
