using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
    }
}

// Shared helper library

#pragma warning disable CA1416

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

    public unsafe class Factory
    {
        private string _typeName;
        private Guid _iid;
        private IntPtr _thisPtr;

        private static readonly Guid IID_IActivationFactory = new(0x35, 0, 0, 0xC0, 0, 0, 0, 0, 0, 0, 0x46);

        public Factory(string typeName)
            : this(typeName, IID_IActivationFactory)
        {
        }

        public Factory(string typeName, Guid iid)
        {
            _typeName = typeName;
            _iid = iid;
        }

        public IntPtr ThisPtr
        {
            get
            {
                return (_thisPtr != IntPtr.Zero) ? _thisPtr : InitializeThisPtr();
            }
        }

        public ObjectReference CreateInstanceAndRegister(object wrapper, Guid iid)
        {
            Debug.Assert(_iid == IID_IActivationFactory);

            var objRef = new ObjectReference();

            var pInspectable = IntPtr.Zero;
            var pIFace = IntPtr.Zero;
            try
            {
                var ptr = ThisPtr;
                var pfn = (delegate* unmanaged<IntPtr, IntPtr*, int>)(*(*(void***)ptr + 6));

                WinRT.ExceptionHelpers.ThrowExceptionForHR(pfn(ptr, &pInspectable));

                WinRT.ExceptionHelpers.ThrowExceptionForHR(Marshal.QueryInterface(pInspectable, ref iid, out pIFace));

                DefaultComWrappers.RegisterObjectForInterface(wrapper, pIFace);

                objRef.Attach(pIFace);
                pIFace = IntPtr.Zero;
            }
            finally
            {
                if (pInspectable != IntPtr.Zero)
                    Marshal.Release(pInspectable);
                if (pIFace != IntPtr.Zero)
                    Marshal.Release(pIFace);
            }
            return objRef;
        }

        private IntPtr InitializeThisPtr()
        {
            IntPtr thisPtr = IntPtr.Zero;
            int hr;

            HSTRINGMarshaler hstring = new (_typeName);
            fixed (void* dummy = hstring)
            {
                hr = RoGetActivationFactory(hstring.UnmanagedValue, ref _iid, out thisPtr);

                if (hr < 0)
                {
                    string name = _typeName;
                    for (;;)
                    {
                        int lastDot = name.LastIndexOf('.');
                        if (lastDot == -1)
                            break;
                        name = name.Substring(0, lastDot);
                        if (!NativeLibrary.TryLoad(name + ".dll", out IntPtr nativeLibrary))
                            continue;
                        if (!NativeLibrary.TryGetExport(nativeLibrary, "DllGetActivationFactory", out IntPtr dllActivationFactory))
                            break;

                        IntPtr pInspectable;
                        if (((delegate* unmanaged<IntPtr, IntPtr*, int>)dllActivationFactory)(hstring.UnmanagedValue, &pInspectable) == 0)
                        {
                            WinRT.ExceptionHelpers.ThrowExceptionForHR(Marshal.QueryInterface(pInspectable, ref _iid, out thisPtr));
                            hr = 0;
                        }
                        break;
                    }
                }
            }

            WinRT.ExceptionHelpers.ThrowExceptionForHR(hr);

            // TODO: Deal with non-agile factories and other rare cases.

            if (Interlocked.CompareExchange(ref _thisPtr, thisPtr, IntPtr.Zero) != IntPtr.Zero)
                Marshal.Release(thisPtr);

            return _thisPtr;
        }

        [DllImport("api-ms-win-core-winrt-l1-1-0.dll")]
        private static extern unsafe int RoGetActivationFactory(IntPtr activatableClassId, ref Guid iid, out IntPtr factory);
    }

    public class DefaultComWrappers : ComWrappers
    {
        private static DefaultComWrappers s_instance = new DefaultComWrappers();

        protected override unsafe ComInterfaceEntry* ComputeVtables(object obj, CreateComInterfaceFlags flags, out int count)
        {
            throw new NotImplementedException();
        }

        protected override object CreateObject(IntPtr externalComObject, CreateObjectFlags flags)
        {
            throw new NotImplementedException();
        }

        protected override void ReleaseObjects(IEnumerable objects)
        {
            throw new NotImplementedException();
        }

        public static void RegisterObjectForInterface(object obj, IntPtr thisPtr)
            => s_instance.GetOrRegisterObjectForComInstance(thisPtr, CreateObjectFlags.TrackerObject, obj);

        public static object GeObjectForComInstance(IntPtr ptr)
            => s_instance.GetOrCreateObjectForComInstance(ptr, CreateObjectFlags.TrackerObject);

        private static Guid IID_IInspectable = new(0xAF86E2E0, 0xB12D, 0x4c6a, 0x9C, 0x5A, 0xD7, 0xAA, 0x65, 0x10, 0x1E, 0x90);

        public static IntPtr GetIInspectableForObject(object o)
        {
            if (o is IWinRTObject winRTObject)
            {
                WinRT.ExceptionHelpers.ThrowExceptionForHR(Marshal.QueryInterface(winRTObject.NativeObject.Ptr, ref IID_IInspectable, out IntPtr retVal));
                return retVal;
            }
            else
            {
                // TODO
                throw new NotImplementedException();
            }
        }
    }

    public class ObjectReference : IDisposable
    {
        private IntPtr _ptr;

        public ObjectReference()
        {
        }

        ~ObjectReference()
        {
            Dispose();
        }

        public void Attach(IntPtr ptr)
        {
            Debug.Assert(_ptr == IntPtr.Zero);
            _ptr = ptr;
        }

        private IntPtr GetThisPtr()
        {
            // TODO: Deal with the context-specific pointers and other slow paths
            throw new ObjectDisposedException("ObjectReference");
        }

        public IntPtr Ptr
        {
            get
            {
                var ptr = _ptr;
                if (ptr == IntPtr.Zero)
                    return GetThisPtr();
                return ptr;
            }
        }

        public void Dispose()
        {
            var ptr = Interlocked.Exchange(ref _ptr, IntPtr.Zero);
            if (ptr != IntPtr.Zero)
                Marshal.Release(ptr);
        }
    }
    public interface IWinRTObject
    {
        ObjectReference NativeObject { get; }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HSTRING_HEADER
    {
        IntPtr reserved1;
        int reserved2;
        int reserved3;
        int reserved4;
        int reserved5;
    }

    public unsafe ref struct HSTRINGMarshaler
    {
        private string _value;
        private HSTRING_HEADER _header;
#if DEBUG
        private bool _pinned;
#endif

        public HSTRINGMarshaler(string value)
        {
            _value = value ?? "";
            _header = default;
#if DEBUG
            _pinned = false;
#endif
        }

        public ref readonly char GetPinnableReference()
        {
#if DEBUG
            _pinned = true;
#endif
            return ref _value.GetPinnableReference();
        }

        public IntPtr UnmanagedValue
        {
            get
            {
#if DEBUG
                // We assume that the string is pinned by the calling code
                Debug.Assert(_pinned);
#endif

                IntPtr hstring;
                int hr = Platform.WindowsCreateStringReference(
                    (char*)Unsafe.AsPointer(ref Unsafe.AsRef(in _value.GetPinnableReference())),
                    _value.Length,
                    Unsafe.AsPointer(ref _header),
                    &hstring);
                Debug.Assert(hr == 0);
                return hstring;
            }
        }
    }

    public unsafe ref struct HSTRINGReturnMarshaler
    {
        private IntPtr _value;

        public void Dispose()
        {
            if (_value != IntPtr.Zero)
                Platform.WindowsDeleteString(_value);
        }

        public string ManagedValue
        {
            get
            {
                if (_value == IntPtr.Zero)
                    return "";
                uint length;
                var buffer = Platform.WindowsGetStringRawBuffer(_value, &length);
                return new string(buffer, 0, (int)length);
            }
        }

        public IntPtr* UnmanagedValue => (IntPtr*)Unsafe.AsPointer(ref _value);
    }

    internal static unsafe class Platform
    {
        [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll")]
        [SuppressGCTransition]
        internal static extern unsafe int WindowsCreateStringReference(char* sourceString,
                                                  int length,
                                                  void* hstring_header,
                                                  IntPtr* hstring);

        [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll")]
        [SuppressGCTransition]
        internal static extern char* WindowsGetStringRawBuffer(IntPtr hstring, uint* length);

        [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll")]
        internal static extern int WindowsDeleteString(IntPtr hstring);

    }
}

// Auto-generated code

namespace Windows.System.Power
{
    public unsafe static class PowerManager
    {
        private static readonly WinRT.Factory s_factory = new("Windows.System.Power.PowerManager", new(0x1394825D, 0x62CE, 0x4364, 0x98, 0xD5, 0xAA, 0x28, 0xC7, 0xFB, 0xD1, 0x5B));

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

namespace Component
{
    public sealed unsafe class Class : WinRT.IWinRTObject
    {
        private static readonly WinRT.Factory s_factory = new("Component.Class");

        private WinRT.ObjectReference _this;

        public Class()
        {
            _this = s_factory.CreateInstanceAndRegister(this, new(0x7FE74C13, 0x06E4, 0x5B51, 0xAE, 0xE2, 0x83, 0x6E, 0xA5, 0x05, 0x02, 0x64));
        }

        WinRT.ObjectReference WinRT.IWinRTObject.NativeObject => _this;

        public int Int32Property
        {
            get
            {
                var ptr = _this.Ptr;
                var pfn = (delegate* unmanaged<IntPtr, int*, int>)(*(*(void***)ptr + 6));

                int retval;
                WinRT.ExceptionHelpers.ThrowExceptionForHR(pfn(ptr, &retval));
                GC.KeepAlive(this);
                return retval;
            }
            set
            {
                var ptr = _this.Ptr;
                var pfn = (delegate* unmanaged<IntPtr, int, int>)(*(*(void***)ptr + 7));
                WinRT.ExceptionHelpers.ThrowExceptionForHR(pfn(ptr, value));
                GC.KeepAlive(this);
            }
        }

        public object ObjectProperty
        {
            get
            {
                IntPtr retval = IntPtr.Zero;

                try
                {
                    var ptr = _this.Ptr;
                    var pfn = (delegate* unmanaged<IntPtr, IntPtr*, int>)(*(*(void***)ptr + 8));
                    WinRT.ExceptionHelpers.ThrowExceptionForHR(pfn(ptr, &retval));
                    GC.KeepAlive(this);

                    return WinRT.DefaultComWrappers.GeObjectForComInstance(retval);
                }
                finally
                {
                    if (retval != IntPtr.Zero)
                        Marshal.Release(retval);
                }
            }
            set
            {
                IntPtr pInspectable = IntPtr.Zero;

                try
                {
                    pInspectable = WinRT.DefaultComWrappers.GetIInspectableForObject(value);

                    var ptr = _this.Ptr;
                    var pfn = (delegate* unmanaged<IntPtr, IntPtr, int>)(*(*(void***)ptr + 9));
                    WinRT.ExceptionHelpers.ThrowExceptionForHR(pfn(ptr, pInspectable));
                    GC.KeepAlive(this);
                }
                finally
                {
                    if (pInspectable != IntPtr.Zero)
                        Marshal.Release(pInspectable);
                }
            }
        }

        public string StringProperty
        {
            get
            {
                using WinRT.HSTRINGReturnMarshaler marshaler = new();

                var ptr = _this.Ptr;
                var pfn = (delegate* unmanaged<IntPtr, IntPtr*, int>)(*(*(void***)ptr + 10));
                WinRT.ExceptionHelpers.ThrowExceptionForHR(pfn(ptr, marshaler.UnmanagedValue));
                GC.KeepAlive(this);

                return marshaler.ManagedValue;
            }
            set
            {
                WinRT.HSTRINGMarshaler marshaler = new(value);

                fixed (char* dummy = marshaler)
                {
                    var ptr = _this.Ptr;
                    var pfn = (delegate* unmanaged<IntPtr, IntPtr, int>)(*(*(void***)ptr + 11));
                    WinRT.ExceptionHelpers.ThrowExceptionForHR(pfn(ptr, marshaler.UnmanagedValue));
                    GC.KeepAlive(this);
                }
            }
        }
    }
}
