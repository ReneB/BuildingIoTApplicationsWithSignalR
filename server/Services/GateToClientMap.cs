namespace Server.Services {
    public class ClientToGateMap {
        private readonly Dictionary<string, int> clientToGateMap = new();
        private readonly GateToGroupMap gateToGroupMap;

        public ClientToGateMap(GateToGroupMap gateToGroupMap) {
            this.gateToGroupMap = gateToGroupMap;
        }

        public string? ClientForGate(int gateNumber) {
            var group = gateToGroupMap.GroupForGate(gateNumber);
            var gatesInArea = gateToGroupMap.GatesForGroup(group);

            return gatesInArea
                .OrderBy(g => Math.Abs(gateNumber - g))
                .Select(ClientForGateInternal)
                .FirstOrDefault(c => c != null);
        }

        public int? GateForClient(string connectionId) {
            return clientToGateMap.TryGetValue(connectionId, out var gateNumber) ? gateNumber : null;
        }

        public void Connect(string connectionId, int gateNumber) {
            clientToGateMap[connectionId] = gateNumber;
        }

        public void Disconnect(string connectionId) {
            clientToGateMap.Remove(connectionId);
        }

        private string? ClientForGateInternal(int gateNumber) {
            return clientToGateMap.FirstOrDefault(kvp => kvp.Value == gateNumber).Key;
        }
    }
}
