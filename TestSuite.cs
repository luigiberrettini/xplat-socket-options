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

        static TestSuite()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                keepAliveOptionNameConsts = new [] { 16, 3, 17 };
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                keepAliveOptionNameConsts = new [] { 0x6, 0x4, 0x5 };
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                keepAliveOptionNameConsts = new [] { 0x102, 0x10, 0x101 };
        }

        private readonly TcpClient tcp;

        public TestSuite()
        {
            tcp = new TcpClient();
        }

        [Fact]
        public void SetKeepAliveRetryCount()
        {
            tcp.Client.SetSocketOption(SocketOptionLevel.Tcp, (SocketOptionName)keepAliveOptionNameConsts[0], keepAliveRetryCountValue);
        }

        [Fact]
        public void SetKeepAliveTime()
        {
            tcp.Client.SetSocketOption(SocketOptionLevel.Tcp, (SocketOptionName)keepAliveOptionNameConsts[1], keepAliveTimeValue);
        }

        [Fact]
        public void SetKeepAliveInterval()
        {
            tcp.Client.SetSocketOption(SocketOptionLevel.Tcp, (SocketOptionName)keepAliveOptionNameConsts[2], keepAliveIntervalValue);
        }

        public void Dispose()
        {
            tcp?.Close();
        }
    }
}