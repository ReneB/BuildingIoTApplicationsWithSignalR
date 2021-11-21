using Microsoft.AspNetCore.SignalR;

namespace Server.Hubs {
    public interface IDevice {
        Task RegisterDevicePresence(string deviceId, DateTime timestamp);
    }

    public class MainHub : Hub<IDevice> {
        private const string groupNameForMasters = "Masters";

        public async Task Ping(string deviceId) {
            await Clients.Group(groupNameForMasters).RegisterDevicePresence(deviceId, DateTime.UtcNow);
        }

        public async Task RegisterAsMaster() {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupNameForMasters);
        }
    }
}