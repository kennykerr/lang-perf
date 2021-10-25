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
        System.Console.WriteLine("Factory calls: {0} ms", timer.ElapsedMilliseconds);

        timer.Restart();
        var o = new Component.Class();

        for (int i = 0; i < 10_000_000; i++)
        {
            o.Int32Property = 123;
            var _ = o.Int32Property;
        }

        timer.Stop();
        System.Console.WriteLine("Int32 parameters: {0} ms", timer.ElapsedMilliseconds);

        timer.Restart();

        for (int i = 0; i < 10_000_000; i++)
        {
            o.ObjectProperty = o;
            var _ = o.ObjectProperty;
        }

        timer.Stop();
        System.Console.WriteLine("Object parameters: {0} ms", timer.ElapsedMilliseconds);

        timer.Restart();

        for (int i = 0; i < 10_000_000; i++)
        {
            o.StringProperty = "value";
            var _ = o.StringProperty;
        }

        timer.Stop();
        System.Console.WriteLine("String parameters: {0} ms", timer.ElapsedMilliseconds);

        timer.Restart();

        for (int i = 0; i < 10_000_000; i++)
        {
            var _ = (o.NewObject() as Component.INonDefault).NonDefaultProperty;
        }

        timer.Stop();
        System.Console.WriteLine("Dynamic cast: {0} ms", timer.ElapsedMilliseconds);

        var process = Windows.System.Diagnostics.ProcessDiagnosticInfo.GetForCurrentProcess();
        System.Console.WriteLine("Private pages: {0}\n", process.MemoryUsage.GetReport().PrivatePageCount);
    }
}
