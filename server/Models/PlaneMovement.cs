namespace Server.Models {
    public abstract record PlaneMovement {
        public string Id =>  $"{Timestamp:yyMMdd}-{FlightNumber}-{GetType().Name}";
        public string Airport { get; init; } = string.Empty;
        public int Gate { get; init; }
        public DateTime Timestamp { get; init; }
        public string FlightNumber { get; init; } = string.Empty;
    }

    public record Departure : PlaneMovement {
        public string Destination { get; init; } = string.Empty;
    }

    public record Arrival : PlaneMovement {
        public string Origin { get; init; } = string.Empty;
    }
}