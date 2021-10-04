using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;

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
        // .NET 6: [StackTraceHidden]
        public static void ThrowExceptionForHR(int hr)
        {
            if (hr < 0)
                Throw(hr);

            // .NET 6: [StackTraceHidden]
            static void Throw(int hr)
            {
                Exception ex = GetExceptionForHR(hr, useGlobalErrorState: true, out bool restoredExceptionFromGlobalState);
                if (restoredExceptionFromGlobalState)
                {
                    ExceptionDispatchInfo.Capture(ex).Throw();
                }
                else
                {
                    throw ex;
                }
            }
        }

        // Placeholder implemenentation. See https://github.com/microsoft/CsWinRT/blob/master/src/WinRT.Runtime/ExceptionHelpers.cs
        // for actual implementation.
        private static Exception GetExceptionForHR(int hr, bool useGlobalErrorState, out bool restoredExceptionFromGlobalState)
        {
            restoredExceptionFromGlobalState = false;
            return new Exception() { HResult = hr };
        }
    }

    public class Factory
    {
        private string _name;
        private Guid _iid;
        private IntPtr _thisPtr;

        public Factory(string name, Guid iid)
        {
            _name = name;
            _iid = iid;
        }

        public IntPtr ThisPtr
        {
            get
            {
                return (_thisPtr != IntPtr.Zero) ? _thisPtr : InitializeThisPtr();
            }
        }

        private IntPtr InitializeThisPtr()
        {
            IntPtr hstring = IntPtr.Zero;
            IntPtr thisPtr = IntPtr.Zero;
            try
            {
                WinRT.ExceptionHelpers.ThrowExceptionForHR(WindowsCreateString(_name, _name.Length, out hstring));
                WinRT.ExceptionHelpers.ThrowExceptionForHR(RoGetActivationFactory(hstring, ref _iid, out thisPtr));
            }
            finally
            {
                WindowsDeleteString(hstring);
            }

            // TODO: Deal with non-agile factories and other rare cases.

            if (Interlocked.CompareExchange(ref _thisPtr, thisPtr, IntPtr.Zero) != IntPtr.Zero)
                Marshal.Release(thisPtr);

            return _thisPtr;
        }

        [DllImport("api-ms-win-core-winrt-l1-1-0.dll")]
        private static extern unsafe int RoGetActivationFactory(IntPtr activatableClassId, ref Guid iid, out IntPtr factory);

        [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CharSet = CharSet.Unicode)]
        private static extern unsafe int WindowsCreateString(string sourceString, int length, out IntPtr hstring);

        [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll")]
        internal static extern int WindowsDeleteString(IntPtr hstring);
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
