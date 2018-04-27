using System;
using System.Runtime.InteropServices;

namespace CrossPlatformKeepAlive
{
    internal static class Interop
    {
        [DllImport("libc", EntryPoint = "setsockopt")]
        internal static extern unsafe int SetSockOptSysCall(IntPtr socketFileDescriptor, int optionLevel, int optionName, byte* optionValue, int optionLen);
    }
}