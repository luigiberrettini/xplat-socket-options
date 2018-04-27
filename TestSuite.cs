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

        private static readonly SocketOptionName[] keepAliveOptionNames;
        private static readonly Func<Socket, SocketOptionName, int> GetSocketOption;
        private static readonly Action<Socket, SocketOptionName, int> SetSocketOption;

        static TestSuite()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                keepAliveOptionNames = new [] { (SocketOptionName)16, (SocketOptionName)3, (SocketOptionName)17 };
                SetSocketOption = (socket, keepAliveOptionName, keepAliveOptionValue) =>
                    socket.SetSocketOption(SocketOptionLevel.Tcp, keepAliveOptionName, keepAliveOptionValue);
                GetSocketOption = (socket, keepAliveOptionName) =>
                    (int)socket.GetSocketOption(SocketOptionLevel.Tcp, keepAliveOptionName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                keepAliveOptionNames = new [] { (SocketOptionName)0x6, (SocketOptionName)0x4, (SocketOptionName)0x5 };
                SetSocketOption = (socket, keepAliveOptionName, keepAliveOptionValue) =>
                    Interop.SetSockOptSysCall(socket, keepAliveOptionName, keepAliveOptionValue);
                GetSocketOption = (socket, keepAliveOptionName) =>
                    Interop.GetSockOptSysCall(socket, keepAliveOptionName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                keepAliveOptionNames = new [] { (SocketOptionName)0x102, (SocketOptionName)0x10, (SocketOptionName)0x101 };
                SetSocketOption = (socket, keepAliveOptionName, keepAliveOptionValue) =>
                    Interop.SetSockOptSysCall(socket, keepAliveOptionName, keepAliveOptionValue);
                GetSocketOption = (socket, keepAliveOptionName) =>
                    Interop.GetSockOptSysCall(socket, keepAliveOptionName);
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
            SetOption(keepAliveOptionNames[0], keepAliveRetryCountValue);
        }

        [Fact]
        public void SetKeepAliveTime()
        {
            SetOption(keepAliveOptionNames[1], keepAliveTimeValue);
        }

        [Fact]
        public void SetKeepAliveInterval()
        {
            SetOption(keepAliveOptionNames[2], keepAliveIntervalValue);
        }

        public void Dispose()
        {
            tcp?.Close();
        }

        private void SetOption(SocketOptionName keepAliveOptionName, int keepAliveOptionValue)
        {
            SetSocketOption(tcp.Client, keepAliveOptionName, keepAliveOptionValue);
            Assert.Equal(keepAliveOptionValue, GetSocketOption(tcp.Client, keepAliveOptionName));
        }
    }
}