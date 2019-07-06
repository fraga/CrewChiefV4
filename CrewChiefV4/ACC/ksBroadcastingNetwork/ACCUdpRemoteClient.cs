using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ksBroadcastingNetwork
{
    public class ACCUdpRemoteClient : IDisposable
    {
        private UdpClient _client;
        
        public BroadcastingNetworkProtocol MessageHandler { get; }
        public string IpPort { get; }
        public string DisplayName { get; }
        public string ConnectionPassword { get; }
        public string CommandPassword { get; }
        public int MsRealtimeUpdateInterval { get; }

        readonly int Port;
        readonly string IpAddress;
        readonly static object udpLock = new object();
        readonly static SemaphoreSlim udpTaskSemaphore = new SemaphoreSlim(1, 1);
        bool allowRun;
        bool serverConnected;

        /// <summary>
        /// To get the events delivered inside the UI thread, just create this object from the UI thread/synchronization context.
        /// </summary>
        public ACCUdpRemoteClient(string ip, int port, string displayName, string connectionPassword, string commandPassword, int msRealtimeUpdateInterval)
        {
            Port = port;
            IpAddress = ip;

            IpPort = $"{ip}:{port}";
            MessageHandler = new BroadcastingNetworkProtocol(IpPort, Send);

            DisplayName = displayName;
            ConnectionPassword = connectionPassword;
            CommandPassword = commandPassword;
            MsRealtimeUpdateInterval = msRealtimeUpdateInterval;

            ConnectAndRun();
        }

        public async Task LockForReadingAsync(Action action)
        {
            await udpTaskSemaphore.WaitAsync();

            try
            {
                action();
            }
            finally
            {
                udpTaskSemaphore.Release();
            }
        }

        private UdpClient GetUdpClient(bool isSending = false)
        {
            lock(udpLock)
            { 
                if (_client == null || !_client.Client.Connected)
                {
                    if (_client != null)
                    {
                        try { _client.Dispose(); } catch { }
                    }

                    _client = new UdpClient();
                    _client.Connect(IpAddress, Port);

                    if(!isSending)
                        MessageHandler.RequestConnection(DisplayName, ConnectionPassword, MsRealtimeUpdateInterval, CommandPassword);
                }

                return _client;
            }
        }

        private void Send(byte[] payload)
        {
            UdpClient client = GetUdpClient(true);

            client.Send(payload, payload.Length);
        }

        public void Shutdown()
        {
            allowRun = false;
        }

        private void ConnectAndRun()
        {
            Task.Run(async () =>
            {
                allowRun = true;

                while (allowRun)
                {
                    await udpTaskSemaphore.WaitAsync();

                    try
                    {
                        UdpClient client = GetUdpClient();

                        if (!client.Client.Connected)
                        {
                            await Task.Delay(1000);
                            continue;
                        }

                        var udpReceiveTask = client.ReceiveAsync();

                        // While hokey, it usually takes 0 milliseconds so this should be plenty
                        if(!udpReceiveTask.IsCompleted)
                            udpReceiveTask.Wait(20000);

                        if (udpReceiveTask.Status == TaskStatus.WaitingForActivation)
                        {
                            try { client.Dispose(); } catch { }
                            _client = null;
                        }
                        else
                        {
                            UdpReceiveResult udpPacket = udpReceiveTask.Result;

                            if (udpPacket.Buffer.Length == 0)
                                await Task.Delay(10);
                            else
                            {
                                using (var ms = new System.IO.MemoryStream(udpPacket.Buffer))
                                using (var reader = new System.IO.BinaryReader(ms))
                                {
                                    MessageHandler.ProcessMessage(reader);
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        udpTaskSemaphore.Release();
                    }
                }
            });
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        if (_client != null)
                        {
                            _client.Close();
                            _client.Dispose();
                            _client = null;
                        }

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ACCUdpRemoteClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
