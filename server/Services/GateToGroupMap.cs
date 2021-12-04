using Microsoft.Extensions.Options;
using Server.Configuration;

namespace Server.Services {
    public class GateToGroupMap {
        private readonly AirportOptions airportOptions;

        public GateToGroupMap(IOptions<AirportOptions> airportOptions) {
            this.airportOptions = airportOptions.Value;
        }

        public string GroupForGate(int gateNumber) {
            return airportOptions.AreaMap
                .FirstOrDefault(kvp => kvp.Value.Contains(gateNumber))
                .Key ?? throw new Exception($"Gate {gateNumber} not registered at this airport");
        }

        public int[] GatesForGroup(string group) {
            var groupIsKnown = airportOptions.AreaMap.TryGetValue(group, out var value);

            return groupIsKnown ? value! : throw new Exception($"Group {group} not registered at this airport");
        }
    }
}