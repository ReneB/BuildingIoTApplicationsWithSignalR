namespace Server.Configuration {
    public class AirportOptions {
        public const string ConfigKey = "Airport";

        public string Current { get; init; } = string.Empty;
        public Dictionary<string, int[]> AreaMap { get; init; } = new();
    }
}
