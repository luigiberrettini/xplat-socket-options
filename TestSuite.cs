using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Xunit;

namespace CrossPlatformSocketOptions
{
    public class TestSuite : IDisposable
    {
        private const int keepAliveRetryCountValue = 25;
        private const int keepAliveTimeValue = 36;
        private const int keepAliveIntervalValue = 47;

        private static readonly bool isWindows;
        private static readonly bool isBelowWin10V1703;
        private static readonly bool isBelowWin10V1709;
        private static readonly SocketOptionName[] optionNames;
        private static readonly Func<Socket, SocketOptionLevel, SocketOptionName, int> GetSocketOption;
        private static readonly Action<Socket, SocketOptionLevel, SocketOptionName, int> SetSocketOption;

        static TestSuite()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var version = Environment.OSVersion.Version;
                isWindows = true;
                isBelowWin10V1703 = version.Major < 10 || version.Major == 10 && version.Build < 15063;
                isBelowWin10V1709 = version.Major < 10 || version.Major == 10 && version.Build < 16299;

                optionNames = new [] { (SocketOptionName)0, (SocketOptionName)16, (SocketOptionName)3, (SocketOptionName)17 };
                SetSocketOption = (socket, optionLevel, optionName, optionValue) =>
                    socket.SetSocketOption(optionLevel, optionName, optionValue);
                GetSocketOption = (socket, optionLevel, optionName) =>
                    (int)socket.GetSocketOption(optionLevel, optionName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                optionNames = new [] { (SocketOptionName)15, (SocketOptionName)0x6, (SocketOptionName)0x4, (SocketOptionName)0x5 };
                SetSocketOption = (socket, optionLevel, optionName, optionValue) =>
                    Interop.SetSockOptSysCall(socket, optionLevel, optionName, optionValue);
                GetSocketOption = (socket, optionLevel, optionName) =>
                    Interop.GetSockOptSysCall(socket, optionLevel, optionName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                optionNames = new [] { (SocketOptionName)0x0200, (SocketOptionName)0x102, (SocketOptionName)0x10, (SocketOptionName)0x101 };
                SetSocketOption = (socket, optionLevel, optionName, optionValue) =>
                    Interop.SetSockOptSysCall(socket, optionLevel, optionName, optionValue);
                GetSocketOption = (socket, optionLevel, optionName) =>
                    Interop.GetSockOptSysCall(socket, optionLevel, optionName);
            }
        }

        private readonly TcpClient tcp;

        public TestSuite()
        {
            tcp = new TcpClient();
        }

        [Fact]
        public void EnableExclusivAddressUse()
        {
            if (!isWindows)
                return;

            Assert.False(tcp.Client.ExclusiveAddressUse);
            tcp.Client.ExclusiveAddressUse = true;
            Assert.True(tcp.Client.ExclusiveAddressUse);
        }

        [Fact]
        public void DisableReuseAddress()
        {
            tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
            Assert.Equal(0, (int)tcp.Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress));
        }

        [Fact]
        public void DisableReusePort()
        {
            if (isWindows)
                return;
            SetOption(SocketOptionLevel.Socket, optionNames[0], 0);
        }

        [Fact]
        public void Socket_KeepAlive_Disabled_By_Default()
        {
            Assert.False(IsKeepAliveEnabled(tcp.Client), "Keep-alive was turned on by default!");
        }

        [Fact]
        public void Socket_KeepAlive_Enable_Success()
        {
            tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            Assert.True(IsKeepAliveEnabled(tcp.Client));
        }

        [Fact]
        public void SetKeepAliveRetryCount()
        {
            if (isWindows && isBelowWin10V1703)
                return;
            SetOption(SocketOptionLevel.Tcp, optionNames[1], keepAliveRetryCountValue);
        }

        [Fact]
        public void SetKeepAliveTime()
        {
            if (isWindows && isBelowWin10V1709)
                return;
            SetOption(SocketOptionLevel.Tcp, optionNames[2], keepAliveTimeValue);
        }

        [Fact]
        public void SetKeepAliveInterval()
        {
            if (isWindows && isBelowWin10V1709)
                return;
            SetOption(SocketOptionLevel.Tcp, optionNames[3], keepAliveIntervalValue);
        }

        public void Dispose()
        {
            tcp?.Close();
        }

        private void SetOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            SetSocketOption(tcp.Client, optionLevel, optionName, optionValue);
            Assert.Equal(optionValue, GetSocketOption(tcp.Client, optionLevel, optionName));
        }

        private bool IsKeepAliveEnabled(Socket socket)
        {
            return (int)socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive) != 0;
        }
    }
}