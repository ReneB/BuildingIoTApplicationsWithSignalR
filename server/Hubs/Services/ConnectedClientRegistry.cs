namespace Server.Hubs.Services {
    public class ConnectedClientRegistry {
        private readonly Dictionary<string, string> connectionIdToDeviceIdMap = new();
        private readonly Dictionary<string, string> deviceIdToConnectionIdMap = new();

        public void Register(string connectionId, string deviceId) {
            connectionIdToDeviceIdMap[connectionId] = deviceId;
            deviceIdToConnectionIdMap[deviceId] = connectionId;
        }

        public string FindDeviceId(string connectionId) {
            return connectionIdToDeviceIdMap.GetValueOrDefault(connectionId) ?? throw new Exception($"Connection ${connectionId} does not belong to a registered device");
        }

        public string FindConnectionId(string deviceId) {
            return deviceIdToConnectionIdMap.GetValueOrDefault(deviceId) ?? throw new Exception($"Device ${deviceId} does not have a registered connection");
        }
    }
}