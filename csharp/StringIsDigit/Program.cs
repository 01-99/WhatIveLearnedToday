using System.Diagnostics;
using System.Text.RegularExpressions;


using (var bench = new Benchmark($"Using Character Comparison"))
{
    string inputString = "Hello1";
    var lastChar = inputString.Last();

    bool result = lastChar >= '0' && lastChar <= '9';
}

using (var bench = new Benchmark($"Using Substring"))
{
    string inputString = "Hello1";
    bool result = inputString.Substring(inputString.Length - 1).All(char.IsDigit);
}

using (var bench = new Benchmark($"Using Regex to Check if a String Ends With a Number"))
{
    string inputString = "Hello1";
    var regex = new Regex(@"\d+$");

    bool result = regex.Match(inputString).Success;
}
using (var bench = new Benchmark($"Check if a String Ends With a Number Using IsDigit"))
{
    string inputString = "Hello1";
    bool result = char.IsDigit(inputString[inputString.Length - 1]);
}

using (var bench = new Benchmark($"Check if a String Ends With a Number Using IsDigit and Last"))
{
    string inputString = "Hello1";
    bool result = Char.IsDigit(inputString.Last());
}


using (var bench = new Benchmark($"Check if a String Ends With a Number Using IsDigit and Last with char alias"))
{
    string inputString = "Hello1";
    bool result = Char.IsDigit(inputString.Last());
}











public class Benchmark : IDisposable
{
    private readonly Stopwatch timer = new Stopwatch();
    private readonly string benchmarkName;

    public Benchmark(string benchmarkName)
    {
        this.benchmarkName = benchmarkName;
        timer.Start();
    }

    public void Dispose()
    {
        timer.Stop();
        Console.WriteLine($"{benchmarkName} {timer.Elapsed}");
    }
}