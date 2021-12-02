namespace Server.Configuration {
    public class SpeechOptions {
        public const string ConfigKey = "Speech";

        public string Key { get; init; } = string.Empty;
        public string Region { get; init; } = string.Empty;
    }
}