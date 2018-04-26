using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Xunit;

namespace CrossPlatformKeepAlive
{
    public class TestSuite : IDisposable
    {
        private const int keepAliveRetryCountValue = 25;
        private const int keepAliveTimeValue = 36;
        private const int keepAliveIntervalValue = 47;
        
        private static readonly int[] keepAliveOptionNameConsts;
        private static readonly Action<Socket, int, int> SetSocketOption;

        static TestSuite()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                keepAliveOptionNameConsts = new [] { 16, 3, 17 };
                SetSocketOption = (socket, keepAliveOptionNameConst, keepAliveOptionValue) =>
                {
                    socket.SetSocketOption(SocketOptionLevel.Tcp, (SocketOptionName)keepAliveOptionNameConst, keepAliveOptionValue);
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                keepAliveOptionNameConsts = new [] { 0x6, 0x4, 0x5 };
                SetSocketOption = (socket, keepAliveOptionNameConst, keepAliveOptionValue) => 
                {
                    int err = 0;
                    unsafe
                    {
                        err = Interop.SetSockOptSysCallLinux(socket.Handle, (int)SocketOptionLevel.Tcp, keepAliveOptionNameConst, (byte*)&keepAliveOptionValue, sizeof(int));
                    }
                    if (err == -1)
                        throw new SocketException(err);
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                keepAliveOptionNameConsts = new [] { 0x102, 0x10, 0x101 };
                SetSocketOption = (socket, keepAliveOptionNameConst, keepAliveOptionValue) => 
                {
                    int err = 0;
                    unsafe
                    {
                        err = Interop.SetSockOptSysCallOsx(socket.Handle, (int)SocketOptionLevel.Tcp, keepAliveOptionNameConst, (byte*)&keepAliveOptionValue, sizeof(int));
                    }
                    if (err == -1)
                        throw new SocketException(err);
                };
            }
        }

        private readonly TcpClient tcp;

        public TestSuite()
        {
            tcp = new TcpClient();
        }

        [Fact]
        public void SetKeepAliveRetryCount()
        {
            SetSocketOption(tcp.Client, keepAliveOptionNameConsts[0], keepAliveRetryCountValue);
        }

        [Fact]
        public void SetKeepAliveTime()
        {
            SetSocketOption(tcp.Client, keepAliveOptionNameConsts[1], keepAliveRetryCountValue);
        }

        [Fact]
        public void SetKeepAliveInterval()
        {
            SetSocketOption(tcp.Client, keepAliveOptionNameConsts[2], keepAliveRetryCountValue);
        }

        public void Dispose()
        {
            tcp?.Close();
        }
    }
}