using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace AlgorithmsAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] dimensions = new[]
            {
                1, 5, 20, 100, 500, 1000, 5000, 20000, 100000, 500000, 1000000, 5000000, 10000000, 100000000, 1000000000
            };
            //DO NOT ENTER VALUES MORE THAN ONE BILLION!!! THAT REQUIRES A LOT OF MACHINE RESOURCES!!!
            
            Tools.ConductResearch( //General testing method, that exports Excel CSV file after test
                dimensions, //Array with dimensions that describes how big arrays should be created
                new LinearSearch() //IResercheable instance that should be tested. Look the IResercheable definition first.
                ); 
            
        }
    }

    class LinearSearch : IResercheable //Example testing class
    {
        public int Run(int[] array, int value) 
        {
            for (int i = 0; i < array.Length; i++)
                if (array[i] == value) return i;
            return -1;
        }
        
        public string Name
        {   //Only latin text supported, sorry, there is some troubles with encoding
            get => "Simple linear search algorithm"; 
        }
    }

    class Tools {

        //Method that creates an array with specified size
        //and absolutely random values inside
        public static int[] GenerateArray(int size) {
            int[] array = new int[size];
            Random random = new Random();
            for (int i = 0; i < array.Length; i++)
                array[i] = random.Next(int.MaxValue);
            return array;
        }

        //Method that creates an array with specified size
        //and it is already sorted
        public static int[] GenerateSortedArray(int size) {
            int[] array = new int[size];
            Random random = new Random();
            for (int i = 1; i < array.Length; i++)
                array[i] = array[i - 1] + random.Next(1000) + 1;
            return array;
        }

        //Method that returns time spent on the algorithm
        //The time is considered to be measured correctly
        public static long MeasureTime(int[] array, IResercheable algorithm) {
            Random random = new Random();
            algorithm.Run(array, array[random.Next(array.Length)]); //to free up resources 
            Stopwatch watch = new Stopwatch();
            GC.Collect();
            watch.Start();
            algorithm.Run(array, array[random.Next(array.Length)]);
            watch.Stop();
            return watch.ElapsedMilliseconds;
        }

        //This is general multiple testing method
        //add `true` value at the end to make all values sorted.
        public static void ConductResearch(int[] dimensions, IResercheable algorithm, bool sorted = false)
        {
            List<(int, long)> results = new List<(int, long)>();
            results.Add((-1, sorted ? 1 : 0));
            foreach (int dimension in dimensions) {
                if (dimension > 1000000000)
                {
                    Console.WriteLine("VALUE MORE THAN ONE BILLION WILL BE SKIPPED, SRY!!!");
                    continue;
                }
                Console.WriteLine($"Checking dimension: {dimension}");
                results.Add((dimension, MeasureTime(sorted ? GenerateSortedArray(dimension) : GenerateArray(dimension), algorithm)));
            }
            Console.WriteLine("Well done!");
            ExportAsCsv(results, algorithm);
        }

        //You shouldn't use this method. It's a king of internal magic ^_^
        private static void ExportAsCsv(List<(int, long)> researches, IResercheable algorithm)
        {
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\export" +
                              DateTime.Now.ToString(CultureInfo.CurrentCulture).Replace(':', '-') + ".csv";
            Stream stream = new FileStream(filename, FileMode.OpenOrCreate);
            Console.WriteLine($"File saved at: {filename}");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Tested algorithm:;{algorithm.GetType().Name} ({algorithm.Name})");
            int serviceIndex = researches.FindIndex(x => x.Item1 == -1);
            string sorted = researches[serviceIndex].Item2 == 1 ? "YES" : "NO";
            sb.AppendLine($"Sorted?;{sorted}");
            sb.AppendLine();
            researches.RemoveAt(serviceIndex);
            sb.AppendLine($"Dimension (elements);Spent time (ms)");
            foreach (var pos in researches)
                sb.AppendLine($"{pos.Item1};{pos.Item2}");
            byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
            stream.Write(data, 0, data.Length);
            stream.Close();
        }
    }

    //Interface to realize testing algorithms
    interface IResercheable
    {
        //Method which describes all of the algorithm,
        //Here is ARRAY - a data collection that will be tested
        //And VALUE - the value that we looking for in the ARRAY
        int Run(int[] array, int value);
        
        //NAME that will be displayed in exported file
        string Name { get; }
    }
}