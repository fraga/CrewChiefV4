using ksBroadcastingNetwork;
using System;

namespace ksBroadcastingTestClient.ClientConnections
{
    public class ClientPanelViewModel : KSObservableObject
    {
        public string IP { get => Get<string>(); set => Set(value); }
        public int Port { get => Get<int>(); set => Set(value); }
        public string DisplayName { get => Get<string>(); set => Set(value); }
        public string ConnectionPw { get => Get<string>(); set => Set(value); }
        public string CommandPw { get => Get<string>(); set => Set(value); }
        public int RealtimeUpdateIntervalMS { get => Get<int>(); set => Set(value); }

        public ClientConnectionViewModel ClientConnection { get => Get<ClientConnectionViewModel>(); set => Set(value); }

    public Action<ACCUdpRemoteClient> OnClientConnectedCallback { get; }

        public ClientPanelViewModel(string udpIpAddress, Action<ACCUdpRemoteClient> onClientConnectedCallback)
        {
            IP = udpIpAddress;
            Port = 9000;
            DisplayName = "Your name";
            ConnectionPw = "asd";
            CommandPw = "";
            RealtimeUpdateIntervalMS = 250;
            OnClientConnectedCallback = onClientConnectedCallback;

            var c = new ACCUdpRemoteClient(IP, Port, DisplayName, ConnectionPw, CommandPw, RealtimeUpdateIntervalMS);

            ClientConnection = new ClientConnectionViewModel(c, OnClientConnectedCallback);
        }

        internal void Shutdown()
        {
            if (ClientConnection != null)
            {
                ClientConnection.Client.Shutdown();
                ClientConnection = null;
            }
        }
    }
}
