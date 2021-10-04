using System;

// Handwritten example that demonstrates performance that cswinrt can achieve with efficient bindings

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

// Shared helper library

namespace WinRT
{
    public static class ExceptionHelpers
    {
        public static void ThrowExceptionForHR(int hr)
        {
            // TODO
        }
    }

    public class Factory
    {
        public Factory(string name, Guid iid)
        {
            // TODO
        }

        public IntPtr ThisPtr
        {
            get
            {
                // TODO
                return IntPtr.Zero;
            }
        }
    }
}

// Auto-generated code

namespace Windows.System.Power
{
    public unsafe static class PowerManager
    {
        private static readonly WinRT.Factory s_factory = new("Windows.System.Power.PowerManager", new Guid(0x1394825D, 0x62CE, 0x4364, 0x98, 0xD5, 0xAA, 0x28, 0xC7, 0xFB, 0xD1, 0x5B));

        public static int RemainingChargePercent
        {
            get
            {
                int retVal;

                var ptr = s_factory.ThisPtr;
                var pfn = (delegate* unmanaged<IntPtr, int*, int>)(*(*(void***)ptr + 15));
                WinRT.ExceptionHelpers.ThrowExceptionForHR(pfn(ptr, &retVal));

                return retVal;
            }
        }
    }
}
