using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace CrossPlatformKeepAlive
{
    internal static class Interop
    {
        internal static int GetSockOptSysCall(Socket socket, SocketOptionLevel optionLevel, SocketOptionName optionName)
        {
            int optionValue;
            int optionLen = sizeof(int);
            unsafe
            {
                ThrowOnError(GetSockOptSysCall(socket.Handle, (int)optionLevel, (int)optionName, (byte*)&optionValue, &optionLen));
            }
            return optionValue;
        }
        
        internal static void SetSockOptSysCall(Socket socket, SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            unsafe
            {
                ThrowOnError(SetSockOptSysCall(socket.Handle, (int)optionLevel, (int)optionName, (byte*)&optionValue, sizeof(int)));
            }
        }
        
        [DllImport("libc", EntryPoint = "getsockopt")]
        private static extern unsafe int GetSockOptSysCall(IntPtr socketFileDescriptor, int optionLevel, int optionName, byte* optionValue, int* optionLen);

        [DllImport("libc", EntryPoint = "setsockopt")]
        private static extern unsafe int SetSockOptSysCall(IntPtr socketFileDescriptor, int optionLevel, int optionName, byte* optionValue, int optionLen);
        
        private static void ThrowOnError(int error)
        {
            if (error != 0)
                throw new ApplicationException($"Socket option syscall error value: '{error}'");
        }
    }
}