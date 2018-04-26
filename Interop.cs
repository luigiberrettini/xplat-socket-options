using System;
using System.Runtime.InteropServices;

namespace CrossPlatformKeepAlive
{
    internal static class Interop
    {
        [DllImport("libc.so.6", EntryPoint = "setsockopt")]
        internal static extern unsafe int SetSockOptSysCallLinux(IntPtr socketFileDescriptor, int optionLevel, int optionName, byte* optionValue, int optionLen);

        [DllImport("libc.dylib", EntryPoint = "setsockopt")]
        internal static extern unsafe int SetSockOptSysCallOsx(IntPtr socketFileDescriptor, int optionLevel, int optionName, byte* optionValue, int optionLen);
    }
}