using Microsoft.AspNetCore.SignalR;
using Server.Hubs.Services;
using Server.Services;

namespace Server.Hubs {
    public interface IDevice {
        Task Announce(string announcementId, string announcementText, byte[] announcementMp3);
        Task RegisterDevicePresence(string deviceId, DateTime timestamp);
        Task RegisterDeviceOffline(string deviceId);
        Task RegisterAudioStarted(string deviceId);
        Task RegisterAudioEnded(string deviceId);
    }

    public class MainHub : Hub<IDevice> {
        private const string groupNameForMasters = "Masters";
        private readonly ConnectedClientRegistry clientRegistry;
        private readonly GateToGroupMap gateToGroupMap;
        private readonly ClientToGateMap clientToGateMap;

        public MainHub(ConnectedClientRegistry clientRegistry, GateToGroupMap gateToGroupMap, ClientToGateMap clientToGateMap) {
            this.clientRegistry = clientRegistry;
            this.gateToGroupMap = gateToGroupMap;
            this.clientToGateMap = clientToGateMap;
        }

        protected IDevice Masters {
            get {
                return Clients.Group(groupNameForMasters);
            }
        }

        public async Task Ping() {
            var deviceId = clientRegistry.FindDeviceId(Context.ConnectionId);

            await Masters.RegisterDevicePresence(deviceId, DateTime.UtcNow);
        }

        public async Task NotifyAudioStarted(int gateNumber) {
            var deviceId = clientRegistry.FindDeviceId(Context.ConnectionId);
            var group = gateToGroupMap.GroupForGate(gateNumber);

            await Clients.Group(group).RegisterAudioStarted(deviceId);
        }

        public async Task NotifyAudioEnded(int gateNumber) {
            var deviceId = clientRegistry.FindDeviceId(Context.ConnectionId);
            var group = gateToGroupMap.GroupForGate(gateNumber);

            await Clients.Group(group).RegisterAudioEnded(deviceId);
        }

        public async Task<string> RegisterDevice(string deviceId, int gateNumber) {
            clientToGateMap.Connect(Context.ConnectionId, gateNumber);

            clientRegistry.Register(Context.ConnectionId, deviceId);

            var group = gateToGroupMap.GroupForGate(gateNumber);

            await Groups.AddToGroupAsync(Context.ConnectionId, group);

            return group;
        }

        public async Task RegisterAsMaster() {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupNameForMasters);
        }

        public override async Task OnDisconnectedAsync(Exception? exception) {
            clientToGateMap.Disconnect(Context.ConnectionId);

            var deviceId = clientRegistry.FindDeviceId(Context.ConnectionId);

            await Masters.RegisterDeviceOffline(deviceId);

            await base.OnDisconnectedAsync(exception);
        }
    }
}