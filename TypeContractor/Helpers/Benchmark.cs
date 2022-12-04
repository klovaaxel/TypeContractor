using System.Diagnostics;

namespace TypeContractor.Helpers
{
    internal class Benchmark
    {
        private static int IndentLevel = -1;
        private readonly Stopwatch Stopwatch = new();
        private readonly string _label;

        public static Benchmark Start(string label, bool writeInitial = false) => new Benchmark(label).Start(writeInitial);
        public static T Measure<T>(string label, Func<T> action) => new Benchmark(label).MeasureImpl(action);
        public static void Measure(string label, Action action) => new Benchmark(label).MeasureImpl(action);

        public Benchmark(string label)
        {
            _label = label;
        }

        public Benchmark Start(bool writeInitial = false)
        {
            IndentLevel++;
            if (writeInitial) Console.WriteLine($"{Indent}[START] {_label}");
            Stopwatch.Start();
            return this;
        }

        public T MeasureImpl<T>(Func<T> action)
        {
            Start();
            var result = action();
            Stop();
            return result;
        }


        public void MeasureImpl(Action action)
        {
            Start();
            action();
            Stop();
        }

        public void Stop()
        {
            Stopwatch.Stop();
            Console.WriteLine($"{Indent}[{Stopwatch.ElapsedMilliseconds,3}ms] {_label}");
            IndentLevel--;
        }

        private static string Indent => new(' ', IndentLevel * 4);
    }
}
