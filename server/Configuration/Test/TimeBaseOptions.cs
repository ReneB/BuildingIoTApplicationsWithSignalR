namespace Server.Configuration.Test {
    public class TimeBaseOptions {
        public const string ConfigKey = "Test";

        public DateTime ApplicationStartTime { get; init; } = default;
    }
}