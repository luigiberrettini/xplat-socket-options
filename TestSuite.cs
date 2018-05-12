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

        private static readonly SocketOptionName[] optionNames;
        private static readonly Func<Socket, SocketOptionLevel, SocketOptionName, int> GetSocketOption;
        private static readonly Action<Socket, SocketOptionLevel, SocketOptionName, int> SetSocketOption;

        static TestSuite()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
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
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            Assert.False(tcp.Client.ExclusiveAddressUse);
            tcp.Client.ExclusiveAddressUse = true;
            Assert.True(tcp.Client.ExclusiveAddressUse);
        }

        [Fact]
        public void DisableReuseAddress()
        {
            Assert.Equal(0, (int)tcp.Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress));
            tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
            Assert.Equal(0, (int)tcp.Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress));
        }

        [Fact]
        public void DisableReusePort()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            CheckAndSetOption(SocketOptionLevel.Socket, optionNames[0], 0, 0);
        }

        [Fact]
        public void SetKeepAliveRetryCount()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;
            SetOption(SocketOptionLevel.Tcp, optionNames[1], keepAliveRetryCountValue);
        }

        [Fact]
        public void SetKeepAliveTime()
        {
            SetOption(SocketOptionLevel.Tcp, optionNames[2], keepAliveTimeValue);
        }

        [Fact]
        public void SetKeepAliveInterval()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;
            SetOption(SocketOptionLevel.Tcp, optionNames[3], keepAliveIntervalValue);
        }

        public void Dispose()
        {
            tcp?.Close();
        }

        private void CheckAndSetOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int oldOptionValue, int newOptionValue)
        {
            //Assert.Equal(oldOptionValue, GetSocketOption(tcp.Client, optionLevel, optionName));
            SetOption(optionLevel, optionName, newOptionValue);
        }

        private void SetOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            SetSocketOption(tcp.Client, optionLevel, optionName, optionValue);
            Assert.Equal(optionValue, GetSocketOption(tcp.Client, optionLevel, optionName));
        }
    }
}