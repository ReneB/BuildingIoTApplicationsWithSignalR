namespace Server.Hubs.Services {
    public class ConnectedClientRegistry {
        private readonly Dictionary<string, string> connectionIdToDeviceIdMap = new();
        private readonly Dictionary<string, string> deviceIdToConnectionIdMap = new();

        public void Register(string connectionId, string deviceId) {
            connectionIdToDeviceIdMap[connectionId] = deviceId;
            deviceIdToConnectionIdMap[deviceId] = connectionId;
        }

        public string? FindDeviceId(string connectionId) {
            return connectionIdToDeviceIdMap.GetValueOrDefault(connectionId);
        }

        public string? FindConnectionId(string deviceId) {
            return deviceIdToConnectionIdMap.GetValueOrDefault(deviceId);
        }
    }
}