class Program
{
    static void Main(string[] args)
    {
        var timer = new System.Diagnostics.Stopwatch();
        timer.Start();

        for (int i = 0; i < 10_000_000; i++)
        {
            var _ = Windows.System.Power.PowerManager.RemainingChargePercent;
        }

        timer.Stop();
        System.Console.WriteLine("{0} ms", timer.ElapsedMilliseconds);
    }
}
